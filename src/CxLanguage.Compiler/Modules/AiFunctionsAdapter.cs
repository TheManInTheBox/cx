using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.AI;
using CxLanguage.Core.Telemetry;
using CxLanguage.Core.Serialization;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Semantic Kernel-based replacement for AiFunctions
/// Maintains the same interface but uses Semantic Kernel internally
/// </summary>
public class AiFunctionsAdapter : AiFunctions
{
    private readonly SemanticKernelAiFunctions _semanticKernelAiFunctions;

    public AiFunctionsAdapter(
        SemanticKernelAiFunctions semanticKernelAiFunctions,
        ILogger<AiFunctions> logger, 
        CxTelemetryService? telemetryService = null)
        : base(null!, logger, telemetryService) // Pass null for IAgenticRuntime since we override everything
    {
        _semanticKernelAiFunctions = semanticKernelAiFunctions ?? throw new ArgumentNullException(nameof(semanticKernelAiFunctions));
    }

    /// <summary>
    /// Implements the 'task' AI function using Semantic Kernel
    /// </summary>
    public new async Task<object> TaskAsync(string goal, object? options = null)
    {
        return await _semanticKernelAiFunctions.TaskAsync(goal, options);
    }

    /// <summary>
    /// Implements the 'synthesize' AI function using Semantic Kernel
    /// </summary>
    public new async Task<object> SynthesizeAsync(string specification, object? options = null)
    {
        return await _semanticKernelAiFunctions.SynthesizeAsync(specification, options);
    }

    /// <summary>
    /// Implements the 'reason' AI function using Semantic Kernel
    /// </summary>
    public new async Task<object> ReasonAsync(string question, object? options = null)
    {
        return await _semanticKernelAiFunctions.ReasonAsync(question, options);
    }

    /// <summary>
    /// Implements the 'process' AI function using Semantic Kernel
    /// </summary>
    public new async Task<object> ProcessAsync(string input, string context, object? options = null)
    {
        return await _semanticKernelAiFunctions.ProcessAsync(input, context, options);
    }

    /// <summary>
    /// Implements the 'generate' AI function using Semantic Kernel
    /// </summary>
    public new async Task<object> GenerateAsync(string prompt, object? options = null)
    {
        return await _semanticKernelAiFunctions.GenerateAsync(prompt, options);
    }

    /// <summary>
    /// Implements the 'embed' AI function using Semantic Kernel
    /// </summary>
    public new async Task<object> EmbedAsync(string text, object? options = null)
    {
        return await _semanticKernelAiFunctions.EmbedAsync(text, options);
    }

    /// <summary>
    /// Implements the 'adapt' AI function using Semantic Kernel
    /// </summary>
    public async Task<object> AdaptAsync(string content, object? options = null)
    {
        return await _semanticKernelAiFunctions.AdaptAsync(content, options);
    }

    /// <summary>
    /// Synchronous wrapper for task function
    /// </summary>
    public new object Task(string goal, object? options = null)
    {
        return _semanticKernelAiFunctions.Task(goal, options);
    }

    /// <summary>
    /// Synchronous wrapper for synthesize function
    /// </summary>
    public new object Synthesize(string specification, object? options = null)
    {
        return _semanticKernelAiFunctions.Synthesize(specification, options);
    }

    /// <summary>
    /// Synchronous wrapper for reason function
    /// </summary>
    public new object Reason(string question, object? options = null)
    {
        return _semanticKernelAiFunctions.Reason(question, options);
    }

    /// <summary>
    /// Synchronous wrapper for process function
    /// </summary>
    public new object Process(string input, string context, object? options = null)
    {
        return _semanticKernelAiFunctions.Process(input, context, options);
    }

    /// <summary>
    /// Synchronous wrapper for generate function
    /// </summary>
    public new object Generate(string prompt, object? options = null)
    {
        return _semanticKernelAiFunctions.Generate(prompt, options);
    }

    /// <summary>
    /// Synchronous wrapper for embed function
    /// </summary>
    public new object Embed(string text, object? options = null)
    {
        return _semanticKernelAiFunctions.Embed(text, options);
    }

    /// <summary>
    /// Synchronous wrapper for adapt function
    /// </summary>
    public new object Adapt(string content, object? options = null)
    {
        return _semanticKernelAiFunctions.Adapt(content, options);
    }
}
