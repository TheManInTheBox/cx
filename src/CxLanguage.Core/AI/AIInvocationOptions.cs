using System;
using System.Collections.Generic;

namespace CxLanguage.Core.AI;

/// <summary>
/// Options for AI invocation calls
/// Provides configuration for AI event processing
/// </summary>
public class AIInvocationOptions
{
    /// <summary>
    /// Processing style (natural, formal, creative, etc.)
    /// </summary>
    public string Style { get; set; } = "natural";

    /// <summary>
    /// Context information for the AI operation
    /// </summary>
    public string Context { get; set; } = "";

    /// <summary>
    /// Processing depth (shallow, medium, deep)
    /// </summary>
    public string ProcessingDepth { get; set; } = "medium";

    /// <summary>
    /// Learning type for ai.learn operations
    /// </summary>
    public string LearningType { get; set; } = "pattern";

    /// <summary>
    /// Whether this operation should be consciousness-aware
    /// </summary>
    public bool ConsciousnessAware { get; set; } = true;

    /// <summary>
    /// Maximum processing time in milliseconds
    /// </summary>
    public int MaxProcessingTimeMs { get; set; } = 5000;

    /// <summary>
    /// Temperature for randomness in AI responses (0.0 to 1.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Additional custom parameters
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; set; } = new();

    /// <summary>
    /// Priority level for processing (low, medium, high, critical)
    /// </summary>
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Whether to enable performance monitoring for this operation
    /// </summary>
    public bool EnableTelemetry { get; set; } = true;
}
