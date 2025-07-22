using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime;

/// <summary>
/// Event scoping strategies for the unified event bus
/// </summary>
public enum UnifiedEventScope
{
    /// <summary>
    /// Global scope - all agents receive all events
    /// </summary>
    Global,
    
    /// <summary>
    /// Agent-specific scope - events only within agent instance
    /// </summary>
    Agent,
    
    /// <summary>
    /// Channel-based scope - agents join specific channels/topics
    /// </summary>
    Channel,
    
    /// <summary>
    /// Role-based scope - events filtered by agent role
    /// </summary>
    Role,
    
    /// <summary>
    /// Namespace-based scope - events routed by namespace patterns
    /// </summary>
    Namespace
}

/// <summary>
/// Agent subscription for event bus registration
/// </summary>
public class EventSubscription
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public UnifiedEventScope Scope { get; set; } = UnifiedEventScope.Global;
    public HashSet<string> Channels { get; set; } = new();
    public HashSet<string> EventFilters { get; set; } = new();
    public object? Instance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string NamespacePrefix { get; set; } = string.Empty; // For namespace-scoped events
}

/// <summary>
/// Event payload wrapper for CX event-driven architecture
/// </summary>
public class EventPayload
{
    public string EventName { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "Unknown";
    public string TargetScope { get; set; } = string.Empty;
}

/// <summary>
/// Event handler delegate for unified event bus
/// </summary>
public delegate Task EventHandler(EventPayload payload);

/// <summary>
/// Statistics for monitoring and debugging the event bus
/// </summary>
public class EventBusStatistics
{
    public int TotalSubscriptions { get; set; }
    public int TotalEventTypes { get; set; }
    public int TotalHandlers { get; set; }
    public Dictionary<UnifiedEventScope, int> SubscriptionsByScope { get; set; } = new();
    public Dictionary<string, int> EventsByType { get; set; } = new();
    public Dictionary<string, int> SubscriptionsByRole { get; set; } = new();
    public Dictionary<string, int> TopEventPatterns { get; set; } = new();
    public DateTime LastEventEmitted { get; set; }
    public DateTime LastStatisticsUpdate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Unified Event Bus for CX Language - consolidates all event bus functionality
/// Supports namespace scoping, agent management, and multiple scoping strategies
/// </summary>
public class UnifiedEventBus : ICxEventBus
{
    #region Private Fields
    
    private readonly ConcurrentDictionary<string, EventSubscription> _subscriptions = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<EventHandler>>> _handlers = new();
    private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _icxHandlers = new(); // For ICxEventBus compatibility
    private readonly ConcurrentDictionary<string, HashSet<string>> _channelMembers = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _roleMembers = new();
    private readonly ILogger<UnifiedEventBus>? _logger;
    private readonly object _lock = new object();
    
    #endregion

    #region Constructors

    public UnifiedEventBus(ILogger<UnifiedEventBus>? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("Unified Event Bus initialized");
    }

    #endregion

    #region ICxEventBus Implementation (Azure Integration)

    /// <summary>
    /// Emit an event to the CX event system (ICxEventBus interface)
    /// </summary>
    public async Task EmitAsync(string eventName, object payload)
    {
        _logger?.LogDebug("ICxEventBus.EmitAsync: {EventName}", eventName);
        
        // Handle ICxEventBus-style handlers (for Azure services)
        if (_icxHandlers.TryGetValue(eventName, out var icxHandlers))
        {
            var tasks = new List<Task>();
            foreach (var handler in icxHandlers.ToArray())
            {
                try
                {
                    tasks.Add(handler(payload ?? new object()));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "ICxEventBus handler failed for event: {EventName}", eventName);
                }
            }
            
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }

        // Also emit through unified event system
        await EmitUnifiedAsync(eventName, payload, "ICxEventBus", UnifiedEventScope.Global);
        
        Console.WriteLine($"🔥 CX EVENT EMITTED: {eventName} (ICx: {(_icxHandlers.ContainsKey(eventName) ? _icxHandlers[eventName].Count : 0)}, Unified: {GetHandlerCount(eventName)})");
    }

    /// <summary>
    /// Subscribe to events from the CX event system (ICxEventBus interface)
    /// </summary>
    public void Subscribe(string eventName, Func<object, Task> handler)
    {
        _logger?.LogDebug("ICxEventBus.Subscribe: {EventName}", eventName);
        
        if (!_icxHandlers.ContainsKey(eventName))
        {
            _icxHandlers[eventName] = new List<Func<object, Task>>();
        }
        
        _icxHandlers[eventName].Add(handler);
        _logger?.LogDebug("ICxEventBus subscription added: {EventName} now has {Count} ICx handlers", 
            eventName, _icxHandlers[eventName].Count);
    }

