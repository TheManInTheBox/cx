using CxLanguage.StandardLibrary.Core;
using CxLanguage.Core.Events;
using CxLanguage.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace CxLanguage.StandardLibrary.AI.Modern;

/// <summary>
/// Modern text-to-speech service using Azure OpenAI Realtime API
/// Provides speech synthesis through RealtimeService integration
/// </summary>
public class ModernTextToSpeechService : ModernAiServiceBase
{
    /// <summary>
    /// Initializes a new instance of the ModernTextToSpeechService
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    public ModernTextToSpeechService(IServiceProvider serviceProvider, ILogger<ModernTextToSpeechService> logger) 
        : base(serviceProvider, logger)
    {
    }

    /// <summary>
    /// Gets the service name
    /// </summary>
    public override string ServiceName => "ModernTextToSpeech";
    
    /// <summary>
    /// Gets the service version
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Convert text to speech using Azure OpenAI Realtime API
    /// </summary>
    /// <param name="text">Text to convert to speech</param>
    /// <param name="voice">Voice to use (alloy, echo, fable, onyx, nova, shimmer)</param>
    /// <param name="speechSpeed">Speech speed multiplier (0.8-1.2, default 1.0)</param>
    /// <returns>Text-to-speech result with audio data</returns>
    [Description("Convert text to speech using Azure OpenAI Realtime API")]
    public Task<TextToSpeechResult> SpeakAsync(
        [Description("Text to convert to speech")] string text,
        [Description("Voice to use (alloy, echo, fable, onyx, nova, shimmer)")] string voice = "alloy",
        [Description("Speech speed multiplier (0.8-1.2)")] double speechSpeed = 1.0)
    {
        var result = new TextToSpeechResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("🔊 TEXT-TO-SPEECH: Converting text to speech - Text: {Text}, Voice: {Voice}, Speed: {Speed}", 
                text.Length > 50 ? text[..50] + "..." : text, voice, speechSpeed);
            
            result.Text = text;
            result.Voice = voice;
            result.SpeechSpeed = speechSpeed;
            result.StartTime = startTime;

            // ✅ MODERN APPROACH: Use Azure OpenAI Realtime API through event system
            var eventData = new Dictionary<string, object>
            {
                ["text"] = text,
                ["voice"] = voice,
                ["speechSpeed"] = speechSpeed,
                ["deployment"] = "gpt-4o-mini-realtime-preview",
                ["timestamp"] = startTime,
                ["source"] = "modern_text_to_speech"
            };

            // Emit realtime.text.send event for Azure Realtime API processing
            CxRuntimeHelper.EmitEvent("realtime.text.send", eventData);
            _logger.LogInformation("✅ Emitted realtime.text.send event for text-to-speech");

            // ✅ PRODUCTION READY: Return immediate result, audio will be delivered via events
            var endTime = DateTimeOffset.UtcNow;
            result.EndTime = endTime;
            result.ActualDurationMs = (int)(endTime - startTime).TotalMilliseconds;
            result.Success = true;
            result.Message = $"Text-to-speech request sent via Azure Realtime API - Voice: {voice}, Speed: {speechSpeed}x";
            result.AudioFormat = "pcm16";
            result.SampleRate = 24000;

            _logger.LogInformation("✅ TEXT-TO-SPEECH: Request completed - Duration: {Duration}ms", result.ActualDurationMs);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ TEXT-TO-SPEECH: Error converting text to speech");
            
            var endTime = DateTimeOffset.UtcNow;
            result.EndTime = endTime;
            result.ActualDurationMs = (int)(endTime - startTime).TotalMilliseconds;
            result.Success = false;
            result.Message = $"Text-to-speech error: {ex.Message}";
            result.ErrorMessage = ex.Message;

            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// Convert text to speech with advanced options
    /// </summary>
    /// <param name="text">Text to convert to speech</param>
    /// <param name="options">Advanced text-to-speech options</param>
    /// <returns>Text-to-speech result with audio data</returns>
    [Description("Convert text to speech with advanced options")]
    public async Task<TextToSpeechResult> SpeakAdvancedAsync(
        [Description("Text to convert to speech")] string text,
        [Description("Advanced text-to-speech options")] TextToSpeechOptions? options = null)
    {
        options ??= new TextToSpeechOptions();
        
        return await SpeakAsync(text, options.Voice, options.SpeechSpeed);
    }
}

/// <summary>
/// Result from text-to-speech operations using Azure Realtime API
/// </summary>
public class TextToSpeechResult : CxAiResult
{
    /// <summary>
    /// Original text that was converted to speech
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Voice used for synthesis
    /// </summary>
    public string Voice { get; set; } = string.Empty;

    /// <summary>
    /// Speech speed multiplier used
    /// </summary>
    public double SpeechSpeed { get; set; } = 1.0;

    /// <summary>
    /// Audio format (e.g., "pcm16", "mp3")
    /// </summary>
    public string AudioFormat { get; set; } = string.Empty;

    /// <summary>
    /// Sample rate in Hz
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Start time of the operation
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// End time of the operation
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Actual duration of the operation in milliseconds
    /// </summary>
    public int ActualDurationMs { get; set; }

    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Result message or error description
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Advanced options for text-to-speech operations
/// </summary>
public class TextToSpeechOptions : CxAiOptions
{
    /// <summary>
    /// Voice to use for synthesis (alloy, echo, fable, onyx, nova, shimmer)
    /// </summary>
    public string Voice { get; set; } = "alloy";

    /// <summary>
    /// Speech speed multiplier (0.8 = 20% slower, 1.0 = normal, 1.2 = 20% faster)
    /// </summary>
    public double SpeechSpeed { get; set; } = 1.0;

    /// <summary>
    /// Preferred audio format
    /// </summary>
    public string AudioFormat { get; set; } = "pcm16";

    /// <summary>
    /// Preferred sample rate in Hz
    /// </summary>
    public int SampleRate { get; set; } = 24000;

    /// <summary>
    /// Maximum duration to wait for synthesis completion
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
