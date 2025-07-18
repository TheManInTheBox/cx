using CxLanguage.Core.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Core.AI.Mock;

/// <summary>
/// Mock implementation of IMultiModalAI for testing and development
/// </summary>
public class MockMultiModalAI : IMultiModalAI
{
    private readonly ILogger<MockMultiModalAI> _logger;

    public MockMultiModalAI(ILogger<MockMultiModalAI> logger)
    {
        _logger = logger;
    }

    public async Task<AiResponse> ProcessTextAsync(string text, MultiModalOptions? options = null)
    {
        _logger.LogInformation("Mock text processing for text length: {Length}", text.Length);
        
        options ??= new MultiModalOptions();
        
        await Task.CompletedTask;
        return AiResponse.Success($"Mock processed text: {text.Substring(0, Math.Min(50, text.Length))}...", new AiUsage
        {
            PromptTokens = text.Length / 4,
            CompletionTokens = 20,
            TotalTokens = (text.Length / 4) + 20
        });
    }

    public async Task<AiResponse> ProcessImageAsync(byte[] imageData, string? description = null, MultiModalOptions? options = null)
    {
        _logger.LogInformation("Mock image processing for image size: {Size} bytes", imageData.Length);
        
        options ??= new MultiModalOptions();
        
        await Task.CompletedTask;
        return AiResponse.Success($"Mock processed image: {imageData.Length} bytes, description: {description ?? "none"}", new AiUsage
        {
            PromptTokens = 100,
            CompletionTokens = 30,
            TotalTokens = 130
        });
    }

    public async Task<AiResponse> ProcessAudioAsync(byte[] audioData, MultiModalOptions? options = null)
    {
        _logger.LogInformation("Mock audio processing for audio size: {Size} bytes", audioData.Length);
        
        options ??= new MultiModalOptions();
        
        await Task.CompletedTask;
        return AiResponse.Success($"Mock processed audio: {audioData.Length} bytes", new AiUsage
        {
            PromptTokens = 150,
            CompletionTokens = 40,
            TotalTokens = 190
        });
    }

    public async Task<AiResponse> ProcessVideoAsync(byte[] videoData, MultiModalOptions? options = null)
    {
        _logger.LogInformation("Mock video processing for video size: {Size} bytes", videoData.Length);
        
        options ??= new MultiModalOptions();
        
        await Task.CompletedTask;
        return AiResponse.Success($"Mock processed video: {videoData.Length} bytes", new AiUsage
        {
            PromptTokens = 200,
            CompletionTokens = 50,
            TotalTokens = 250
        });
    }

    public async Task<EmbeddingResult> GenerateEmbeddingsAsync(string text, EmbeddingOptions? options = null)
    {
        _logger.LogInformation("Mock embedding generation for text length: {Length}", text.Length);
        
        options ??= new EmbeddingOptions();
        
        // Generate mock embedding vector
        var dimensions = options.Dimensions ?? 1536;
        var embedding = new float[dimensions];
        var random = new Random(text.GetHashCode()); // Deterministic based on input
        
        for (int i = 0; i < dimensions; i++)
        {
            embedding[i] = (float)(random.NextDouble() - 0.5) * 2.0f; // Range [-1, 1]
        }
        
        await Task.CompletedTask;
        return EmbeddingResult.Success(embedding, new AiUsage
        {
            PromptTokens = text.Length / 4,
            CompletionTokens = 0,
            TotalTokens = text.Length / 4
        });
    }

    public async Task<FunctionCallResult> ExecuteFunctionAsync(string functionName, object[] parameters, FunctionCallOptions? options = null)
    {
        _logger.LogInformation("Mock function execution for function: {FunctionName}", functionName);
        
        options ??= new FunctionCallOptions();
        
        var startTime = DateTime.UtcNow;
        await Task.Delay(50); // Simulate processing time
        var endTime = DateTime.UtcNow;
        
        return FunctionCallResult.Success(functionName, parameters, 
            $"Mock result for {functionName}", endTime - startTime);
    }
}
