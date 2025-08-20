using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Visualization
{
    public class ConsciousnessWebSocketServer
    {
        private readonly ILogger<ConsciousnessWebSocketServer> _logger;
        private HttpListener _httpListener;
        private readonly List<WebSocket> _connectedClients = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isRunning = false;

        public ConsciousnessWebSocketServer(ILogger<ConsciousnessWebSocketServer> logger = null)
        {
            _logger = logger ?? new ConsoleLogger();
        }

        public async Task StartAsync(int port = 8080)
        {
            if (_isRunning) return;

            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add($"http://localhost:{port}/");
                _httpListener.Start();
                _isRunning = true;

                _logger.LogInformation($"🔗 Consciousness WebSocket server started at ws://localhost:{port}/consciousness");

                _ = Task.Run(async () => await AcceptClientsAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to start WebSocket server: {ex.Message}");
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning && _httpListener.IsListening)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    
                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = Task.Run(async () => await HandleWebSocketAsync(context));
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        _logger.LogError($"❌ Error accepting client: {ex.Message}");
                    }
                }
            }
        }

        private async Task HandleWebSocketAsync(HttpListenerContext context)
        {
            WebSocket webSocket = null;
            
            try
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                webSocket = webSocketContext.WebSocket;
                
                lock (_connectedClients)
                {
                    _connectedClients.Add(webSocket);
                }

                _logger.LogInformation($"🎯 New consciousness client connected. Total clients: {_connectedClients.Count}");

                // Send initial welcome message
                await SendToClientAsync(webSocket, new
                {
                    type = "connection.established",
                    message = "Connected to CX Language consciousness stream",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    clientId = Guid.NewGuid().ToString("N")[..8]
                });

                // Keep connection alive and handle incoming messages
                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client requested close", CancellationToken.None);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ WebSocket error: {ex.Message}");
            }
            finally
            {
                if (webSocket != null)
                {
                    lock (_connectedClients)
                    {
                        _connectedClients.Remove(webSocket);
                    }
                    
                    _logger.LogInformation($"🔌 Client disconnected. Remaining clients: {_connectedClients.Count}");
                    
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closing", CancellationToken.None);
                    }
                }
            }
        }

        public async Task BroadcastConsciousnessDataAsync(object data)
        {
            if (!_isRunning || _connectedClients.Count == 0) return;

            var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var buffer = Encoding.UTF8.GetBytes(jsonData);
            var clientsToRemove = new List<WebSocket>();

            WebSocket[] clients;
            lock (_connectedClients)
            {
                clients = _connectedClients.ToArray();
            }

            foreach (var client in clients)
            {
                try
                {
                    if (client.State == WebSocketState.Open)
                    {
                        await client.SendAsync(
                            new ArraySegment<byte>(buffer),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None
                        );
                    }
                    else
                    {
                        clientsToRemove.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"⚠️ Failed to send to client: {ex.Message}");
                    clientsToRemove.Add(client);
                }
            }

            if (clientsToRemove.Count > 0)
            {
                lock (_connectedClients)
                {
                    foreach (var client in clientsToRemove)
                    {
                        _connectedClients.Remove(client);
                    }
                }
            }
        }

        private async Task SendToClientAsync(WebSocket client, object data)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                var buffer = Encoding.UTF8.GetBytes(jsonData);
                
                await client.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to send message to client: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cancellationTokenSource.Cancel();

            try
            {
                // Close all client connections
                WebSocket[] clients;
                lock (_connectedClients)
                {
                    clients = _connectedClients.ToArray();
                    _connectedClients.Clear();
                }

                foreach (var client in clients)
                {
                    try
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"⚠️ Error closing client connection: {ex.Message}");
                    }
                }

                _httpListener?.Stop();
                _httpListener?.Close();

                _logger.LogInformation("🛑 Consciousness WebSocket server stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error stopping WebSocket server: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _cancellationTokenSource?.Dispose();
        }
    }

    // Simple console logger for standalone use
    public class ConsoleLogger : ILogger<ConsciousnessWebSocketServer>
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            
            var prefix = logLevel switch
            {
                LogLevel.Information => "ℹ️",
                LogLevel.Warning => "⚠️",
                LogLevel.Error => "❌",
                LogLevel.Debug => "🔍",
                _ => "📝"
            };

            Console.WriteLine($"[{timestamp}] {prefix} {message}");
        }
    }
}
