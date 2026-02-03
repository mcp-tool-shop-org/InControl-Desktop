using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Volt.ViewModels.Onboarding;

namespace Volt.App.Controls;

/// <summary>
/// Welcome panel for re-entry experience.
/// Shows quick actions for returning users.
/// </summary>
public sealed partial class WelcomePanel : UserControl
{
    private readonly ReentryViewModel _viewModel = new();

    public WelcomePanel()
    {
        this.InitializeComponent();
        UpdateUI();
    }

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
    /// Gets the view model.
    /// </summary>
    public ReentryViewModel ViewModel => _viewModel;

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
        UpdateUI();
    }

    /// <summary>
    /// Sets the current model status.
    /// </summary>
    public void SetModelStatus(string modelName, bool isReady)
    {
        if (isReady && !string.IsNullOrEmpty(modelName))
        {
            ModelStatusPanel.Visibility = Visibility.Visible;
            ModelStatusText.Text = $"{modelName} ready";
        }
        else
        {
            ModelStatusPanel.Visibility = Visibility.Collapsed;
        }
    }

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

        // Style new session button based on suggested action
        if (_viewModel.SuggestedAction == ReentryAction.NewSession)
        {
            // Keep accent style (primary action)
        }
        else if (_viewModel.HasRecentSession)
        {
            // Demote to secondary when continue is suggested
            NewSessionButton.Style = null;
        }
    }
}
