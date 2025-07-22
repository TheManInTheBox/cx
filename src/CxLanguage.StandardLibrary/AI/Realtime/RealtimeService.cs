using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Collections.Concurrent;

namespace CxLanguage.StandardLibrary.AI.Realtime;

/// <summary>
/// Azure OpenAI Realtime service for CX standard library
/// Provides voice-controlled cognitive programming with real-time AI processing
/// </summary>
public class RealtimeService : CxAiServiceBase
{
    private readonly AzureRealtimeApiClient _apiClient;
    private readonly ConcurrentDictionary<string, RealtimeSession> _activeSessions;
    private readonly Timer _heartbeatTimer;
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public RealtimeService(Kernel kernel, ILogger<RealtimeService> logger, IConfiguration configuration, ILoggerFactory loggerFactory) 
        : base(kernel, logger)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _apiClient = new AzureRealtimeApiClient(configuration, _loggerFactory.CreateLogger<AzureRealtimeApiClient>());
        _activeSessions = new ConcurrentDictionary<string, RealtimeSession>();
        _connectionSemaphore = new SemaphoreSlim(10, 10); // Max 10 concurrent sessions for realtime
        
        // Heartbeat timer to maintain session health
        _heartbeatTimer = new Timer(ProcessHeartbeat, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        
        // Subscribe to API client events
        _apiClient.MessageReceived += OnMessageReceived;
        _apiClient.AudioReceived += OnAudioReceived;
        _apiClient.ErrorReceived += OnErrorReceived;
        _apiClient.Connected += OnConnected;
        _apiClient.Disconnected += OnDisconnected;
    }

    public override string ServiceName => "AzureRealtimeService";
    public override string Version => "1.0.0";
    
    /// <summary>
    /// Connection status of the Azure OpenAI Realtime API
    /// </summary>
    public bool IsConnected => _apiClient.IsConnected;

    /// <summary>
    /// Connect to Azure OpenAI Realtime API
    /// </summary>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Connecting to Azure OpenAI Realtime API...");
        
        var connected = await _apiClient.ConnectAsync(cancellationToken);
        
        if (connected)
        {
            // Configure default session settings
            var config = new RealtimeSessionConfig
            {
                Modalities = new[] { "text", "audio" },
                Instructions = "You are a helpful AI assistant for the CX programming language. Respond naturally and help with cognitive programming tasks.",
                Voice = "alloy",
                InputAudioFormat = "pcm16",
                OutputAudioFormat = "pcm16",
                Temperature = 0.8,
                TurnDetection = new RealtimeTurnDetection
                {
                    Type = "server_vad",
                    Threshold = 0.5,
                    PrefixPaddingMs = 300,
                    SilenceDurationMs = 500
                }
            };
            
            await _apiClient.ConfigureSessionAsync(config, cancellationToken);
            _logger.LogInformation("Azure OpenAI Realtime API connected and configured successfully");
        }
        
