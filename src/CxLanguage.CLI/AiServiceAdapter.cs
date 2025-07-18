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
            Usage = azureResponse.Usage != null ? new CxCoreAI.AiUsage
            {
                PromptTokens = azureResponse.Usage.PromptTokens,
                CompletionTokens = azureResponse.Usage.CompletionTokens,
                TotalTokens = azureResponse.Usage.TotalTokens,
                EstimatedCost = azureResponse.Usage.EstimatedCost
            } : null
        };
    }

    public async Task<CxCoreAI.AiResponse> AnalyzeAsync(string content, CxCoreAI.AiAnalysisOptions options)
    {
        // Map from Core options to Azure options
        var azureOptions = new CxAzureAI.AiAnalysisOptions
        {
            Task = options.Task,
            ResponseFormat = options.ResponseFormat
        };

        var azureResponse = await _azureService.AnalyzeAsync(content, azureOptions);
        
        return new CxCoreAI.AiResponse
        {
            IsSuccess = azureResponse.IsSuccess,
            Content = azureResponse.Content,
            Usage = azureResponse.Usage != null ? new CxCoreAI.AiUsage
            {
                PromptTokens = azureResponse.Usage.PromptTokens,
                CompletionTokens = azureResponse.Usage.CompletionTokens,
                TotalTokens = azureResponse.Usage.TotalTokens,
                EstimatedCost = azureResponse.Usage.EstimatedCost
            } : null
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
            Usage = r.Usage != null ? new CxCoreAI.AiUsage
            {
                PromptTokens = r.Usage.PromptTokens,
                CompletionTokens = r.Usage.CompletionTokens,
                TotalTokens = r.Usage.TotalTokens,
                EstimatedCost = r.Usage.EstimatedCost
            } : null
        }).ToArray();
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
        return _azureResponse.GetTokensAsync();
    }

    public override void Dispose()
    {
        _azureResponse?.Dispose();
    }
}
