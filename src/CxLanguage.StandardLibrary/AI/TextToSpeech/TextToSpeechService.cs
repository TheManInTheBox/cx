using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CxLanguage.StandardLibrary.AI.TextToSpeech;

/// <summary>
/// Text to speech service for CX standard library
/// Specialized for real-time speech synthesis with streaming capabilities
/// </summary>
public class TextToSpeechService : CxAiServiceBase
{
    private readonly HttpClient _httpClient;
    private readonly List<SpeechSynthesizer> _activeSynthesizers;

    /// <summary>
    /// Initializes a new instance of the TextToSpeechService class
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <param name="logger">Logger for the service</param>
    public TextToSpeechService(Kernel kernel, ILogger<TextToSpeechService> logger) 
        : base(kernel, logger)
    {
        _httpClient = new HttpClient();
        _activeSynthesizers = new List<SpeechSynthesizer>();
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
    /// Synthesize speech with real-time streaming
    /// </summary>
    public async IAsyncEnumerable<SpeechStreamResult> SynthesizeSpeechStreamAsync(
        string text, 
        TextToSpeechOptions? options = null)
    {
        var startTime = DateTimeOffset.UtcNow;
        var synthesizer = new SpeechSynthesizer(options);
        _activeSynthesizers.Add(synthesizer);

        try
        {
            _logger.LogInformation("Starting speech synthesis stream for text of length: {Length}", text.Length);

            var chunks = SplitTextIntoChunks(text, options?.ChunkSize ?? 200);

            foreach (var chunk in chunks)
            {
                var audioChunk = await synthesizer.SynthesizeChunkAsync(chunk);
                
                yield return new SpeechStreamResult
                {
                    IsSuccess = true,
                    AudioChunk = audioChunk,
                    TextChunk = chunk,
                    IsComplete = false,
                    ChunkIndex = chunks.IndexOf(chunk),
                    TotalChunks = chunks.Count,
                    ExecutionTime = DateTimeOffset.UtcNow - startTime
                };

                // Small delay to simulate real-time streaming
                await Task.Delay(10);
            }

            yield return new SpeechStreamResult
            {
                IsSuccess = true,
                AudioChunk = Array.Empty<byte>(),
                TextChunk = string.Empty,
                IsComplete = true,
                ChunkIndex = chunks.Count,
                TotalChunks = chunks.Count,
                ExecutionTime = DateTimeOffset.UtcNow - startTime
            };
        }
        finally
        {
            _activeSynthesizers.Remove(synthesizer);
            synthesizer.Dispose();
        }
    }

    /// <summary>
    /// Synthesize speech with advanced prosody controls
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
            _logger.LogInformation("Synthesizing speech with prosody controls");

            var synthesizer = new SpeechSynthesizer(options);
            var audioData = await synthesizer.SynthesizeWithProsodyAsync(text, prosody);

            result.IsSuccess = true;
            result.AudioData = audioData;
            result.InputText = text;
            result.Prosody = prosody;
            result.Duration = CalculateSpeechDuration(text, prosody.Rate);
            result.Voice = options?.Voice ?? "default";
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing speech with prosody");
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

            var synthesizer = new SpeechSynthesizer(options);
            
            // Adjust speech parameters based on conversation context
            var adjustedOptions = AdjustForConversation(options, context);
            var audioData = await synthesizer.SynthesizeConversationalAsync(text, adjustedOptions);

            result.IsSuccess = true;
            result.AudioData = audioData;
            result.InputText = text;
            result.Context = context;
            result.TurnNumber = context.TurnNumber;
            result.SpeakingRate = adjustedOptions.Speed ?? 1.0f;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

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
            var synthesizer = new SpeechSynthesizer(options);

            foreach (var text in textList)
            {
                var audioData = await synthesizer.SynthesizeAsync(text);
                synthesisResults.Add(new SpeechSynthesisResult
                {
                    IsSuccess = true,
                    AudioData = audioData,
                    InputText = text,
                    Duration = CalculateSpeechDuration(text, options?.Speed ?? 1.0f)
                });
            }

            result.IsSuccess = true;
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
    /// Releases the unmanaged resources used by the TextToSpeechService and optionally releases the managed resources
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var synthesizer in _activeSynthesizers.ToList())
            {
                synthesizer.Dispose();
            }
            _activeSynthesizers.Clear();
            _httpClient?.Dispose();
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
/// Internal speech synthesizer helper class
/// </summary>
internal class SpeechSynthesizer : IDisposable
{
    private readonly TextToSpeechOptions? _options;
    private bool _disposed;

    public SpeechSynthesizer(TextToSpeechOptions? options)
    {
        _options = options;
    }

    public async Task<byte[]> SynthesizeAsync(string text)
    {
        await Task.Delay(50); // Simulate synthesis time
        return new byte[text.Length * 100]; // Simulated audio data
    }

    public async Task<byte[]> SynthesizeChunkAsync(string chunk)
    {
        await Task.Delay(20); // Simulate chunk synthesis
        return new byte[chunk.Length * 80]; // Simulated audio chunk
    }

    public async Task<byte[]> SynthesizeWithProsodyAsync(string text, ProsodyOptions prosody)
    {
        await Task.Delay(100); // Simulate prosody processing
        return new byte[text.Length * 120]; // Simulated prosodic audio
    }

    public async Task<byte[]> SynthesizeConversationalAsync(string text, TextToSpeechOptions options)
    {
        await Task.Delay(75); // Simulate conversational processing
        return new byte[text.Length * 110]; // Simulated conversational audio
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
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
