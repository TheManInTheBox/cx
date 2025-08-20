using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.UnityServer.Core.Runtime
{
    /// <summary>
    /// Actor configuration for runtime creation
    /// </summary>
    public class ActorConfiguration
    {
        public string Namespace { get; set; } = "default";
        public bool EnableConsciousness { get; set; } = false;
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public TimeSpan? Lifetime { get; set; }
        public int MaxConcurrentMessages { get; set; } = 100;
        public string UnityPrefabPath { get; set; } = string.Empty;
        public UnityTransform? InitialTransform { get; set; }
    }

    /// <summary>
    /// Actor state representation
    /// </summary>
    public class ActorState
    {
        public string ActorId { get; set; } = string.Empty;
        public string ActorType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastActivity { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public UnityTransform? Transform { get; set; }
        public ConsciousnessState? Consciousness { get; set; }
    }

    /// <summary>
    /// Actor state update command
    /// </summary>
    public class ActorStateUpdate
    {
        public string ActorId { get; set; } = string.Empty;
        public Dictionary<string, object> PropertyUpdates { get; set; } = new();
        public UnityTransform? TransformUpdate { get; set; }
        public ConsciousnessUpdate? ConsciousnessUpdate { get; set; }
        public bool IsConsciousnessRelevant { get; set; } = false;
        public string UpdateReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Actor message for inter-actor communication
    /// </summary>
    public class ActorMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public string FromActorId { get; set; } = string.Empty;
        public string ToActorId { get; set; } = string.Empty;
        public object Message { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Actor information for runtime queries
    /// </summary>
    public class ActorInfo
    {
        public string ActorId { get; set; } = string.Empty;
        public string ActorType { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DestroyedAt { get; set; }
        public float ConsciousnessLevel { get; set; } = 0f;
        public int MessageCount { get; set; } = 0;
        public TimeSpan Uptime => DestroyedAt.HasValue 
            ? DestroyedAt.Value - CreatedAt 
            : DateTime.UtcNow - CreatedAt;
    }

    /// <summary>
    /// Actor metrics for performance monitoring
    /// </summary>
    public class ActorMetrics
    {
        public string ActorId { get; set; } = string.Empty;
        public string ActorType { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? DestroyedAt { get; set; }
        public long MessagesReceived { get; set; } = 0;
        public long MessagesSent { get; set; } = 0;
        public long StateUpdates { get; set; } = 0;
        public DateTime LastStateUpdate { get; set; }
        public DateTime LastActivity { get; set; }
        public double AverageResponseTimeMs { get; set; } = 0;
    }

    /// <summary>
    /// Actor transaction for atomic operations
    /// </summary>
    public class ActorTransaction
    {
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();
        public List<TransactionOperation> Operations { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual operation within a transaction
    /// </summary>
    public class TransactionOperation
    {
        public string OperationType { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, MESSAGE
        public string TargetActorId { get; set; } = string.Empty;
        public object OperationData { get; set; } = new();
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Transaction validation result
    /// </summary>
    public class TransactionValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Unity scene update for bidirectional synchronization
    /// </summary>
    public class UnitySceneUpdate
    {
        public string UpdateId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<UnityEntityChange> Changes { get; set; } = new();
        public string SourceUnityInstance { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual Unity entity change
    /// </summary>
    public class UnityEntityChange
    {
        public string EntityId { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, TRANSFORM
        public UnityTransform? Transform { get; set; }
        public Dictionary<string, object> ComponentUpdates { get; set; } = new();
        public bool IsConsciousnessRelevant { get; set; } = false;
    }

    /// <summary>
    /// Unity transform data
    /// </summary>
    public class UnityTransform
    {
        public UnityVector3 Position { get; set; } = new();
        public UnityVector3 Rotation { get; set; } = new();
        public UnityVector3 Scale { get; set; } = new(1, 1, 1);
    }

    /// <summary>
    /// Unity Vector3 representation
    /// </summary>
    public class UnityVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public UnityVector3() { }
        public UnityVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    /// Consciousness update for AI integration
    /// </summary>
    public class ConsciousnessUpdate
    {
        public string EntityId { get; set; } = string.Empty;
        public ConsciousnessState State { get; set; } = new();
        public List<ConsciousnessEvent> Events { get; set; } = new();
        public Dictionary<string, object> Context { get; set; } = new();
    }

    /// <summary>
    /// Consciousness state representation
    /// </summary>
    public class ConsciousnessState
    {
        public float AwarenessLevel { get; set; } = 0f;
        public float FocusLevel { get; set; } = 0f;
        public string CurrentGoal { get; set; } = string.Empty;
        public List<string> ActiveConcerns { get; set; } = new();
        public Dictionary<string, float> EmotionalState { get; set; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Consciousness event for processing
    /// </summary>
    public class ConsciousnessEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = string.Empty;
        public string SourceEntityId { get; set; } = string.Empty;
        public object EventData { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public float Priority { get; set; } = 0.5f;
    }

    /// <summary>
    /// AI response to consciousness events
    /// </summary>
    public class AiResponse
    {
        public string ResponseId { get; set; } = Guid.NewGuid().ToString();
        public string EventId { get; set; } = string.Empty;
        public string ResponseType { get; set; } = string.Empty;
        public object ResponseData { get; set; } = new();
        public float Confidence { get; set; } = 0f;
        public Dictionary<string, object> Context { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Runtime performance metrics
    /// </summary>
    public class RuntimeMetrics
    {
        public int ActiveActors { get; set; } = 0;
        public long TotalMessages { get; set; } = 0;
        public long TotalTransactions { get; set; } = 0;
        public double MessagesPerSecond { get; set; } = 0;
        public double TransactionsPerSecond { get; set; } = 0;
        public TimeSpan Uptime { get; set; }
        public int ActiveNamespaces { get; set; } = 0;
        public long MemoryUsageMB { get; set; } = 0;
        public int ConsciousnessEntities { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

namespace CxLanguage.UnityServer.Core.Runtime.Abstractions
{
    /// <summary>
    /// Base consciousness actor interface
    /// </summary>
    public interface IConsciousnessActor
    {
        string ActorId { get; }
        string ActorType { get; }
        ActorConfiguration Configuration { get; }
        
        Task StartAsync();
        Task StopAsync();
        Task<bool> IsActiveAsync();
        Task<ActorState> GetStateAsync();
        Task<bool> UpdateStateAsync(ActorStateUpdate update);
        Task ReceiveMessageAsync(ActorMessage message);
        Task ApplyUnityChangeAsync(UnityEntityChange change);
    }

    /// <summary>
    /// Actor factory for creating typed actors
    /// </summary>
    public interface IActorFactory
    {
        Task<ConsciousnessActor> CreateActorAsync(string actorType, string actorId, ActorConfiguration config);
        Task<List<string>> GetSupportedActorTypesAsync();
    }

    /// <summary>
    /// Namespace manager for actor isolation
    /// </summary>
    public interface INamespaceManager
    {
        Task InitializeAsync();
        Task<ActorNamespace> CreateNamespaceAsync(string namespaceName);
        Task<bool> DeleteNamespaceAsync(string namespaceName);
        Task<List<string>> GetNamespacesAsync();
    }

    /// <summary>
    /// Transaction manager for atomic operations
    /// </summary>
    public interface ITransactionManager
    {
        Task<TransactionValidationResult> ValidateTransactionAsync(ActorTransaction transaction);
        Task<bool> ExecuteTransactionAsync(ActorTransaction transaction, 
            System.Collections.Concurrent.ConcurrentDictionary<string, ConsciousnessActor> actors);
    }

    /// <summary>
    /// Consciousness processor for AI integration
    /// </summary>
    public interface IConsciousnessProcessor
    {
        Task InitializeActorConsciousnessAsync(ConsciousnessActor actor);
        Task CleanupActorConsciousnessAsync(ConsciousnessActor actor);
        Task ProcessStateUpdateAsync(ConsciousnessActor actor, ActorStateUpdate update);
        Task ProcessUnityChangeAsync(ConsciousnessActor actor, UnityEntityChange change);
        Task ApplyConsciousnessUpdateAsync(ConsciousnessActor actor, ConsciousnessUpdate update);
        Task ApplyAiResponseAsync(ConsciousnessActor actor, ConsciousnessEvent evt, AiResponse response);
        Task<float> GetConsciousnessLevelAsync(ConsciousnessActor actor);
    }

    /// <summary>
    /// Actor namespace for isolation
    /// </summary>
    public class ActorNamespace
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        private readonly HashSet<string> _actorIds = new();
        private readonly object _lock = new();

        public void AddActor(ConsciousnessActor actor)
        {
            lock (_lock)
            {
                _actorIds.Add(actor.ActorId);
            }
        }

        public void RemoveActor(string actorId)
        {
            lock (_lock)
            {
                _actorIds.Remove(actorId);
            }
        }

        public bool ContainsActor(string actorId)
        {
            lock (_lock)
            {
                return _actorIds.Contains(actorId);
            }
        }

        public int ActorCount
        {
            get
            {
                lock (_lock)
                {
                    return _actorIds.Count;
                }
            }
        }
    }

    /// <summary>
    /// Base consciousness actor implementation
    /// </summary>
    public abstract class ConsciousnessActor : IConsciousnessActor
    {
        public string ActorId { get; }
        public string ActorType { get; }
        public ActorConfiguration Configuration { get; }

        protected bool _isActive = false;
        protected DateTime _lastActivity = DateTime.UtcNow;
        protected readonly object _stateLock = new();

        protected ConsciousnessActor(string actorId, string actorType, ActorConfiguration configuration)
        {
            ActorId = actorId;
            ActorType = actorType;
            Configuration = configuration;
        }

        public virtual async Task StartAsync()
        {
            _isActive = true;
            _lastActivity = DateTime.UtcNow;
            await OnStartAsync();
        }

        public virtual async Task StopAsync()
        {
            _isActive = false;
            await OnStopAsync();
        }

        public virtual Task<bool> IsActiveAsync()
        {
            return Task.FromResult(_isActive);
        }

        public abstract Task<ActorState> GetStateAsync();
        public abstract Task<bool> UpdateStateAsync(ActorStateUpdate update);
        public abstract Task ReceiveMessageAsync(ActorMessage message);
        public abstract Task ApplyUnityChangeAsync(UnityEntityChange change);

        protected virtual Task OnStartAsync() => Task.CompletedTask;
        protected virtual Task OnStopAsync() => Task.CompletedTask;

        protected void UpdateActivity()
        {
            _lastActivity = DateTime.UtcNow;
        }
    }
}
