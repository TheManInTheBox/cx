using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CxLanguage.UnityServer.Core.Runtime
{
    /// <summary>
    /// Actor Model Runtime for CX Language Unity Server
    /// Manages stateful consciousness entities with concurrent processing and transactional updates
    /// </summary>
    public interface IActorModelRuntime
    {
        Task InitializeAsync();
        Task<string> CreateActorAsync(string actorType, string actorId, ActorConfiguration config);
        Task<bool> DestroyActorAsync(string actorId);
        Task<ActorState> GetActorStateAsync(string actorId);
        Task<bool> UpdateActorStateAsync(string actorId, ActorStateUpdate update);
        Task<ActorMessage> SendMessageAsync(string fromActorId, string toActorId, object message);
        Task<List<ActorInfo>> GetActiveActorsAsync(string? namespaceFilter = null);
        Task<bool> ExecuteTransactionAsync(ActorTransaction transaction);
        Task ApplyUnityUpdateAsync(UnitySceneUpdate update);
        Task UpdateConsciousnessAsync(ConsciousnessUpdate update);
        Task UpdateConsciousnessStateAsync(ConsciousnessEvent evt, AiResponse response);
        Task<RuntimeMetrics> GetRuntimeMetricsAsync();
    }

    /// <summary>
    /// High-performance Actor Model Runtime implementation
    /// Supports consciousness-aware entities with Unity synchronization
    /// </summary>
    public class ActorModelRuntime : IActorModelRuntime
    {
        private readonly ILogger<ActorModelRuntime> _logger;
        private readonly IActorFactory _actorFactory;
        private readonly INamespaceManager _namespaceManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IConsciousnessProcessor _consciousnessProcessor;
        
        // Thread-safe actor management
        private readonly ConcurrentDictionary<string, ConsciousnessActor> _actors;
        private readonly ConcurrentDictionary<string, ActorNamespace> _namespaces;
        private readonly ConcurrentQueue<ActorMessage> _messageQueue;
        private readonly ConcurrentDictionary<string, ActorMetrics> _actorMetrics;
        
        // Runtime coordination
        private readonly SemaphoreSlim _transactionSemaphore;
        private readonly Timer _maintenanceTimer;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        // Performance tracking
        private long _totalMessages = 0;
        private long _totalTransactions = 0;
        private DateTime _startTime;

        public ActorModelRuntime(
            ILogger<ActorModelRuntime> logger,
            IActorFactory actorFactory,
            INamespaceManager namespaceManager,
            ITransactionManager transactionManager,
            IConsciousnessProcessor consciousnessProcessor)
        {
            _logger = logger;
            _actorFactory = actorFactory;
            _namespaceManager = namespaceManager;
            _transactionManager = transactionManager;
            _consciousnessProcessor = consciousnessProcessor;
            
            _actors = new ConcurrentDictionary<string, ConsciousnessActor>();
            _namespaces = new ConcurrentDictionary<string, ActorNamespace>();
            _messageQueue = new ConcurrentQueue<ActorMessage>();
            _actorMetrics = new ConcurrentDictionary<string, ActorMetrics>();
            
            _transactionSemaphore = new SemaphoreSlim(10, 10); // Allow 10 concurrent transactions
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Maintenance timer for cleanup and optimization
            _maintenanceTimer = new Timer(PerformMaintenance, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("🎭 Initializing Actor Model Runtime");
            
            _startTime = DateTime.UtcNow;
            
            try
            {
                // Initialize namespace manager
                await _namespaceManager.InitializeAsync();
                
                // Create default namespaces
                await CreateDefaultNamespacesAsync();
                
                // Start message processing
                _ = Task.Run(ProcessMessagesAsync, _cancellationTokenSource.Token);
                
                _logger.LogInformation("✅ Actor Model Runtime initialized");
                _logger.LogInformation("  🌐 Namespace isolation enabled");
                _logger.LogInformation("  🧠 Consciousness processing active");
                _logger.LogInformation("  ⚡ Concurrent actor processing ready");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize Actor Model Runtime");
                throw;
            }
        }

        public async Task<string> CreateActorAsync(string actorType, string actorId, ActorConfiguration config)
        {
            _logger.LogDebug("🎭 Creating actor: {ActorId} of type: {ActorType}", actorId, actorType);

            try
            {
                // Validate actor configuration
                await ValidateActorConfigurationAsync(config);
                
                // Get or create namespace
                var actorNamespace = await GetOrCreateNamespaceAsync(config.Namespace);
                
                // Create actor instance
                var actor = await _actorFactory.CreateActorAsync(actorType, actorId, config);
                
                // Configure consciousness processing if enabled
                if (config.EnableConsciousness)
                {
                    await _consciousnessProcessor.InitializeActorConsciousnessAsync(actor);
                }
                
                // Register actor in runtime
                if (_actors.TryAdd(actorId, actor))
                {
                    // Add to namespace
                    actorNamespace.AddActor(actor);
                    
                    // Initialize metrics
                    _actorMetrics.TryAdd(actorId, new ActorMetrics
                    {
                        ActorId = actorId,
                        ActorType = actorType,
                        CreatedAt = DateTime.UtcNow,
                        Namespace = config.Namespace
                    });
                    
                    // Start actor lifecycle
                    await actor.StartAsync();
                    
                    _logger.LogInformation("✅ Created actor: {ActorId} in namespace: {Namespace}", actorId, config.Namespace);
                    return actorId;
                }
                else
                {
                    throw new InvalidOperationException($"Actor with ID {actorId} already exists");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to create actor: {ActorId}", actorId);
                throw;
            }
        }

        public async Task<bool> DestroyActorAsync(string actorId)
        {
            _logger.LogDebug("🗑️ Destroying actor: {ActorId}", actorId);

            try
            {
                if (_actors.TryRemove(actorId, out var actor))
                {
                    // Remove from namespace
                    var actorNamespace = _namespaces.Values.FirstOrDefault(ns => ns.ContainsActor(actorId));
                    actorNamespace?.RemoveActor(actorId);
                    
                    // Stop actor lifecycle
                    await actor.StopAsync();
                    
                    // Cleanup consciousness processing
                    if (actor.Configuration.EnableConsciousness)
                    {
                        await _consciousnessProcessor.CleanupActorConsciousnessAsync(actor);
                    }
                    
                    // Update metrics
                    if (_actorMetrics.TryGetValue(actorId, out var metrics))
                    {
                        metrics.DestroyedAt = DateTime.UtcNow;
                    }
                    
                    _logger.LogInformation("✅ Destroyed actor: {ActorId}", actorId);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to destroy actor: {ActorId}", actorId);
                return false;
            }
        }

        public async Task<ActorState> GetActorStateAsync(string actorId)
        {
            if (_actors.TryGetValue(actorId, out var actor))
            {
                return await actor.GetStateAsync();
            }
            
            throw new ArgumentException($"Actor not found: {actorId}");
        }

        public async Task<bool> UpdateActorStateAsync(string actorId, ActorStateUpdate update)
        {
            _logger.LogDebug("🔄 Updating actor state: {ActorId}", actorId);

            try
            {
                if (_actors.TryGetValue(actorId, out var actor))
                {
                    // Apply state update
                    var success = await actor.UpdateStateAsync(update);
                    
                    // Update consciousness if relevant
                    if (success && update.IsConsciousnessRelevant)
                    {
                        await _consciousnessProcessor.ProcessStateUpdateAsync(actor, update);
                    }
                    
                    // Update metrics
                    if (_actorMetrics.TryGetValue(actorId, out var metrics))
                    {
                        metrics.StateUpdates++;
                        metrics.LastStateUpdate = DateTime.UtcNow;
                    }
                    
                    return success;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to update actor state: {ActorId}", actorId);
                return false;
            }
        }

        public async Task<ActorMessage> SendMessageAsync(string fromActorId, string toActorId, object message)
        {
            var actorMessage = new ActorMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                FromActorId = fromActorId,
                ToActorId = toActorId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            
            _messageQueue.Enqueue(actorMessage);
            Interlocked.Increment(ref _totalMessages);
            
            _logger.LogDebug("📨 Queued message: {MessageId} from {FromActor} to {ToActor}", 
                actorMessage.MessageId, fromActorId, toActorId);
            
            return actorMessage;
        }

        public async Task<List<ActorInfo>> GetActiveActorsAsync(string? namespaceFilter = null)
        {
            var actors = new List<ActorInfo>();
            
            foreach (var kvp in _actors)
            {
                var actor = kvp.Value;
                
                if (namespaceFilter == null || actor.Configuration.Namespace == namespaceFilter)
                {
                    actors.Add(new ActorInfo
                    {
                        ActorId = actor.ActorId,
                        ActorType = actor.ActorType,
                        Namespace = actor.Configuration.Namespace,
                        IsActive = await actor.IsActiveAsync(),
                        CreatedAt = _actorMetrics.GetValueOrDefault(actor.ActorId, new ActorMetrics()).CreatedAt,
                        ConsciousnessLevel = actor.Configuration.EnableConsciousness 
                            ? await _consciousnessProcessor.GetConsciousnessLevelAsync(actor)
                            : 0f
                    });
                }
            }
            
            return actors;
        }

        public async Task<bool> ExecuteTransactionAsync(ActorTransaction transaction)
        {
            _logger.LogDebug("💳 Executing transaction: {TransactionId}", transaction.TransactionId);

            await _transactionSemaphore.WaitAsync();
            
            try
            {
                // Validate transaction
                var validationResult = await _transactionManager.ValidateTransactionAsync(transaction);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("⚠️ Transaction validation failed: {TransactionId} - {Errors}", 
                        transaction.TransactionId, string.Join(", ", validationResult.Errors));
                    return false;
                }
                
                // Execute transaction operations
                var success = await _transactionManager.ExecuteTransactionAsync(transaction, _actors);
                
                if (success)
                {
                    Interlocked.Increment(ref _totalTransactions);
                    _logger.LogDebug("✅ Transaction executed: {TransactionId}", transaction.TransactionId);
                }
                else
                {
                    _logger.LogWarning("❌ Transaction failed: {TransactionId}", transaction.TransactionId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error executing transaction: {TransactionId}", transaction.TransactionId);
                return false;
            }
            finally
            {
                _transactionSemaphore.Release();
            }
        }

        public async Task ApplyUnityUpdateAsync(UnitySceneUpdate update)
        {
            _logger.LogDebug("🎮 Applying Unity update: {UpdateId}", update.UpdateId);

            try
            {
                foreach (var change in update.Changes)
                {
                    if (_actors.TryGetValue(change.EntityId, out var actor))
                    {
                        await actor.ApplyUnityChangeAsync(change);
                        
                        // Process consciousness implications
                        if (actor.Configuration.EnableConsciousness && change.IsConsciousnessRelevant)
                        {
                            await _consciousnessProcessor.ProcessUnityChangeAsync(actor, change);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error applying Unity update: {UpdateId}", update.UpdateId);
            }
        }

        public async Task UpdateConsciousnessAsync(ConsciousnessUpdate update)
        {
            _logger.LogDebug("🧠 Updating consciousness: {EntityId}", update.EntityId);

            try
            {
                if (_actors.TryGetValue(update.EntityId, out var actor))
                {
                    await _consciousnessProcessor.ApplyConsciousnessUpdateAsync(actor, update);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating consciousness: {EntityId}", update.EntityId);
            }
        }

        public async Task UpdateConsciousnessStateAsync(ConsciousnessEvent evt, AiResponse response)
        {
            _logger.LogDebug("🧠 Updating consciousness state from event: {EventId}", evt.EventId);

            try
            {
                if (!string.IsNullOrEmpty(evt.SourceEntityId) && _actors.TryGetValue(evt.SourceEntityId, out var actor))
                {
                    await _consciousnessProcessor.ApplyAiResponseAsync(actor, evt, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating consciousness state: {EventId}", evt.EventId);
            }
        }

        public async Task<RuntimeMetrics> GetRuntimeMetricsAsync()
        {
            var uptime = DateTime.UtcNow - _startTime;
            
            return new RuntimeMetrics
            {
                ActiveActors = _actors.Count,
                TotalMessages = _totalMessages,
                TotalTransactions = _totalTransactions,
                MessagesPerSecond = _totalMessages / Math.Max(1, uptime.TotalSeconds),
                TransactionsPerSecond = _totalTransactions / Math.Max(1, uptime.TotalSeconds),
                Uptime = uptime,
                ActiveNamespaces = _namespaces.Count,
                MemoryUsageMB = GC.GetTotalMemory(false) / (1024 * 1024),
                ConsciousnessEntities = _actors.Values.Count(a => a.Configuration.EnableConsciousness)
            };
        }

        // Private helper methods

        private async Task CreateDefaultNamespacesAsync()
        {
            await GetOrCreateNamespaceAsync("default");
            await GetOrCreateNamespaceAsync("unity");
            await GetOrCreateNamespaceAsync("consciousness");
            await GetOrCreateNamespaceAsync("ai");
        }

        private async Task<ActorNamespace> GetOrCreateNamespaceAsync(string namespaceName)
        {
            if (_namespaces.TryGetValue(namespaceName, out var existing))
            {
                return existing;
            }
            
            var newNamespace = await _namespaceManager.CreateNamespaceAsync(namespaceName);
            _namespaces.TryAdd(namespaceName, newNamespace);
            
            _logger.LogInformation("📁 Created namespace: {Namespace}", namespaceName);
            return newNamespace;
        }

        private async Task ValidateActorConfigurationAsync(ActorConfiguration config)
        {
            if (string.IsNullOrEmpty(config.Namespace))
            {
                config.Namespace = "default";
            }
            
            await Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync()
        {
            _logger.LogInformation("📨 Message processor started");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (_messageQueue.TryDequeue(out var message))
                    {
                        await ProcessSingleMessageAsync(message);
                    }
                    else
                    {
                        await Task.Delay(1, _cancellationTokenSource.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing message");
                }
            }
        }

        private async Task ProcessSingleMessageAsync(ActorMessage message)
        {
            try
            {
                if (_actors.TryGetValue(message.ToActorId, out var targetActor))
                {
                    await targetActor.ReceiveMessageAsync(message);
                    
                    _logger.LogDebug("📬 Delivered message: {MessageId} to {ToActor}", 
                        message.MessageId, message.ToActorId);
                }
                else
                {
                    _logger.LogWarning("⚠️ Target actor not found for message: {MessageId} to {ToActor}", 
                        message.MessageId, message.ToActorId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error delivering message: {MessageId}", message.MessageId);
            }
        }

        private void PerformMaintenance(object? state)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // Cleanup inactive actors
                    await CleanupInactiveActorsAsync();
                    
                    // Optimize memory usage
                    GC.Collect();
                    
                    // Log maintenance metrics
                    var metrics = await GetRuntimeMetricsAsync();
                    _logger.LogInformation("🔧 Maintenance: {ActiveActors} actors, {MemoryMB}MB memory", 
                        metrics.ActiveActors, metrics.MemoryUsageMB);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during maintenance");
                }
            });
        }

        private async Task CleanupInactiveActorsAsync()
        {
            var inactiveActors = new List<string>();
            
            foreach (var kvp in _actors)
            {
                var actor = kvp.Value;
                if (!await actor.IsActiveAsync())
                {
                    inactiveActors.Add(kvp.Key);
                }
            }
            
            foreach (var actorId in inactiveActors)
            {
                await DestroyActorAsync(actorId);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _maintenanceTimer?.Dispose();
            _transactionSemaphore?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
