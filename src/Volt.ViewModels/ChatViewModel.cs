using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Volt.Core.Models;
using Volt.Services.Interfaces;

namespace Volt.ViewModels;

/// <summary>
/// ViewModel for the main chat interface.
/// </summary>
public partial class ChatViewModel : ViewModelBase
{
    private readonly IChatService _chatService;
    private CancellationTokenSource? _generationCts;

    /// <summary>
    /// The current conversation.
    /// </summary>
    [ObservableProperty]
    private Conversation? _currentConversation;

    /// <summary>
    /// Messages in the current conversation.
    /// </summary>
    public ObservableCollection<MessageViewModel> Messages { get; } = [];

    /// <summary>
    /// The current user input.
    /// </summary>
    [ObservableProperty]
    private string _inputText = string.Empty;

    /// <summary>
    /// Indicates whether a response is being generated.
    /// </summary>
    [ObservableProperty]
    private bool _isGenerating;

    /// <summary>
    /// The currently selected model.
    /// </summary>
    [ObservableProperty]
    private string? _selectedModel;

    /// <summary>
    /// Available models.
    /// </summary>
    public ObservableCollection<ModelInfo> AvailableModels { get; } = [];

    /// <summary>
    /// Current generation status text.
    /// </summary>
    [ObservableProperty]
    private string? _statusText;

    /// <summary>
    /// Tokens per second during generation.
    /// </summary>
    [ObservableProperty]
    private double? _tokensPerSecond;

    public ChatViewModel(
        IChatService chatService,
        ILogger<ChatViewModel> logger)
        : base(logger)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// Indicates whether a message can be sent.
    /// </summary>
    public bool CanSend => !string.IsNullOrWhiteSpace(InputText) && !IsGenerating;

    /// <summary>
    /// Sends the current input message.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (CurrentConversation is null)
        {
            await CreateNewConversationAsync();
        }

        if (CurrentConversation is null)
        {
            SetError("Failed to create conversation");
            return;
        }

        var message = InputText.Trim();
        InputText = string.Empty;

        // Add user message to UI
        Messages.Add(new MessageViewModel(Message.User(message)));

        // Create placeholder for assistant response
        var assistantMessage = new MessageViewModel(Message.Assistant(string.Empty, SelectedModel))
        {
            IsStreaming = true
        };
        Messages.Add(assistantMessage);

        IsGenerating = true;
        _generationCts = new CancellationTokenSource();

        try
        {
            await foreach (var token in _chatService.SendMessageAsync(
                CurrentConversation.Id,
                message,
                _generationCts.Token))
            {
                assistantMessage.AppendContent(token);
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Generation stopped";
        }
        catch (Exception ex)
        {
            SetError(ex, "Failed to generate response");
        }
        finally
        {
            assistantMessage.IsStreaming = false;
            IsGenerating = false;
            _generationCts?.Dispose();
            _generationCts = null;
        }
    }

    /// <summary>
    /// Stops the current generation.
    /// </summary>
    [RelayCommand]
    private void StopGeneration()
    {
        if (CurrentConversation is not null)
        {
            _chatService.StopGeneration(CurrentConversation.Id);
        }
        _generationCts?.Cancel();
    }

    /// <summary>
    /// Creates a new conversation.
    /// </summary>
    [RelayCommand]
    private async Task CreateNewConversationAsync()
    {
        await ExecuteAsync(async () =>
        {
            Messages.Clear();
            CurrentConversation = await _chatService.CreateConversationAsync(
                model: SelectedModel);
        });
    }

    /// <summary>
    /// Loads an existing conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID to load.</param>
    [RelayCommand]
    private async Task LoadConversationAsync(Guid conversationId)
    {
        await ExecuteAsync(async () =>
        {
            var conversation = await _chatService.GetConversationAsync(conversationId);
            if (conversation is null)
            {
                SetError("Conversation not found");
                return;
            }

            CurrentConversation = conversation;
            Messages.Clear();

            foreach (var message in conversation.Messages)
            {
                Messages.Add(new MessageViewModel(message));
            }
        });
    }

    /// <summary>
    /// Regenerates the last assistant response.
    /// </summary>
    [RelayCommand]
    private async Task RegenerateAsync()
    {
        if (CurrentConversation is null || Messages.Count == 0) return;

        // Remove the last assistant message
        if (Messages[^1].Role == MessageRole.Assistant)
        {
            Messages.RemoveAt(Messages.Count - 1);
        }

        // Create new placeholder
        var assistantMessage = new MessageViewModel(Message.Assistant(string.Empty, SelectedModel))
        {
            IsStreaming = true
        };
        Messages.Add(assistantMessage);

        IsGenerating = true;
        _generationCts = new CancellationTokenSource();

        try
        {
            await foreach (var token in _chatService.RegenerateLastResponseAsync(
                CurrentConversation.Id,
                _generationCts.Token))
            {
                assistantMessage.AppendContent(token);
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Generation stopped";
        }
        catch (Exception ex)
        {
            SetError(ex, "Failed to regenerate response");
        }
        finally
        {
            assistantMessage.IsStreaming = false;
            IsGenerating = false;
            _generationCts?.Dispose();
            _generationCts = null;
        }
    }

    partial void OnInputTextChanged(string value)
    {
        SendCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsGeneratingChanged(bool value)
    {
        SendCommand.NotifyCanExecuteChanged();
    }
}
