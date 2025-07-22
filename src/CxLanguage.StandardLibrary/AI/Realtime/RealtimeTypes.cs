using System.Text.Json.Serialization;

namespace CxLanguage.StandardLibrary.AI.Realtime;

/// <summary>
/// Configuration for Azure OpenAI Realtime API session
/// </summary>
public class RealtimeSessionConfig
{
    /// <summary>
    /// Modalities supported by the session (e.g., ["text", "audio"])
    /// </summary>
    public string[] Modalities { get; set; } = { "text", "audio" };

    /// <summary>
    /// Instructions for the AI assistant
    /// </summary>
    public string Instructions { get; set; } = "You are a helpful AI assistant.";

    /// <summary>
    /// Voice to use for audio output (e.g., "alloy", "echo", "fable")
    /// </summary>
    public string Voice { get; set; } = "alloy";

    /// <summary>
    /// Input audio format (e.g., "pcm16", "g711_ulaw", "g711_alaw")
    /// </summary>
    public string InputAudioFormat { get; set; } = "pcm16";

    /// <summary>
    /// Output audio format (e.g., "pcm16", "g711_ulaw", "g711_alaw")
    /// </summary>
    public string OutputAudioFormat { get; set; } = "pcm16";

    /// <summary>
    /// Input audio transcription settings
    /// </summary>
    public RealtimeInputAudioTranscription? InputAudioTranscription { get; set; }

    /// <summary>
    /// Turn detection settings for voice activity
    /// </summary>
    public RealtimeTurnDetection? TurnDetection { get; set; }

    /// <summary>
    /// Available tools/functions for the assistant
    /// </summary>
    public object[]? Tools { get; set; }

    /// <summary>
    /// Tool choice strategy (e.g., "auto", "none", or specific tool)
    /// </summary>
    public object? ToolChoice { get; set; }

    /// <summary>
    /// Sampling temperature for response generation
    /// </summary>
    public double Temperature { get; set; } = 0.8;

    /// <summary>
    /// Maximum number of output tokens for text responses
    /// </summary>
    public int? MaxResponseOutputTokens { get; set; }
}

/// <summary>
/// Input audio transcription configuration
/// </summary>
public class RealtimeInputAudioTranscription
{
    /// <summary>
    /// Model to use for transcription (e.g., "whisper-1")
    /// </summary>
    public string Model { get; set; } = "whisper-1";
}

/// <summary>
/// Turn detection configuration for voice activity detection
/// </summary>
public class RealtimeTurnDetection
{
    /// <summary>
    /// Type of turn detection ("server_vad" for server-side voice activity detection)
    /// </summary>
    public string Type { get; set; } = "server_vad";

    /// <summary>
    /// Activation threshold for voice activity detection (0.0 to 1.0)
    /// </summary>
    public double Threshold { get; set; } = 0.5;

    /// <summary>
    /// Amount of audio to include before speech starts (in milliseconds)
    /// </summary>
    public int PrefixPaddingMs { get; set; } = 300;

    /// <summary>
    /// Duration of silence before considering speech ended (in milliseconds)
    /// </summary>
    public int SilenceDurationMs { get; set; } = 200;
}

/// <summary>
/// Event arguments for realtime messages
/// </summary>
public class RealtimeMessageEventArgs : EventArgs
{
    public string Content { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public string MessageType { get; set; } = "text";
    public string? MessageId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Event arguments for realtime audio
/// </summary>
public class RealtimeAudioEventArgs : EventArgs
{
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public bool IsComplete { get; set; }
    public string AudioFormat { get; set; } = "pcm16";
    public int SampleRate { get; set; } = 24000;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Event arguments for realtime errors
/// </summary>
public class RealtimeErrorEventArgs : EventArgs
{
    public string Error { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? RawMessage { get; set; }
    public Exception? Exception { get; set; }
}

/// <summary>
/// Connection status for realtime API
/// </summary>
public enum RealtimeConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error,
    Reconnecting
}

/// <summary>
/// Connection info for realtime API
/// </summary>
public class RealtimeConnectionInfo
{
    public RealtimeConnectionStatus Status { get; set; } = RealtimeConnectionStatus.Disconnected;
    public string? SessionId { get; set; }
    public DateTimeOffset? ConnectedAt { get; set; }
    public TimeSpan? Uptime => ConnectedAt.HasValue ? DateTimeOffset.UtcNow - ConnectedAt.Value : null;
    public string? LastError { get; set; }
    public int ReconnectAttempts { get; set; }
}
