using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.UnityServer.Unity.Synchronization
{
    /// <summary>
    /// Unity synchronization configuration
    /// </summary>
    public class UnitySyncConfiguration
    {
        public int WebSocketPort { get; set; } = 8081;
        public int GrpcPort { get; set; } = 8082;
        public int MaxChangesPerFrame { get; set; } = 100;
        public int MaxChangeBufferSize { get; set; } = 1000;
        public int ChangeBufferFlushIntervalMs { get; set; } = 16; // 60 FPS
        public bool EnableLivePrototyping { get; set; } = true;
        public bool EnableHotReload { get; set; } = true;
        public double MaxLatencyMs { get; set; } = 16.0; // 60 FPS target
    }

    /// <summary>
    /// Unity client information for registration
    /// </summary>
    public class UnityClientInfo
    {
        public string ProjectName { get; set; } = string.Empty;
        public string UnityVersion { get; set; } = string.Empty;
        public string CurrentScene { get; set; } = string.Empty;
        public bool EnableLivePrototyping { get; set; } = false;
        public UnitySyncSettings? SyncSettings { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Unity synchronization settings per client
    /// </summary>
    public class UnitySyncSettings
    {
        public bool SyncTransforms { get; set; } = true;
        public bool SyncComponents { get; set; } = true;
        public bool SyncGameObjects { get; set; } = true;
        public bool SyncPhysics { get; set; } = false;
        public bool SyncAnimations { get; set; } = false;
        public int UpdateFrequencyHz { get; set; } = 60;
        public List<string> ExcludedTags { get; set; } = new();
        public List<string> ExcludedLayers { get; set; } = new();
    }

    /// <summary>
    /// Unity scene change from client
    /// </summary>
    public class UnitySceneChange
    {
        public string ChangeId { get; set; } = Guid.NewGuid().ToString();
        public string ClientId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, TRANSFORM, COMPONENT
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public UnitySyncPriority Priority { get; set; } = UnitySyncPriority.Normal;
        
        // Change data
        public UnityTransform? Transform { get; set; }
        public Dictionary<string, object> ComponentUpdates { get; set; } = new();
        public object ChangeData { get; set; } = new();
        
        // Consciousness integration
        public bool IsConsciousnessRelevant { get; set; } = false;
        public bool RequiresActorUpdate { get; set; } = false;
        public float ConsciousnessImpact { get; set; } = 0f;
    }

    /// <summary>
    /// Unity sync priority levels
    /// </summary>
    public enum UnitySyncPriority
    {
        Low = 1,
        Normal = 5,
        High = 8,
        Immediate = 10
    }

    /// <summary>
    /// Unity client representation
    /// </summary>
    public class UnityClient
    {
        public string ClientId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string UnityVersion { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime? DisconnectedAt { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public UnitySyncSettings SyncSettings { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Unity scene representation
    /// </summary>
    public class UnityScene
    {
        public string ClientId { get; set; } = string.Empty;
        public string SceneName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, UnityGameObject> GameObjects { get; set; } = new();
        public Dictionary<string, object> SceneSettings { get; set; } = new();
    }

    /// <summary>
    /// Unity GameObject representation
    /// </summary>
    public class UnityGameObject
    {
        public string GameObjectId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public UnityTransform Transform { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int Layer { get; set; } = 0;
        public Dictionary<string, object> Components { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Unity command to execute on client
    /// </summary>
    public class UnityCommand
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString();
        public string CommandType { get; set; } = string.Empty; // CREATE_OBJECT, UPDATE_TRANSFORM, DESTROY_OBJECT, EXECUTE_SCRIPT
        public string TargetEntityId { get; set; } = string.Empty;
        public object CommandData { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
        public bool RequiresResponse { get; set; } = false;
    }

    /// <summary>
    /// Consciousness update with Unity targeting
    /// </summary>
    public class ConsciousnessUpdate
    {
        public string UpdateId { get; set; } = Guid.NewGuid().ToString();
        public string EntityId { get; set; } = string.Empty;
        public string UpdateType { get; set; } = string.Empty;
        public object Data { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TargetClientId { get; set; } = string.Empty; // Empty = broadcast to all
        public UnitySyncPriority Priority { get; set; } = UnitySyncPriority.Normal;
    }

    /// <summary>
    /// Live prototyping session
    /// </summary>
    public class LivePrototypingSession : IAsyncDisposable
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string ClientId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> SessionData { get; set; } = new();

        public async Task<HotReloadResult> ProcessHotReloadAsync(HotReloadRequest request)
        {
            // Implementation would process hot-reload logic
            return await Task.FromResult(new HotReloadResult
            {
                RequestId = request.RequestId,
                Success = true,
                EntityId = request.EntityId,
                ProcessingTimeMs = 50, // Simulated processing time
                UpdatedState = new { Status = "HotReloaded", Timestamp = DateTime.UtcNow }
            });
        }

        public async ValueTask DisposeAsync()
        {
            IsActive = false;
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Hot-reload request from Unity client
    /// </summary>
    public class HotReloadRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string ClientId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string ScriptContent { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // SCRIPT_UPDATE, BEHAVIOR_CHANGE, PROPERTY_UPDATE
        public object ChangeData { get; set; } = new();
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Hot-reload processing result
    /// </summary>
    public class HotReloadResult
    {
        public string RequestId { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public string EntityId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public double ProcessingTimeMs { get; set; } = 0;
        public object UpdatedState { get; set; } = new();
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Scene change buffer for batching
    /// </summary>
    public class SceneChangeBuffer
    {
        public string ClientId { get; set; } = string.Empty;
        public int MaxBufferSize { get; set; } = 1000;
        public int FlushIntervalMs { get; set; } = 16;
        public int QueuedChanges { get; set; } = 0;
        private DateTime _lastFlush = DateTime.UtcNow;

        public bool ShouldFlush()
        {
            return QueuedChanges >= MaxBufferSize || 
                   (DateTime.UtcNow - _lastFlush).TotalMilliseconds >= FlushIntervalMs;
        }

        public async Task FlushAsync()
        {
            QueuedChanges = 0;
            _lastFlush = DateTime.UtcNow;
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Unity synchronization metrics
    /// </summary>
    public class UnitySyncMetrics
    {
        public int ConnectedClients { get; set; } = 0;
        public long UnityChangesReceived { get; set; } = 0;
        public long ConsciousnessUpdatesSent { get; set; } = 0;
        public long CommandsSent { get; set; } = 0;
        public long HotReloadsProcessed { get; set; } = 0;
        public long ProcessingErrors { get; set; } = 0;
        public long HotReloadErrors { get; set; } = 0;
        public double AverageLatencyMs { get; set; } = 0;
        public TimeSpan Uptime { get; set; }
        public double ChangesPerSecond { get; set; } = 0;
        public double UpdatesPerSecond { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Unity client status for monitoring
    /// </summary>
    public class UnityClientStatus
    {
        public string ClientId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string UnityVersion { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public bool IsActive { get; set; } = false;
        public bool LivePrototypingEnabled { get; set; } = false;
        public int QueuedChanges { get; set; } = 0;
        public DateTime LastActivity { get; set; }
        public TimeSpan ConnectionDuration => DateTime.UtcNow - ConnectedAt;
    }

    /// <summary>
    /// Synchronization performance metrics
    /// </summary>
    internal class SynchronizationMetrics
    {
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public long ConnectedClients { get; set; } = 0;
        public long UnityChangesReceived { get; set; } = 0;
        public long ConsciousnessUpdatesSent { get; set; } = 0;
        public long CommandsSent { get; set; } = 0;
        public long HotReloadsProcessed { get; set; } = 0;
        public long ProcessingErrors { get; set; } = 0;
        public long HotReloadErrors { get; set; } = 0;
    }
}

namespace CxLanguage.UnityServer.Unity.Synchronization.Abstractions
{
    /// <summary>
    /// Unity protocol manager interface
    /// </summary>
    public interface IUnityProtocolManager
    {
        Task InitializeAsync();
        Task StartListeningAsync(int port, string protocolType);
        Task StopListeningAsync();
        Task SendUpdateToClientAsync(string clientId, ConsciousnessUpdate update);
        Task SendCommandToClientAsync(string clientId, UnityCommand command);
        Task DisconnectClientAsync(string clientId);
        Task<List<string>> GetConnectedClientsAsync();
    }

    /// <summary>
    /// Live prototyping engine interface
    /// </summary>
    public interface ILivePrototypingEngine
    {
        Task InitializeAsync();
        Task<LivePrototypingSession> CreateSessionAsync(string clientId);
        Task<bool> DestroySessionAsync(string sessionId);
        Task<List<LivePrototypingSession>> GetActiveSessionsAsync();
    }

    /// <summary>
    /// Consciousness processor interface for Unity integration
    /// </summary>
    public interface IConsciousnessProcessor
    {
        Task ProcessUnityChangeAsync(string entityId, UnitySceneChange change, ConsciousnessEvent consciousnessEvent);
        Task<ConsciousnessUpdate> GenerateConsciousnessUpdateAsync(string entityId, object data);
    }
}
