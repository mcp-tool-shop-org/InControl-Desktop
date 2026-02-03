using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using InControl.Core.UX;

namespace InControl.App.Controls;

/// <summary>
/// Top application bar with app name, model selector, status capsule, and global search.
/// </summary>
public sealed partial class AppBar : UserControl
{
    private ExecutionState _executionState = ExecutionState.Idle;

    public AppBar()
    {
        this.InitializeComponent();
        CancelButton.Click += OnCancelClick;
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
                UpdateStatusDisplay();
            }
        }
    }

    /// <summary>
    /// Event raised when cancel is requested.
    /// </summary>
    public event EventHandler? CancelRequested;

    /// <summary>
    /// Sets the available models in the selector.
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
    /// Gets the currently selected model.
    /// </summary>
    public string? SelectedModel => ModelSelector.SelectedItem as string;

    /// <summary>
    /// Updates the elapsed time display.
    /// </summary>
    public void UpdateElapsedTime(string elapsedTimeText)
    {
        ElapsedTimeText.Text = elapsedTimeText;
    }

    private void UpdateStatusDisplay()
    {
        var isExecuting = _executionState.IsExecuting();

        // Toggle state visibility
        IdleState.Visibility = isExecuting ? Visibility.Collapsed : Visibility.Visible;
        ExecutingState.Visibility = isExecuting ? Visibility.Visible : Visibility.Collapsed;
        ExecutingProgress.IsActive = isExecuting;

        // Show/hide elapsed time and cancel
        ElapsedTimeText.Visibility = isExecuting ? Visibility.Visible : Visibility.Collapsed;
        CancelButton.Visibility = _executionState.CanCancel() ? Visibility.Visible : Visibility.Collapsed;

        // Update text and colors
        if (isExecuting)
        {
            ExecutingText.Text = _executionState.ToCapsuleText();
        }
        else
        {
            StatusText.Text = _executionState.ToCapsuleText();
            StatusIndicator.Fill = GetStatusBrush(_executionState);
        }
    }

    private Brush GetStatusBrush(ExecutionState state)
    {
        var resourceKey = state switch
        {
            ExecutionState.Complete => "SystemFillColorSuccessBrush",
            ExecutionState.Issue => "SystemFillColorCriticalBrush",
            ExecutionState.Cancelled => "SystemFillColorNeutralBrush",
            _ => "SystemFillColorSuccessBrush"
        };

        if (Application.Current.Resources.TryGetValue(resourceKey, out var brush) && brush is Brush b)
            return b;

        return new SolidColorBrush(Microsoft.UI.Colors.Green);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
