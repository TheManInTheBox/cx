using CxLanguage.StandardLibrary.Core;
using CxLanguage.StandardLibrary.AI.MicrophoneCapture;
using CxLanguage.StandardLibrary.AI.AudioToText;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Collections.Concurrent;

namespace CxLanguage.StandardLibrary.AI.LiveAudio;

/// <summary>
/// Live audio processing service that bridges microphone capture to CX event system
/// This is the core of the Always-On Audio Processing capability for Aura
/// </summary>
public class LiveAudioService : CxAiServiceBase
{
    private readonly MicrophoneCaptureService _microphoneService;
    private readonly AudioToTextService _audioToTextService;
    private readonly ConcurrentQueue<byte[]> _audioProcessingQueue;
    private readonly Timer _processingTimer;
    private readonly SemaphoreSlim _processingLock;
    private bool _isActive = false;
    private List<byte> _audioBuffer = new();
    private readonly int _transcriptionIntervalMs = 1000; // Process every 1 second

    // Event for CX event system integration
    public static event EventHandler<LiveAudioEventArgs>? LiveAudioTranscribed;

    public LiveAudioService(
        Kernel kernel, 
        ILogger<LiveAudioService> logger,
        MicrophoneCaptureService microphoneService,
        AudioToTextService audioToTextService) 
        : base(kernel, logger)
    {
        _microphoneService = microphoneService;
        _audioToTextService = audioToTextService;
        _audioProcessingQueue = new ConcurrentQueue<byte[]>();
        _processingLock = new SemaphoreSlim(1, 1);

        // Set up continuous processing timer
        _processingTimer = new Timer(ProcessBufferedAudio, null, Timeout.Infinite, _transcriptionIntervalMs);

        // Subscribe to microphone events
        _microphoneService.AudioCaptured += OnAudioCaptured;

        _logger.LogInformation("🎤 LiveAudioService initialized for Always-On Audio Processing");
    }

    public override string ServiceName => "LiveAudio";
    public override string Version => "1.0.0";

    /// <summary>
    /// Start always-on audio processing
    /// </summary>
    public async Task<LiveAudioStartResult> StartAsync(LiveAudioOptions? options = null)
    {
        var result = new LiveAudioStartResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            if (_isActive)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Live audio processing is already active";
                return result;
            }

            // Start microphone capture
            var micResult = await _microphoneService.StartListeningAsync();
            if (!micResult.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Failed to start microphone: {micResult.ErrorMessage}";
                return result;
            }

            // Start processing timer
            _processingTimer.Change(0, _transcriptionIntervalMs);
            _isActive = true;

            result.IsSuccess = true;
            result.SampleRate = micResult.SampleRate;
            result.Channels = micResult.Channels;
            result.ProcessingInterval = _transcriptionIntervalMs;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("🎯 ALWAYS-ON AUDIO PROCESSING: STARTED");
            _logger.LogInformation("   📊 Processing interval: {Interval}ms", _transcriptionIntervalMs);
            _logger.LogInformation("   🎤 Audio: {SampleRate}Hz, {Channels}ch", micResult.SampleRate, micResult.Channels);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start live audio processing");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Stop always-on audio processing
    /// </summary>
    public async Task<LiveAudioStopResult> StopAsync()
    {
        var result = new LiveAudioStopResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            if (!_isActive)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Live audio processing is not currently active";
                return result;
            }

            // Stop processing timer
            _processingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // Stop microphone
            await _microphoneService.StopListeningAsync();

            // Clear buffers
            _audioBuffer.Clear();
            while (_audioProcessingQueue.TryDequeue(out _)) { }

            _isActive = false;

            result.IsSuccess = true;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("🔇 ALWAYS-ON AUDIO PROCESSING: STOPPED");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to stop live audio processing");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Get current status of live audio processing
    /// </summary>
    public async Task<LiveAudioStatusResult> GetStatusAsync()
    {
        var micStatus = await _microphoneService.GetStatusAsync();
        
        return new LiveAudioStatusResult
        {
            IsSuccess = true,
            IsActive = _isActive,
            IsListening = micStatus.IsListening,
            BufferedChunks = _audioProcessingQueue.Count,
            BufferSizeBytes = _audioBuffer.Count,
            ProcessingInterval = _transcriptionIntervalMs,
            SampleRate = micStatus.SampleRate,
            Channels = micStatus.Channels
        };
    }

