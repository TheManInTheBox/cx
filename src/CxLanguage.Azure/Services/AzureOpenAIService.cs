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
using System.Collections.Generic;

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
            
            // Check if this is new multi-service configuration format
            var servicesSection = configSection.GetSection("Services");
            if (servicesSection.Exists() && servicesSection.GetChildren().Any())
            {
                // New multi-service configuration format
                _logger.LogInformation("Using new multi-service configuration format");
                var multiConfig = configSection.Get<MultiServiceAzureOpenAIConfig>();
                
                var defaultServiceName = multiConfig?.DefaultService ?? multiConfig?.Services?.FirstOrDefault()?.Name;
                var defaultService = multiConfig?.Services?.FirstOrDefault(s => s.Name == defaultServiceName)
                                   ?? multiConfig?.Services?.FirstOrDefault();
                
                if (defaultService != null)
                {
                    _config = new AzureOpenAIConfig
                    {
                        Endpoint = defaultService.Endpoint,
                        DeploymentName = defaultService.Models?.ChatCompletion ?? "gpt-4",
                        ImageDeploymentName = defaultService.Models?.TextToImage ?? "dall-e-3",
                        TextToSpeechDeploymentName = defaultService.Models?.TextToSpeech ?? string.Empty,
                        ApiKey = defaultService.ApiKey,
                        ApiVersion = defaultService.ApiVersion ?? "2024-10-21",
                        MultiServiceConfig = defaultService // Store reference for model-specific API versions
                    };
                    
                    _logger.LogInformation("Selected service: {ServiceName} at {Endpoint}", defaultService.Name, defaultService.Endpoint);
                    if (!string.IsNullOrEmpty(_config.TextToSpeechDeploymentName))
                    {
                        _logger.LogInformation("  TextToSpeechDeploymentName: {DeploymentName}", _config.TextToSpeechDeploymentName);
                    }
                }
                else
                {
                    throw new InvalidOperationException("No valid Azure OpenAI service configuration found");
                }
            }
            else
            {
                // Legacy single-service configuration format
                _logger.LogInformation("Using legacy single-service configuration format");
                _config = new AzureOpenAIConfig
                {
                    Endpoint = configSection["Endpoint"] ?? "https://your-resource-name.openai.azure.com/",
                    DeploymentName = configSection["DeploymentName"] ?? "gpt-4",
                    ImageDeploymentName = configSection["ImageDeploymentName"] ?? "dall-e-3",
                    TextToSpeechDeploymentName = configSection["TextToSpeechDeploymentName"] ?? string.Empty,
                    ApiKey = configSection["ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API Key is required"),
                    ApiVersion = configSection["ApiVersion"] ?? "2024-06-01"
                };
            }
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

    /// <summary>
    /// Gets the API version for a specific model type, with fallback to service-level API version
    /// </summary>
    private string GetApiVersionForModel(string modelType)
    {
        // If we have multi-service configuration with model-specific API versions
        if (_config.MultiServiceConfig?.ModelApiVersions != null)
        {
            return modelType switch
            {
                "ChatCompletion" => _config.MultiServiceConfig.ModelApiVersions.ChatCompletion,
                "TextGeneration" => _config.MultiServiceConfig.ModelApiVersions.TextGeneration,
                "TextEmbedding" => _config.MultiServiceConfig.ModelApiVersions.TextEmbedding,
                "TextToImage" => _config.MultiServiceConfig.ModelApiVersions.TextToImage,
                "TextToSpeech" => _config.MultiServiceConfig.ModelApiVersions.TextToSpeech,
                "AudioToText" => _config.MultiServiceConfig.ModelApiVersions.AudioToText,
                "ImageToText" => _config.MultiServiceConfig.ModelApiVersions.ImageToText,
                _ => _config.ApiVersion
            };
        }
        
        // Fallback to service-level API version
        return _config.ApiVersion;
    }

    /// <summary>
    /// Builds the request URI with model-specific API version
    /// </summary>
    private Uri BuildRequestUri(string deploymentName, string endpoint, string modelType)
    {
        var apiVersion = GetApiVersionForModel(modelType);
        return new Uri($"{_config.Endpoint}openai/deployments/{deploymentName}/{endpoint}?api-version={apiVersion}");
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

            var requestUri = BuildRequestUri(_config.DeploymentName, "chat/completions", "ChatCompletion");
            
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

    public Task<CxLanguage.Core.AI.AiStreamResponse> StreamGenerateTextAsync(string prompt, CxLanguage.Core.AI.AiRequestOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Starting stream generation for prompt length: {Length}", prompt.Length);

            // Create streaming tokens using Azure OpenAI streaming API
            var tokens = StreamTokensFromAzureOpenAI(prompt, options);
            return Task.FromResult<CxLanguage.Core.AI.AiStreamResponse>(new AzureOpenAIStreamResponse(tokens));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting stream generation");
            throw;
        }
    }

    private async IAsyncEnumerable<string> StreamTokensFromAzureOpenAI(string prompt, CxLanguage.Core.AI.AiRequestOptions? options = null)
    {
        var operation = "StreamGenerateText";
        var stopwatch = Stopwatch.StartNew();
        
        using var httpClient = new HttpClient();
        
        // Add the API key if available
        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
        }
        
        // Create the streaming request content
        var requestContent = new
        {
            messages = new[]
            {
                new { role = "system", content = options?.SystemPrompt ?? "You are a helpful AI assistant in a programming language." },
                new { role = "user", content = prompt }
            },
            temperature = options?.Temperature ?? 0.7,
            max_tokens = options?.MaxTokens ?? 1000,
            stream = true // Enable streaming
        };
        
        var requestJson = JsonSerializer.Serialize(requestContent);
        var requestBody = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
        
        // Send the streaming request
        var requestUri = BuildRequestUri(_config.DeploymentName, "chat/completions", "ChatCompletion");
        using var response = await httpClient.PostAsync(requestUri, requestBody);
        
        if (response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6); // Remove "data: " prefix
                    
                    if (data == "[DONE]")
                    {
                        _logger.LogInformation("Streaming completed successfully");
                        _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, true);
                        break;
                    }
                    
                    // Parse streaming data and yield token if found
                    var token = ParseStreamingToken(data);
                    if (!string.IsNullOrEmpty(token))
                    {
                        yield return token;
                    }
                }
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Streaming request failed with status: {StatusCode}, Content: {Content}", 
                response.StatusCode, errorContent);
            _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, $"{response.StatusCode} - {errorContent}");
            yield return $"[ERROR] Streaming failed: {response.StatusCode} - {errorContent}";
        }
    }

    private string? ParseStreamingToken(string data)
    {
        try
        {
            var jsonData = JsonSerializer.Deserialize<JsonElement>(data);
            
            if (jsonData.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices.EnumerateArray().First();
                if (firstChoice.TryGetProperty("delta", out var delta) && 
                    delta.TryGetProperty("content", out var content))
                {
                    return content.GetString();
                }
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Error parsing streaming JSON: {Data}", data);
        }
        
        return null;
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

    public async Task<CxLanguage.Core.AI.AiEmbeddingResponse> GenerateEmbeddingAsync(string text, CxLanguage.Core.AI.AiRequestOptions? options = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var operation = "GenerateEmbedding";
        
        try
        {
            _logger.LogInformation("Generating embedding for text length: {Length}", text.Length);

            // Use Azure OpenAI's text-embedding-3-small API endpoint
            var embeddingDeployment = options?.Model ?? "text-embedding-3-small";
            var requestUri = BuildRequestUri(embeddingDeployment, "embeddings", "TextEmbedding");
            
            // Create an HttpClient
            using var httpClient = new HttpClient();
            
            // Add the API key if available
            if (!string.IsNullOrEmpty(_config.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
            }
            
            // Create the request content for embeddings
            var requestContent = new
            {
                input = text,
                user = "cx-language-runtime"
            };
            
            // Serialize the request content
            var requestJson = JsonSerializer.Serialize(requestContent);
            var requestBody = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
            
            // Send the request
            var response = await httpClient.PostAsync(requestUri, requestBody);
            
            // Read the complete response content
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Log the response for debugging
            _logger.LogInformation("Embedding API Response Status: {StatusCode}", response.StatusCode);
            
            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Parse the response
                    var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    // Extract the embedding data from the response
                    var dataArray = responseJson.GetProperty("data");
                    var firstEmbedding = dataArray.EnumerateArray().First();
                    var embeddingArray = firstEmbedding.GetProperty("embedding");
                    
                    // Convert JsonElement array to float array
                    var embedding = embeddingArray.EnumerateArray()
                        .Select(e => (float)e.GetDouble())
                        .ToArray();
                    
                    // Extract usage information if available
                    var usage = new CxLanguage.Core.AI.AiUsage();
                    if (responseJson.TryGetProperty("usage", out var usageJson))
                    {
                        usage.PromptTokens = usageJson.GetProperty("prompt_tokens").GetInt32();
                        usage.TotalTokens = usageJson.GetProperty("total_tokens").GetInt32();
                        usage.CompletionTokens = 0; // Embeddings don't have completion tokens
                    }
                    
                    // Track successful telemetry
                    _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, true, usage.TotalTokens);
                    
                    _logger.LogInformation("Embedding generation successful. Vector size: {Size}", embedding.Length);
                    return CxLanguage.Core.AI.AiEmbeddingResponse.Success(embedding, usage);
                }
                catch (Exception parseEx)
                {
                    _logger.LogError(parseEx, "Error parsing embedding response: {ResponseContent}", responseContent);
                    _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, parseEx.Message);
                    return CxLanguage.Core.AI.AiEmbeddingResponse.Failure($"Error parsing response: {parseEx.Message}");
                }
            }
            else
            {
                _logger.LogError("Embedding API request failed with status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);
                _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, $"{response.StatusCode} - {responseContent}");
                return CxLanguage.Core.AI.AiEmbeddingResponse.Failure($"Azure OpenAI Embedding Error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding");
            _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, ex.Message);
            return CxLanguage.Core.AI.AiEmbeddingResponse.Failure($"Error: {ex.Message}");
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
            var requestUri = BuildRequestUri(_config.ImageDeploymentName, "images/generations", "TextToImage");
            
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

            // Use GPT-4 Vision API for image analysis
            var requestUri = BuildRequestUri(_config.DeploymentName, "chat/completions", "ImageToText").ToString();
            
            using var httpClient = new HttpClient();
            
            // Add the API key if available
            if (!string.IsNullOrEmpty(_config.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
            }
            
            // Build analysis prompt based on options
            var analysisPrompt = BuildImageAnalysisPrompt(options);
            
            // Create the request content for image analysis using vision model
            var requestContent = new
            {
                messages = new[]
                {
                    new 
                    { 
                        role = "user", 
                        content = new object[]
                        {
                            new { type = "text", text = analysisPrompt },
                            new { type = "image_url", image_url = new { url = imageUrl } }
                        }
                    }
                },
                temperature = 0.1, // Low temperature for consistent analysis
                max_tokens = 1000,
                model = "gpt-4-vision-preview" // Use vision model
            };
            
            var requestJson = JsonSerializer.Serialize(requestContent);
            var requestBody = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
            
            // Send the request
            var response = await httpClient.PostAsync(requestUri, requestBody);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var content = responseJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
                    
                    // Parse the structured response
                    var analysisResult = ParseImageAnalysisResponse(content, options);
                    
                    // Track successful telemetry
                    _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, true, null, null);
                    return analysisResult;
                }
                catch (Exception parseEx)
                {
                    _logger.LogError(parseEx, "Error parsing image analysis response: {ResponseContent}", responseContent);
                    _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, parseEx.Message);
                    return CxLanguage.Core.AI.AiImageAnalysisResponse.Failure($"Error parsing response: {parseEx.Message}");
                }
            }
            else
            {
                _logger.LogError("Image analysis request failed with status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);
                _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, $"{response.StatusCode} - {responseContent}");
                return CxLanguage.Core.AI.AiImageAnalysisResponse.Failure($"Image Analysis Error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            _telemetryService?.TrackAzureOpenAIUsage(operation, stopwatch.Elapsed, false, null, ex.Message);
            return CxLanguage.Core.AI.AiImageAnalysisResponse.Failure($"Error: {ex.Message}");
        }
    }

    private string BuildImageAnalysisPrompt(CxLanguage.Core.AI.AiImageAnalysisOptions? options)
    {
        var promptParts = new List<string>
        {
            "Analyze this image and provide a structured response in JSON format with the following fields:"
        };

        if (options?.EnableDescription == true)
        {
            promptParts.Add("- description: A detailed description of the image");
        }

        if (options?.EnableTags == true)
        {
            promptParts.Add("- tags: An array of relevant tags or keywords");
        }

        if (options?.EnableObjects == true)
        {
            promptParts.Add("- objects: An array of objects or items visible in the image");
        }

        if (options?.EnableOCR == true)
        {
            promptParts.Add("- extractedText: Any text visible in the image");
        }

        promptParts.Add("Respond only with valid JSON.");

        return string.Join("\n", promptParts);
    }

    private CxLanguage.Core.AI.AiImageAnalysisResponse ParseImageAnalysisResponse(string content, CxLanguage.Core.AI.AiImageAnalysisOptions? options)
    {
        try
        {
            // Try to extract JSON from the response
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var analysisJson = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                var description = analysisJson.TryGetProperty("description", out var descElement) ? descElement.GetString() ?? "" : "";
                var extractedText = analysisJson.TryGetProperty("extractedText", out var textElement) ? textElement.GetString() ?? "" : "";
                
                var tags = new List<string>();
                if (analysisJson.TryGetProperty("tags", out var tagsElement) && tagsElement.ValueKind == JsonValueKind.Array)
                {
                    tags.AddRange(tagsElement.EnumerateArray().Select(t => t.GetString() ?? ""));
                }
                
                var objects = new List<string>();
                if (analysisJson.TryGetProperty("objects", out var objectsElement) && objectsElement.ValueKind == JsonValueKind.Array)
                {
                    objects.AddRange(objectsElement.EnumerateArray().Select(o => o.GetString() ?? ""));
                }
                
                return CxLanguage.Core.AI.AiImageAnalysisResponse.Success(description, extractedText, tags.ToArray(), objects.ToArray());
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Failed to parse JSON response, using fallback parsing");
        }
        
        // Fallback: Use the raw content as description
        return CxLanguage.Core.AI.AiImageAnalysisResponse.Success(content, "", new[] { "ai-generated" }, new[] { "analysis" });
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
    public string TextToSpeechDeploymentName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-06-01";
    
    // Reference to the original multi-service configuration for API version lookup
    public AzureOpenAIServiceConfig? MultiServiceConfig { get; set; }
}

