using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CxLanguage.Runtime;
using CxLanguage.StandardLibrary.Services;
using System;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.EventBridges;

/// <summary>
/// Event bridge connecting CX events to VoiceOutputService for audio playback
/// Handles voice.output.* events and coordinates with hardware audio output
/// </summary>
public class VoiceOutputEventBridge
{
    private readonly ILogger<VoiceOutputEventBridge> _logger;
    private readonly IVoiceOutputService _voiceOutputService;
    private readonly ICxEventBus _eventBus;

    public VoiceOutputEventBridge(
        ILogger<VoiceOutputEventBridge> logger,
        IVoiceOutputService voiceOutputService,
        ICxEventBus eventBus)
    {
        _logger = logger;
        _voiceOutputService = voiceOutputService;
        _eventBus = eventBus;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🔗 Registering Voice Output Event Bridge handlers");

            // Subscribe to voice output events
            _eventBus.Subscribe("voice.output.play", OnVoiceOutputPlay);
            _eventBus.Subscribe("voice.output.play.file", OnVoiceOutputPlayFile);
            _eventBus.Subscribe("voice.output.stop", OnVoiceOutputStop);
            _eventBus.Subscribe("voice.output.device.set", OnVoiceOutputDeviceSet);

            _logger.LogInformation("✅ Voice Output Event Bridge handlers registered");
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing Voice Output Event Bridge");
        }
    }

    private void OnVoiceOutputPlay(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("🔊 Voice Output Play event received - starting audio playback");

            // Extract audio data from event
            var audioData = GetPropertyValue<byte[]>(cxEvent, "audioData");
            var sampleRate = GetPropertyValue<int>(cxEvent, "sampleRate", 24000);
            var channels = GetPropertyValue<int>(cxEvent, "channels", 1);

            if (audioData != null && audioData.Length > 0)
            {
                _ = Task.Run(async () => await _voiceOutputService.PlayAudioAsync(audioData, sampleRate, channels));
                _logger.LogInformation("✅ Audio playback initiated successfully");
            }
            else
            {
                _logger.LogWarning("⚠️ No audio data provided for playback");
                _ = Task.Run(async () => await _eventBus.EmitAsync("voice.output.error", new { error = "No audio data provided" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling voice output play event");
            _ = Task.Run(async () => await _eventBus.EmitAsync("voice.output.error", new { error = ex.Message }));
        }
    }

    private void OnVoiceOutputPlayFile(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("🔊 Voice Output Play File event received");

            var filePath = GetPropertyValue<string>(cxEvent, "filePath");

            if (!string.IsNullOrEmpty(filePath))
            {
                _ = Task.Run(async () => await _voiceOutputService.PlayAudioFileAsync(filePath));
                _logger.LogInformation("✅ Audio file playback initiated: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("⚠️ No file path provided for audio playback");
                _ = Task.Run(async () => await _eventBus.EmitAsync("voice.output.error", new { error = "No file path provided" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling voice output play file event");
            _ = Task.Run(async () => await _eventBus.EmitAsync("voice.output.error", new { error = ex.Message }));
        }
    }

    private void OnVoiceOutputStop(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("🔇 Voice Output Stop event received - stopping audio playback");

            _ = Task.Run(async () => await _voiceOutputService.StopPlaybackAsync());
            _logger.LogInformation("✅ Audio playback stopped successfully via event bridge");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling voice output stop event");
            _ = Task.Run(async () => await _eventBus.EmitAsync("voice.output.error", new { error = ex.Message }));
        }
    }

    private void OnVoiceOutputDeviceSet(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("🔊 Voice Output Device Set event received");

            var deviceIndex = GetPropertyValue<int>(cxEvent, "deviceIndex", 0);

            _ = Task.Run(async () => await _voiceOutputService.SetOutputDeviceAsync(deviceIndex));
            _logger.LogInformation("✅ Voice output device set to index: {DeviceIndex}", deviceIndex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling voice output device set event");
            _ = Task.Run(async () => await _eventBus.EmitAsync("voice.output.error", new { error = ex.Message }));
        }
    }

    private T? GetPropertyValue<T>(CxEvent cxEvent, string propertyName, T? defaultValue = default)
    {
        try
        {
            if (cxEvent.payload != null && cxEvent.payload is System.Collections.Generic.Dictionary<string, object> dict)
            {
                if (dict.TryGetValue(propertyName, out var value))
                {
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    
                    // Try to convert the value
                    if (value != null)
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }

            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting property {PropertyName} from event, using default value", propertyName);
            return defaultValue;
        }
    }
}
