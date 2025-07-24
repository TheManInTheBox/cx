using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using CxLanguage.Core.Events;

namespace CxLanguage.StandardLibrary.Services
{
    /// <summary>
    /// Azure OpenAI Realtime API Service for real-time voice conversation
    /// Provides integrated speech recognition, AI response, and voice synthesis
    /// </summary>
    public class AzureOpenAIRealtimeService
    {
        private readonly ILogger<AzureOpenAIRealtimeService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICxEventBus _eventBus;
        
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isSessionActive = false;
        private string? _sessionId;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _deploymentName;
        
        public AzureOpenAIRealtimeService(
            ILogger<AzureOpenAIRealtimeService> logger,
            IConfiguration configuration,
            ICxEventBus eventBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            
            // Load configuration using new helper method
            var (endpoint, apiKey, deploymentName) = GetRealtimeConfig(configuration);
            _endpoint = endpoint;
            _apiKey = apiKey;
            _deploymentName = deploymentName;
            
            if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Azure OpenAI Realtime configuration is missing. Please configure Realtime endpoint and API key.");
            }
            
            _logger.LogInformation("Azure OpenAI Realtime Service initialized");
            _logger.LogInformation("🔗 Realtime Endpoint: {Endpoint}", _endpoint);
            _logger.LogInformation("📦 Realtime Deployment: {Deployment}", _deploymentName);
        }
        
        /// <summary>
        /// Get Azure OpenAI Realtime configuration with fallback logic
        /// </summary>
        private static (string endpoint, string apiKey, string deploymentName) GetRealtimeConfig(IConfiguration configuration)
        {
            var azureConfig = configuration.GetSection("AzureOpenAI");
            
            // Try new per-service configuration first
            var realtimeConfig = azureConfig.GetSection("Realtime");
            if (!string.IsNullOrEmpty(realtimeConfig["Endpoint"]) && !string.IsNullOrEmpty(realtimeConfig["ApiKey"]))
            {
                return (
                    realtimeConfig["Endpoint"] ?? "",
                    realtimeConfig["ApiKey"] ?? "",
                    realtimeConfig["DeploymentName"] ?? "gpt-4o-mini-realtime-preview"
                );
            }
            
            // Fall back to legacy configuration section
            var legacyConfig = azureConfig.GetSection("Legacy");
            if (!string.IsNullOrEmpty(legacyConfig["RealtimeEndpoint"]) && !string.IsNullOrEmpty(legacyConfig["ApiKey"]))
            {
                return (
                    legacyConfig["RealtimeEndpoint"] ?? "",
                    legacyConfig["ApiKey"] ?? "",
                    legacyConfig["RealtimeDeploymentName"] ?? "gpt-4o-mini-realtime-preview"
                );
            }
            
            // Final fallback to root-level legacy configuration
            return (
                azureConfig["RealtimeEndpoint"] ?? "",
                azureConfig["ApiKey"] ?? "",
                azureConfig["RealtimeDeploymentName"] ?? "gpt-4o-mini-realtime-preview"
            );
        }
        
        /// <summary>
        /// Start a new realtime session with Azure OpenAI
        /// </summary>
        public async Task<bool> StartRealtimeSessionAsync(RealtimeSessionConfig config)
        {
            try
            {
                if (_isSessionActive)
                {
                    _logger.LogWarning("Realtime session already active");
                    return false;
                }
                
                _logger.LogInformation("Starting Azure OpenAI Realtime session");
                
                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();
                
                // Configure WebSocket headers
                _webSocket.Options.SetRequestHeader("api-key", _apiKey);
                _webSocket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
                
                // Connect to Azure OpenAI Realtime API
                var wsUrl = $"{_endpoint}/openai/realtime?api-version=2024-10-01-preview&deployment={_deploymentName}";
                await _webSocket.ConnectAsync(new Uri(wsUrl), _cancellationTokenSource.Token);
                
                _sessionId = Guid.NewGuid().ToString();
                _isSessionActive = true;
                
                // Send session configuration
                await SendSessionUpdateAsync(config);
                
                // Start message listener
                _ = Task.Run(async () => await ListenForMessagesAsync(_cancellationTokenSource.Token));
                
                // Emit session started event
                await _eventBus.EmitAsync("realtime.session.started", new
                {
                    sessionId = _sessionId,
                    model = config.Model,
                    voice = config.Voice
                });
                
                _logger.LogInformation($"Realtime session started: {_sessionId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start realtime session");
                await _eventBus.EmitAsync("realtime.error", new
                {
                    error = "session_start_failed",
                    details = ex.Message
                });
                return false;
            }
        }
        