    private void OnAudioCaptured(object? sender, AudioCapturedEventArgs e)
    {
        if (_isActive && e.AudioData.Length > 0)
        {
            // Add to continuous buffer for transcription
            lock (_audioBuffer)
            {
                _audioBuffer.AddRange(e.AudioData);
            }

            // Optional debug logging
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("📊 Audio buffered: {Bytes} bytes, Total buffer: {Total}", 
                    e.AudioData.Length, _audioBuffer.Count);
            }
        }
    }

    private async void ProcessBufferedAudio(object? state)
    {
        if (!_isActive || !await _processingLock.WaitAsync(10))
        {
            return;
        }

        try
        {
            byte[] audioToProcess;
            
            lock (_audioBuffer)
            {
                if (_audioBuffer.Count < 8000) // Minimum threshold for processing
                {
                    return;
                }

                // Take audio data for processing
                audioToProcess = _audioBuffer.ToArray();
                _audioBuffer.Clear();
            }

            // Transcribe audio
            var transcriptionResult = await _audioToTextService.TranscribeAsync(audioToProcess);

            if (transcriptionResult.IsSuccess && !string.IsNullOrWhiteSpace(transcriptionResult.Transcription))
            {
                // Filter out noise/empty results
                var transcript = transcriptionResult.Transcription.Trim();
                if (transcript.Length > 2 && !IsLikelyNoise(transcript))
                {
                    _logger.LogInformation("🎤 LIVE AUDIO: '{Transcript}' (confidence: {Confidence:F2})", 
                        transcript, transcriptionResult.Confidence);

                    // Fire event for CX event system
                    var eventArgs = new LiveAudioEventArgs
                    {
                        Transcript = transcript,
                        Confidence = transcriptionResult.Confidence,
                        AudioSize = audioToProcess.Length,
                        Timestamp = DateTimeOffset.UtcNow
                    };

                    LiveAudioTranscribed?.Invoke(this, eventArgs);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing buffered audio");
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private bool IsLikelyNoise(string transcript)
    {
        // Filter out common transcription noise patterns
        var lowercaseTranscript = transcript.ToLowerInvariant();
        
        // Common noise patterns from speech recognition
        string[] noisePatterns = {
            "word1 word2", "chunk1 chunk2", "audio_chunk", 
            "...", "um", "uh", "ah", "mm"
        };

        return noisePatterns.Any(pattern => lowercaseTranscript.Contains(pattern));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _processingTimer?.Dispose();
            _processingLock?.Dispose();
            if (_microphoneService != null)
            {
                _microphoneService.AudioCaptured -= OnAudioCaptured;
            }
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Event arguments for live audio transcription events
/// </summary>
public class LiveAudioEventArgs : EventArgs
{
    public string Transcript { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public int AudioSize { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Options for live audio processing
/// </summary>
public class LiveAudioOptions : CxAiOptions
{
    public int ProcessingIntervalMs { get; set; } = 1000;
    public int MinAudioThresholdBytes { get; set; } = 8000;
    public float MinConfidenceThreshold { get; set; } = 0.3f;
    public bool EnableNoiseFiltering { get; set; } = true;
}

/// <summary>
/// Result from live audio start operations
/// </summary>
public class LiveAudioStartResult : CxAiResult
{
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int ProcessingInterval { get; set; }
}

/// <summary>
/// Result from live audio stop operations
/// </summary>
public class LiveAudioStopResult : CxAiResult
{
    // Inherits IsSuccess, ErrorMessage, ExecutionTime from CxAiResult
}

/// <summary>
/// Result from live audio status operations
/// </summary>
public class LiveAudioStatusResult : CxAiResult
{
    public bool IsActive { get; set; }
    public bool IsListening { get; set; }
    public int BufferedChunks { get; set; }
    public int BufferSizeBytes { get; set; }
    public int ProcessingInterval { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }
}
