using System.ComponentModel;

namespace CxLanguage.StandardLibrary.AI.Common;

/// <summary>
/// Base class for AI operation options with comprehensive configuration
/// </summary>
public abstract class AIOptionsBase
{
    /// <summary>
    /// Maximum number of tokens to generate
    /// </summary>
    [Description("Maximum tokens to generate in the response")]
    public int? MaxTokens { get; set; }
    
    /// <summary>
    /// Temperature for randomness (0.0 to 2.0)
    /// </summary>
    [Description("Controls randomness: 0.0 = deterministic, 2.0 = very random")]
    public double? Temperature { get; set; }
    
    /// <summary>
    /// Top-p nucleus sampling (0.0 to 1.0)
    /// </summary>
    [Description("Nucleus sampling: controls diversity via cumulative probability")]
    public double? TopP { get; set; }
    
    /// <summary>
    /// Random seed for reproducible results
    /// </summary>
    [Description("Seed for deterministic/reproducible results")]
    public int? Seed { get; set; }
    
    /// <summary>
    /// Enable streaming responses
    /// </summary>
    [Description("Enable streaming response for real-time output")]
    public bool? Stream { get; set; }
    
    /// <summary>
    /// Stop sequences to end generation
    /// </summary>
    [Description("Sequences that will stop generation when encountered")]
    public List<string> StopSequences { get; set; } = new();
    
    /// <summary>
    /// Custom metadata for the operation
    /// </summary>
    [Description("Custom parameters specific to the AI service")]
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Timeout for the operation in seconds
    /// </summary>
    [Description("Operation timeout in seconds")]
    public int? TimeoutSeconds { get; set; }
    
    /// <summary>
    /// Enable detailed logging for debugging
    /// </summary>
    [Description("Enable detailed logging for debugging")]
    public bool EnableLogging { get; set; } = false;
}

/// <summary>
/// Options for text generation operations
/// </summary>
public class TextGenerationOptions : AIOptionsBase
{
    /// <summary>
    /// Stop sequences to halt generation (text-specific)
    /// </summary>
    public new string[]? StopSequences { get; set; }
    
    /// <summary>
    /// Frequency penalty (-2.0 to 2.0)
    /// </summary>
    public double? FrequencyPenalty { get; set; }
    
    /// <summary>
    /// Presence penalty (-2.0 to 2.0)
    /// </summary>
    public double? PresencePenalty { get; set; }
}

/// <summary>
/// Options for chat completion operations
/// </summary>
public class ChatCompletionOptions : AIOptionsBase
{
    /// <summary>
    /// System message to set context
    /// </summary>
    public string? SystemMessage { get; set; }
    
    /// <summary>
    /// Chat history for context
    /// </summary>
    public List<ChatMessage>? History { get; set; }
    
    /// <summary>
    /// Available functions for tool calling
    /// </summary>
    public List<FunctionDefinition>? Functions { get; set; }
    
    /// <summary>
    /// Force function calling behavior
    /// </summary>
    public string? FunctionCall { get; set; }
}

/// <summary>
/// Options for text embedding operations
/// </summary>
public class EmbeddingOptions : AIOptionsBase
{
    /// <summary>
    /// Embedding model to use
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    /// Dimension reduction for embeddings
    /// </summary>
    public int? Dimensions { get; set; }
    
    /// <summary>
    /// Batch size for multiple texts
    /// </summary>
    public int? BatchSize { get; set; }
}

/// <summary>
/// Options for image generation operations
/// </summary>
public class ImageGenerationOptions : AIOptionsBase
{
    /// <summary>
    /// Image size (e.g., "1024x1024")
    /// </summary>
    public string? Size { get; set; }
    
    /// <summary>
    /// Image quality (standard, hd)
    /// </summary>
    public string? Quality { get; set; }
    
    /// <summary>
    /// Number of images to generate
    /// </summary>
    public int? Count { get; set; }
    
    /// <summary>
    /// Style of the generated image
    /// </summary>
    public string? Style { get; set; }
}

/// <summary>
/// Chat message for conversation context
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Role of the message sender (e.g., "user", "assistant", "system")
    /// </summary>
    public string Role { get; set; } = "user";
    
    /// <summary>
    /// Content of the message
    /// </summary>
    public string Content { get; set; } = "";
    
    /// <summary>
    /// Optional metadata for the message
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Function definition for tool calling
/// </summary>
public class FunctionDefinition
{
    /// <summary>
    /// Name of the function
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Description of what the function does
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// JSON schema for function parameters
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}
