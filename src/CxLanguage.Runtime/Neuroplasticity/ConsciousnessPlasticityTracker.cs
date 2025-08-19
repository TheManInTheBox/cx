using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CxLanguage.Runtime.Neuroplasticity
{
    /// <summary>
    /// Tracks neuroplasticity for consciousness entities in the CX Language runtime
    /// </summary>
    public class ConsciousnessPlasticityTracker
    {
        private readonly string _entityId;
        private readonly NeuroplasticityMeasurement _measurement;
        private readonly Dictionary<string, double> _connectionStrengths;
        private readonly List<PlasticityOptimization> _appliedOptimizations;
        private DateTime _lastOptimization;

        public ConsciousnessPlasticityTracker(string entityId, NeuroplasticityMeasurement measurement)
        {
            _entityId = entityId;
            _measurement = measurement;
            _connectionStrengths = new Dictionary<string, double>();
            _appliedOptimizations = new List<PlasticityOptimization>();
            _lastOptimization = DateTime.UtcNow;
        }

        /// <summary>
        /// Records a consciousness event and measures resulting neuroplasticity
        /// </summary>
        public NeuroplasticityEvent RecordConsciousnessEvent(string eventType, double stimulusStrength, TimeSpan timing)
        {
            var connectionId = $"{_entityId}_{eventType}";
            var synapticEventType = MapEventTypeToSynaptic(eventType);
            
            return _measurement.RecordSynapticEvent(connectionId, synapticEventType, stimulusStrength, timing);
        }

        /// <summary>
        /// Calculates biological authenticity of consciousness plasticity
        /// </summary>
        public double CalculateBiologicalAuthenticity()
        {
            var metrics = _measurement.MeasureSystemNeuroplasticity(_entityId);
            return metrics.BiologicalTimingAccuracy;
        }

        /// <summary>
        /// Gets current learning efficiency for this consciousness entity
        /// </summary>
        public double GetLearningEfficiency()
        {
            var metrics = _measurement.MeasureSystemNeuroplasticity(_entityId);
            return metrics.LearningEfficiency;
        }

        /// <summary>
        /// Generates adaptation recommendations based on current plasticity state
        /// </summary>
        public List<string> GetAdaptationRecommendations()
        {
            var metrics = _measurement.MeasureSystemNeuroplasticity(_entityId);
            var recommendations = new List<string>();

            if (metrics.BiologicalTimingAccuracy < 0.7)
            {
                recommendations.Add("Adjust synaptic timing to 5-25ms biological windows");
            }

            if (metrics.LearningEfficiency < 0.6)
            {
                recommendations.Add("Increase stimulus strength for better adaptation");
            }

            if (metrics.AdaptationCapacity < 0.5)
            {
                recommendations.Add("Diversify connection types for enhanced plasticity");
            }

            if (metrics.PlasticityRate < 1.0)
            {
                recommendations.Add("Increase event frequency to stimulate more plasticity");
            }

            return recommendations;
        }

        /// <summary>
        /// Optimizes plasticity parameters for target efficiency
        /// </summary>
        public async Task<Dictionary<string, object>> OptimizePlasticityParameters(double targetEfficiency, string strategy)
        {
            var currentMetrics = _measurement.MeasureSystemNeuroplasticity(_entityId);
            var optimizations = new Dictionary<string, object>();

            switch (strategy.ToLower())
            {
                case "aggressive":
                    optimizations = await ApplyAggressiveOptimizations(currentMetrics, targetEfficiency);
                    break;
                case "conservative":
                    optimizations = await ApplyConservativeOptimizations(currentMetrics, targetEfficiency);
                    break;
                case "balanced":
                default:
                    optimizations = await ApplyBalancedOptimizations(currentMetrics, targetEfficiency);
                    break;
            }

            _lastOptimization = DateTime.UtcNow;
            return optimizations;
        }

        private async Task<Dictionary<string, object>> ApplyAggressiveOptimizations(NeuroplasticityMetrics metrics, double target)
        {
            return await Task.FromResult(new Dictionary<string, object>
            {
                ["timingAdjustment"] = "Optimize to 8-12ms for maximum LTP",
                ["stimulusIncrease"] = "Increase stimulus strength by 50%",
                ["connectionPruning"] = "Remove weak connections below 0.3 strength",
                ["learningRateBoost"] = "Increase plasticity coefficient by 30%"
            });
        }

        private async Task<Dictionary<string, object>> ApplyConservativeOptimizations(NeuroplasticityMetrics metrics, double target)
        {
            return await Task.FromResult(new Dictionary<string, object>
            {
                ["timingAdjustment"] = "Gradual shift to 10-15ms optimal window",
                ["stimulusIncrease"] = "Increase stimulus strength by 10%",
                ["connectionStabilization"] = "Strengthen existing connections",
                ["learningRateAdjustment"] = "Increase plasticity coefficient by 5%"
            });
        }

        private async Task<Dictionary<string, object>> ApplyBalancedOptimizations(NeuroplasticityMetrics metrics, double target)
        {
            return await Task.FromResult(new Dictionary<string, object>
            {
                ["timingAdjustment"] = "Target 9-14ms for balanced LTP/LTD",
                ["stimulusIncrease"] = "Increase stimulus strength by 25%",
                ["connectionRebalancing"] = "Optimize connection strength distribution",
                ["learningRateOptimization"] = "Increase plasticity coefficient by 15%"
            });
        }

        private SynapticEventType MapEventTypeToSynaptic(string eventType)
        {
            return eventType.ToLower() switch
            {
                "cognitive" => SynapticEventType.PreSynapticSpike,
                "learning" => SynapticEventType.SynchronousActivation,
                "memory" => SynapticEventType.PostSynapticSpike,
                "decision" => SynapticEventType.ExcitatorySignal,
                "inhibition" => SynapticEventType.InhibitorySignal,
                _ => SynapticEventType.PreSynapticSpike
            };
        }
    }

    /// <summary>
    /// Represents a plasticity optimization that was applied
    /// </summary>
    public class PlasticityOptimization
    {
        public required string OptimizationType { get; set; }
        public required string Description { get; set; }
        public DateTime AppliedAt { get; set; }
        public double ExpectedImpact { get; set; }
        public string Strategy { get; set; } = "balanced";
    }
}

