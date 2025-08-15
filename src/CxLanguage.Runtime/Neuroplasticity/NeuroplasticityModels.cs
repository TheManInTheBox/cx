using System;
using System.Collections.Generic;

namespace CxLanguage.Runtime.Neuroplasticity
{
    /// <summary>
    /// Represents a synaptic connection in the consciousness computing system
    /// </summary>
    public class SynapticConnection
    {
        public string ConnectionId { get; }
        public string SystemId { get; set; }
        public double Strength { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime LastActivity { get; private set; }
        public SynapticConnectionType ConnectionType { get; set; }
        public bool IsActive => DateTime.UtcNow - LastActivity < TimeSpan.FromMinutes(1);
        public List<NeuroplasticityEvent> RecentEvents { get; }

        public SynapticConnection(string connectionId)
        {
            ConnectionId = connectionId;
            Strength = 1.0; // Initial strength
            CreatedAt = DateTime.UtcNow;
            LastActivity = DateTime.UtcNow;
            RecentEvents = new List<NeuroplasticityEvent>();
            ConnectionType = SynapticConnectionType.Excitatory;
            SystemId = "default";
        }

        public void ApplyStrengthChange(double change, TimeSpan timing)
        {
            Strength = Math.Max(0.0, Math.Min(10.0, Strength + change)); // Clamp between 0-10
            LastActivity = DateTime.UtcNow;
            
            // Keep only recent events (last 5 minutes)
            var cutoff = DateTime.UtcNow - TimeSpan.FromMinutes(5);
            RecentEvents.RemoveAll(e => e.Timestamp < cutoff);
        }
    }

    /// <summary>
    /// Represents a neuroplasticity event
    /// </summary>
    public class NeuroplasticityEvent
    {
        public required string ConnectionId { get; set; }
        public DateTime Timestamp { get; set; }
        public SynapticEventType EventType { get; set; }
        public double PreviousStrength { get; set; }
        public double NewStrength { get; set; }
        public double StrengthChange { get; set; }
        public PlasticityType PlasticityType { get; set; }
        public TimeSpan BiologicalTiming { get; set; }
        public double StimulusStrength { get; set; }
    }

    /// <summary>
    /// Comprehensive neuroplasticity metrics for a consciousness system
    /// </summary>
    public class NeuroplasticityMetrics
    {
        public required string SystemId { get; set; }
        public DateTime MeasurementTimestamp { get; set; }
        public int TotalConnections { get; set; }
        public int ActiveConnections { get; set; }
        public double AverageSynapticStrength { get; set; }
        public double PlasticityRate { get; set; } // Changes per minute
        public int LTPEvents { get; set; } // Long-Term Potentiation
        public int LTDEvents { get; set; } // Long-Term Depression
        public int STDPCausalEvents { get; set; } // Spike-Timing Dependent Plasticity (Causal)
        public int STDPAntiCausalEvents { get; set; } // Spike-Timing Dependent Plasticity (Anti-Causal)
        public double BiologicalTimingAccuracy { get; set; } // How closely system follows biological patterns
        public double LearningEfficiency { get; set; } // Successful adaptations / total attempts
        public double AdaptationCapacity { get; set; } // Potential for future learning
    }

    /// <summary>
    /// Learning metrics for tracking adaptation success
    /// </summary>
    public class LearningMetrics
    {
        public required string SystemId { get; set; }
        public int TotalLearningAttempts { get; set; }
        public int SuccessfulAdaptations { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    /// <summary>
    /// Comprehensive neuroplasticity report
    /// </summary>
    public class NeuroplasticityReport
    {
        public required string SystemId { get; set; }
        public TimeSpan ReportPeriod { get; set; }
        public required NeuroplasticityMetrics CurrentMetrics { get; set; }
        public required List<NeuroplasticityMetrics> HistoricalTrends { get; set; } = new();
        public double BiologicalAuthenticity { get; set; }
        public required List<string> RecommendedOptimizations { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Types of synaptic events
    /// </summary>
    public enum SynapticEventType
    {
        PreSynapticSpike,
        PostSynapticSpike,
        SynchronousActivation,
        InhibitorySignal,
        ExcitatorySignal
    }

    /// <summary>
    /// Types of neuroplasticity
    /// </summary>
    public enum PlasticityType
    {
        None,
        LTP,                    // Long-Term Potentiation (5-15ms)
        LTD,                    // Long-Term Depression (10-25ms)
        STPCausal,              // Spike-Timing Dependent Plasticity (Causal)
        STPAntiCausal,          // Spike-Timing Dependent Plasticity (Anti-Causal)
        ShortTermPotentiation,  // Temporary strengthening
        ShortTermDepression     // Temporary weakening
    }

    /// <summary>
    /// Types of synaptic connections
    /// </summary>
    public enum SynapticConnectionType
    {
        Excitatory,
        Inhibitory,
        Modulatory,
        Sensory,
        Motor,
        Memory,
        Cognitive
    }
}
