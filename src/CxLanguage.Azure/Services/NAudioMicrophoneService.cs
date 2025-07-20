using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace CxLanguage.Azure.Services;

/// <summary>
/// NAudio Microphone Capture Service for Phase 8.3 Real-Time Hardware Integration.
/// Bridges live microphone input to the existing AzureOpenAIRealtimeService in StandardLibrary.
/// This service focuses on hardware audio capture and event emission to maintain Phase 8.2 architecture.
/// </summary>
public class NAudioMicrophoneService : IDisposable
{
    private readonly ILogger<NAudioMicrophoneService> _logger;
    private readonly IConfiguration _configuration;
    
    // NAudio components for microphone capture
    private WaveInEvent? _waveIn;
    private readonly WaveFormat _waveFormat;
    
    // Hardware capture state
    private bool _isCapturing = false;
    private bool _disposed = false;
    private int _currentDeviceId = 0;
    
    // Event handlers for live audio processing
    public event EventHandler<LiveAudioCapturedEventArgs>? AudioCaptured;
    public event EventHandler<MicrophoneErrorEventArgs>? ErrorOccurred;
    public event EventHandler<AudioDeviceChangedEventArgs>? DeviceChanged;
    
    // Audio buffer for real-time processing
    private readonly ConcurrentQueue<AudioChunk> _audioBuffer = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public NAudioMicrophoneService(
        ILogger<NAudioMicrophoneService> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        // Set up audio format optimized for real-time processing
        // 16kHz, 16-bit, mono - optimal for speech recognition and low latency
        _waveFormat = new WaveFormat(16000, 16, 1);
        
        _logger.LogInformation("🎤 NAudioMicrophoneService initialized for Phase 8.3 hardware integration");
    }
    
    /// <summary>
    /// Start live microphone capture for Phase 8.3 hardware integration
    /// </summary>
    public Task StartCapturingAsync()
    {
        if (_isCapturing)
        {
            _logger.LogWarning("🎤 Already capturing - ignoring start request");
            return Task.CompletedTask;
        }
        
        try
        {
            _logger.LogInformation("🎤 Starting Phase 8.3 live microphone capture...");
            
            // Initialize NAudio microphone capture
            _waveIn = new WaveInEvent
            {
                DeviceNumber = _currentDeviceId,
                WaveFormat = _waveFormat,
                BufferMilliseconds = 100 // 100ms buffer for real-time capture
            };
            
            // Set up event handlers
            _waveIn.DataAvailable += OnMicrophoneDataReceived;
            _waveIn.RecordingStopped += OnMicrophoneRecordingStopped;
            
            // Start hardware recording
            _waveIn.StartRecording();
            _isCapturing = true;
            
            // Start background processing for audio chunking
            _ = Task.Run(ProcessAudioChunksAsync, _cancellationTokenSource.Token);
            
            _logger.LogInformation("✅ Phase 8.3 live microphone capture started successfully");
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start live microphone capture");
            ErrorOccurred?.Invoke(this, new MicrophoneErrorEventArgs(ex));
            throw;
        }
    }
    
