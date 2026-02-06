using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Extensions.Options;
using InControl.App.Controls;
using InControl.App.Pages;
using InControl.App.Services;
using InControl.Core.Configuration;
using InControl.Core.Models;
using InControl.Core.UX;
using InControl.Inference.Interfaces;
using InControl.Services.Interfaces;
using InControl.Services.Voice;
using InControl.ViewModels.ConversationView;

namespace InControl.App;

/// <summary>
/// Main application window with page navigation support.
/// Provides global control bar, status strip, and seamless navigation between pages.
/// Uses NavigationService for consistent backstack behavior.
/// </summary>
public sealed partial class MainWindow : Window
{
    private UserControl? _currentPage;
    private bool _isOffline;
    private readonly NavigationService _navigation = NavigationService.Instance;
    private readonly ConversationViewModel _conversationVm = new();
    private CancellationTokenSource? _runCts;

    /// <summary>
    /// Model families that are embedding-only and cannot be used for chat.
    /// </summary>
    private static readonly HashSet<string> EmbeddingFamilies = new(StringComparer.OrdinalIgnoreCase)
    {
        "nomic-bert", "bert"
    };

    /// <summary>
    /// Model name prefixes that indicate embedding-only models.
    /// </summary>
    private static readonly string[] EmbeddingPrefixes =
    [
        "nomic-embed", "all-minilm", "mxbai-embed", "snowflake-arctic-embed",
        "bge-", "gte-", "e5-"
    ];

    public MainWindow()
    {
        InitializeComponent();
        SetupWindow();
        InitializeTheme();
        SetupNavigation();
        SetupEventHandlers();
        InitializeStatusStrip();
        _ = LoadModelsAsync();
        _ = InitializeVoiceAsync();
    }

    private void InitializeTheme()
    {
        // Initialize theme service with the root content element
        if (this.Content is FrameworkElement rootElement)
        {
            // Load saved theme preference (default to System)
            ThemeService.Instance.Initialize(rootElement, "System");
        }
    }

    private void SetupWindow()
    {
        var appWindow = this.AppWindow;
        appWindow.Title = "InControl - Local AI Chat";
        appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
    }

    private void SetupNavigation()
    {
        _navigation.Navigated += OnNavigated;
        _navigation.NavigatedBack += OnNavigatedBack;
        _navigation.NavigatedHome += (s, e) => NavigateHomeInternal();
    }

    private void SetupEventHandlers()
    {
        // AppBar events - use NavigationService
        AppBar.SettingsRequested += (s, e) => _navigation.Navigate<SettingsPage>();
        AppBar.AssistantRequested += (s, e) => _navigation.Navigate<AssistantPage>();
        AppBar.ExtensionsRequested += (s, e) => _navigation.Navigate<ExtensionsPage>();
        AppBar.PolicyRequested += (s, e) => _navigation.Navigate<PolicyPage>();
        AppBar.ConnectivityRequested += (s, e) => _navigation.Navigate<ConnectivityPage>();
        AppBar.HelpRequested += (s, e) => _navigation.Navigate<HelpPage>();
        AppBar.ModelManagerRequested += (s, e) => _navigation.Navigate<ModelManagerPage>();
        AppBar.CommandPaletteRequested += (s, e) => ShowCommandPalette();

        // Command Palette events
        CommandPalette.CommandExecuted += OnCommandExecuted;
        CommandPalette.CloseRequested += (s, e) => HideCommandPalette();

        // StatusStrip events - use NavigationService
        StatusStrip.ModelClicked += (s, e) => _navigation.Navigate<ModelManagerPage>();
        StatusStrip.DeviceClicked += (s, e) => _navigation.Navigate<ModelManagerPage>();
        StatusStrip.ConnectivityClicked += (s, e) => _navigation.Navigate<ConnectivityPage>();
        StatusStrip.PolicyClicked += (s, e) => _navigation.Navigate<PolicyPage>();
        StatusStrip.AssistantClicked += (s, e) => _navigation.Navigate<AssistantPage>();
        StatusStrip.MemoryClicked += (s, e) => _navigation.Navigate<SettingsPage>();

        // SessionSidebar events
        SessionSidebar.NewSessionRequested += OnNewSessionRequested;

        // ConversationView InputComposer events
        ConversationView.Composer.ModelManagerRequested += (s, e) => _navigation.Navigate<ModelManagerPage>();
        ConversationView.Composer.RunRequested += OnRunRequested;
        ConversationView.Composer.CancelRequested += OnCancelRequested;

        // ConversationView speak events
        ConversationView.SpeakRequested += OnSpeakRequested;
        ConversationView.StopSpeakRequested += OnStopSpeakRequested;

        // Global keyboard shortcuts
        this.Content.KeyDown += OnGlobalKeyDown;
    }