    /// <summary>
    /// Unsubscribe from events (ICxEventBus interface)
    /// </summary>
    public void Unsubscribe(string eventName)
    {
        _logger?.LogDebug("ICxEventBus.Unsubscribe: {EventName}", eventName);
        
        if (_icxHandlers.TryRemove(eventName, out _))
        {
            _logger?.LogDebug("ICxEventBus handlers removed for: {EventName}", eventName);
        }
    }

    #endregion

    #region Unified Event System (Core Functionality)

    /// <summary>
    /// Register a subscription (agent, instance, or service) with the event bus
    /// </summary>
    public string RegisterSubscription(string name, string role = "agent", UnifiedEventScope scope = UnifiedEventScope.Global,
        string[]? channels = null, string[]? eventFilters = null, object? instance = null)
    {
        var subscriptionId = Guid.NewGuid().ToString("N")[..8];
        
        var subscription = new EventSubscription
        {
            SubscriptionId = subscriptionId,
            Name = name,
            Role = role,
            Scope = scope,
            Channels = channels?.ToHashSet() ?? new HashSet<string>(),
            EventFilters = eventFilters?.ToHashSet() ?? new HashSet<string>(),
            Instance = instance,
            IsActive = true,
            NamespacePrefix = scope == UnifiedEventScope.Namespace ? $"{name}." : string.Empty
        };

        _subscriptions[subscriptionId] = subscription;
        
        // Initialize handler registry for this subscription
        _handlers[subscriptionId] = new ConcurrentDictionary<string, List<EventHandler>>();

        // Track role and channel membership
        UpdateMembership(subscriptionId, subscription);

        _logger?.LogInformation("Registered subscription: {Name} ({Id}) with {Scope} scope, role: {Role}", 
            name, subscriptionId, scope, role);

        return subscriptionId;
    }

    /// <summary>
    /// Subscribe a registered subscription to specific events
    /// </summary>
    public bool Subscribe(string subscriptionId, string eventPattern, EventHandler handler)
    {
        if (!_subscriptions.TryGetValue(subscriptionId, out var subscription))
        {
            _logger?.LogWarning("Cannot subscribe - subscription not found: {Id}", subscriptionId);
            return false;
        }

        var subscriptionHandlers = _handlers[subscriptionId];
        
        if (!subscriptionHandlers.ContainsKey(eventPattern))
        {
            subscriptionHandlers[eventPattern] = new List<EventHandler>();
        }

        subscriptionHandlers[eventPattern].Add(handler);

        _logger?.LogDebug("Subscription {Name} subscribed to: {Pattern}", subscription.Name, eventPattern);
        return true;
    }

    /// <summary>
    /// Emit an event through the unified event system with intelligent scoping
    /// </summary>
    public async Task EmitUnifiedAsync(string eventName, object? data = null, string source = "System", 
        UnifiedEventScope? forcedScope = null, string? targetChannel = null, string? targetRole = null)
    {
        var payload = new EventPayload
        {
            EventName = eventName,
            Data = data,
            Timestamp = DateTime.UtcNow,
            Source = source
        };

        _logger?.LogDebug("Emitting unified event: {EventName} from {Source}", eventName, source);

        var tasks = new List<Task>();
        var handlerCount = 0;

        foreach (var subscriptionKvp in _subscriptions.Where(s => s.Value.IsActive))
        {
            var subscriptionId = subscriptionKvp.Key;
            var subscription = subscriptionKvp.Value;

            // Apply scoping logic
            if (!ShouldReceiveEvent(subscription, eventName, forcedScope, targetChannel, targetRole))
            {
                continue;
            }

            // Get matching handlers for this subscription
            var matchingHandlers = GetMatchingHandlers(subscriptionId, eventName, subscription);
            
            foreach (var handler in matchingHandlers)
            {
                try
                {
                    tasks.Add(ExecuteHandlerWithContext(handler, payload, subscription));
                    handlerCount++;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error queuing handler for subscription: {Name}", subscription.Name);
                }
            }
        }

        // Execute all handlers concurrently
        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
                _logger?.LogDebug("Event {EventName} delivered to {HandlerCount} handlers", eventName, handlerCount);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing handlers for event: {EventName}", eventName);
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
        Task.Run(async () => await EmitUnifiedAsync(eventName, data, source));
    }

