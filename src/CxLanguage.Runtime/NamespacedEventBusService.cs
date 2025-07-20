using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Runtime;

/// <summary>
/// Namespace-based Event Bus Service - Uses event names as scoping mechanism
/// Event names follow hierarchical patterns: scope.target.event
/// Examples: global.all.announcement, team.dev.task, role.manager.update, agent.alice.message
/// </summary>
public class NamespacedEventBusService
{
    private readonly ConcurrentDictionary<string, AgentSubscription> _agents = new();
    private readonly ConcurrentDictionary<string, List<CxEventHandler>> _eventHandlers = new();
    private readonly ILogger<NamespacedEventBusService>? _logger;
    private readonly object _lock = new object();

    public NamespacedEventBusService(ILogger<NamespacedEventBusService>? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("Namespaced Event Bus Service initialized");
    }

    #region Agent Registration

    /// <summary>
    /// Register an agent with namespace-based event routing
    /// </summary>
    public string RegisterAgent(string agentName, string? team = null, string? role = null, 
        string[]? channels = null, object? agentInstance = null)
    {
        var agentId = Guid.NewGuid().ToString("N")[..8];
        
        var subscription = new AgentSubscription
        {
            AgentId = agentId,
            AgentName = agentName,
            Role = role ?? "agent",
            Channels = channels?.ToHashSet() ?? new HashSet<string>(),
            AgentInstance = agentInstance,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Add team information
        if (!string.IsNullOrEmpty(team))
        {
            subscription.Channels.Add($"team.{team}");
        }

        _agents[agentId] = subscription;

        _logger?.LogInformation("Agent {AgentName} ({AgentId}) registered for namespace-based events", 
            agentName, agentId);

        return agentId;
    }

    /// <summary>
    /// Unregister an agent
    /// </summary>
    public bool UnregisterAgent(string agentId)
    {
        if (!_agents.TryRemove(agentId, out var subscription))
        {
            return false;
        }

        // Remove all event handlers for this agent
        var agentHandlers = _eventHandlers.Where(kvp => kvp.Key.Contains($"agent.{subscription.AgentName}")).ToList();
        foreach (var handler in agentHandlers)
        {
            _eventHandlers.TryRemove(handler.Key, out _);
        }

        _logger?.LogInformation("Agent {AgentName} ({AgentId}) unregistered", subscription.AgentName, agentId);
        return true;
    }

    #endregion

    #region Event Handler Registration

    /// <summary>
    /// Subscribe to events using namespace patterns
    /// Supports 'any' wildcards: team.any.task, role.manager.any, global.any
    /// </summary>
    public bool Subscribe(string agentId, string eventPattern, CxEventHandler handler)
    {
        if (!_agents.TryGetValue(agentId, out var subscription))
        {
            _logger?.LogWarning("Cannot subscribe - Agent {AgentId} not registered", agentId);
            return false;
        }

        lock (_lock)
        {
            if (!_eventHandlers.ContainsKey(eventPattern))
            {
                _eventHandlers[eventPattern] = new List<CxEventHandler>();
            }
            
            _eventHandlers[eventPattern].Add(handler);
        }

        _logger?.LogDebug("Agent {AgentName} subscribed to pattern: {EventPattern}", 
            subscription.AgentName, eventPattern);
        return true;
    }

    /// <summary>
    /// Unsubscribe from event pattern
    /// </summary>
    public bool Unsubscribe(string agentId, string eventPattern, CxEventHandler handler)
    {
        if (!_eventHandlers.TryGetValue(eventPattern, out var handlers))
        {
            return false;
        }

        lock (_lock)
        {
            var removed = handlers.Remove(handler);
            if (handlers.Count == 0)
            {
                _eventHandlers.TryRemove(eventPattern, out _);
            }
            return removed;
        }
    }

    #endregion

    #region Namespace-based Event Emission

    /// <summary>
    /// Emit event with namespace-based routing
    /// Event name determines scope: global.*, team.name.*, role.name.*, agent.name.*, channel.name.*
    /// </summary>
    public async Task EmitAsync(string eventName, object? data = null, string source = "System")
    {
        var payload = new CxEventPayload
        {
            EventName = eventName,
            Data = data,
            Timestamp = DateTime.UtcNow,
            Source = source
        };

        _logger?.LogDebug("Emitting namespaced event: {EventName} from {Source}", eventName, source);

        var matchingHandlers = FindMatchingHandlers(eventName);
        var tasks = new List<Task>();

        foreach (var handler in matchingHandlers)
        {
            try
            {
                tasks.Add(handler(payload));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing handler for event: {EventName}", eventName);
            }
        }

        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
                _logger?.LogDebug("Event {EventName} delivered to {HandlerCount} handlers", 
                    eventName, tasks.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing handlers for: {EventName}", eventName);
            }
        }
        else
        {
            _logger?.LogDebug("Event {EventName} had no matching handlers", eventName);
        }
    }

    /// <summary>
    /// Synchronous emit for compiled CX compatibility
    /// </summary>
    public void Emit(string eventName, object? data = null, string source = "System")
    {
        Task.Run(async () => await EmitAsync(eventName, data, source));
    }

    #endregion

    #region Namespace Pattern Matching

