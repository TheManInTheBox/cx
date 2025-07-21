using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CxLanguage.StandardLibrary.Events;
using CxLanguage.Azure.Services;
using System;
using System.Threading.Tasks;
using System.Text.Json;

namespace CxLanguage.StandardLibrary.Services;

/// <summary>
/// Phase 8.3 Real-Time Speech Service - Complete Pipeline Integration
/// Connects NAudio hardware → Azure OpenAI Realtime API → live.audio events
/// This service provides the complete speech-to-text-to-response pipeline for CX Language.
/// </summary>
public class RealtimeSpeechService : IDisposable
{
    private readonly ILogger<RealtimeSpeechService> _logger;
    private readonly LiveAudioBridge _audioBridge;
    private readonly AzureOpenAIRealtimeService _realtimeService;
    private readonly ICxEventBus _eventBus;
    
    private bool _isActive = false;
    private bool _disposed = false;
    private double _speechDetectionThreshold = 0.02; // Audio level threshold for speech detection
    private TimeSpan _silenceTimeout = TimeSpan.FromSeconds(2); // How long to wait after silence before processing
    private DateTime _lastAudioActivity = DateTime.MinValue;
    private bool _isSpeaking = false;
    
    public RealtimeSpeechService(
        ILogger<RealtimeSpeechService> logger,
        IConfiguration configuration,
        ICxEventBus eventBus)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        
        // Initialize audio bridge for hardware capture
        _audioBridge = new LiveAudioBridge(
            CreateLoggerFor<LiveAudioBridge>(),
            configuration);
            
        // Initialize Azure OpenAI Realtime service
        _realtimeService = new AzureOpenAIRealtimeService(
            CreateLoggerFor<AzureOpenAIRealtimeService>(),
            configuration,
            eventBus);
        
        SetupEventPipeline();
        
