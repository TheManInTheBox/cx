using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace CxLanguage.StandardLibrary.AI.Realtime;

/// <summary>
/// Azure OpenAI Realtime API WebSocket client implementation
/// Provides direct integration with Azure OpenAI's Realtime API for voice processing
/// </summary>
public class AzureRealtimeApiClient : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureRealtimeApiClient> _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _pendingRequests;
    
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _receiveTask;
    private readonly object _connectionLock = new object();
    private bool _isConnected = false;
    private bool _disposed = false;

    // Azure OpenAI configuration
    private string? _endpoint;
    private string? _deploymentName;
    private string? _apiKey;
    private string? _apiVersion;

    public event EventHandler<RealtimeMessageEventArgs>? MessageReceived;
    public event EventHandler<RealtimeAudioEventArgs>? AudioReceived;
    public event EventHandler<RealtimeErrorEventArgs>? ErrorReceived;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public bool IsConnected => _isConnected && _webSocket?.State == WebSocketState.Open;

    public AzureRealtimeApiClient(IConfiguration configuration, ILogger<AzureRealtimeApiClient> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _pendingRequests = new ConcurrentDictionary<string, TaskCompletionSource<object>>();
        
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        var azureSection = _configuration.GetSection("AzureOpenAI");
        _endpoint = azureSection["RealtimeEndpoint"];
        _deploymentName = azureSection["RealtimeDeploymentName"];
        _apiKey = azureSection["ApiKey"];
        _apiVersion = azureSection["ApiVersion"];

        if (string.IsNullOrEmpty(_endpoint))
        {
            _logger.LogWarning("Azure OpenAI RealtimeEndpoint not configured, using default endpoint pattern");
            var baseEndpoint = azureSection["Endpoint"];
            if (!string.IsNullOrEmpty(baseEndpoint))
            {
                _endpoint = baseEndpoint.Replace("https://", "wss://");
            }
        }

        if (string.IsNullOrEmpty(_deploymentName))
        {
            _deploymentName = "gpt-4o-realtime-preview";
        }

        if (string.IsNullOrEmpty(_apiVersion))
        {
            _apiVersion = "2024-10-21";
        }
    }

    /// <summary>
    /// Connect to Azure OpenAI Realtime API
    /// </summary>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(AzureRealtimeApiClient));
        }

        lock (_connectionLock)
        {
            if (_isConnected)
            {
                return true;
            }
        }

        try
        {
            if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("Azure OpenAI Realtime API configuration is incomplete");
                return false;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _webSocket = new ClientWebSocket();

            // Build WebSocket URL for Azure OpenAI Realtime API
            var wsUrl = $"{_endpoint}/openai/realtime?api-version={_apiVersion}&deployment={_deploymentName}";
            
            // Add authentication header
            _webSocket.Options.SetRequestHeader("api-key", _apiKey);
            _webSocket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");

            _logger.LogInformation("Connecting to Azure OpenAI Realtime API: {Url}", wsUrl);

            await _webSocket.ConnectAsync(new Uri(wsUrl), cancellationToken);

            lock (_connectionLock)
            {
                _isConnected = true;
            }

            // Start receiving messages
            _receiveTask = ReceiveMessagesAsync(_cancellationTokenSource.Token);

            _logger.LogInformation("Successfully connected to Azure OpenAI Realtime API");
            Connected?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Azure OpenAI Realtime API");
            
            await CleanupConnectionAsync();
            return false;
        }
    }

    /// <summary>
    /// Send a session configuration message
    /// </summary>
    public async Task<bool> ConfigureSessionAsync(RealtimeSessionConfig config, CancellationToken cancellationToken = default)
    {
        var sessionUpdate = new
        {
            type = "session.update",
            session = new
            {
                modalities = config.Modalities,
                instructions = config.Instructions,
                voice = config.Voice,
                input_audio_format = config.InputAudioFormat,
                output_audio_format = config.OutputAudioFormat,
                input_audio_transcription = config.InputAudioTranscription != null ? new
                {
                    model = config.InputAudioTranscription.Model
                } : null,
                turn_detection = config.TurnDetection != null ? new
                {
                    type = config.TurnDetection.Type,
                    threshold = config.TurnDetection.Threshold,
                    prefix_padding_ms = config.TurnDetection.PrefixPaddingMs,
                    silence_duration_ms = config.TurnDetection.SilenceDurationMs
                } : null,
                tools = config.Tools,
                tool_choice = config.ToolChoice,
                temperature = config.Temperature,
                max_response_output_tokens = config.MaxResponseOutputTokens
            }
        };

        return await SendMessageAsync(sessionUpdate, cancellationToken);
    }

    /// <summary>
    /// Send audio data to the Realtime API
    /// </summary>
    public async Task<bool> SendAudioAsync(byte[] audioData, CancellationToken cancellationToken = default)
    {
        var audioMessage = new
        {
            type = "input_audio_buffer.append",
            audio = Convert.ToBase64String(audioData)
        };

        return await SendMessageAsync(audioMessage, cancellationToken);
    }

    /// <summary>
    /// Commit the audio input buffer and request response
    /// </summary>
    public async Task<bool> CommitAudioAsync(CancellationToken cancellationToken = default)
    {
        var commitMessage = new
        {
            type = "input_audio_buffer.commit"
        };

        return await SendMessageAsync(commitMessage, cancellationToken);
    }

    /// <summary>
    /// Send a text message to the Realtime API
    /// </summary>
    public async Task<bool> SendTextAsync(string text, CancellationToken cancellationToken = default)
    {
        var textMessage = new
        {
            type = "conversation.item.create",
            item = new
            {
                type = "message",
                role = "user",
                content = new[]
                {
                    new { type = "input_text", text = text }
                }
            }
        };

        var success = await SendMessageAsync(textMessage, cancellationToken);
        if (success)
        {
            // Also send response create to trigger a response
            var responseMessage = new
            {
                type = "response.create"
            };
            
            return await SendMessageAsync(responseMessage, cancellationToken);
        }

        return false;
    }

    /// <summary>
    /// Cancel the current response generation
    /// </summary>
    public async Task<bool> CancelResponseAsync(CancellationToken cancellationToken = default)
    {
        var cancelMessage = new
        {
            type = "response.cancel"
        };

        return await SendMessageAsync(cancelMessage, cancellationToken);
    }

    private async Task<bool> SendMessageAsync(object message, CancellationToken cancellationToken)
    {
        if (!IsConnected || _webSocket == null)
        {
            _logger.LogWarning("Cannot send message - not connected to Azure OpenAI Realtime API");
            return false;
        }

        try
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            _logger.LogDebug("Sent message to Azure OpenAI Realtime API: {MessageType}", message.GetType().GetProperty("type")?.GetValue(message));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to Azure OpenAI Realtime API");
            return false;
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4]; // 4KB buffer
        var messageBuilder = new StringBuilder();

        while (!cancellationToken.IsCancellationRequested && IsConnected && _webSocket != null)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Azure OpenAI Realtime API connection closed by server");
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                messageBuilder.Append(message);

                if (result.EndOfMessage)
                {
                    var fullMessage = messageBuilder.ToString();
                    messageBuilder.Clear();

                    ProcessReceivedMessage(fullMessage);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving message from Azure OpenAI Realtime API");
                
                // Trigger error event
                ErrorReceived?.Invoke(this, new RealtimeErrorEventArgs 
                { 
                    Error = ex.Message,
                    Exception = ex 
                });
                break;
            }
        }

        await CleanupConnectionAsync();
    }

    private void ProcessReceivedMessage(string messageJson)
    {
        try
        {
            using var document = JsonDocument.Parse(messageJson);
            var root = document.RootElement;

            if (!root.TryGetProperty("type", out var typeProperty))
            {
                _logger.LogWarning("Received message without type property: {Message}", messageJson);
                return;
            }

            var messageType = typeProperty.GetString();
            _logger.LogDebug("Received message type: {MessageType}", messageType);

            switch (messageType)
            {
                case "session.created":
                    _logger.LogInformation("Azure OpenAI Realtime API session created successfully");
                    break;

                case "session.updated":
                    _logger.LogInformation("Azure OpenAI Realtime API session updated successfully");
                    break;

                case "response.audio.delta":
                    if (root.TryGetProperty("delta", out var deltaProperty))
                    {
                        var audioBase64 = deltaProperty.GetString();
                        if (!string.IsNullOrEmpty(audioBase64))
                        {
                            var audioData = Convert.FromBase64String(audioBase64);
                            AudioReceived?.Invoke(this, new RealtimeAudioEventArgs
                            {
                                AudioData = audioData,
                                IsComplete = false
                            });
                        }
                    }
                    break;

                case "response.audio.done":
                    AudioReceived?.Invoke(this, new RealtimeAudioEventArgs
                    {
                        AudioData = Array.Empty<byte>(),
                        IsComplete = true
                    });
                    break;

                case "response.text.delta":
                    if (root.TryGetProperty("delta", out var textDeltaProperty))
                    {
                        var text = textDeltaProperty.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            MessageReceived?.Invoke(this, new RealtimeMessageEventArgs
                            {
                                Content = text,
                                IsComplete = false,
                                MessageType = "text"
                            });
                        }
                    }
                    break;

                case "response.text.done":
                    MessageReceived?.Invoke(this, new RealtimeMessageEventArgs
                    {
                        Content = "",
                        IsComplete = true,
                        MessageType = "text"
                    });
                    break;

                case "error":
                    var errorMessage = "Unknown error";
                    if (root.TryGetProperty("error", out var errorProperty) &&
                        errorProperty.TryGetProperty("message", out var errorMessageProperty))
                    {
                        errorMessage = errorMessageProperty.GetString() ?? errorMessage;
                    }

                    _logger.LogError("Azure OpenAI Realtime API error: {Error}", errorMessage);
                    ErrorReceived?.Invoke(this, new RealtimeErrorEventArgs
                    {
                        Error = errorMessage,
                        RawMessage = messageJson
                    });
                    break;

                case "input_audio_buffer.speech_started":
                    _logger.LogDebug("Speech started detection");
                    break;

                case "input_audio_buffer.speech_stopped":
                    _logger.LogDebug("Speech stopped detection");
                    break;

                case "conversation.item.created":
                    _logger.LogDebug("Conversation item created");
                    break;

                case "response.created":
                    _logger.LogDebug("Response generation started");
                    break;

                case "response.done":
                    _logger.LogDebug("Response generation completed");
                    break;

                default:
                    _logger.LogDebug("Unhandled message type: {MessageType}", messageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process received message: {Message}", messageJson);
        }
    }

    /// <summary>
    /// Disconnect from the Realtime API
    /// </summary>
    public async Task DisconnectAsync()
    {
        await CleanupConnectionAsync();
    }

    private async Task CleanupConnectionAsync()
    {
        lock (_connectionLock)
        {
            _isConnected = false;
        }

        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        if (_webSocket != null)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing WebSocket connection");
            }
            finally
            {
                _webSocket.Dispose();
                _webSocket = null;
            }
        }

        if (_receiveTask != null)
        {
            try
            {
                await _receiveTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error waiting for receive task to complete");
            }
            finally
            {
                _receiveTask = null;
            }
        }

        Disconnected?.Invoke(this, EventArgs.Empty);
        _logger.LogInformation("Disconnected from Azure OpenAI Realtime API");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            DisconnectAsync().GetAwaiter().GetResult();
            _disposed = true;
        }
    }
}
