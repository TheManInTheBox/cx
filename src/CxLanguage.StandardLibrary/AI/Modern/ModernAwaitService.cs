using CxLanguage.StandardLibrary.Core;
using CxLanguage.Core.Events;
using CxLanguage.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace CxLanguage.StandardLibrary.AI.Modern;

/// <summary>
/// Modern await service for CX standard library
/// Provides intelligent timing and delay capabilities using modern AI architecture
/// </summary>
public class ModernAwaitService : ModernAiServiceBase
{
    /// <summary>
    /// Initializes a new instance of the ModernAwaitService
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    public ModernAwaitService(IServiceProvider serviceProvider, ILogger<ModernAwaitService> logger) 
        : base(serviceProvider, logger)
    {
    }

    /// <summary>
    /// Gets the service name
    /// </summary>
    public override string ServiceName => "ModernAwait";
    
    /// <summary>
    /// Gets the service version
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Smart await with AI-determined optimal timing for natural interactions
    /// </summary>
    /// <param name="reason">Reason for waiting</param>
    /// <param name="context">Context information for the wait</param>
    /// <param name="minDurationMs">Minimum duration in milliseconds</param>
    /// <param name="maxDurationMs">Maximum duration in milliseconds</param>
    /// <returns>Await result with timing information</returns>
    [Description("Smart await with AI-determined optimal timing for natural interactions")]
    public async Task<AwaitResult> SmartAwaitAsync(
        [Description("Reason for waiting")] string reason,
        [Description("Context information for the wait")] string? context = null,
        [Description("Minimum duration in milliseconds")] int minDurationMs = 1000,
        [Description("Maximum duration in milliseconds")] int maxDurationMs = 3000)
    {
        var result = new AwaitResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("⏰ SMART AWAIT: Starting intelligent wait - Reason: {Reason}, Range: {Min}-{Max}ms", 
                reason, minDurationMs, maxDurationMs);
            
            result.Reason = reason;
            result.Context = context ?? "";
            result.MinDurationMs = minDurationMs;
            result.MaxDurationMs = maxDurationMs;
            result.StartTime = startTime;

            // ✅ AI-DETERMINED TIMING: Calculate optimal duration based on context
            var optimalDuration = CalculateOptimalDuration(reason, context, minDurationMs, maxDurationMs);
            result.RequestedDurationMs = optimalDuration;

            // Emit waiting started event
            var startEventData = new Dictionary<string, object>
            {
                ["reason"] = reason,
                ["context"] = context ?? "",
                ["minDurationMs"] = minDurationMs,
                ["maxDurationMs"] = maxDurationMs,
                ["optimalDurationMs"] = optimalDuration,
                ["startTime"] = startTime,
                ["source"] = "modern_await_service"
            };

            CxRuntimeHelper.EmitEvent("agent.waiting.started", startEventData);
            _logger.LogInformation("✅ Emitted agent.waiting.started event");

            // ✅ ACTUAL INTELLIGENT DELAY: Use calculated optimal timing
            await Task.Delay(optimalDuration);

            var endTime = DateTimeOffset.UtcNow;
            result.EndTime = endTime;
            result.ActualDurationMs = (int)(endTime - startTime).TotalMilliseconds;
            result.Success = true;
            result.Message = $"Smart await completed - Optimal duration: {optimalDuration}ms, Actual: {result.ActualDurationMs}ms";

            _logger.LogInformation("✅ SMART AWAIT: Completed - Optimal: {Optimal}ms, Actual: {Actual}ms", 
                optimalDuration, result.ActualDurationMs);

            // Emit waiting completed event
            var completeEventData = new Dictionary<string, object>
            {
                ["reason"] = reason,
                ["context"] = context ?? "",
                ["optimalDurationMs"] = optimalDuration,
                ["actualDurationMs"] = result.ActualDurationMs,
                ["success"] = true,
                ["endTime"] = endTime,
                ["source"] = "modern_await_service"
            };

            CxRuntimeHelper.EmitEvent("agent.waiting.completed", completeEventData);
            _logger.LogInformation("✅ Emitted agent.waiting.completed event");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SMART AWAIT: Error during intelligent wait");
            
            var endTime = DateTimeOffset.UtcNow;
            result.EndTime = endTime;
            result.ActualDurationMs = (int)(endTime - startTime).TotalMilliseconds;
            result.Success = false;
            result.Message = $"Smart await error: {ex.Message}";
            result.ErrorMessage = ex.Message;

            return result;
        }
    }

    /// <summary>
    /// Basic await/sleep for a specified duration
    /// </summary>
    /// <param name="reason">Reason for waiting</param>
    /// <param name="context">Context information for the wait</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <returns>Await result with timing information</returns>
    [Description("Basic await/sleep for a specified duration")]
    public async Task<AwaitResult> AwaitAsync(
        [Description("Reason for waiting")] string reason,
        [Description("Context information for the wait")] string? context = null,
        [Description("Duration in milliseconds")] int durationMs = 2000)
    {
        return await SmartAwaitAsync(reason, context, durationMs, durationMs);
    }

    /// <summary>
    /// Calculate optimal duration based on context using AI heuristics
    /// </summary>
    private int CalculateOptimalDuration(string reason, string? context, int minMs, int maxMs)
    {
        // ✅ INTELLIGENT TIMING LOGIC: AI-driven duration calculation
        var baseWeight = 0.5; // Start in the middle
        
        // Adjust based on reason keywords
        if (reason.Contains("pause") || reason.Contains("breath"))
            baseWeight += 0.2; // Longer for natural pauses
        else if (reason.Contains("quick") || reason.Contains("brief"))
            baseWeight -= 0.3; // Shorter for quick operations
        else if (reason.Contains("turn") || reason.Contains("speech"))
            baseWeight += 0.1; // Slightly longer for speech-related timing
        else if (reason.Contains("pre") || reason.Contains("preparation"))
            baseWeight -= 0.1; // Slightly shorter for preparation
        else if (reason.Contains("post") || reason.Contains("after"))
            baseWeight += 0.3; // Longer for post-operation pauses
        
        // Adjust based on context
        if (!string.IsNullOrEmpty(context))
        {
            if (context.Contains("debate") || context.Contains("conversation"))
                baseWeight += 0.2; // Longer for conversational contexts
            else if (context.Contains("fast") || context.Contains("quick"))
                baseWeight -= 0.2; // Shorter for fast contexts
        }
        
        // Ensure weight stays within bounds
        baseWeight = Math.Max(0.0, Math.Min(1.0, baseWeight));
        
        // Calculate optimal duration within the specified range
        var range = maxMs - minMs;
        var optimal = minMs + (int)(range * baseWeight);
        
        return optimal;
    }
}

/// <summary>
/// Result from await operations
/// </summary>
public class AwaitResult : CxAiResult
{
    /// <summary>
    /// Reason for the wait operation
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Context information for the wait
    /// </summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Minimum duration in milliseconds
    /// </summary>
    public int MinDurationMs { get; set; }

    /// <summary>
    /// Maximum duration in milliseconds
    /// </summary>
    public int MaxDurationMs { get; set; }

    /// <summary>
    /// Requested/optimal duration in milliseconds
    /// </summary>
    public int RequestedDurationMs { get; set; }

    /// <summary>
    /// Actual duration in milliseconds
    /// </summary>
    public int ActualDurationMs { get; set; }

    /// <summary>
    /// Start time of the operation
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// End time of the operation
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Result message or error description
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

