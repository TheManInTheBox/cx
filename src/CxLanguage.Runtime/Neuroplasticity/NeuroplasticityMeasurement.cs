using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Runtime.Neuroplasticity
{
    /// <summary>
    /// Comprehensive neuroplasticity measurement system for consciousness computing.
    /// Tracks synaptic strength changes, learning patterns, and adaptation metrics.
    /// </summary>
    public class NeuroplasticityMeasurement
    {
        private readonly ILogger<NeuroplasticityMeasurement> _logger;
        private readonly Dictionary<string, SynapticConnection> _synapticConnections;
        private readonly Dictionary<string, LearningMetrics> _learningHistory;
        private readonly TimeSpan _measurementWindow = TimeSpan.FromMinutes(5);
        
        public NeuroplasticityMeasurement(ILogger<NeuroplasticityMeasurement> logger)
        {
            _logger = logger;
            _synapticConnections = new Dictionary<string, SynapticConnection>();
            _learningHistory = new Dictionary<string, LearningMetrics>();
        }

        /// <summary>
        /// Records a synaptic event and measures plasticity changes
        /// </summary>
        public NeuroplasticityEvent RecordSynapticEvent(string connectionId, SynapticEventType eventType, 
            double stimulusStrength, TimeSpan timing)
        {
            var connection = GetOrCreateConnection(connectionId);
            var previousStrength = connection.Strength;
            
            // Apply biological plasticity rules
            var strengthChange = CalculatePlasticityChange(connection, eventType, stimulusStrength, timing);
            connection.ApplyStrengthChange(strengthChange, timing);
            
            var plasticityEvent = new NeuroplasticityEvent
            {
                ConnectionId = connectionId,
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                PreviousStrength = previousStrength,
                NewStrength = connection.Strength,
                StrengthChange = strengthChange,
                PlasticityType = DeterminePlasticityType(strengthChange, timing),
                BiologicalTiming = timing,
                StimulusStrength = stimulusStrength
            };

            UpdateLearningMetrics(connectionId, plasticityEvent);
            
            _logger.LogDebug("Neuroplasticity event recorded: {ConnectionId}, Change: {Change}ms, Type: {Type}",
                connectionId, strengthChange, plasticityEvent.PlasticityType);
                
            return plasticityEvent;
        }

        /// <summary>
        /// Calculates plasticity change based on biological rules
        /// LTP (Long-Term Potentiation): 5-15ms timing window, strengthens connections
        /// LTD (Long-Term Depression): 10-25ms timing window, weakens connections
        /// STDP (Spike-Timing Dependent Plasticity): Causal timing relationships
        /// </summary>
        private double CalculatePlasticityChange(SynapticConnection connection, SynapticEventType eventType, 
            double stimulusStrength, TimeSpan timing)
        {
            var timingMs = timing.TotalMilliseconds;
            var baseChange = stimulusStrength * 0.1; // Base plasticity coefficient
            
            // Biological timing windows
            if (timingMs >= 5 && timingMs <= 15) // LTP window
            {
                return baseChange * Math.Exp(-Math.Abs(timingMs - 10) / 5.0); // Peak at 10ms
            }
            else if (timingMs >= 10 && timingMs <= 25) // LTD window
            {
                return -baseChange * 0.5 * Math.Exp(-Math.Abs(timingMs - 17.5) / 7.5); // Peak at 17.5ms
            }
            else if (timingMs < 5) // Pre-synaptic spike timing
            {
                return baseChange * 0.8 * Math.Exp(-timingMs / 2.0); // STDP causality
            }
            else if (timingMs > 25) // Post-synaptic delayed response
            {
                return -baseChange * 0.3 * Math.Exp(-(timingMs - 25) / 10.0); // Delayed depression
            }
            
            return 0.0; // No plasticity outside biological windows
        }

        /// <summary>
        /// Determines the type of plasticity based on strength change and timing
        /// </summary>
        private PlasticityType DeterminePlasticityType(double strengthChange, TimeSpan timing)
        {
            var timingMs = timing.TotalMilliseconds;
            
            if (strengthChange > 0)
            {
                if (timingMs >= 5 && timingMs <= 15)
                    return PlasticityType.LTP; // Long-Term Potentiation
                else if (timingMs < 5)
                    return PlasticityType.STPCausal; // Spike-Timing Dependent Plasticity (Causal)
                else
                    return PlasticityType.ShortTermPotentiation;
            }
            else if (strengthChange < 0)
            {
                if (timingMs >= 10 && timingMs <= 25)
                    return PlasticityType.LTD; // Long-Term Depression
                else
                    return PlasticityType.STPAntiCausal; // Spike-Timing Dependent Plasticity (Anti-Causal)
            }
            
            return PlasticityType.None;
        }

        /// <summary>
        /// Measures overall neuroplasticity metrics for a consciousness system
        /// </summary>
        public NeuroplasticityMetrics MeasureSystemNeuroplasticity(string systemId)
        {
            var systemConnections = _synapticConnections.Values
                .Where(c => c.SystemId == systemId)
                .ToList();
                
            var recentEvents = systemConnections
                .SelectMany(c => c.RecentEvents)
                .Where(e => e.Timestamp > DateTime.UtcNow - _measurementWindow)
                .ToList();
                
            var metrics = new NeuroplasticityMetrics
            {
                SystemId = systemId,
                MeasurementTimestamp = DateTime.UtcNow,
                TotalConnections = systemConnections.Count,
                ActiveConnections = systemConnections.Count(c => c.IsActive),
                AverageSynapticStrength = systemConnections.Average(c => c.Strength),
                PlasticityRate = CalculatePlasticityRate(recentEvents),
                LTPEvents = recentEvents.Count(e => e.PlasticityType == PlasticityType.LTP),
                LTDEvents = recentEvents.Count(e => e.PlasticityType == PlasticityType.LTD),
                STDPCausalEvents = recentEvents.Count(e => e.PlasticityType == PlasticityType.STPCausal),
                STDPAntiCausalEvents = recentEvents.Count(e => e.PlasticityType == PlasticityType.STPAntiCausal),
                BiologicalTimingAccuracy = CalculateBiologicalTimingAccuracy(recentEvents),
                LearningEfficiency = CalculateLearningEfficiency(systemId),
                AdaptationCapacity = CalculateAdaptationCapacity(systemConnections)
            };
            
            return metrics;
        }

        /// <summary>
        /// Calculates the rate of plastic changes over time
        /// </summary>
        private double CalculatePlasticityRate(List<NeuroplasticityEvent> events)
        {
            if (!events.Any()) return 0.0;
            
            var totalChange = events.Sum(e => Math.Abs(e.StrengthChange));
            var timeSpan = _measurementWindow.TotalMinutes;
            
            return totalChange / timeSpan; // Changes per minute
        }

        /// <summary>
        /// Measures how closely the system follows biological timing patterns
        /// </summary>
        private double CalculateBiologicalTimingAccuracy(List<NeuroplasticityEvent> events)
        {
            if (!events.Any()) return 0.0;
            
            var biologicalEvents = events.Count(e => 
                e.PlasticityType == PlasticityType.LTP || 
                e.PlasticityType == PlasticityType.LTD ||
                e.PlasticityType == PlasticityType.STPCausal);
                
            return (double)biologicalEvents / events.Count;
        }

        /// <summary>
        /// Calculates learning efficiency based on successful adaptations
        /// </summary>
        private double CalculateLearningEfficiency(string systemId)
        {
            if (!_learningHistory.ContainsKey(systemId))
                return 0.0;
                
            var metrics = _learningHistory[systemId];
            if (metrics.TotalLearningAttempts == 0) return 0.0;
            
            return (double)metrics.SuccessfulAdaptations / metrics.TotalLearningAttempts;
        }

        /// <summary>
        /// Measures the system's capacity for future adaptation
        /// </summary>
        private double CalculateAdaptationCapacity(List<SynapticConnection> connections)
        {
            if (!connections.Any()) return 0.0;
            
            // Capacity based on connection diversity and strength distribution
            var strengthVariance = CalculateVariance(connections.Select(c => c.Strength));
            var connectionTypes = connections.Select(c => c.ConnectionType).Distinct().Count();
            
            // Higher variance and more connection types = higher adaptation capacity
            return (strengthVariance * connectionTypes) / connections.Count;
        }

        private double CalculateVariance(IEnumerable<double> values)
        {
            var enumerable = values.ToList();
            if (!enumerable.Any()) return 0.0;
            
            var mean = enumerable.Average();
            return enumerable.Select(x => Math.Pow(x - mean, 2)).Average();
        }

        private SynapticConnection GetOrCreateConnection(string connectionId)
        {
            if (!_synapticConnections.ContainsKey(connectionId))
            {
                _synapticConnections[connectionId] = new SynapticConnection(connectionId);
            }
            return _synapticConnections[connectionId];
        }

        private void UpdateLearningMetrics(string connectionId, NeuroplasticityEvent plasticityEvent)
        {
            var systemId = _synapticConnections[connectionId].SystemId;
            
            if (!_learningHistory.ContainsKey(systemId))
            {
                _learningHistory[systemId] = new LearningMetrics { SystemId = systemId };
            }
            
            var metrics = _learningHistory[systemId];
            metrics.TotalLearningAttempts++;
            
            // Consider significant positive changes as successful adaptations
            if (plasticityEvent.StrengthChange > 0.1)
            {
                metrics.SuccessfulAdaptations++;
            }
            
            metrics.LastUpdate = DateTime.UtcNow;
        }

        /// <summary>
        /// Generates a comprehensive neuroplasticity report
        /// </summary>
        public NeuroplasticityReport GenerateReport(string systemId, TimeSpan reportPeriod)
        {
            var metrics = MeasureSystemNeuroplasticity(systemId);
            var historicalData = GetHistoricalMetrics(systemId, reportPeriod);
            
            return new NeuroplasticityReport
            {
                SystemId = systemId,
                ReportPeriod = reportPeriod,
                CurrentMetrics = metrics,
                HistoricalTrends = historicalData,
                BiologicalAuthenticity = CalculateBiologicalAuthenticity(metrics),
                RecommendedOptimizations = GenerateOptimizationRecommendations(metrics),
                GeneratedAt = DateTime.UtcNow
            };
        }

        private List<NeuroplasticityMetrics> GetHistoricalMetrics(string systemId, TimeSpan period)
        {
            // Implementation would retrieve historical metrics from storage
            // For now, return empty list as placeholder
            return new List<NeuroplasticityMetrics>();
        }

        private double CalculateBiologicalAuthenticity(NeuroplasticityMetrics metrics)
        {
            // Score based on how closely the system follows biological patterns
            var timingScore = metrics.BiologicalTimingAccuracy;
            var plasticityBalance = Math.Min(metrics.LTPEvents, metrics.LTDEvents) / 
                                   (double)Math.Max(metrics.LTPEvents + metrics.LTDEvents, 1);
            
            return (timingScore + plasticityBalance) / 2.0;
        }

        private List<string> GenerateOptimizationRecommendations(NeuroplasticityMetrics metrics)
        {
            var recommendations = new List<string>();
            
            if (metrics.BiologicalTimingAccuracy < 0.7)
            {
                recommendations.Add("Improve synaptic timing to align with biological patterns (5-25ms windows)");
            }
            
            if (metrics.LearningEfficiency < 0.5)
            {
                recommendations.Add("Optimize learning algorithms to improve adaptation success rate");
            }
            
            if (metrics.AdaptationCapacity < 0.3)
            {
                recommendations.Add("Increase connection diversity to enhance adaptation capacity");
            }
            
            return recommendations;
        }
    }
}

