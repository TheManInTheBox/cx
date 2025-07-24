using CxLanguage.Runtime;
using CxLanguage.StandardLibrary.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CxLanguage.StandardLibrary.EventBridges;

/// <summary>
/// Voice Input Event Bridge - Connects CX events to VoiceInputService
/// Bridges the gap between CX event system and actual voice hardware services
/// </summary>
public class VoiceInputEventBridge
{
    private readonly ILogger<VoiceInputEventBridge> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICxEventBus _eventBus;
    private IVoiceInputService? _voiceService;

    public VoiceInputEventBridge(
        ILogger<VoiceInputEventBridge> logger,
        IServiceProvider serviceProvider,
        ICxEventBus eventBus)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _eventBus = eventBus;
        
        // Subscribe to voice input events
        RegisterEventHandlers();
    }

    private void RegisterEventHandlers()
    {
        _logger.LogInformation("🔗 Registering Voice Input Event Bridge handlers");
        
        // Subscribe to voice input control events
        _eventBus.Subscribe("voice.input.start", OnVoiceInputStart);
        _eventBus.Subscribe("voice.input.stop", OnVoiceInputStop);
        _eventBus.Subscribe("voice.input.device.set", OnVoiceInputDeviceSet);
        
        _logger.LogInformation("✅ Voice Input Event Bridge handlers registered");
    }

    private void OnVoiceInputStart(object eventData)
    {
        // Fire-and-forget async operation for voice input start
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("🎤 Voice Input Start event received - activating microphone");
                
                // Get or create voice service
                _voiceService ??= _serviceProvider.GetRequiredService<IVoiceInputService>();
                
                // Start voice input capture
                await _voiceService.StartListeningAsync();
                
                _logger.LogInformation("✅ Voice input capture started successfully via event bridge");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error starting voice input via event bridge");
                
                // Emit error event
                await _eventBus.EmitAsync("voice.input.error", new
                {
                    error = "start_failed",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        });
    }

    private void OnVoiceInputStop(object eventData)
    {
        // Fire-and-forget async operation for voice input stop
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("🔇 Voice Input Stop event received - stopping microphone");
                
                if (_voiceService != null)
                {
                    await _voiceService.StopListeningAsync();
                    _logger.LogInformation("✅ Voice input capture stopped successfully via event bridge");
                }
                else
                {
                    _logger.LogWarning("⚠️ Voice service not available - cannot stop");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error stopping voice input via event bridge");
                
                // Emit error event
                await _eventBus.EmitAsync("voice.input.error", new
                {
                    error = "stop_failed",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        });
    }

    private void OnVoiceInputDeviceSet(object eventData)
    {
        // Fire-and-forget async operation for device change
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("🎤 Voice Input Device Set event received");
                
                // Parse device index from event data
                if (eventData is Dictionary<string, object> data && 
                    data.TryGetValue("deviceIndex", out var deviceIndexObj) &&
                    int.TryParse(deviceIndexObj.ToString(), out var deviceIndex))
                {
                    _voiceService ??= _serviceProvider.GetRequiredService<IVoiceInputService>();
                    await _voiceService.SetInputDeviceAsync(deviceIndex);
                    
                    _logger.LogInformation($"✅ Voice input device set to index {deviceIndex} via event bridge");
                }
                else
                {
                    _logger.LogWarning("⚠️ Invalid device index in voice.input.device.set event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting voice input device via event bridge");
                
                // Emit error event
                await _eventBus.EmitAsync("voice.input.error", new
                {
                    error = "device_set_failed",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        });
    }
}