    /// <summary>
    /// Unregister a subscription from the event bus
    /// </summary>
    public bool UnregisterSubscription(string subscriptionId)
    {
        if (!_subscriptions.TryRemove(subscriptionId, out var subscription))
        {
            return false;
        }

        // Clean up handlers
        _handlers.TryRemove(subscriptionId, out _);

        // Clean up membership tracking
        CleanupMembership(subscriptionId, subscription);

        _logger?.LogInformation("Unregistered subscription: {Name} ({Id})", subscription.Name, subscriptionId);
        return true;
    }

    #endregion

    #region Event Matching and Scoping Logic

    /// <summary>
    /// Determine if a subscription should receive an event based on scoping rules
    /// </summary>
    private bool ShouldReceiveEvent(EventSubscription subscription, string eventName,
        UnifiedEventScope? forcedScope = null, string? targetChannel = null, string? targetRole = null)
    {
        var effectiveScope = forcedScope ?? subscription.Scope;

        // Apply event filters if configured
        if (subscription.EventFilters.Count > 0 && !subscription.EventFilters.Contains(eventName))
        {
            return false;
        }

        return effectiveScope switch
        {
            UnifiedEventScope.Global => true,
            
            UnifiedEventScope.Agent => false, // Agent events handled separately via specific targeting
            
            UnifiedEventScope.Channel => targetChannel == null || subscription.Channels.Contains(targetChannel),
            
            UnifiedEventScope.Role => targetRole == null || subscription.Role == targetRole,
            
            UnifiedEventScope.Namespace => IsNamespaceMatch(eventName, subscription.NamespacePrefix),
            
            _ => true
        };
    }

    /// <summary>
    /// Get handlers that match the event name for a specific subscription
    /// </summary>
    private List<EventHandler> GetMatchingHandlers(string subscriptionId, string eventName, EventSubscription subscription)
    {
        var matchingHandlers = new List<EventHandler>();
        
        if (!_handlers.TryGetValue(subscriptionId, out var subscriptionHandlers))
        {
            return matchingHandlers;
        }

        foreach (var handlerEntry in subscriptionHandlers)
        {
            var pattern = handlerEntry.Key;
            var handlers = handlerEntry.Value;

            if (IsEventMatch(eventName, pattern, subscription))
            {
                matchingHandlers.AddRange(handlers);
                _logger?.LogTrace("Pattern {Pattern} matches event {EventName} for subscription {Name}", 
                    pattern, eventName, subscription.Name);
            }
        }

        return matchingHandlers;
    }

    /// <summary>
    /// Check if an event matches a pattern for a specific subscription
    /// </summary>
    private bool IsEventMatch(string eventName, string pattern, EventSubscription subscription)
    {
        // Exact match
        if (eventName == pattern)
        {
            return true;
        }

        // Handle .any. wildcard patterns for ALL scopes (not just namespace scope)
        if (pattern.Contains(".any."))
        {
            return IsNamespacePatternMatch(eventName, pattern);
        }

        // Handle namespace-scoped patterns
        if (subscription.Scope == UnifiedEventScope.Namespace)
        {
            return IsNamespacePatternMatch(eventName, pattern);
        }

        // Handle wildcard patterns
        if (pattern.Contains("*"))
        {
            return IsWildcardMatch(eventName, pattern);
        }

        return false;
    }

