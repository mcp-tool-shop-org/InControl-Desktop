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
        SetupKeyboardNavigation();
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
