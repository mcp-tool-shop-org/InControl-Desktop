using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using InControl.Core.Models;
using InControl.Inference.Interfaces;
using InControl.Services.Interfaces;

namespace InControl.Services.Chat;

/// <summary>
/// Chat service that orchestrates conversations using the inference client.
/// Manages conversation lifecycle and delegates inference to IInferenceClient.
/// </summary>
public sealed class ChatService : IChatService
{
    private readonly IInferenceClient _inferenceClient;
    private readonly ILogger<ChatService> _logger;
    private readonly Dictionary<Guid, Conversation> _conversations = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _activeGenerations = new();

    public event EventHandler<ConversationEventArgs>? ConversationCreated;
    public event EventHandler<ConversationEventArgs>? ConversationUpdated;
    public event EventHandler<ConversationEventArgs>? ConversationDeleted;

    public ChatService(
        IInferenceClient inferenceClient,
        ILogger<ChatService> logger)
    {
        _inferenceClient = inferenceClient;
        _logger = logger;
    }

    public Task<Conversation> CreateConversationAsync(
        string? title = null,
        string? model = null,
        string? systemPrompt = null,
        CancellationToken ct = default)
    {
        var conversation = Conversation.Create(title, model, systemPrompt);
        _conversations[conversation.Id] = conversation;

        _logger.LogInformation("Created conversation {Id} with model {Model}", conversation.Id, model);
        ConversationCreated?.Invoke(this, new ConversationEventArgs { Conversation = conversation });

        return Task.FromResult(conversation);
    }

    public Task<Conversation?> GetConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        _conversations.TryGetValue(conversationId, out var conversation);
        return Task.FromResult(conversation);
    }

    public Task<IReadOnlyList<Conversation>> GetConversationsAsync(CancellationToken ct = default)
    {
        var list = _conversations.Values
            .OrderByDescending(c => c.ModifiedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Conversation>>(list);
    }

    public Task<Conversation> UpdateConversationAsync(
        Guid conversationId,
        string? title = null,
        string? model = null,
        CancellationToken ct = default)
    {
        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            throw new KeyNotFoundException($"Conversation {conversationId} not found");
        }

        if (title is not null)
        {
            conversation = conversation.WithTitle(title);
        }

        if (model is not null)
        {
            conversation = conversation with { Model = model, ModifiedAt = DateTimeOffset.UtcNow };
        }

        _conversations[conversationId] = conversation;
        ConversationUpdated?.Invoke(this, new ConversationEventArgs { Conversation = conversation });

        return Task.FromResult(conversation);
    }

    public Task DeleteConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        if (_conversations.Remove(conversationId, out var conversation))
        {
            ConversationDeleted?.Invoke(this, new ConversationEventArgs { Conversation = conversation });
        }
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> SendMessageAsync(
        Guid conversationId,
        string message,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            throw new KeyNotFoundException($"Conversation {conversationId} not found");
        }

        // Append user message to conversation
        var userMessage = Message.User(message);
        conversation = conversation.WithMessage(userMessage);
        _conversations[conversationId] = conversation;

        var model = conversation.Model
            ?? throw new InvalidOperationException("No model selected for this conversation");

        // Build the chat request from conversation history
        var request = ChatRequest.FromConversation(conversation);

        _logger.LogDebug("Sending message to {Model}, conversation {Id}", model, conversationId);

        // Track active generation for cancellation
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _activeGenerations[conversationId] = cts;

        var responseContent = new System.Text.StringBuilder();

        try
        {
            await foreach (var token in _inferenceClient.StreamChatAsync(request, cts.Token))
            {
                responseContent.Append(token);
                yield return token;
            }

            // Append assistant response to conversation
            var assistantMessage = Message.Assistant(responseContent.ToString(), model);
            conversation = conversation.WithMessage(assistantMessage);
            _conversations[conversationId] = conversation;

            ConversationUpdated?.Invoke(this, new ConversationEventArgs { Conversation = conversation });
        }
        finally
        {
            _activeGenerations.Remove(conversationId);
            cts.Dispose();
        }
    }

    public async IAsyncEnumerable<string> RegenerateLastResponseAsync(
        Guid conversationId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            throw new KeyNotFoundException($"Conversation {conversationId} not found");
        }

        // Remove the last assistant message if present
        var messages = conversation.Messages.ToList();
        if (messages.Count > 0 && messages[^1].Role == MessageRole.Assistant)
        {
            messages.RemoveAt(messages.Count - 1);
        }

        // Get the last user message to regenerate from
        var lastUserMessage = messages.LastOrDefault(m => m.Role == MessageRole.User);
        if (lastUserMessage is null)
        {
            yield break;
        }

        // Update conversation without the last assistant message
        conversation = conversation with
        {
            Messages = messages,
            ModifiedAt = DateTimeOffset.UtcNow
        };
        _conversations[conversationId] = conversation;

        var model = conversation.Model
            ?? throw new InvalidOperationException("No model selected for this conversation");

        var request = ChatRequest.FromConversation(conversation);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _activeGenerations[conversationId] = cts;

        var responseContent = new System.Text.StringBuilder();

        try
        {
            await foreach (var token in _inferenceClient.StreamChatAsync(request, cts.Token))
            {
                responseContent.Append(token);
                yield return token;
            }

            var assistantMessage = Message.Assistant(responseContent.ToString(), model);
            conversation = conversation.WithMessage(assistantMessage);
            _conversations[conversationId] = conversation;
        }
        finally
        {
            _activeGenerations.Remove(conversationId);
            cts.Dispose();
        }
    }

    public void StopGeneration(Guid conversationId)
    {
        if (_activeGenerations.TryGetValue(conversationId, out var cts))
        {
            _logger.LogDebug("Stopping generation for conversation {Id}", conversationId);
            cts.Cancel();
        }
    }
}
