// 🎮 VISUALIZATION WEBSOCKET SERVER - UNREAL ENGINE BRIDGE
// Real-time streaming server for consciousness data to Unreal Engine
// Core Engineering Team - WebSocket bridge implementation

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Runtime.Events;

namespace CxLanguage.Demos
{
    /// <summary>
    /// WebSocket server for streaming consciousness data to Unreal Engine
    /// Provides real-time bridge between CX Language and Unreal Engine visualization
    /// </summary>
    public class VisualizationWebSocketServer
    {
        private readonly ILogger<VisualizationWebSocketServer> _logger;
        private HttpListener _httpListener;
        private readonly ConcurrentDictionary<string, WebSocket> _connectedClients;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private string _serverAddress;
        private int _serverPort;

        // Performance tracking
        private long _messagesSent;
        private long _bytesTransferred;
        private DateTime _startTime;

        public VisualizationWebSocketServer(ILogger<VisualizationWebSocketServer> logger)
        {
            _logger = logger;
            _connectedClients = new ConcurrentDictionary<string, WebSocket>();
            _cancellationTokenSource = new CancellationTokenSource();
            _messagesSent = 0;
            _bytesTransferred = 0;
        }

        /// <summary>
        /// Start WebSocket server for Unreal Engine connections
        /// </summary>
        public async Task StartAsync(string address, int port)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("Server is already running");
            }

            _serverAddress = address;
            _serverPort = port;
            _startTime = DateTime.UtcNow;

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://{address}:{port}/");
            _httpListener.Start();
            _isRunning = true;

            _logger.LogWarning("🌐 Consciousness visualization server started: ws://{Address}:{Port}/consciousness", address, port);

