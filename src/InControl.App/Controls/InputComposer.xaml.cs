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
        IntentInput.KeyDown += OnIntentInputKeyDown;
        RunButton.Click += OnRunButtonClick;
        CancelButton.Click += OnCancelButtonClick;
        AttachFileButton.Click += OnAttachFileButtonClick;
        ModelSelector.SelectionChanged += OnModelSelectionChanged;
        DisabledActionButton.Click += OnDisabledActionClick;

        // Context menu items
        AddPreviousOutputMenuItem.Click += OnAddPreviousOutputClick;
        AddFileMenuItem.Click += OnAddFileClick;
        ClearContextMenuItem.Click += OnClearContextClick;

        // Initial tooltip state
        UpdateTooltips();
    }

    /// <summary>
    /// Handles Enter to send and Shift+Enter for new line.
    /// </summary>
    private void OnIntentInputKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key != Windows.System.VirtualKey.Enter)
            return;

        var shiftDown = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(
            Windows.System.VirtualKey.Shift)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        if (shiftDown)
        {
            // Shift+Enter → insert newline manually
            e.Handled = true;
            var textBox = (TextBox)sender;
            var selStart = textBox.SelectionStart;
            var text = textBox.Text;
            textBox.Text = text.Insert(selStart, "\r\n");
            textBox.SelectionStart = selStart + 2;
            return;
        }

        // Bare Enter → send
        e.Handled = true;

        if (CanRun())
        {
            var args = new RunRequestedEventArgs(IntentInput.Text.TrimEnd('\r', '\n'), SelectedModel);
            RunRequested?.Invoke(this, args);
        }
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
        // Only show banner for actionable issues (no models available, offline blocked)
        string? reason = null;
        string? actionText = null;
        bool showAction = false;

        if (_isOfflineBlocked)
        {
            reason = "Run is blocked by connectivity policy";
        }
        else if (ModelSelector.SelectedItem == null && ModelSelector.Items.Count == 0)
        {
            reason = "No models available. Add a model to get started.";
            actionText = "Open Model Manager";
            showAction = true;
        }

        if (reason != null)
        {
            DisabledBanner.Visibility = Visibility.Visible;
            DisabledReasonText.Text = reason;
            DisabledActionButton.Content = actionText;
            DisabledActionButton.Visibility = showAction ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            DisabledBanner.Visibility = Visibility.Collapsed;
        }

        UpdateTooltips();
    }

    private void UpdateTooltips()
    {
        // Update Run button tooltip based on enabled state
        if (RunButton.IsEnabled)
        {
            ToolTipService.SetToolTip(RunButton, "Run inference (Enter)");
        }
        else
        {
            var hasModel = ModelSelector.SelectedItem != null;
            var hasText = !string.IsNullOrWhiteSpace(IntentInput.Text);

            if (_isOfflineBlocked)
            {
                ToolTipService.SetToolTip(RunButton, "Blocked by connectivity policy");
            }
            else if (!hasModel)
            {
                ToolTipService.SetToolTip(RunButton, "Select a model to enable");
            }
            else if (!hasText)
            {
                ToolTipService.SetToolTip(RunButton, "Type a prompt to enable");
            }
            else
            {
                ToolTipService.SetToolTip(RunButton, "Waiting for current operation");
            }
        }

        // Update Clear Context tooltip based on enabled state
        if (ClearContextMenuItem.IsEnabled)
        {
            ToolTipService.SetToolTip(ClearContextMenuItem, "Remove all context items");
        }
        else
        {
            ToolTipService.SetToolTip(ClearContextMenuItem, "No context items to clear");
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
