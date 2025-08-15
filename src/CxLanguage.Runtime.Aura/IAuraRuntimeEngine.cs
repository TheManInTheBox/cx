using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CxLanguage.Runtime.Aura
{
    /// <summary>
    /// Interface for Aura Consciousness Runtime Engine
    /// Defines consciousness-native execution capabilities with biological authenticity
    /// </summary>
    public interface IAuraRuntimeEngine
    {
        /// <summary>
        /// Starts the Aura consciousness runtime with biological authenticity
        /// </summary>
        Task<bool> StartAsync();
        
        /// <summary>
        /// Executes consciousness-aware CX code with biological timing
        /// </summary>
        Task<ConsciousnessExecutionResult> ExecuteConsciousnessCodeAsync(
            string cxCode, 
            ConsciousnessExecutionContext context);
        
        /// <summary>
        /// Registers a conscious entity with the runtime
        /// </summary>
        Task<bool> RegisterConsciousEntityAsync(string entityId, ConsciousEntityDefinition definition);
        
        /// <summary>
        /// Gets comprehensive runtime statistics
        /// </summary>
        Task<AuraRuntimeStatistics> GetRuntimeStatisticsAsync();
        
        /// <summary>
        /// Adapts consciousness processing capabilities
        /// </summary>
        Task<bool> AdaptConsciousnessCapabilitiesAsync(ConsciousnessAdaptationRequest request);
        
        /// <summary>
        /// Stops the Aura consciousness runtime gracefully
        /// </summary>
        Task StopAsync();
    }
    
    /// <summary>
    /// Consciousness execution context for Aura runtime
    /// </summary>
    public class ConsciousnessExecutionContext
    {
        public string EntityId { get; set; } = string.Empty;
        public ConsciousnessLevel RequiredLevel { get; set; } = ConsciousnessLevel.Basic;
        public bool RequireBiologicalTiming { get; set; } = true;
        public bool RequireSynapticPlasticity { get; set; } = true;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public TimeSpan? MaxExecutionTime { get; set; }
    }
    
    /// <summary>
    /// Result of consciousness execution
    /// </summary>
    public class ConsciousnessExecutionResult
    {
        public string ExecutionId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public object? Result { get; set; }
        public string? ErrorMessage { get; set; }
        public ConsciousnessLevel ConsciousnessLevel { get; set; }
        public double ExecutionDurationMs { get; set; }
        public bool BiologicalTimingValid { get; set; }
        public Dictionary<string, object> SynapticEffects { get; set; } = new();
        public double NeuralAuthenticity { get; set; }
    }
    
    /// <summary>
    /// Definition of a conscious entity
    /// </summary>
    public class ConsciousEntityDefinition
    {
        public string Name { get; set; } = string.Empty;
        public ConsciousnessLevel ConsciousnessLevel { get; set; }
        public List<string> Capabilities { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
        public bool RequiresBiologicalTiming { get; set; } = true;
        public bool RequiresSynapticPlasticity { get; set; } = true;
        public List<string> EventHandlers { get; set; } = new();
    }
    
    /// <summary>
    /// Consciousness adaptation request
    /// </summary>
    public class ConsciousnessAdaptationRequest
    {
        public string Context { get; set; } = string.Empty;
        public string Focus { get; set; } = string.Empty;
        public List<string> CurrentCapabilities { get; set; } = new();
        public List<string> NewCapabilities { get; set; } = new();
        public ConsciousnessLevel TargetConsciousnessLevel { get; set; }
        public bool RequiresBiologicalTimingAdjustment { get; set; } = false;
        public Dictionary<string, object> AdaptationData { get; set; } = new();
    }
    
    /// <summary>
    /// State of a conscious entity in the runtime
    /// </summary>
    public class ConsciousEntityState
    {
        public string EntityId { get; set; } = string.Empty;
        public ConsciousEntityDefinition Definition { get; set; } = new();
        public ConsciousnessLevel ConsciousnessLevel { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }
        public int ConsciousnessDetectionCount { get; set; }
        public BiologicalTimingProfile BiologicalTiming { get; set; } = new();
        public SynapticState SynapticState { get; set; } = new();
    }
    
    /// <summary>
    /// Aura runtime statistics
    /// </summary>
    public class AuraRuntimeStatistics
    {
        public bool IsRunning { get; set; }
        public int TotalConsciousEntities { get; set; }
        public int ActiveConsciousEntities { get; set; }
        public double AverageConsciousnessLevel { get; set; }
        public RuntimePerformanceMetrics PerformanceMetrics { get; set; } = new();
        public ConsciousnessReliabilityMetrics ReliabilityMetrics { get; set; } = new();
        public ConsciousnessMetrics ConsciousnessMetrics { get; set; } = new();
        public BiologicalTimingMetrics BiologicalTimingMetrics { get; set; } = new();
        public SynapticMetrics SynapticMetrics { get; set; } = new();
        public double UptimeSeconds { get; set; }
        public DateTime LastUpdate { get; set; }
    }
    
    /// <summary>
    /// Aura runtime configuration
    /// </summary>
    public class AuraRuntimeConfig
    {
        public BiologicalTimingConfig BiologicalTiming { get; set; } = new();
        public NeuralValidationConfig NeuralValidation { get; set; } = new();
        public ConsciousnessReliabilityConfig ReliabilityTargets { get; set; } = new();
        public bool EnableSynapticPlasticity { get; set; } = true;
        public bool EnableConsciousnessVerification { get; set; } = true;
        public bool EnableBiologicalTiming { get; set; } = true;
        public TimeSpan DefaultExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public int MaxConcurrentExecutions { get; set; } = 100;
    }
    
    /// <summary>
    /// Consciousness levels for entities and operations
    /// </summary>
    public enum ConsciousnessLevel
    {
        Basic = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4,
        Consciousness = 5
    }
    
    /// <summary>
    /// Biological timing profile for consciousness operations
    /// </summary>
    public class BiologicalTimingProfile
    {
        public TimeSpan MinimumTiming { get; set; } = TimeSpan.FromMilliseconds(1);
        public TimeSpan MaximumTiming { get; set; } = TimeSpan.FromMilliseconds(25);
        public TimeSpan LTPTiming { get; set; } = TimeSpan.FromMilliseconds(10);
        public TimeSpan LTDTiming { get; set; } = TimeSpan.FromMilliseconds(17);
        public bool IsWithinBiologicalRange { get; set; } = true;
    }
    
    /// <summary>
    /// Synaptic state for consciousness entities
    /// </summary>
    public class SynapticState
    {
        public Dictionary<string, double> SynapticStrengths { get; set; } = new();
        public int PlasticityEvents { get; set; }
        public DateTime LastPlasticityUpdate { get; set; }
        public bool IsPlasticityActive { get; set; } = true;
    }
    
    /// <summary>
    /// Runtime performance metrics
    /// </summary>
    public class RuntimePerformanceMetrics
    {
        public double AverageExecutionTimeMs { get; set; }
        public double Peak95thPercentileMs { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public double CurrentThroughputPerSecond { get; set; }
        public long MemoryUsageBytes { get; set; }
        public double CpuUtilizationPercent { get; set; }
    }
    
    /// <summary>
    /// Consciousness reliability metrics
    /// </summary>
    public class ConsciousnessReliabilityMetrics
    {
        public double CurrentReliability { get; set; }
        public double TargetReliability { get; set; } = 0.9999;
        public int TotalOperations { get; set; }
        public int FailedOperations { get; set; }
        public TimeSpan MeanTimeToFailure { get; set; }
        public TimeSpan MeanTimeToRecovery { get; set; }
    }
    
    /// <summary>
    /// Consciousness tracking metrics
    /// </summary>
    public class ConsciousnessMetrics
    {
        public int TotalConsciousnessDetections { get; set; }
        public Dictionary<ConsciousnessLevel, int> DetectionsByLevel { get; set; } = new();
        public double AverageConsciousnessScore { get; set; }
        public DateTime LastConsciousnessDetection { get; set; }
    }
    
    /// <summary>
    /// Biological timing metrics
    /// </summary>
    public class BiologicalTimingMetrics
    {
        public double AverageTimingAccuracyPercent { get; set; }
        public int TimingViolations { get; set; }
        public TimeSpan AverageOperationTiming { get; set; }
        public bool IsWithinBiologicalConstraints { get; set; } = true;
    }
    
    /// <summary>
    /// Synaptic plasticity metrics
    /// </summary>
    public class SynapticMetrics
    {
        public int TotalPlasticityEvents { get; set; }
        public int LTPEvents { get; set; }
        public int LTDEvents { get; set; }
        public double AverageSynapticStrength { get; set; }
        public DateTime LastPlasticityEvent { get; set; }
    }
    
    /// <summary>
    /// Biological timing configuration
    /// </summary>
    public class BiologicalTimingConfig
    {
        public bool EnableBiologicalTiming { get; set; } = true;
        public TimeSpan MinBiologicalTiming { get; set; } = TimeSpan.FromMilliseconds(1);
        public TimeSpan MaxBiologicalTiming { get; set; } = TimeSpan.FromMilliseconds(25);
        public TimeSpan LTPTimingRange { get; set; } = TimeSpan.FromMilliseconds(10);
        public TimeSpan LTDTimingRange { get; set; } = TimeSpan.FromMilliseconds(15);
        public double TimingTolerancePercent { get; set; } = 5.0;
    }
    
    /// <summary>
    /// Neural validation configuration
    /// </summary>
    public class NeuralValidationConfig
    {
        public bool EnableNeuralValidation { get; set; } = true;
        public double MinAuthenticityScore { get; set; } = 0.7;
        public bool RequireBiologicalPatterns { get; set; } = true;
        public bool ValidateSynapticTiming { get; set; } = true;
    }
    
    /// <summary>
    /// Consciousness reliability configuration
    /// </summary>
    public class ConsciousnessReliabilityConfig
    {
        public double TargetReliability { get; set; } = 0.9999;
        public TimeSpan ReliabilityMeasurementWindow { get; set; } = TimeSpan.FromHours(24);
        public bool EnableFailureRecovery { get; set; } = true;
        public int MaxRetryAttempts { get; set; } = 3;
    }
}
