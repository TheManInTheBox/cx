using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.TextToAudio;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;
using System.IO;
using NAudio.Wave;

namespace CxLanguage.StandardLibrary.AI.TextToSpeech;

/// <summary>
/// Text to speech service for CX standard library using Azure OpenAI TTS
/// </summary>
public class TextToSpeechService : CxAiServiceBase
{
    private readonly ITextToAudioService _textToAudioService;

    /// <summary>
    /// Initializes a new instance of the TextToSpeechService class
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <param name="logger">Logger for the service</param>
    public TextToSpeechService(Kernel kernel, ILogger<TextToSpeechService> logger) 
        : base(kernel, logger)
    {
        _textToAudioService = kernel.GetRequiredService<ITextToAudioService>();
    }

    /// <summary>
    /// Gets the name of this service
    /// </summary>
    public override string ServiceName => "TextToSpeech";
    
    /// <summary>
    /// Gets the version of this service
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Synthesize speech with streaming support (simulated chunking for compatibility)
    /// </summary>
    public async IAsyncEnumerable<SpeechStreamResult> SynthesizeSpeechStreamAsync(
        string text, 
        TextToSpeechOptions? options = null)
    {
        var startTime = DateTimeOffset.UtcNow;
        SpeechStreamResult result;

        _logger.LogInformation("Starting speech synthesis for text of length: {Length}", text.Length);

        // For now, Azure OpenAI TTS doesn't support streaming, so we synthesize the whole text
        var synthesisResult = await SynthesizeAsync(text, options);
        
        if (synthesisResult.IsSuccess)
        {
            // Return the complete audio as a single chunk for compatibility
            result = new SpeechStreamResult
            {
                IsSuccess = true,
                AudioChunk = synthesisResult.AudioData ?? Array.Empty<byte>(),
                TextChunk = text,
                IsComplete = true,
                ChunkIndex = 0,
                TotalChunks = 1,
                ExecutionTime = DateTimeOffset.UtcNow - startTime
            };
        }
        else
        {
            result = new SpeechStreamResult
            {
                IsSuccess = false,
                AudioChunk = Array.Empty<byte>(),
                TextChunk = text,
                IsComplete = true,
                ChunkIndex = 0,
                TotalChunks = 1,
                ExecutionTime = DateTimeOffset.UtcNow - startTime,
                ErrorMessage = synthesisResult.ErrorMessage
            };
        }

        yield return result;
    }

    /// <summary>
    /// Simple text-to-speech synthesis using Azure OpenAI
    /// </summary>
    public async Task<SpeechSynthesisResult> SynthesizeAsync(
        string text, 
        TextToSpeechOptions? options = null)
    {
        var defaultProsody = new ProsodyOptions
        {
            Rate = 1.0f,
            Pitch = 0.0f,
            Volume = 1.0f
        };
        
        return await SynthesizeWithProsodyAsync(text, defaultProsody, options);
    }

