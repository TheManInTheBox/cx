using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CxLanguage.Azure.Services;

/// <summary>
/// Phase 8.3 Live Audio Event Bridge - Connects NAudio hardware capture to Phase 8.2 event architecture.
/// This bridge service provides hardware microphone integration for the CX Language runtime.
/// It emits events that can be consumed by the existing CX agent event system.
/// </summary>
public class Phase8dot3LiveAudioBridge : IDisposable
{
    private readonly ILogger<Phase8dot3LiveAudioBridge> _logger;
    private readonly NAudioMicrophoneService _microphoneService;
    
    private bool _isActive = false;
    private bool _disposed = false;
    
    // Events that can be consumed by CX Language agents
    public event EventHandler<Phase8LiveAudioEventArgs>? LiveAudioCaptured;
    public event EventHandler<Phase8ErrorEventArgs>? ErrorOccurred;
    public event EventHandler<Phase8StatusEventArgs>? StatusChanged;
    
    public Phase8dot3LiveAudioBridge(
        ILogger<Phase8dot3LiveAudioBridge> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Create microphone service with console logging
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _microphoneService = new NAudioMicrophoneService(
            loggerFactory.CreateLogger<NAudioMicrophoneService>(), 
            configuration);
        
        SetupEventBridge();
        
        _logger.LogInformation("🌉 Phase 8.3 Live Audio Bridge initialized - Ready for hardware integration");
    }
    
    /// <summary>
    /// Start Phase 8.3 live hardware audio processing
    /// </summary>
    public Task StartAsync()
    {
        if (_isActive)
        {
            _logger.LogWarning("Bridge already active");
            return Task.CompletedTask;
        }
        
        try
        {
            _logger.LogInformation("🚀 Starting Phase 8.3 live hardware audio bridge...");
            
            // Start microphone capture
            _microphoneService.StartCapturingAsync();
            
            _isActive = true;
            
            _logger.LogInformation("✅ Phase 8.3 live hardware audio bridge started successfully");
            
            // Emit bridge activation event
            StatusChanged?.Invoke(this, new Phase8StatusEventArgs
            {
                Status = "Started",
                Message = "Phase 8.3 hardware audio capture active",
                Timestamp = DateTimeOffset.UtcNow
            });
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start Phase 8.3 audio bridge");
            ErrorOccurred?.Invoke(this, new Phase8ErrorEventArgs(ex));
            throw;
        }
    }
    
    /// <summary>
    /// Stop the live audio bridge
    /// </summary>
    public Task StopAsync()
    {
        if (!_isActive)
            return Task.CompletedTask;
        
        try
        {
            _logger.LogInformation("🛑 Stopping Phase 8.3 live hardware audio bridge...");
            
            // Stop microphone capture
            _microphoneService.StopCapturingAsync();
            
            _isActive = false;
            
            _logger.LogInformation("✅ Phase 8.3 audio bridge stopped successfully");
            
            // Emit bridge deactivation event
            StatusChanged?.Invoke(this, new Phase8StatusEventArgs
            {
                Status = "Stopped",
                Message = "Phase 8.3 hardware audio capture stopped",
                Timestamp = DateTimeOffset.UtcNow
            });
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping Phase 8.3 audio bridge");
            ErrorOccurred?.Invoke(this, new Phase8ErrorEventArgs(ex));
            throw;
        }
    }
    
    /// <summary>
    /// Get available microphone devices
    /// </summary>
    public List<MicrophoneDevice> GetAvailableDevices()
    {
        return _microphoneService.GetAvailableDevices();
    }
    
    /// <summary>
    /// Set the active microphone device
    /// </summary>
    public Task SetMicrophoneDeviceAsync(int deviceId)
    {
        _microphoneService.SetMicrophoneDeviceAsync(deviceId);
        
        // Emit device change event
        var device = _microphoneService.GetCurrentDevice();
        if (device != null)
        {
            StatusChanged?.Invoke(this, new Phase8StatusEventArgs
            {
                Status = "DeviceChanged",
                Message = $"Microphone device changed to: {device.Name}",
                Timestamp = DateTimeOffset.UtcNow,
                DeviceId = device.Id,
                DeviceName = device.Name
            });
        }
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Set up the event bridge between hardware capture and Phase 8.2 architecture
    /// </summary>
    private void SetupEventBridge()
    {
        // Bridge microphone audio to live.audio events for Phase 8.2 compatibility
        _microphoneService.AudioCaptured += (sender, args) =>
        {
            try
            {
                if (_isActive && args.AudioChunk?.Data != null)
                {
                    // Emit live audio event to maintain Phase 8.2 architecture compatibility
                    // This ensures existing AuraListeningAgent continues to work with real hardware
                    LiveAudioCaptured?.Invoke(this, new Phase8LiveAudioEventArgs
                    {
                        Transcript = "", // Will be filled by speech recognition service
                        Confidence = 1.0, // Hardware capture confidence
                        Timestamp = args.AudioChunk.Timestamp,
                        AudioLengthMs = args.AudioChunk is ProcessedAudioChunk processed ? processed.DurationMs : 100,
                        SampleRate = args.AudioChunk.SampleRate,
                        IsLiveCapture = true,
                        AudioData = args.AudioChunk.Data
                    });
                    
                    _logger.LogDebug($"🎤 Bridged live audio: {args.AudioChunk.Data.Length} bytes at {args.AudioChunk.SampleRate}Hz");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error bridging microphone audio to live.audio events");
                ErrorOccurred?.Invoke(this, new Phase8ErrorEventArgs(ex));
            }
        };
        
        // Bridge microphone errors
        _microphoneService.ErrorOccurred += (sender, args) =>
        {
            _logger.LogError(args.Exception, "🎤 Microphone error in Phase 8.3 bridge");
            ErrorOccurred?.Invoke(this, new Phase8ErrorEventArgs(args.Exception));
        };
        
        // Bridge device changes
        _microphoneService.DeviceChanged += (sender, args) =>
        {
            _logger.LogInformation($"🎤 Microphone device changed to: {args.DeviceName}");
            
            StatusChanged?.Invoke(this, new Phase8StatusEventArgs
            {
                Status = "DeviceChanged",
                Message = $"Microphone device changed to: {args.DeviceName}",
                Timestamp = DateTimeOffset.UtcNow,
                DeviceId = args.DeviceId,
                DeviceName = args.DeviceName
            });
        };
        
        _logger.LogInformation("🔗 Phase 8.3 event bridge configured successfully");
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            
            // Stop the bridge
            StopAsync().Wait(TimeSpan.FromSeconds(5));
            
            // Dispose services
            _microphoneService?.Dispose();
            
            _logger.LogInformation("🌉 Phase 8.3 Live Audio Bridge disposed");
        }
    }
}

/// <summary>
/// Event args for Phase 8.3 live audio events that maintain Phase 8.2 compatibility
/// </summary>
public class Phase8LiveAudioEventArgs : EventArgs
{
    public string Transcript { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public int AudioLengthMs { get; set; }
    public int SampleRate { get; set; }
    public bool IsLiveCapture { get; set; }
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Event args for Phase 8.3 errors
/// </summary>
public class Phase8ErrorEventArgs : EventArgs
{
    public Exception Exception { get; }
    
    public Phase8ErrorEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}

/// <summary>
/// Event args for Phase 8.3 status changes
/// </summary>
public class Phase8StatusEventArgs : EventArgs
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
}