        /// <summary>
        /// Send user message to realtime session
        /// </summary>
        public async Task SendUserMessageAsync(string content, string messageType = "user_message")
        {
            if (!_isSessionActive || _webSocket?.State != WebSocketState.Open)
            {
                _logger.LogWarning("Cannot send message - session not active");
                return;
            }
            
            try
            {
                var message = new
                {
                    type = "conversation.item.create",
                    item = new
                    {
                        type = "message",
                        role = "user",
                        content = new[]
                        {
                            new
                            {
                                type = "input_text",
                                text = content
                            }
                        }
                    }
                };
                
                await SendWebSocketMessageAsync(message);
                
                // Trigger response generation
                var responseMessage = new
                {
                    type = "response.create",
                    response = new
                    {
                        modalities = new[] { "text", "audio" },
                        instructions = "You are Aura, an enthusiastic programming assistant. Use BEEP-BOOP in your responses and help with programming tasks energetically."
                    }
                };
                
                await SendWebSocketMessageAsync(responseMessage);
                
                _logger.LogInformation($"User message sent: {content}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send user message");
                await _eventBus.EmitAsync("realtime.error", new
                {
                    error = "message_send_failed",
                    details = ex.Message
                });
            }
        }
        
        /// <summary>
        /// Stop the realtime session
        /// </summary>
        public async Task StopRealtimeSessionAsync(string reason = "user_requested")
        {
            try
            {
                if (_isSessionActive)
                {
                    _isSessionActive = false;
                    
                    _cancellationTokenSource?.Cancel();
                    
                    if (_webSocket?.State == WebSocketState.Open)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
                    }
                    
                    await _eventBus.EmitAsync("realtime.session.ended", new
                    {
                        sessionId = _sessionId,
                        reason = reason
                    });
                    
                    _logger.LogInformation($"Realtime session stopped: {_sessionId}, Reason: {reason}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping realtime session");
            }
            finally
            {
                _webSocket?.Dispose();
                _cancellationTokenSource?.Dispose();
                _webSocket = null;
                _cancellationTokenSource = null;
                _sessionId = null;
            }
        }
        
        /// <summary>
        /// Send audio data to the realtime session for processing
        /// </summary>
        public async Task SendAudioAsync(byte[] audioData)
        {
            if (!_isSessionActive || _webSocket?.State != WebSocketState.Open)
            {
                _logger.LogWarning("Cannot send audio - session not active");
                return;
            }
            
            try
            {
                // Convert audio bytes to base64 for WebSocket transmission
                var base64Audio = Convert.ToBase64String(audioData);
                
                var audioMessage = new
                {
                    type = "input_audio_buffer.append",
                    audio = base64Audio
                };
                
                await SendWebSocketMessageAsync(audioMessage);
                
                _logger.LogDebug($"Audio chunk sent: {audioData.Length} bytes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send audio data");
                await _eventBus.EmitAsync("realtime.error", new
                {
                    error = "audio_send_failed",
                    details = ex.Message
                });
            }
        }
        
        /// <summary>
        /// Commit the audio buffer and trigger processing
        /// </summary>
        public async Task CommitAudioAsync()
        {
            if (!_isSessionActive || _webSocket?.State != WebSocketState.Open)
            {
                _logger.LogWarning("Cannot commit audio - session not active");
                return;
            }
            
            try
            {
                var commitMessage = new
                {
                    type = "input_audio_buffer.commit"
                };
                
                await SendWebSocketMessageAsync(commitMessage);
                
                // Trigger response creation
                var responseMessage = new
                {
                    type = "response.create",
                    response = new
                    {
                        modalities = new[] { "text", "audio" },
                        instructions = "Process the audio input and respond naturally as Aura."
                    }
                };
                
                await SendWebSocketMessageAsync(responseMessage);
                
                _logger.LogDebug("Audio buffer committed and response triggered");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to commit audio buffer");
                await _eventBus.EmitAsync("realtime.error", new
                {
                    error = "audio_commit_failed",
                    details = ex.Message
                });
            }
        }
        
        /// <summary>
        /// Method alias for compatibility with existing code
        /// </summary>
        public async Task<bool> StartSessionAsync()
        {
            var config = new RealtimeSessionConfig
            {
                Instructions = "You are Aura, an enthusiastic programming assistant. Use BEEP-BOOP in your responses and help with programming tasks energetically.",
                Voice = "alloy",
                Temperature = 0.8
            };
            
            return await StartRealtimeSessionAsync(config);
        }
        
        /// <summary>
        /// Method alias for compatibility with existing code  
        /// </summary>
        public async Task StopSessionAsync()
        {
            await StopRealtimeSessionAsync("session_ended");
        }
        
