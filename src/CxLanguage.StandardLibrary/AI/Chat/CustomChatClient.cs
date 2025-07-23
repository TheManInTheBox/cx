using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;

namespace CxLanguage.StandardLibrary.AI.Chat;

/// <summary>
/// Custom chat client implementation that wraps Azure OpenAI for Microsoft.Extensions.AI compatibility
/// Provides production-ready chat capabilities for the Aura Cognitive Framework
/// </summary>
public class CustomChatClient : IChatClient
{
    private readonly AzureOpenAIClient _azureClient;
    private readonly string _deploymentName;
    private readonly ILogger<CustomChatClient> _logger;

    /// <summary>
    /// Metadata for the chat client
    /// </summary>
    public ChatClientMetadata Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the CustomChatClient
    /// </summary>
    /// <param name="azureClient">Azure OpenAI client</param>
    /// <param name="deploymentName">Deployment name for the model</param>
    /// <param name="logger">Logger instance</param>
    public CustomChatClient(AzureOpenAIClient azureClient, string deploymentName, ILogger<CustomChatClient> logger)
    {
        _azureClient = azureClient ?? throw new ArgumentNullException(nameof(azureClient));
        _deploymentName = deploymentName ?? throw new ArgumentNullException(nameof(deploymentName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        Metadata = new ChatClientMetadata(GetType().Name);
        
        _logger.LogInformation("✅ CustomChatClient initialized with deployment: {DeploymentName}", deploymentName);
    }

    /// <summary>
    /// Gets a response asynchronously (Microsoft.Extensions.AI interface requirement)
    /// </summary>
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<Microsoft.Extensions.AI.ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messagesList = chatMessages.ToList();
            _logger.LogInformation("🗣️ Starting chat completion with {MessageCount} messages", messagesList.Count);

            // Convert Microsoft.Extensions.AI messages to Azure OpenAI format
            var azureMessages = ConvertToAzureMessages(messagesList);
            
            // Create Azure OpenAI chat completion options
            var azureOptions = new OpenAI.Chat.ChatCompletionOptions
            {
                MaxOutputTokenCount = options?.MaxOutputTokens ?? 2048
            };
            if (options?.Temperature.HasValue == true)
            {
                azureOptions.Temperature = (float)options.Temperature.Value;
            }

            // Get Azure OpenAI chat client
            var chatClient = _azureClient.GetChatClient(_deploymentName);
            
            // Call Azure OpenAI
            var response = await chatClient.CompleteChatAsync(azureMessages, azureOptions, cancellationToken);
            
            _logger.LogInformation("✅ Chat completion successful");
            
            // Convert response back to Microsoft.Extensions.AI format
            return ConvertToExtensionsAIResponse(response.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during chat completion");
            throw;
        }
    }

    /// <summary>
    /// Gets a streaming response asynchronously (Microsoft.Extensions.AI interface requirement)
    /// </summary>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<Microsoft.Extensions.AI.ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messagesList = chatMessages.ToList();
        _logger.LogInformation("🌊 Starting streaming chat completion with {MessageCount} messages", messagesList.Count);

        // Convert messages to Azure format
        var azureMessages = ConvertToAzureMessages(messagesList);
        
        // Create streaming options
        var azureOptions = new OpenAI.Chat.ChatCompletionOptions
        {
            MaxOutputTokenCount = options?.MaxOutputTokens ?? 2048
        };
        if (options?.Temperature.HasValue == true)
        {
            azureOptions.Temperature = (float)options.Temperature.Value;
        }

        // Get Azure OpenAI chat client
        var chatClient = _azureClient.GetChatClient(_deploymentName);
        
        // Stream from Azure OpenAI
        await foreach (var update in chatClient.CompleteChatStreamingAsync(azureMessages, azureOptions, cancellationToken))
        {
            if (update.ContentUpdate.Count > 0)
            {
                var responseUpdate = new ChatResponseUpdate();
                // Use reflection or create a simple wrapper to set the Text property
                var textProperty = typeof(ChatResponseUpdate).GetProperty("Text");
                if (textProperty != null && textProperty.CanWrite)
                {
                    textProperty.SetValue(responseUpdate, string.Join("", update.ContentUpdate));
                }
                yield return responseUpdate;
            }
        }

        _logger.LogInformation("✅ Streaming chat completion finished");
    }

    /// <summary>
    /// Gets a service instance (Microsoft.Extensions.AI interface requirement)
    /// </summary>
    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return this as TService;
    }

    /// <summary>
    /// Gets a service instance (Microsoft.Extensions.AI interface requirement)
    /// </summary>
    public object? GetService(Type serviceType, object? key = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <summary>
    /// Convert Microsoft.Extensions.AI messages to Azure OpenAI format
    /// </summary>
    private static IList<OpenAI.Chat.ChatMessage> ConvertToAzureMessages(IList<Microsoft.Extensions.AI.ChatMessage> messages)
    {
        var azureMessages = new List<OpenAI.Chat.ChatMessage>();
        
        foreach (var message in messages)
        {
            var content = message.Text ?? "";
            
            switch (message.Role.Value.ToLowerInvariant())
            {
                case "system":
                    azureMessages.Add(new OpenAI.Chat.SystemChatMessage(content));
                    break;
                case "user":
                    azureMessages.Add(new OpenAI.Chat.UserChatMessage(content));
                    break;
                case "assistant":
                    azureMessages.Add(new OpenAI.Chat.AssistantChatMessage(content));
                    break;
                default:
                    // Default to user message for unknown roles
                    azureMessages.Add(new OpenAI.Chat.UserChatMessage(content));
                    break;
            }
        }
        
        return azureMessages;
    }

    /// <summary>
    /// Convert Azure OpenAI completion to Microsoft.Extensions.AI format
    /// </summary>
    private static ChatResponse ConvertToExtensionsAIResponse(OpenAI.Chat.ChatCompletion azureCompletion)
    {
        var content = azureCompletion.Content.Count > 0 ? azureCompletion.Content[0].Text : "";
        
        var message = new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, content);
        
        return new ChatResponse(message)
        {
            CreatedAt = azureCompletion.CreatedAt,
            FinishReason = ConvertFinishReason(azureCompletion.FinishReason),
            Usage = new UsageDetails
            {
                InputTokenCount = azureCompletion.Usage?.InputTokenCount,
                OutputTokenCount = azureCompletion.Usage?.OutputTokenCount,
                TotalTokenCount = azureCompletion.Usage?.TotalTokenCount
            }
        };
    }

    /// <summary>
    /// Convert Azure OpenAI finish reason to Microsoft.Extensions.AI format
    /// </summary>
    private static Microsoft.Extensions.AI.ChatFinishReason? ConvertFinishReason(OpenAI.Chat.ChatFinishReason? azureFinishReason)
    {
        return azureFinishReason?.ToString().ToLowerInvariant() switch
        {
            "stop" => Microsoft.Extensions.AI.ChatFinishReason.Stop,
            "length" => Microsoft.Extensions.AI.ChatFinishReason.Length,
            "content_filter" => Microsoft.Extensions.AI.ChatFinishReason.ContentFilter,
            "function_call" => Microsoft.Extensions.AI.ChatFinishReason.ToolCalls,
            _ => null
        };
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        // Azure client is injected, so we don't dispose it here
        _logger.LogInformation("🧹 CustomChatClient disposed");
    }
}
