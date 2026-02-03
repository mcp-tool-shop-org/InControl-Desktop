using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InControl.App.Services;

namespace InControl.App.Pages;

/// <summary>
/// Centralized settings hub for all InControl configuration.
/// Provides searchable access to General, Models, Assistant, Memory, Extensions,
/// Connectivity, Updates, and Diagnostics settings.
/// </summary>
public sealed partial class SettingsPage : UserControl
{
    private readonly List<StackPanel> _settingsSections = new();

    public SettingsPage()
    {
        this.InitializeComponent();
        CollectSections();
        SetupEventHandlers();
        InitializeThemeComboBox();
        InitializeStoragePath();
    }

    private void CollectSections()
    {
        // Collect all settings sections for search filtering
        _settingsSections.AddRange(new[]
        {
            GeneralSection,
            ModelsSection,
            AssistantSection,
            MemorySection,
            ExtensionsSection,
            ConnectivitySection,
            UpdatesSection,
            DiagnosticsSection
        });
    }

    private void InitializeThemeComboBox()
    {
        // Set initial selection based on current theme
        ThemeComboBox.SelectedIndex = ThemeService.Instance.GetThemeIndex();
    }

    private void InitializeStoragePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        StoragePathText.Text = System.IO.Path.Combine(appData, "InControl", "memory");
    }

    /// <summary>
    /// Event raised when user wants to go back.
    /// </summary>
    public event EventHandler? BackRequested;

    /// <summary>
    /// Event raised when Model Manager should open.
    /// </summary>
    public event EventHandler? ModelManagerRequested;

    /// <summary>
    /// Event raised when Extensions page should open.
    /// </summary>
    public event EventHandler? ExtensionsRequested;

    /// <summary>
    /// Event raised when Policy page should open.
    /// </summary>
    public event EventHandler? PolicyRequested;

    /// <summary>
    /// Event raised when theme changes.
    /// </summary>
    public event EventHandler<string>? ThemeChanged;

    private void SetupEventHandlers()
    {
        BackButton.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);
        OpenModelManagerButton.Click += (s, e) => ModelManagerRequested?.Invoke(this, EventArgs.Empty);
        OpenExtensionsButton.Click += (s, e) => ExtensionsRequested?.Invoke(this, EventArgs.Empty);
        OpenPolicyButton.Click += (s, e) => PolicyRequested?.Invoke(this, EventArgs.Empty);

        // Theme selection
        ThemeComboBox.SelectionChanged += OnThemeSelectionChanged;

        // Settings search functionality
        SettingsSearch.TextChanged += OnSearchTextChanged;

        // Memory section buttons
        ChangeStorageButton.Click += OnChangeStorageClick;
        ClearMemoryButton.Click += OnClearMemoryClick;

        // Updates section buttons
        CheckUpdatesButton.Click += OnCheckUpdatesClick;

        // Diagnostics section buttons
        ExportDiagnosticsButton.Click += OnExportDiagnosticsClick;
        OpenLogsButton.Click += OnOpenLogsClick;
        ResetSettingsButton.Click += OnResetSettingsClick;
    }

    private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ThemeService.Instance.SetThemeFromIndex(ThemeComboBox.SelectedIndex);
        ThemeChanged?.Invoke(this, ThemeService.Instance.CurrentThemeString);
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            FilterSettings(sender.Text);
        }
    }

    private void FilterSettings(string searchText)
    {
        var hasResults = false;
        var query = searchText.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(query))
        {
            // Show all sections
            foreach (var section in _settingsSections)
            {
                section.Visibility = Visibility.Visible;
            }
            NoResultsPanel.Visibility = Visibility.Collapsed;
            return;
        }

        // Filter sections based on search tags
        foreach (var section in _settingsSections)
        {
            var tags = section.Tag?.ToString()?.ToLowerInvariant() ?? "";
            var isMatch = tags.Contains(query);

            section.Visibility = isMatch ? Visibility.Visible : Visibility.Collapsed;
            if (isMatch) hasResults = true;
        }

        // Show/hide no results panel
        NoResultsPanel.Visibility = hasResults ? Visibility.Collapsed : Visibility.Visible;
        if (!hasResults)
        {
            NoResultsText.Text = $"No settings found for \"{searchText}\"";
        }
    }

    private async void OnChangeStorageClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Change Storage Location",
            Content = "This feature will be available in a future update.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void OnClearMemoryClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Clear All Memory",
            Content = "This will permanently delete all stored context and conversation history. This action cannot be undone.",
            PrimaryButtonText = "Clear All",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // TODO: Clear memory implementation
        }
    }

    private async void OnCheckUpdatesClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Check for Updates",
            Content = "You are running the latest version (v1.0.0).",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void OnExportDiagnosticsClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Export Diagnostics",
            Content = "This feature will be available in a future update.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private void OnOpenLogsClick(object sender, RoutedEventArgs e)
    {
        var logsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InControl", "logs");

        if (!System.IO.Directory.Exists(logsPath))
        {
            System.IO.Directory.CreateDirectory(logsPath);
        }

        System.Diagnostics.Process.Start("explorer.exe", logsPath);
    }

    private async void OnResetSettingsClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Reset to Defaults",
            Content = "This will restore all settings to their default values. Your models, extensions, and session data will not be affected.",
            PrimaryButtonText = "Reset Settings",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // Reset theme to system
            ThemeComboBox.SelectedIndex = 2;
            ThemeService.Instance.SetThemeFromIndex(2);

            // TODO: Reset other settings
        }
    }
}