    /// <summary>
    /// Check namespace pattern matching (e.g., "global.*", "agent.alice.*")
    /// </summary>
    private bool IsNamespacePatternMatch(string eventName, string pattern)
    {
        if (pattern.EndsWith("*"))
        {
            var prefix = pattern[..^1]; // Remove trailing *
            return eventName.StartsWith(prefix);
        }

        if (pattern.Contains(".any."))
        {
            var parts = eventName.Split('.');
            var patternParts = pattern.Split('.');
            
            if (parts.Length != patternParts.Length)
            {
                return false;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (patternParts[i] == "any")
                {
                    continue;
                }
                
                if (parts[i] != patternParts[i])
                {
                    return false;
                }
            }
            
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if event matches namespace prefix
    /// </summary>
    private bool IsNamespaceMatch(string eventName, string namespacePrefix)
    {
        if (string.IsNullOrEmpty(namespacePrefix))
        {
            return true;
        }

        return eventName.StartsWith(namespacePrefix);
    }

    /// <summary>
    /// Basic wildcard matching
    /// </summary>
    private bool IsWildcardMatch(string eventName, string pattern)
    {
        if (pattern == "*")
        {
            return true;
        }

        if (pattern.EndsWith("*"))
        {
            var prefix = pattern[..^1];
            return eventName.StartsWith(prefix);
        }

        if (pattern.StartsWith("*"))
        {
            var suffix = pattern[1..];
            return eventName.EndsWith(suffix);
        }

        return false;
    }

    /// <summary>
    /// Execute handler with subscription context
    /// </summary>
    private async Task ExecuteHandlerWithContext(EventHandler handler, EventPayload payload, EventSubscription subscription)
    {
        try
        {
            // Add subscription context to payload
            var contextualPayload = new EventPayload
            {
                EventName = payload.EventName,
                Data = payload.Data,
                Timestamp = payload.Timestamp,
                Source = $"{payload.Source}→{subscription.Name}",
                TargetScope = subscription.Scope.ToString()
            };

            await handler(contextualPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Handler execution failed for subscription {Name}: {EventName}", 
                subscription.Name, payload.EventName);
            throw;
        }
    }

    #endregion

    #region Membership Management

    /// <summary>
    /// Update role and channel membership tracking
    /// </summary>
    private void UpdateMembership(string subscriptionId, EventSubscription subscription)
    {
        // Track role membership
        if (!_roleMembers.ContainsKey(subscription.Role))
        {
            _roleMembers[subscription.Role] = new HashSet<string>();
        }
        _roleMembers[subscription.Role].Add(subscriptionId);

        // Track channel membership
        foreach (var channel in subscription.Channels)
        {
            if (!_channelMembers.ContainsKey(channel))
            {
                _channelMembers[channel] = new HashSet<string>();
            }
            _channelMembers[channel].Add(subscriptionId);
        }
    }

    /// <summary>
    /// Clean up membership tracking when subscription is removed
    /// </summary>
    private void CleanupMembership(string subscriptionId, EventSubscription subscription)
    {
        // Clean up role membership
        if (_roleMembers.TryGetValue(subscription.Role, out var roleMembers))
        {
            roleMembers.Remove(subscriptionId);
            if (roleMembers.Count == 0)
            {
                _roleMembers.TryRemove(subscription.Role, out _);
            }
        }

        // Clean up channel membership
        foreach (var channel in subscription.Channels)
        {
            if (_channelMembers.TryGetValue(channel, out var channelMembers))
            {
                channelMembers.Remove(subscriptionId);
                if (channelMembers.Count == 0)
                {
                    _channelMembers.TryRemove(channel, out _);
                }
            }
        }
    }

    #endregion

    #region Statistics and Information

    /// <summary>
    /// Get comprehensive event bus statistics
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        var stats = new Dictionary<string, object>
        {
            ["TotalSubscriptions"] = _subscriptions.Count(s => s.Value.IsActive),
            ["TotalEventTypes"] = _icxHandlers.Keys.Union(
                _handlers.Values.SelectMany(h => h.Keys)
            ).Distinct().Count(),
            ["TotalHandlers"] = _icxHandlers.Values.Sum(h => h.Count) + 
                               _handlers.Values.Sum(sh => sh.Values.Sum(h => h.Count)),
            ["SubscriptionsByScope"] = _subscriptions.Values
                .Where(s => s.IsActive)
                .GroupBy(s => s.Scope.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ["SubscriptionsByRole"] = _subscriptions.Values
                .Where(s => s.IsActive)
                .GroupBy(s => s.Role)
                .ToDictionary(g => g.Key, g => g.Count()),
            ["LastStatisticsUpdate"] = DateTime.UtcNow
        };

        return stats;
    }

    /// <summary>
    /// Get all active subscriptions
    /// </summary>
    public IEnumerable<EventSubscription> GetActiveSubscriptions()
    {
        return _subscriptions.Values.Where(s => s.IsActive).ToArray();
    }

    /// <summary>
    /// Get handler count for an event (for debugging)
    /// </summary>
    public int GetHandlerCount(string eventName)
    {
        var count = 0;
        foreach (var handlerDict in _handlers.Values)
        {
            if (handlerDict.TryGetValue(eventName, out var handlers))
            {
                count += handlers.Count;
            }
        }
        return count;
    }

    /// <summary>
    /// Clear all registrations (for testing)
    /// </summary>
    public void Clear()
    {
        _subscriptions.Clear();
        _handlers.Clear();
        _icxHandlers.Clear();
        _channelMembers.Clear();
        _roleMembers.Clear();
        _logger?.LogInformation("Unified Event Bus cleared - all registrations removed");
    }

    /// <summary>
    /// Reset all registrations (alias for Clear)
    /// </summary>
    public void Reset() => Clear();

    #endregion
}

/// <summary>
/// Global registry for the Unified Event Bus
/// </summary>
public static class UnifiedEventBusRegistry
{
    private static UnifiedEventBus? _instance;
    private static readonly object _lock = new object();

    /// <summary>
    /// Get or create the global Unified Event Bus instance
    /// </summary>
    public static UnifiedEventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new UnifiedEventBus();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Reset the global instance (for testing)
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _instance?.Reset();
            _instance = null;
        }
    }
}
