using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Volt.Core.UX;

namespace Volt.App.Controls;

/// <summary>
/// Input composer control panel for entering prompts and managing execution.
/// Follows the control panel metaphor - this is a command interface, not a chat box.
/// </summary>
public sealed partial class InputComposer : UserControl
{
    private ExecutionState _executionState = ExecutionState.Idle;

    public InputComposer()
    {
        this.InitializeComponent();
        SetupEventHandlers();
    }

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

    private void SetupEventHandlers()
    {
        IntentInput.TextChanged += OnIntentTextChanged;
        RunButton.Click += OnRunButtonClick;
        CancelButton.Click += OnCancelButtonClick;
        AttachFileButton.Click += OnAttachFileButtonClick;
        ModelSelector.SelectionChanged += OnModelSelectionChanged;
    }

    private void OnIntentTextChanged(object sender, TextChangedEventArgs e)
    {
        var text = IntentInput.Text;
        CharacterCount.Text = $"{text.Length} characters";
        UpdateRunButtonState();
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
    }

    private void UpdateRunButtonState()
    {
        var hasText = !string.IsNullOrWhiteSpace(IntentInput.Text);
        var hasModel = ModelSelector.SelectedItem != null;
        var allowsInput = _executionState.AllowsInput();

        RunButton.IsEnabled = hasText && hasModel && allowsInput;
    }

    private bool CanRun()
    {
        return !string.IsNullOrWhiteSpace(IntentInput.Text)
            && ModelSelector.SelectedItem != null
            && _executionState.AllowsInput();
    }
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
