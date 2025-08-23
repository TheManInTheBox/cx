using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using CxLanguage.Core.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CxLanguage.Core.IDE
{
    /// <summary>
    /// IDE Performance Monitor for real-time development metrics
    /// Provides instant feedback on IDE runtime performance
    /// </summary>
    public class IDEPerformanceMonitor
    {
        private readonly ILogger _logger;
        private readonly object _metricsLock = new object();
        private IDEPerformanceMetrics _currentMetrics;
        private DateTime _lastMetricsUpdate;
        private DateTime _lastCpuMeasurement;
        private TimeSpan _lastCpuTime;
        
        public IDEPerformanceMonitor(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentMetrics = new IDEPerformanceMetrics
            {
                Timestamp = DateTime.UtcNow,
                ActiveSessions = 0,
                TotalExecutions = 0,
                AverageExecutionTime = 0.0,
                EventsPerSecond = 0.0,
                CpuUsagePercent = 0.0,
                MemoryUsageBytes = 0,
                HardwareAccelerationActive = false
            };
            
            // Initialize CPU measurement baseline
            var currentProcess = Process.GetCurrentProcess();
            _lastCpuTime = currentProcess.TotalProcessorTime;
            _lastCpuMeasurement = DateTime.UtcNow;
            
            Console.WriteLine("📊 IDE Performance Monitor initialized");
        }
        
        /// <summary>
        /// Get cross-platform CPU usage percentage
        /// </summary>
        private double GetProcessCpuUsage()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var currentTime = DateTime.UtcNow;
                var currentCpuTime = currentProcess.TotalProcessorTime;
                
                if (_lastCpuMeasurement != default)
                {
                    var timeDiff = (currentTime - _lastCpuMeasurement).TotalMilliseconds;
                    var cpuDiff = (currentCpuTime - _lastCpuTime).TotalMilliseconds;
                    
                    if (timeDiff > 0)
                    {
                        var cpuUsage = (cpuDiff / timeDiff) * 100.0 / Environment.ProcessorCount;
                        _lastCpuTime = currentCpuTime;
                        _lastCpuMeasurement = currentTime;
                        return Math.Min(100.0, Math.Max(0.0, cpuUsage));
                    }
                }
                
                _lastCpuTime = currentCpuTime;
                _lastCpuMeasurement = currentTime;
                return 0.0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get CPU usage");
                return 0.0;
            }
        }
        
        /// <summary>
        /// Collect current performance metrics
        /// </summary>
        public async Task CollectMetricsAsync()
        {
            try
            {
                var timestamp = DateTime.UtcNow;
                var metrics = new IDEPerformanceMetrics { Timestamp = timestamp };
                
                // Collect CPU usage using cross-platform method
                metrics.CpuUsagePercent = Math.Round(GetProcessCpuUsage(), 2);
                
                // Collect memory usage
                var currentProcess = Process.GetCurrentProcess();
                metrics.MemoryUsageBytes = currentProcess.WorkingSet64;
                
                // Calculate events per second
                if (_lastMetricsUpdate != default)
                {
                    var timeDiff = (timestamp - _lastMetricsUpdate).TotalSeconds;
                    if (timeDiff > 0)
                    {
                        var executionDiff = metrics.TotalExecutions - _currentMetrics.TotalExecutions;
                        metrics.EventsPerSecond = Math.Round(executionDiff / timeDiff, 2);
                    }
                }
                
                // Update thread-safe metrics
                lock (_metricsLock)
                {
                    _currentMetrics = metrics;
                    _lastMetricsUpdate = timestamp;
                }
                
                // Collect additional IDE-specific metrics
                await CollectIDESpecificMetricsAsync(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect performance metrics");
            }
        }
        
        /// <summary>
        /// Get current performance metrics
        /// </summary>
        public IDEPerformanceMetrics GetCurrentMetrics()
        {
            lock (_metricsLock)
            {
                return new IDEPerformanceMetrics
                {
                    Timestamp = _currentMetrics.Timestamp,
                    ActiveSessions = _currentMetrics.ActiveSessions,
                    TotalExecutions = _currentMetrics.TotalExecutions,
                    AverageExecutionTime = _currentMetrics.AverageExecutionTime,
                    EventsPerSecond = _currentMetrics.EventsPerSecond,
                    CpuUsagePercent = _currentMetrics.CpuUsagePercent,
                    MemoryUsageBytes = _currentMetrics.MemoryUsageBytes,
                    HardwareAccelerationActive = _currentMetrics.HardwareAccelerationActive
                };
            }
        }
        
        /// <summary>
        /// Update execution metrics
        /// </summary>
        public void UpdateExecutionMetrics(int executionTimeMs, bool hardwareAccelerated)
        {
            lock (_metricsLock)
            {
                _currentMetrics.TotalExecutions++;
                
                // Update average execution time
                var currentTotal = _currentMetrics.AverageExecutionTime * (_currentMetrics.TotalExecutions - 1);
                _currentMetrics.AverageExecutionTime = (currentTotal + executionTimeMs) / _currentMetrics.TotalExecutions;
                
                // Update hardware acceleration status
                _currentMetrics.HardwareAccelerationActive = hardwareAccelerated;
            }
        }
        
        /// <summary>
        /// Update active session count
        /// </summary>
        public void UpdateActiveSessionCount(int activeSessions)
        {
            lock (_metricsLock)
            {
                _currentMetrics.ActiveSessions = activeSessions;
            }
        }
        
        /// <summary>
        /// Collect IDE-specific performance metrics
        /// </summary>
        private async Task CollectIDESpecificMetricsAsync(IDEPerformanceMetrics metrics)
        {
            try
            {
                // Simulate IDE-specific metrics collection
                await Task.Delay(1);
                
                // Additional metrics would be collected here
                // - Code completion response time
                // - Syntax highlighting performance
                // - IntelliSense response time
                // - Real-time compilation performance
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to collect IDE-specific metrics");
            }
        }
        
        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("📊 IDE Performance Monitor disposed");
        }
    }
    
    /// <summary>
    /// Consciousness Stream Processor for IDE integration
    /// Processes consciousness events in real-time for IDE visualization
    /// </summary>
    public class ConsciousnessStreamProcessor
    {
        private readonly ILogger _logger;
        private readonly ICxEventBus _eventBus;
        private readonly Dictionary<string, ConsciousnessStream> _activeStreams;
        private readonly object _streamsLock = new object();
        private bool _isProcessing;
        private CancellationTokenSource? _cancellationTokenSource;
        
        public ConsciousnessStreamProcessor(ILogger logger, ICxEventBus eventBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _activeStreams = new Dictionary<string, ConsciousnessStream>();
            
            Console.WriteLine("🧠 Consciousness Stream Processor initialized");
        }
        
        /// <summary>
        /// Start consciousness stream processing
        /// </summary>
        public async Task StartProcessingAsync()
        {
            if (_isProcessing)
            {
                return;
            }
            
            _isProcessing = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            Console.WriteLine("🧠 Consciousness stream processing started");
            
            // Subscribe to consciousness events
            _eventBus.Subscribe("consciousness", ProcessConsciousnessEvent);
            _eventBus.Subscribe("ide.execution", ProcessIDEExecutionEvent);
            _eventBus.Subscribe("ide.session", ProcessIDESessionEvent);
            
            // Start stream monitoring loop
            _ = Task.Run(() => MonitorConsciousnessStreamsAsync(_cancellationTokenSource.Token));
        }
        
        /// <summary>
        /// Stop consciousness stream processing
        /// </summary>
        public async Task StopProcessingAsync()
        {
            if (!_isProcessing)
            {
                return;
            }
            
            _isProcessing = false;
            _cancellationTokenSource?.Cancel();
            
            Console.WriteLine("🧠 Consciousness stream processing stopped");
            
            // Unsubscribe from events
            _eventBus.Unsubscribe("consciousness", ProcessConsciousnessEvent);
            _eventBus.Unsubscribe("ide.execution", ProcessIDEExecutionEvent);
            _eventBus.Unsubscribe("ide.session", ProcessIDESessionEvent);
            
            // Cleanup active streams
            lock (_streamsLock)
            {
                _activeStreams.Clear();
            }
        }
        
        /// <summary>
        /// Initialize consciousness stream for a session
        /// </summary>
        public async Task InitializeSessionStreamAsync(string sessionId)
        {
            var stream = new ConsciousnessStream
            {
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EventCount = 0,
                LastEventTime = DateTime.UtcNow,
                HealthScore = 1.0,
                ActivePatterns = new List<string>()
            };
            
            lock (_streamsLock)
            {
                _activeStreams[sessionId] = stream;
            }
            
            Console.WriteLine($"🧠 Consciousness stream initialized for session: {sessionId}");
            
            // Emit stream initialization event
            await _eventBus.EmitAsync("consciousness.stream.initialized", new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["timestamp"] = DateTime.UtcNow
            });
        }
        
        /// <summary>
        /// Cleanup consciousness stream for a session
        /// </summary>
        public async Task CleanupSessionStreamAsync(string sessionId)
        {
            lock (_streamsLock)
            {
                if (_activeStreams.TryGetValue(sessionId, out var stream))
                {
                    stream.IsActive = false;
                    _activeStreams.Remove(sessionId);
                }
            }
            
            Console.WriteLine($"🧠 Consciousness stream cleaned up for session: {sessionId}");
            
            // Emit stream cleanup event
            await _eventBus.EmitAsync("consciousness.stream.cleanup", new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["timestamp"] = DateTime.UtcNow
            });
        }
        
        /// <summary>
        /// Get consciousness stream status for a session
        /// </summary>
        public ConsciousnessStreamStatus? GetStreamStatus(string sessionId)
        {
            lock (_streamsLock)
            {
                if (_activeStreams.TryGetValue(sessionId, out var stream))
                {
                    return new ConsciousnessStreamStatus
                    {
                        IsActive = stream.IsActive,
                        StreamEventCount = stream.EventCount,
                        LastEventTime = stream.LastEventTime,
                        StreamHealthScore = stream.HealthScore,
                        ActivePatterns = stream.ActivePatterns.ToArray()
                    };
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Process consciousness events
        /// </summary>
        private async Task<bool> ProcessConsciousnessEvent(object? sender, string eventName, IDictionary<string, object>? eventData)
        {
            try
            {
                if (eventData == null) return true;
                
                var sessionId = eventData.TryGetValue("sessionId", out var sid) ? sid?.ToString() : null;
                
                if (sessionId != null)
                {
                    await UpdateConsciousnessStream(sessionId, "consciousness", eventData);
                }
                
                // Emit processed consciousness event for IDE visualization
                await _eventBus.EmitAsync("ide.consciousness.processed", eventData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process consciousness event");
                return false;
            }
        }
        
        /// <summary>
        /// Process IDE execution events
        /// </summary>
        private async Task<bool> ProcessIDEExecutionEvent(object? sender, string eventName, IDictionary<string, object>? eventData)
        {
            try
            {
                if (eventData == null) return true;
                
                var sessionId = eventData.TryGetValue("sessionId", out var sid) ? sid?.ToString() : null;
                
                if (sessionId != null)
                {
                    await UpdateConsciousnessStream(sessionId, "execution", eventData);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process IDE execution event");
                return false;
            }
        }
        
        /// <summary>
        /// Process IDE session events
        /// </summary>
        private async Task<bool> ProcessIDESessionEvent(object? sender, string eventName, IDictionary<string, object>? eventData)
        {
            try
            {
                if (eventData == null) return true;
                
                var sessionId = eventData.TryGetValue("sessionId", out var sid) ? sid?.ToString() : null;
                
                if (sessionId != null)
                {
                    await UpdateConsciousnessStream(sessionId, "session", eventData);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process IDE session event");
                return false;
            }
        }
        
        /// <summary>
        /// Update consciousness stream with new event
        /// </summary>
        private async Task UpdateConsciousnessStream(string sessionId, string eventType, IDictionary<string, object> eventData)
        {
            lock (_streamsLock)
            {
                if (_activeStreams.TryGetValue(sessionId, out var stream))
                {
                    stream.EventCount++;
                    stream.LastEventTime = DateTime.UtcNow;
                    
                    // Update active patterns
                    if (!stream.ActivePatterns.Contains(eventType))
                    {
                        stream.ActivePatterns.Add(eventType);
                    }
                    
                    // Update health score based on event frequency
                    var timeSinceLastEvent = DateTime.UtcNow - stream.LastEventTime;
                    if (timeSinceLastEvent.TotalSeconds < 1.0)
                    {
                        stream.HealthScore = Math.Min(1.0, stream.HealthScore + 0.1);
                    }
                    else if (timeSinceLastEvent.TotalSeconds > 10.0)
                    {
                        stream.HealthScore = Math.Max(0.1, stream.HealthScore - 0.1);
                    }
                }
            }
            
            // This would emit stream update events for real-time visualization
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Monitor consciousness streams for health and performance
        /// </summary>
        private async Task MonitorConsciousnessStreamsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, cancellationToken); // Monitor every second
                    
                    lock (_streamsLock)
                    {
                        foreach (var stream in _activeStreams.Values)
                        {
                            // Check stream health
                            var timeSinceLastEvent = DateTime.UtcNow - stream.LastEventTime;
                            if (timeSinceLastEvent.TotalSeconds > 30.0)
                            {
                                stream.HealthScore = Math.Max(0.1, stream.HealthScore - 0.05);
                            }
                            
                            // Clean up old patterns
                            if (timeSinceLastEvent.TotalMinutes > 5.0)
                            {
                                stream.ActivePatterns.Clear();
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in consciousness stream monitoring");
                }
            }
        }
    }
    
    /// <summary>
    /// Consciousness stream data structure
    /// </summary>
    public class ConsciousnessStream
    {
        public required string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int EventCount { get; set; }
        public DateTime LastEventTime { get; set; }
        public double HealthScore { get; set; }
        public List<string> ActivePatterns { get; set; } = new();
    }
}
