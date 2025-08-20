using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.UnityServer.Unity.Synchronization;

namespace CxLanguage.UnityServer.Unity.Protocols
{
    /// <summary>
    /// Unity Protocol Manager - Concrete implementation for Unity-CX communication
    /// Handles WebSocket and gRPC protocols for real-time Unity synchronization
    /// </summary>
    public class UnityProtocolManager : IUnityProtocolManager
    {
        private readonly ILogger<UnityProtocolManager> _logger;
        private readonly ConcurrentDictionary<string, UnityClientConnection> _connectedClients;
        private readonly WebSocketServer _webSocketServer;
        private readonly GrpcServer _grpcServer;
        private bool _isListening = false;

        public UnityProtocolManager(ILogger<UnityProtocolManager> logger)
        {
            _logger = logger;
            _connectedClients = new ConcurrentDictionary<string, UnityClientConnection>();
            _webSocketServer = new WebSocketServer(_logger);
            _grpcServer = new GrpcServer(_logger);
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("🌐 Initializing Unity Protocol Manager");
            
            try
            {
                await _webSocketServer.InitializeAsync();
                await _grpcServer.InitializeAsync();
                
                // Subscribe to client connection events
                _webSocketServer.OnClientConnected += OnWebSocketClientConnected;
                _webSocketServer.OnClientDisconnected += OnWebSocketClientDisconnected;
                _webSocketServer.OnMessageReceived += OnWebSocketMessageReceived;
                
                _logger.LogInformation("✅ Unity Protocol Manager initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize Unity Protocol Manager");
                throw;
            }
        }

