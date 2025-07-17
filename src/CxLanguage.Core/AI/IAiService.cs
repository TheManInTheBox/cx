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
