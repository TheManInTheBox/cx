namespace CxLanguage.Azure.Services;

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
/// Options for AI requests
/// </summary>
public class AiRequestOptions
{
    public int MaxTokens { get; set; } = 1000;
    public double Temperature { get; set; } = 0.7;
    public double? TopP { get; set; }
    public double? FrequencyPenalty { get; set; }
    public double? PresencePenalty { get; set; }
    public string[]? StopSequences { get; set; }
    public Dictionary<string, object> AdditionalParameters { get; set; } = new();
}

/// <summary>
/// Options for AI analysis tasks
/// </summary>
public class AiAnalysisOptions
{
    public string Task { get; set; } = string.Empty;
    public string ResponseFormat { get; set; } = "text"; // "text" or "json"
    public string? JsonSchema { get; set; }
    public int? MaxTokens { get; set; }
    public double? Temperature { get; set; }
    public string[]? AdditionalInstructions { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Parallel processing options
/// </summary>
public class ParallelProcessingOptions
{
    public int MaxConcurrency { get; set; } = 5;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool FailOnFirstError { get; set; } = false;
    public RetryPolicy? RetryPolicy { get; set; }
}

/// <summary>
/// Retry policy for AI operations
/// </summary>
public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
    public double BackoffMultiplier { get; set; } = 2.0;
    public Func<Exception, bool>? ShouldRetry { get; set; }
}
