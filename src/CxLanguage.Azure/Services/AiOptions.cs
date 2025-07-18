using System;
using System.Collections.Generic;

namespace CxLanguage.Azure.Services
{
    /// <summary>
    /// Options for AI text generation requests
    /// </summary>
    public class AiRequestOptions
    {
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 1000;
        public string Model { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalOptions { get; set; } = new();
    }

    /// <summary>
    /// Options for AI analysis requests
    /// </summary>
    public class AiAnalysisOptions
    {
        public string AnalysisType { get; set; } = "general";
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Options for AI image generation requests
    /// </summary>
    public class AiImageOptions
    {
        public string Size { get; set; } = "1024x1024";
        public string Quality { get; set; } = "standard";
        public string Style { get; set; } = "vivid";
        public Dictionary<string, object> AdditionalOptions { get; set; } = new();
    }

    /// <summary>
    /// Options for AI image analysis requests
    /// </summary>
    public class AiImageAnalysisOptions
    {
        public bool EnableOCR { get; set; } = true;
        public bool EnableDescription { get; set; } = true;
        public bool EnableTags { get; set; } = true;
        public bool EnableObjects { get; set; } = true;
        public string Language { get; set; } = "en";
        public Dictionary<string, object> AdditionalOptions { get; set; } = new();
    }
}
