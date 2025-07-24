using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using CxLanguage.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace CxLanguage.StandardLibrary.Services;

/// <summary>
/// Voice output service for audio synthesis and playback using NAudio
/// Handles text-to-speech synthesis and audio file playback for voice responses
/// Features direct hardware control with Dr. Thorne's optimizations
/// </summary>
public interface IVoiceOutputService
{
    /// <summary>
    /// Play audio data directly from byte array (for Azure OpenAI Realtime API responses)
    /// </summary>
    Task PlayAudioAsync(byte[] audioData, int sampleRate = 24000, int channels = 1);
    
    /// <summary>
    /// Play audio from a file path
    /// </summary>
    Task PlayAudioFileAsync(string filePath);
    
    /// <summary>
    /// Stop all audio playback
    /// </summary>
    Task StopPlaybackAsync();
    
    /// <summary>
    /// Check if audio is currently playing
    /// </summary>
    bool IsPlaying { get; }
    
    /// <summary>
    /// Get available audio output devices
    /// </summary>
    string[] GetOutputDevices();
    
    /// <summary>
    /// Set the output device for audio playback
    /// </summary>
    Task SetOutputDeviceAsync(int deviceIndex);
}

public class VoiceOutputService : IVoiceOutputService, IDisposable
{
    private readonly ILogger<VoiceOutputService> _logger;
    private readonly ICxEventBus _eventBus;
    private IWavePlayer? _waveOut;
    private TaskCompletionSource<bool>? _playbackTcs;
    private bool _isDisposed;
    private volatile bool _isShuttingDown;
    private readonly object _playbackLock = new();
    private int _deviceIndex = -1; // -1 means use default audio device

    public VoiceOutputService(ILogger<VoiceOutputService> logger, ICxEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
        _eventBus.Subscribe("system.shutdown", OnSystemShutdown);
        _logger.LogInformation("🔊 Voice Output Service initialized with direct hardware control");
    }

    private void OnSystemShutdown(CxEvent cxEvent)
    {
        _logger.LogWarning("Received system.shutdown event. Preparing to stop voice output service.");
        _isShuttingDown = true;
        _ = StopPlaybackAsync();
    }

    public bool IsPlaying
    {
        get
        {
            lock (_playbackLock)
            {
                return _waveOut?.PlaybackState == PlaybackState.Playing;
            }
        }
    }

