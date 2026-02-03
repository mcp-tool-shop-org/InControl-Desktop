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
    public SettingsPage()
    {
        this.InitializeComponent();
        SetupEventHandlers();
        InitializeThemeComboBox();
    }

    private void InitializeThemeComboBox()
    {
        // Set initial selection based on current theme
        ThemeComboBox.SelectedIndex = ThemeService.Instance.GetThemeIndex();
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
            // Future: Implement settings search/filter
            var searchText = sender.Text;
            // Filter visible settings based on search
        }
    }
}