    private void InitializeStatusStrip()
    {
        // Set initial status strip values
        StatusStrip.SetModelStatus(null, false);
        StatusStrip.SetDeviceStatus("GPU", true);
        StatusStrip.SetConnectivityStatus(false);
        StatusStrip.SetPolicyStatus(false);
        StatusStrip.SetAssistantStatus(true, "Assistant");
    }

    /// <summary>
    /// Loads available models from Ollama and populates the InputComposer dropdown.
    /// Filters out embedding-only models that cannot be used for chat.
    /// </summary>
    private async Task LoadModelsAsync()
    {
        try
        {
            var modelManager = App.GetService<IModelManager>();
            var allModels = await modelManager.ListModelsAsync();

            // Filter out embedding models — they can't be used for chat
            var chatModels = allModels
                .Where(m => !IsEmbeddingModel(m))
                .Select(m => m.Name)
                .ToList();

            // Update UI on dispatcher thread
            DispatcherQueue.TryEnqueue(() =>
            {
                ConversationView.SetAvailableModels(chatModels);

                if (chatModels.Count > 0)
                {
                    StatusStrip.SetModelStatus(chatModels[0], true);
                    StatusStrip.SetConnectivityStatus(true);
                }
            });
        }
        catch (Exception)
        {
            // Ollama not running — that's OK, user can open Model Manager
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusStrip.SetModelStatus(null, false);
                StatusStrip.SetConnectivityStatus(false);
            });
        }
    }

    /// <summary>
    /// Attempts to connect the voice service on startup.
    /// Voice is optional — failures are silently handled.
    /// </summary>
    private async Task InitializeVoiceAsync()
    {
        try
        {
            var voiceService = App.GetService<IVoiceService>();
            await voiceService.ConnectAsync();
        }
        catch
        {
            // Voice engine not available — that's fine, it's optional
        }
    }

    /// <summary>
    /// Determines whether a model is an embedding-only model.
    /// </summary>
    private static bool IsEmbeddingModel(InControl.Core.Models.ModelInfo model)
    {
        // Check by family
        if (!string.IsNullOrEmpty(model.Family) && EmbeddingFamilies.Contains(model.Family))
        {
            return true;
        }

        // Check by name prefix
        var name = model.Name.ToLowerInvariant();
        foreach (var prefix in EmbeddingPrefixes)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Handles the Run button click from the InputComposer.
    /// Creates a conversation if needed, sends the message, and streams the response.
    /// </summary>
    private async void OnRunRequested(object? sender, RunRequestedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Intent) || string.IsNullOrWhiteSpace(e.Model))
            return;

        var chatService = App.GetService<IChatService>();
        var model = e.Model;

        // Bind the ViewModel to the view on first run
        if (ConversationView.ViewModel is null)
        {
            ConversationView.ViewModel = _conversationVm;
        }

        // Create conversation if we don't have one (include system prompt from config)
        var conversation = _conversationVm.GetConversation();
        if (conversation is null)
        {
            var chatOptions = App.GetService<IOptions<ChatOptions>>();
            var systemPrompt = chatOptions?.Value.DefaultSystemPrompt;

            conversation = await chatService.CreateConversationAsync(
                title: e.Intent.Length > 50 ? e.Intent[..50] + "..." : e.Intent,
                model: model,
                systemPrompt: systemPrompt);
            _conversationVm.LoadConversation(conversation);
        }

        // Add user message to UI
        _conversationVm.AddUserIntent(e.Intent);

        // Update UI state
        _conversationVm.ExecutionState = ExecutionState.Running;
        _conversationVm.CurrentModel = model;
        ConversationView.Composer.ExecutionState = ExecutionState.Running;
        ConversationView.Composer.IntentText = string.Empty;
        StatusStrip.SetModelStatus(model, true);

        // Start elapsed time tracking
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var timer = DispatcherQueue.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += (t, a) => _conversationVm.ElapsedTime = stopwatch.Elapsed;
        timer.Start();

        _runCts = new CancellationTokenSource();

        // Begin streaming output
        _conversationVm.BeginModelOutput(model);
        ConversationView.ShowMessages();

        string? completedContent = null;

        try
        {
            _conversationVm.ExecutionState = ExecutionState.Streaming;
            ConversationView.Composer.ExecutionState = ExecutionState.Streaming;

            var contentBuilder = new System.Text.StringBuilder();

            await foreach (var token in chatService.SendMessageAsync(
                conversation.Id, e.Intent, _runCts.Token))
            {
                contentBuilder.Append(token);
                _conversationVm.AppendToModelOutput(token);
                ConversationView.ScrollToBottom();
            }

            completedContent = contentBuilder.ToString();

            // Finalize the streaming message — stops the spinner
            _conversationVm.FinalizeModelOutput();

            _conversationVm.ExecutionState = ExecutionState.Complete;
            ConversationView.Composer.ExecutionState = ExecutionState.Idle;
        }
        catch (OperationCanceledException)
        {
            _conversationVm.FinalizeModelOutput();
            _conversationVm.ExecutionState = ExecutionState.Cancelled;
            ConversationView.Composer.ExecutionState = ExecutionState.Idle;
        }
        catch (Exception ex)
        {
            _conversationVm.FinalizeModelOutput();
            _conversationVm.ExecutionState = ExecutionState.Issue;
            ConversationView.Composer.ExecutionState = ExecutionState.Idle;
            System.Diagnostics.Debug.WriteLine($"Chat error: {ex.Message}");
        }
        finally
        {
            timer.Stop();
            stopwatch.Stop();
            _conversationVm.ElapsedTime = stopwatch.Elapsed;
            _runCts?.Dispose();
            _runCts = null;

            // Auto-speak if enabled and voice is connected
            AutoSpeakIfEnabled(completedContent);

            // Reset execution state after a brief pause
            await Task.Delay(1500);
            if (_conversationVm.ExecutionState is ExecutionState.Complete
                or ExecutionState.Cancelled
                or ExecutionState.Issue)
            {
                _conversationVm.ExecutionState = ExecutionState.Idle;
            }
        }
    }

    /// <summary>
    /// Auto-speaks the completed response if voice is configured and connected.
    /// </summary>
    private void AutoSpeakIfEnabled(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        try
        {
            var voiceOptions = App.GetService<IOptions<VoiceOptions>>();
            var voiceService = App.GetService<IVoiceService>();

            if (voiceOptions?.Value.AutoSpeak == true)
            {
                _ = voiceService.SpeakAsync(content);
            }
        }
        catch
        {
            // Voice is optional — never crash the app
        }
    }

    /// <summary>
    /// Handles the speak button click on a message card.
    /// Triggers lazy voice engine initialization if needed.
    /// </summary>
    private async void OnSpeakRequested(object? sender, InControl.ViewModels.MessageViewModel msg)
    {
        if (string.IsNullOrWhiteSpace(msg.Content)) return;

        try
        {
            var voiceService = App.GetService<IVoiceService>();
            await voiceService.SpeakAsync(msg.Content);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Speak failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the stop speaking button click.
    /// </summary>
    private async void OnStopSpeakRequested(object? sender, EventArgs e)
    {
        try
        {
            var voiceService = App.GetService<IVoiceService>();
            await voiceService.StopSpeakingAsync();
        }
        catch
        {
            // Voice is optional
        }
    }

    /// <summary>
    /// Handles the Cancel button click — stops the current generation.
    /// </summary>
    private void OnCancelRequested(object? sender, EventArgs e)
    {
        _runCts?.Cancel();

        var conversation = _conversationVm.GetConversation();
        if (conversation is not null)
        {
            var chatService = App.GetService<IChatService>();
            chatService.StopGeneration(conversation.Id);
        }
    }

    private void OnGlobalKeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Ctrl+K - Command Palette
        if (e.Key == Windows.System.VirtualKey.K &&
            Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            ShowCommandPalette();
            e.Handled = true;
            return;
        }

        // Escape - Close overlays or go back
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            if (CommandPaletteOverlay.Visibility == Visibility.Visible)
            {
                HideCommandPalette();
                e.Handled = true;
            }
            else if (_currentPage != null)
            {
                // Use backstack navigation instead of always going home
                _navigation.GoBack();
                e.Handled = true;
            }
        }

        // Alt+Left - Navigate back (standard Windows behavior)
        if (e.Key == Windows.System.VirtualKey.Left &&
            Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            if (_navigation.CanGoBack)
            {
                _navigation.GoBack();
                e.Handled = true;
            }
        }
    }

    private void OnCommandExecuted(object? sender, string commandId)
    {
        switch (commandId)
        {
            case "new-session":
                _navigation.GoHome();
                break;
            case "open-settings":
                _navigation.Navigate<SettingsPage>();
                break;
            case "open-model-manager":
                _navigation.Navigate<ModelManagerPage>();
                break;
            case "toggle-offline":
                ToggleOfflineMode();
                break;
            case "open-extensions":
                _navigation.Navigate<ExtensionsPage>();
                break;
            case "open-assistant":
                _navigation.Navigate<AssistantPage>();
                break;
            case "view-policy":
                _navigation.Navigate<PolicyPage>();
                break;
            case "open-connectivity":
                _navigation.Navigate<ConnectivityPage>();
                break;
            case "open-help":
                _navigation.Navigate<HelpPage>();
                break;
            case "search-sessions":
                _navigation.GoHome();
                // Focus session search
                break;
        }
    }

    private void OnNavigated(object? sender, NavigationEventArgs e)
    {
        if (e.PageType == null) return;
        NavigateToPageInternal(e.PageType);
    }

    private void OnNavigatedBack(object? sender, NavigationEventArgs e)
    {
        if (e.PageType == null)
        {
            NavigateHomeInternal();
            return;
        }
        NavigateToPageInternal(e.PageType);
    }

    private void NavigateToPageInternal(Type pageType)
    {
        // Remove current page if any
        if (_currentPage != null)
        {
            PageHost.Children.Remove(_currentPage);
        }

        // Create and add new page
        var page = (UserControl)Activator.CreateInstance(pageType)!;
        _currentPage = page;

        // Wire up back navigation and events
        WirePageEvents(page);

        PageHost.Children.Add(page);

        // Show page host, hide home view
        HomeView.Visibility = Visibility.Collapsed;
        PageHost.Visibility = Visibility.Visible;
    }

    private void WirePageEvents(UserControl page)
    {
        // All pages use NavigationService.GoBack() for consistent behavior
        if (page is SettingsPage settings)
        {
            settings.BackRequested += (s, e) => _navigation.GoBack();
            settings.ModelManagerRequested += (s, e) => _navigation.Navigate<ModelManagerPage>();
            settings.ExtensionsRequested += (s, e) => _navigation.Navigate<ExtensionsPage>();
            settings.PolicyRequested += (s, e) => _navigation.Navigate<PolicyPage>();
        }
        else if (page is ModelManagerPage modelManager)
        {
            modelManager.BackRequested += (s, e) =>
            {
                _navigation.GoBack();
                // Refresh model list when coming back from Model Manager
                _ = LoadModelsAsync();
            };
            modelManager.ModelSelected += OnModelSelected;
        }
        else if (page is AssistantPage assistant)
        {
            assistant.BackRequested += (s, e) => _navigation.GoBack();
        }
        else if (page is ExtensionsPage extensions)
        {
            extensions.BackRequested += (s, e) => _navigation.GoBack();
        }
        else if (page is PolicyPage policy)
        {
            policy.BackRequested += (s, e) => _navigation.GoBack();
            policy.OfflineModeChanged += (s, isOffline) => SetOfflineMode(isOffline);
        }
        else if (page is ConnectivityPage connectivity)
        {
            connectivity.BackRequested += (s, e) => _navigation.GoBack();
            connectivity.OfflineModeChanged += (s, isOffline) => SetOfflineMode(isOffline);
            connectivity.IsOffline = _isOffline;
        }
        else if (page is HelpPage help)
        {
            help.BackRequested += (s, e) => _navigation.GoBack();
            help.ModelManagerRequested += (s, e) => _navigation.Navigate<ModelManagerPage>();
        }
    }

    private void ToggleOfflineMode()
    {
        _isOffline = !_isOffline;
        AppBar.IsOffline = _isOffline;
        StatusStrip.SetConnectivityStatus(_isOffline);
    }

    private void SetOfflineMode(bool isOffline)
    {
        _isOffline = isOffline;
        AppBar.IsOffline = _isOffline;
        StatusStrip.SetConnectivityStatus(_isOffline);
    }

    private void ShowCommandPalette()
    {
        CommandPaletteOverlay.Visibility = Visibility.Visible;
        CommandPalette.Reset();
        CommandPalette.Focus();
    }

    private void HideCommandPalette()
    {
        CommandPaletteOverlay.Visibility = Visibility.Collapsed;
    }

    private void OnModelSelected(object? sender, string modelName)
    {
        AppBar.SetSelectedModel(modelName);
        StatusStrip.SetModelStatus(modelName, true);
    }

    private void OnNewSessionRequested(object? sender, EventArgs e)
    {
        // Clear current conversation and navigate home
        _conversationVm.ClearConversation();
        ConversationView.Composer.Clear();
        _navigation.GoHome();
    }

    private void NavigateHomeInternal()
    {
        // Remove current page
        if (_currentPage != null)
        {
            PageHost.Children.Remove(_currentPage);
            _currentPage = null;
        }

        // Show home view, hide page host
        HomeView.Visibility = Visibility.Visible;
        PageHost.Visibility = Visibility.Collapsed;
    }
}
