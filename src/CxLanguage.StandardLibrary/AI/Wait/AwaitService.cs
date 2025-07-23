using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.KernelMemory;
using System.ComponentModel;

namespace CxLanguage.StandardLibrary.AI.Wait;

/// <summary>
/// Await service for CX standard library
/// Provides thread sleep/delay capabilities for proper waiting behavior
/// </summary>
public class AwaitService : AiServiceBase
{
    private readonly IKernelMemory? _memory;

    /// <summary>
    /// Initializes a new instance of the AwaitService
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    public AwaitService(IServiceProvider serviceProvider, ILogger<AwaitService> logger) 
        : base(serviceProvider, logger)
    {
        // Try to get Kernel Memory service for vector database operations
        _memory = serviceProvider.GetService<IKernelMemory>();
        
        if (_memory != null)
        {
            _logger.LogInformation("✅ AwaitService initialized with Kernel Memory support");
        }
        else
        {
            _logger.LogWarning("⚠️ AwaitService initialized without Kernel Memory - vector learning disabled");
        }
    }

    /// <summary>
    /// Gets the service name
    /// </summary>
    public string ServiceName => "Await";
    
    /// <summary>
    /// Gets the service version
    /// </summary>
    public string Version => "1.0.0";

    /// <summary>
    /// Await/sleep for a specified duration with optional AI decision making
    /// </summary>
    /// <param name="reason">Reason for waiting</param>
    /// <param name="context">Context information for the wait</param>
    /// <param name="durationMs">Duration in milliseconds (default: 2000ms)</param>
    /// <returns>Await result with timing information</returns>
    [KernelFunction]
    [Description("Await/sleep for a specified duration with optional AI decision making")]
    public async Task<AwaitResult> AwaitAsync(
        [Description("Reason for waiting")] string reason,
        [Description("Context information for the wait")] string? context = null,
        [Description("Duration in milliseconds")] int durationMs = 7000)
    {
        var result = new AwaitResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("⏰ AWAIT: Starting wait - Reason: {Reason}, Duration: {Duration}ms", reason, durationMs);
            
            result.Reason = reason;
            result.Context = context ?? "";
            result.RequestedDurationMs = durationMs;
            result.StartTime = startTime;

            // Emit waiting started event to prevent audio streaming
            var eventBus = GetEventBus();
            if (eventBus != null)
            {
                await eventBus.EmitAsync("agent.waiting.started", new { 
                    reason = reason, 
                    context = context, 
                    durationMs = durationMs,
                    startTime = startTime
                });
            }

            // Actual thread sleep/delay
            await Task.Delay(durationMs);

            var endTime = DateTimeOffset.UtcNow;
            result.EndTime = endTime;
            result.ActualDurationMs = (int)(endTime - startTime).TotalMilliseconds;
            result.Success = true;
            result.Message = $"Successfully waited {result.ActualDurationMs}ms for: {reason}";

            _logger.LogInformation("✅ AWAIT: Completed wait - Actual duration: {ActualDuration}ms", result.ActualDurationMs);

            // Emit waiting completed event to re-enable audio streaming
            if (eventBus != null)
            {
                await eventBus.EmitAsync("agent.waiting.completed", result);
            }

            // Emit completion event via event bus
            if (eventBus != null)
            {
                await eventBus.EmitAsync("await.completed", result);
            }
        }
        catch (Exception ex)
        {
            var endTime = DateTimeOffset.UtcNow;
            result.EndTime = endTime;
            result.ActualDurationMs = (int)(endTime - startTime).TotalMilliseconds;
            result.Success = false;
            result.Error = ex.Message;
            result.Message = $"Await failed after {result.ActualDurationMs}ms: {ex.Message}";

            _logger.LogError(ex, "❌ AWAIT: Wait failed - {Error}", ex.Message);

            // Emit waiting failed event to re-enable audio streaming
            var eventBus = GetEventBus();
            if (eventBus != null)
            {
                await eventBus.EmitAsync("agent.waiting.failed", result);
            }

            // Emit error event via event bus
            if (eventBus != null)
            {
                await eventBus.EmitAsync("await.error", result);
            }
        }

