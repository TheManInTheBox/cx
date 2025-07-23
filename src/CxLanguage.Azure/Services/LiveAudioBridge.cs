using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAudio.Wave;

namespace CxLanguage.Azure.Services;

// --- Data Structures ---
public class MicrophoneDevice
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Live Audio Event Bridge - Connects NAudio hardware capture to CX Language event architecture.
/// This bridge service provides hardware microphone integration for the CX Language runtime.
/// It emits events that can be consumed by the existing CX agent event system.
/// </summary>
public class LiveAudioBridge : IDisposable
{
    private readonly ILogger<LiveAudioBridge> _logger;
    private WaveInEvent? _waveIn;
    private int _deviceId = 0;
    
    private bool _isActive = false;
    private bool _disposed = false;
    
    // Events that can be consumed by CX Language agents
    public event EventHandler<Phase8LiveAudioEventArgs>? LiveAudioCaptured;
    public event EventHandler<Phase8ErrorEventArgs>? ErrorOccurred;
    public event EventHandler<Phase8StatusEventArgs>? StatusChanged;
    
    public LiveAudioBridge(
        ILogger<LiveAudioBridge> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        SetupEventBridge();
        
        _logger.LogInformation("🌉 Live Audio Bridge initialized - Ready for hardware integration");
    }
    
    /// <summary>
    /// Start live hardware audio processing
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
            _logger.LogInformation("🚀 Starting live hardware audio bridge...");
            
            // Initialize NAudio directly
            _waveIn = new WaveInEvent
            {
                DeviceNumber = _deviceId,
                WaveFormat = new WaveFormat(16000, 16, 1), // 16kHz, 16-bit, Mono
                BufferMilliseconds = 100
            };

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;
            _waveIn.StartRecording();
            
            _isActive = true;
            
            _logger.LogInformation("✅ Live hardware audio bridge started successfully");
            
            // Emit bridge activation event
            StatusChanged?.Invoke(this, new Phase8StatusEventArgs
            {
                Status = "Started",
                Message = "Live hardware audio capture active",
                Timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start live audio bridge");
            ErrorOccurred?.Invoke(this, new Phase8ErrorEventArgs(ex));
            throw;
        }

        return Task.CompletedTask;
    }    /// <summary>
    /// Stop the live audio bridge
    /// </summary>
    public Task StopAsync()
    {
        if (!_isActive)
            return Task.CompletedTask;

        try
        {
            _logger.LogInformation("🛑 Stopping live hardware audio bridge...");
            
            // Stop NAudio recording
            _waveIn?.StopRecording();
            
            _isActive = false;
            
            _logger.LogInformation("✅ Live audio bridge stopped successfully");
            
            // Emit bridge deactivation event
            StatusChanged?.Invoke(this, new Phase8StatusEventArgs
            {
                Status = "Stopped",
                Message = "Live hardware audio capture stopped",
                Timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping live audio bridge");
            ErrorOccurred?.Invoke(this, new Phase8ErrorEventArgs(ex));
            throw;
        }

        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Get available microphone devices
    /// </summary>
    public List<MicrophoneDevice> GetAvailableDevices()
    {
        var devices = new List<MicrophoneDevice>();
        for (int i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var caps = WaveInEvent.GetCapabilities(i);
            devices.Add(new MicrophoneDevice { Id = i, Name = caps.ProductName });
        }
        return devices;
    }

    /// <summary>
    /// Set the active microphone device
    /// </summary>
    public Task SetMicrophoneDeviceAsync(int deviceId)
    {
        if (deviceId >= WaveInEvent.DeviceCount)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId), "Invalid microphone device ID.");
        }
        
        if (_deviceId == deviceId) return Task.CompletedTask;

        _deviceId = deviceId;
        var device = GetCurrentDevice();
        if (device != null)
        {
            _logger.LogInformation($"Microphone device set to: {device.Name}");
            
            StatusChanged?.Invoke(this, new Phase8StatusEventArgs
            {
                Status = "DeviceChanged",
                Message = $"Microphone device changed to: {device.Name}",
                Timestamp = DateTimeOffset.UtcNow,
                DeviceId = device.Id,
                DeviceName = device.Name
            });
        }

        // If currently recording, restart with new device
        if (_waveIn != null)
        {
            StopAsync().Wait();
            StartAsync().Wait();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Get the current microphone device
    /// </summary>
    public MicrophoneDevice? GetCurrentDevice()
    {
        if (_deviceId < WaveInEvent.DeviceCount)
        {
            var caps = WaveInEvent.GetCapabilities(_deviceId);
            return new MicrophoneDevice { Id = _deviceId, Name = caps.ProductName };
        }
        return null;
    }    /// <summary>
    /// NAudio event handler for captured audio data
    /// </summary>
    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded > 0 && _isActive)
        {
            var buffer = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, buffer, e.BytesRecorded);

            try
            {
                // Emit live audio event to maintain existing architecture compatibility
                LiveAudioCaptured?.Invoke(this, new Phase8LiveAudioEventArgs
                {
                    Transcript = "", // Will be filled by speech recognition service
                    Confidence = 1.0, // Hardware capture confidence
                    Timestamp = DateTimeOffset.UtcNow,
                    AudioLengthMs = (int)((double)e.BytesRecorded / (_waveIn?.WaveFormat.AverageBytesPerSecond ?? 32000) * 1000),
                    SampleRate = _waveIn?.WaveFormat.SampleRate ?? 16000,
                    IsLiveCapture = true,
                    AudioData = buffer
                });
                
                _logger.LogDebug($"🎤 Bridged live audio: {buffer.Length} bytes at {_waveIn?.WaveFormat.SampleRate ?? 16000}Hz");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error bridging microphone audio to live.audio events");
                ErrorOccurred?.Invoke(this, new Phase8ErrorEventArgs(ex));
            }
        }
    }

    /// <summary>
    /// NAudio event handler for recording stopped
    /// </summary>
    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogInformation("Microphone capture stopped.");
        
        if (_waveIn != null)
        {
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.RecordingStopped -= OnRecordingStopped;
            _waveIn.Dispose();
            _waveIn = null;
        }
        
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, "Microphone capture stopped with an error.");
            ErrorOccurred?.Invoke(this, new Phase8ErrorEventArgs(e.Exception));
        }
    }

    /// <summary>
    /// Set up the event bridge between hardware capture and CX Language event architecture
    /// </summary>
    private void SetupEventBridge()
    {
        _logger.LogInformation("🔗 Live audio event bridge configured successfully");
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            
            // Stop the bridge
            StopAsync().Wait(TimeSpan.FromSeconds(5));
            
            // Dispose NAudio components
            _waveIn?.Dispose();
            
            _logger.LogInformation("🌉 Live Audio Bridge disposed");
        }
    }
}

/// <summary>
/// Event args for live audio events that maintain CX Language compatibility
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
/// Event args for live audio bridge errors
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
/// Event args for live audio bridge status changes
/// </summary>
public class Phase8StatusEventArgs : EventArgs
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
}
