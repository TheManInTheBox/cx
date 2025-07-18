using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CxLanguage.StandardLibrary.AI.ChatCompletion;

/// <summary>
/// Provides conversational AI capabilities using Semantic Kernel
/// </summary>
public class ChatCompletionService : CxAiServiceBase
{
    private readonly IChatCompletionService _chatCompletionService;

    /// <summary>
    /// Initializes a new instance of the ChatCompletionService
    /// </summary>
    /// <param name="kernel">Semantic Kernel instance</param>
    /// <param name="logger">Logger instance</param>
    public ChatCompletionService(Kernel kernel, ILogger<ChatCompletionService> logger) 
        : base(kernel, logger)
    {
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    /// <summary>
    /// Gets the service name
    /// </summary>
    public override string ServiceName => "ChatCompletion";
    
    /// <summary>
    /// Gets the service version
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Send a message and get a chat completion response
    /// </summary>
    public async Task<ChatCompletionResult> CompleteAsync(string message, ChatCompletionOptions? options = null)
    {
        var result = new ChatCompletionResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Processing chat completion for message: {Message}", message);

            var chatHistory = new ChatHistory();
            if (options?.SystemMessage != null)
            {
                chatHistory.AddSystemMessage(options.SystemMessage);
            }
            chatHistory.AddUserMessage(message);

            var settings = new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>()
            };

            if (options != null)
            {
                if (options.MaxTokens.HasValue)
                    settings.ExtensionData["max_tokens"] = options.MaxTokens.Value;
                if (options.Temperature.HasValue)
                    settings.ExtensionData["temperature"] = options.Temperature.Value;
                if (options.TopP.HasValue)
                    settings.ExtensionData["top_p"] = options.TopP.Value;
            }

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, settings);

            result.IsSuccess = true;
            result.Response = response.Content ?? string.Empty;
            result.Role = response.Role.ToString();
            result.TokenCount = response.Metadata?.Count ?? 0;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Chat completion successful. Response length: {Length}", result.Response.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat completion for message: {Message}", message);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Continue a conversation with chat history
    /// </summary>
    public async Task<ChatCompletionResult> ContinueConversationAsync(
        List<ChatMessage> conversationHistory, 
        string newMessage,
        ChatCompletionOptions? options = null)
    {
        var result = new ChatCompletionResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Continuing conversation with {Count} previous messages", conversationHistory.Count);

            var chatHistory = new ChatHistory();
            
            if (options?.SystemMessage != null)
            {
                chatHistory.AddSystemMessage(options.SystemMessage);
            }

            // Add conversation history
            foreach (var message in conversationHistory)
            {
                switch (message.Role.ToLowerInvariant())
                {
                    case "user":
                        chatHistory.AddUserMessage(message.Content);
                        break;
                    case "assistant":
                        chatHistory.AddAssistantMessage(message.Content);
                        break;
                    case "system":
                        chatHistory.AddSystemMessage(message.Content);
                        break;
                }
            }

            chatHistory.AddUserMessage(newMessage);

            var settings = new PromptExecutionSettings();
            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, settings);

            result.IsSuccess = true;
            result.Response = response.Content ?? string.Empty;
            result.Role = response.Role.ToString();
            result.TokenCount = response.Metadata?.Count ?? 0;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error continuing conversation: {Error}", ex.Message);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Get streaming chat completion response
    /// </summary>
    public async IAsyncEnumerable<ChatCompletionStreamResult> CompleteStreamAsync(
        string message, 
        ChatCompletionOptions? options = null)
    {
        var startTime = DateTimeOffset.UtcNow;
        var chatHistory = new ChatHistory();
        
        if (options?.SystemMessage != null)
        {
            chatHistory.AddSystemMessage(options.SystemMessage);
        }
        chatHistory.AddUserMessage(message);

        await foreach (var streamContent in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            yield return new ChatCompletionStreamResult
            {
                IsSuccess = true,
                ContentChunk = streamContent.Content ?? string.Empty,
                Role = streamContent.Role?.ToString() ?? "assistant",
                IsComplete = false,
                ExecutionTime = DateTimeOffset.UtcNow - startTime
            };
        }

        yield return new ChatCompletionStreamResult
        {
            IsSuccess = true,
            ContentChunk = string.Empty,
            IsComplete = true,
            ExecutionTime = DateTimeOffset.UtcNow - startTime
        };
    }

    /// <summary>
    /// Synchronous wrapper for CompleteAsync
    /// </summary>
    public ChatCompletionResult Complete(string message, ChatCompletionOptions? options = null)
    {
        return CompleteAsync(message, options).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Represents a chat message in conversation history
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Role of the message sender (user, assistant, system)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Options for chat completion operations
/// </summary>
public class ChatCompletionOptions : CxAiOptions
{
    /// <summary>
    /// System message to set context for the conversation
    /// </summary>
    public string? SystemMessage { get; set; }

    /// <summary>
    /// Maximum number of tokens to generate
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Temperature for randomness (0.0 to 2.0)
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Top-p nucleus sampling parameter
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Frequency penalty (-2.0 to 2.0)
    /// </summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Presence penalty (-2.0 to 2.0)
    /// </summary>
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Stop sequences to halt generation
    /// </summary>
    public string[]? StopSequences { get; set; }
}

/// <summary>
/// Result from chat completion operations
/// </summary>
public class ChatCompletionResult : CxAiResult
{
    /// <summary>
    /// The response message content
    /// </summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Role of the responder (typically "assistant")
    /// </summary>
    public string Role { get; set; } = "assistant";

    /// <summary>
    /// Number of tokens used in the completion
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>
    /// Finish reason (completed, length, stop, etc.)
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// Result from streaming chat completion
/// </summary>
public class ChatCompletionStreamResult : CxAiResult
{
    /// <summary>
    /// Content chunk from the stream
    /// </summary>
    public string ContentChunk { get; set; } = string.Empty;

    /// <summary>
    /// Role of the message sender
    /// </summary>
    public string Role { get; set; } = "assistant";

    /// <summary>
    /// Whether this is the final chunk
    /// </summary>
    public bool IsComplete { get; set; }
}