/// <summary>
/// Multi-service configuration for Azure OpenAI services
/// </summary>
public class MultiServiceAzureOpenAIConfig
{
    public List<AzureOpenAIServiceConfig> Services { get; set; } = new();
    public string DefaultService { get; set; } = string.Empty;
    public Dictionary<string, string> ServiceSelection { get; set; } = new();
}

/// <summary>
/// Individual Azure OpenAI service configuration
/// </summary>
public class AzureOpenAIServiceConfig
{
    public string Name { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-10-21";
    public string Region { get; set; } = string.Empty;
    public AzureOpenAIModelsConfig Models { get; set; } = new();
    public ModelApiVersionsConfig? ModelApiVersions { get; set; }
}

/// <summary>
/// Azure OpenAI model deployments configuration
/// </summary>
public class AzureOpenAIModelsConfig
{
    public string? ChatCompletion { get; set; }
    public string? TextGeneration { get; set; }
    public string? TextEmbedding { get; set; }
    public string? TextToImage { get; set; }
    public string? TextToSpeech { get; set; }
    public string? AudioToText { get; set; }
    public string? ImageToText { get; set; }
}

/// <summary>
/// Model-specific API versions configuration
/// </summary>
public class ModelApiVersionsConfig
{
    public string ChatCompletion { get; set; } = "2024-10-21";
    public string TextGeneration { get; set; } = "2024-10-21";
    public string TextEmbedding { get; set; } = "2024-10-21";
    public string TextToImage { get; set; } = "2024-10-01";
    public string TextToSpeech { get; set; } = "2024-10-01";
    public string AudioToText { get; set; } = "2024-10-01";
    public string ImageToText { get; set; } = "2024-10-01";
}
