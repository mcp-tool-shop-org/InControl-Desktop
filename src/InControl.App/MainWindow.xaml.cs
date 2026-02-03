using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using InControl.App.Pages;

namespace InControl.App;

/// <summary>
/// Main application window with page navigation support.
/// Provides global control bar, status strip, and seamless navigation between pages.
/// </summary>
public sealed partial class MainWindow : Window
{
    private UserControl? _currentPage;
    private bool _isOffline;

    public MainWindow()
    {
        InitializeComponent();
        SetupWindow();
        SetupEventHandlers();
        InitializeStatusStrip();
    }

    private void SetupWindow()
    {
        var appWindow = this.AppWindow;
        appWindow.Title = "InControl - Local AI Chat";
        appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
    }

    private void SetupEventHandlers()
    {
        // AppBar events
        AppBar.SettingsRequested += (s, e) => NavigateToPage<SettingsPage>();
        AppBar.AssistantRequested += (s, e) => NavigateToPage<AssistantPage>();
        AppBar.ExtensionsRequested += (s, e) => NavigateToPage<ExtensionsPage>();
        AppBar.PolicyRequested += (s, e) => NavigateToPage<PolicyPage>();
        AppBar.ConnectivityRequested += (s, e) => NavigateToPage<ConnectivityPage>();
        AppBar.HelpRequested += (s, e) => NavigateToPage<HelpPage>();
        AppBar.ModelManagerRequested += (s, e) => NavigateToPage<ModelManagerPage>();
        AppBar.CommandPaletteRequested += (s, e) => ShowCommandPalette();

        // Command Palette events
        CommandPalette.CommandExecuted += OnCommandExecuted;
        CommandPalette.CloseRequested += (s, e) => HideCommandPalette();

        // StatusStrip events
        StatusStrip.ModelClicked += (s, e) => NavigateToPage<ModelManagerPage>();
        StatusStrip.DeviceClicked += (s, e) => NavigateToPage<ModelManagerPage>();
        StatusStrip.ConnectivityClicked += (s, e) => NavigateToPage<ConnectivityPage>();
        StatusStrip.PolicyClicked += (s, e) => NavigateToPage<PolicyPage>();
        StatusStrip.AssistantClicked += (s, e) => NavigateToPage<AssistantPage>();

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

        // Escape - Close overlays
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            if (CommandPaletteOverlay.Visibility == Visibility.Visible)
            {
                HideCommandPalette();
                e.Handled = true;
            }
            else if (_currentPage != null)
            {
                NavigateHome();
                e.Handled = true;
            }
        }
    }

    private void OnCommandExecuted(object? sender, string commandId)
    {
        switch (commandId)
        {
            case "new-session":
                NavigateHome();
                // Trigger new session creation
                break;
            case "open-settings":
                NavigateToPage<SettingsPage>();
                break;
            case "open-model-manager":
                NavigateToPage<ModelManagerPage>();
                break;
            case "toggle-offline":
                ToggleOfflineMode();
                break;
            case "open-extensions":
                NavigateToPage<ExtensionsPage>();
                break;
            case "open-assistant":
                NavigateToPage<AssistantPage>();
                break;
            case "view-policy":
                NavigateToPage<PolicyPage>();
                break;
            case "open-connectivity":
                NavigateToPage<ConnectivityPage>();
                break;
            case "open-help":
                NavigateToPage<HelpPage>();
                break;
            case "search-sessions":
                NavigateHome();
                // Focus session search
                break;
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

    private void NavigateToPage<T>() where T : UserControl, new()
    {
        // Remove current page if any
        if (_currentPage != null)
        {
            PageHost.Children.Remove(_currentPage);
        }

        // Create and add new page
        var page = new T();
        _currentPage = page;

        // Wire up back navigation and events
        if (page is SettingsPage settings)
        {
            settings.BackRequested += (s, e) => NavigateHome();
            settings.ModelManagerRequested += (s, e) => NavigateToPage<ModelManagerPage>();
            settings.ExtensionsRequested += (s, e) => NavigateToPage<ExtensionsPage>();
            settings.PolicyRequested += (s, e) => NavigateToPage<PolicyPage>();
        }
        else if (page is ModelManagerPage modelManager)
        {
            modelManager.BackRequested += (s, e) => NavigateHome();
            modelManager.ModelSelected += OnModelSelected;
        }
        else if (page is AssistantPage assistant)
        {
            assistant.BackRequested += (s, e) => NavigateHome();
        }
        else if (page is ExtensionsPage extensions)
        {
            extensions.BackRequested += (s, e) => NavigateHome();
        }
        else if (page is PolicyPage policy)
        {
            policy.BackRequested += (s, e) => NavigateHome();
            policy.OfflineModeChanged += (s, isOffline) => SetOfflineMode(isOffline);
        }
        else if (page is ConnectivityPage connectivity)
        {
            connectivity.BackRequested += (s, e) => NavigateHome();
            connectivity.OfflineModeChanged += (s, isOffline) => SetOfflineMode(isOffline);
            connectivity.IsOffline = _isOffline;
        }
        else if (page is HelpPage help)
        {
            help.BackRequested += (s, e) => NavigateHome();
            help.ModelManagerRequested += (s, e) => NavigateToPage<ModelManagerPage>();
        }

        PageHost.Children.Add(page);

        // Show page host, hide home view
        HomeView.Visibility = Visibility.Collapsed;
        PageHost.Visibility = Visibility.Visible;
    }

    private void OnModelSelected(object? sender, string modelName)
    {
        AppBar.SetSelectedModel(modelName);
        StatusStrip.SetModelStatus(modelName, true);
    }

    private void NavigateHome()
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