        return result;
    }

    /// <summary>
    /// Smart await that uses AI to determine optimal wait duration
    /// </summary>
    /// <param name="reason">Reason for waiting</param>
    /// <param name="context">Context information for the wait</param>
    /// <param name="minDurationMs">Minimum duration in milliseconds</param>
    /// <param name="maxDurationMs">Maximum duration in milliseconds</param>
    /// <returns>Await result with AI-determined timing</returns>
    [KernelFunction]
    [Description("Smart await that uses AI to determine optimal wait duration")]
    public async Task<AwaitResult> SmartAwaitAsync(
        [Description("Reason for waiting")] string reason,
        [Description("Context information for the wait")] string? context = null,
        [Description("Minimum duration in milliseconds")] int minDurationMs = 1000,
        [Description("Maximum duration in milliseconds")] int maxDurationMs = 5000)
    {
        var result = new AwaitResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("🧠 SMART AWAIT: Determining optimal wait duration - Reason: {Reason}", reason);

            // First, query vector memory for similar wait patterns if available
            string contextualInfo = "";
            if (_memory != null)
            {
                try
                {
                    _logger.LogInformation("🔍 Querying vector memory for similar wait patterns");
                    var memoryQuery = $"await wait duration pattern reason: {reason} context: {context}";
                    var searchResult = await _memory.SearchAsync(memoryQuery, limit: 3, minRelevance: 0.7);
                    
                    if (searchResult.Results.Any())
                    {
                        var patterns = searchResult.Results.Select(r => 
                            r.Partitions.FirstOrDefault()?.Text ?? "").Where(t => !string.IsNullOrEmpty(t));
                        contextualInfo = string.Join("; ", patterns);
                        _logger.LogInformation("✅ Found {Count} similar wait patterns in memory", searchResult.Results.Count());
                    }
                    else
                    {
                        _logger.LogInformation("ℹ️ No similar wait patterns found in memory");
                    }
                }
                catch (Exception memEx)
                {
                    _logger.LogWarning(memEx, "⚠️ Failed to query vector memory for wait patterns");
                }
            }

            // Use AI to determine optimal wait time with memory context
            var aiPrompt = $"Given the reason '{reason}' and context '{context}', " +
                          $"determine an optimal wait duration between {minDurationMs} and {maxDurationMs} milliseconds. ";
            
            if (!string.IsNullOrEmpty(contextualInfo))
            {
                aiPrompt += $"Consider these similar patterns from memory: {contextualInfo}. ";
            }
            
            aiPrompt += "Respond with just a number representing milliseconds.";

            // Use Kernel Memory AskAsync for AI request
            string aiDurationText = "";
            if (_memory != null)
            {
                try
                {
                    var aiResponse = await _memory.AskAsync(aiPrompt);
                    aiDurationText = aiResponse.Result?.Trim() ?? "";
                    _logger.LogInformation("🤖 AI response: {Response}", aiDurationText);
                }
                catch (Exception aiEx)
                {
                    _logger.LogWarning(aiEx, "🤖 AI request failed, using fallback duration");
                    var fallbackDuration = (minDurationMs + maxDurationMs) / 2;
                    return await AwaitAsync(reason, context, fallbackDuration);
                }
            }
            else
            {
                _logger.LogWarning("🤖 Kernel Memory not available, using fallback duration");
                var fallbackDuration = (minDurationMs + maxDurationMs) / 2;
                return await AwaitAsync(reason, context, fallbackDuration);
            }
            
            // Parse AI response to get duration
            if (!int.TryParse(aiDurationText, out var aiDuration))
            {
                // Fallback to average if AI response is invalid
                aiDuration = (minDurationMs + maxDurationMs) / 2;
                _logger.LogWarning("🤖 AI response '{Response}' invalid, using fallback: {Duration}ms", aiDurationText, aiDuration);
            }

            // Ensure within bounds
            var finalDuration = Math.Max(minDurationMs, Math.Min(maxDurationMs, aiDuration));

            _logger.LogInformation("🎯 SMART AWAIT: AI determined duration: {Duration}ms", finalDuration);

            // Store this wait pattern in vector memory for future learning
            if (_memory != null)
            {
                try
                {
                    var waitPattern = $"Await pattern: reason='{reason}', context='{context}', " +
                                    $"duration={finalDuration}ms, range={minDurationMs}-{maxDurationMs}ms, " +
                                    $"timestamp={DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
                    
                    var documentId = $"await_pattern_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
                    await _memory.ImportTextAsync(waitPattern, documentId);
                    _logger.LogInformation("📝 Stored wait pattern in vector memory: {DocumentId}", documentId);
                }
                catch (Exception storeEx)
                {
                    _logger.LogWarning(storeEx, "⚠️ Failed to store wait pattern in vector memory");
                }
            }

            // Perform the actual await
            return await AwaitAsync(reason, context, finalDuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SMART AWAIT: Failed to determine duration, using default");
            
            // Fallback to regular await with average duration
            var fallbackDuration = (minDurationMs + maxDurationMs) / 2;
            return await AwaitAsync(reason, context, fallbackDuration);
        }
    }
}

/// <summary>
/// Result of an await operation
/// </summary>
public class AwaitResult
{
    public string Reason { get; set; } = "";
    public string Context { get; set; } = "";
    public int RequestedDurationMs { get; set; }
    public int ActualDurationMs { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? Error { get; set; }
}
