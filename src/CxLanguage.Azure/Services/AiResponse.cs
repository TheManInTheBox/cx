using System;
using System.Collections.Generic;

namespace CxLanguage.Azure.Services
{
    /// <summary>
    /// Response from AI text generation
    /// </summary>
    public class AiResponse
    {
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IsSuccess { get; set; } = true;
        public string? Error { get; set; }
    }

    /// <summary>
    /// Stream response from AI text generation
    /// </summary>
    public class AiStreamResponse
    {
        public IAsyncEnumerable<string> ContentStream { get; set; } = null!;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IsSuccess { get; set; } = true;
        public string? Error { get; set; }
    }

    /// <summary>
    /// Response from AI embedding generation
    /// </summary>
    public class AiEmbeddingResponse
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IsSuccess { get; set; } = true;
        public string? Error { get; set; }
    }

    /// <summary>
    /// Response from AI image generation
    /// </summary>
    public class AiImageResponse
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string RevisedPrompt { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IsSuccess { get; set; } = true;
        public string? Error { get; set; }
    }

    /// <summary>
    /// Response from AI image analysis
    /// </summary>
    public class AiImageAnalysisResponse
    {
        public string ExtractedText { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public List<AiDetectedObject> Objects { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IsSuccess { get; set; } = true;
        public string? Error { get; set; }
    }

    /// <summary>
    /// Detected object in image analysis
    /// </summary>
    public class AiDetectedObject
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public BoundingBox BoundingBox { get; set; } = new();
    }

    /// <summary>
    /// Bounding box for detected objects
    /// </summary>
    public class BoundingBox
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
