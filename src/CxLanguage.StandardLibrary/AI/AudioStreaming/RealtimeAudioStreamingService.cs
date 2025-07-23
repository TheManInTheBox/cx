using NAudio.Wave;
using Microsoft.Extensions.Logging;
using CxLanguage.Runtime;
using System.Collections.Concurrent;

namespace CxLanguage.StandardLibrary.AI.AudioStreaming
{
    /// <summary>
    /// Service for streaming Azure Realtime API audio data to NAudio for real-time playback
    /// Handles PCM audio streaming from Azure OpenAI Realtime API
    /// </summary>
    public class RealtimeAudioStreamingService : IDisposable
    {
        private readonly ILogger<RealtimeAudioStreamingService> _logger;
        private readonly ICxEventBus _eventBus;
        
        private WaveOutEvent? _outputDevice;
        private BufferedWaveProvider? _waveProvider;
        private readonly ConcurrentQueue<byte[]> _audioQueue = new();
        private bool _isPlaying = false;
        private bool _disposed = false;
        
        // Azure Realtime API uses 24kHz 16-bit mono PCM
        private readonly WaveFormat _waveFormat = new(24000, 16, 1);

        public RealtimeAudioStreamingService(
            ILogger<RealtimeAudioStreamingService> logger,
            ICxEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
            
            InitializeAudioOutput();
            
            _logger.LogInformation("✅ RealtimeAudioStreamingService initialized for Azure PCM streaming");
        }

        private void InitializeAudioOutput()
        {
            try
            {
                _outputDevice = new WaveOutEvent();
                _waveProvider = new BufferedWaveProvider(_waveFormat)
                {
                    BufferDuration = TimeSpan.FromSeconds(5), // 5 second buffer
                    DiscardOnBufferOverflow = true // Prevent memory buildup
                };
                
                _outputDevice.Init(_waveProvider);
                _outputDevice.PlaybackStopped += OnPlaybackStopped;
                
                _logger.LogInformation("✅ NAudio initialized for 24kHz 16-bit mono PCM streaming");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize NAudio for realtime streaming");
                throw;
            }
        }

        /// <summary>
        /// Stream audio data from Azure Realtime API to NAudio for immediate playback
        /// </summary>
        public async Task StreamAudioAsync(byte[] audioData)
        {
            if (_disposed || audioData == null || audioData.Length == 0)
            {
                _logger.LogWarning("⚠️ Cannot stream audio - service disposed or no data");
                return;
            }

            try
            {
                _logger.LogInformation("🎵 Streaming {Size} bytes of Azure Realtime audio to NAudio", audioData.Length);

                // Add audio data to buffer
                _waveProvider?.AddSamples(audioData, 0, audioData.Length);

                // Start playback if not already playing
                if (!_isPlaying && _outputDevice != null)
                {
                    _outputDevice.Play();
                    _isPlaying = true;
                    
                    _logger.LogInformation("🔊 NAudio playback started for realtime audio stream");
                    
                    // Emit playback started event
                    await EmitEvent("naudio.playback.started", new
                    {
                        audioSize = audioData.Length,
                        format = "24kHz_16bit_mono_PCM",
                        timestamp = DateTimeOffset.UtcNow
                    });
                }
                
                // Emit streaming complete event
                await EmitEvent("audio.streaming.complete", new
                {
                    duration = CalculateAudioDuration(audioData.Length),
                    bytesStreamed = audioData.Length,
                    timestamp = DateTimeOffset.UtcNow
                });
                
                _logger.LogInformation("✅ Audio chunk streamed successfully to NAudio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error streaming audio to NAudio");
                
                // Emit error event
                await EmitEvent("audio.streaming.error", new
                {
                    error = ex.Message,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Stop audio playback and clear buffers
        /// </summary>
        public async Task StopPlaybackAsync()
        {
            if (_outputDevice != null && _isPlaying)
            {
                _outputDevice.Stop();
                _waveProvider?.ClearBuffer();
                _isPlaying = false;
                
                _logger.LogInformation("🔇 Audio playback stopped and buffers cleared");
                
                await EmitEvent("naudio.playback.stopped", new
                {
                    timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            _isPlaying = false;
            
            if (e.Exception != null)
            {
                _logger.LogError(e.Exception, "❌ NAudio playback stopped with error");
            }
            else
            {
                _logger.LogInformation("✅ NAudio playback completed successfully");
            }
        }

        private double CalculateAudioDuration(int audioDataSize)
        {
            // 24kHz 16-bit mono = 48,000 bytes per second
            var bytesPerSecond = _waveFormat.SampleRate * _waveFormat.BitsPerSample / 8 * _waveFormat.Channels;
            return (double)audioDataSize / bytesPerSecond * 1000; // Return duration in milliseconds
        }

        private async Task EmitEvent(string eventName, object data)
        {
            try
            {
                _eventBus.Emit(eventName, data);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to emit event: {EventName}", eventName);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _outputDevice?.Dispose();
                _waveProvider = null;
                _disposed = true;
                
                _logger.LogInformation("🔇 RealtimeAudioStreamingService disposed");
            }
        }
    }
}
