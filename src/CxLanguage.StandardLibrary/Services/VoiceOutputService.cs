using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using CxLanguage.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Services;

/// <summary>
/// Voice output service for audio synthesis and playback using NAudio
/// Handles text-to-speech synthesis and audio file playback for voice responses
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
    private readonly IServiceProvider _serviceProvider;
    private WaveOutEvent? _waveOut;
    private bool _isDisposed;
    private readonly object _playbackLock = new();

    public VoiceOutputService(ILogger<VoiceOutputService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _logger.LogInformation("🔊 Voice Output Service initialized");
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
        
        try
        {
            _logger.LogInformation("🔊 Playing audio data: {Length} bytes, {SampleRate}Hz, {Channels} channels", 
                audioData.Length, sampleRate, channels);

            await Task.Run(() =>
            {
                lock (_playbackLock)
                {
                    // Stop any current playback
                    _waveOut?.Stop();
                    _waveOut?.Dispose();

                    // Create wave format for the audio data
                    var waveFormat = new WaveFormat(sampleRate, 16, channels);
                    
                    // Create memory stream from audio data
                    var memoryStream = new MemoryStream(audioData);
                    var rawSourceWaveStream = new RawSourceWaveStream(memoryStream, waveFormat);
                    
                    // Create wave out device
                    _waveOut = new WaveOutEvent();
                    _waveOut.Init(rawSourceWaveStream);
                    _waveOut.PlaybackStopped += OnPlaybackStopped;
                    
                    // Start playback
                    _waveOut.Play();
                    
                    _logger.LogInformation("✅ Audio playback started successfully");
                    
                    // Emit event for playback started
                    _ = Task.Run(() => EmitEventAsync("voice.output.started", new 
                    { 
                        audioLength = audioData.Length, 
                        sampleRate = sampleRate, 
                        channels = channels,
                        timestamp = DateTime.UtcNow
                    }));
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error playing audio data");
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
                    // Stop any current playback
                    _waveOut?.Stop();
                    _waveOut?.Dispose();

                    // Create audio file reader
                    var audioFile = new AudioFileReader(filePath);
                    
                    // Create wave out device
                    _waveOut = new WaveOutEvent();
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

    public async Task StopPlaybackAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                lock (_playbackLock)
                {
                    if (_waveOut != null)
                    {
                        _waveOut.Stop();
                        _logger.LogInformation("🔇 Audio playback stopped");
                    }
                }
            });
            
            await EmitEventAsync("voice.output.stopped", new { timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping audio playback");
        }
    }

    public string[] GetOutputDevices()
    {
        try
        {
            // Simplified device enumeration - NAudio WaveOutEvent uses default device
            var devices = new[] { "Default Audio Device" };
            
            _logger.LogInformation("🔊 Found {Count} audio output devices", devices.Length);
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
            await Task.Run(() =>
            {
                lock (_playbackLock)
                {
                    // Stop current playback
                    _waveOut?.Stop();
                    _waveOut?.Dispose();
                    
                    // Note: NAudio WaveOutEvent doesn't support device selection directly
                    // This would require using DirectSound or WASAPI for device selection
                    _logger.LogInformation("🔊 Output device selection requested: {DeviceIndex}", deviceIndex);
                }
            });
            
            await EmitEventAsync("voice.output.device.set", new { deviceIndex = deviceIndex, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error setting output device");
            await EmitEventAsync("voice.output.error", new { error = ex.Message, deviceIndex = deviceIndex });
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogInformation("🔇 Audio playback completed");
        
        // Emit playback completed event
        _ = Task.Run(() => EmitEventAsync("voice.output.completed", new 
        { 
            timestamp = DateTime.UtcNow,
            exception = e.Exception?.Message
        }));
    }

    private async Task EmitEventAsync(string eventName, object eventData)
    {
        if (_isDisposed) return;
        
        try
        {
            var eventBus = _serviceProvider.GetService<ICxEventBus>();
            if (eventBus != null)
            {
                await eventBus.EmitAsync(eventName, eventData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error emitting event: {EventName}", eventName);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        
        lock (_playbackLock)
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
        }
        
        _logger.LogInformation("🔇 Voice Output Service disposed");
    }
}