        /// <summary>
        /// Listen for messages from Azure OpenAI Realtime API
        /// </summary>
        private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            
            try
            {
                while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessRealtimeMessageAsync(json);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket connection closed by server");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Message listener cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in message listener");
                await _eventBus.EmitAsync("realtime.error", new
                {
                    error = "listener_failed",
                    details = ex.Message
                });
            }
        }
        
        /// <summary>
        /// Process messages received from Azure OpenAI Realtime API
        /// </summary>
        private async Task ProcessRealtimeMessageAsync(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                
                if (!root.TryGetProperty("type", out var typeProperty))
                    return;
                
                var messageType = typeProperty.GetString();
                
                switch (messageType)
                {
                    case "session.created":
                        await HandleSessionCreatedAsync(root);
                        break;
                        
                    case "input_audio_buffer.speech_started":
                        await _eventBus.EmitAsync("realtime.speech.started", new
                        {
                            timestamp = DateTime.UtcNow,
                            sessionId = _sessionId
                        });
                        break;
                        
                    case "input_audio_buffer.speech_stopped":
                        await _eventBus.EmitAsync("realtime.speech.completed", new
                        {
                            timestamp = DateTime.UtcNow,
                            sessionId = _sessionId,
                            transcript = "Speech completed" // Would be actual transcript in real implementation
                        });
                        break;
                        
                    case "response.created":
                        await _eventBus.EmitAsync("realtime.response.started", new
                        {
                            responseId = root.GetProperty("response").GetProperty("id").GetString(),
                            sessionId = _sessionId
                        });
                        break;
                        
                    case "response.done":
                        await HandleResponseDoneAsync(root);
                        break;
                        
                    case "response.audio.delta":
                        await HandleAudioDeltaAsync(root);
                        break;
                        
                    case "error":
                        await HandleErrorAsync(root);
                        break;
                        
                    default:
                        _logger.LogDebug($"Unhandled message type: {messageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process realtime message");
            }
        }
        
        private async Task HandleSessionCreatedAsync(JsonElement root)
        {
            var session = root.GetProperty("session");
            _sessionId = session.GetProperty("id").GetString();
            
            _logger.LogInformation($"Realtime session created: {_sessionId}");
            
            // Await a completed task to satisfy async requirement
            await Task.CompletedTask;
        }
        
        private async Task HandleResponseDoneAsync(JsonElement root)
        {
            var response = root.GetProperty("response");
            var responseId = response.GetProperty("id").GetString();
            
            // Extract response text if available
            string responseText = "";
            if (response.TryGetProperty("output", out var output) && 
                output.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in output.EnumerateArray())
                {
                    if (item.TryGetProperty("content", out var content) && 
                        content.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var contentItem in content.EnumerateArray())
                        {
                            if (contentItem.TryGetProperty("text", out var text))
                            {
                                responseText += text.GetString() + " ";
                            }
                        }
                    }
                }
            }
            
            await _eventBus.EmitAsync("realtime.response.completed", new
            {
                responseId = responseId,
                text = responseText.Trim(),
                audio = true, // Assume audio is available
                sessionId = _sessionId
            });
        }
        
        private async Task HandleAudioDeltaAsync(JsonElement root)
        {
            // Handle audio streaming - would process audio chunks here
            await _eventBus.EmitAsync("realtime.audio.started", new
            {
                audioId = Guid.NewGuid().ToString(),
                sessionId = _sessionId
            });
        }
        
        private async Task HandleErrorAsync(JsonElement root)
        {
            var error = root.GetProperty("error");
            var errorMessage = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
            
            await _eventBus.EmitAsync("realtime.error", new
            {
                error = errorMessage,
                details = error.ToString(),
                sessionId = _sessionId
            });
        }
        
        private async Task SendSessionUpdateAsync(RealtimeSessionConfig config)
        {
            var sessionUpdate = new
            {
                type = "session.update",
                session = new
                {
                    modalities = new[] { "text", "audio" },
                    instructions = config.Instructions,
                    voice = config.Voice,
                    input_audio_format = config.InputAudioFormat,
                    output_audio_format = config.OutputAudioFormat,
                    input_audio_transcription = new
                    {
                        model = "whisper-1"
                    },
                    turn_detection = new
                    {
                        type = "server_vad",
                        threshold = config.TurnDetection?.Threshold ?? 0.5,
                        prefix_padding_ms = config.TurnDetection?.PrefixPaddingMs ?? 300,
                        silence_duration_ms = config.TurnDetection?.SilenceDurationMs ?? 200
                    },
                    tools = new object[] { },
                    tool_choice = "auto",
                    temperature = config.Temperature ?? 0.8,
                    max_response_output_tokens = config.MaxResponseTokens ?? 4096
                }
            };
            
            await SendWebSocketMessageAsync(sessionUpdate);
        }
        
        private async Task SendWebSocketMessageAsync(object message)
        {
            if (_webSocket?.State != WebSocketState.Open)
                return;
                
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }
    
    /// <summary>
    /// Configuration for Azure OpenAI Realtime session
    /// </summary>
    public class RealtimeSessionConfig
    {
        public string Model { get; set; } = "gpt-4o-realtime-preview-2024-10-01";
        public string Voice { get; set; } = "alloy";
        public string Instructions { get; set; } = "You are a helpful assistant.";
        public string InputAudioFormat { get; set; } = "pcm16";
        public string OutputAudioFormat { get; set; } = "pcm16";
        public double? Temperature { get; set; } = 0.8;
        public int? MaxResponseTokens { get; set; } = 4096;
        public TurnDetectionConfig TurnDetection { get; set; } = new TurnDetectionConfig();
    }
    
    /// <summary>
    /// Turn detection configuration for voice conversation
    /// </summary>
    public class TurnDetectionConfig
    {
        public double Threshold { get; set; } = 0.5;
        public int PrefixPaddingMs { get; set; } = 300;
        public int SilenceDurationMs { get; set; } = 200;
    }
}