            // Start accepting connections
            _ = Task.Run(AcceptConnectionsAsync);
        }

        /// <summary>
        /// Stop WebSocket server
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cancellationTokenSource.Cancel();

            // Close all client connections
            foreach (var client in _connectedClients.Values)
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutdown", CancellationToken.None);
                }
            }

            _connectedClients.Clear();
            _httpListener?.Stop();
            _httpListener?.Close();

            _logger.LogWarning("🔌 Consciousness visualization server stopped");
        }

        /// <summary>
        /// Wait for Unreal Engine connection
        /// </summary>
        public async Task WaitForConnectionAsync(TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            
            while (_connectedClients.IsEmpty && !timeoutTask.IsCompleted)
            {
                await Task.Delay(100);
            }

            if (_connectedClients.IsEmpty)
            {
                throw new TimeoutException($"No Unreal Engine connection received within {timeout.TotalSeconds} seconds");
            }

            _logger.LogInformation("✅ Unreal Engine client connected successfully");
        }

        /// <summary>
        /// Broadcast consciousness event to all connected Unreal Engine clients
        /// </summary>
        public async Task BroadcastConsciousnessEventAsync(string eventType, Dictionary<string, object> payload)
        {
            var message = new
            {
                messageType = "consciousness_event",
                eventType = eventType,
                payload = payload,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await BroadcastMessageAsync(JsonSerializer.Serialize(message, JsonSerializerOptions.Web));
        }

        /// <summary>
        /// Broadcast synaptic update to Unreal Engine
        /// </summary>
        public async Task BroadcastSynapticUpdateAsync(string eventType, Dictionary<string, object> payload)
        {
            var message = new
            {
                messageType = "synaptic_update",
                eventType = eventType,
                payload = payload,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await BroadcastMessageAsync(JsonSerializer.Serialize(message, JsonSerializerOptions.Web));
        }

        /// <summary>
        /// Broadcast network topology update to Unreal Engine
        /// </summary>
        public async Task BroadcastTopologyUpdateAsync(string eventType, Dictionary<string, object> payload)
        {
            var message = new
            {
                messageType = "network_topology",
                eventType = eventType,
                payload = payload,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await BroadcastMessageAsync(JsonSerializer.Serialize(message, JsonSerializerOptions.Web));
        }

        /// <summary>
        /// Get server statistics
        /// </summary>
        public string GetServerStatistics()
        {
            var uptime = DateTime.UtcNow - _startTime;
            var messagesPerSecond = _messagesSent / Math.Max(uptime.TotalSeconds, 1);
            var mbTransferred = _bytesTransferred / (1024.0 * 1024.0);

            return $"Uptime: {uptime:hh\\:mm\\:ss}, " +
                   $"Clients: {_connectedClients.Count}, " +
                   $"Messages: {_messagesSent:N0} ({messagesPerSecond:F1}/sec), " +
                   $"Data: {mbTransferred:F2} MB";
        }

        /// <summary>
        /// Accept incoming WebSocket connections
        /// </summary>
        private async Task AcceptConnectionsAsync()
        {
            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    
                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = Task.Run(() => HandleWebSocketConnectionAsync(context));
                    }
                    else
                    {
                        // Return 400 for non-WebSocket requests
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Server is shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting WebSocket connection");
                }
            }
        }

        /// <summary>
        /// Handle individual WebSocket connection
        /// </summary>
        private async Task HandleWebSocketConnectionAsync(HttpListenerContext context)
        {
            WebSocket webSocket = null;
            string clientId = Guid.NewGuid().ToString();

            try
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                webSocket = webSocketContext.WebSocket;
                
                _connectedClients.TryAdd(clientId, webSocket);
                _logger.LogInformation("🔗 Unreal Engine client connected: {ClientId} from {RemoteEndpoint}", 
                    clientId, context.Request.RemoteEndPoint);

                // Send welcome message
                var welcomeMessage = new
                {
                    messageType = "welcome",
                    clientId = clientId,
                    serverVersion = "1.0.0",
                    features = new[] { "consciousness_events", "synaptic_updates", "network_topology", "performance_metrics" },
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                await SendMessageToClientAsync(webSocket, JsonSerializer.Serialize(welcomeMessage, JsonSerializerOptions.Web));

                // Handle incoming messages
                await HandleClientMessagesAsync(webSocket, clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling WebSocket connection for client {ClientId}", clientId);
            }
            finally
            {
                // Clean up client connection
                _connectedClients.TryRemove(clientId, out _);
                
                if (webSocket?.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }

                _logger.LogInformation("🔌 Unreal Engine client disconnected: {ClientId}", clientId);
            }
        }

        /// <summary>
        /// Handle incoming messages from Unreal Engine client
        /// </summary>
        private async Task HandleClientMessagesAsync(WebSocket webSocket, string clientId)
        {
            var buffer = new byte[4096];

            while (webSocket.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessClientMessageAsync(clientId, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving message from client {ClientId}", clientId);
                    break;
                }
            }
        }

        /// <summary>
        /// Process message received from Unreal Engine client
        /// </summary>
        private async Task ProcessClientMessageAsync(string clientId, string message)
        {
            try
            {
                var messageObj = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
                var messageType = messageObj.GetValueOrDefault("messageType")?.ToString();

                _logger.LogInformation("📨 Received message from {ClientId}: {MessageType}", clientId, messageType);

                switch (messageType)
                {
                    case "heartbeat":
                        await SendHeartbeatResponseAsync(clientId);
                        break;
                    case "request_state":
                        await SendCurrentStateAsync(clientId);
                        break;
                    case "performance_request":
                        await SendPerformanceDataAsync(clientId);
                        break;
                    default:
                        _logger.LogWarning("Unknown message type from client {ClientId}: {MessageType}", clientId, messageType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from client {ClientId}", clientId);
            }
        }

        /// <summary>
        /// Send heartbeat response to client
        /// </summary>
        private async Task SendHeartbeatResponseAsync(string clientId)
        {
            if (_connectedClients.TryGetValue(clientId, out var webSocket))
            {
                var response = new
                {
                    messageType = "heartbeat_response",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    serverStatus = "healthy"
                };

                await SendMessageToClientAsync(webSocket, JsonSerializer.Serialize(response, JsonSerializerOptions.Web));
            }
        }

        /// <summary>
        /// Send current consciousness state to client
        /// </summary>
        private async Task SendCurrentStateAsync(string clientId)
        {
            if (_connectedClients.TryGetValue(clientId, out var webSocket))
            {
                var state = new
                {
                    messageType = "current_state",
                    activeConnections = _connectedClients.Count,
                    serverStatistics = GetServerStatistics(),
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                await SendMessageToClientAsync(webSocket, JsonSerializer.Serialize(state, JsonSerializerOptions.Web));
            }
        }

        /// <summary>
        /// Send performance data to client
        /// </summary>
        private async Task SendPerformanceDataAsync(string clientId)
        {
            if (_connectedClients.TryGetValue(clientId, out var webSocket))
            {
                var uptime = DateTime.UtcNow - _startTime;
                var performance = new
                {
                    messageType = "performance_data",
                    messagesPerSecond = _messagesSent / Math.Max(uptime.TotalSeconds, 1),
                    totalMessages = _messagesSent,
                    totalBytes = _bytesTransferred,
                    connectedClients = _connectedClients.Count,
                    uptime = uptime.TotalSeconds,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                await SendMessageToClientAsync(webSocket, JsonSerializer.Serialize(performance, JsonSerializerOptions.Web));
            }
        }

        /// <summary>
        /// Broadcast message to all connected clients
        /// </summary>
        private async Task BroadcastMessageAsync(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var tasks = new List<Task>();

            foreach (var kvp in _connectedClients)
            {
                var clientId = kvp.Key;
                var webSocket = kvp.Value;

                if (webSocket.State == WebSocketState.Open)
                {
                    tasks.Add(SendMessageToClientAsync(webSocket, message));
                }
                else
                {
                    // Remove disconnected client
                    _connectedClients.TryRemove(clientId, out _);
                }
            }

            await Task.WhenAll(tasks);
            
            // Update statistics
            Interlocked.Increment(ref _messagesSent);
            Interlocked.Add(ref _bytesTransferred, messageBytes.Length * tasks.Count);
        }

        /// <summary>
        /// Send message to specific client
        /// </summary>
        private async Task SendMessageToClientAsync(WebSocket webSocket, string message)
        {
            try
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(messageBytes),
                    WebSocketMessageType.Text,
                    true,
                    _cancellationTokenSource.Token
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to WebSocket client");
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _httpListener?.Close();
        }
    }
}
