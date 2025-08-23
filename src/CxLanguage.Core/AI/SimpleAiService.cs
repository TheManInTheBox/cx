using CxLanguage.Core.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Core.AI;

/// <summary>
/// Simple mock AI service for testing
/// </summary>
public class SimpleAiService : IAiService
{
    private readonly ILogger<SimpleAiService> _logger;

    public SimpleAiService(ILogger<SimpleAiService> logger)
    {
        _logger = logger;
    }

    public async Task<AiResponse> GenerateTextAsync(string prompt, AiRequestOptions? options = null)
    {
        _logger.LogInformation("Simple AI generating text for prompt: {Prompt}", prompt);
        await Task.Delay(100);
        return AiResponse.Success($"Simple AI response to: {prompt}");
    }

    public async Task<AiResponse> AnalyzeAsync(string content, AiAnalysisOptions options)
    {
        _logger.LogInformation("Simple AI analyzing content for task: {Task}", options.Task);
        await Task.Delay(100);
        return AiResponse.Success($"Simple AI analysis of content for task: {options.Task}");
    }

    public async Task<AiStreamResponse> StreamGenerateTextAsync(string prompt, AiRequestOptions? options = null)
    {
        _logger.LogInformation("Simple AI streaming text for prompt: {Prompt}", prompt);
        await Task.Delay(100);
        return new SimpleStreamResponse($"Simple AI streaming response to: {prompt}");
    }

    public async Task<AiResponse[]> ProcessBatchAsync(string[] prompts, AiRequestOptions? options = null)
    {
        _logger.LogInformation("Simple AI processing batch of {Count} prompts", prompts.Length);
        await Task.Delay(100);
        return prompts.Select(p => AiResponse.Success($"Simple AI response to: {p}")).ToArray();
    }

    public Task<AiEmbeddingResponse> GenerateEmbeddingAsync(string text, AiRequestOptions? options = null)
    {
        _logger.LogInformation("Simple AI generating embedding for text: {Text}", text);
        return Task.FromResult(AiEmbeddingResponse.Success(new float[] { 0.1f, 0.2f, 0.3f }));
    }

    public Task<AiImageResponse> GenerateImageAsync(string prompt, AiImageOptions? options = null)
    {
        _logger.LogInformation("Simple AI generating image for prompt: {Prompt}", prompt);
        return Task.FromResult(AiImageResponse.Success("https://mock-image-url.jpg", null, $"Generated image for: {prompt}"));
    }

    public Task<AiImageAnalysisResponse> AnalyzeImageAsync(string imageUrl, AiImageAnalysisOptions? options = null)
    {
        _logger.LogInformation("Simple AI analyzing image: {ImageUrl}", imageUrl);
        
        var description = $"Basic analysis of the image at {imageUrl}";
        var extractedText = "Extracted text from the image";
        var tags = new[] { "image", "analysis", "content", "visual" };
        var objects = new[] { "detected-object", "visual-element" };
        
        return Task.FromResult(AiImageAnalysisResponse.Success(description, extractedText, tags, objects));
    }
}

/// <summary>
/// Simple stream response for testing
/// </summary>
public class SimpleStreamResponse : AiStreamResponse
{
    private readonly string _response;

    public SimpleStreamResponse(string response)
    {
        _response = response;
    }

    public override async IAsyncEnumerable<string> GetTokensAsync()
    {
        var words = _response.Split(' ');
        foreach (var word in words)
        {
            await Task.Delay(50);
            yield return word + " ";
        }
    }

    public override void Dispose()
    {
        // No cleanup needed
    }
}
