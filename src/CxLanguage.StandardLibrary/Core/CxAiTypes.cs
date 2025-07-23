using System;
using System.Collections.Generic;

namespace CxLanguage.StandardLibrary.Core;

/// <summary>
/// Base class for CX AI service results
/// </summary>
public abstract class CxAiResult
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata about the result
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Timestamp when the result was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Base class for CX AI service options
/// </summary>
public abstract class CxAiOptions
{
    /// <summary>
    /// Maximum number of tokens to generate
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Temperature for response generation
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Additional parameters for the AI service
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}
