using System.ComponentModel;
using InControl.Core.Models;
using InControl.Core.UX;

namespace InControl.ViewModels.Sessions;

/// <summary>
/// ViewModel for a session item in the sidebar list.
/// </summary>
public sealed class SessionItemViewModel : INotifyPropertyChanged
{
    private readonly Conversation _conversation;
    private bool _isPinned;
    private bool _isSelected;

    public SessionItemViewModel(Conversation conversation)
    {
        _conversation = conversation;
    }

    /// <summary>
    /// The unique identifier for this session.
    /// </summary>
    public Guid Id => _conversation.Id;

    /// <summary>
    /// The session title. Falls back to "Untitled session" if empty.
    /// </summary>
    public string Title => string.IsNullOrWhiteSpace(_conversation.Title)
        ? UXStrings.Session.EmptyTitle
        : _conversation.Title;

    /// <summary>
    /// The number of messages in this session.
    /// </summary>
    public int MessageCount => _conversation.Messages.Count;

    /// <summary>
    /// When this session was created.
    /// </summary>
    public DateTimeOffset CreatedAt => _conversation.CreatedAt;

    /// <summary>
    /// When this session was last modified.
    /// </summary>
    public DateTimeOffset LastModified => _conversation.ModifiedAt;

    /// <summary>
    /// Human-readable relative time since last modification.
    /// </summary>
    public string RelativeTime => UXStrings.Time.Relative(LastModified);

    /// <summary>
    /// Whether this session is pinned to the top.
    /// </summary>
    public bool IsPinned
    {
        get => _isPinned;
        set
        {
            if (_isPinned != value)
            {
                _isPinned = value;
                OnPropertyChanged(nameof(IsPinned));
            }
        }
    }

    /// <summary>
    /// Whether this session is currently selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    /// <summary>
    /// Gets the underlying conversation.
    /// </summary>
    public Conversation GetConversation() => _conversation;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
