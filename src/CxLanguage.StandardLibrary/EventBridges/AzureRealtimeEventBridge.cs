using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CxLanguage.Runtime;
using CxLanguage.StandardLibrary.Services;
using System;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.EventBridges;

/// <summary>
/// Event bridge connecting CX events to Azure OpenAI Realtime API service
/// Handles realtime.* events and coordinates with Azure OpenAI WebSocket API
/// </summary>
public class AzureRealtimeEventBridge
{
    private readonly ILogger<AzureRealtimeEventBridge> _logger;
    private readonly ICxEventBus _eventBus;
    private readonly IServiceProvider _serviceProvider;

    public AzureRealtimeEventBridge(
        ILogger<AzureRealtimeEventBridge> logger,
        ICxEventBus eventBus,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _eventBus = eventBus;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🔗 Registering Azure Realtime Event Bridge handlers");

            // Subscribe to Azure Realtime API events
            _eventBus.Subscribe("realtime.connect", OnRealtimeConnect);
            _eventBus.Subscribe("realtime.session.create", OnRealtimeSessionCreate);
            _eventBus.Subscribe("realtime.text.send", OnRealtimeTextSend);
            _eventBus.Subscribe("realtime.audio.send", OnRealtimeAudioSend);

            _logger.LogInformation("✅ Azure Realtime Event Bridge handlers registered");
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing Azure Realtime Event Bridge");
        }
    }

    private void OnRealtimeConnect(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("🔗 Azure Realtime Connect event received");

            var demo = GetPropertyValue<string>(cxEvent, "demo", "default");
            
            _logger.LogInformation("Connecting to Azure OpenAI Realtime API for demo: {Demo}", demo);
            
            // For now, simulate the connection success
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000); // Simulate connection time
                
                _logger.LogInformation("✅ Simulated Azure Realtime API connection successful");
                await _eventBus.EmitAsync("realtime.connected", new 
                { 
                    demo = demo,
                    connected = true,
                    timestamp = DateTime.UtcNow
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling realtime connect event");
            _ = Task.Run(async () => await _eventBus.EmitAsync("realtime.error", new { error = ex.Message }));
        }
    }

    private void OnRealtimeSessionCreate(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("🎯 Azure Realtime Session Create event received");

            var deployment = GetPropertyValue<string>(cxEvent, "deployment", "gpt-4o-mini-realtime-preview");
            var mode = GetPropertyValue<string>(cxEvent, "mode", "voice");
            
            _logger.LogInformation("Creating session with deployment: {Deployment}, mode: {Mode}", deployment, mode);
            
            // Simulate session creation
            _ = Task.Run(async () =>
            {
                await Task.Delay(500); // Simulate session creation time
                
                _logger.LogInformation("✅ Simulated Azure Realtime session created");
                await _eventBus.EmitAsync("realtime.session.created", new 
                { 
                    deployment = deployment,
                    mode = mode,
                    sessionId = Guid.NewGuid().ToString(),
                    timestamp = DateTime.UtcNow
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling realtime session create event");
            _ = Task.Run(async () => await _eventBus.EmitAsync("realtime.error", new { error = ex.Message }));
        }
    }

    private void OnRealtimeTextSend(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("📝 Azure Realtime Text Send event received");

            var text = GetPropertyValue<string>(cxEvent, "text", "");
            var deployment = GetPropertyValue<string>(cxEvent, "deployment", "gpt-4o-mini-realtime-preview");
            
            _logger.LogInformation("Processing text for synthesis: {Text}", 
                string.IsNullOrEmpty(text) ? "(empty)" : 
                text.Length > 50 ? text.Substring(0, 50) + "..." : text);
            
            // Simulate text processing and audio generation
            _ = Task.Run(async () =>
            {
                // Simulate text processing
                await Task.Delay(200);
                await _eventBus.EmitAsync("realtime.text.response", new 
                { 
                    content = $"Processed: {text}",
                    isComplete = true,
                    timestamp = DateTime.UtcNow
                });
                
                // Simulate audio generation (generate sample audio data)
                await Task.Delay(800);
                var sampleAudioData = GenerateSampleAudioData(text ?? "Hello World");
                
                _logger.LogInformation("✅ Generated sample audio data: {Length} bytes", sampleAudioData.Length);
                await _eventBus.EmitAsync("realtime.audio.response", new 
                { 
                    audioData = sampleAudioData,
                    isComplete = true,
                    format = "pcm16",
                    sampleRate = 24000,
                    channels = 1,
                    timestamp = DateTime.UtcNow
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling realtime text send event");
            _ = Task.Run(async () => await _eventBus.EmitAsync("realtime.error", new { error = ex.Message }));
        }
    }

    private void OnRealtimeAudioSend(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("🎤 Azure Realtime Audio Send event received");

            var audioData = GetPropertyValue<byte[]>(cxEvent, "audio");
            var deployment = GetPropertyValue<string>(cxEvent, "deployment", "gpt-4o-mini-realtime-preview");
            
            if (audioData != null)
            {
                _logger.LogInformation("Processing audio data: {Length} bytes", audioData.Length);
                
                // Simulate audio processing
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await _eventBus.EmitAsync("realtime.audio.processed", new 
                    { 
                        processed = true,
                        inputLength = audioData.Length,
                        timestamp = DateTime.UtcNow
                    });
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling realtime audio send event");
            _ = Task.Run(async () => await _eventBus.EmitAsync("realtime.error", new { error = ex.Message }));
        }
    }

    private byte[] GenerateSampleAudioData(string text)
    {
        // Generate a simple sine wave audio sample for testing
        int sampleRate = 24000;
        int duration = Math.Min(3, Math.Max(1, text.Length / 10)); // 1-3 seconds based on text length
        int samples = sampleRate * duration;
        byte[] audioData = new byte[samples * 2]; // 16-bit samples
        
        double frequency = 440.0; // A4 note
        
        for (int i = 0; i < samples; i++)
        {
            double sample = Math.Sin(2 * Math.PI * frequency * i / sampleRate);
            short value = (short)(sample * 16383); // 16-bit signed sample
            
            audioData[i * 2] = (byte)(value & 0xFF);
            audioData[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }
        
        return audioData;
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
