using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using InControl.ViewModels.Onboarding;

namespace InControl.App.Controls;

/// <summary>
/// Welcome panel with Guided Quick Start for new users.
/// Shows step-by-step onboarding and quick actions for returning users.
/// </summary>
public sealed partial class WelcomePanel : UserControl
{
    private readonly ReentryViewModel _viewModel = new();
    private bool _hasModel;
    private bool _hasSession;

    public WelcomePanel()
    {
        this.InitializeComponent();
        UpdateUI();
    }

    #region Events

    /// <summary>
    /// Event raised when user wants to continue last session.
    /// </summary>
    public event EventHandler? ContinueSessionRequested;

    /// <summary>
    /// Event raised when user wants to start a new session.
    /// </summary>
    public event EventHandler? NewSessionRequested;

    /// <summary>
    /// Event raised when user wants to browse sessions.
    /// </summary>
    public event EventHandler? BrowseSessionsRequested;

    /// <summary>
    /// Event raised when user wants to open Model Manager.
    /// </summary>
    public event EventHandler? ModelManagerRequested;

    /// <summary>
    /// Event raised when user wants to insert an example prompt.
    /// </summary>
    public event EventHandler<string>? InsertExampleRequested;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the view model.
    /// </summary>
    public ReentryViewModel ViewModel => _viewModel;

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the panel with session history.
    /// </summary>
    public void UpdateFromHistory(
        string? lastSessionTitle,
        DateTimeOffset? lastSessionTime,
        string? lastUsedModel,
        int totalSessions)
    {
        _viewModel.UpdateFromHistory(lastSessionTitle, lastSessionTime, lastUsedModel, totalSessions);
        _hasSession = totalSessions > 0;
        UpdateUI();
    }

    /// <summary>
    /// Sets the current model status.
    /// </summary>
    public void SetModelStatus(string modelName, bool isReady)
    {
        _hasModel = isReady && !string.IsNullOrEmpty(modelName);

        if (_hasModel)
        {
            ModelStatusPanel.Visibility = Visibility.Visible;
            ModelStatusText.Text = $"{modelName} ready";
            NoModelWarning.Visibility = Visibility.Collapsed;

            // Update Quick Start step 1
            Step1Check.Visibility = Visibility.Visible;
            Step1Number.Visibility = Visibility.Collapsed;
            Step1Background.Color = Colors.Green;
            Step1Description.Text = $"Using {modelName}";

            // Activate step 2
            Step2Background.Color = Windows.UI.Color.FromArgb(255, 0, 120, 212); // Accent blue
        }
        else
        {
            ModelStatusPanel.Visibility = Visibility.Collapsed;
            NoModelWarning.Visibility = Visibility.Visible;

            // Reset Quick Start step 1
            Step1Check.Visibility = Visibility.Collapsed;
            Step1Number.Visibility = Visibility.Visible;
            Step1Background.Color = Windows.UI.Color.FromArgb(255, 0, 120, 212); // Accent blue
            Step1Description.Text = "Select or download an AI model to power your conversations.";

            // Dim step 2
            Step2Background.Color = Windows.UI.Color.FromArgb(255, 136, 136, 136); // Gray
        }

        UpdateQuickStartVisibility();
    }

    /// <summary>
    /// Marks the session step as complete.
    /// </summary>
    public void SetSessionCreated(bool hasSession)
    {
        _hasSession = hasSession;

        if (hasSession && _hasModel)
        {
            // Mark step 2 complete
            Step2Check.Visibility = Visibility.Visible;
            Step2Number.Visibility = Visibility.Collapsed;
            Step2Background.Color = Colors.Green;

            // Activate step 3
            Step3Background.Color = Windows.UI.Color.FromArgb(255, 0, 120, 212); // Accent blue
        }

        UpdateQuickStartVisibility();
    }

    #endregion

    #region Event Handlers

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        ContinueSessionRequested?.Invoke(this, EventArgs.Empty);
    }

    private void NewSessionButton_Click(object sender, RoutedEventArgs e)
    {
        NewSessionRequested?.Invoke(this, EventArgs.Empty);
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        BrowseSessionsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OpenModelManagerButton_Click(object sender, RoutedEventArgs e)
    {
        ModelManagerRequested?.Invoke(this, EventArgs.Empty);
    }

    private void InsertExampleButton_Click(object sender, RoutedEventArgs e)
    {
        // Provide a simple example prompt
        var examplePrompt = "Explain the concept of local AI inference in simple terms.";
        InsertExampleRequested?.Invoke(this, examplePrompt);
    }

    #endregion

    #region Private Methods

    private void UpdateUI()
    {
        // Update greeting
        GreetingText.Text = _viewModel.WelcomeGreeting;

        // Update continue button
        if (_viewModel.HasRecentSession && _viewModel.HasLastSession)
        {
            ContinueButton.Visibility = Visibility.Visible;
            ContinueSessionTitle.Text = $"Continue \"{_viewModel.LastSessionTitle}\"";
            ContinueSessionTime.Text = _viewModel.LastSessionTimeText;
        }
        else
        {
            ContinueButton.Visibility = Visibility.Collapsed;
        }

        // Update browse button
        if (_viewModel.TotalSessions > 0)
        {
            BrowseButton.Visibility = Visibility.Visible;
            SessionCountText.Text = _viewModel.TotalSessionsText;
        }
        else
        {
            BrowseButton.Visibility = Visibility.Collapsed;
        }

        UpdateQuickStartVisibility();
    }

    private void UpdateQuickStartVisibility()
    {
        // Show Quick Start for new users or users without a model
        // Hide it for experienced users with a model and sessions
        var isExperiencedUser = _hasModel && _viewModel.TotalSessions >= 3;
        QuickStartPanel.Visibility = isExperiencedUser ? Visibility.Collapsed : Visibility.Visible;
    }

    #endregion
}
