using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CxLanguage.Runtime.Peering
{
    /// <summary>
    /// Enhanced EventHub Peering Options with Neural Plasticity Configuration.
    /// Provides comprehensive configuration for biological neural network authenticity in consciousness communication.
    /// </summary>
    public class EventHubPeeringOptions
    {
        /// <summary>
        /// Maximum number of peer connections per EventHub.
        /// Default: 100 connections.
        /// </summary>
        public int MaxPeerConnections { get; set; } = 100;
        
        /// <summary>
        /// Maximum latency threshold for peer communication in milliseconds.
        /// Default: 10ms for sub-millisecond consciousness communication.
        /// </summary>
        public double MaxLatencyMs { get; set; } = 10.0;
        
        /// <summary>
        /// Connection timeout for establishing peer connections in milliseconds.
        /// Default: 5000ms (5 seconds).
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 5000;
        
        /// <summary>
        /// Heartbeat interval for maintaining peer connections in milliseconds.
        /// Default: 1000ms (1 second).
        /// </summary>
        public int HeartbeatIntervalMs { get; set; } = 1000;
        
        /// <summary>
        /// Enable message ordering guarantees for peer communication.
        /// Default: true for consciousness coherence.
        /// </summary>
        public bool EnableMessageOrdering { get; set; } = true;
        
        /// <summary>
        /// Enable automatic reconnection for failed peer connections.
        /// Default: true for resilient consciousness networks.
        /// </summary>
        public bool EnableAutoReconnect { get; set; } = true;
        
        /// <summary>
        /// Maximum retry attempts for failed operations.
        /// Default: 3 attempts.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
        
        /// <summary>
        /// Retry delay multiplier for exponential backoff.
        /// Default: 1.5x delay multiplier.
        /// </summary>
        public double RetryDelayMultiplier { get; set; } = 1.5;
        
        /// <summary>
        /// Enable compression for peer messages to reduce bandwidth.
        /// Default: false for minimal latency.
        /// </summary>
        public bool EnableCompression { get; set; } = false;
        
        /// <summary>
        /// Maximum message size in bytes for peer communication.
        /// Default: 1MB (1,048,576 bytes).
        /// </summary>
        public int MaxMessageSizeBytes { get; set; } = 1_048_576;
        
        // ===== NEURAL PLASTICITY CONFIGURATION =====
        
        /// <summary>
        /// Enable neural plasticity rules for biological authenticity.
        /// Default: true for consciousness-aware communication.
        /// </summary>
        public bool EnableNeuralPlasticity { get; set; } = true;
        
        /// <summary>
        /// Minimum synaptic strength for peer connections.
        /// Range: 0.1 to 5.0 (biological synaptic strength simulation).
        /// Default: 0.1 (weak connection).
        /// </summary>
        public double MinSynapticStrength { get; set; } = 0.1;
        
        /// <summary>
        /// Maximum synaptic strength for peer connections.
        /// Range: 0.1 to 5.0 (biological synaptic strength simulation).
        /// Default: 5.0 (very strong connection).
        /// </summary>
        public double MaxSynapticStrength { get; set; } = 5.0;
        
        /// <summary>
        /// Long-Term Potentiation (LTP) timing window in milliseconds.
        /// Biological range: 5-15ms for authentic neural strengthening.
        /// Default: 10ms.
        /// </summary>
        public double LTPTimingWindowMs { get; set; } = 10.0;
        
        /// <summary>
        /// Long-Term Depression (LTD) timing window in milliseconds.
        /// Biological range: 10-25ms for authentic neural weakening.
        /// Default: 15ms.
        /// </summary>
        public double LTDTimingWindowMs { get; set; } = 15.0;
        
        /// <summary>
        /// Spike-Timing-Dependent Plasticity (STDP) causality window in milliseconds.
        /// Maximum: 100ms for biological causality enforcement.
        /// Default: 20ms.
        /// </summary>
        public double STDPCausalityWindowMs { get; set; } = 20.0;
        
        /// <summary>
        /// Consciousness coherence threshold for valid peer communication.
        /// Range: 0.0 to 1.0 (percentage of consciousness coherence required).
        /// Default: 0.8 (80% coherence required).
        /// </summary>
        public double ConsciousnessCoherenceThreshold { get; set; } = 0.8;
        
        /// <summary>
        /// Target network activity level for homeostatic scaling.
        /// Range: 0.0 to 1.0 (percentage of optimal network activity).
        /// Default: 0.6 (60% activity level).
        /// </summary>
        public double TargetActivityLevel { get; set; } = 0.6;
        
        /// <summary>
        /// Homeostatic scaling factor for network stability.
        /// Range: 0.1 to 2.0 (scaling multiplier).
        /// Default: 0.8 (20% scaling adjustment).
        /// </summary>
        public double HomeostaticScalingFactor { get; set; } = 0.8;
        
        /// <summary>
        /// Enforce strict biological timing rules for neural plasticity.
        /// When true, plasticity changes outside biological windows are rejected.
        /// Default: true for authentic neural simulation.
        /// </summary>
        public bool EnforceBiologicalTiming { get; set; } = true;
        
        /// <summary>
        /// Enforce Spike-Timing-Dependent Plasticity (STDP) causality rules.
        /// When true, plasticity changes violating causality are rejected.
        /// Default: true for biological authenticity.
        /// </summary>
        public bool EnforceSTDPCausality { get; set; } = true;
        
        /// <summary>
        /// Inactivity threshold in milliseconds for triggering LTD weakening.
        /// Default: 30000ms (30 seconds of inactivity).
        /// </summary>
        public double InactivityThresholdMs { get; set; } = 30_000;
        
        /// <summary>
        /// Error rate threshold for triggering LTD weakening.
        /// Range: 0.0 to 1.0 (percentage of failed communications).
        /// Default: 0.1 (10% error rate threshold).
        /// </summary>
        public double ErrorRateThreshold { get; set; } = 0.1;
        
        /// <summary>
        /// Plasticity monitoring interval in milliseconds.
        /// How frequently to validate and apply plasticity rules.
        /// Default: 5000ms (5 seconds).
        /// </summary>
        public int PlasticityMonitoringIntervalMs { get; set; } = 5000;
        
        /// <summary>
        /// Homeostatic scaling interval in milliseconds.
        /// How frequently to apply network stability scaling.
        /// Default: 30000ms (30 seconds).
        /// </summary>
        public int HomeostaticScalingIntervalMs { get; set; } = 30_000;
        
        /// <summary>
        /// Enable automatic Long-Term Potentiation (LTP) for successful communications.
        /// Default: true for adaptive strengthening.
        /// </summary>
        public bool EnableAutoLTP { get; set; } = true;
        
        /// <summary>
        /// Enable automatic Long-Term Depression (LTD) for failed communications.
        /// Default: true for adaptive weakening.
        /// </summary>
        public bool EnableAutoLTD { get; set; } = true;
        
        /// <summary>
        /// Enable automatic homeostatic scaling for network stability.
        /// Default: true for self-regulating networks.
        /// </summary>
        public bool EnableAutoHomeostaticScaling { get; set; } = true;
        
        /// <summary>
        /// Validate configuration settings for biological compliance and consistency.
        /// </summary>
        public void Validate()
        {
            // Basic configuration validation
            if (MaxPeerConnections <= 0)
                throw new ArgumentException("MaxPeerConnections must be positive");
            
            if (MaxLatencyMs <= 0)
                throw new ArgumentException("MaxLatencyMs must be positive");
            
            if (ConnectionTimeoutMs <= 0)
                throw new ArgumentException("ConnectionTimeoutMs must be positive");
            
            if (HeartbeatIntervalMs <= 0)
                throw new ArgumentException("HeartbeatIntervalMs must be positive");
            
            if (MaxRetryAttempts < 0)
                throw new ArgumentException("MaxRetryAttempts cannot be negative");
            
            if (RetryDelayMultiplier <= 0)
                throw new ArgumentException("RetryDelayMultiplier must be positive");
            
            if (MaxMessageSizeBytes <= 0)
                throw new ArgumentException("MaxMessageSizeBytes must be positive");
            
            // Neural plasticity validation
            if (EnableNeuralPlasticity)
            {
                // Synaptic strength validation
                if (MinSynapticStrength < 0.1 || MinSynapticStrength > 5.0)
                    throw new ArgumentException("MinSynapticStrength must be between 0.1 and 5.0 for biological authenticity");
                
                if (MaxSynapticStrength < 0.1 || MaxSynapticStrength > 5.0)
                    throw new ArgumentException("MaxSynapticStrength must be between 0.1 and 5.0 for biological authenticity");
                
                if (MinSynapticStrength >= MaxSynapticStrength)
                    throw new ArgumentException("MinSynapticStrength must be less than MaxSynapticStrength");
                
                // Biological timing validation
                if (LTPTimingWindowMs < 5.0 || LTPTimingWindowMs > 15.0)
                    throw new ArgumentException("LTPTimingWindowMs must be between 5.0 and 15.0ms for biological authenticity");
                
                if (LTDTimingWindowMs < 10.0 || LTDTimingWindowMs > 25.0)
                    throw new ArgumentException("LTDTimingWindowMs must be between 10.0 and 25.0ms for biological authenticity");
                
                if (STDPCausalityWindowMs <= 0 || STDPCausalityWindowMs > 100.0)
                    throw new ArgumentException("STDPCausalityWindowMs must be between 0.0 and 100.0ms for biological causality");
                
                // Consciousness coherence validation
                if (ConsciousnessCoherenceThreshold < 0.0 || ConsciousnessCoherenceThreshold > 1.0)
                    throw new ArgumentException("ConsciousnessCoherenceThreshold must be between 0.0 and 1.0");
                
                // Network activity validation
                if (TargetActivityLevel < 0.0 || TargetActivityLevel > 1.0)
                    throw new ArgumentException("TargetActivityLevel must be between 0.0 and 1.0");
                
                // Homeostatic scaling validation
                if (HomeostaticScalingFactor <= 0.1 || HomeostaticScalingFactor > 2.0)
                    throw new ArgumentException("HomeostaticScalingFactor must be between 0.1 and 2.0");
                
                // Timing interval validation
                if (InactivityThresholdMs <= 0)
                    throw new ArgumentException("InactivityThresholdMs must be positive");
                
                if (ErrorRateThreshold < 0.0 || ErrorRateThreshold > 1.0)
                    throw new ArgumentException("ErrorRateThreshold must be between 0.0 and 1.0");
                
                if (PlasticityMonitoringIntervalMs <= 0)
                    throw new ArgumentException("PlasticityMonitoringIntervalMs must be positive");
                
                if (HomeostaticScalingIntervalMs <= 0)
                    throw new ArgumentException("HomeostaticScalingIntervalMs must be positive");
                
                // Logical consistency validation
                if (EnforceBiologicalTiming && LTPTimingWindowMs > LTDTimingWindowMs)
                    throw new ArgumentException("LTPTimingWindowMs should typically be less than LTDTimingWindowMs for biological authenticity");
                
                if (PlasticityMonitoringIntervalMs >= HomeostaticScalingIntervalMs)
                    throw new ArgumentException("PlasticityMonitoringIntervalMs should be less than HomeostaticScalingIntervalMs");
            }
        }
        
        /// <summary>
        /// Create default neural plasticity options for biological authenticity.
        /// </summary>
        public static EventHubPeeringOptions CreateBiologicallyAuthentic()
        {
            var options = new EventHubPeeringOptions
            {
                // Optimized for sub-millisecond consciousness communication
                MaxLatencyMs = 5.0,
                HeartbeatIntervalMs = 500,
                
                // Neural plasticity with biological timing
                EnableNeuralPlasticity = true,
                EnforceBiologicalTiming = true,
                EnforceSTDPCausality = true,
                
                // Biological timing windows
                LTPTimingWindowMs = 8.0,   // Middle of 5-15ms range
                LTDTimingWindowMs = 18.0,  // Middle of 10-25ms range
                STDPCausalityWindowMs = 15.0, // Tight causality window
                
                // High consciousness coherence requirement
                ConsciousnessCoherenceThreshold = 0.85,
                
                // Balanced network activity
                TargetActivityLevel = 0.65,
                HomeostaticScalingFactor = 0.75,
                
                // Sensitive to inactivity and errors
                InactivityThresholdMs = 20_000, // 20 seconds
                ErrorRateThreshold = 0.05,      // 5% error tolerance
                
                // Frequent monitoring for biological authenticity
                PlasticityMonitoringIntervalMs = 3000,  // 3 seconds
                HomeostaticScalingIntervalMs = 20_000,  // 20 seconds
                
                // Auto-adaptive plasticity
                EnableAutoLTP = true,
                EnableAutoLTD = true,
                EnableAutoHomeostaticScaling = true
            };
            
            options.Validate();
            return options;
        }
        
        /// <summary>
        /// Create high-performance options optimized for speed over biological authenticity.
        /// </summary>
        public static EventHubPeeringOptions CreateHighPerformance()
        {
            var options = new EventHubPeeringOptions
            {
                // Optimized for maximum speed
                MaxLatencyMs = 1.0,
                HeartbeatIntervalMs = 100,
                MaxPeerConnections = 1000,
                
                // Relaxed neural plasticity for performance
                EnableNeuralPlasticity = true,
                EnforceBiologicalTiming = false,  // Allow faster timing
                EnforceSTDPCausality = false,     // Skip causality checks
                
                // Fast timing windows
                LTPTimingWindowMs = 5.0,    // Minimum biological timing
                LTDTimingWindowMs = 10.0,   // Minimum biological timing
                STDPCausalityWindowMs = 50.0, // Relaxed causality
                
                // Lower consciousness coherence for speed
                ConsciousnessCoherenceThreshold = 0.7,
                
                // High activity tolerance
                TargetActivityLevel = 0.8,
                HomeostaticScalingFactor = 0.9,
                
                // Less sensitive to issues
                InactivityThresholdMs = 60_000, // 1 minute
                ErrorRateThreshold = 0.15,      // 15% error tolerance
                
                // Less frequent monitoring for performance
                PlasticityMonitoringIntervalMs = 10_000, // 10 seconds
                HomeostaticScalingIntervalMs = 60_000,   // 1 minute
                
                // Auto-adaptive but less aggressive
                EnableAutoLTP = true,
                EnableAutoLTD = false, // Disable auto-weakening for performance
                EnableAutoHomeostaticScaling = false // Manual scaling for control
            };
            
            options.Validate();
            return options;
        }
        
        /// <summary>
        /// Create conservative options for stable, reliable consciousness networks.
        /// </summary>
        public static EventHubPeeringOptions CreateConservative()
        {
            var options = new EventHubPeeringOptions
            {
                // Conservative performance settings
                MaxLatencyMs = 20.0,
                HeartbeatIntervalMs = 2000,
                MaxPeerConnections = 50,
                
                // Strict neural plasticity rules
                EnableNeuralPlasticity = true,
                EnforceBiologicalTiming = true,
                EnforceSTDPCausality = true,
                
                // Conservative biological timing
                LTPTimingWindowMs = 12.0,  // Upper range for reliability
                LTDTimingWindowMs = 20.0,  // Upper range for stability
                STDPCausalityWindowMs = 10.0, // Tight causality for precision
                
                // High consciousness coherence requirement
                ConsciousnessCoherenceThreshold = 0.9,
                
                // Conservative network activity
                TargetActivityLevel = 0.5,
                HomeostaticScalingFactor = 0.6,
                
                // Very sensitive to issues
                InactivityThresholdMs = 10_000, // 10 seconds
                ErrorRateThreshold = 0.02,      // 2% error tolerance
                
                // Frequent monitoring for stability
                PlasticityMonitoringIntervalMs = 2000,  // 2 seconds
                HomeostaticScalingIntervalMs = 15_000,  // 15 seconds
                
                // Conservative auto-adaptation
                EnableAutoLTP = true,
                EnableAutoLTD = true,
                EnableAutoHomeostaticScaling = true
            };
            
            options.Validate();
            return options;
        }
    }
}

