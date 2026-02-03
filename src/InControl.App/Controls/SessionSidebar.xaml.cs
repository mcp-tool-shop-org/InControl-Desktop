using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace InControl.App.Controls;

/// <summary>
/// Left sidebar containing session list with new session button, search, and pinned items.
/// Supports keyboard navigation and context menu actions.
/// </summary>
public sealed partial class SessionSidebar : UserControl
{
    public SessionSidebar()
    {
        this.InitializeComponent();
        SetupEventHandlers();
        SetupKeyboardNavigation();
    }

    #region Events

    /// <summary>
    /// Event raised when a new session is requested.
    /// </summary>
    public event EventHandler? NewSessionRequested;

    /// <summary>
    /// Event raised when a session is selected.
    /// </summary>
    public event EventHandler<string>? SessionSelected;

    /// <summary>
    /// Event raised when session search text changes.
    /// </summary>
    public event EventHandler<string>? SearchTextChanged;

    #endregion

    private void SetupEventHandlers()
    {
        // New Session button
        NewSessionButton.Click += OnNewSessionClick;

        // Session search
        SessionSearch.TextChanged += OnSearchTextChanged;
        SessionSearch.QuerySubmitted += OnSearchQuerySubmitted;

        // Context menu items
        RenameMenuItem.Click += OnRenameClick;
        DuplicateMenuItem.Click += OnDuplicateClick;
        PinMenuItem.Click += OnPinClick;
        ExportMenuItem.Click += OnExportClick;
        DeleteMenuItem.Click += OnDeleteClick;

        // Session list item click
        SessionList.ItemClick += OnSessionItemClick;
        PinnedList.ItemClick += OnSessionItemClick;
    }

    private void OnNewSessionClick(object sender, RoutedEventArgs e)
    {
        NewSessionRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            SearchTextChanged?.Invoke(this, sender.Text);
        }
    }

    private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        // Search is handled through text changed event
    }

    private void OnSessionItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SessionItem session)
        {
            SessionSelected?.Invoke(this, session.Id);
        }
    }

    private async void OnRenameClick(object sender, RoutedEventArgs e)
    {
        await ShowNotImplementedDialog("Rename Session");
    }

    private async void OnDuplicateClick(object sender, RoutedEventArgs e)
    {
        await ShowNotImplementedDialog("Duplicate Session");
    }

    private async void OnPinClick(object sender, RoutedEventArgs e)
    {
        await ShowNotImplementedDialog("Pin Session");
    }

    private async void OnExportClick(object sender, RoutedEventArgs e)
    {
        await ShowNotImplementedDialog("Export Session");
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        await ShowNotImplementedDialog("Delete Session");
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

    private void SetupKeyboardNavigation()
    {
        // Enable keyboard navigation on the session list
        SessionList.KeyDown += OnSessionListKeyDown;
    }

    private void OnSessionListKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (SessionList.SelectedItem == null)
            return;

        switch (e.Key)
        {
            case Windows.System.VirtualKey.Enter:
                // Open selected session
                OpenSelectedSession();
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.F2:
                // Rename selected session
                RenameSelectedSession();
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Delete:
                // Delete selected session (with confirmation)
                DeleteSelectedSession();
                e.Handled = true;
                break;
        }
    }

    private void OpenSelectedSession()
    {
        // TODO: Implement session opening via ViewModel command
    }

    private void RenameSelectedSession()
    {
        // TODO: Implement rename dialog via ViewModel command
    }

    private void DeleteSelectedSession()
    {
        // TODO: Implement delete confirmation via ViewModel command
    }

    /// <summary>
    /// Updates the visibility of empty state vs session list.
    /// </summary>
    public void UpdateEmptyState(bool hasItems)
    {
        EmptyState.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;
        SessionList.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Updates the visibility of the pinned section.
    /// </summary>
    public void UpdatePinnedSection(bool hasPinnedItems)
    {
        PinnedSection.Visibility = hasPinnedItems ? Visibility.Visible : Visibility.Collapsed;
    }
}

/// <summary>
/// Represents a session item in the sidebar.
/// </summary>
public class SessionItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RelativeTime { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
}
