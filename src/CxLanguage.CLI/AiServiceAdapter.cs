using CxLanguage.Core.AI;
using CxLanguage.Azure.Services;
using CxCoreAI = CxLanguage.Core.AI;
using CxAzureAI = CxLanguage.Azure.Services;

namespace CxLanguage.CLI;

/// <summary>
/// Adapter to bridge Azure AI service with Core AI interface
/// </summary>
public class AiServiceAdapter : CxCoreAI.IAiService
{
    private readonly CxAzureAI.IAiService _azureService;

    public AiServiceAdapter(CxAzureAI.IAiService azureService)
    {
        _azureService = azureService;
    }

    public async Task<CxCoreAI.AiResponse> GenerateTextAsync(string prompt, CxCoreAI.AiRequestOptions? options = null)
    {
        // Map from Core options to Azure options if needed
        var azureOptions = options != null ? new CxAzureAI.AiRequestOptions
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens
        } : null;

        var azureResponse = await _azureService.GenerateTextAsync(prompt, azureOptions);
        return new CxCoreAI.AiResponse
        {
            IsSuccess = azureResponse.IsSuccess,
            Content = azureResponse.Content,
            ErrorMessage = azureResponse.Error,
            Usage = null // Azure does not provide Usage
        };
    }

    public async Task<CxCoreAI.AiResponse> AnalyzeAsync(string content, CxCoreAI.AiAnalysisOptions options)
    {
        // Map from Core options to Azure options
        var azureOptions = new CxAzureAI.AiAnalysisOptions
        {
            AnalysisType = options.Task,
            Parameters = options.Parameters
        };

        var azureResponse = await _azureService.AnalyzeAsync(content, azureOptions);
        return new CxCoreAI.AiResponse
        {
            IsSuccess = azureResponse.IsSuccess,
            Content = azureResponse.Content,
            ErrorMessage = azureResponse.Error,
            Usage = null
        };
    }

    public async Task<CxCoreAI.AiStreamResponse> StreamGenerateTextAsync(string prompt, CxCoreAI.AiRequestOptions? options = null)
    {
        // Map from Core options to Azure options if needed
        var azureOptions = options != null ? new CxAzureAI.AiRequestOptions
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens
        } : null;

        var azureResponse = await _azureService.StreamGenerateTextAsync(prompt, azureOptions);
        return new CoreAiStreamResponseAdapter(azureResponse);
    }

    public async Task<CxCoreAI.AiResponse[]> ProcessBatchAsync(string[] prompts, CxCoreAI.AiRequestOptions? options = null)
    {
        // Map from Core options to Azure options if needed
        var azureOptions = options != null ? new CxAzureAI.AiRequestOptions
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens
        } : null;

        var azureResponses = await _azureService.ProcessBatchAsync(prompts, azureOptions);
        return azureResponses.Select(r => new CxCoreAI.AiResponse
        {
            IsSuccess = r.IsSuccess,
            Content = r.Content,
            ErrorMessage = r.Error,
            Usage = null
        }).ToArray();
    }

    public async Task<CxCoreAI.AiEmbeddingResponse> GenerateEmbeddingAsync(string text, CxCoreAI.AiRequestOptions? options = null)
    {
        // Map from Core options to Azure options if needed
        var azureOptions = options != null ? new CxAzureAI.AiRequestOptions
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens
        } : null;

        var azureResponse = await _azureService.GenerateEmbeddingAsync(text, azureOptions);
        return new CxCoreAI.AiEmbeddingResponse
        {
            IsSuccess = azureResponse.IsSuccess,
            Embedding = azureResponse.Embedding,
            ErrorMessage = azureResponse.Error,
            Usage = null
        };
    }

    public async Task<CxCoreAI.AiImageResponse> GenerateImageAsync(string prompt, CxCoreAI.AiImageOptions? options = null)
    {
        // Map from Core options to Azure options if needed
        var azureOptions = options != null ? new CxAzureAI.AiImageOptions
        {
            Size = options.Size,
            Quality = options.Quality,
            Style = options.Style,
            AdditionalOptions = new Dictionary<string, object>()
        } : null;

        var azureResponse = await _azureService.GenerateImageAsync(prompt, azureOptions);
        return new CxCoreAI.AiImageResponse
        {
            IsSuccess = azureResponse.IsSuccess,
            ImageUrl = azureResponse.ImageUrl,
            RevisedPrompt = azureResponse.RevisedPrompt,
            ErrorMessage = azureResponse.Error,
            Usage = null
        };
    }

    public async Task<CxCoreAI.AiImageAnalysisResponse> AnalyzeImageAsync(string imageUrl, CxCoreAI.AiImageAnalysisOptions? options = null)
    {
        // Map from Core options to Azure options if needed
        var azureOptions = options != null ? new CxAzureAI.AiImageAnalysisOptions
        {
            EnableOCR = options.EnableOCR,
            EnableDescription = options.EnableDescription,
            EnableTags = options.EnableTags,
            EnableObjects = options.EnableObjects,
            Language = options.Language,
            AdditionalOptions = new Dictionary<string, object>()
        } : null;

        var azureResponse = await _azureService.AnalyzeImageAsync(imageUrl, azureOptions);
        // Convert List<string> to string[] for Tags
        var tagsArray = azureResponse.Tags != null ? azureResponse.Tags.ToArray() : Array.Empty<string>();
        // Convert List<AiDetectedObject> to string[] for Objects (use Name property)
        var objectsArray = azureResponse.Objects != null ? azureResponse.Objects.Select(o => o.Name).ToArray() : Array.Empty<string>();
        return new CxCoreAI.AiImageAnalysisResponse
        {
            IsSuccess = azureResponse.IsSuccess,
            Description = azureResponse.Description,
            ExtractedText = azureResponse.ExtractedText,
            Tags = tagsArray,
            Objects = objectsArray,
            ErrorMessage = azureResponse.Error
        };
    }
}

/// <summary>
/// Adapter for stream response
/// </summary>
public class CoreAiStreamResponseAdapter : CxCoreAI.AiStreamResponse
{
    private readonly CxAzureAI.AiStreamResponse _azureResponse;

    public CoreAiStreamResponseAdapter(CxAzureAI.AiStreamResponse azureResponse)
    {
        _azureResponse = azureResponse;
    }

    public override IAsyncEnumerable<string> GetTokensAsync()
    {
        return _azureResponse.ContentStream;
    }

    public override void Dispose()
    {
        // No disposal needed for Azure AiStreamResponse
    }
}