        public async Task StartListeningAsync(int port, string protocolType)
        {
            _logger.LogInformation("🎧 Starting Unity protocol listener on port {Port} ({Protocol})", port, protocolType);

            try
            {
                switch (protocolType.ToLower())
                {
                    case "websocket":
                        await _webSocketServer.StartAsync(port);
                        break;
                    case "grpc":
                        await _grpcServer.StartAsync(port);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported protocol type: {protocolType}");
                }

                _isListening = true;
                _logger.LogInformation("✅ Unity protocol listener started on port {Port}", port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to start Unity protocol listener on port {Port}", port);
                throw;
            }
        }

        public async Task StopListeningAsync()
        {
            _logger.LogInformation("🛑 Stopping Unity protocol listeners");

            try
            {
                await _webSocketServer.StopAsync();
                await _grpcServer.StopAsync();
                
                // Disconnect all clients
                foreach (var client in _connectedClients.Values)
                {
                    await DisconnectClientAsync(client.ClientId);
                }
                
                _connectedClients.Clear();
                _isListening = false;
                
                _logger.LogInformation("✅ Unity protocol listeners stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error stopping Unity protocol listeners");
            }
        }

        public async Task SendUpdateToClientAsync(string clientId, ConsciousnessUpdate update)
        {
            _logger.LogDebug("📤 Sending consciousness update to Unity client: {ClientId}", clientId);

            try
            {
                if (_connectedClients.TryGetValue(clientId, out var client))
                {
                    var message = new UnityProtocolMessage
                    {
                        MessageType = "consciousness_update",
                        ClientId = clientId,
                        Data = update,
                        Timestamp = DateTime.UtcNow
                    };

                    await client.SendMessageAsync(message);
                    _logger.LogDebug("✅ Consciousness update sent to client: {ClientId}", clientId);
                }
                else
                {
                    _logger.LogWarning("⚠️ Client not found for consciousness update: {ClientId}", clientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending consciousness update to client: {ClientId}", clientId);
            }
        }

        public async Task SendCommandToClientAsync(string clientId, UnityCommand command)
        {
            _logger.LogDebug("⚡ Sending Unity command to client: {ClientId} - {CommandType}", clientId, command.CommandType);

            try
            {
                if (_connectedClients.TryGetValue(clientId, out var client))
                {
                    var message = new UnityProtocolMessage
                    {
                        MessageType = "unity_command",
                        ClientId = clientId,
                        Data = command,
                        Timestamp = DateTime.UtcNow
                    };

                    await client.SendMessageAsync(message);
                    _logger.LogDebug("✅ Unity command sent to client: {ClientId}", clientId);
                }
                else
                {
                    _logger.LogWarning("⚠️ Client not found for Unity command: {ClientId}", clientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending Unity command to client: {ClientId}", clientId);
            }
        }

        public async Task DisconnectClientAsync(string clientId)
        {
            _logger.LogInformation("🔌 Disconnecting Unity client: {ClientId}", clientId);

            try
            {
                if (_connectedClients.TryRemove(clientId, out var client))
                {
                    await client.DisconnectAsync();
                    _logger.LogInformation("✅ Unity client disconnected: {ClientId}", clientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disconnecting Unity client: {ClientId}", clientId);
            }
        }

        public async Task<List<string>> GetConnectedClientsAsync()
        {
            return await Task.FromResult(_connectedClients.Keys.ToList());
        }

        // WebSocket event handlers

        private async Task OnWebSocketClientConnected(string clientId, WebSocket webSocket)
        {
            _logger.LogInformation("🔌 Unity WebSocket client connected: {ClientId}", clientId);

            try
            {
                var connection = new UnityClientConnection(clientId, webSocket, _logger);
                _connectedClients.TryAdd(clientId, connection);

                // Send welcome message to client
                var welcomeMessage = new UnityProtocolMessage
                {
                    MessageType = "connection_established",
                    ClientId = clientId,
                    Data = new { Status = "Connected", ServerVersion = "1.0.0" },
                    Timestamp = DateTime.UtcNow
                };

                await connection.SendMessageAsync(welcomeMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling WebSocket client connection: {ClientId}", clientId);
            }
        }

        private async Task OnWebSocketClientDisconnected(string clientId)
        {
            _logger.LogInformation("🔌 Unity WebSocket client disconnected: {ClientId}", clientId);

            _connectedClients.TryRemove(clientId, out _);
            await Task.CompletedTask;
        }

        private async Task OnWebSocketMessageReceived(string clientId, string messageJson)
        {
            _logger.LogDebug("📥 Received message from Unity client: {ClientId}", clientId);

            try
            {
                var message = JsonSerializer.Deserialize<UnityProtocolMessage>(messageJson);
                if (message != null)
                {
                    await ProcessUnityMessage(clientId, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing message from Unity client: {ClientId}", clientId);
            }
        }

        private async Task ProcessUnityMessage(string clientId, UnityProtocolMessage message)
        {
            switch (message.MessageType.ToLower())
            {
                case "scene_change":
                    await ProcessSceneChangeMessage(clientId, message);
                    break;
                case "client_registration":
                    await ProcessClientRegistrationMessage(clientId, message);
                    break;
                case "hot_reload_request":
                    await ProcessHotReloadRequestMessage(clientId, message);
                    break;
                case "heartbeat":
                    await ProcessHeartbeatMessage(clientId, message);
                    break;
                default:
                    _logger.LogWarning("⚠️ Unknown message type from Unity client {ClientId}: {MessageType}", 
                        clientId, message.MessageType);
                    break;
            }
        }

        private async Task ProcessSceneChangeMessage(string clientId, UnityProtocolMessage message)
        {
            try
            {
                if (message.Data is JsonElement dataElement)
                {
                    var sceneChange = JsonSerializer.Deserialize<UnitySceneChange>(dataElement.GetRawText());
                    if (sceneChange != null)
                    {
                        sceneChange.ClientId = clientId;
                        
                        // Forward to Unity Synchronization Manager via event
                        OnSceneChangeReceived?.Invoke(sceneChange);
                        
                        _logger.LogDebug("✅ Processed scene change from Unity client: {ClientId}", clientId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing scene change from Unity client: {ClientId}", clientId);
            }
        }

        private async Task ProcessClientRegistrationMessage(string clientId, UnityProtocolMessage message)
        {
            try
            {
                if (message.Data is JsonElement dataElement)
                {
                    var clientInfo = JsonSerializer.Deserialize<UnityClientInfo>(dataElement.GetRawText());
                    if (clientInfo != null)
                    {
                        // Forward to Unity Synchronization Manager via event
                        OnClientRegistrationReceived?.Invoke(clientId, clientInfo);
                        
                        _logger.LogDebug("✅ Processed client registration from Unity client: {ClientId}", clientId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing client registration from Unity client: {ClientId}", clientId);
            }
        }

        private async Task ProcessHotReloadRequestMessage(string clientId, UnityProtocolMessage message)
        {
            try
            {
                if (message.Data is JsonElement dataElement)
                {
                    var hotReloadRequest = JsonSerializer.Deserialize<HotReloadRequest>(dataElement.GetRawText());
                    if (hotReloadRequest != null)
                    {
                        hotReloadRequest.ClientId = clientId;
                        
                        // Forward to Unity Synchronization Manager via event
                        OnHotReloadRequestReceived?.Invoke(clientId, hotReloadRequest);
                        
                        _logger.LogDebug("✅ Processed hot-reload request from Unity client: {ClientId}", clientId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing hot-reload request from Unity client: {ClientId}", clientId);
            }
        }

        private async Task ProcessHeartbeatMessage(string clientId, UnityProtocolMessage message)
        {
            // Update client last activity
            if (_connectedClients.TryGetValue(clientId, out var client))
            {
                client.UpdateLastActivity();
            }

            // Send heartbeat response
            var response = new UnityProtocolMessage
            {
                MessageType = "heartbeat_response",
                ClientId = clientId,
                Data = new { Timestamp = DateTime.UtcNow },
                Timestamp = DateTime.UtcNow
            };

            await SendMessageToClient(clientId, response);
        }

        private async Task SendMessageToClient(string clientId, UnityProtocolMessage message)
        {
            if (_connectedClients.TryGetValue(clientId, out var client))
            {
                await client.SendMessageAsync(message);
            }
        }

        // Events for Unity Synchronization Manager integration
        public event Action<UnitySceneChange>? OnSceneChangeReceived;
        public event Action<string, UnityClientInfo>? OnClientRegistrationReceived;
        public event Action<string, HotReloadRequest>? OnHotReloadRequestReceived;
    }

    /// <summary>
    /// Unity client connection wrapper
    /// </summary>
    public class UnityClientConnection
    {
        public string ClientId { get; }
        public DateTime ConnectedAt { get; }
        public DateTime LastActivity { get; private set; }
        
        private readonly WebSocket _webSocket;
        private readonly ILogger _logger;

        public UnityClientConnection(string clientId, WebSocket webSocket, ILogger logger)
        {
            ClientId = clientId;
            ConnectedAt = DateTime.UtcNow;
            LastActivity = DateTime.UtcNow;
            _webSocket = webSocket;
            _logger = logger;
        }

        public async Task SendMessageAsync(UnityProtocolMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);
                
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending message to Unity client: {ClientId}", ClientId);
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Server disconnect",
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disconnecting Unity client: {ClientId}", ClientId);
            }
        }

        public void UpdateLastActivity()
        {
            LastActivity = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Unity protocol message structure
    /// </summary>
    public class UnityProtocolMessage
    {
        public string MessageType { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Simple WebSocket server implementation
    /// </summary>
    public class WebSocketServer
    {
        private readonly ILogger _logger;
        private System.Net.HttpListener? _httpListener;
        private bool _isListening = false;
        private CancellationTokenSource? _cancellationTokenSource;

        public event Func<string, WebSocket, Task>? OnClientConnected;
        public event Func<string, Task>? OnClientDisconnected;
        public event Func<string, string, Task>? OnMessageReceived;

        public WebSocketServer(ILogger logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task StartAsync(int port)
        {
            _logger.LogInformation("🌐 Starting WebSocket server on port {Port}", port);

            try
            {
                _httpListener = new System.Net.HttpListener();
                _httpListener.Prefixes.Add($"http://localhost:{port}/");
                _httpListener.Start();
                
                _cancellationTokenSource = new CancellationTokenSource();
                _isListening = true;

                // Start accepting connections
                _ = Task.Run(AcceptClientsAsync, _cancellationTokenSource.Token);

                _logger.LogInformation("✅ WebSocket server started on port {Port}", port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to start WebSocket server on port {Port}", port);
                throw;
            }
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("🛑 Stopping WebSocket server");

            _isListening = false;
            _cancellationTokenSource?.Cancel();
            _httpListener?.Stop();
            _httpListener?.Close();

            await Task.CompletedTask;
        }

        private async Task AcceptClientsAsync()
        {
            while (_isListening && _httpListener != null)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    
                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = Task.Run(() => HandleWebSocketConnection(context));
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (Exception ex) when (_isListening)
                {
                    _logger.LogError(ex, "❌ Error accepting WebSocket connection");
                }
            }
        }

        private async Task HandleWebSocketConnection(System.Net.HttpListenerContext context)
        {
            string clientId = Guid.NewGuid().ToString();
            WebSocket? webSocket = null;

            try
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                webSocket = webSocketContext.WebSocket;

                _logger.LogInformation("🔌 WebSocket client connected: {ClientId}", clientId);
                
                // Notify connection
                if (OnClientConnected != null)
                {
                    await OnClientConnected(clientId, webSocket);
                }

                // Start message loop
                await ProcessWebSocketMessages(clientId, webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling WebSocket connection: {ClientId}", clientId);
            }
            finally
            {
                try
                {
                    webSocket?.Dispose();
                    
                    // Notify disconnection
                    if (OnClientDisconnected != null)
                    {
                        await OnClientDisconnected(clientId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error cleaning up WebSocket connection: {ClientId}", clientId);
                }
            }
        }

        private async Task ProcessWebSocketMessages(string clientId, WebSocket webSocket)
        {
            var buffer = new byte[4096];

            while (webSocket.State == WebSocketState.Open && _isListening)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellationTokenSource?.Token ?? CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        
                        if (OnMessageReceived != null)
                        {
                            await OnMessageReceived(clientId, message);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (_isListening)
                {
                    _logger.LogError(ex, "❌ Error processing WebSocket message from client: {ClientId}", clientId);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Simple gRPC server implementation
    /// </summary>
    public class GrpcServer
    {
        private readonly ILogger _logger;

        public GrpcServer(ILogger logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task StartAsync(int port)
        {
            _logger.LogInformation("🌐 gRPC server would start on port {Port} (not implemented yet)", port);
            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("🛑 gRPC server would stop (not implemented yet)");
            await Task.CompletedTask;
        }
    }
}
