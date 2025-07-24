using System.Collections.Concurrent;
using CxLanguage.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;

namespace CxLanguage.StandardLibrary.Services;

/// <summary>
/// Production-ready real-time voice input capture service
/// Captures microphone audio for voice-controlled programming in CX Language
/// </summary>
public class VoiceInputService : IVoiceInputService
{
    private WaveInEvent? _waveIn;
    private readonly ConcurrentQueue<byte[]> _audioBuffer = new();
    private readonly Timer _processingTimer;
    private bool _isListening;
    private int _deviceIndex = -1; // Default device
    private readonly ILogger<VoiceInputService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed = false;
    
    // Audio format compatible with Azure OpenAI Realtime API
    private const int SampleRate = 16000;  // 16kHz
    private const int Channels = 1;        // Mono
    private const int BitsPerSample = 16;  // 16-bit PCM
    private const int BufferMs = 100;      // 100ms buffer chunks
    
    public VoiceInputService(IServiceProvider serviceProvider, ILogger<VoiceInputService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _processingTimer = new Timer(ProcessAudioBuffer, null, Timeout.Infinite, Timeout.Infinite);
    }

    public bool IsListening => _isListening;

    public async Task StartListeningAsync()
    {
        if (_isListening)
        {
            _logger.LogWarning("Voice input already listening");
            return;
        }

        try
        {
            _logger.LogInformation("🎤 Starting voice input capture");
            
            // Initialize NAudio WaveIn
            _waveIn = new WaveInEvent
            {
                DeviceNumber = _deviceIndex == -1 ? 0 : _deviceIndex,
                WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels),
                BufferMilliseconds = BufferMs
            };

            // Subscribe to audio data events
            _waveIn.DataAvailable += OnAudioDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            // Start recording
            _waveIn.StartRecording();
            _isListening = true;

            // Start audio processing timer (process every 50ms)
            _processingTimer.Change(50, 50);

            _logger.LogInformation("✅ Voice input capture started successfully");
            
            // Emit voice capture started event
            await EmitEventAsync("voice.input.started", new
            {
                sampleRate = SampleRate,
                channels = Channels,
                bitsPerSample = BitsPerSample,
                deviceIndex = _waveIn.DeviceNumber,
                deviceName = WaveInEvent.GetCapabilities(_waveIn.DeviceNumber).ProductName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start voice input capture");
            _isListening = false;
            throw;
        }
    }

    public async Task StopListeningAsync()
    {
        if (!_isListening)
        {
            _logger.LogWarning("Voice input not currently listening");
            return;
        }

        try
        {
            _logger.LogInformation("🔇 Stopping voice input capture");
            
            _isListening = false;
            
            // Stop processing timer
            _processingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            
            // Stop recording
            _waveIn?.StopRecording();
            
            // Process any remaining audio in buffer
            ProcessAudioBuffer(null);
            
            _logger.LogInformation("✅ Voice input capture stopped successfully");
            
            // Emit voice capture stopped event
            await EmitEventAsync("voice.input.stopped", new
            {
                timestamp = DateTime.UtcNow,
                totalBufferedFrames = _audioBuffer.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping voice input capture");
            throw;
        }
    }

    public Task<string[]> GetAvailableDevicesAsync()
    {
        try
        {
            var devices = new List<string>();
            int deviceCount = WaveInEvent.DeviceCount;
            
            for (int i = 0; i < deviceCount; i++)
            {
                var capabilities = WaveInEvent.GetCapabilities(i);
                devices.Add($"{i}: {capabilities.ProductName} ({capabilities.Channels} channels)");
            }
            
            _logger.LogInformation($"📱 Found {deviceCount} audio input devices");
            return Task.FromResult(devices.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error enumerating audio devices");
            return Task.FromResult(Array.Empty<string>());
        }
    }

    public async Task SetInputDeviceAsync(int deviceIndex)
    {
        if (deviceIndex < 0 || deviceIndex >= WaveInEvent.DeviceCount)
        {
            throw new ArgumentException($"Invalid device index: {deviceIndex}. Available devices: 0-{WaveInEvent.DeviceCount - 1}");
        }

        if (_isListening)
        {
            await StopListeningAsync();
            _deviceIndex = deviceIndex;
            await StartListeningAsync();
        }
        else
        {
            _deviceIndex = deviceIndex;
        }

        var capabilities = WaveInEvent.GetCapabilities(deviceIndex);
        _logger.LogInformation($"🎤 Set input device to: {capabilities.ProductName}");
        
        await EmitEventAsync("voice.input.device.changed", new
        {
            deviceIndex,
            deviceName = capabilities.ProductName,
            channels = capabilities.Channels
        });
    }

    private void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_isListening || e.BytesRecorded == 0)
            return;

        // Copy audio data to buffer for processing
        var audioData = new byte[e.BytesRecorded];
        Array.Copy(e.Buffer, audioData, e.BytesRecorded);
        _audioBuffer.Enqueue(audioData);
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, "❌ Voice input recording stopped with error");
        }
        else
        {
            _logger.LogInformation("🔇 Voice input recording stopped normally");
        }
        
        _isListening = false;
    }

    private void ProcessAudioBuffer(object? state)
    {
        if (_audioBuffer.IsEmpty)
            return;

        try
        {
            var audioChunks = new List<byte[]>();
            var totalBytes = 0;

            // Dequeue all available audio chunks
            while (_audioBuffer.TryDequeue(out var chunk))
            {
                audioChunks.Add(chunk);
                totalBytes += chunk.Length;
            }

            if (audioChunks.Count == 0)
                return;

            // Combine audio chunks into single buffer
            var combinedAudio = new byte[totalBytes];
            var offset = 0;
            
            foreach (var chunk in audioChunks)
            {
                Array.Copy(chunk, 0, combinedAudio, offset, chunk.Length);
                offset += chunk.Length;
            }

            // Calculate audio duration
            var bytesPerSecond = SampleRate * Channels * (BitsPerSample / 8);
            var durationMs = (combinedAudio.Length * 1000.0) / bytesPerSecond;

            // Emit voice input event with audio data
            Task.Run(async () =>
            {
                try
                {
                    await EmitEventAsync("voice.input.captured", new
                    {
                        timestamp = DateTime.UtcNow,
                        durationMs = Math.Round(durationMs, 2),
                        audioData = combinedAudio,
                        sampleRate = SampleRate,
                        channels = Channels,
                        bitsPerSample = BitsPerSample,
                        bytesLength = combinedAudio.Length
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error emitting voice input event");
                }
            });

            _logger.LogDebug($"🎵 Processed {durationMs:F1}ms of audio ({combinedAudio.Length} bytes)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing audio buffer");
        }
    }

    private async Task EmitEventAsync(string eventName, object eventData)
    {
        try
        {
            // Get event bus from service provider
            var eventBus = _serviceProvider.GetService<ICxEventBus>();
            if (eventBus != null)
            {
                await eventBus.EmitAsync(eventName, eventData);
            }
            else
            {
                _logger.LogWarning($"Event bus not available, could not emit event: {eventName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error emitting event: {eventName}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_isListening)
            {
                StopListeningAsync().Wait(1000); // Wait up to 1 second
            }
            
            _processingTimer?.Dispose();
            _waveIn?.Dispose();
            
            // Clear audio buffer
            while (_audioBuffer.TryDequeue(out _)) { }
            
            _disposed = true;
        }
    }
}
