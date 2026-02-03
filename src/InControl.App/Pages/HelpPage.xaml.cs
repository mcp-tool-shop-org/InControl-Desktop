using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace InControl.App.Pages;

/// <summary>
/// Built-in Help Center - no external wiki required.
/// Provides searchable documentation, troubleshooting, and diagnostics export.
/// </summary>
public sealed partial class HelpPage : UserControl
{
    public HelpPage()
    {
        this.InitializeComponent();
        SetupEventHandlers();
    }

    /// <summary>
    /// Event raised when user wants to go back.
    /// </summary>
    public event EventHandler? BackRequested;

    /// <summary>
    /// Event raised when Model Manager should open.
    /// </summary>
    public event EventHandler? ModelManagerRequested;

    private void SetupEventHandlers()
    {
        BackButton.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);

        // Navigation
        GettingStartedNav.Checked += (s, e) => ShowSection("GettingStarted");
        ModelsNav.Checked += (s, e) => ShowSection("Models");
        AssistantNav.Checked += (s, e) => ShowSection("Assistant");
        ExtensionsNav.Checked += (s, e) => ShowSection("Extensions");
        PoliciesNav.Checked += (s, e) => ShowSection("Policies");
        ConnectivityNav.Checked += (s, e) => ShowSection("Connectivity");
        TroubleshootingNav.Checked += (s, e) => ShowSection("Troubleshooting");
        ShortcutsNav.Checked += (s, e) => ShowSection("Shortcuts");

        // Actions
        CopyDiagnosticsButton.Click += OnCopyDiagnosticsClick;
        ExportSupportBundleButton.Click += OnExportSupportBundleClick;
        ExportSupportBundleHelpButton.Click += OnExportSupportBundleClick;
        OpenModelManagerHelpButton.Click += (s, e) => ModelManagerRequested?.Invoke(this, EventArgs.Empty);

        // Search
        HelpSearch.TextChanged += OnSearchTextChanged;
    }

    private void ShowSection(string sectionName)
    {
        // Hide all sections
        GettingStartedSection.Visibility = Visibility.Collapsed;
        ModelsSection.Visibility = Visibility.Collapsed;
        ShortcutsSection.Visibility = Visibility.Collapsed;
        TroubleshootingSection.Visibility = Visibility.Collapsed;

        // Show requested section
        switch (sectionName)
        {
            case "GettingStarted":
                GettingStartedSection.Visibility = Visibility.Visible;
                break;
            case "Models":
                ModelsSection.Visibility = Visibility.Visible;
                break;
            case "Shortcuts":
                ShortcutsSection.Visibility = Visibility.Visible;
                break;
            case "Troubleshooting":
                TroubleshootingSection.Visibility = Visibility.Visible;
                break;
            // Add more sections as needed
            default:
                GettingStartedSection.Visibility = Visibility.Visible;
                break;
        }
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var searchText = sender.Text.ToLowerInvariant();
            // Future: Implement help content search
        }
    }

    private void OnCopyDiagnosticsClick(object sender, RoutedEventArgs e)
    {
        var diagnostics = GenerateDiagnostics();

        var dataPackage = new DataPackage();
        dataPackage.SetText(diagnostics);
        Clipboard.SetContent(dataPackage);

        // Show confirmation (in production, use a proper notification)
    }

    private async void OnExportSupportBundleClick(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("ZIP Archive", new[] { ".zip" });
        picker.SuggestedFileName = $"incontrol-support-{DateTime.Now:yyyyMMdd-HHmmss}";

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            // Generate and save support bundle
            var diagnostics = GenerateDiagnostics();
            await Windows.Storage.FileIO.WriteTextAsync(file, diagnostics);
        }
    }

    private string GenerateDiagnostics()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== InControl Diagnostics ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine("== System Information ==");
        sb.AppendLine($"OS: {Environment.OSVersion}");
        sb.AppendLine($".NET Version: {Environment.Version}");
        sb.AppendLine($"Machine: {Environment.MachineName}");
        sb.AppendLine($"Processors: {Environment.ProcessorCount}");
        sb.AppendLine();

        sb.AppendLine("== Application ==");
        sb.AppendLine($"Version: 1.0.0");
        sb.AppendLine($"Working Directory: {Environment.CurrentDirectory}");
        sb.AppendLine($"App Data: {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}");
        sb.AppendLine();

        sb.AppendLine("== Memory ==");
        sb.AppendLine($"Working Set: {Environment.WorkingSet / 1024 / 1024} MB");
        sb.AppendLine();

        return sb.ToString();
    }
}