        return connected;
    }

    /// <summary>
    /// Disconnect from Azure OpenAI Realtime API
    /// </summary>
    public async Task DisconnectAsync()
    {
        _logger.LogInformation("Disconnecting from Azure OpenAI Realtime API...");
        await _apiClient.DisconnectAsync();
    }

    /// <summary>
    /// Send text message to Azure OpenAI Realtime API
    /// </summary>
    public async Task<bool> SendTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot send text - not connected to Azure OpenAI Realtime API");
            return false;
        }

        _logger.LogDebug("Sending text message: {Text}", text);
        return await _apiClient.SendTextAsync(text, cancellationToken);
    }

    /// <summary>
    /// Send audio data to Azure OpenAI Realtime API
    /// </summary>
    public async Task<bool> SendAudioAsync(byte[] audioData, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot send audio - not connected to Azure OpenAI Realtime API");
            return false;
        }

        return await _apiClient.SendAudioAsync(audioData, cancellationToken);
    }

    /// <summary>
    /// Commit audio buffer and trigger response generation
    /// </summary>
    public async Task<bool> CommitAudioAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot commit audio - not connected to Azure OpenAI Realtime API");
            return false;
        }

        return await _apiClient.CommitAudioAsync(cancellationToken);
    }

    // Event handlers for Azure OpenAI Realtime API events
    private void OnMessageReceived(object? sender, RealtimeMessageEventArgs e)
    {
        _logger.LogDebug("Received message: {Content} (Complete: {IsComplete})", e.Content, e.IsComplete);
        
        // Here you can emit CX events or handle the message as needed
        // For now, just log the received content
    }

    private void OnAudioReceived(object? sender, RealtimeAudioEventArgs e)
    {
        _logger.LogDebug("Received audio chunk: {Size} bytes (Complete: {IsComplete})", e.AudioData.Length, e.IsComplete);
        
        // Here you can handle audio playback or emit CX audio events
    }

    private void OnErrorReceived(object? sender, RealtimeErrorEventArgs e)
    {
        _logger.LogError("Azure OpenAI Realtime API error: {Error}", e.Error);
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        _logger.LogInformation("Connected to Azure OpenAI Realtime API");
    }

    private void OnDisconnected(object? sender, EventArgs e)
    {
        _logger.LogInformation("Disconnected from Azure OpenAI Realtime API");
    }

    /// <summary>
    /// Start a real-time AI conversation session with Azure OpenAI
    /// </summary>
    public async Task<RealtimeSessionResult> StartSessionAsync(RealtimeOptions? options = null)
    {
        var result = new RealtimeSessionResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            await _connectionSemaphore.WaitAsync();

            var sessionId = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting Azure OpenAI realtime session: {SessionId}", sessionId);

            // Connect to Azure OpenAI Realtime API if not already connected
            if (!IsConnected)
            {
                var connected = await ConnectAsync();
                if (!connected)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Failed to connect to Azure OpenAI Realtime API";
                    result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
                    return result;
                }
            }

            // Create session entry
            var session = new RealtimeSession(sessionId, options ?? new RealtimeOptions(), _loggerFactory.CreateLogger<RealtimeSession>());
            await session.InitializeAsync();

            _activeSessions[sessionId] = session;

            result.IsSuccess = true;
            result.SessionId = sessionId;
            result.ConnectionUrl = "Azure OpenAI Realtime API";
            result.MaxLatency = options?.MaxLatency ?? TimeSpan.FromMilliseconds(200);
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Azure OpenAI realtime session started successfully: {SessionId}", sessionId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting Azure OpenAI realtime session");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Send message in real-time session with streaming response
    /// </summary>
    public async IAsyncEnumerable<RealtimeMessageResult> SendMessageStreamAsync(
        string sessionId,
        string message,
        RealtimeMessageOptions? options = null)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session))
        {
            yield return new RealtimeMessageResult
            {
                IsSuccess = false,
                ErrorMessage = $"Session not found: {sessionId}",
                SessionId = sessionId
            };
            yield break;
        }

        var startTime = DateTimeOffset.UtcNow;
        var messageId = Guid.NewGuid().ToString();

        _logger.LogInformation("Processing realtime message in session {SessionId}", sessionId);

        await foreach (var chunk in ProcessMessageInternalAsync(session, message, messageId, options, startTime))
        {
            yield return chunk;
        }
    }

    private async IAsyncEnumerable<RealtimeMessageResult> ProcessMessageInternalAsync(
        RealtimeSession session,
        string message,
        string messageId,
        RealtimeMessageOptions? options,
        DateTimeOffset startTime)
    {
        await foreach (var chunk in session.ProcessMessageStreamAsync(message, messageId, options))
        {
            yield return new RealtimeMessageResult
            {
                IsSuccess = true,
                SessionId = session.SessionId,
                MessageId = messageId,
                Content = chunk.Content,
                ContentType = chunk.ContentType,
                IsPartial = !chunk.IsComplete,
                Latency = DateTimeOffset.UtcNow - startTime,
                ExecutionTime = DateTimeOffset.UtcNow - startTime
            };

            if (chunk.IsComplete)
                break;
        }
    }

    /// <summary>
    /// Real-time audio processing with streaming
    /// </summary>
    public async IAsyncEnumerable<RealtimeAudioResult> ProcessAudioStreamAsync(
        string sessionId,
        IAsyncEnumerable<byte[]> audioStream,
        RealtimeAudioOptions? options = null)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session))
        {
            yield return new RealtimeAudioResult
            {
                IsSuccess = false,
                ErrorMessage = $"Session not found: {sessionId}",
                SessionId = sessionId
            };
            yield break;
        }

        var startTime = DateTimeOffset.UtcNow;

        _logger.LogInformation("Processing realtime audio stream in session {SessionId}", sessionId);

        await foreach (var result in ProcessAudioInternalAsync(session, audioStream, options, startTime))
        {
            yield return result;
        }
    }

    private async IAsyncEnumerable<RealtimeAudioResult> ProcessAudioInternalAsync(
        RealtimeSession session,
        IAsyncEnumerable<byte[]> audioStream,
        RealtimeAudioOptions? options,
        DateTimeOffset startTime)
    {
        await foreach (var audioChunk in audioStream)
        {
            var result = await session.ProcessAudioChunkAsync(audioChunk, options);
            
            yield return new RealtimeAudioResult
            {
                IsSuccess = true,
                SessionId = session.SessionId,
                AudioData = result.AudioData,
                Transcription = result.Transcription,
                SpeechDetected = result.SpeechDetected,
                Confidence = result.Confidence,
                Latency = DateTimeOffset.UtcNow - startTime,
                ExecutionTime = DateTimeOffset.UtcNow - startTime
            };
        }
    }

    /// <summary>
    /// Real-time function calling with immediate execution
    /// </summary>
    public async Task<RealtimeFunctionResult> CallFunctionAsync(
        string sessionId,
        string functionName,
        object[] parameters,
        RealtimeFunctionOptions? options = null)
    {
        var result = new RealtimeFunctionResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                throw new InvalidOperationException($"Session not found: {sessionId}");
            }

            _logger.LogInformation("Calling realtime function {Function} in session {SessionId}", functionName, sessionId);

            var functionResult = await session.CallFunctionAsync(functionName, parameters, options);

            result.IsSuccess = true;
            result.SessionId = sessionId;
            result.FunctionName = functionName;
            result.Result = functionResult;
            result.Latency = DateTimeOffset.UtcNow - startTime;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling realtime function {Function} in session {SessionId}", functionName, sessionId);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.SessionId = sessionId;
            result.FunctionName = functionName;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Get real-time session status and metrics
    /// </summary>
    public async Task<RealtimeStatusResult> GetSessionStatusAsync(string sessionId)
    {
        var result = new RealtimeStatusResult();

        try
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Session not found: {sessionId}";
                return result;
            }

            var status = await session.GetStatusAsync();

            result.IsSuccess = true;
            result.SessionId = sessionId;
            result.Status = status.Status;
            result.UpTime = DateTimeOffset.UtcNow - status.StartTime;
            result.MessageCount = status.MessageCount;
            result.AverageLatency = status.AverageLatency;
            result.LastActivity = status.LastActivity;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session status for {SessionId}", sessionId);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.SessionId = sessionId;
            return result;
        }
    }

    /// <summary>
    /// Close a real-time session
    /// </summary>
    public async Task<RealtimeCloseResult> CloseSessionAsync(string sessionId)
    {
        var result = new RealtimeCloseResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Closing realtime session: {SessionId}", sessionId);

            if (_activeSessions.TryRemove(sessionId, out var session))
            {
                await session.CloseAsync();
                session.Dispose();
            }

            result.IsSuccess = true;
            result.SessionId = sessionId;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Realtime session closed: {SessionId}", sessionId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing session {SessionId}", sessionId);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.SessionId = sessionId;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    private void ProcessHeartbeat(object? state)
    {
        try
        {
            var sessionsToRemove = new List<string>();

            foreach (var kvp in _activeSessions)
            {
                var session = kvp.Value;
                if (session.IsExpired || session.HasErrors)
                {
                    sessionsToRemove.Add(kvp.Key);
                }
                else
                {
                    _ = Task.Run(async () => await session.SendHeartbeatAsync());
                }
            }

            foreach (var sessionId in sessionsToRemove)
            {
                if (_activeSessions.TryRemove(sessionId, out var session))
                {
                    _logger.LogInformation("Removing expired session: {SessionId}", sessionId);
                    session.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in heartbeat processing");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _heartbeatTimer?.Dispose();
            
            // Dispose Azure OpenAI client
            _apiClient?.Dispose();
            
            foreach (var session in _activeSessions.Values)
            {
                try
                {
                    session.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing session during service cleanup");
                }
            }
            
            _activeSessions.Clear();
            _connectionSemaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Internal real-time session management
/// </summary>
internal class RealtimeSession : IDisposable
{
    public string SessionId { get; }
    public bool IsExpired => DateTimeOffset.UtcNow - _startTime > _options.SessionTimeout;
    public bool HasErrors { get; private set; }

    private readonly RealtimeOptions _options;
    private readonly ILogger _logger;
    private readonly DateTimeOffset _startTime;
    private int _messageCount;
    private DateTimeOffset _lastActivity;
    private readonly List<TimeSpan> _latencyHistory;
    private bool _disposed;

    public RealtimeSession(string sessionId, RealtimeOptions options, ILogger logger)
    {
        SessionId = sessionId;
        _options = options;
        _logger = logger;
        _startTime = DateTimeOffset.UtcNow;
        _lastActivity = _startTime;
        _latencyHistory = new List<TimeSpan>();
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(10); // Simulate initialization
    }

    public async IAsyncEnumerable<RealtimeChunk> ProcessMessageStreamAsync(
        string message, 
        string messageId,
        RealtimeMessageOptions? options)
    {
        _messageCount++;
        _lastActivity = DateTimeOffset.UtcNow;

        // Simulate streaming response
        var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var responseWords = words.Select(w => $"processed_{w}").ToArray();

        for (int i = 0; i < responseWords.Length; i++)
        {
            await Task.Delay(10); // Simulate real-time processing
            
            yield return new RealtimeChunk
            {
                Content = responseWords[i] + " ",
                ContentType = "text/plain",
                IsComplete = i == responseWords.Length - 1
            };
        }
    }

    public async Task<RealtimeAudioChunkResult> ProcessAudioChunkAsync(
        byte[] audioChunk,
        RealtimeAudioOptions? options)
    {
        await Task.Delay(5); // Simulate audio processing

        return new RealtimeAudioChunkResult
        {
            AudioData = new byte[audioChunk.Length / 2], // Simulated processed audio
            Transcription = $"audio_chunk_{audioChunk.Length}",
            SpeechDetected = audioChunk.Length > 1000,
            Confidence = 0.85f
        };
    }

    public async Task<object> CallFunctionAsync(
        string functionName,
        object[] parameters,
        RealtimeFunctionOptions? options)
    {
        await Task.Delay(20); // Simulate function execution
        return $"function_{functionName}_result";
    }

    public async Task<RealtimeSessionStatus> GetStatusAsync()
    {
        await Task.CompletedTask;
        
        return new RealtimeSessionStatus
        {
            Status = "active",
            StartTime = _startTime,
            MessageCount = _messageCount,
            LastActivity = _lastActivity,
            AverageLatency = _latencyHistory.Count > 0 ? 
                TimeSpan.FromMilliseconds(_latencyHistory.Average(l => l.TotalMilliseconds)) : 
                TimeSpan.Zero
        };
    }

    public async Task SendHeartbeatAsync()
    {
        await Task.Delay(1); // Simulate heartbeat
        _lastActivity = DateTimeOffset.UtcNow;
    }

    public async Task CloseAsync()
    {
        await Task.Delay(5); // Simulate cleanup
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}

// Supporting classes and results follow the same pattern as previous services...
// (Truncated for brevity, but would include all the result classes, options, etc.)

/// <summary>
/// Options for realtime operations
/// </summary>
public class RealtimeOptions : CxAiOptions
{
    public TimeSpan MaxLatency { get; set; } = TimeSpan.FromMilliseconds(100);
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(30);
    public int MaxConcurrentConnections { get; set; } = 100;
    public bool EnableAudioProcessing { get; set; } = true;
    public bool EnableFunctionCalling { get; set; } = true;
    public string? PreferredModel { get; set; }
}

public class RealtimeMessageOptions : CxAiOptions
{
    public string? ContentType { get; set; }
    public bool StreamResponse { get; set; } = true;
    public TimeSpan? ResponseTimeout { get; set; }
}

public class RealtimeAudioOptions : CxAiOptions
{
    public string? AudioFormat { get; set; }
    public int? SampleRate { get; set; }
    public bool EnableTranscription { get; set; } = true;
    public bool EnableSpeechDetection { get; set; } = true;
}

public class RealtimeFunctionOptions : CxAiOptions
{
    public TimeSpan? ExecutionTimeout { get; set; }
    public bool EnableStreaming { get; set; } = false;
}

// Result classes
public class RealtimeSessionResult : CxAiResult
{
    public string SessionId { get; set; } = string.Empty;
    public string ConnectionUrl { get; set; } = string.Empty;
    public TimeSpan MaxLatency { get; set; }
}

public class RealtimeMessageResult : CxAiResult
{
    public string SessionId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public bool IsPartial { get; set; }
    public TimeSpan Latency { get; set; }
}

/// <summary>
/// Result from realtime audio processing operations
/// </summary>
public class RealtimeAudioResult : CxAiResult
{
    /// <summary>
    /// Unique identifier for the realtime session
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Audio data bytes from the realtime processing
    /// </summary>
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Transcribed text from the audio input
    /// </summary>
    public string Transcription { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates whether speech was detected in the audio
    /// </summary>
    public bool SpeechDetected { get; set; }
    
    /// <summary>
    /// Confidence score for the speech detection (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; set; }
    
    /// <summary>
    /// Processing latency for the audio operation
    /// </summary>
    public TimeSpan Latency { get; set; }
}

/// <summary>
/// Result from realtime function execution operations
/// </summary>
public class RealtimeFunctionResult : CxAiResult
{
    /// <summary>
    /// Unique identifier for the realtime session
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the function that was executed
    /// </summary>
    public string FunctionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Result returned by the executed function
    /// </summary>
    public object? Result { get; set; }
    
    /// <summary>
    /// Processing latency for the function execution
    /// </summary>
    public TimeSpan Latency { get; set; }
}

/// <summary>
/// Result containing status information for realtime operations
/// </summary>
public class RealtimeStatusResult : CxAiResult
{
    /// <summary>
    /// Unique identifier for the realtime session
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the realtime session
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Duration the session has been active
    /// </summary>
    public TimeSpan UpTime { get; set; }
    
    /// <summary>
    /// Total number of messages processed in the session
    /// </summary>
    public int MessageCount { get; set; }
    
    /// <summary>
    /// Average processing latency across all operations
    /// </summary>
    public TimeSpan AverageLatency { get; set; }
    
    /// <summary>
    /// Timestamp of the last activity in the session
    /// </summary>
    public DateTimeOffset LastActivity { get; set; }
}

/// <summary>
/// Result from realtime session close operations
/// </summary>
public class RealtimeCloseResult : CxAiResult
{
    /// <summary>
    /// Unique identifier for the closed realtime session
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
}

// Supporting internal classes
internal class RealtimeChunk
{
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
}

internal class RealtimeAudioChunkResult
{
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string Transcription { get; set; } = string.Empty;
    public bool SpeechDetected { get; set; }
    public float Confidence { get; set; }
}

internal class RealtimeSessionStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public int MessageCount { get; set; }
    public DateTimeOffset LastActivity { get; set; }
    public TimeSpan AverageLatency { get; set; }
}
