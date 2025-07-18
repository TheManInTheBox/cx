using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;

namespace CxLanguage.StandardLibrary.AI.TextToImage;

/// <summary>
/// Text to image generation service for CX standard library
/// Provides image generation capabilities using Semantic Kernel
/// </summary>
public class TextToImageService : CxAiServiceBase
{
    private readonly ITextToImageService _textToImageService;

    /// <summary>
    /// Initializes a new instance of the TextToImageService class
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <param name="logger">Logger for the service</param>
    public TextToImageService(Kernel kernel, ILogger<TextToImageService> logger) 
        : base(kernel, logger)
    {
        _textToImageService = kernel.GetRequiredService<ITextToImageService>();
    }

    /// <summary>
    /// Gets the name of this service
    /// </summary>
    public override string ServiceName => "TextToImage";
    
    /// <summary>
    /// Gets the version of this service
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Generate an image from text description
    /// </summary>
    public async Task<ImageGenerationResult> GenerateImageAsync(string description, TextToImageOptions? options = null)
    {
        var result = new ImageGenerationResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Generating image for description: {Description}", description);

            var imageUrl = await _textToImageService.GenerateImageAsync(
                description, 
                options?.Width ?? 1024, 
                options?.Height ?? 1024);

            result.IsSuccess = true;
            result.ImageUrl = imageUrl;
            result.Description = description;
            result.Width = options?.Width ?? 1024;
            result.Height = options?.Height ?? 1024;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Image generation successful. URL: {Url}", imageUrl);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image for description: {Description}", description);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Generate multiple images from text description
    /// </summary>
    public async Task<BatchImageGenerationResult> GenerateImagesAsync(
        string description, 
        int count = 1,
        TextToImageOptions? options = null)
    {
        var result = new BatchImageGenerationResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Generating {Count} images for description: {Description}", count, description);

            var images = new List<ImageGenerationResult>();

            for (int i = 0; i < count; i++)
            {
                var imageResult = await GenerateImageAsync(description, options);
                if (imageResult.IsSuccess)
                {
                    images.Add(imageResult);
                }
                else
                {
                    _logger.LogWarning("Failed to generate image {Index} of {Count}", i + 1, count);
                }
            }

            result.IsSuccess = true;
            result.Images = images;
            result.Description = description;
            result.Count = images.Count;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Batch image generation completed. Generated {Generated} of {Requested} images", 
                images.Count, count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch images for description: {Description}", description);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Download image from URL to byte array
    /// </summary>
    public async Task<ImageDownloadResult> DownloadImageAsync(string imageUrl)
    {
        var result = new ImageDownloadResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Downloading image from URL: {Url}", imageUrl);

            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

            result.IsSuccess = true;
            result.ImageData = imageBytes;
            result.ImageUrl = imageUrl;
            result.Size = imageBytes.Length;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Image download successful. Size: {Size} bytes", result.Size);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image from URL: {Url}", imageUrl);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Generate image with variations (if supported by the model)
    /// </summary>
    public async Task<ImageVariationResult> GenerateVariationsAsync(
        string originalImageUrl, 
        int count = 1,
        TextToImageOptions? options = null)
    {
        var result = new ImageVariationResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Generating {Count} variations of image: {Url}", count, originalImageUrl);

            // For now, we'll use the text-to-image service with a variation prompt
            // In a real implementation, this would use a dedicated variation service
            var variations = new List<ImageGenerationResult>();
            var baseDescription = $"A variation of the image at {originalImageUrl}";

            for (int i = 0; i < count; i++)
            {
                var variationDescription = $"{baseDescription} - variation {i + 1}";
                var variation = await GenerateImageAsync(variationDescription, options);
                if (variation.IsSuccess)
                {
                    variations.Add(variation);
                }
            }

            result.IsSuccess = true;
            result.OriginalImageUrl = originalImageUrl;
            result.Variations = variations;
            result.Count = variations.Count;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image variations: {Error}", ex.Message);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Synchronous wrapper for GenerateImageAsync
    /// </summary>
    public ImageGenerationResult GenerateImage(string description, TextToImageOptions? options = null)
    {
        return GenerateImageAsync(description, options).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Options for text to image operations
/// </summary>
public class TextToImageOptions : CxAiOptions
{
    /// <summary>
    /// Width of the generated image
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Height of the generated image
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Image quality (high, standard)
    /// </summary>
    public string? Quality { get; set; }

    /// <summary>
    /// Image style (natural, vivid)
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// Response format (url, b64_json)
    /// </summary>
    public string? ResponseFormat { get; set; }

    /// <summary>
    /// Number of images to generate
    /// </summary>
    public int? Count { get; set; }
}

/// <summary>
/// Result from image generation operations
/// </summary>
public class ImageGenerationResult : CxAiResult
{
    /// <summary>
    /// URL of the generated image
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Base64 encoded image data (if requested)
    /// </summary>
    public string? ImageData { get; set; }

    /// <summary>
    /// Description used to generate the image
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Width of the generated image
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Height of the generated image
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Revised prompt (if the model modified the input)
    /// </summary>
    public string? RevisedPrompt { get; set; }
}

/// <summary>
/// Result from batch image generation
/// </summary>
public class BatchImageGenerationResult : CxAiResult
{
    /// <summary>
    /// List of generated images
    /// </summary>
    public List<ImageGenerationResult> Images { get; set; } = new();

    /// <summary>
    /// Description used to generate the images
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Number of images successfully generated
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Result from image download operations
/// </summary>
public class ImageDownloadResult : CxAiResult
{
    /// <summary>
    /// Binary image data
    /// </summary>
    public byte[] ImageData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Source URL of the image
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Size of the image in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Content type of the image
    /// </summary>
    public string? ContentType { get; set; }
}

/// <summary>
/// Result from image variation operations
/// </summary>
public class ImageVariationResult : CxAiResult
{
    /// <summary>
    /// Original image URL used as source
    /// </summary>
    public string OriginalImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// List of generated variations
    /// </summary>
    public List<ImageGenerationResult> Variations { get; set; } = new();

    /// <summary>
    /// Number of variations successfully generated
    /// </summary>
    public int Count { get; set; }
}
