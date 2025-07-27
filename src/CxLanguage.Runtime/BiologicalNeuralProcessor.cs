using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime;

/// <summary>
/// Biological Neural Processor for CX Language
/// Implements authentic neural pathway modeling with synaptic timing
/// Based on neuroscience research for LTP, LTD, STDP, and homeostatic regulation
/// </summary>
public class BiologicalNeuralProcessor
{
    private readonly ILogger<BiologicalNeuralProcessor> _logger;
    private readonly ICxEventBus _eventBus;
    private readonly ConcurrentDictionary<string, NeuralPathway> _pathways = new();
    private readonly ConcurrentDictionary<string, Synapse> _synapses = new();
    private readonly Timer _homeostasisTimer;
    private readonly Random _random = new();
    
    // Biological timing constants (milliseconds)
    private const int LTP_MIN_DURATION = 5;   // Long-term potentiation
    private const int LTP_MAX_DURATION = 15;
    private const int LTD_MIN_DURATION = 10;  // Long-term depression
    private const int LTD_MAX_DURATION = 25;
    private const int STDP_WINDOW = 50;       // Spike-timing dependent plasticity
    private const double HOMEOSTASIS_TARGET = 0.65; // 65% optimal activity
    
    public BiologicalNeuralProcessor(
        ILogger<BiologicalNeuralProcessor> logger,
        ICxEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
        
        // Initialize neural pathways
        InitializeNeuralPathways();
        
        // Register event handlers
        RegisterNeuralEventHandlers();
        
        // Start homeostasis regulation timer (every 1 second)
        _homeostasisTimer = new Timer(PerformHomeostasisRegulation, null, 
            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        
        _logger.LogInformation("🧬 Biological Neural Processor: Initialized with authentic synaptic timing");
    }
    
    /// <summary>
    /// Neural pathway representation
    /// </summary>
    public class NeuralPathway
    {
        public string PathwayId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // sensory, cognitive, motor, memory, association
        public double ActivityLevel { get; set; } = 0.0;
        public double Strength { get; set; } = 1.0;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public List<string> ConnectedSynapses { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public double UtilizationHistory { get; set; } = 0.0;
    }
    
    /// <summary>
    /// Synapse representation with biological properties
    /// </summary>
    public class Synapse
    {
        public string SynapseId { get; set; } = string.Empty;
        public string PresynapticPathway { get; set; } = string.Empty;
        public string PostsynapticPathway { get; set; } = string.Empty;
        public double Weight { get; set; } = 1.0;
        public double LastActivityTime { get; set; } = 0.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public SynapticPlasticityType PlasticityType { get; set; } = SynapticPlasticityType.None;
        public int ActivityCount { get; set; } = 0;
        public List<double> ActivityTimestamps { get; set; } = new();
    }
    
    /// <summary>
    /// Synaptic plasticity mechanisms
    /// </summary>
    public enum SynapticPlasticityType
    {
        None,
        LTP,        // Long-term potentiation (strengthening)
        LTD,        // Long-term depression (weakening)
        STDP,       // Spike-timing dependent plasticity
        Homeostatic // Homeostatic scaling
    }
    
    /// <summary>
    /// Initialize the five major neural pathways
    /// </summary>
    private void InitializeNeuralPathways()
    {
        var pathwayTypes = new[]
        {
            "sensory",      // Environmental input processing
            "cognitive",    // Complex reasoning and analysis
            "motor",        // Action and response coordination
            "memory",       // Experience storage and retrieval
            "association"   // Cross-pathway integration
        };
        
        foreach (var type in pathwayTypes)
        {
            var pathway = new NeuralPathway
            {
                PathwayId = $"pathway_{type}_{Guid.NewGuid():N}",
                Type = type,
                ActivityLevel = _random.NextDouble() * 0.3, // Start with low activity
                Strength = 1.0,
                LastActivity = DateTime.UtcNow,
                IsActive = true
            };
            
            _pathways[pathway.PathwayId] = pathway;
            _logger.LogDebug("🧠 Neural Pathway Initialized: {Type} - {PathwayId}", type, pathway.PathwayId);
        }
        
        // Create inter-pathway synapses
        CreateInterPathwaySynapses();
    }
    
    /// <summary>
    /// Create synaptic connections between pathways
    /// </summary>
    private void CreateInterPathwaySynapses()
    {
        var pathwayList = _pathways.Values.ToList();
        
        for (int i = 0; i < pathwayList.Count; i++)
        {
            for (int j = i + 1; j < pathwayList.Count; j++)
            {
                var presynaptic = pathwayList[i];
                var postsynaptic = pathwayList[j];
                
                // Create bidirectional synapses
                CreateSynapse(presynaptic.PathwayId, postsynaptic.PathwayId);
                CreateSynapse(postsynaptic.PathwayId, presynaptic.PathwayId);
            }
        }
        
        _logger.LogInformation("🔗 Neural Synapses Created: {Count} synaptic connections", _synapses.Count);
    }
    
    /// <summary>
    /// Create a synapse between two pathways
    /// </summary>
    private void CreateSynapse(string presynapticId, string postsynapticId)
    {
        var synapseId = $"synapse_{presynapticId}_{postsynapticId}";
        
        var synapse = new Synapse
        {
            SynapseId = synapseId,
            PresynapticPathway = presynapticId,
            PostsynapticPathway = postsynapticId,
            Weight = _random.NextDouble() * 0.5 + 0.5, // Random weight 0.5-1.0
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        
        _synapses[synapseId] = synapse;
        
        // Add to pathway connections
        if (_pathways.TryGetValue(presynapticId, out var prePathway))
        {
            prePathway.ConnectedSynapses.Add(synapseId);
        }
    }
    
    /// <summary>
    /// Register event handlers for neural processing
    /// </summary>
    private void RegisterNeuralEventHandlers()
    {
        _eventBus.Subscribe("neural.signal", async payload => await OnNeuralSignal(ConvertPayloadToCxEvent(payload)));
        _eventBus.Subscribe("pathway.activate", async payload => await OnPathwayActivation(ConvertPayloadToCxEvent(payload)));
        _eventBus.Subscribe("synaptic.event", async payload => await OnSynapticEvent(ConvertPayloadToCxEvent(payload)));
        _eventBus.Subscribe("consciousness.activity.detected", async payload => await OnConsciousnessActivity(ConvertPayloadToCxEvent(payload)));
        
        _logger.LogInformation("🧬 Neural Event Handlers: Registered for biological processing");
    }
    
    /// <summary>
    /// Process neural signals with biological timing
    /// </summary>
    private async Task OnNeuralSignal(CxEvent? cxEvent)
    {
        if (cxEvent == null) return;
        
        try
        {
            var pathwayType = ExtractStringProperty(cxEvent.payload, "pathway_type", "cognitive");
            var signal = ExtractProperty(cxEvent.payload, "signal");
            var timestamp = DateTime.UtcNow;
            
            // Find pathway by type
            var pathway = _pathways.Values.FirstOrDefault(p => p.Type == pathwayType);
            if (pathway == null)
            {
                _logger.LogWarning("🚫 Neural Signal: Pathway type not found: {Type}", pathwayType);
                return;
            }
            
            // Activate pathway with biological timing
            await ActivatePathway(pathway, signal, timestamp);
            
            _logger.LogDebug("⚡ Neural Signal Processed: {PathwayType} - Activity: {Activity:F3}", 
                pathwayType, pathway.ActivityLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing neural signal");
        }
    }
    
    /// <summary>
    /// Process pathway activation events
    /// </summary>
    private async Task OnPathwayActivation(CxEvent? cxEvent)
    {
        if (cxEvent == null) return;
        
        try
        {
            var pathwayId = ExtractStringProperty(cxEvent.payload, "pathway_id");
            var activationType = ExtractStringProperty(cxEvent.payload, "activation_type", "normal");
            
            if (_pathways.TryGetValue(pathwayId, out var pathway))
            {
                await ActivatePathway(pathway, cxEvent.payload, DateTime.UtcNow);
                _logger.LogDebug("🧠 Pathway Activated: {PathwayId} - Type: {Type}", pathwayId, activationType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pathway activation");
        }
    }
    
    /// <summary>
    /// Process synaptic events with plasticity
    /// </summary>
    private async Task OnSynapticEvent(CxEvent? cxEvent)
    {
        if (cxEvent == null) return;
        
        try
        {
            var eventType = ExtractStringProperty(cxEvent.payload, "type");
            var synapseId = ExtractStringProperty(cxEvent.payload, "synapse_id");
            var timestamp = DateTime.UtcNow;
            
            if (_synapses.TryGetValue(synapseId, out var synapse))
            {
                await ProcessSynapticTransmission(synapse, timestamp);
                _logger.LogDebug("🔗 Synaptic Event: {SynapseId} - Type: {Type}", synapseId, eventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing synaptic event");
        }
    }
    
    /// <summary>
    /// Process consciousness activity for neural correlation
    /// </summary>
    private async Task OnConsciousnessActivity(CxEvent? cxEvent)
    {
        if (cxEvent == null) return;
        
        try
        {
            var activityType = ExtractStringProperty(cxEvent.payload, "type");
            var complexity = ExtractStringProperty(cxEvent.payload, "complexity");
            
            // Consciousness activity triggers neural pathway activation
            await TriggerConsciousnessNeuralActivity(activityType, complexity);
            
            _logger.LogDebug("🎭 Consciousness Neural Activity: {Type} - {Complexity}", activityType, complexity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing consciousness activity");
        }
    }
    
    /// <summary>
    /// Activate neural pathway with biological timing
    /// </summary>
    private async Task ActivatePathway(NeuralPathway pathway, object? signal, DateTime timestamp)
    {
        // Update pathway activity
        pathway.LastActivity = timestamp;
        pathway.ActivityLevel = Math.Min(1.0, pathway.ActivityLevel + 0.1);
        pathway.UtilizationHistory = (pathway.UtilizationHistory * 0.9) + (pathway.ActivityLevel * 0.1);
        
        // Process connected synapses
        foreach (var synapseId in pathway.ConnectedSynapses)
        {
            if (_synapses.TryGetValue(synapseId, out var synapse))
            {
                await ProcessSynapticTransmission(synapse, timestamp);
            }
        }
        
        // Emit neural activity event
        await _eventBus.EmitAsync("neural.activity.detected", new
        {
            pathway_id = pathway.PathwayId,
            pathway_type = pathway.Type,
            activity_level = pathway.ActivityLevel,
            strength = pathway.Strength,
            timestamp = timestamp,
            signal = signal
        });
    }
    
    /// <summary>
    /// Process synaptic transmission with biological timing
    /// </summary>
    private async Task ProcessSynapticTransmission(Synapse synapse, DateTime timestamp)
    {
        var currentTime = timestamp.Ticks / TimeSpan.TicksPerMillisecond;
        synapse.LastActivityTime = currentTime;
        synapse.ActivityCount++;
        synapse.ActivityTimestamps.Add(currentTime);
        
        // Keep only recent activity (last 1000ms)
        synapse.ActivityTimestamps.RemoveAll(t => currentTime - t > 1000);
        
        // Determine plasticity type based on activity pattern
        var plasticityType = DeterminePlasticityType(synapse, currentTime);
        
        // Apply synaptic plasticity with biological timing
        await ApplySynapticPlasticity(synapse, plasticityType, timestamp);
        
        // Emit synaptic activity
        await _eventBus.EmitAsync("synaptic.activity.detected", new
        {
            synapse_id = synapse.SynapseId,
            plasticity_type = plasticityType.ToString(),
            weight = synapse.Weight,
            activity_count = synapse.ActivityCount,
            timestamp = timestamp
        });
    }
    
    /// <summary>
    /// Determine synaptic plasticity type based on timing
    /// </summary>
    private SynapticPlasticityType DeterminePlasticityType(Synapse synapse, double currentTime)
    {
        var recentActivity = synapse.ActivityTimestamps.Count(t => currentTime - t < STDP_WINDOW);
        
        if (recentActivity >= 3)
        {
            // High frequency activity -> LTP (strengthening)
            return SynapticPlasticityType.LTP;
        }
        else if (recentActivity == 2)
        {
            // Moderate activity -> STDP (timing-dependent)
            return SynapticPlasticityType.STDP;
        }
        else if (synapse.ActivityTimestamps.Count == 0 && currentTime - synapse.LastActivityTime > 500)
        {
            // Low activity -> LTD (weakening)
            return SynapticPlasticityType.LTD;
        }
        
        return SynapticPlasticityType.None;
    }
    
    /// <summary>
    /// Apply synaptic plasticity with biological timing
    /// </summary>
    private async Task ApplySynapticPlasticity(Synapse synapse, SynapticPlasticityType plasticityType, DateTime timestamp)
    {
        double weightChange = 0.0;
        int duration = 0;
        
        switch (plasticityType)
        {
            case SynapticPlasticityType.LTP:
                // Strengthen synapse (5-15ms duration)
                weightChange = 0.05 + (_random.NextDouble() * 0.05); // +5-10%
                duration = _random.Next(LTP_MIN_DURATION, LTP_MAX_DURATION + 1);
                synapse.Weight = Math.Min(2.0, synapse.Weight + weightChange);
                break;
                
            case SynapticPlasticityType.LTD:
                // Weaken synapse (10-25ms duration)
                weightChange = -0.03 - (_random.NextDouble() * 0.03); // -3-6%
                duration = _random.Next(LTD_MIN_DURATION, LTD_MAX_DURATION + 1);
                synapse.Weight = Math.Max(0.1, synapse.Weight + weightChange);
                break;
                
            case SynapticPlasticityType.STDP:
                // Spike-timing dependent plasticity
                duration = _random.Next(5, 20);
                var timingFactor = CalculateSTDPTiming(synapse);
                weightChange = timingFactor * 0.02; // ±2% based on timing
                synapse.Weight = Math.Max(0.1, Math.Min(2.0, synapse.Weight + weightChange));
                break;
                
            default:
                return; // No plasticity
        }
        
        if (plasticityType != SynapticPlasticityType.None)
        {
            synapse.PlasticityType = plasticityType;
            synapse.LastModified = timestamp;
            
            // Simulate biological timing delay
            if (duration > 0)
            {
                await Task.Delay(duration);
            }
            
            // Emit plasticity event
            await _eventBus.EmitAsync("neural.plasticity.applied", new
            {
                synapse_id = synapse.SynapseId,
                plasticity_type = plasticityType.ToString(),
                weight_change = weightChange,
                new_weight = synapse.Weight,
                duration_ms = duration,
                timestamp = timestamp
            });
            
            _logger.LogDebug("🧬 Synaptic Plasticity: {Type} - Weight: {Weight:F3} (Δ{Change:F3})", 
                plasticityType, synapse.Weight, weightChange);
        }
    }
    
    /// <summary>
    /// Calculate STDP timing factor
    /// </summary>
    private double CalculateSTDPTiming(Synapse synapse)
    {
        if (synapse.ActivityTimestamps.Count < 2)
            return 0.0;
        
        // Get last two spikes
        var times = synapse.ActivityTimestamps.TakeLast(2).ToArray();
        var deltaTime = times[1] - times[0];
        
        // STDP function: positive if post-before-pre, negative if pre-before-post
        if (deltaTime > 0 && deltaTime < STDP_WINDOW)
        {
            return Math.Exp(-deltaTime / 20.0); // Strengthening
        }
        else if (deltaTime < 0 && Math.Abs(deltaTime) < STDP_WINDOW)
        {
            return -Math.Exp(Math.Abs(deltaTime) / 20.0); // Weakening
        }
        
        return 0.0;
    }
    
    /// <summary>
    /// Trigger neural activity from consciousness events
    /// </summary>
    private async Task TriggerConsciousnessNeuralActivity(string activityType, string complexity)
    {
        // Map consciousness activities to neural pathways
        var pathwayType = activityType switch
        {
            "self_reflection" => "cognitive",
            "sensory_processing" => "sensory",
            "motor_coordination" => "motor",
            "memory_formation" => "memory",
            "cross_modal_integration" => "association",
            _ => "cognitive"
        };
        
        var pathway = _pathways.Values.FirstOrDefault(p => p.Type == pathwayType);
        if (pathway != null)
        {
            await ActivatePathway(pathway, new { activity_type = activityType, complexity }, DateTime.UtcNow);
        }
    }
    
    /// <summary>
    /// Perform homeostatic regulation (called by timer)
    /// </summary>
    private async void PerformHomeostasisRegulation(object? state)
    {
        try
        {
            foreach (var pathway in _pathways.Values)
            {
                // Calculate current network activity
                var networkActivity = _pathways.Values.Average(p => p.ActivityLevel);
                
                if (networkActivity > HOMEOSTASIS_TARGET + 0.1)
                {
                    // Network too active - scale down
                    pathway.ActivityLevel *= 0.95;
                    ApplyHomeostasisToConnectedSynapses(pathway, -0.01);
                }
                else if (networkActivity < HOMEOSTASIS_TARGET - 0.1)
                {
                    // Network too quiet - scale up
                    pathway.ActivityLevel *= 1.02;
                    ApplyHomeostasisToConnectedSynapses(pathway, 0.01);
                }
                
                // Natural decay
                pathway.ActivityLevel *= 0.98;
            }
            
            // Emit homeostasis event
            var avgActivity = _pathways.Values.Average(p => p.ActivityLevel);
            await _eventBus.EmitAsync("neural.homeostasis.regulated", new
            {
                network_activity = avgActivity,
                target_activity = HOMEOSTASIS_TARGET,
                pathway_count = _pathways.Count,
                synapse_count = _synapses.Count,
                timestamp = DateTime.UtcNow
            });
            
            _logger.LogDebug("🌐 Homeostasis: Network activity: {Activity:F3} (target: {Target:F2})", 
                avgActivity, HOMEOSTASIS_TARGET);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during homeostasis regulation");
        }
    }
    
    /// <summary>
    /// Apply homeostatic scaling to synapses
    /// </summary>
    private void ApplyHomeostasisToConnectedSynapses(NeuralPathway pathway, double scalingFactor)
    {
        foreach (var synapseId in pathway.ConnectedSynapses)
        {
            if (_synapses.TryGetValue(synapseId, out var synapse))
            {
                synapse.Weight = Math.Max(0.1, Math.Min(2.0, synapse.Weight + scalingFactor));
                synapse.PlasticityType = SynapticPlasticityType.Homeostatic;
                synapse.LastModified = DateTime.UtcNow;
            }
        }
    }
    
    /// <summary>
    /// Convert payload to legacy CxEvent type
    /// </summary>
    private CxEvent ConvertPayloadToCxEvent(CxEventPayload payload)
    {
        var cxEvent = new CxEvent
        {
            name = payload.EventName,
            timestamp = payload.Timestamp
        };

        if (payload.Data is Dictionary<string, object> data)
        {
            cxEvent.payload = data;
        }
        else if (payload.Data != null)
        {
            // Fallback for non-dictionary payloads
            cxEvent.payload = new Dictionary<string, object> { { "data", payload.Data } };
        }
        else
        {
            cxEvent.payload = new Dictionary<string, object>();
        }

        return cxEvent;
    }
    
    /// <summary>
    /// Extract string property from payload data
    /// </summary>
    private string ExtractStringProperty(object? data, string propertyName, string defaultValue = "")
    {
        if (data == null) return defaultValue;
        
        try
        {
            if (data is Dictionary<string, object> dict)
            {
                return dict.TryGetValue(propertyName, out var value) ? value?.ToString() ?? defaultValue : defaultValue;
            }
            
            var property = data.GetType().GetProperty(propertyName, 
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            return property?.GetValue(data)?.ToString() ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
    
    /// <summary>
    /// Extract property from payload data
    /// </summary>
    private object? ExtractProperty(object? data, string propertyName)
    {
        if (data == null) return null;
        
        try
        {
            if (data is Dictionary<string, object> dict)
            {
                return dict.TryGetValue(propertyName, out var value) ? value : null;
            }
            
            var property = data.GetType().GetProperty(propertyName, 
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            return property?.GetValue(data);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Get neural pathway statistics
    /// </summary>
    public object GetNeuralStatistics()
    {
        return new
        {
            pathway_count = _pathways.Count,
            synapse_count = _synapses.Count,
            average_activity = _pathways.Values.Average(p => p.ActivityLevel),
            average_weight = _synapses.Values.Average(s => s.Weight),
            homeostasis_target = HOMEOSTASIS_TARGET,
            timestamp = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        _homeostasisTimer?.Dispose();
        _logger.LogInformation("🧬 Biological Neural Processor: Disposed");
    }
}
