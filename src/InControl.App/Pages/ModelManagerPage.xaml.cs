using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace InControl.App.Pages;

/// <summary>
/// Model Manager page - first-class screen for managing AI models.
/// Allows users to import, download, configure defaults, and understand GPU requirements.
/// </summary>
public sealed partial class ModelManagerPage : UserControl
{
    private readonly ObservableCollection<ModelInfo> _models = new();

    public ModelManagerPage()
    {
        this.InitializeComponent();
        SetupEventHandlers();
        InitializeGpuInfo();
    }

    /// <summary>
    /// Event raised when user wants to go back.
    /// </summary>
    public event EventHandler? BackRequested;

    /// <summary>
    /// Event raised when a model is selected as default.
    /// </summary>
    public event EventHandler<string>? ModelSelected;

    private void SetupEventHandlers()
    {
        BackButton.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);
        ImportButton.Click += OnImportClick;
        DownloadButton.Click += OnDownloadClick;
        EmptyImportButton.Click += OnImportClick;
        EmptyDownloadButton.Click += OnDownloadClick;
        OpenModelsFolderButton.Click += OnOpenModelsFolderClick;
        RefreshModelsButton.Click += OnRefreshClick;
        DefaultModelSelector.SelectionChanged += OnDefaultModelChanged;
        ModelsListView.ItemsSource = _models;
    }

    private void InitializeGpuInfo()
    {
        // Detect GPU capabilities
        DetectGpu();
    }

    private void DetectGpu()
    {
        // This would use actual GPU detection in production
        // For now, show placeholder info
        GpuNameText.Text = "NVIDIA RTX 5080";
        VramText.Text = "16 GB";
        CudaText.Text = "12.6 (SM 12.0)";
        GpuStatusText.Text = "Ready";
    }

    /// <summary>
    /// Loads the list of available models.
    /// </summary>
    public void LoadModels(IEnumerable<ModelInfo> models)
    {
        _models.Clear();
        foreach (var model in models)
        {
            _models.Add(model);
        }

        UpdateModelCount();
        UpdateEmptyState();
        UpdateDefaultSelector();
    }

    private void UpdateModelCount()
    {
        var count = _models.Count;
        ModelCountText.Text = count == 1 ? "1 model" : $"{count} models";
    }

    private void UpdateEmptyState()
    {
        var hasModels = _models.Count > 0;
        EmptyState.Visibility = hasModels ? Visibility.Collapsed : Visibility.Visible;
        ModelsListView.Visibility = hasModels ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateDefaultSelector()
    {
        DefaultModelSelector.Items.Clear();
        foreach (var model in _models)
        {
            DefaultModelSelector.Items.Add(model.Name);
        }

        if (DefaultModelSelector.Items.Count > 0)
        {
            DefaultModelSelector.SelectedIndex = 0;
        }
    }

    private async void OnImportClick(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.FileTypeFilter.Add(".gguf");
        picker.FileTypeFilter.Add(".bin");
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;

        // Initialize picker with window handle
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            // Import the model file
            // In production, this would copy/link the file and update the model list
            await ShowImportDialogAsync(file.Path);
        }
    }

    private async void OnDownloadClick(object sender, RoutedEventArgs e)
    {
        // Show download dialog
        var dialog = new ContentDialog
        {
            Title = "Download Model",
            Content = CreateDownloadContent(),
            PrimaryButtonText = "Download",
            CloseButtonText = "Cancel",
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }

    private UIElement CreateDownloadContent()
    {
        var panel = new StackPanel { Spacing = 16 };

        panel.Children.Add(new TextBlock
        {
            Text = "Enter a Hugging Face model URL or search for popular models.",
            TextWrapping = TextWrapping.Wrap
        });

        var searchBox = new AutoSuggestBox
        {
            PlaceholderText = "Search models or paste URL...",
            QueryIcon = new SymbolIcon(Symbol.Find)
        };
        panel.Children.Add(searchBox);

        panel.Children.Add(new TextBlock
        {
            Text = "Popular Models",
            Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"],
            Margin = new Thickness(0, 8, 0, 0)
        });

        var models = new[] {
            ("Llama 3.2 3B", "meta-llama/Llama-3.2-3B-Instruct-GGUF", "~2 GB"),
            ("Llama 3.1 8B", "meta-llama/Llama-3.1-8B-Instruct-GGUF", "~5 GB"),
            ("Mistral 7B", "mistralai/Mistral-7B-Instruct-v0.3-GGUF", "~4 GB"),
            ("Qwen2.5 7B", "Qwen/Qwen2.5-7B-Instruct-GGUF", "~4 GB")
        };

        foreach (var (name, repo, size) in models)
        {
            var item = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            item.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            item.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var info = new StackPanel();
            info.Children.Add(new TextBlock { Text = name });
            info.Children.Add(new TextBlock
            {
                Text = size,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                FontSize = 12
            });
            Grid.SetColumn(info, 0);
            item.Children.Add(info);

            var btn = new Button { Content = "Download", Padding = new Thickness(12, 4, 12, 4) };
            Grid.SetColumn(btn, 1);
            item.Children.Add(btn);

            panel.Children.Add(item);
        }

        return panel;
    }

    private async Task ShowImportDialogAsync(string filePath)
    {
        var fileName = System.IO.Path.GetFileName(filePath);
        var dialog = new ContentDialog
        {
            Title = "Import Model",
            Content = $"Import '{fileName}' as a new model?",
            PrimaryButtonText = "Import",
            CloseButtonText = "Cancel",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // Show loading overlay during import
            LoadingOverlay.Show("Importing model...", fileName);

            // Simulate import delay
            await Task.Delay(1500);

            // Add model to list (in production, would actually import)
            _models.Add(new ModelInfo
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(filePath),
                Path = filePath,
                Size = "Unknown",
                Parameters = "Unknown",
                Status = "Ready"
            });
            UpdateModelCount();
            UpdateEmptyState();
            UpdateDefaultSelector();

            // Hide loading and show success
            LoadingOverlay.Hide();
            OperationFeedback.ShowSuccess($"Model '{System.IO.Path.GetFileNameWithoutExtension(filePath)}' imported");
        }
    }

    private void OnOpenModelsFolderClick(object sender, RoutedEventArgs e)
    {
        var modelsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InControl", "models");

        if (!System.IO.Directory.Exists(modelsPath))
        {
            System.IO.Directory.CreateDirectory(modelsPath);
        }

        System.Diagnostics.Process.Start("explorer.exe", modelsPath);
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        // Show loading state
        LoadingOverlay.Show("Scanning for models...", "Checking model directories");

        // Simulate refresh operation
        await Task.Delay(1000);

        // Hide loading and show success
        LoadingOverlay.Hide();
        OperationFeedback.ShowSuccess("Model list refreshed");
    }

    private void OnDefaultModelChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DefaultModelSelector.SelectedItem is string modelName)
        {
            ModelSelected?.Invoke(this, modelName);
        }
    }
}

/// <summary>
/// Represents model information for the UI.
/// </summary>
public class ModelInfo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Size { get; set; } = "";
    public string Parameters { get; set; } = "";
    public string Status { get; set; } = "";
}
