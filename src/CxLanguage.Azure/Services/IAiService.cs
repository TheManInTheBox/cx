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
    Task<AiEmbeddingResponse> GenerateEmbeddingAsync(string text, AiRequestOptions? options = null);
    Task<AiImageResponse> GenerateImageAsync(string prompt, AiImageOptions? options = null);
    Task<AiImageAnalysisResponse> AnalyzeImageAsync(string imageUrl, AiImageAnalysisOptions? options = null);
}
