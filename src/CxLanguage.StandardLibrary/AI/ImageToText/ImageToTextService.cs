using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CxLanguage.StandardLibrary.AI.ImageToText;

/// <summary>
/// Provides image analysis and description capabilities using Semantic Kernel
/// </summary>
public class ImageToTextService : CxAiServiceBase
{
    private readonly IChatCompletionService _chatCompletionService;

    /// <summary>
    /// Initializes a new instance of the ImageToTextService
    /// </summary>
    /// <param name="kernel">Semantic Kernel instance</param>
    /// <param name="logger">Logger instance</param>
    public ImageToTextService(Kernel kernel, ILogger<ImageToTextService> logger) 
        : base(kernel, logger)
    {
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    /// <summary>
    /// Gets the service name
    /// </summary>
    public override string ServiceName => "ImageToText";
    
    /// <summary>
    /// Gets the service version
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Analyze an image and generate a text description
    /// </summary>
    public async Task<ImageAnalysisResult> AnalyzeImageAsync(string imageUrl, ImageToTextOptions? options = null)
    {
        var result = new ImageAnalysisResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Analyzing image: {ImageUrl}", imageUrl);

            var chatHistory = new ChatHistory();
            
            var systemMessage = options?.SystemPrompt ?? 
                "You are an AI assistant that can analyze images. Describe what you see in the image in detail.";
            chatHistory.AddSystemMessage(systemMessage);

            var userMessage = options?.Prompt ?? "Please describe this image in detail.";
            
            // Add the image to the chat history
            chatHistory.AddUserMessage([
                new TextContent(userMessage),
                new ImageContent(new Uri(imageUrl))
            ]);

            var settings = new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>()
            };

            if (options?.MaxTokens.HasValue == true)
                settings.ExtensionData["max_tokens"] = options.MaxTokens.Value;

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, settings);

            result.IsSuccess = true;
            result.Description = response.Content ?? string.Empty;
            result.ImageUrl = imageUrl;
            result.Confidence = CalculateConfidence(response.Content);
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Image analysis successful. Description length: {Length}", result.Description.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image: {ImageUrl}", imageUrl);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Analyze an image from byte data
    /// </summary>
    public async Task<ImageAnalysisResult> AnalyzeImageDataAsync(byte[] imageData, string mimeType, ImageToTextOptions? options = null)
    {
        var result = new ImageAnalysisResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Analyzing image data of size: {Size} bytes", imageData.Length);

            var chatHistory = new ChatHistory();
            
            var systemMessage = options?.SystemPrompt ?? 
                "You are an AI assistant that can analyze images. Describe what you see in the image in detail.";
            chatHistory.AddSystemMessage(systemMessage);

            var userMessage = options?.Prompt ?? "Please describe this image in detail.";
            
            // Add the image data to the chat history
            chatHistory.AddUserMessage([
                new TextContent(userMessage),
                new ImageContent(imageData, mimeType)
            ]);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);

            result.IsSuccess = true;
            result.Description = response.Content ?? string.Empty;
            result.ImageSize = imageData.Length;
            result.MimeType = mimeType;
            result.Confidence = CalculateConfidence(response.Content);
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Image data analysis successful. Description length: {Length}", result.Description.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image data");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Extract text from an image (OCR)
    /// </summary>
    public async Task<TextExtractionResult> ExtractTextAsync(string imageUrl, ImageToTextOptions? options = null)
    {
        var result = new TextExtractionResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Extracting text from image: {ImageUrl}", imageUrl);

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("You are an OCR assistant. Extract all text visible in the image. Return only the extracted text, no description or commentary.");

            var userMessage = "Please extract all text from this image. Return only the text content.";
            chatHistory.AddUserMessage([
                new TextContent(userMessage),
                new ImageContent(new Uri(imageUrl))
            ]);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);

            result.IsSuccess = true;
            result.ExtractedText = response.Content ?? string.Empty;
            result.ImageUrl = imageUrl;
            result.Confidence = CalculateConfidence(response.Content);
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Text extraction successful. Extracted {Length} characters", result.ExtractedText.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from image: {ImageUrl}", imageUrl);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Classify the content of an image
    /// </summary>
    public async Task<ImageClassificationResult> ClassifyImageAsync(string imageUrl, string[] categories, ImageToTextOptions? options = null)
    {
        var result = new ImageClassificationResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Classifying image: {ImageUrl} into {Count} categories", imageUrl, categories.Length);

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage($"You are an image classifier. Classify the image into one of these categories: {string.Join(", ", categories)}. Respond with only the category name that best matches.");

            var userMessage = $"Classify this image into one of these categories: {string.Join(", ", categories)}";
            chatHistory.AddUserMessage([
                new TextContent(userMessage),
                new ImageContent(new Uri(imageUrl))
            ]);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);

            var classification = response.Content?.Trim() ?? string.Empty;

            result.IsSuccess = true;
            result.Classification = classification;
            result.Categories = categories.ToList();
            result.ImageUrl = imageUrl;
            result.Confidence = CalculateConfidence(response.Content);
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Image classification successful. Result: {Classification}", result.Classification);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying image: {ImageUrl}", imageUrl);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Calculate confidence score based on response characteristics
    /// </summary>
    private float CalculateConfidence(string? response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return 0.0f;

        // Simple heuristic: longer, more detailed responses indicate higher confidence
        var length = response.Length;
        var confidence = Math.Min(1.0f, length / 500.0f); // Cap at 1.0
        return Math.Max(0.1f, confidence); // Minimum 0.1
    }

    /// <summary>
    /// Synchronous wrapper for AnalyzeImageAsync
    /// </summary>
    public ImageAnalysisResult AnalyzeImage(string imageUrl, ImageToTextOptions? options = null)
    {
        return AnalyzeImageAsync(imageUrl, options).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Options for image to text operations
/// </summary>
public class ImageToTextOptions : CxAiOptions
{
    /// <summary>
    /// Custom prompt for image analysis
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// System prompt to set context
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Maximum number of tokens in response
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Level of detail for analysis (brief, detailed, comprehensive)
    /// </summary>
    public string? DetailLevel { get; set; }

    /// <summary>
    /// Focus areas for analysis (objects, people, text, emotions, etc.)
    /// </summary>
    public string[]? FocusAreas { get; set; }
}

/// <summary>
/// Result from image analysis operations
/// </summary>
public class ImageAnalysisResult : CxAiResult
{
    /// <summary>
    /// Generated description of the image
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL of the analyzed image
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Size of the image data in bytes
    /// </summary>
    public long? ImageSize { get; set; }

    /// <summary>
    /// MIME type of the image
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Confidence score of the analysis (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Detected objects in the image
    /// </summary>
    public List<string> DetectedObjects { get; set; } = new();
}

/// <summary>
/// Result from text extraction operations
/// </summary>
public class TextExtractionResult : CxAiResult
{
    /// <summary>
    /// Extracted text from the image
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// URL of the source image
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Confidence score of the extraction (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Language detected in the text
    /// </summary>
    public string? DetectedLanguage { get; set; }
}

/// <summary>
/// Result from image classification operations
/// </summary>
public class ImageClassificationResult : CxAiResult
{
    /// <summary>
    /// The classification result
    /// </summary>
    public string Classification { get; set; } = string.Empty;

    /// <summary>
    /// Available categories for classification
    /// </summary>
    public List<string> Categories { get; set; } = new();

    /// <summary>
    /// URL of the classified image
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Confidence score of the classification (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; set; }
}
