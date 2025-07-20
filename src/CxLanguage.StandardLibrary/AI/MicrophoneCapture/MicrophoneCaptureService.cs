using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using NAudio.Wave;
using System.Collections.Concurrent;

namespace CxLanguage.StandardLibrary.AI.MicrophoneCapture;

/// <summary>
/// Microphone capture service for continuous audio input
/// Provides always-on audio processing for the Aura Live Audio Presence System
/// </summary>
public class MicrophoneCaptureService : CxAiServiceBase, IDisposable
{
    private WaveInEvent? _waveIn;
    private readonly ConcurrentQueue<byte[]> _audioBuffer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly SemaphoreSlim _processingLock;
    private bool _isListening = false;
    private readonly int _sampleRate = 16000; // Optimized for speech recognition
    private readonly int _channels = 1; // Mono for efficiency
    private readonly int _bitsPerSample = 16;

    public MicrophoneCaptureService(Kernel kernel, ILogger<MicrophoneCaptureService> logger) 
        : base(kernel, logger)
    {
        _audioBuffer = new ConcurrentQueue<byte[]>();
        _cancellationTokenSource = new CancellationTokenSource();
        _processingLock = new SemaphoreSlim(1, 1);
        
        InitializeMicrophone();
    }

    public override string ServiceName => "MicrophoneCapture";
    public override string Version => "1.0.0";

    /// <summary>
    /// Event fired when audio is captured and ready for processing
    /// </summary>
    public event EventHandler<AudioCapturedEventArgs>? AudioCaptured;

    /// <summary>
    /// Start continuous microphone listening
    /// </summary>
    public async Task<MicrophoneStartResult> StartListeningAsync(MicrophoneCaptureOptions? options = null)
    {
        var result = new MicrophoneStartResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            await _processingLock.WaitAsync();

            if (_isListening)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Microphone is already listening";
                return result;
            }

            if (_waveIn == null)
            {
                InitializeMicrophone();
            }

            _waveIn?.StartRecording();
            _isListening = true;

            result.IsSuccess = true;
            result.SampleRate = _sampleRate;
            result.Channels = _channels;
            result.BitsPerSample = _bitsPerSample;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("🎤 ALWAYS-ON MICROPHONE: Started continuous listening");
            _logger.LogInformation("   📊 Sample Rate: {SampleRate}Hz, Channels: {Channels}, Bits: {BitsPerSample}", 
                _sampleRate, _channels, _bitsPerSample);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting microphone listening");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
        finally
        {
            _processingLock.Release();
        }
    }

    /// <summary>
    /// Stop microphone listening
    /// </summary>
    public async Task<MicrophoneStopResult> StopListeningAsync()
    {
        var result = new MicrophoneStopResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            await _processingLock.WaitAsync();

            if (!_isListening)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Microphone is not currently listening";
                return result;
            }

            _waveIn?.StopRecording();
            _isListening = false;

            // Clear any remaining audio buffer
            while (_audioBuffer.TryDequeue(out _)) { }

            result.IsSuccess = true;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("🔇 ALWAYS-ON MICROPHONE: Stopped listening");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping microphone listening");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
        finally
        {
            _processingLock.Release();
        }
    }

    /// <summary>
    /// Get current microphone status
    /// </summary>
    public async Task<MicrophoneStatusResult> GetStatusAsync()
    {
        await Task.CompletedTask;
        
        return new MicrophoneStatusResult
        {
            IsSuccess = true,
            IsListening = _isListening,
            SampleRate = _sampleRate,
            Channels = _channels,
            BitsPerSample = _bitsPerSample,
            BufferedChunks = _audioBuffer.Count,
            DeviceCount = WaveInEvent.DeviceCount
        };
    }

    private void InitializeMicrophone()
    {
        try
        {
            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(_sampleRate, _bitsPerSample, _channels),
                BufferMilliseconds = 200 // 200ms buffer for responsive audio capture
            };

            _waveIn.DataAvailable += OnAudioDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            _logger.LogInformation("🎤 Microphone initialized: {SampleRate}Hz, {Channels}ch, {Bits}bit", 
                _sampleRate, _channels, _bitsPerSample);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize microphone");
            throw;
        }
    }

    private void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded > 0 && _isListening)
        {
            // Copy audio data to prevent buffer reuse issues
            var audioData = new byte[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, audioData, 0, e.BytesRecorded);
            
            // Queue for processing
            _audioBuffer.Enqueue(audioData);
            
            // Fire event for immediate processing
            AudioCaptured?.Invoke(this, new AudioCapturedEventArgs
            {
                AudioData = audioData,
                SampleRate = _sampleRate,
                Channels = _channels,
                BitsPerSample = _bitsPerSample,
                Timestamp = DateTimeOffset.UtcNow
            });

            // Optional: Log for debugging (remove in production for performance)
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("📊 Audio captured: {BytesRecorded} bytes", e.BytesRecorded);
            }
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogInformation("🎤 Recording stopped");
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, "❌ Recording stopped due to error");
        }
    }

    /// <summary>
    /// Get available audio devices
    /// </summary>
    public async Task<AudioDevicesResult> GetAudioDevicesAsync()
    {
        await Task.CompletedTask;
        
        var result = new AudioDevicesResult { IsSuccess = true };
        var devices = new List<AudioDeviceInfo>();

        try
        {
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var capabilities = WaveInEvent.GetCapabilities(i);
                devices.Add(new AudioDeviceInfo
                {
                    DeviceId = i,
                    Name = capabilities.ProductName,
                    Channels = capabilities.Channels,
                    IsDefault = i == 0 // First device is typically default
                });
            }

            result.Devices = devices;
            _logger.LogInformation("🎤 Found {DeviceCount} audio input devices", devices.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error enumerating audio devices");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _processingLock?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Event arguments for audio capture events
/// </summary>
public class AudioCapturedEventArgs : EventArgs
{
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int BitsPerSample { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Audio device information
/// </summary>
public class AudioDeviceInfo
{
    public int DeviceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Channels { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Options for microphone capture
/// </summary>
public class MicrophoneCaptureOptions : CxAiOptions
{
    public int SampleRate { get; set; } = 16000;
    public int Channels { get; set; } = 1;
    public int BitsPerSample { get; set; } = 16;
    public int BufferMilliseconds { get; set; } = 200;
    public int? DeviceId { get; set; }
}

/// <summary>
/// Result from microphone start operations
/// </summary>
public class MicrophoneStartResult : CxAiResult
{
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int BitsPerSample { get; set; }
}

/// <summary>
/// Result from microphone stop operations
/// </summary>
public class MicrophoneStopResult : CxAiResult
{
    // Inherits IsSuccess, ErrorMessage, ExecutionTime from CxAiResult
}

/// <summary>
/// Result from microphone status operations
/// </summary>
public class MicrophoneStatusResult : CxAiResult
{
    public bool IsListening { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int BitsPerSample { get; set; }
    public int BufferedChunks { get; set; }
    public int DeviceCount { get; set; }
}

/// <summary>
/// Result from audio devices enumeration
/// </summary>
public class AudioDevicesResult : CxAiResult
{
    public List<AudioDeviceInfo> Devices { get; set; } = new();
}