    public async Task PlayAudioAsync(byte[] audioData, int sampleRate = 24000, int channels = 1)
    {
        if (_isDisposed) return;

        await StopPlaybackAsync(); // Clean up previous playback
        _playbackTcs = new TaskCompletionSource<bool>();

        try
        {
            _logger.LogInformation("🔊 Playing audio data: {Length} bytes, {SampleRate}Hz, {Channels} channels", 
                audioData.Length, sampleRate, channels);

            // Dr. Thorne's Hardware-Level Audio Bridge
            var waveFormat = new WaveFormat(sampleRate, 16, channels);
            
            // Create a persistent buffer that won't be GC'd during playback
            var bufferedProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = audioData.Length * 4, // 4x buffer for safety
                DiscardOnBufferOverflow = false,
                ReadFully = true
            };
            bufferedProvider.AddSamples(audioData, 0, audioData.Length);

            // Use WaveOutEvent for maximum hardware compatibility
            var waveOut = new WaveOutEvent { DeviceNumber = _deviceIndex };
            
            // Hardware-optimized event handler
            waveOut.PlaybackStopped += (s, e) =>
            {
                _logger.LogInformation("🔇 Hardware playback stopped.");
                
                // Clean disposal in correct order
                bufferedProvider.ClearBuffer();
                waveOut.Dispose();

                if (e.Exception != null)
                {
                    _logger.LogError(e.Exception, "Hardware playback failed.");
                    _playbackTcs.TrySetException(e.Exception);
                }
                else
                {
                    _logger.LogInformation("✅ Hardware playback completed successfully.");
                    _playbackTcs.TrySetResult(true);
                }
                
                _ = EmitEventAsync("voice.output.completed", new { timestamp = DateTime.UtcNow, exception = e.Exception?.Message });
            };

            lock(_playbackLock)
            {
                _waveOut = waveOut;
            }

            waveOut.Init(bufferedProvider);
            waveOut.Play();

            _logger.LogInformation("▶️ Direct hardware playback initiated.");
            await EmitEventAsync("voice.output.started", new { audioLength = audioData.Length, sampleRate, channels, timestamp = DateTime.UtcNow });

            await _playbackTcs.Task; // Wait for playback completion
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in direct hardware playback");
            _playbackTcs?.TrySetException(ex);
            await EmitEventAsync("voice.output.error", new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    public async Task PlayAudioFileAsync(string filePath)
    {
        if (_isDisposed) return;
        
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("❌ Audio file not found: {FilePath}", filePath);
                await EmitEventAsync("voice.output.error", new { error = "File not found", filePath = filePath });
                return;
            }

            _logger.LogInformation("🔊 Playing audio file: {FilePath}", filePath);

            await Task.Run(() =>
            {
                lock (_playbackLock)
                {
                    // Stop any current playbook
                    _waveOut?.Stop();
                    _waveOut?.Dispose();

                    // Create audio file reader
                    var audioFile = new AudioFileReader(filePath);
                    
                    // Create wave out device
                    _waveOut = new WaveOutEvent{ DeviceNumber = _deviceIndex };
                    _waveOut.Init(audioFile);
                    _waveOut.PlaybackStopped += OnPlaybackStopped;
                    
                    // Start playback
                    _waveOut.Play();
                    
                    _logger.LogInformation("✅ Audio file playback started successfully");
                    
                    // Emit event for playback started
                    _ = Task.Run(() => EmitEventAsync("voice.output.started", new 
                    { 
                        filePath = filePath, 
                        timestamp = DateTime.UtcNow
                    }));
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error playing audio file: {FilePath}", filePath);
            await EmitEventAsync("voice.output.error", new { error = ex.Message, filePath = filePath });
        }
    }

    public Task StopPlaybackAsync()
    {
        IWavePlayer? playerToStop = null;
        lock (_playbackLock)
        {
            playerToStop = _waveOut;
            _waveOut = null;
        }

        if (playerToStop?.PlaybackState == PlaybackState.Playing)
        {
            playerToStop.Stop();
        }
        
        _playbackTcs?.TrySetResult(true); // Signal that we are done.
        return Task.CompletedTask;
    }

    public string[] GetOutputDevices()
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                                    .Select(d => d.FriendlyName)
                                    .ToArray();
            
            _logger.LogInformation("🔊 Found {Count} audio output devices", devices.Length);
            enumerator.Dispose();
            return devices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting output devices");
            return Array.Empty<string>();
        }
    }

    public async Task SetOutputDeviceAsync(int deviceIndex)
    {
        try
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                if (deviceIndex < -1 || deviceIndex >= devices.Count)
                {
                    _logger.LogWarning("Invalid device index: {DeviceIndex}. Using default device.", deviceIndex);
                    _deviceIndex = -1;
                }
                else
                {
                    _deviceIndex = deviceIndex;
                }
            }

            _logger.LogInformation("🔊 Output device set to index: {DeviceIndex}", _deviceIndex);

            await Task.CompletedTask;
            
            await EmitEventAsync("voice.output.device.set", new { deviceIndex = _deviceIndex, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error setting output device");
            await EmitEventAsync("voice.output.error", new { error = ex.Message, deviceIndex = deviceIndex });
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogInformation("🔇 Audio playback completed (file handler)");
        
        // Emit playback completed event
        _ = Task.Run(() => EmitEventAsync("voice.output.completed", new 
        { 
            timestamp = DateTime.UtcNow,
            exception = e.Exception?.Message
        }));
    }

    private async Task EmitEventAsync(string eventName, object eventData)
    {
        if (_isDisposed || _isShuttingDown)
        {
            _logger.LogWarning("Skipping event emission during shutdown: {EventName}", eventName);
            return;
        }
        
        try
        {
            await _eventBus.EmitAsync(eventName, eventData);
        }
        catch (Exception ex)
        {
            // If the service provider is disposed, this will be caught here.
            if (ex is ObjectDisposedException) {
                _logger.LogWarning("Could not emit event '{EventName}' because the event bus is disposed.", eventName);
            } else {
                _logger.LogError(ex, "Error emitting event: {EventName}", eventName);
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isShuttingDown = true;
        _isDisposed = true;
        
        // There is no Unsubscribe method on ICxEventBus, so we can't unsubscribe.
        // This is acceptable for a singleton service that lives for the app's lifetime.
        
        lock (_playbackLock)
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
        }
        
        _logger.LogInformation("🔇 Voice Output Service disposed");
    }
}
