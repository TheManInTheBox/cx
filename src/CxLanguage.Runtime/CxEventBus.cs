using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Runtime;

/// <summary>
/// Event payload wrapper for CX event-driven architecture
/// </summary>
public class CxEventPayload
{
    public string EventName { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "Unknown";
}

/// <summary>
/// Event handler delegate for CX events
/// </summary>
public delegate Task CxEventHandler(CxEventPayload payload);

/// <summary>
/// Core Event Bus for CX autonomous programming language
/// Enables agent-to-agent communication through event-driven architecture
/// </summary>
public class CxEventBus
{
    private readonly ConcurrentDictionary<string, List<CxEventHandler>> _eventHandlers;
    private readonly ILogger<CxEventBus>? _logger;
    private readonly object _lock = new object();

    public CxEventBus(ILogger<CxEventBus>? logger = null)
    {
        _eventHandlers = new ConcurrentDictionary<string, List<CxEventHandler>>();
        _logger = logger;
        _logger?.LogInformation("CX Event Bus initialized");
    }

    /// <summary>
    /// Subscribe to an event with a handler function
    /// </summary>
    public void Subscribe(string eventName, CxEventHandler handler)
    {
        lock (_lock)
        {
            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = new List<CxEventHandler>();
            }
            
            _eventHandlers[eventName].Add(handler);
            _logger?.LogDebug("Subscribed to event: {EventName}", eventName);
        }
    }

    /// <summary>
    /// Unsubscribe from an event
    /// </summary>
    public void Unsubscribe(string eventName, CxEventHandler handler)
    {
        lock (_lock)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName].Remove(handler);
                if (_eventHandlers[eventName].Count == 0)
                {
                    _eventHandlers.TryRemove(eventName, out _);
                }
                _logger?.LogDebug("Unsubscribed from event: {EventName}", eventName);
            }
        }
    }

    /// <summary>
    /// Emit an event with data payload
    /// </summary>
    public async Task EmitAsync(string eventName, object? data = null, string source = "Unknown")
    {
        var payload = new CxEventPayload
        {
            EventName = eventName,
            Data = data,
            Timestamp = DateTime.UtcNow,
            Source = source
        };

        _logger?.LogDebug("Emitting event: {EventName} from {Source}", eventName, source);

        if (_eventHandlers.TryGetValue(eventName, out var handlers))
        {
            var tasks = new List<Task>();
            
            // Execute all handlers concurrently
            foreach (var handler in handlers.ToArray()) // ToArray to avoid modification during iteration
            {
                try
                {
                    tasks.Add(handler(payload));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error executing event handler for: {EventName}", eventName);
                }
            }

            // Wait for all handlers to complete
            if (tasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(tasks);
                    _logger?.LogDebug("All handlers completed for event: {EventName}", eventName);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error waiting for event handlers to complete: {EventName}", eventName);
                }
            }
        }
        else
        {
            _logger?.LogWarning("No handlers registered for event: {EventName}", eventName);
        }
    }

    /// <summary>
    /// Synchronous emit for compatibility with compiled CX code
    /// </summary>
    public void Emit(string eventName, object? data = null, string source = "Unknown")
    {
        // Run async version synchronously for compiled code compatibility
        Task.Run(async () => await EmitAsync(eventName, data, source));
    }

    /// <summary>
    /// Get all registered event names
    /// </summary>
    public IEnumerable<string> GetRegisteredEvents()
    {
        return _eventHandlers.Keys.ToArray();
    }

    /// <summary>
    /// Get handler count for an event
    /// </summary>
    public int GetHandlerCount(string eventName)
    {
        return _eventHandlers.TryGetValue(eventName, out var handlers) ? handlers.Count : 0;
    }

    /// <summary>
    /// Clear all event handlers (useful for testing)
    /// </summary>
    public void ClearAllHandlers()
    {
        lock (_lock)
        {
            _eventHandlers.Clear();
            _logger?.LogInformation("All event handlers cleared");
        }
    }

    /// <summary>
    /// Get debugging information about the event bus state
    /// </summary>
    public Dictionary<string, int> GetDebugInfo()
    {
        var info = new Dictionary<string, int>();
        foreach (var kvp in _eventHandlers)
        {
            info[kvp.Key] = kvp.Value.Count;
        }
        return info;
    }
}

/// <summary>
/// Static global event bus instance for CX runtime
/// This enables event-driven communication across all CX agents and scripts
/// </summary>
public static class GlobalEventBus
{
    private static CxEventBus? _instance;
    private static readonly object _lock = new object();

    /// <summary>
    /// Get or create the global event bus instance
    /// </summary>
    public static CxEventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new CxEventBus();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Reset the global event bus (useful for testing)
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _instance?.ClearAllHandlers();
            _instance = null;
        }
    }
}