    /// <summary>
    /// Find handlers that match the event name using namespace patterns
    /// </summary>
    private List<CxEventHandler> FindMatchingHandlers(string eventName)
    {
        var matchingHandlers = new List<CxEventHandler>();
        var eventParts = eventName.Split('.');

        foreach (var handlerEntry in _eventHandlers)
        {
            var pattern = handlerEntry.Key;
            var handlers = handlerEntry.Value;

            if (IsEventMatch(eventName, eventParts, pattern))
            {
                matchingHandlers.AddRange(handlers);
                _logger?.LogTrace("Pattern {Pattern} matches event {EventName}", pattern, eventName);
            }
        }

        return matchingHandlers;
    }

    /// <summary>
    /// Check if an event matches a namespace pattern
    /// Supports exact matches, 'any' wildcards, and scope-based routing
    /// </summary>
    private bool IsEventMatch(string eventName, string[] eventParts, string pattern)
    {
        // Exact match
        if (eventName == pattern)
        {
            return true;
        }

        var patternParts = pattern.Split('.');

        // Different number of parts (unless wildcard at end)
        if (eventParts.Length != patternParts.Length && !pattern.EndsWith("any"))
        {
            return false;
        }

        // Check each part
        for (int i = 0; i < Math.Min(eventParts.Length, patternParts.Length); i++)
        {
            var eventPart = eventParts[i];
            var patternPart = patternParts[i];

            // 'any' wildcard matches anything
            if (patternPart == "any")
            {
                continue;
            }

            // Exact part match required
            if (eventPart != patternPart)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get scope information from event name
    /// </summary>
    private EventScope ParseEventScope(string eventName)
    {
        var parts = eventName.Split('.');
        if (parts.Length < 2)
        {
            return new EventScope { Type = "unknown", Target = "", Event = eventName };
        }

        return new EventScope
        {
            Type = parts[0],           // global, team, role, agent, channel
            Target = parts[1],         // specific target (team name, role name, agent name, etc.)
            Event = string.Join(".", parts.Skip(2))  // remaining parts as event name
        };
    }

    #endregion

    #region Helper Methods and Service Information

    /// <summary>
    /// Auto-subscribe agent to relevant namespace patterns based on their properties
    /// </summary>
    public void AutoSubscribeAgent(string agentId, CxEventHandler handler)
    {
        if (!_agents.TryGetValue(agentId, out var subscription))
        {
            return;
        }

        var patterns = new List<string>
        {
            "global.*",                                    // All global events
            $"agent.{subscription.AgentName}.*",          // Agent-specific events
            $"role.{subscription.Role}.*"                 // Role-specific events
        };

        // Subscribe to team events
        foreach (var channel in subscription.Channels.Where(c => c.StartsWith("team.")))
        {
            var teamName = channel.Substring(5); // Remove "team." prefix
            patterns.Add($"team.{teamName}.*");
        }

        // Subscribe to channel events
        foreach (var channel in subscription.Channels.Where(c => !c.StartsWith("team.")))
        {
            patterns.Add($"channel.{channel}.*");
        }

        // Register all patterns
        foreach (var pattern in patterns)
        {
            Subscribe(agentId, pattern, handler);
            _logger?.LogDebug("Auto-subscribed agent {AgentName} to pattern: {Pattern}", 
                subscription.AgentName, pattern);
        }
    }

    /// <summary>
    /// Get comprehensive service statistics
    /// </summary>
    public NamespacedBusStatistics GetStatistics()
    {
        var eventsByScope = _eventHandlers.Keys
            .GroupBy(pattern => pattern.Split('.')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        var agentsByRole = _agents.Values.Where(a => a.IsActive)
            .GroupBy(a => a.Role)
            .ToDictionary(g => g.Key, g => g.Count());

        return new NamespacedBusStatistics
        {
            TotalAgents = _agents.Count(a => a.Value.IsActive),
            TotalEventPatterns = _eventHandlers.Count,
            EventsByScope = eventsByScope,
            AgentsByRole = agentsByRole,
            TopEventPatterns = _eventHandlers
                .OrderByDescending(kvp => kvp.Value.Count)
                .Take(10)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count)
        };
    }

    /// <summary>
    /// Clear all registrations (for testing)
    /// </summary>
    public void Reset()
    {
        _agents.Clear();
        _eventHandlers.Clear();
        _logger?.LogInformation("Namespaced Event Bus Service reset");
    }

    #endregion
}

/// <summary>
/// Event scope information parsed from namespace
/// </summary>
public class EventScope
{
    public string Type { get; set; } = string.Empty;    // global, team, role, agent, channel
    public string Target { get; set; } = string.Empty;  // specific target identifier
    public string Event { get; set; } = string.Empty;   // actual event name
}

/// <summary>
/// Statistics for the namespaced event bus
/// </summary>
public class NamespacedBusStatistics
{
    public int TotalAgents { get; set; }
    public int TotalEventPatterns { get; set; }
    public Dictionary<string, int> EventsByScope { get; set; } = new();
    public Dictionary<string, int> AgentsByRole { get; set; } = new();
    public Dictionary<string, int> TopEventPatterns { get; set; } = new();
}

/// <summary>
/// Global registry for the Namespaced Event Bus Service
/// </summary>
public static class NamespacedEventBusRegistry
{
    private static NamespacedEventBusService? _instance;
    private static readonly object _lock = new object();

    public static NamespacedEventBusService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new NamespacedEventBusService();
                }
            }
            return _instance;
        }
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _instance?.Reset();
            _instance = null;
        }
    }
}
