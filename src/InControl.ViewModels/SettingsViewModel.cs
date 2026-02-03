using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using InControl.Core.Models;
using InControl.Inference.Interfaces;
using InControl.Services.Interfaces;

namespace InControl.ViewModels;

/// <summary>
/// ViewModel for the settings page.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IModelManager _modelManager;

    /// <summary>
    /// Available themes.
    /// </summary>
    public static IReadOnlyList<string> Themes { get; } = ["System", "Light", "Dark"];

    /// <summary>
    /// The selected theme.
    /// </summary>
    [ObservableProperty]
    private string _selectedTheme = "System";

    /// <summary>
    /// Whether to show the tray icon.
    /// </summary>
    [ObservableProperty]
    private bool _showTrayIcon = true;

    /// <summary>
    /// Whether to minimize to tray.
    /// </summary>
    [ObservableProperty]
    private bool _minimizeToTray = true;

    /// <summary>
    /// Whether to check for updates.
    /// </summary>
    [ObservableProperty]
    private bool _checkForUpdates = true;

    /// <summary>
    /// The Ollama base URL.
    /// </summary>
    [ObservableProperty]
    private string _ollamaBaseUrl = "http://localhost:11434";

    /// <summary>
    /// The default model.
    /// </summary>
    [ObservableProperty]
    private string? _defaultModel;

    /// <summary>
    /// Default temperature.
    /// </summary>
    [ObservableProperty]
    private double _defaultTemperature = 0.7;

    /// <summary>
    /// Whether to stream responses.
    /// </summary>
    [ObservableProperty]
    private bool _streamResponses = true;

    /// <summary>
    /// Whether to show token counts.
    /// </summary>
    [ObservableProperty]
    private bool _showTokenCounts = true;

    /// <summary>
    /// Whether to show generation speed.
    /// </summary>
    [ObservableProperty]
    private bool _showGenerationSpeed = true;

    /// <summary>
    /// Available models for selection.
    /// </summary>
    public ObservableCollection<ModelInfo> AvailableModels { get; } = [];

    /// <summary>
    /// Ollama connection status.
    /// </summary>
    [ObservableProperty]
    private string _connectionStatus = "Not connected";

    /// <summary>
    /// Whether Ollama is connected.
    /// </summary>
    [ObservableProperty]
    private bool _isConnected;

    public SettingsViewModel(
        ISettingsService settingsService,
        IModelManager modelManager,
        ILogger<SettingsViewModel> logger)
        : base(logger)
    {
        _settingsService = settingsService;
        _modelManager = modelManager;

        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        var app = _settingsService.AppOptions;
        var chat = _settingsService.ChatOptions;
        var inference = _settingsService.InferenceOptions;
        var ollama = _settingsService.OllamaOptions;

        SelectedTheme = app.Theme;
        ShowTrayIcon = app.ShowTrayIcon;
        MinimizeToTray = app.MinimizeToTray;
        CheckForUpdates = app.CheckForUpdates;

        OllamaBaseUrl = ollama.BaseUrl;
        DefaultModel = inference.DefaultModel;
        DefaultTemperature = inference.DefaultTemperature;

        StreamResponses = chat.StreamResponses;
        ShowTokenCounts = chat.ShowTokenCounts;
        ShowGenerationSpeed = chat.ShowGenerationSpeed;
    }

    /// <summary>
    /// Tests the Ollama connection.
    /// </summary>
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        ConnectionStatus = "Testing...";

        await ExecuteAsync(async () =>
        {
            var models = await _modelManager.ListModelsAsync();

            AvailableModels.Clear();
            foreach (var model in models)
            {
                AvailableModels.Add(model);
            }

            IsConnected = true;
            ConnectionStatus = $"Connected ({models.Count} models)";
        });

        if (HasError)
        {
            IsConnected = false;
            ConnectionStatus = $"Failed: {ErrorMessage}";
        }
    }

    /// <summary>
    /// Saves all settings.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        await ExecuteAsync(async () =>
        {
            await _settingsService.UpdateAppOptionsAsync(o =>
            {
                o.Theme = SelectedTheme;
                o.ShowTrayIcon = ShowTrayIcon;
                o.MinimizeToTray = MinimizeToTray;
                o.CheckForUpdates = CheckForUpdates;
            });

            await _settingsService.UpdateOllamaOptionsAsync(o =>
            {
                o.BaseUrl = OllamaBaseUrl;
            });

            await _settingsService.UpdateInferenceOptionsAsync(o =>
            {
                o.DefaultModel = DefaultModel ?? "llama3.2";
                o.DefaultTemperature = DefaultTemperature;
            });

            await _settingsService.UpdateChatOptionsAsync(o =>
            {
                o.StreamResponses = StreamResponses;
                o.ShowTokenCounts = ShowTokenCounts;
                o.ShowGenerationSpeed = ShowGenerationSpeed;
            });
        });
    }

    /// <summary>
    /// Resets settings to defaults.
    /// </summary>
    [RelayCommand]
    private async Task ResetToDefaultsAsync()
    {
        await ExecuteAsync(async () =>
        {
            await _settingsService.ResetToDefaultsAsync();
            LoadCurrentSettings();
        });
    }
}
