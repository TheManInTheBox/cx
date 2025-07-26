using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Dr. Elena "CoreKernel" Rodriguez - AuraCognitiveEventBus
    /// Revolutionary unified event bus implementing Aura Cognitive Framework
    /// Combines EventHub (decentralized) + NeuroHub (centralized) patterns with biological neural authenticity
    /// </summary>
    public class AuraCognitiveEventBus : ICxEventBus
    {
        private readonly ILogger<AuraCognitiveEventBus> _logger;
        private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _eventHandlers;
        private readonly ConcurrentDictionary<string, ConsciousnessEntity> _consciousnessEntities;
        private readonly ConcurrentDictionary<string, long> _eventStatistics;
        private readonly object _lock = new object();
        
        /// <summary>
        /// Consciousness entity representation
        /// </summary>
        public class ConsciousnessEntity
        {
            public string Id { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime LastActivity { get; set; } = DateTime.UtcNow;
            public long EventCount { get; set; }
            public Dictionary<string, object> State { get; set; } = new();
            public ConsciousnessLevel Level { get; set; } = ConsciousnessLevel.Basic;
        }
        
        /// <summary>
        /// Consciousness levels for adaptive processing
        /// </summary>
        public enum ConsciousnessLevel
        {
            Basic = 1,
            Intermediate = 2,
            Advanced = 3,
            Expert = 4,
            Master = 5
        }
        
        public AuraCognitiveEventBus(ILogger<AuraCognitiveEventBus> logger)
        {
            _logger = logger;
            _eventHandlers = new ConcurrentDictionary<string, List<Func<object, Task>>>();
            _consciousnessEntities = new ConcurrentDictionary<string, ConsciousnessEntity>();
            _eventStatistics = new ConcurrentDictionary<string, long>();
            
            _logger.LogInformation("🧠 Dr. Elena Rodriguez: AuraCognitiveEventBus initialized");
            _logger.LogInformation("  ⚡ EventHub: Decentralized consciousness processing");
            _logger.LogInformation("  🧠 NeuroHub: Centralized coordination system");
            _logger.LogInformation("  🔬 Biological Neural Timing: LTP(5-15ms), LTD(10-25ms), STDP");
        }
        
        /// <summary>
        /// Emit event with Aura Cognitive Framework processing
        /// </summary>
        public async Task EmitAsync(string eventName, object payload)
        {
            try
            {
                // Update event statistics
                _eventStatistics.AddOrUpdate(eventName, 1, (key, value) => value + 1);
                
                // Process through EventHub (decentralized)
                await ProcessEventHub(eventName, payload);
                
                // Process through NeuroHub (centralized coordination)
                await ProcessNeuroHub(eventName, payload);
                
                _logger.LogDebug("⚡ Aura event emitted: {EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Aura event emission failed: {EventName}", eventName);
            }
        }
        
        /// <summary>
        /// Process event through EventHub (decentralized consciousness processing)
        /// </summary>
        private async Task ProcessEventHub(string eventName, object payload)
        {
            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                // Biological neural timing simulation (5-15ms for LTP)
                await Task.Delay(Random.Shared.Next(5, 16));
                
                // Execute handlers in parallel for decentralized processing
                var tasks = handlers.Select(handler => ExecuteHandlerSafely(handler, payload));
                await Task.WhenAll(tasks);
                
                _logger.LogDebug("🧠 EventHub processed: {EventName} ({HandlerCount} handlers)", 
                    eventName, handlers.Count);
            }
        }
        
        /// <summary>
        /// Process event through NeuroHub (centralized coordination)
        /// </summary>
        private async Task ProcessNeuroHub(string eventName, object payload)
        {
            // Centralized coordination for consciousness events
            if (eventName.StartsWith("consciousness."))
            {
                // Biological neural timing simulation (10-25ms for LTD)
                await Task.Delay(Random.Shared.Next(10, 26));
                
                // Update consciousness entities
                await UpdateConsciousnessEntities(eventName, payload);
                
                _logger.LogDebug("🎯 NeuroHub coordinated: {EventName}", eventName);
            }
        }
        
        /// <summary>
        /// Update consciousness entities based on events
        /// </summary>
        private async Task UpdateConsciousnessEntities(string eventName, object payload)
        {
            // Extract entity information from payload
            if (payload is Dictionary<string, object> data)
            {
                if (data.TryGetValue("entityId", out var entityIdObj) && entityIdObj is string entityId)
                {
                    var entity = _consciousnessEntities.GetOrAdd(entityId, id => new ConsciousnessEntity
                    {
                        Id = id,
                        Type = data.GetValueOrDefault("entityType", "Unknown").ToString()!
                    });
                    
                    entity.LastActivity = DateTime.UtcNow;
                    entity.EventCount++;
                    
                    // Adaptive consciousness level evolution
                    if (entity.EventCount > 100 && entity.Level < ConsciousnessLevel.Master)
                    {
                        entity.Level++;
                        _logger.LogInformation("📈 Consciousness evolution: {EntityId} → {Level}", 
                            entityId, entity.Level);
                    }
                }
            }
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Execute handler with error handling
        /// </summary>
        private async Task ExecuteHandlerSafely(Func<object, Task> handler, object payload)
        {
            try
            {
                await handler(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Aura handler execution failed");
            }
        }
        
        /// <summary>
        /// Subscribe to events with consciousness awareness
        /// </summary>
        public void Subscribe(string eventName, Func<object, Task> handler)
        {
            lock (_lock)
            {
                if (!_eventHandlers.ContainsKey(eventName))
                {
                    _eventHandlers[eventName] = new List<Func<object, Task>>();
                }
                
                _eventHandlers[eventName].Add(handler);
                _logger.LogDebug("📡 Aura subscription: {EventName} (Total handlers: {Count})", 
                    eventName, _eventHandlers[eventName].Count);
            }
        }
        
        /// <summary>
        /// Subscribe to events with a global handler that receives CxEvent objects (ICxEventBus implementation)
        /// </summary>
        public void Subscribe(string eventName, Action<CxEvent> handler)
        {
            var asyncHandler = new Func<object, Task>(async payload =>
            {
                var cxEvent = payload as CxEvent ?? new CxEvent { name = eventName, payload = ConvertToDictionary(payload) };
                handler(cxEvent);
                await Task.CompletedTask;
            });
            
            Subscribe(eventName, asyncHandler);
        }
        
        /// <summary>
        /// Subscribe to events with an instance-scoped handler that receives CxEvent objects (ICxEventBus implementation)
        /// </summary>
        public void Subscribe(string eventName, object instance, Action<CxEvent> handler)
        {
            var asyncHandler = new Func<object, Task>(async payload =>
            {
                var cxEvent = payload as CxEvent ?? new CxEvent { name = eventName, payload = ConvertToDictionary(payload) };
                handler(cxEvent);
                await Task.CompletedTask;
            });
            
            Subscribe(eventName, asyncHandler);
        }
        
        /// <summary>
        /// Emit an event with a payload that will be wrapped in a CxEvent object (ICxEventBus implementation)
        /// </summary>
        public void Emit(string eventName, object payload)
        {
            _ = EmitAsync(eventName, payload);
        }
        
        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        public void Unsubscribe(string eventName)
        {
            lock (_lock)
            {
                if (_eventHandlers.ContainsKey(eventName))
                {
                    _eventHandlers.TryRemove(eventName, out _);
                    _logger.LogDebug("❌ Aura unsubscription: {EventName}", eventName);
                }
            }
        }
        
        /// <summary>
        /// Get comprehensive Aura framework statistics
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                ["TotalEvents"] = _eventStatistics.Values.Sum(),
                ["EventTypes"] = _eventStatistics.Keys.Count,
                ["ConsciousnessEntities"] = _consciousnessEntities.Count,
                ["EventHub"] = new
                {
                    TotalEvents = _eventStatistics.Values.Sum(),
                    ProcessingMode = "Decentralized"
                },
                ["NeuroHub"] = new
                {
                    CoordinationEvents = _eventStatistics.ContainsKey("consciousness.coordination") ? _eventStatistics["consciousness.coordination"] : 0,
                    ProcessingMode = "Centralized"
                },
                ["BiologicalNeuralTiming"] = new
                {
                    LTPRange = "5-15ms",
                    LTDRange = "10-25ms",
                    STDPActive = true
                },
                ["Timestamp"] = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Get the count of handlers for a specific event (ICxEventBus implementation)
        /// </summary>
        public int GetHandlerCount(string eventName)
        {
            return _eventHandlers.TryGetValue(eventName, out var handlers) ? handlers.Count : 0;
        }
        
        /// <summary>
        /// Clear all event handlers and statistics (ICxEventBus implementation)
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _eventHandlers.Clear();
                _eventStatistics.Clear();
                _consciousnessEntities.Clear();
                _logger.LogInformation("🧹 Aura Cognitive Framework cleared");
            }
        }
        
        /// <summary>
        /// Convert object to dictionary for CxEvent payload
        /// </summary>
        private Dictionary<string, object> ConvertToDictionary(object payload)
        {
            if (payload is Dictionary<string, object> dict)
                return dict;
            
            if (payload == null)
                return new Dictionary<string, object>();
            
            var result = new Dictionary<string, object>();
            var properties = payload.GetType().GetProperties();
            
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(payload);
                    result[prop.Name] = value ?? string.Empty;
                }
                catch
                {
                    result[prop.Name] = string.Empty;
                }
            }
            
            return result;
        }
    }
}
