using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.AI;
using CxLanguage.Runtime;

namespace CxLanguage.StandardLibrary.Core;

/// <summary>
/// Modern base class for CX AI services using Microsoft.Extensions.AI
/// Replaces heavy Semantic Kernel + Kernel Memory with lightweight, direct AI integration
/// Maintains the same interface for CX Language compatibility while eliminating dependency bloat
/// </summary>
public abstract class ModernAiServiceBase
{
    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    protected readonly IServiceProvider _serviceProvider = null!;
    
    /// <summary>
    /// Logger instance for comprehensive runtime logging
    /// </summary>
    protected readonly ILogger _logger = null!;

    /// <summary>
    /// Decentralized event hub for this service instance.
    /// </summary>
    public EventHub Hub { get; }

    /// <summary>
    /// Service name - can be overridden by derived classes
    /// </summary>
    public virtual string ServiceName => GetType().Name;

    /// <summary>
    /// Service version - can be overridden by derived classes
    /// </summary>
    public virtual string Version => "1.0.0";

    /// <summary>
    /// Chat client for direct AI operations - lightweight replacement for Semantic Kernel
    /// </summary>
    protected readonly IChatClient? _chatClient;

    /// <summary>
    /// Embedding client for vector operations - simple replacement for Kernel Memory
    /// </summary>
    protected readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingClient;

    /// <summary>
    /// Initializes a new instance of the ModernAiServiceBase with lightweight AI integration
    /// </summary>
    protected ModernAiServiceBase(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Hub = new EventHub(logger);

        // Optional dependencies - graceful degradation if not available
        _chatClient = _serviceProvider.GetService<IChatClient>();
        _embeddingClient = _serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

        _logger.LogInformation("🚀 Modern AI Service Base initialized");
        LogServiceAvailability();
    }

    /// <summary>
    /// Log availability of core AI services for debugging
    /// </summary>
    private void LogServiceAvailability()
    {
        _logger.LogInformation("📋 AI Service Availability:");
        _logger.LogInformation("  Event Hub: {Available}", Hub is not null ? "✅ Available" : "❌ Not Available");
        _logger.LogInformation("  Chat Client: {Available}", _chatClient is not null ? "✅ Available" : "❌ Not Available");
        _logger.LogInformation("  Embedding Client: {Available}", _embeddingClient is not null ? "✅ Available" : "❌ Not Available");
    }

    /// <summary>
    /// Extract payload from CX event - maintains compatibility with existing CX Language patterns
    /// </summary>
    protected virtual object? ExtractPayload(CxEvent eventData)
    {
        try
        {
            if (eventData?.payload is null)
            {
                _logger.LogWarning("⚠️ Event payload is null");
                return null;
            }

            _logger.LogDebug("📦 Extracting payload from event: {EventName}", eventData.name);
            return eventData.payload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error extracting payload from event");
            return null;
        }
    }

    /// <summary>
    /// Emit AI response event - maintains compatibility with CX Language event system
    /// </summary>
    protected virtual async Task EmitAiResponseEventAsync(string response, string sourceEvent)
    {
        var responseEvent = new CxEvent(
            "ai.response.generated",
            new { response, sourceEvent }
        );

        await Hub.EmitAsync(responseEvent.name, responseEvent);
        _logger.LogInformation("✅ AI response event emitted via local EventHub");
    }

    /// <summary>
    /// Complete chat operation using modern, lightweight AI client
    /// </summary>
    protected virtual async Task<string> CompleteChatAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        if (_chatClient is null)
        {
            _logger.LogError("❌ ChatClient is not available. Cannot complete chat.");
            return "Error: ChatClient is not configured.";
        }

        try
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, userPrompt)
            };

            var options = new ChatOptions { MaxOutputTokens = 2048 };
            
            _logger.LogInformation("🗣️ Sending chat completion request...");
            var response = await _chatClient.GetResponseAsync(messages, options, cancellationToken);

            var responseText = response.Messages?.LastOrDefault()?.Text ?? "";
            _logger.LogInformation("✅ Chat completion successful");
            return responseText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during chat completion");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Generate embedding using modern, lightweight AI client
    /// </summary>
    protected virtual async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (_embeddingClient is null)
        {
            _logger.LogError("❌ EmbeddingClient is not available. Cannot generate embedding.");
            return new List<float>();
        }

        try
        {
            _logger.LogInformation("🧠 Generating embedding for text...");
            var values = new[] { text };
            var response = await _embeddingClient.GenerateAsync(values, null, cancellationToken);
            var embedding = response.FirstOrDefault();
            _logger.LogInformation("✅ Embedding generation successful");
            return embedding?.Vector.ToArray() ?? (IReadOnlyList<float>)new List<float>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating embedding");
            return new List<float>();
        }
    }
}
