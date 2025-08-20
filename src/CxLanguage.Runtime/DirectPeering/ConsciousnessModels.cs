using System;
using System.Collections.Generic;

namespace CxLanguage.Runtime.DirectPeering
{
    /// <summary>
    /// Consciousness event for neural-speed peer communication.
    /// Optimized for sub-millisecond transmission between agents.
    /// </summary>
    public class ConsciousnessEvent
    {
        /// <summary>
        /// Unique identifier for the consciousness event
        /// </summary>
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Type of consciousness event (e.g., "consciousness.state.sync", "neural.activation")
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// High-precision timestamp for neural timing accuracy
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Neural timing timestamp for biological authenticity
        /// </summary>
        public DateTime NeuralTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Source agent identifier
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Target agent identifier (null for broadcast)
        /// </summary>
        public string? Target { get; set; }

        /// <summary>
        /// Consciousness event data payload
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new();

        /// <summary>
        /// Priority level for consciousness processing (0 = highest, 10 = lowest)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Consciousness coherence level (0.0 to 1.0)
        /// </summary>
        public double CoherenceLevel { get; set; } = 1.0;

        /// <summary>
        /// Neural pathway identifier for biological authenticity
        /// </summary>
        public string? NeuralPathway { get; set; }

        /// <summary>
        /// Synaptic strength for event processing (0.0 to 1.0)
        /// </summary>
        public double SynapticStrength { get; set; } = 0.8;

        /// <summary>
        /// Biological authenticity flag for neural processing
        /// </summary>
        public bool BiologicalAuthenticity { get; set; } = true;

        /// <summary>
        /// Correlation ID for multi-event consciousness sequences
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Create consciousness event for peer communication
        /// </summary>
        public static ConsciousnessEvent Create(string eventType, string source, Dictionary<string, object>? data = null)
        {
            return new ConsciousnessEvent
            {
                EventType = eventType,
                Source = source,
                Data = data ?? new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// Create high-priority consciousness event
        /// </summary>
        public static ConsciousnessEvent CreateHighPriority(string eventType, string source, Dictionary<string, object>? data = null)
        {
            return new ConsciousnessEvent
            {
                EventType = eventType,
                Source = source,
                Data = data ?? new Dictionary<string, object>(),
                Priority = 0,
                CoherenceLevel = 1.0,
                SynapticStrength = 1.0
            };
        }

        /// <summary>
        /// Create consciousness synchronization event
        /// </summary>
        public static ConsciousnessEvent CreateSyncEvent(string source, string target, ConsciousnessState state)
        {
            return new ConsciousnessEvent
            {
                EventType = "consciousness.state.sync",
                Source = source,
                Target = target,
                Data = state.ToDictionary(),
                Priority = 1,
                CoherenceLevel = state.CoherenceLevel,
                CorrelationId = Guid.NewGuid().ToString()
            };
        }
    }

    /// <summary>
    /// Consciousness state for distributed processing coordination
    /// </summary>
    public class ConsciousnessState
    {
        /// <summary>
        /// Agent consciousness identifier
        /// </summary>
        public string AgentId { get; set; } = string.Empty;

        /// <summary>
        /// Current consciousness level (0.0 to 1.0)
        /// </summary>
        public double ConsciousnessLevel { get; set; } = 0.8;

        /// <summary>
        /// Consciousness coherence (0.0 to 1.0)
        /// </summary>
        public double CoherenceLevel { get; set; } = 0.9;

        /// <summary>
        /// Current processing capacity (0.0 to 1.0)
        /// </summary>
        public double ProcessingCapacity { get; set; } = 0.7;

        /// <summary>
        /// Neural activation patterns
        /// </summary>
        public Dictionary<string, double> NeuralActivations { get; set; } = new();

        /// <summary>
        /// Active consciousness contexts
        /// </summary>
        public List<string> ActiveContexts { get; set; } = new();

        /// <summary>
        /// Memory state snapshot
        /// </summary>
        public Dictionary<string, object> MemoryState { get; set; } = new();

        /// <summary>
        /// Attention focus areas
        /// </summary>
        public List<string> AttentionFocus { get; set; } = new();

        /// <summary>
        /// Emotional state indicators
        /// </summary>
        public Dictionary<string, double> EmotionalState { get; set; } = new();

        /// <summary>
        /// Last state update timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Convert consciousness state to dictionary for transmission
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["agentId"] = AgentId,
                ["consciousnessLevel"] = ConsciousnessLevel,
                ["coherenceLevel"] = CoherenceLevel,
                ["processingCapacity"] = ProcessingCapacity,
                ["neuralActivations"] = NeuralActivations,
                ["activeContexts"] = ActiveContexts,
                ["memoryState"] = MemoryState,
                ["attentionFocus"] = AttentionFocus,
                ["emotionalState"] = EmotionalState,
                ["lastUpdated"] = LastUpdated
            };
        }

        /// <summary>
        /// Create consciousness state from dictionary
        /// </summary>
        public static ConsciousnessState FromDictionary(Dictionary<string, object> data)
        {
            var state = new ConsciousnessState();
            
            if (data.TryGetValue("agentId", out var agentId))
                state.AgentId = agentId.ToString() ?? string.Empty;
            
            if (data.TryGetValue("consciousnessLevel", out var consciousnessLevel))
                state.ConsciousnessLevel = Convert.ToDouble(consciousnessLevel);
            
            if (data.TryGetValue("coherenceLevel", out var coherenceLevel))
                state.CoherenceLevel = Convert.ToDouble(coherenceLevel);
            
            if (data.TryGetValue("processingCapacity", out var processingCapacity))
                state.ProcessingCapacity = Convert.ToDouble(processingCapacity);
            
            // Add other property mappings as needed
            
            return state;
        }
    }

