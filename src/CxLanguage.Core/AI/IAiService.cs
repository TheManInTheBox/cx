namespace CxLanguage.Core.AI;

/// <summary>
/// Common interface for AI services in Cx language
/// </summary>
public interface IAiService
{
    Task<AiResponse> GenerateTextAsync(string prompt, AiRequestOptions? options = null);
    Task<AiResponse> AnalyzeAsync(string content, AiAnalysisOptions options);
    Task<AiStreamResponse> StreamGenerateTextAsync(string prompt, AiRequestOptions? options = null);
    Task<AiResponse[]> ProcessBatchAsync(string[] prompts, AiRequestOptions? options = null);
    Task<AiEmbeddingResponse> GenerateEmbeddingAsync(string text, AiRequestOptions? options = null);
    Task<AiImageResponse> GenerateImageAsync(string prompt, AiImageOptions? options = null);
    Task<AiImageAnalysisResponse> AnalyzeImageAsync(string imageUrl, AiImageAnalysisOptions? options = null);
}

/// <summary>
/// AI service response
/// </summary>
public class AiResponse
{
    public bool IsSuccess { get; init; }
    public string Content { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public AiUsage? Usage { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();

    public static AiResponse Success(string content, AiUsage? usage = null) =>
        new() { IsSuccess = true, Content = content, Usage = usage };

    public static AiResponse Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// AI embedding response
/// </summary>
public class AiEmbeddingResponse
{
    public bool IsSuccess { get; init; }
    public float[] Embedding { get; init; } = Array.Empty<float>();
    public string? ErrorMessage { get; init; }
    public AiUsage? Usage { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();

    public static AiEmbeddingResponse Success(float[] embedding, AiUsage? usage = null) =>
        new() { IsSuccess = true, Embedding = embedding, Usage = usage };

    public static AiEmbeddingResponse Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// AI image generation response
/// </summary>
public class AiImageResponse
{
    public bool IsSuccess { get; init; }
    public string? ImageUrl { get; init; }
    public byte[]? ImageData { get; init; }
    public string? RevisedPrompt { get; init; }
    public string? ErrorMessage { get; init; }
    public AiUsage? Usage { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();

    public static AiImageResponse Success(string? imageUrl = null, byte[]? imageData = null, string? revisedPrompt = null, AiUsage? usage = null) =>
        new() { IsSuccess = true, ImageUrl = imageUrl, ImageData = imageData, RevisedPrompt = revisedPrompt, Usage = usage };

    public static AiImageResponse Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Streaming AI response
/// </summary>
public abstract class AiStreamResponse : IDisposable
{
    public abstract IAsyncEnumerable<string> GetTokensAsync();
    public abstract void Dispose();
}

/// <summary>
/// Token usage information
/// </summary>
public class AiUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal? EstimatedCost { get; set; }
}

/// <summary>
/// Request options for AI services
/// </summary>
public class AiRequestOptions
{
    public string? Model { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4000;
    public string? SystemPrompt { get; set; }
    public bool UseStreaming { get; set; } = false;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Analysis options for AI services
/// </summary>
public class AiAnalysisOptions
{
    public string Task { get; set; } = string.Empty;
    public string ResponseFormat { get; set; } = "text";
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Image generation options for AI services
/// </summary>
public class AiImageOptions
{
    public string? Model { get; set; } = "dall-e-3";
    public string Size { get; set; } = "1024x1024";
    public string Quality { get; set; } = "standard";
    public string Style { get; set; } = "vivid";
    public string ResponseFormat { get; set; } = "url";
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Image analysis response
/// </summary>
public class AiImageAnalysisResponse
{
    public bool IsSuccess { get; init; }
    public string Description { get; init; } = string.Empty;
    public string ExtractedText { get; init; } = string.Empty;
    public string[] Tags { get; init; } = Array.Empty<string>();
    public string[] Objects { get; init; } = Array.Empty<string>();
    public string? ErrorMessage { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();

    public static AiImageAnalysisResponse Success(string description, string extractedText, string[] tags, string[] objects) =>
        new() { IsSuccess = true, Description = description, ExtractedText = extractedText, Tags = tags, Objects = objects };

    public static AiImageAnalysisResponse Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Image analysis options for AI services
/// </summary>
public class AiImageAnalysisOptions
{
    public bool EnableOCR { get; set; } = true;
    public bool EnableDescription { get; set; } = true;
    public bool EnableTags { get; set; } = true;
    public bool EnableObjects { get; set; } = true;
    public string Language { get; set; } = "en";
    public Dictionary<string, object> Parameters { get; set; } = new();
}
