using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CxLanguage.IDE.WinUI.Services;

/// <summary>
/// Performance monitoring service for GitHub Issue #220 performance requirements
/// Tracks sub-100ms response time requirement
/// </summary>
public interface ICxPerformanceMonitor
{
    IDisposable StartOperation(string operationName);
    void RecordOperation(string operationName, TimeSpan duration);
    PerformanceStats GetStats(string operationName);
    PerformanceStats GetOverallStats();
    void ResetStats();
}

public class CxPerformanceMonitor : ICxPerformanceMonitor
{
    private readonly ILogger<CxPerformanceMonitor> _logger;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<TimeSpan>> _operationTimes = new();
    
    public CxPerformanceMonitor(ILogger<CxPerformanceMonitor> logger)
    {
        _logger = logger;
    }
    
    public IDisposable StartOperation(string operationName)
    {
        return new PerformanceScope(this, operationName);
    }
    
    public void RecordOperation(string operationName, TimeSpan duration)
    {
        var queue = _operationTimes.GetOrAdd(operationName, _ => new ConcurrentQueue<TimeSpan>());
        queue.Enqueue(duration);
        
        // Keep only recent measurements (last 100)
        while (queue.Count > 100)
        {
            queue.TryDequeue(out _);
        }
        
        // Log warning if operation exceeds 100ms threshold
        if (duration.TotalMilliseconds > 100)
        {
            _logger.LogWarning($"Performance warning: {operationName} took {duration.TotalMilliseconds:F0}ms (target: <100ms)");
        }
        else
        {
            _logger.LogDebug($"Performance: {operationName} completed in {duration.TotalMilliseconds:F0}ms");
        }
    }
    
    public PerformanceStats GetStats(string operationName)
    {
        if (!_operationTimes.TryGetValue(operationName, out var queue) || queue.IsEmpty)
        {
            return new PerformanceStats(operationName, 0, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, 0);
        }
        
        var times = queue.ToArray();
        var average = TimeSpan.FromTicks((long)times.Average(t => t.Ticks));
        var min = times.Min();
        var max = times.Max();
        var exceedsThreshold = times.Count(t => t.TotalMilliseconds > 100);
        
        return new PerformanceStats(operationName, times.Length, average, min, max, exceedsThreshold);
    }
    
    public PerformanceStats GetOverallStats()
    {
        var allTimes = _operationTimes.Values
            .SelectMany(queue => queue.ToArray())
            .ToArray();
        
        if (allTimes.Length == 0)
        {
            return new PerformanceStats("Overall", 0, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, 0);
        }
        
        var average = TimeSpan.FromTicks((long)allTimes.Average(t => t.Ticks));
        var min = allTimes.Min();
        var max = allTimes.Max();
        var exceedsThreshold = allTimes.Count(t => t.TotalMilliseconds > 100);
        
        return new PerformanceStats("Overall", allTimes.Length, average, min, max, exceedsThreshold);
    }
    
    public void ResetStats()
    {
        _operationTimes.Clear();
        _logger.LogInformation("Performance statistics reset");
    }
    
    private class PerformanceScope : IDisposable
    {
        private readonly CxPerformanceMonitor _monitor;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        
        public PerformanceScope(CxPerformanceMonitor monitor, string operationName)
        {
            _monitor = monitor;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
        }
        
        public void Dispose()
        {
            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed);
        }
    }
}

public record PerformanceStats(
    string OperationName,
    int SampleCount,
    TimeSpan AverageTime,
    TimeSpan MinTime,
    TimeSpan MaxTime,
    int ExceedsThresholdCount)
{
    public double AverageMilliseconds => AverageTime.TotalMilliseconds;
    public double MinMilliseconds => MinTime.TotalMilliseconds;
    public double MaxMilliseconds => MaxTime.TotalMilliseconds;
    public double ThresholdExceedanceRate => SampleCount > 0 ? (double)ExceedsThresholdCount / SampleCount * 100 : 0;
    
    public bool MeetsPerformanceTarget => AverageMilliseconds < 100 && ThresholdExceedanceRate < 10;
    
    public override string ToString()
    {
        return $"{OperationName}: Avg={AverageMilliseconds:F1}ms, Min={MinMilliseconds:F1}ms, Max={MaxMilliseconds:F1}ms, " +
               $"Samples={SampleCount}, Exceeds100ms={ExceedsThresholdCount} ({ThresholdExceedanceRate:F1}%)";
    }
}