    /// <summary>
    /// Performance metrics for Direct EventHub Peer monitoring
    /// </summary>
    public class PerformanceMetrics
    {
        private readonly object _lock = new();
        private long _eventsSent;
        private long _eventsReceived;
        private readonly List<TimeSpan> _transmissionLatencies = new();
        private readonly List<TimeSpan> _peeringLatencies = new();
        private readonly List<TimeSpan> _synchronizationLatencies = new();

        /// <summary>
        /// Total events sent through peer connection
        /// </summary>
        public long EventsSent => _eventsSent;

        /// <summary>
        /// Total events received through peer connection
        /// </summary>
        public long EventsReceived => _eventsReceived;

        /// <summary>
        /// Average transmission latency
        /// </summary>
        public TimeSpan AverageTransmissionLatency
        {
            get
            {
                lock (_lock)
                {
                    if (_transmissionLatencies.Count == 0) return TimeSpan.Zero;
                    var totalTicks = _transmissionLatencies.Sum(l => l.Ticks);
                    return new TimeSpan(totalTicks / _transmissionLatencies.Count);
                }
            }
        }

        /// <summary>
        /// Minimum transmission latency achieved
        /// </summary>
        public TimeSpan MinimumTransmissionLatency
        {
            get
            {
                lock (_lock)
                {
                    return _transmissionLatencies.Count > 0 ? _transmissionLatencies.Min() : TimeSpan.Zero;
                }
            }
        }

        /// <summary>
        /// Maximum transmission latency recorded
        /// </summary>
        public TimeSpan MaximumTransmissionLatency
        {
            get
            {
                lock (_lock)
                {
                    return _transmissionLatencies.Count > 0 ? _transmissionLatencies.Max() : TimeSpan.Zero;
                }
            }
        }

        /// <summary>
        /// Percentage of transmissions achieving sub-millisecond target
        /// </summary>
        public double SubMillisecondAchievementRate
        {
            get
            {
                lock (_lock)
                {
                    if (_transmissionLatencies.Count == 0) return 0.0;
                    var subMillisecondCount = _transmissionLatencies.Count(l => l.TotalMilliseconds < 1.0);
                    return (double)subMillisecondCount / _transmissionLatencies.Count * 100.0;
                }
            }
        }

        /// <summary>
        /// Events per second throughput
        /// </summary>
        public double EventsPerSecond { get; private set; }

        /// <summary>
        /// Record transmission latency measurement
        /// </summary>
        public void RecordTransmissionLatency(TimeSpan latency)
        {
            lock (_lock)
            {
                _transmissionLatencies.Add(latency);
                Interlocked.Increment(ref _eventsSent);
                
                // Keep only recent measurements (last 1000 events)
                if (_transmissionLatencies.Count > 1000)
                {
                    _transmissionLatencies.RemoveAt(0);
                }
                
                UpdateThroughput();
            }
        }

        /// <summary>
        /// Record peering establishment latency
        /// </summary>
        public void RecordPeeringLatency(TimeSpan latency)
        {
            lock (_lock)
            {
                _peeringLatencies.Add(latency);
                
                // Keep only recent measurements
                if (_peeringLatencies.Count > 100)
                {
                    _peeringLatencies.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Record consciousness synchronization latency
        /// </summary>
        public void RecordSynchronizationLatency(TimeSpan latency)
        {
            lock (_lock)
            {
                _synchronizationLatencies.Add(latency);
                
                // Keep only recent measurements
                if (_synchronizationLatencies.Count > 100)
                {
                    _synchronizationLatencies.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Record event received
        /// </summary>
        public void RecordEventReceived()
        {
            Interlocked.Increment(ref _eventsReceived);
            UpdateThroughput();
        }

        /// <summary>
        /// Get performance summary for monitoring
        /// </summary>
        public Dictionary<string, object> GetPerformanceSummary()
        {
            lock (_lock)
            {
                return new Dictionary<string, object>
                {
                    ["eventsSent"] = EventsSent,
                    ["eventsReceived"] = EventsReceived,
                    ["eventsPerSecond"] = EventsPerSecond,
                    ["averageLatencyMs"] = AverageTransmissionLatency.TotalMilliseconds,
                    ["minLatencyMs"] = MinimumTransmissionLatency.TotalMilliseconds,
                    ["maxLatencyMs"] = MaximumTransmissionLatency.TotalMilliseconds,
                    ["subMillisecondRate"] = SubMillisecondAchievementRate,
                    ["measurementCount"] = _transmissionLatencies.Count
                };
            }
        }

        private void UpdateThroughput()
        {
            // Calculate events per second based on recent activity
            var totalEvents = EventsSent + EventsReceived;
            if (totalEvents > 0)
            {
                // Simple throughput calculation - would be enhanced with time windowing
                EventsPerSecond = totalEvents / Math.Max(1, DateTime.UtcNow.Second);
            }
        }
    }
}