        _logger.LogInformation("🎤→🧠→🔊 Real-Time Speech Service initialized - Complete pipeline ready");
    }
    
    /// <summary>
    /// Start the complete real-time speech processing pipeline
    /// </summary>
    public async Task StartAsync()
    {
        if (_isActive)
        {
            _logger.LogWarning("Real-time speech service already active");
            return;
        }
        
        try
        {
            _logger.LogInformation("🚀 Starting Phase 8.3 real-time speech pipeline...");
            
            // Start Azure OpenAI Realtime session
            var sessionStarted = await _realtimeService.StartSessionAsync();
            if (!sessionStarted)
            {
                throw new InvalidOperationException("Failed to start Azure OpenAI Realtime session");
            }
            
            // Start hardware audio capture
            await _audioBridge.StartAsync();
            
            _isActive = true;
            _logger.LogInformation("✅ Real-time speech pipeline active - Ready for voice commands");
            
            // Emit system ready event
            await _eventBus.EmitAsync("system.speech.ready", new
            {
                pipeline = "hardware→openai→events",
                capabilities = new[] { "speech-to-text", "ai-response", "text-to-speech" },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start real-time speech pipeline");
            await StopAsync();
            throw;
        }
    }
    
    /// <summary>
    /// Stop the real-time speech processing pipeline
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isActive)
            return;
            
        try
        {
            _logger.LogInformation("🛑 Stopping real-time speech pipeline...");
            
            _isActive = false;
            
            // Stop hardware capture
            await _audioBridge.StopAsync();
            
            // Stop Azure OpenAI session
            await _realtimeService.StopSessionAsync();
            
            _logger.LogInformation("✅ Real-time speech pipeline stopped");
            
            // Emit system stopped event
            await _eventBus.EmitAsync("system.speech.stopped", new
            {
                reason = "manual_stop",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping real-time speech pipeline");
        }
    }
    
    /// <summary>
    /// Set up the complete event processing pipeline
    /// </summary>
    private void SetupEventPipeline()
    {
        // Handle raw audio from hardware
        _audioBridge.LiveAudioCaptured += OnHardwareAudioCaptured;
        _audioBridge.ErrorOccurred += OnAudioError;
        
        // Handle Azure OpenAI responses
        _eventBus.Subscribe("realtime.response.completed", OnOpenAIResponse);
        _eventBus.Subscribe("realtime.speech.completed", OnTranscriptReceived);
        _eventBus.Subscribe("realtime.audio.started", OnAudioResponseReceived);
        _eventBus.Subscribe("realtime.error", OnOpenAIError);
    }
    
    /// <summary>
    /// Process raw audio from hardware and detect speech
    /// </summary>
    private async void OnHardwareAudioCaptured(object? sender, Phase8LiveAudioEventArgs e)
    {
        if (!_isActive)
            return;
            
        try
        {
            // Calculate audio level for speech detection
            double audioLevel = CalculateAudioLevel(e.AudioData);
            
            if (audioLevel > _speechDetectionThreshold)
            {
                _lastAudioActivity = DateTime.UtcNow;
                
                if (!_isSpeaking)
                {
                    _isSpeaking = true;
                    _logger.LogDebug("🎤 Speech detected - Starting capture");
                    
                    await _eventBus.EmitAsync("system.speech.started", new
                    {
                        audioLevel = audioLevel,
                        timestamp = DateTime.UtcNow
                    });
                }
                
                // Send audio to Azure OpenAI Realtime API
                await _realtimeService.SendAudioAsync(e.AudioData);
            }
            else if (_isSpeaking && DateTime.UtcNow - _lastAudioActivity > _silenceTimeout)
            {
                _isSpeaking = false;
                _logger.LogDebug("🤫 Silence detected - Processing speech");
                
                // Trigger speech processing
                await _realtimeService.CommitAudioAsync();
                
                await _eventBus.EmitAsync("system.speech.ended", new
                {
                    silenceDuration = DateTime.UtcNow - _lastAudioActivity,
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing hardware audio");
            await _eventBus.EmitAsync("system.speech.error", new
            {
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
    
    /// <summary>
    /// Handle transcript from Azure OpenAI
    /// </summary>
    private async Task OnTranscriptReceived(object eventData)
    {
        try
        {
            var transcriptData = JsonSerializer.Deserialize<JsonElement>(eventData.ToString() ?? "{}");
            
            if (transcriptData.TryGetProperty("transcript", out var transcript))
            {
                var transcriptText = transcript.GetString() ?? "";
                _logger.LogInformation($"🎤→📝 Speech recognized: {transcriptText}");
                
                // Emit the standard live.audio event that existing agents expect
                await _eventBus.EmitAsync("live.audio", new
                {
                    transcript = transcriptText,
                    confidence = transcriptData.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.95,
                    source = "azure_openai_realtime",
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transcript");
        }
    }
    
    /// <summary>
    /// Handle AI response from Azure OpenAI
    /// </summary>
    private async Task OnOpenAIResponse(object eventData)
    {
        try
        {
            var responseData = JsonSerializer.Deserialize<JsonElement>(eventData.ToString() ?? "{}");
            
            if (responseData.TryGetProperty("text", out var content))
            {
                var responseText = content.GetString() ?? "";
                _logger.LogInformation($"🧠 AI Response: {responseText}");
                
                // Emit AI response event
                await _eventBus.EmitAsync("ai.response.generated", new
                {
                    response = responseText,
                    source = "azure_openai_realtime",
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI response");
        }
    }
    
    /// <summary>
    /// Handle audio response from Azure OpenAI (TTS)
    /// </summary>
    private async Task OnAudioResponseReceived(object eventData)
    {
        try
        {
            _logger.LogInformation("🔊 Audio response received from Azure OpenAI");
            
            // Emit audio response event
            await _eventBus.EmitAsync("ai.audio.response", new
            {
                audioData = eventData,
                source = "azure_openai_realtime",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio response");
        }
    }
    
    /// <summary>
    /// Handle audio errors
    /// </summary>
    private async void OnAudioError(object? sender, Phase8ErrorEventArgs e)
    {
        _logger.LogError(e.Exception, "Hardware audio error");
        
        await _eventBus.EmitAsync("system.audio.error", new
        {
            error = e.Exception.Message,
            source = "hardware_audio",
            timestamp = DateTime.UtcNow
        });
    }
    
    /// <summary>
    /// Handle OpenAI errors
    /// </summary>
    private async Task OnOpenAIError(object eventData)
    {
        _logger.LogError($"Azure OpenAI error: {eventData}");
        
        await _eventBus.EmitAsync("system.openai.error", new
        {
            error = eventData.ToString(),
            source = "azure_openai_realtime",
            timestamp = DateTime.UtcNow
        });
    }
    
    /// <summary>
    /// Calculate audio level for speech detection
    /// </summary>
    private double CalculateAudioLevel(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
            return 0.0;
            
        double sum = 0.0;
        for (int i = 0; i < audioData.Length; i += 2)
        {
            if (i + 1 < audioData.Length)
            {
                // Convert bytes to 16-bit sample
                short sample = (short)(audioData[i] | (audioData[i + 1] << 8));
                sum += Math.Abs(sample);
            }
        }
        
        return sum / (audioData.Length / 2) / 32768.0; // Normalize to 0-1 range
    }
    
    /// <summary>
    /// Create logger for dependency injection
    /// </summary>
    private ILogger<T> CreateLoggerFor<T>()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<T>();
    }
    
    /// <summary>
    /// Update speech detection settings
    /// </summary>
    public void ConfigureSpeechDetection(double threshold = 0.02, TimeSpan? silenceTimeout = null)
    {
        _speechDetectionThreshold = threshold;
        _silenceTimeout = silenceTimeout ?? TimeSpan.FromSeconds(2);
        
        _logger.LogInformation($"🎛️ Speech detection configured - Threshold: {threshold:F3}, Silence timeout: {_silenceTimeout.TotalSeconds}s");
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
            
        _disposed = true;
        
        StopAsync().Wait();
        _audioBridge?.Dispose();
        
        _logger.LogInformation("Real-time speech service disposed");
    }
}
