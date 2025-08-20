using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CxLanguage.UnityServer.Core.Protocols
{
    /// <summary>
    /// Protocol dispatcher interface for multi-protocol support
    /// </summary>
    public interface IProtocolDispatcher
    {
        Task InitializeAsync();
        Task<bool> RegisterProtocolHandlerAsync<T>(string protocolName, IProtocolHandler<T> handler);
        Task<ProtocolResponse> ProcessRequestAsync(ProtocolRequest request);
        Task StartListeningAsync(int port, string protocolType);
        Task StopListeningAsync();
        Task<List<ProtocolInfo>> GetActiveProtocolsAsync();
        Task BroadcastToClientsAsync(string protocolType, object message);
    }

    /// <summary>
    /// Multi-protocol dispatcher implementation
    /// Supports WebSocket, gRPC, and Binary protocols with type-safe API contracts
    /// </summary>
    public class ProtocolDispatcher : IProtocolDispatcher
    {
        private readonly ILogger<ProtocolDispatcher> _logger;
        private readonly IWebSocketManager _webSocketManager;
        private readonly IGrpcServerManager _grpcServerManager;
        private readonly IBinaryProtocolManager _binaryProtocolManager;
        private readonly IApiContractGenerator _apiContractGenerator;
        
        // Protocol handler registry
        private readonly ConcurrentDictionary<string, object> _protocolHandlers;
        private readonly ConcurrentDictionary<string, ProtocolInfo> _activeProtocols;
        private readonly ConcurrentBag<IProtocolListener> _listeners;
        
        // Performance tracking
        private readonly ConcurrentDictionary<string, ProtocolMetrics> _protocolMetrics;
        private long _totalRequests = 0;
        private DateTime _startTime;

        public ProtocolDispatcher(
            ILogger<ProtocolDispatcher> logger,
            IWebSocketManager webSocketManager,
            IGrpcServerManager grpcServerManager,
            IBinaryProtocolManager binaryProtocolManager,
            IApiContractGenerator apiContractGenerator)
        {
            _logger = logger;
            _webSocketManager = webSocketManager;
            _grpcServerManager = grpcServerManager;
            _binaryProtocolManager = binaryProtocolManager;
            _apiContractGenerator = apiContractGenerator;
            
            _protocolHandlers = new ConcurrentDictionary<string, object>();
            _activeProtocols = new ConcurrentDictionary<string, ProtocolInfo>();
            _listeners = new ConcurrentBag<IProtocolListener>();
            _protocolMetrics = new ConcurrentDictionary<string, ProtocolMetrics>();
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("🌐 Initializing Protocol Dispatcher");
            
            _startTime = DateTime.UtcNow;
            
            try
            {
                // Initialize protocol managers
                await _webSocketManager.InitializeAsync();
                await _grpcServerManager.InitializeAsync();
                await _binaryProtocolManager.InitializeAsync();
                
                // Generate API contracts
                await _apiContractGenerator.InitializeAsync();
                
                // Register default protocol handlers
                await RegisterDefaultHandlersAsync();
                
                _logger.LogInformation("✅ Protocol Dispatcher initialized");
                _logger.LogInformation("  🌐 WebSocket support enabled");
                _logger.LogInformation("  🔌 gRPC support enabled");
                _logger.LogInformation("  📦 Binary protocol support enabled");
                _logger.LogInformation("  📋 Auto-generated API contracts ready");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize Protocol Dispatcher");
                throw;
            }
        }

        public async Task<bool> RegisterProtocolHandlerAsync<T>(string protocolName, IProtocolHandler<T> handler)
        {
            _logger.LogDebug("📝 Registering protocol handler: {ProtocolName}", protocolName);

            try
            {
                if (_protocolHandlers.TryAdd(protocolName, handler))
                {
                    // Initialize protocol metrics
                    _protocolMetrics.TryAdd(protocolName, new ProtocolMetrics
                    {
                        ProtocolName = protocolName,
                        RegisteredAt = DateTime.UtcNow
                    });
                    
                    // Generate API contract for the protocol
                    await _apiContractGenerator.GenerateContractAsync<T>(protocolName);
                    
                    _logger.LogInformation("✅ Registered protocol handler: {ProtocolName}", protocolName);
                    return true;
                }
                
                _logger.LogWarning("⚠️ Protocol handler already exists: {ProtocolName}", protocolName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to register protocol handler: {ProtocolName}", protocolName);
                return false;
            }
        }

        public async Task<ProtocolResponse> ProcessRequestAsync(ProtocolRequest request)
        {
            _logger.LogDebug("📥 Processing protocol request: {RequestId} via {Protocol}", 
                request.RequestId, request.ProtocolType);

            Interlocked.Increment(ref _totalRequests);
            var startTime = DateTime.UtcNow;

            try
            {
                // Update protocol metrics
                if (_protocolMetrics.TryGetValue(request.ProtocolType, out var metrics))
                {
                    metrics.RequestCount++;
                    metrics.LastRequestAt = DateTime.UtcNow;
                }

                // Route to appropriate protocol handler
                var response = request.ProtocolType.ToLower() switch
                {
                    "websocket" => await ProcessWebSocketRequestAsync(request),
                    "grpc" => await ProcessGrpcRequestAsync(request),
                    "binary" => await ProcessBinaryRequestAsync(request),
                    _ => await ProcessCustomProtocolRequestAsync(request)
                };

                // Update response metrics
                var duration = DateTime.UtcNow - startTime;
                if (metrics != null)
                {
                    metrics.AverageResponseTimeMs = 
                        (metrics.AverageResponseTimeMs + duration.TotalMilliseconds) / 2;
                }

                _logger.LogDebug("📤 Processed request: {RequestId} in {Duration}ms", 
                    request.RequestId, duration.TotalMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing request: {RequestId}", request.RequestId);
                
                return new ProtocolResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task StartListeningAsync(int port, string protocolType)
        {
            _logger.LogInformation("🎧 Starting listener for {ProtocolType} on port {Port}", protocolType, port);

            try
            {
                IProtocolListener listener = protocolType.ToLower() switch
                {
                    "websocket" => await _webSocketManager.StartListenerAsync(port, ProcessWebSocketMessageAsync),
                    "grpc" => await _grpcServerManager.StartListenerAsync(port),
                    "binary" => await _binaryProtocolManager.StartListenerAsync(port, ProcessBinaryMessageAsync),
                    _ => throw new NotSupportedException($"Protocol type not supported: {protocolType}")
                };

                _listeners.Add(listener);
                
                _activeProtocols.TryAdd($"{protocolType}:{port}", new ProtocolInfo
                {
                    ProtocolType = protocolType,
                    Port = port,
                    StartedAt = DateTime.UtcNow,
                    IsActive = true
                });

                _logger.LogInformation("✅ Listener started for {ProtocolType} on port {Port}", protocolType, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to start listener for {ProtocolType} on port {Port}", protocolType, port);
                throw;
            }
        }

        public async Task StopListeningAsync()
        {
            _logger.LogInformation("🛑 Stopping all protocol listeners");

            var stopTasks = new List<Task>();
            
            foreach (var listener in _listeners)
            {
                stopTasks.Add(listener.StopAsync());
            }
            
            await Task.WhenAll(stopTasks);
            
            // Update active protocols
            foreach (var protocol in _activeProtocols.Values)
            {
                protocol.IsActive = false;
                protocol.StoppedAt = DateTime.UtcNow;
            }

            _logger.LogInformation("✅ All protocol listeners stopped");
        }

        public async Task<List<ProtocolInfo>> GetActiveProtocolsAsync()
        {
            return await Task.FromResult(_activeProtocols.Values.ToList());
        }

        public async Task BroadcastToClientsAsync(string protocolType, object message)
        {
            _logger.LogDebug("📢 Broadcasting message via {ProtocolType}", protocolType);

            try
            {
                switch (protocolType.ToLower())
                {
                    case "websocket":
                        await _webSocketManager.BroadcastAsync(message);
                        break;
                    case "grpc":
                        await _grpcServerManager.BroadcastAsync(message);
                        break;
                    case "binary":
                        await _binaryProtocolManager.BroadcastAsync(message);
                        break;
                    default:
                        _logger.LogWarning("⚠️ Unknown protocol type for broadcast: {ProtocolType}", protocolType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error broadcasting via {ProtocolType}", protocolType);
            }
        }

        // Private implementation methods

        private async Task RegisterDefaultHandlersAsync()
        {
            // Unity synchronization handler
            await RegisterProtocolHandlerAsync("unity.sync", new UnitySyncProtocolHandler(_logger));
            
            // CX script execution handler
            await RegisterProtocolHandlerAsync("cx.execute", new CxExecutionProtocolHandler(_logger));
            
            // Actor model handler
            await RegisterProtocolHandlerAsync("actor.runtime", new ActorRuntimeProtocolHandler(_logger));
            
            // Consciousness AI handler
            await RegisterProtocolHandlerAsync("consciousness.ai", new ConsciousnessAiProtocolHandler(_logger));
        }

        private async Task<ProtocolResponse> ProcessWebSocketRequestAsync(ProtocolRequest request)
        {
            return await _webSocketManager.ProcessRequestAsync(request);
        }

        private async Task<ProtocolResponse> ProcessGrpcRequestAsync(ProtocolRequest request)
        {
            return await _grpcServerManager.ProcessRequestAsync(request);
        }

        private async Task<ProtocolResponse> ProcessBinaryRequestAsync(ProtocolRequest request)
        {
            return await _binaryProtocolManager.ProcessRequestAsync(request);
        }

        private async Task<ProtocolResponse> ProcessCustomProtocolRequestAsync(ProtocolRequest request)
        {
            if (_protocolHandlers.TryGetValue(request.ProtocolType, out var handler))
            {
                // Use reflection to call the appropriate handler method
                var method = handler.GetType().GetMethod("HandleAsync");
                if (method != null)
                {
                    var result = await (Task<ProtocolResponse>)method.Invoke(handler, new object[] { request })!;
                    return result;
                }
            }

            return new ProtocolResponse
            {
                RequestId = request.RequestId,
                Success = false,
                ErrorMessage = $"No handler found for protocol: {request.ProtocolType}",
                Timestamp = DateTime.UtcNow
            };
        }

        private async Task ProcessWebSocketMessageAsync(WebSocket webSocket, string message)
        {
            try
            {
                var request = JsonSerializer.Deserialize<ProtocolRequest>(message);
                if (request != null)
                {
                    var response = await ProcessRequestAsync(request);
                    var responseJson = JsonSerializer.Serialize(response);
                    
                    var buffer = Encoding.UTF8.GetBytes(responseJson);
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(buffer),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing WebSocket message");
            }
        }

        private async Task ProcessBinaryMessageAsync(byte[] data)
        {
            try
            {
                // Deserialize binary message to protocol request
                var request = BinarySerializer.Deserialize<ProtocolRequest>(data);
                var response = await ProcessRequestAsync(request);
                
                // Send binary response back (implementation depends on connection management)
                var responseData = BinarySerializer.Serialize(response);
                await _binaryProtocolManager.SendResponseAsync(responseData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing binary message");
            }
        }
    }
}

namespace CxLanguage.UnityServer.Core.Protocols.Models
{
    /// <summary>
    /// Protocol request structure
    /// </summary>
    public class ProtocolRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string ProtocolType { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public object Data { get; set; } = new();
        public Dictionary<string, object> Headers { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ClientId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Protocol response structure
    /// </summary>
    public class ProtocolResponse
    {
        public string RequestId { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public object? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, object> Headers { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double ProcessingTimeMs { get; set; } = 0;
    }

    /// <summary>
    /// Protocol information
    /// </summary>
    public class ProtocolInfo
    {
        public string ProtocolType { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public DateTime StartedAt { get; set; }
        public DateTime? StoppedAt { get; set; }
        public bool IsActive { get; set; } = false;
        public long ConnectionCount { get; set; } = 0;
        public double UptimeSeconds => StoppedAt.HasValue 
            ? (StoppedAt.Value - StartedAt).TotalSeconds 
            : (DateTime.UtcNow - StartedAt).TotalSeconds;
    }

    /// <summary>
    /// Protocol performance metrics
    /// </summary>
    public class ProtocolMetrics
    {
        public string ProtocolName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public long RequestCount { get; set; } = 0;
        public DateTime LastRequestAt { get; set; }
        public double AverageResponseTimeMs { get; set; } = 0;
        public long ErrorCount { get; set; } = 0;
        public double RequestsPerSecond => RequestCount / Math.Max(1, (DateTime.UtcNow - RegisteredAt).TotalSeconds);
    }
}

namespace CxLanguage.UnityServer.Core.Protocols.Abstractions
{
    /// <summary>
    /// Generic protocol handler interface
    /// </summary>
    public interface IProtocolHandler<T>
    {
        Task<ProtocolResponse> HandleAsync(ProtocolRequest request);
        Task<bool> ValidateRequestAsync(ProtocolRequest request);
        Task<T> DeserializeDataAsync(object data);
    }

    /// <summary>
    /// Protocol listener interface
    /// </summary>
    public interface IProtocolListener
    {
        Task StartAsync();
        Task StopAsync();
        bool IsListening { get; }
        int Port { get; }
        string ProtocolType { get; }
    }

    /// <summary>
    /// WebSocket manager interface
    /// </summary>
    public interface IWebSocketManager
    {
        Task InitializeAsync();
        Task<IProtocolListener> StartListenerAsync(int port, Func<WebSocket, string, Task> messageHandler);
        Task<ProtocolResponse> ProcessRequestAsync(ProtocolRequest request);
        Task BroadcastAsync(object message);
        Task<List<string>> GetConnectedClientsAsync();
    }

    /// <summary>
    /// gRPC server manager interface
    /// </summary>
    public interface IGrpcServerManager
    {
        Task InitializeAsync();
        Task<IProtocolListener> StartListenerAsync(int port);
        Task<ProtocolResponse> ProcessRequestAsync(ProtocolRequest request);
        Task BroadcastAsync(object message);
    }

    /// <summary>
    /// Binary protocol manager interface
    /// </summary>
    public interface IBinaryProtocolManager
    {
        Task InitializeAsync();
        Task<IProtocolListener> StartListenerAsync(int port, Func<byte[], Task> messageHandler);
        Task<ProtocolResponse> ProcessRequestAsync(ProtocolRequest request);
        Task BroadcastAsync(object message);
        Task SendResponseAsync(byte[] data);
    }

    /// <summary>
    /// API contract generator interface
    /// </summary>
    public interface IApiContractGenerator
    {
        Task InitializeAsync();
        Task GenerateContractAsync<T>(string protocolName);
        Task<string> GenerateCSharpStubsAsync(string protocolName);
        Task<string> GenerateTypeScriptStubsAsync(string protocolName);
        Task<string> GenerateOpenApiSpecAsync();
    }

    /// <summary>
    /// Binary serializer utility
    /// </summary>
    public static class BinarySerializer
    {
        public static byte[] Serialize<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T Deserialize<T>(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Failed to deserialize data");
        }
    }
}

namespace CxLanguage.UnityServer.Core.Protocols.Handlers
{
    /// <summary>
    /// Unity synchronization protocol handler
    /// </summary>
    public class UnitySyncProtocolHandler : IProtocolHandler<UnitySyncRequest>
    {
        private readonly ILogger _logger;

        public UnitySyncProtocolHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ProtocolResponse> HandleAsync(ProtocolRequest request)
        {
            var data = await DeserializeDataAsync(request.Data);
            
            // Handle Unity synchronization logic
            _logger.LogDebug("🎮 Processing Unity sync request: {RequestId}", request.RequestId);
            
            return new ProtocolResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Data = new { Message = "Unity sync processed", SyncId = data.SyncId },
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<bool> ValidateRequestAsync(ProtocolRequest request)
        {
            return await Task.FromResult(!string.IsNullOrEmpty(request.RequestId));
        }

        public async Task<UnitySyncRequest> DeserializeDataAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            return await Task.FromResult(JsonSerializer.Deserialize<UnitySyncRequest>(json) ?? new UnitySyncRequest());
        }
    }

    /// <summary>
    /// CX execution protocol handler
    /// </summary>
    public class CxExecutionProtocolHandler : IProtocolHandler<CxExecutionRequest>
    {
        private readonly ILogger _logger;

        public CxExecutionProtocolHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ProtocolResponse> HandleAsync(ProtocolRequest request)
        {
            var data = await DeserializeDataAsync(request.Data);
            
            _logger.LogDebug("🔧 Processing CX execution request: {RequestId}", request.RequestId);
            
            return new ProtocolResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Data = new { Message = "CX script executed", ScriptId = data.ScriptId },
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<bool> ValidateRequestAsync(ProtocolRequest request)
        {
            return await Task.FromResult(!string.IsNullOrEmpty(request.RequestId));
        }

        public async Task<CxExecutionRequest> DeserializeDataAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            return await Task.FromResult(JsonSerializer.Deserialize<CxExecutionRequest>(json) ?? new CxExecutionRequest());
        }
    }

    /// <summary>
    /// Actor runtime protocol handler
    /// </summary>
    public class ActorRuntimeProtocolHandler : IProtocolHandler<ActorRuntimeRequest>
    {
        private readonly ILogger _logger;

        public ActorRuntimeProtocolHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ProtocolResponse> HandleAsync(ProtocolRequest request)
        {
            var data = await DeserializeDataAsync(request.Data);
            
            _logger.LogDebug("🎭 Processing Actor runtime request: {RequestId}", request.RequestId);
            
            return new ProtocolResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Data = new { Message = "Actor operation processed", ActorId = data.ActorId },
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<bool> ValidateRequestAsync(ProtocolRequest request)
        {
            return await Task.FromResult(!string.IsNullOrEmpty(request.RequestId));
        }

        public async Task<ActorRuntimeRequest> DeserializeDataAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            return await Task.FromResult(JsonSerializer.Deserialize<ActorRuntimeRequest>(json) ?? new ActorRuntimeRequest());
        }
    }

    /// <summary>
    /// Consciousness AI protocol handler
    /// </summary>
    public class ConsciousnessAiProtocolHandler : IProtocolHandler<ConsciousnessAiRequest>
    {
        private readonly ILogger _logger;

        public ConsciousnessAiProtocolHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ProtocolResponse> HandleAsync(ProtocolRequest request)
        {
            var data = await DeserializeDataAsync(request.Data);
            
            _logger.LogDebug("🧠 Processing Consciousness AI request: {RequestId}", request.RequestId);
            
            return new ProtocolResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Data = new { Message = "Consciousness AI processed", EntityId = data.EntityId },
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<bool> ValidateRequestAsync(ProtocolRequest request)
        {
            return await Task.FromResult(!string.IsNullOrEmpty(request.RequestId));
        }

        public async Task<ConsciousnessAiRequest> DeserializeDataAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            return await Task.FromResult(JsonSerializer.Deserialize<ConsciousnessAiRequest>(json) ?? new ConsciousnessAiRequest());
        }
    }

    // Request models for protocol handlers
    public class UnitySyncRequest
    {
        public string SyncId { get; set; } = string.Empty;
        public object SyncData { get; set; } = new();
    }

    public class CxExecutionRequest
    {
        public string ScriptId { get; set; } = string.Empty;
        public string ScriptContent { get; set; } = string.Empty;
    }

    public class ActorRuntimeRequest
    {
        public string ActorId { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public object OperationData { get; set; } = new();
    }

    public class ConsciousnessAiRequest
    {
        public string EntityId { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public object RequestData { get; set; } = new();
    }
}
