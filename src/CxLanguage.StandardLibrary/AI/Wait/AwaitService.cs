using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using CxLanguage.Runtime;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Azure.AI.OpenAI;

namespace CxLanguage.StandardLibrary.AI.Wait;

/// <summary>
/// Await service for CX standard library
/// Provides thread sleep/delay capabilities for proper waiting behavior
/// </summary>
public class AwaitService : ModernAiServiceBase
{
    /// <summary>
    /// Initializes a new instance of the AwaitService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    public AwaitService(IServiceProvider serviceProvider, ILogger<AwaitService> logger) 
        : base(serviceProvider, logger)
    {
        _logger.LogInformation("✅ Modern AwaitService initialized with Microsoft.Extensions.AI architecture");
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
            await Hub.EmitAsync("agent.waiting.started", new { 
                reason = reason, 
                context = context, 
                durationMs = durationMs,
                startTime = startTime
            });

            // Actual thread sleep/delay
            await Task.Delay(durationMs);

            var endTime = DateTimeOffset.UtcNow;
            result.EndTime = endTime;
            result.ActualDurationMs = (int)(endTime - startTime).TotalMilliseconds;
            result.Success = true;
            result.Message = $"Successfully waited {result.ActualDurationMs}ms for: {reason}";

            _logger.LogInformation("✅ AWAIT: Completed wait - Actual duration: {ActualDuration}ms", result.ActualDurationMs);

            // Emit waiting completed event to re-enable audio streaming
            await Hub.EmitAsync("agent.waiting.completed", result);

            // Emit completion event via event bus
            await Hub.EmitAsync("await.completed", result);
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
            await Hub.EmitAsync("agent.waiting.failed", result);

            // Emit error event via event bus
            await Hub.EmitAsync("await.error", result);
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

            // Use AI to determine optimal wait time with memory context
            var aiPrompt = $"Given the reason '{reason}' and context '{context}', " +
                          $"determine an optimal wait duration between {minDurationMs} and {maxDurationMs} milliseconds. " +
                          "Respond with just a number representing milliseconds.";

            string aiDurationText = "";
            if (_chatClient != null)
            {
                try
                {
                    var messages = new List<ChatMessage> { new SystemChatMessage(aiPrompt) };
                    var response = await _chatClient.CompleteChatAsync(messages);
                    var responseValue = response.Value;
                    if (responseValue != null && responseValue.Choices.Count > 0)
                    {
                        aiDurationText = responseValue.Choices[0].Message.Content;
                    }
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
                _logger.LogWarning("🤖 ChatClient not available, using fallback duration");
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
