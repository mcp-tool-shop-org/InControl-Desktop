using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using InControl.Core.Models;
using InControl.ViewModels;

namespace InControl.App.Controls;

/// <summary>
/// Displays a single message card in the conversation.
/// Shows different layouts for user intents vs model output.
/// </summary>
public sealed partial class MessageCard : UserControl
{
    public MessageCard()
    {
        this.InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        CopyMenuItem.Click += OnCopyClick;
        CopyAsMarkdownMenuItem.Click += OnCopyAsMarkdownClick;
        AddToContextMenuItem.Click += OnAddToContextClick;
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        if (Message?.Content != null)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(Message.Content);
            Clipboard.SetContent(dataPackage);
        }
    }

    private void OnCopyAsMarkdownClick(object sender, RoutedEventArgs e)
    {
        if (Message?.Content != null)
        {
            // For now, just copy as-is (content may already be markdown)
            var dataPackage = new DataPackage();
            dataPackage.SetText(Message.Content);
            Clipboard.SetContent(dataPackage);
        }
    }

    private async void OnAddToContextClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Coming Soon",
            Content = "Add to Context will be available in a future update.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    /// <summary>
    /// The message view model to display.
    /// </summary>
    public MessageViewModel? Message
    {
        get => (MessageViewModel?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(MessageViewModel),
            typeof(MessageCard),
            new PropertyMetadata(null, OnMessageChanged));

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MessageCard card)
        {
            card.UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        var message = Message;
        if (message == null)
        {
            HideAllCards();
            return;
        }

        switch (message.Role)
        {
            case MessageRole.User:
                ShowUserIntent(message);
                break;
            case MessageRole.Assistant:
                ShowModelOutput(message);
                break;
            case MessageRole.System:
                ShowSystemMessage(message);
                break;
            default:
                HideAllCards();
                break;
        }
    }

    private void ShowUserIntent(MessageViewModel message)
    {
        HideAllCards();
        UserIntentCard.Visibility = Visibility.Visible;
        UserContent.Text = message.Content;
        UserTimestamp.Text = message.TimestampDisplay;
    }

    private void ShowModelOutput(MessageViewModel message)
    {
        HideAllCards();
        ModelOutputCard.Visibility = Visibility.Visible;
        ModelContent.Text = message.Content;
        ModelTimestamp.Text = message.TimestampDisplay;

        // Show model name if available
        if (!string.IsNullOrEmpty(message.Model))
        {
            ModelName.Text = message.Model;
        }
        else
        {
            ModelName.Text = "Model output";
        }

        // Show streaming indicator or footer
        if (message.IsStreaming)
        {
            StreamingIndicator.Visibility = Visibility.Visible;
            ModelFooter.Visibility = Visibility.Collapsed;
        }
        else
        {
            StreamingIndicator.Visibility = Visibility.Collapsed;

            // Show token count if available
            if (message.Message.TokenCount.HasValue)
            {
                TokenCountText.Text = $"{message.Message.TokenCount.Value} tokens";
                ModelFooter.Visibility = Visibility.Visible;
            }
            else
            {
                ModelFooter.Visibility = Visibility.Collapsed;
            }
        }

        // Subscribe to property changes for streaming updates
        message.PropertyChanged += OnMessagePropertyChanged;
    }

    private void ShowSystemMessage(MessageViewModel message)
    {
        HideAllCards();
        SystemMessageCard.Visibility = Visibility.Visible;
        SystemContent.Text = message.Content;
    }

    private void HideAllCards()
    {
        UserIntentCard.Visibility = Visibility.Collapsed;
        ModelOutputCard.Visibility = Visibility.Collapsed;
        SystemMessageCard.Visibility = Visibility.Collapsed;
    }

    private void OnMessagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is MessageViewModel message && e.PropertyName == nameof(MessageViewModel.Content))
        {
            // Update content during streaming
            DispatcherQueue.TryEnqueue(() =>
            {
                if (message.IsAssistant)
                {
                    ModelContent.Text = message.Content;
                }
            });
        }
    }
}
