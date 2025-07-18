using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using CxLanguage.Core.AI;
using CxLanguage.Core.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text.Json;
using System.ClientModel;
using System.Diagnostics;

namespace CxLanguage.Azure.Services;

/// <summary>
/// Azure OpenAI service integration for Cx language
/// TODO: Update to use correct Azure OpenAI SDK 2.1.0-beta.1 APIs
/// </summary>
public class AzureOpenAIService : CxLanguage.Core.AI.IAiService, CxLanguage.Azure.Services.IAiService
{
    private readonly AzureOpenAIClient _client;
    private readonly ILogger<AzureOpenAIService> _logger;
    private readonly AzureOpenAIConfig _config;
    private readonly CxTelemetryService? _telemetryService;

    public AzureOpenAIService(IConfiguration configuration, ILogger<AzureOpenAIService> logger, CxTelemetryService? telemetryService = null)
    {
        _logger = logger;
        _telemetryService = telemetryService;
        
        try 
        {
            // Try to get configuration directly from appsettings.json
            var configSection = configuration.GetSection("AzureOpenAI");
            if (configSection == null || !configSection.Exists())
            {
                _logger.LogError("AzureOpenAI configuration section not found");
                throw new InvalidOperationException("AzureOpenAI configuration section not found");
            }
            
            // Log available configuration sections for debugging
            _logger.LogInformation("Available configuration sections:");
            foreach (var section in configuration.GetChildren())
            {
                _logger.LogInformation("Section: {SectionKey}", section.Key);
            }
            
            // Log all configuration values in the AzureOpenAI section
            _logger.LogInformation("AzureOpenAI configuration values:");
            foreach (var setting in configSection.GetChildren())
            {
                _logger.LogInformation("  {Key}: {Value}", setting.Key, setting.Value);
            }
            
            // Manual configuration parsing as a fallback
            _config = new AzureOpenAIConfig
            {
                Endpoint = configSection["Endpoint"] ?? "https://your-resource-name.openai.azure.com/",
                DeploymentName = configSection["DeploymentName"] ?? "gpt-4",
                ImageDeploymentName = configSection["ImageDeploymentName"] ?? "dall-e-3",
                ApiKey = configSection["ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API Key is required"),
                ApiVersion = configSection["ApiVersion"] ?? "2024-06-01"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading AzureOpenAI configuration");
            
            // Hardcoded fallback configuration for testing
            _config = new AzureOpenAIConfig
            {
                Endpoint = "https://your-resource-name.openai.azure.com/",
                DeploymentName = "gpt-4",
                ImageDeploymentName = "dall-e-3",
                ApiKey = string.Empty, // Will throw later if empty
                ApiVersion = "2024-06-01"
            };
            
            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                throw new InvalidOperationException("Azure OpenAI API Key is required");
            }
        }

        // Check if API key is provided in config
        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            // Use API key authentication
            _logger.LogInformation("Using API Key authentication for Azure OpenAI");
            _client = new AzureOpenAIClient(
                new Uri(_config.Endpoint), 
                new System.ClientModel.ApiKeyCredential(_config.ApiKey));
        }
        else
        {
            // Fall back to Managed Identity for authentication
            _logger.LogInformation("Using Managed Identity authentication for Azure OpenAI");
            var credential = new DefaultAzureCredential();
            _client = new AzureOpenAIClient(new Uri(_config.Endpoint), credential);
        }
    }

    public async Task<CxLanguage.Core.AI.AiResponse> GenerateTextAsync(string prompt, CxLanguage.Core.AI.AiRequestOptions? options = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var operation = "GenerateText";
        
        try
        {
            _logger.LogInformation("Generating text for prompt: {PromptStart}...", 
                prompt.Substring(0, Math.Min(50, prompt.Length)));

            // For beta versions, the API is still evolving
            // Let's use a simple approach for now by just constructing the request directly
            // This avoids issues with changing API names and structures

            var requestUri = new Uri($"{_config.Endpoint}openai/deployments/{_config.DeploymentName}/chat/completions?api-version={_config.ApiVersion}");
            
            // Create an HttpClient
            using var httpClient = new HttpClient();
            
            // Add the API key if available
            if (!string.IsNullOrEmpty(_config.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
            }
            
            // Create the request content
            var requestContent = new
            {
                messages = new[]
                {
                    new { role = "system", content = options?.SystemPrompt ?? "You are a helpful AI assistant in a programming language." },
                    new { role = "user", content = prompt }
                },
                temperature = options?.Temperature ?? 0.7,
                max_tokens = options?.MaxTokens ?? 1000,
                stream = false  // Explicitly disable streaming
            };
            
            // Serialize the request content
            var requestJson = JsonSerializer.Serialize(requestContent);
            var requestBody = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
            
            // Send the request
            var response = await httpClient.PostAsync(requestUri, requestBody);
            
            // Read the complete response content
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Log the full response for debugging
            _logger.LogInformation("Azure OpenAI Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("Azure OpenAI Response Content: {Content}", responseContent);
            
            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Parse the response
                    var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    // Extract the response content
                    var content = responseJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
                    
                    // Extract usage information if available
                    var usage = new CxLanguage.Core.AI.AiUsage();
                    int? totalTokens = null;
                    if (responseJson.TryGetProperty("usage", out var usageJson))
                    {
                        usage.PromptTokens = usageJson.GetProperty("prompt_tokens").GetInt32();
                        usage.CompletionTokens = usageJson.GetProperty("completion_tokens").GetInt32();
                        usage.TotalTokens = usageJson.GetProperty("total_tokens").GetInt32();
                        totalTokens = usage.TotalTokens;
                    }
                    
                    // Track successful telemetry
                    _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, true, totalTokens);
                    
                    return CxLanguage.Core.AI.AiResponse.Success(content, usage);
                }
                catch (Exception parseEx)
                {
                    _logger.LogError(parseEx, "Error parsing Azure OpenAI response: {ResponseContent}", responseContent);
                    _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, parseEx.Message);
                    return CxLanguage.Core.AI.AiResponse.Failure($"Error parsing response: {parseEx.Message}");
                }
            }
            else
            {
                _logger.LogError("Azure OpenAI request failed with status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);
                _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, $"{response.StatusCode} - {responseContent}");
                return CxLanguage.Core.AI.AiResponse.Failure($"Azure OpenAI Error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text");
            _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, ex.Message);
            return CxLanguage.Core.AI.AiResponse.Failure(ex.Message);
        }
    }

    public async Task<CxLanguage.Core.AI.AiResponse> AnalyzeAsync(string content, CxLanguage.Core.AI.AiAnalysisOptions options)
    {
        try
        {
            _logger.LogInformation("Analyzing content for task: {Task}", options.Task);

            // TODO: Implement with correct Azure OpenAI SDK APIs
            await Task.Delay(100); // Simulate API call
            
            var result = options.ResponseFormat == "json" 
                ? JsonSerializer.Serialize(new { analysis = "Placeholder analysis", task = options.Task })
                : $"Analysis result for task: {options.Task}";
                
            return CxLanguage.Core.AI.AiResponse.Success(result, new CxLanguage.Core.AI.AiUsage { PromptTokens = 15, CompletionTokens = 25, TotalTokens = 40 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing content");
            return CxLanguage.Core.AI.AiResponse.Failure(ex.Message);
        }
    }

    public async Task<CxLanguage.Core.AI.AiStreamResponse> StreamGenerateTextAsync(string prompt, CxLanguage.Core.AI.AiRequestOptions? options = null)
    {
        await Task.CompletedTask;
        try
        {
            _logger.LogInformation("Starting stream generation for prompt length: {Length}", prompt.Length);

            // TODO: Implement with correct Azure OpenAI SDK APIs
            // Placeholder streaming implementation
            var tokens = GenerateTokens(prompt);
            return new AzureOpenAIStreamResponse(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting stream generation");
            throw;
        }
    }

    public async Task<CxLanguage.Core.AI.AiResponse[]> ProcessBatchAsync(string[] prompts, CxLanguage.Core.AI.AiRequestOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Processing batch of {Count} prompts", prompts.Length);

            var tasks = prompts.Select(prompt => GenerateTextAsync(prompt, options));
            var results = await Task.WhenAll(tasks);
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch");
            throw;
        }
    }

    public Task<CxLanguage.Core.AI.AiEmbeddingResponse> GenerateEmbeddingAsync(string text, CxLanguage.Core.AI.AiRequestOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Generating embedding for text: {Text}", text);

            // Note: This is a placeholder implementation
            // In a real implementation, you would use Azure OpenAI's embedding API
            _logger.LogWarning("Embedding generation not yet implemented for Azure OpenAI service");
            
            // Return a failure response for now
            return Task.FromResult(CxLanguage.Core.AI.AiEmbeddingResponse.Failure("Embedding generation not implemented for Azure OpenAI service"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding");
            return Task.FromResult(CxLanguage.Core.AI.AiEmbeddingResponse.Failure($"Error: {ex.Message}"));
        }
    }

    public async Task<CxLanguage.Core.AI.AiImageResponse> GenerateImageAsync(string prompt, CxLanguage.Core.AI.AiImageOptions? options = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var operation = "GenerateImage";
        
        try
        {
            _logger.LogInformation("Generating image for prompt: {Prompt}", prompt);

            // Use DALL-E 3 API endpoint
            var requestUri = new Uri($"{_config.Endpoint}openai/deployments/{_config.ImageDeploymentName}/images/generations?api-version={_config.ApiVersion}");
            
            // Create an HttpClient
            using var httpClient = new HttpClient();
            
            // Add the API key if available
            if (!string.IsNullOrEmpty(_config.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
            }
            
            // Create the request content for DALL-E 3
            var requestContent = new
            {
                prompt = prompt,
                size = options?.Size ?? "1024x1024",
                quality = options?.Quality ?? "standard",
                style = options?.Style ?? "vivid",
                n = 1,
                response_format = options?.ResponseFormat ?? "url"
            };
            
            // Serialize the request content
            var requestJson = JsonSerializer.Serialize(requestContent);
            var requestBody = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
            
            // Send the request
            var response = await httpClient.PostAsync(requestUri, requestBody);
            
            // Read the complete response content
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Log the response for debugging
            _logger.LogInformation("DALL-E 3 Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("DALL-E 3 Response Content: {Content}", responseContent);
            
            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Parse the response
                    var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    // Extract the image data from the response
                    var dataArray = responseJson.GetProperty("data");
                    var firstImage = dataArray.EnumerateArray().First();
                    
                    string? imageUrl = null;
                    byte[]? imageData = null;
                    string? revisedPrompt = null;
                    
                    // Get the image URL or data based on response format
                    if (firstImage.TryGetProperty("url", out var urlElement))
                    {
                        imageUrl = urlElement.GetString();
                    }
                    
                    if (firstImage.TryGetProperty("b64_json", out var b64Element))
                    {
                        var base64Data = b64Element.GetString();
                        if (!string.IsNullOrEmpty(base64Data))
                        {
                            imageData = Convert.FromBase64String(base64Data);
                        }
                    }
                    
                    if (firstImage.TryGetProperty("revised_prompt", out var revisedElement))
                    {
                        revisedPrompt = revisedElement.GetString();
                    }
                    
                    // Track successful telemetry
                    _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, true);
                    
                    var metadata = new Dictionary<string, object>
                    {
                        ["model"] = "dall-e-3",
                        ["size"] = options?.Size ?? "1024x1024",
                        ["quality"] = options?.Quality ?? "standard",
                        ["style"] = options?.Style ?? "vivid",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                    };
                    
                    var result = CxLanguage.Core.AI.AiImageResponse.Success(imageUrl, imageData, revisedPrompt);
                    result.Metadata.Clear();
                    foreach (var kvp in metadata)
                    {
                        result.Metadata[kvp.Key] = kvp.Value;
                    }
                    
                    return result;
                }
                catch (Exception parseEx)
                {
                    _logger.LogError(parseEx, "Error parsing DALL-E 3 response: {ResponseContent}", responseContent);
                    _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, parseEx.Message);
                    return CxLanguage.Core.AI.AiImageResponse.Failure($"Error parsing response: {parseEx.Message}");
                }
            }
            else
            {
                _logger.LogError("DALL-E 3 request failed with status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);
                _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, $"{response.StatusCode} - {responseContent}");
                return CxLanguage.Core.AI.AiImageResponse.Failure($"DALL-E 3 Error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image with DALL-E 3");
            _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, ex.Message);
            return CxLanguage.Core.AI.AiImageResponse.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<CxLanguage.Core.AI.AiImageAnalysisResponse> AnalyzeImageAsync(string imageUrl, CxLanguage.Core.AI.AiImageAnalysisOptions? options = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var operation = "AnalyzeImage";
        
        try
        {
            _logger.LogInformation("Analyzing image: {ImageUrl}", imageUrl);

            // For now, use a simple mock implementation
            // TODO: Implement actual Azure Computer Vision API integration
            await Task.Delay(500); // Simulate processing time

            var description = $"Mock analysis of image: {imageUrl}";
            var extractedText = "Mock extracted text from image";
            var tags = new[] { "mock", "analysis", "image" };
            var objects = new[] { "mock-object" };

            _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, true, null, null);
            return CxLanguage.Core.AI.AiImageAnalysisResponse.Success(description, extractedText, tags, objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, ex.Message);
            return CxLanguage.Core.AI.AiImageAnalysisResponse.Failure($"Error: {ex.Message}");
        }
    }

    private async IAsyncEnumerable<string> GenerateTokens(string prompt)
    {
        // Placeholder token generation
        var words = $"This is a streaming response to: {prompt}".Split(' ');
        foreach (var word in words)
        {
            await Task.Delay(50); // Simulate streaming delay
            yield return word + " ";
        }
    }

    #region Azure Interface Implementations

    // Azure interface versions for explicit implementation
    async Task<CxLanguage.Azure.Services.AiResponse> IAiService.GenerateTextAsync(string prompt, CxLanguage.Azure.Services.AiRequestOptions? options)
    {
        var coreOptions = options != null ? new CxLanguage.Core.AI.AiRequestOptions
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            Model = options.Model,
            Parameters = options.AdditionalOptions
        } : null;

        var coreResponse = await GenerateTextAsync(prompt, coreOptions);
        return new CxLanguage.Azure.Services.AiResponse
        {
            Content = coreResponse.Content,
            Metadata = coreResponse.Metadata,
            IsSuccess = coreResponse.IsSuccess,
            Error = coreResponse.ErrorMessage
        };
    }

    async Task<CxLanguage.Azure.Services.AiResponse> IAiService.AnalyzeAsync(string content, CxLanguage.Azure.Services.AiAnalysisOptions options)
    {
        var coreOptions = new CxLanguage.Core.AI.AiAnalysisOptions
        {
            Task = options.AnalysisType,
            ResponseFormat = "text", // Default to text for now
            Parameters = options.Parameters
        };

        var coreResponse = await AnalyzeAsync(content, coreOptions);
        return new CxLanguage.Azure.Services.AiResponse
        {
            Content = coreResponse.Content,
            Metadata = coreResponse.Metadata,
            IsSuccess = coreResponse.IsSuccess,
            Error = coreResponse.ErrorMessage
        };
    }

    async Task<CxLanguage.Azure.Services.AiStreamResponse> IAiService.StreamGenerateTextAsync(string prompt, CxLanguage.Azure.Services.AiRequestOptions? options)
    {
        var coreOptions = options != null ? new CxLanguage.Core.AI.AiRequestOptions
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            Model = options.Model,
            Parameters = options.AdditionalOptions
        } : null;

        var coreResponse = await StreamGenerateTextAsync(prompt, coreOptions);
        var streamResponse = new CxLanguage.Azure.Services.AiStreamResponse
        {
            // Convert Core stream to Azure stream format
            ContentStream = coreResponse.GetTokensAsync(),
            Metadata = new Dictionary<string, object>(),
            IsSuccess = true,
            Error = null
        };
        return streamResponse;
    }

    async Task<CxLanguage.Azure.Services.AiResponse[]> IAiService.ProcessBatchAsync(string[] prompts, CxLanguage.Azure.Services.AiRequestOptions? options)
    {
        var coreOptions = options != null ? new CxLanguage.Core.AI.AiRequestOptions
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            Model = options.Model,
            Parameters = options.AdditionalOptions
        } : null;

        var coreResponses = await ProcessBatchAsync(prompts, coreOptions);
        return coreResponses.Select(r => new CxLanguage.Azure.Services.AiResponse
        {
            Content = r.Content,
            Metadata = r.Metadata,
            IsSuccess = r.IsSuccess,
            Error = r.ErrorMessage
        }).ToArray();
    }

    async Task<CxLanguage.Azure.Services.AiEmbeddingResponse> IAiService.GenerateEmbeddingAsync(string text, CxLanguage.Azure.Services.AiRequestOptions? options)
    {
        var coreOptions = options != null ? new CxLanguage.Core.AI.AiRequestOptions
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            Model = options.Model
            // Note: Core and Azure have different property names
        } : null;

        var coreResponse = await GenerateEmbeddingAsync(text, coreOptions);
        return new CxLanguage.Azure.Services.AiEmbeddingResponse
        {
            Embedding = coreResponse.Embedding,
            Metadata = coreResponse.Metadata,
            IsSuccess = coreResponse.IsSuccess,
            Error = coreResponse.ErrorMessage
        };
    }

    async Task<CxLanguage.Azure.Services.AiImageResponse> IAiService.GenerateImageAsync(string prompt, CxLanguage.Azure.Services.AiImageOptions? options)
    {
        var coreOptions = options != null ? new CxLanguage.Core.AI.AiImageOptions
        {
            Size = options.Size,
            Quality = options.Quality,
            Style = options.Style,
            Parameters = options.AdditionalOptions
        } : null;

        var coreResponse = await GenerateImageAsync(prompt, coreOptions);
        return new CxLanguage.Azure.Services.AiImageResponse
        {
            ImageUrl = coreResponse.ImageUrl ?? string.Empty,
            RevisedPrompt = coreResponse.RevisedPrompt ?? string.Empty,
            Metadata = coreResponse.Metadata,
            IsSuccess = coreResponse.IsSuccess,
            Error = coreResponse.ErrorMessage
        };
    }
    
    async Task<CxLanguage.Azure.Services.AiImageAnalysisResponse> IAiService.AnalyzeImageAsync(string imageUrl, CxLanguage.Azure.Services.AiImageAnalysisOptions? options)
    {
        var coreOptions = options != null ? new CxLanguage.Core.AI.AiImageAnalysisOptions
        {
            EnableOCR = options.EnableOCR,
            EnableDescription = options.EnableDescription,
            EnableTags = options.EnableTags,
            EnableObjects = options.EnableObjects,
            Language = options.Language,
            Parameters = options.AdditionalOptions
        } : null;

        var coreResponse = await AnalyzeImageAsync(imageUrl, coreOptions);
        
        // Convert from Core string[] to Azure List<AiDetectedObject>
        var objectsList = new List<CxLanguage.Azure.Services.AiDetectedObject>();
        if (coreResponse.Objects != null)
        {
            foreach (var obj in coreResponse.Objects)
            {
                objectsList.Add(new CxLanguage.Azure.Services.AiDetectedObject
                {
                    Name = obj,
                    Confidence = 1.0, // Default confidence
                    BoundingBox = new CxLanguage.Azure.Services.BoundingBox() // Empty bounding box
                });
            }
        }
        
        return new CxLanguage.Azure.Services.AiImageAnalysisResponse
        {
            Description = coreResponse.Description,
            ExtractedText = coreResponse.ExtractedText,
            Tags = new List<string>(coreResponse.Tags),
            Objects = objectsList,
            Metadata = coreResponse.Metadata,
            IsSuccess = coreResponse.IsSuccess,
            Error = coreResponse.ErrorMessage
        };
    }

    #endregion
}

/// <summary>
/// Streaming response wrapper for Azure OpenAI
/// </summary>
public class AzureOpenAIStreamResponse : CxLanguage.Core.AI.AiStreamResponse
{
    private readonly IAsyncEnumerable<string> _tokens;

    public AzureOpenAIStreamResponse(IAsyncEnumerable<string> tokens)
    {
        _tokens = tokens;
    }

    public override async IAsyncEnumerable<string> GetTokensAsync()
    {
        await foreach (var token in _tokens)
        {
            yield return token;
        }
    }

    public override void Dispose()
    {
        // No disposal needed for this implementation
    }
}

/// <summary>
/// Configuration for Azure OpenAI service
/// </summary>
public class AzureOpenAIConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ImageDeploymentName { get; set; } = "dall-e-3";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-06-01";
}