    /// <summary>
    /// Synthesize speech with advanced prosody controls using Azure OpenAI TTS
    /// </summary>
    public async Task<SpeechSynthesisResult> SynthesizeWithProsodyAsync(
        string text, 
        ProsodyOptions prosody,
        TextToSpeechOptions? options = null)
    {
        var result = new SpeechSynthesisResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Synthesizing speech with Azure OpenAI TTS");

            // Configure Azure OpenAI TTS execution settings
            var executionSettings = new OpenAITextToAudioExecutionSettings
            {
                Voice = options?.Voice ?? "alloy",
                ResponseFormat = "mp3",
                Speed = (float)Math.Max(0.25, Math.Min(4.0, prosody.Rate))
            };

            // Call Azure OpenAI TTS service through Semantic Kernel
            var audioResult = await _textToAudioService.GetAudioContentAsync(text, executionSettings);

            result.IsSuccess = true;
            result.AudioData = audioResult.Data?.ToArray() ?? Array.Empty<byte>();
            result.InputText = text;
            result.Prosody = prosody;
            result.Duration = CalculateSpeechDuration(text, prosody.Rate);
            result.Voice = executionSettings.Voice;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Speech synthesis completed successfully. Audio data size: {Size} bytes", result.AudioData.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing speech with Azure OpenAI TTS");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Real-time speech synthesis for conversational AI
    /// </summary>
    public async Task<ConversationalSpeechResult> SynthesizeConversationalAsync(
        string text,
        ConversationContext context,
        TextToSpeechOptions? options = null)
    {
        var result = new ConversationalSpeechResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Synthesizing conversational speech with context");

            // Use the regular synthesis method for conversational text
            var synthesisResult = await SynthesizeAsync(text, options);
            
            if (synthesisResult.IsSuccess)
            {
                result.IsSuccess = true;
                result.AudioData = synthesisResult.AudioData;
                result.InputText = text;
                result.Context = context;
                result.TurnNumber = context.TurnNumber;
                result.SpeakingRate = options?.Speed ?? 1.0f;
                result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            }
            else
            {
                result.IsSuccess = false;
                result.ErrorMessage = synthesisResult.ErrorMessage;
                result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing conversational speech");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Batch synthesis for multiple texts
    /// </summary>
    public async Task<BatchSpeechResult> SynthesizeBatchAsync(
        IEnumerable<string> texts,
        TextToSpeechOptions? options = null)
    {
        var result = new BatchSpeechResult();
        var startTime = DateTimeOffset.UtcNow;
        var textList = texts.ToList();

        try
        {
            _logger.LogInformation("Synthesizing batch of {Count} texts", textList.Count);

            var synthesisResults = new List<SpeechSynthesisResult>();

            foreach (var text in textList)
            {
                var synthesisResult = await SynthesizeAsync(text, options);
                synthesisResults.Add(new SpeechSynthesisResult
                {
                    IsSuccess = synthesisResult.IsSuccess,
                    AudioData = synthesisResult.AudioData,
                    InputText = text,
                    Voice = synthesisResult.Voice,
                    Duration = CalculateSpeechDuration(text, options?.Speed ?? 1.0f),
                    ErrorMessage = synthesisResult.ErrorMessage
                });
            }

            result.IsSuccess = synthesisResults.All(r => r.IsSuccess);
            result.Results = synthesisResults;
            result.InputTexts = textList;
            result.TotalDuration = synthesisResults.Sum(r => r.Duration);
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch speech synthesis");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    private List<string> SplitTextIntoChunks(string text, int chunkSize)
    {
        var chunks = new List<string>();
        var sentences = text.Split('.', StringSplitOptions.RemoveEmptyEntries);
        
        var currentChunk = "";
        foreach (var sentence in sentences)
        {
            if (currentChunk.Length + sentence.Length > chunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.Trim());
                currentChunk = sentence;
            }
            else
            {
                currentChunk += sentence + ".";
            }
        }
        
        if (!string.IsNullOrWhiteSpace(currentChunk))
        {
            chunks.Add(currentChunk.Trim());
        }

        return chunks;
    }

    private float CalculateSpeechDuration(string text, float rate)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var baseMinutes = words / 150.0f; // Average speaking rate
        return (baseMinutes * 60.0f) / rate;
    }

    private TextToSpeechOptions AdjustForConversation(TextToSpeechOptions? options, ConversationContext context)
    {
        var adjusted = options ?? new TextToSpeechOptions();
        
        // Adjust based on conversation context
        if (context.IsFirstTurn)
        {
            adjusted.Speed = Math.Max(0.9f, adjusted.Speed ?? 1.0f); // Slightly slower for introduction
        }
        else if (context.UserSentiment == "urgent")
        {
            adjusted.Speed = Math.Min(1.2f, adjusted.Speed ?? 1.0f); // Faster for urgent responses
        }

        return adjusted;
    }

    /// <summary>
    /// Synthesize speech and play it immediately using NAudio direct memory playback
    /// </summary>
    public async Task<PlaybackResult> SpeakAsync(
        string text,
        TextToSpeechOptions? options = null)
    {
        var result = new PlaybackResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Synthesizing and playing speech directly from memory for text: {Text}", text.Length > 100 ? text.Substring(0, 100) + "..." : text);

            // Configure Azure OpenAI TTS to return WAV format for direct playback
            var executionSettings = new OpenAITextToAudioExecutionSettings
            {
                Voice = options?.Voice ?? "alloy",
                ResponseFormat = "mp3", // Use MP3 format for better NAudio compatibility
                Speed = options?.Speed ?? 1.0f
            };

            // Call Azure OpenAI TTS service directly for MP3 audio
            var audioResult = await _textToAudioService.GetAudioContentAsync(text, executionSettings);
            
            if (audioResult?.Data == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "No audio data received from TTS service";
                result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
                return result;
            }

            var audioData = audioResult.Data?.ToArray() ?? Array.Empty<byte>();
            _logger.LogInformation("Received MP3 audio data: {Size} bytes", audioData.Length);

            // Play audio directly from memory using NAudio
            await PlayAudioFromMemoryAsync(audioData);

            result.IsSuccess = true;
            result.InputText = text;
            result.AudioDataSize = audioData.Length;
            result.AudioPath = "[Direct Memory Playback - No File Created]";
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Direct memory playback successful. Size: {Size} bytes, Duration: {Duration}ms", 
                audioData.Length, result.ExecutionTime.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in direct memory audio playback");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Play audio directly from memory using NAudio with MP3 support
    /// </summary>
    private async Task PlayAudioFromMemoryAsync(byte[] audioData)
    {
        try
        {
            _logger.LogInformation("Starting NAudio MP3 memory streaming playback...");

            // MP3 is much simpler - NAudio has built-in support
            if (await TryPlayMp3FromMemoryAsync(audioData))
            {
                _logger.LogInformation("MP3 memory streaming successful!");
                return;
            }

            // If MP3 streaming fails, throw an exception instead of falling back
            throw new InvalidOperationException("MP3 memory streaming failed - no fallback to temporary files");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NAudio MP3 memory streaming");
            throw;
        }
    }

    /// <summary>
    /// Attempt MP3 memory streaming using NAudio's built-in Mp3FileReader equivalent
    /// </summary>
    private async Task<bool> TryPlayMp3FromMemoryAsync(byte[] audioData)
    {
        try
        {
            _logger.LogInformation("Starting MP3 memory streaming with {Size} bytes", audioData.Length);

            // Create memory stream from MP3 data
            using var memoryStream = new MemoryStream(audioData);
            
            // NAudio can read MP3 directly from stream
            using var mp3Reader = new Mp3FileReader(memoryStream);
            using var outputDevice = new WaveOutEvent();

            outputDevice.Init(mp3Reader);

            var playbackCompleted = new TaskCompletionSource<bool>();

            outputDevice.PlaybackStopped += (sender, e) =>
            {
                if (e.Exception != null)
                {
                    playbackCompleted.SetException(e.Exception);
                }
                else
                {
                    playbackCompleted.SetResult(true);
                }
            };

            outputDevice.Play();
            _logger.LogInformation("NAudio MP3 memory streaming started, waiting for completion...");

            await playbackCompleted.Task;
            _logger.LogInformation("NAudio MP3 memory streaming completed successfully - NO TEMP FILES!");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MP3 memory streaming failed");
            return false;
        }
    }

    /// <summary>
    /// Fallback audio playback using system command
    /// </summary>
    private async Task PlayAudioFallbackAsync(byte[] audioData)
    {
        try
        {
            var tempWavPath = Path.GetTempFileName() + ".wav";
            await File.WriteAllBytesAsync(tempWavPath, audioData);
            
            _logger.LogInformation("Fallback: Using system command for playback");
            
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start /wait wmplayer \"{tempWavPath}\" /close",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            
            if (process != null)
            {
                await process.WaitForExitAsync();
            }
            
            // Clean up temporary file
            try { File.Delete(tempWavPath); } catch { /* Ignore cleanup errors */ }
        }
        catch (Exception fallbackEx)
        {
            _logger.LogError(fallbackEx, "Fallback playback also failed");
            throw;
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the TextToSpeechService and optionally releases the managed resources
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // No resources to dispose for Azure OpenAI TTS service
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Synchronous wrapper for basic speech synthesis
    /// </summary>
    public SpeechSynthesisResult SynthesizeSpeech(string text, TextToSpeechOptions? options = null)
    {
        return SynthesizeWithProsodyAsync(text, new ProsodyOptions(), options).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Prosody control options for speech synthesis
/// </summary>
public class ProsodyOptions
{
    /// <summary>
    /// Speech rate multiplier (default 1.0f)
    /// </summary>
    public float Rate { get; set; } = 1.0f; // Speech rate
    
    /// <summary>
    /// Pitch adjustment in Hz (default 0.0f)
    /// </summary>
    public float Pitch { get; set; } = 0.0f; // Pitch adjustment in Hz
    
    /// <summary>
    /// Volume level (default 1.0f)
    /// </summary>
    public float Volume { get; set; } = 1.0f; // Volume level
    
    /// <summary>
    /// Emphasis style for speech
    /// </summary>
    public string? Emphasis { get; set; } // Emphasis style
    
    /// <summary>
    /// Additional pause duration in seconds (default 0.0f)
    /// </summary>
    public float Pause { get; set; } = 0.0f; // Additional pause in seconds
}

/// <summary>
/// Conversation context for speech synthesis adaptation
/// </summary>
public class ConversationContext
{
    /// <summary>
    /// Current turn number in the conversation
    /// </summary>
    public int TurnNumber { get; set; }
    
    /// <summary>
    /// Indicates whether this is the first turn in the conversation
    /// </summary>
    public bool IsFirstTurn { get; set; }
    
    /// <summary>
    /// Detected sentiment of the user (e.g., "happy", "angry", "urgent")
    /// </summary>
    public string? UserSentiment { get; set; }
    
    /// <summary>
    /// Current topic of the conversation
    /// </summary>
    public string? ConversationTopic { get; set; }
    
    /// <summary>
    /// Total duration of the conversation so far
    /// </summary>
    public TimeSpan ConversationDuration { get; set; }
    
    /// <summary>
    /// Additional metadata for conversation context
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Options for text to speech operations
/// </summary>
public class TextToSpeechOptions : CxAiOptions
{
    /// <summary>
    /// Voice to use for speech synthesis
    /// </summary>
    public string? Voice { get; set; }
    
    /// <summary>
    /// Speech speed multiplier
    /// </summary>
    public float? Speed { get; set; }
    
    /// <summary>
    /// Pitch adjustment for the voice
    /// </summary>
    public float? Pitch { get; set; }
    
    /// <summary>
    /// Volume level for the speech
    /// </summary>
    public float? Volume { get; set; }
    
    /// <summary>
    /// Language for speech synthesis
    /// </summary>
    public string? Language { get; set; }
    
    /// <summary>
    /// Size of text chunks for streaming synthesis
    /// </summary>
    public int? ChunkSize { get; set; }
    
    /// <summary>
    /// Whether to enable streaming synthesis
    /// </summary>
    public bool EnableStreaming { get; set; } = false;
    
    /// <summary>
    /// Output audio format (e.g., "mp3", "wav")
    /// </summary>
    public string? OutputFormat { get; set; }
}

/// <summary>
/// Result from streaming speech synthesis
/// </summary>
public class SpeechStreamResult : CxAiResult
{
    /// <summary>
    /// Audio data for the current chunk
    /// </summary>
    public byte[] AudioChunk { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Text content for the current chunk
    /// </summary>
    public string TextChunk { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates whether the streaming is complete
    /// </summary>
    public bool IsComplete { get; set; }
    
    /// <summary>
    /// Index of the current chunk
    /// </summary>
    public int ChunkIndex { get; set; }
    
    /// <summary>
    /// Total number of chunks in the stream
    /// </summary>
    public int TotalChunks { get; set; }
}

/// <summary>
/// Result from speech synthesis operations
/// </summary>
public class SpeechSynthesisResult : CxAiResult
{
    /// <summary>
    /// Generated audio data
    /// </summary>
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Original input text that was synthesized
    /// </summary>
    public string InputText { get; set; } = string.Empty;
    
    /// <summary>
    /// Voice used for synthesis
    /// </summary>
    public string Voice { get; set; } = string.Empty;
    
    /// <summary>
    /// Duration of the synthesized audio in seconds
    /// </summary>
    public float Duration { get; set; }
    
    /// <summary>
    /// Prosody options used for synthesis
    /// </summary>
    public ProsodyOptions? Prosody { get; set; }
}

/// <summary>
/// Result from conversational speech synthesis
/// </summary>
public class ConversationalSpeechResult : CxAiResult
{
    /// <summary>
    /// Generated audio data for the conversation turn
    /// </summary>
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Original input text that was synthesized
    /// </summary>
    public string InputText { get; set; } = string.Empty;
    
    /// <summary>
    /// Conversation context used for synthesis adaptation
    /// </summary>
    public ConversationContext? Context { get; set; }
    
    /// <summary>
    /// Turn number in the conversation
    /// </summary>
    public int TurnNumber { get; set; }
    
    /// <summary>
    /// Speaking rate used for this turn
    /// </summary>
    public float SpeakingRate { get; set; }
}

/// <summary>
/// Result from batch speech synthesis
/// </summary>
public class BatchSpeechResult : CxAiResult
{
    /// <summary>
    /// Individual synthesis results for each input text
    /// </summary>
    public List<SpeechSynthesisResult> Results { get; set; } = new();
    
    /// <summary>
    /// Original input texts that were synthesized
    /// </summary>
    public List<string> InputTexts { get; set; } = new();
    
    /// <summary>
    /// Total duration of all synthesized audio in seconds
    /// </summary>
    public float TotalDuration { get; set; }
}

/// <summary>
/// Result from playing synthesized speech
/// </summary>
public class PlaybackResult : CxAiResult
{
    /// <summary>
    /// Path to the temporary audio file that was played
    /// </summary>
    public string AudioPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Original input text that was synthesized and played
    /// </summary>
    public string InputText { get; set; } = string.Empty;
    
    /// <summary>
    /// Size of the audio data in bytes
    /// </summary>
    public int AudioDataSize { get; set; }
}