    /// <summary>
    /// Stop microphone capture
    /// </summary>
    public Task StopCapturingAsync()
    {
        if (!_isCapturing)
            return Task.CompletedTask;
        
        try
        {
            _logger.LogInformation("🛑 Stopping Phase 8.3 microphone capture...");
            
            _isCapturing = false;
            _cancellationTokenSource.Cancel();
            
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
            }
            
            _logger.LogInformation("✅ Microphone capture stopped successfully");
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping microphone capture");
            ErrorOccurred?.Invoke(this, new MicrophoneErrorEventArgs(ex));
            throw;
        }
    }
    
    /// <summary>
    /// Get available microphone devices for Phase 8.3 hardware selection
    /// </summary>
    public List<MicrophoneDevice> GetAvailableDevices()
    {
        var devices = new List<MicrophoneDevice>();
        
        try
        {
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var capabilities = WaveInEvent.GetCapabilities(i);
                devices.Add(new MicrophoneDevice
                {
                    Id = i,
                    Name = capabilities.ProductName,
                    Channels = capabilities.Channels
                });
                
                _logger.LogDebug($"Found microphone device {i}: {capabilities.ProductName} ({capabilities.Channels} channels)");
            }
            
            _logger.LogInformation($"🎤 Found {devices.Count} microphone devices");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error enumerating microphone devices");
            ErrorOccurred?.Invoke(this, new MicrophoneErrorEventArgs(ex));
        }
        
        return devices;
    }
    
    /// <summary>
    /// Set the active microphone device
    /// </summary>
    public Task SetMicrophoneDeviceAsync(int deviceId)
    {
        try
        {
            if (deviceId < 0 || deviceId >= WaveInEvent.DeviceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId), $"Device ID must be between 0 and {WaveInEvent.DeviceCount - 1}");
            }
            
            var wasCapturing = _isCapturing;
            
            // Stop current capture if active
            if (_isCapturing)
            {
                StopCapturingAsync();
            }
            
            _currentDeviceId = deviceId;
            var capabilities = WaveInEvent.GetCapabilities(deviceId);
            
            _logger.LogInformation($"🎤 Microphone device set to: {capabilities.ProductName} (ID: {deviceId})");
            DeviceChanged?.Invoke(this, new AudioDeviceChangedEventArgs(deviceId, capabilities.ProductName));
            
            // Restart capture if it was previously active
            if (wasCapturing)
            {
                StartCapturingAsync();
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Failed to set microphone device {deviceId}");
            ErrorOccurred?.Invoke(this, new MicrophoneErrorEventArgs(ex));
            throw;
        }
    }
    
    /// <summary>
    /// Get the current microphone device information
    /// </summary>
    public MicrophoneDevice? GetCurrentDevice()
    {
        try
        {
            if (_currentDeviceId >= 0 && _currentDeviceId < WaveInEvent.DeviceCount)
            {
                var capabilities = WaveInEvent.GetCapabilities(_currentDeviceId);
                return new MicrophoneDevice
                {
                    Id = _currentDeviceId,
                    Name = capabilities.ProductName,
                    Channels = capabilities.Channels
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting current microphone device");
        }
        
        return null;
    }
    
    /// <summary>
    /// Handle incoming microphone data
    /// </summary>
    private void OnMicrophoneDataReceived(object? sender, WaveInEventArgs e)
    {
        if (!_isCapturing)
            return;
        
        try
        {
            // Create audio chunk with timestamp and metadata
            var audioChunk = new AudioChunk
            {
                Data = new byte[e.BytesRecorded],
                Timestamp = DateTimeOffset.UtcNow,
                SampleRate = _waveFormat.SampleRate,
                Channels = _waveFormat.Channels,
                BitsPerSample = _waveFormat.BitsPerSample
            };
            
            // Copy audio data
            Buffer.BlockCopy(e.Buffer, 0, audioChunk.Data, 0, e.BytesRecorded);
            
            // Queue for processing
            _audioBuffer.Enqueue(audioChunk);
            
            // Emit immediate event for real-time processing
            AudioCaptured?.Invoke(this, new LiveAudioCapturedEventArgs(audioChunk));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling microphone data");
            ErrorOccurred?.Invoke(this, new MicrophoneErrorEventArgs(ex));
        }
    }
    
    /// <summary>
    /// Handle microphone recording stopped
    /// </summary>
    private void OnMicrophoneRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, "🎤 Microphone recording stopped with error");
            ErrorOccurred?.Invoke(this, new MicrophoneErrorEventArgs(e.Exception));
        }
        else
        {
            _logger.LogInformation("🎤 Microphone recording stopped normally");
        }
    }
    
    /// <summary>
    /// Background processing of audio chunks for optimized delivery
    /// </summary>
    private async Task ProcessAudioChunksAsync()
    {
        _logger.LogInformation("🔄 Starting Phase 8.3 audio chunk processing...");
        
        const int targetChunkSize = 3200; // ~200ms at 16kHz 16-bit mono
        var accumulatedData = new List<byte>();
        var lastProcessTime = DateTimeOffset.UtcNow;
        
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                // Process available chunks
                while (_audioBuffer.TryDequeue(out var chunk))
                {
                    accumulatedData.AddRange(chunk.Data);
                }
                
                // Process accumulated data when we have enough or after a timeout
                var timeSinceLastProcess = DateTimeOffset.UtcNow - lastProcessTime;
                if (accumulatedData.Count >= targetChunkSize || 
                    (accumulatedData.Count > 0 && timeSinceLastProcess.TotalMilliseconds > 200))
                {
                    if (accumulatedData.Count > 0)
                    {
                        var processedChunk = new ProcessedAudioChunk
                        {
                            Data = accumulatedData.ToArray(),
                            Timestamp = DateTimeOffset.UtcNow,
                            SampleRate = _waveFormat.SampleRate,
                            Channels = _waveFormat.Channels,
                            BitsPerSample = _waveFormat.BitsPerSample,
                            DurationMs = (int)(accumulatedData.Count / (double)_waveFormat.AverageBytesPerSecond * 1000)
                        };
                        
                        // Emit processed chunk event for Phase 8.3 integration
                        AudioCaptured?.Invoke(this, new LiveAudioCapturedEventArgs(processedChunk));
                        
                        accumulatedData.Clear();
                        lastProcessTime = DateTimeOffset.UtcNow;
                    }
                }
                
                // Small delay to prevent excessive CPU usage
                await Task.Delay(10, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in audio chunk processing loop");
                ErrorOccurred?.Invoke(this, new MicrophoneErrorEventArgs(ex));
            }
        }
        
        _logger.LogInformation("🔄 Audio chunk processing loop stopped");
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            
            // Stop capturing and cleanup resources
            StopCapturingAsync().Wait(TimeSpan.FromSeconds(5));
            
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            
            _waveIn?.Dispose();
            
            _logger.LogInformation("🎤 NAudioMicrophoneService disposed");
        }
    }
}

/// <summary>
/// Represents a microphone device available for capture
/// </summary>
public class MicrophoneDevice
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Channels { get; set; }
}

/// <summary>
/// Raw audio chunk captured from microphone
/// </summary>
public class AudioChunk
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTimeOffset Timestamp { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int BitsPerSample { get; set; }
}

/// <summary>
/// Processed audio chunk ready for AI processing
/// </summary>
public class ProcessedAudioChunk : AudioChunk
{
    public int DurationMs { get; set; }
}

/// <summary>
/// Event args for live audio capture events
/// </summary>
public class LiveAudioCapturedEventArgs : EventArgs
{
    public AudioChunk AudioChunk { get; }
    
    public LiveAudioCapturedEventArgs(AudioChunk audioChunk)
    {
        AudioChunk = audioChunk ?? throw new ArgumentNullException(nameof(audioChunk));
    }
}

/// <summary>
/// Event args for microphone errors
/// </summary>
public class MicrophoneErrorEventArgs : EventArgs
{
    public Exception Exception { get; }
    
    public MicrophoneErrorEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}

/// <summary>
/// Event args for audio device changes
/// </summary>
public class AudioDeviceChangedEventArgs : EventArgs
{
    public int DeviceId { get; }
    public string DeviceName { get; }
    
    public AudioDeviceChangedEventArgs(int deviceId, string deviceName)
    {
        DeviceId = deviceId;
        DeviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
    }
}
