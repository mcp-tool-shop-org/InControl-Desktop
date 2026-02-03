using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InControl.Core.UX;

namespace InControl.App.Controls;

/// <summary>
/// Input composer control panel for entering prompts and managing execution.
/// Follows the control panel metaphor - this is a command interface, not a chat box.
/// Provides disabled state explanations so users always know what to do next.
/// </summary>
public sealed partial class InputComposer : UserControl
{
    private ExecutionState _executionState = ExecutionState.Idle;
    private bool _isOfflineBlocked;

    public InputComposer()
    {
        this.InitializeComponent();
        SetupEventHandlers();
    }

    #region Properties

    /// <summary>
    /// The current execution state.
    /// </summary>
    public ExecutionState ExecutionState
    {
        get => _executionState;
        set
        {
            if (_executionState != value)
            {
                _executionState = value;
                UpdateUIForState();
            }
        }
    }

    /// <summary>
    /// The current intent text.
    /// </summary>
    public string IntentText
    {
        get => IntentInput.Text;
        set => IntentInput.Text = value;
    }

    /// <summary>
    /// The selected model name.
    /// </summary>
    public string? SelectedModel => ModelSelector.SelectedItem as string;

    /// <summary>
    /// Whether the Run button is blocked due to offline policy.
    /// </summary>
    public bool IsOfflineBlocked
    {
        get => _isOfflineBlocked;
        set
        {
            _isOfflineBlocked = value;
            UpdateDisabledState();
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the user initiates a run.
    /// </summary>
    public event EventHandler<RunRequestedEventArgs>? RunRequested;

    /// <summary>
    /// Event raised when the user cancels execution.
    /// </summary>
    public event EventHandler? CancelRequested;

    /// <summary>
    /// Event raised when a file attachment is requested.
    /// </summary>
    public event EventHandler? AttachFileRequested;

    /// <summary>
    /// Event raised when Model Manager should open (from disabled banner).
    /// </summary>
    public event EventHandler? ModelManagerRequested;

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the available models for selection.
    /// </summary>
    public void SetAvailableModels(IEnumerable<string> models)
    {
        ModelSelector.Items.Clear();
        foreach (var model in models)
        {
            ModelSelector.Items.Add(model);
        }

        if (ModelSelector.Items.Count > 0)
        {
            ModelSelector.SelectedIndex = 0;
        }

        UpdateDisabledState();
    }

    /// <summary>
    /// Updates the context count display.
    /// </summary>
    public void SetContextCount(int count)
    {
        if (count > 0)
        {
            ContextCountText.Text = count.ToString();
            ContextCountText.Visibility = Visibility.Visible;
            ClearContextMenuItem.IsEnabled = true;
        }
        else
        {
            ContextCountText.Visibility = Visibility.Collapsed;
            ClearContextMenuItem.IsEnabled = false;
        }
    }

    /// <summary>
    /// Clears the input and resets to idle state.
    /// </summary>
    public void Clear()
    {
        IntentInput.Text = string.Empty;
        ExecutionState = ExecutionState.Idle;
    }

    /// <summary>
    /// Focuses the input area.
    /// </summary>
    public void FocusInput()
    {
        IntentInput.Focus(FocusState.Programmatic);
    }

    #endregion

    #region Private Methods

    private void SetupEventHandlers()
    {
        IntentInput.TextChanged += OnIntentTextChanged;
        RunButton.Click += OnRunButtonClick;
        CancelButton.Click += OnCancelButtonClick;
        AttachFileButton.Click += OnAttachFileButtonClick;
        ModelSelector.SelectionChanged += OnModelSelectionChanged;
        DisabledActionButton.Click += OnDisabledActionClick;

        // Context menu items
        AddPreviousOutputMenuItem.Click += OnAddPreviousOutputClick;
        AddFileMenuItem.Click += OnAddFileClick;
        ClearContextMenuItem.Click += OnClearContextClick;
    }

    private void OnIntentTextChanged(object sender, TextChangedEventArgs e)
    {
        var text = IntentInput.Text;
        CharacterCount.Text = $"{text.Length} characters";
        UpdateRunButtonState();
        UpdateDisabledState();
    }

    private void OnRunButtonClick(object sender, RoutedEventArgs e)
    {
        if (CanRun())
        {
            var args = new RunRequestedEventArgs(IntentInput.Text, SelectedModel);
            RunRequested?.Invoke(this, args);
        }
    }

    private void OnCancelButtonClick(object sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnAttachFileButtonClick(object sender, RoutedEventArgs e)
    {
        AttachFileRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnModelSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateRunButtonState();
        UpdateDisabledState();
    }

    private void OnDisabledActionClick(object sender, RoutedEventArgs e)
    {
        ModelManagerRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnAddPreviousOutputClick(object sender, RoutedEventArgs e)
    {
        await ShowNotImplementedDialog("Add Previous Output");
    }

    private void OnAddFileClick(object sender, RoutedEventArgs e)
    {
        AttachFileRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnClearContextClick(object sender, RoutedEventArgs e)
    {
        // Clear context logic - will reset context count
        SetContextCount(0);
    }

    private async System.Threading.Tasks.Task ShowNotImplementedDialog(string feature)
    {
        var dialog = new ContentDialog
        {
            Title = "Coming Soon",
            Content = $"{feature} will be available in a future update.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private void UpdateUIForState()
    {
        var isExecuting = _executionState.IsExecuting();
        var allowsInput = _executionState.AllowsInput();

        // Input area
        IntentInput.IsEnabled = allowsInput;

        // Buttons
        CancelButton.Visibility = isExecuting ? Visibility.Visible : Visibility.Collapsed;
        RunButton.Visibility = isExecuting ? Visibility.Collapsed : Visibility.Visible;

        // Model selector
        ModelSelector.IsEnabled = allowsInput;

        // Context buttons
        AttachFileButton.IsEnabled = allowsInput;
        ContextMenuButton.IsEnabled = allowsInput;

        UpdateRunButtonState();
        UpdateDisabledState();
    }

    private void UpdateRunButtonState()
    {
        var hasText = !string.IsNullOrWhiteSpace(IntentInput.Text);
        var hasModel = ModelSelector.SelectedItem != null;
        var allowsInput = _executionState.AllowsInput();

        RunButton.IsEnabled = hasText && hasModel && allowsInput && !_isOfflineBlocked;
    }

    private void UpdateDisabledState()
    {
        var hasText = !string.IsNullOrWhiteSpace(IntentInput.Text);
        var hasModel = ModelSelector.SelectedItem != null;
        var allowsInput = _executionState.AllowsInput();

        // Determine the disabled reason (priority order)
        string? reason = null;
        string? actionText = null;
        bool showAction = false;

        if (_isOfflineBlocked)
        {
            reason = "Run is blocked by connectivity policy";
            actionText = "View Policy";
            showAction = false; // Policy can't be changed from here
        }
        else if (!hasModel && ModelSelector.Items.Count == 0)
        {
            reason = "No models available. Add a model to get started.";
            actionText = "Open Model Manager";
            showAction = true;
        }
        else if (!hasModel)
        {
            reason = "Select a model from the dropdown to enable Run";
            showAction = false;
        }
        else if (!hasText && hasModel)
        {
            reason = "Type a prompt to enable Run";
            showAction = false;
        }
        else if (!allowsInput)
        {
            reason = "Wait for the current operation to complete";
            showAction = false;
        }

        // Show or hide the disabled banner
        if (reason != null && !RunButton.IsEnabled)
        {
            DisabledBanner.Visibility = Visibility.Visible;
            DisabledReasonText.Text = reason;
            DisabledActionButton.Content = actionText;
            DisabledActionButton.Visibility = showAction ? Visibility.Visible : Visibility.Collapsed;
            KeyboardHint.Visibility = Visibility.Collapsed;
        }
        else
        {
            DisabledBanner.Visibility = Visibility.Collapsed;
            KeyboardHint.Visibility = Visibility.Visible;
        }
    }

    private bool CanRun()
    {
        return !string.IsNullOrWhiteSpace(IntentInput.Text)
            && ModelSelector.SelectedItem != null
            && _executionState.AllowsInput()
            && !_isOfflineBlocked;
    }

    #endregion
}

/// <summary>
/// Event arguments for run requests.
/// </summary>
public sealed class RunRequestedEventArgs : EventArgs
{
    public RunRequestedEventArgs(string intent, string? model)
    {
        Intent = intent;
        Model = model;
    }

    /// <summary>
    /// The user's intent text.
    /// </summary>
    public string Intent { get; }

    /// <summary>
    /// The selected model name.
    /// </summary>
    public string? Model { get; }
}
