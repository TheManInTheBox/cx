using CxLanguage.StandardLibrary.Core;
using CxLanguage.StandardLibrary.AI.Common;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using System.ComponentModel;

namespace CxLanguage.StandardLibrary.AI.TextGeneration;

/// <summary>
/// Text generation service for CX standard library
/// Provides basic text generation capabilities using Semantic Kernel
/// </summary>
public class TextGenerationService : CxAiServiceBase
{
    private readonly ITextGenerationService _textGenerationService;

    /// <summary>
    /// Initializes a new instance of the TextGenerationService
    /// </summary>
    /// <param name="kernel">Semantic Kernel instance</param>
    /// <param name="logger">Logger instance</param>
    public TextGenerationService(Kernel kernel, ILogger<TextGenerationService> logger) 
        : base(kernel, logger)
    {
        _textGenerationService = kernel.GetRequiredService<ITextGenerationService>();
    }

    /// <summary>
    /// Gets the service name
    /// </summary>
    public override string ServiceName => "TextGeneration";
    
    /// <summary>
    /// Gets the service version
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Generate text based on a prompt
    /// </summary>
    /// <param name="prompt">The text prompt</param>
    /// <param name="options">Optional text generation settings</param>
    /// <returns>Text generation result</returns>
    public async Task<TextGenerationResult> GenerateAsync(string prompt, TextGenerationOptions? options = null)
    {
        var result = new TextGenerationResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Generating text for prompt: {Prompt}", prompt);

            var settings = new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>()
            };

            // Apply options if provided
            if (options != null)
            {
                if (options.MaxTokens.HasValue)
                    settings.ExtensionData["max_tokens"] = options.MaxTokens.Value;
                if (options.Temperature.HasValue)
                    settings.ExtensionData["temperature"] = options.Temperature.Value;
                if (options.TopP.HasValue)
                    settings.ExtensionData["top_p"] = options.TopP.Value;
                if (options.Seed.HasValue)
                    settings.ExtensionData["seed"] = options.Seed.Value;
                if (options.FrequencyPenalty.HasValue)
                    settings.ExtensionData["frequency_penalty"] = options.FrequencyPenalty.Value;
                if (options.PresencePenalty.HasValue)
                    settings.ExtensionData["presence_penalty"] = options.PresencePenalty.Value;
                if (options.StopSequences != null && options.StopSequences.Length > 0)
                    settings.ExtensionData["stop"] = options.StopSequences;
                    
                // Handle streaming if requested
                if (options.Stream == true)
                {
                    // For streaming, we'll return a special result indicating streaming is happening
                    var streamResult = new TextGenerationResult
                    {
                        IsSuccess = true,
                        GeneratedText = "[Streaming response - use GenerateStreamAsync for actual streaming]",
                        TokenCount = 0,
                        ExecutionTime = TimeSpan.Zero
                    };
                    return streamResult;
                }
            }

            var textContent = await _textGenerationService.GetTextContentAsync(prompt, settings);

            result.IsSuccess = true;
            result.GeneratedText = textContent.Text ?? string.Empty;
            result.TokenCount = textContent.Metadata?.Count ?? 0;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Text generation completed successfully. Generated {Length} characters", 
                result.GeneratedText.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text for prompt: {Prompt}", prompt);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Generate text with streaming support
    /// </summary>
    public async IAsyncEnumerable<TextGenerationStreamResult> GenerateStreamAsync(
        string prompt, 
        TextGenerationOptions? options = null)
    {
        var startTime = DateTimeOffset.UtcNow;

        await foreach (var streamContent in _textGenerationService.GetStreamingTextContentsAsync(prompt))
        {
            yield return new TextGenerationStreamResult
            {
                IsSuccess = true,
                TextChunk = streamContent.Text ?? string.Empty,
                IsComplete = false,
                ExecutionTime = DateTimeOffset.UtcNow - startTime
            };
        }

        yield return new TextGenerationStreamResult
        {
            IsSuccess = true,
            TextChunk = string.Empty,
            IsComplete = true,
            ExecutionTime = DateTimeOffset.UtcNow - startTime
        };
    }

    /// <summary>
    /// Synchronous wrapper for GenerateAsync
    /// </summary>
    public TextGenerationResult Generate(string prompt, TextGenerationOptions? options = null)
    {
        return GenerateAsync(prompt, options).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Options for text generation operations
/// </summary>
public class TextGenerationOptions : CxAiOptions
{
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
    
    /// <summary>
    /// Random seed for reproducible results
    /// </summary>
    public int? Seed { get; set; }
    
    /// <summary>
    /// Enable streaming responses
    /// </summary>
    public bool? Stream { get; set; }
}

/// <summary>
/// Result from text generation operations
/// </summary>
public class TextGenerationResult : CxAiResult
{
    /// <summary>
    /// The generated text
    /// </summary>
    public string GeneratedText { get; set; } = string.Empty;

    /// <summary>
    /// Number of tokens used in generation
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>
    /// Finish reason (completed, length, stop, etc.)
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// Result from streaming text generation
/// </summary>
public class TextGenerationStreamResult : CxAiResult
{
    /// <summary>
    /// Text chunk from the stream
    /// </summary>
    public string TextChunk { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the final chunk
    /// </summary>
    public bool IsComplete { get; set; }
}
