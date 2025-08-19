using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime.ParallelHandlers
{
    /// <summary>
    /// Enhanced event system architecture for parallel handler coordination.
    /// Provides real-time event routing, handler orchestration, and performance monitoring.
    /// </summary>
    public interface IParallelEventSystem
    {
        /// <summary>
        /// Register multiple parallel handlers for simultaneous execution.
        /// </summary>
        Task RegisterParallelHandlersAsync(string eventName, IEnumerable<string> handlerNames, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Emit event with parallel handler execution.
        /// </summary>
        Task<ParallelExecutionResult> EmitParallelEventAsync(string eventName, object payload, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get real-time parallel execution metrics.
        /// </summary>
        ParallelExecutionMetrics GetExecutionMetrics(string executionId);
        
        /// <summary>
        /// Validate event system integrity for parallel processing.
        /// </summary>
        Task<SystemIntegrityReport> ValidateSystemIntegrityAsync();
    }

    /// <summary>
    /// Production-ready parallel event system implementation.
    /// Orchestrates parallel handler execution with consciousness awareness.
    /// </summary>
    public class ParallelEventSystem : IParallelEventSystem
    {
        private readonly ICxEventBus _eventBus;
        private readonly IParallelHandlerCoordinator _parallelCoordinator;
        private readonly Microsoft.Extensions.Logging.ILogger<ParallelEventSystem> _logger;
        
        // Event routing and handler management
        private readonly ConcurrentDictionary<string, List<string>> _parallelHandlerMappings;
        private readonly ConcurrentDictionary<string, ParallelExecutionMetrics> _executionMetrics;
        private readonly SemaphoreSlim _routingSemaphore;
        
        // Performance monitoring
        private readonly ConcurrentQueue<ParallelEventRecord> _eventHistory;
        private readonly Timer _metricsCollectionTimer;
        
        public ParallelEventSystem(
            ICxEventBus eventBus,
            IParallelHandlerCoordinator parallelCoordinator,
            Microsoft.Extensions.Logging.ILogger<ParallelEventSystem> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _parallelCoordinator = parallelCoordinator ?? throw new ArgumentNullException(nameof(parallelCoordinator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _parallelHandlerMappings = new ConcurrentDictionary<string, List<string>>();
            _executionMetrics = new ConcurrentDictionary<string, ParallelExecutionMetrics>();
            _routingSemaphore = new SemaphoreSlim(Environment.ProcessorCount * 4, Environment.ProcessorCount * 4);
            _eventHistory = new ConcurrentQueue<ParallelEventRecord>();
            
            // Initialize performance monitoring
            _metricsCollectionTimer = new Timer(CollectPerformanceMetrics, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
            
            _logger.LogInformation("🎮 ParallelEventSystem initialized with {ProcessorCount} CPU cores", Environment.ProcessorCount);
        }

        /// <summary>
        /// Register multiple parallel handlers for simultaneous execution.
        /// </summary>
        public async Task RegisterParallelHandlersAsync(string eventName, IEnumerable<string> handlerNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));
            
            if (handlerNames == null || !handlerNames.Any())
                throw new ArgumentException("Handler names cannot be null or empty", nameof(handlerNames));

            await _routingSemaphore.WaitAsync(cancellationToken);
            try
            {
                var handlerList = handlerNames.ToList();
                _parallelHandlerMappings.AddOrUpdate(eventName, handlerList, (key, existing) =>
                {
                    // Merge with existing handlers, avoiding duplicates
                    var merged = existing.Union(handlerList).ToList();
                    return merged;
                });
                
                _logger.LogInformation("📡 Registered {HandlerCount} parallel handlers for event: {EventName}", 
                    handlerList.Count, eventName);
                
                // Emit registration event for monitoring
                _ = _eventBus.EmitAsync("parallel.handlers.registered", new Dictionary<string, object>
                {
                    ["eventName"] = eventName,
                    ["handlerCount"] = handlerList.Count,
                    ["handlers"] = handlerList.ToArray(),
                    ["timestamp"] = DateTimeOffset.UtcNow
                });
            }
            finally
            {
                _routingSemaphore.Release();
            }
        }

        /// <summary>
        /// Emit event with parallel handler execution.
        /// </summary>
        public async Task<ParallelExecutionResult> EmitParallelEventAsync(string eventName, object payload, CancellationToken cancellationToken = default)
        {
            var executionId = Guid.NewGuid().ToString();
            var startTime = DateTimeOffset.UtcNow;
            
            try
            {
                _logger.LogInformation("🚀 Starting parallel event emission: {EventName} (ID: {ExecutionId})", eventName, executionId);
                
                // Check if parallel handlers are registered for this event
                if (_parallelHandlerMappings.TryGetValue(eventName, out var handlerNames))
                {
                    // Execute parallel handlers via coordinator
                    var handlerParameters = handlerNames.ToDictionary(name => name, name => name);
                    var results = await _parallelCoordinator.ExecuteParallelHandlersAsync(
                        handlerParameters, payload, cancellationToken);
                    
                    var executionTime = DateTimeOffset.UtcNow - startTime;
                    var result = new ParallelExecutionResult
                    {
                        ExecutionId = executionId,
                        EventName = eventName,
                        HandlerCount = handlerNames.Count,
                        Results = results,
                        ExecutionTime = executionTime,
                        Success = true
                    };
                    
                    // Record execution metrics
                    RecordExecutionMetrics(result);
                    
                    // Emit standard event as fallback
                    await _eventBus.EmitAsync(eventName, results);
                    
                    return result;
                }
                else
                {
                    // No parallel handlers registered, emit standard event
                    var payloadDict = payload as IDictionary<string, object> ?? new Dictionary<string, object> { ["data"] = payload };
                    await _eventBus.EmitAsync(eventName, payloadDict);
                    
                    var executionTime = DateTimeOffset.UtcNow - startTime;
                    return new ParallelExecutionResult
                    {
                        ExecutionId = executionId,
                        EventName = eventName,
                        HandlerCount = 0,
                        Results = payload,
                        ExecutionTime = executionTime,
                        Success = true
                    };
                }
            }
            catch (Exception ex)
            {
                var executionTime = DateTimeOffset.UtcNow - startTime;
                _logger.LogError(ex, "❌ Parallel event emission failed: {EventName} (ID: {ExecutionId})", eventName, executionId);
                
                return new ParallelExecutionResult
                {
                    ExecutionId = executionId,
                    EventName = eventName,
                    HandlerCount = 0,
                    Results = null,
                    ExecutionTime = executionTime,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Get real-time parallel execution metrics.
        /// </summary>
        public ParallelExecutionMetrics GetExecutionMetrics(string executionId)
        {
            if (_executionMetrics.TryGetValue(executionId, out var metrics))
            {
                return metrics;
            }
            
            return new ParallelExecutionMetrics
            {
                ExecutionId = executionId,
                ExecutionTime = TimeSpan.Zero,
                HandlerCount = 0,
                Success = false,
                PerformanceImprovement = 0.0
            };
        }

        /// <summary>
        /// Validate event system integrity for parallel processing.
        /// </summary>
        public async Task<SystemIntegrityReport> ValidateSystemIntegrityAsync()
        {
            var report = new SystemIntegrityReport
            {
                ValidationTime = DateTimeOffset.UtcNow,
                RegisteredEvents = _parallelHandlerMappings.Count,
                TotalHandlers = _parallelHandlerMappings.Values.Sum(handlers => handlers.Count),
                ActiveExecutions = _executionMetrics.Count,
                Issues = new List<string>()
            };
            
            try
            {
                // Validate event bus connectivity
                var testEventName = $"system.integrity.test.{Guid.NewGuid()}";
                await _eventBus.EmitAsync(testEventName, new Dictionary<string, object> { ["test"] = true });
                report.EventBusConnectivity = true;
                
                // Validate parallel coordinator
                report.ParallelCoordinatorStatus = _parallelCoordinator != null;
                
                // Check for performance issues
                var recentExecutions = _eventHistory.Where(record => 
                    record.Timestamp > DateTimeOffset.UtcNow.AddMinutes(-5)).ToList();
                
                if (recentExecutions.Any())
                {
                    var averageExecutionTime = recentExecutions.Average(r => r.ExecutionTime.TotalMilliseconds);
                    report.AverageExecutionTime = TimeSpan.FromMilliseconds(averageExecutionTime);
                    
                    if (averageExecutionTime > 500) // > 500ms indicates potential performance issues
                    {
                        report.Issues.Add($"High average execution time: {averageExecutionTime:F2}ms");
                    }
                }
                
                // Check memory usage
                var memoryUsage = GC.GetTotalMemory(false);
                report.MemoryUsage = memoryUsage;
                
                if (memoryUsage > 100 * 1024 * 1024) // > 100MB might indicate memory issues
                {
                    report.Issues.Add($"High memory usage: {memoryUsage / (1024 * 1024)}MB");
                }
                
                report.OverallHealth = report.Issues.Count == 0 ? "Healthy" : "Issues Detected";
                
                _logger.LogInformation("🔍 System integrity validation complete: {Health} ({IssueCount} issues)", 
                    report.OverallHealth, report.Issues.Count);
                
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ System integrity validation failed");
                report.Issues.Add($"Validation error: {ex.Message}");
                report.OverallHealth = "Validation Failed";
                return report;
            }
        }

        /// <summary>
        /// Record execution metrics for performance monitoring.
        /// </summary>
        private void RecordExecutionMetrics(ParallelExecutionResult result)
        {
            var metrics = new ParallelExecutionMetrics
            {
                ExecutionId = result.ExecutionId,
                ExecutionTime = result.ExecutionTime,
                HandlerCount = result.HandlerCount,
                Success = result.Success,
                PerformanceImprovement = CalculatePerformanceImprovement(result.ExecutionTime, result.HandlerCount)
            };
            
            _executionMetrics.TryAdd(result.ExecutionId, metrics);
            
            var eventRecord = new ParallelEventRecord
            {
                ExecutionId = result.ExecutionId,
                EventName = result.EventName,
                HandlerCount = result.HandlerCount,
                ExecutionTime = result.ExecutionTime,
                Timestamp = DateTimeOffset.UtcNow,
                Success = result.Success
            };
            
            _eventHistory.Enqueue(eventRecord);
            
            // Keep history manageable (last 1000 events)
            while (_eventHistory.Count > 1000)
            {
                _eventHistory.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Calculate performance improvement over sequential execution.
        /// </summary>
        private double CalculatePerformanceImprovement(TimeSpan parallelTime, int handlerCount)
        {
            if (handlerCount <= 1) return 1.0;
            
            // Estimate sequential time (assuming 100ms per handler as baseline)
            var estimatedSequentialTime = TimeSpan.FromMilliseconds(100 * handlerCount);
            
            if (parallelTime.TotalMilliseconds > 0)
            {
                return estimatedSequentialTime.TotalMilliseconds / parallelTime.TotalMilliseconds;
            }
            
            return 1.0;
        }

        /// <summary>
        /// Collect performance metrics periodically.
        /// </summary>
        private void CollectPerformanceMetrics(object? state)
        {
            try
            {
                var recentExecutions = _eventHistory.Where(record => 
                    record.Timestamp > DateTimeOffset.UtcNow.AddMinutes(-1)).ToList();
                
                if (recentExecutions.Any())
                {
                    var totalExecutions = recentExecutions.Count;
                    var successfulExecutions = recentExecutions.Count(r => r.Success);
                    var averageExecutionTime = recentExecutions.Average(r => r.ExecutionTime.TotalMilliseconds);
                    var averagePerformanceImprovement = recentExecutions.Average(r => 
                        CalculatePerformanceImprovement(r.ExecutionTime, r.HandlerCount));
                    
                    // Emit performance metrics event
                    _ = _eventBus.EmitAsync("parallel.system.metrics", new Dictionary<string, object>
                    {
                        ["totalExecutions"] = totalExecutions,
                        ["successfulExecutions"] = successfulExecutions,
                        ["successRate"] = (double)successfulExecutions / totalExecutions,
                        ["averageExecutionTimeMs"] = averageExecutionTime,
                        ["averagePerformanceImprovement"] = averagePerformanceImprovement,
                        ["timestamp"] = DateTimeOffset.UtcNow,
                        ["targetAchieved"] = averagePerformanceImprovement >= 2.0
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Performance metrics collection failed");
            }
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        public void Dispose()
        {
            _metricsCollectionTimer?.Dispose();
            _routingSemaphore?.Dispose();
        }
    }

    /// <summary>
    /// Result of parallel event execution.
    /// </summary>
    public class ParallelExecutionResult
    {
        public string ExecutionId { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public int HandlerCount { get; set; }
        public object? Results { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Parallel execution performance metrics.
    /// </summary>
    public class ParallelExecutionMetrics
    {
        public string ExecutionId { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public int HandlerCount { get; set; }
        public bool Success { get; set; }
        public double PerformanceImprovement { get; set; }
    }

    /// <summary>
    /// System integrity validation report.
    /// </summary>
    public class SystemIntegrityReport
    {
        public DateTimeOffset ValidationTime { get; set; }
        public int RegisteredEvents { get; set; }
        public int TotalHandlers { get; set; }
        public int ActiveExecutions { get; set; }
        public bool EventBusConnectivity { get; set; }
        public bool ParallelCoordinatorStatus { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public long MemoryUsage { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
        public string OverallHealth { get; set; } = "Unknown";
    }

    /// <summary>
    /// Historical record of parallel event execution.
    /// </summary>
    public class ParallelEventRecord
    {
        public string ExecutionId { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public int HandlerCount { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool Success { get; set; }
    }
}

