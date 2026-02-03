using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using InControl.App.Pages;
using InControl.App.Services;

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

    public MainWindow()
    {
        InitializeComponent();
        SetupWindow();
        InitializeTheme();
        SetupNavigation();
        SetupEventHandlers();
        InitializeStatusStrip();
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
            modelManager.BackRequested += (s, e) => _navigation.GoBack();
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
        // Navigate home to show empty conversation
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
