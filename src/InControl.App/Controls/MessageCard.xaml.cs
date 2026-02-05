using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using InControl.Core.Models;
using InControl.ViewModels;
using InControl.App.Services;

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
        // User message context menu
        CopyMenuItem.Click += OnCopyClick;
        CopyAsMarkdownMenuItem.Click += OnCopyAsMarkdownClick;
        AddToContextMenuItem.Click += OnAddToContextClick;

        // Assistant message context menu (includes report option)
        AssistantCopyMenuItem.Click += OnCopyClick;
        AssistantCopyAsMarkdownMenuItem.Click += OnCopyAsMarkdownClick;
        AssistantAddToContextMenuItem.Click += OnAddToContextClick;
        ReportInappropriateMenuItem.Click += OnReportInappropriateClick;
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        if (Message?.Content != null)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(Message.Content);
            Clipboard.SetContent(dataPackage);
            CopyFeedback.ShowCopied();
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
            CopyFeedback.ShowSuccess("Copied as Markdown");
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

    private async void OnReportInappropriateClick(object sender, RoutedEventArgs e)
    {
        if (Message == null) return;

        var reportDialog = new ContentDialog
        {
            Title = "Report Inappropriate Content",
            XamlRoot = this.XamlRoot,
            PrimaryButtonText = "Submit Report",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        // Create report form
        var panel = new StackPanel { Spacing = 12 };

        panel.Children.Add(new TextBlock
        {
            Text = "Help us improve by reporting AI-generated content that is inappropriate, harmful, or incorrect.",
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.8
        });

        var reasonCombo = new ComboBox
        {
            Header = "Reason for report",
            PlaceholderText = "Select a reason",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource = new[]
            {
                "Harmful or unsafe content",
                "Factually incorrect information",
                "Biased or discriminatory content",
                "Inappropriate language",
                "Privacy concern",
                "Other"
            }
        };
        panel.Children.Add(reasonCombo);

        var detailsBox = new TextBox
        {
            Header = "Additional details (optional)",
            PlaceholderText = "Provide any additional context...",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 80,
            MaxHeight = 150
        };
        panel.Children.Add(detailsBox);

        reportDialog.Content = panel;

        var result = await reportDialog.ShowAsync();

        if (result == ContentDialogResult.Primary && reasonCombo.SelectedItem != null)
        {
            // Save the report
            var report = new ContentReport
            {
                MessageId = Message.Id,
                MessageContent = Message.Content,
                Model = Message.Model ?? "Unknown",
                Reason = reasonCombo.SelectedItem.ToString()!,
                Details = detailsBox.Text,
                ReportedAt = DateTimeOffset.Now
            };

            ContentReportService.Instance.SaveReport(report);
            CopyFeedback.ShowSuccess("Report submitted");
        }
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
