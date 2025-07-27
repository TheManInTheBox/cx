using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime;

/// <summary>
/// Event Bus Scoping Strategies for CX Autonomous Agents
/// </summary>
public enum EventBusScope
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
    /// Hierarchical scope - events bubble up/down agent hierarchy
    /// </summary>
    Hierarchy
}

/// <summary>
/// Agent Registration for Event Bus Subscription
/// </summary>
public class AgentSubscription
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public EventBusScope Scope { get; set; } = EventBusScope.Global;
    public HashSet<string> Channels { get; set; } = new();
    public HashSet<string> EventFilters { get; set; } = new();
    public object? AgentInstance { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Event Bus Service - Advanced Pub/Sub system for CX Agents
/// Provides multiple scoping strategies and agent lifecycle management
/// </summary>
public class EventBusService
{
    private readonly ConcurrentDictionary<string, AgentSubscription> _agents = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<CxEventHandler>>> _scopedHandlers = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _channelMembers = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _roleMembers = new();
    private readonly ILogger<EventBusService>? _logger;
    private readonly object _lock = new object();

    public EventBusService(ILogger<EventBusService>? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("Event Bus Service initialized with advanced scoping");
    }

    #region Agent Lifecycle Management

    /// <summary>
    /// Register an agent with the Event Bus Service
    /// </summary>
    public string JoinBus(string agentName, string role = "agent", EventBusScope scope = EventBusScope.Global, 
        string[]? channels = null, string[]? eventFilters = null, object? agentInstance = null)
    {
        var agentId = Guid.NewGuid().ToString("N")[..8]; // Short unique ID
        
        var subscription = new AgentSubscription
        {
            AgentId = agentId,
            AgentName = agentName,
            Role = role,
            Scope = scope,
            Channels = channels?.ToHashSet() ?? new HashSet<string>(),
            EventFilters = eventFilters?.ToHashSet() ?? new HashSet<string>(),
            AgentInstance = agentInstance,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _agents[agentId] = subscription;

        // Track role membership
        if (!_roleMembers.ContainsKey(role))
        {
            _roleMembers[role] = new HashSet<string>();
        }
        _roleMembers[role].Add(agentId);

        // Track channel membership
        foreach (var channel in subscription.Channels)
        {
            if (!_channelMembers.ContainsKey(channel))
            {
                _channelMembers[channel] = new HashSet<string>();
            }
            _channelMembers[channel].Add(agentId);
        }

        _logger?.LogInformation("Agent {AgentName} ({AgentId}) joined bus with {Scope} scope, role: {Role}", 
            agentName, agentId, scope, role);

        return agentId;
    }

    /// <summary>
    /// Remove an agent from the Event Bus Service
    /// </summary>
    public bool LeaveBus(string agentId)
    {
        if (!_agents.TryRemove(agentId, out var subscription))
        {
            return false;
        }

        // Clean up role membership
        if (_roleMembers.TryGetValue(subscription.Role, out var roleMembers))
        {
            roleMembers.Remove(agentId);
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
                channelMembers.Remove(agentId);
                if (channelMembers.Count == 0)
                {
                    _channelMembers.TryRemove(channel, out _);
                }
            }
        }

        // Remove scoped handlers
        _scopedHandlers.TryRemove(agentId, out _);

        _logger?.LogInformation("Agent {AgentName} ({AgentId}) left the bus", subscription.AgentName, agentId);
        return true;
    }

    /// <summary>
    /// Join a channel (for Channel-scoped agents)
    /// </summary>
    public bool JoinChannel(string agentId, string channel)
    {
        if (!_agents.TryGetValue(agentId, out var subscription))
        {
            return false;
        }

        subscription.Channels.Add(channel);
        
        if (!_channelMembers.ContainsKey(channel))
        {
            _channelMembers[channel] = new HashSet<string>();
        }
        _channelMembers[channel].Add(agentId);

        _logger?.LogDebug("Agent {AgentName} joined channel: {Channel}", subscription.AgentName, channel);
        return true;
    }

    /// <summary>
    /// Leave a channel (for Channel-scoped agents)
    /// </summary>
    public bool LeaveChannel(string agentId, string channel)
    {
        if (!_agents.TryGetValue(agentId, out var subscription))
        {
            return false;
        }

        subscription.Channels.Remove(channel);

        if (_channelMembers.TryGetValue(channel, out var channelMembers))
        {
            channelMembers.Remove(agentId);
            if (channelMembers.Count == 0)
            {
                _channelMembers.TryRemove(channel, out _);
            }
        }

        _logger?.LogDebug("Agent {AgentName} left channel: {Channel}", subscription.AgentName, channel);
        return true;
    }

    #endregion

    #region Event Handler Registration

    /// <summary>
    /// Subscribe an agent to an event with scoped handling
    /// </summary>
    public bool Subscribe(string agentId, string eventName, CxEventHandler handler)
    {
        if (!_agents.TryGetValue(agentId, out var subscription))
        {
            _logger?.LogWarning("Cannot subscribe - Agent {AgentId} not registered", agentId);
            return false;
        }

        // Create scoped handler registry for this agent
        if (!_scopedHandlers.ContainsKey(agentId))
        {
            _scopedHandlers[agentId] = new ConcurrentDictionary<string, List<CxEventHandler>>();
        }

        var agentHandlers = _scopedHandlers[agentId];
        if (!agentHandlers.ContainsKey(eventName))
        {
            agentHandlers[eventName] = new List<CxEventHandler>();
        }

        agentHandlers[eventName].Add(handler);
        
        _logger?.LogDebug("Agent {AgentName} subscribed to event: {EventName}", subscription.AgentName, eventName);
        return true;
    }

    /// <summary>
    /// Unsubscribe an agent from an event
    /// </summary>
    public bool Unsubscribe(string agentId, string eventName, CxEventHandler handler)
    {
        if (!_scopedHandlers.TryGetValue(agentId, out var agentHandlers))
        {
            return false;
        }

        if (!agentHandlers.TryGetValue(eventName, out var handlers))
        {
            return false;
        }

        var removed = handlers.Remove(handler);
        if (handlers.Count == 0)
        {
            agentHandlers.TryRemove(eventName, out _);
        }

        if (_agents.TryGetValue(agentId, out var subscription))
        {
            _logger?.LogDebug("Agent {AgentName} unsubscribed from event: {EventName}", subscription.AgentName, eventName);
        }

        return removed;
    }

    #endregion

    #region Event Emission with Scoping

    /// <summary>
    /// Emit an event with intelligent scoping based on agent configurations
    /// </summary>
    public async Task EmitAsync(string eventName, object? data = null, string source = "System", 
        EventBusScope? forcedScope = null, string? targetChannel = null, string? targetRole = null)
    {
        var payload = new CxEventPayload
        {
            EventName = eventName,
            Data = data,
            Timestamp = DateTime.UtcNow,
            Source = source
        };

        _logger?.LogDebug("Emitting event: {EventName} from {Source} with scope strategy", eventName, source);

        var tasks = new List<Task>();
        
        foreach (var agentKvp in _agents.Where(a => a.Value.IsActive))
        {
            var agentId = agentKvp.Key;
            var subscription = agentKvp.Value;

            // Apply scoping logic
            if (!ShouldReceiveEvent(subscription, eventName, forcedScope, targetChannel, targetRole))
            {
                continue;
            }

            // Get handlers for this agent
            if (_scopedHandlers.TryGetValue(agentId, out var agentHandlers) && 
                agentHandlers.TryGetValue(eventName, out var handlers))
            {
                foreach (var handler in handlers.ToArray())
                {
                    try
                    {
                        tasks.Add(ExecuteHandlerWithContext(handler, payload, subscription));
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error executing handler for agent {AgentName}", subscription.AgentName);
                    }
                }
            }
        }

        // Execute all handlers concurrently
        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
                _logger?.LogDebug("Event {EventName} delivered to {HandlerCount} handlers", eventName, tasks.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing event handlers for: {EventName}", eventName);
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
    public void Emit(string eventName, object? data = null, string source = "System",
        EventBusScope? forcedScope = null, string? targetChannel = null, string? targetRole = null)
    {
        Task.Run(async () => await EmitAsync(eventName, data, source, forcedScope, targetChannel, targetRole));
    }

    #endregion

    #region Scoping Logic

    /// <summary>
    /// Determine if an agent should receive an event based on scoping rules
    /// </summary>
    private bool ShouldReceiveEvent(AgentSubscription subscription, string eventName, 
        EventBusScope? forcedScope = null, string? targetChannel = null, string? targetRole = null)
    {
        var effectiveScope = forcedScope ?? subscription.Scope;

        // Apply event filters if configured
        if (subscription.EventFilters.Count > 0 && !subscription.EventFilters.Contains(eventName))
        {
            return false;
        }

        return effectiveScope switch
        {
            EventBusScope.Global => true, // Global scope receives all events

            EventBusScope.Agent => false, // Agent scope only receives own events (handled separately)

            EventBusScope.Channel => targetChannel == null || subscription.Channels.Contains(targetChannel),

            EventBusScope.Role => targetRole == null || subscription.Role == targetRole,

            EventBusScope.Hierarchy => true, // TODO: Implement hierarchy logic

            _ => true
        };
    }

    /// <summary>
    /// Execute handler with agent context
    /// </summary>
    private async Task ExecuteHandlerWithContext(CxEventHandler handler, CxEventPayload payload, AgentSubscription subscription)
    {
        try
        {
            // Add agent context to payload
            var contextualPayload = new CxEventPayload
            {
                EventName = payload.EventName,
                Data = payload.Data,
                Timestamp = payload.Timestamp,
                Source = $"{payload.Source}→{subscription.AgentName}"
            };

            await handler(contextualPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Handler execution failed for agent {AgentName}: {EventName}", 
                subscription.AgentName, payload.EventName);
            throw;
        }
    }

    #endregion

    #region Service Information and Management

    /// <summary>
    /// Get all registered agents
    /// </summary>
    public IEnumerable<AgentSubscription> GetRegisteredAgents()
    {
        return _agents.Values.Where(a => a.IsActive).ToArray();
    }

    /// <summary>
    /// Get agents by role
    /// </summary>
    public IEnumerable<AgentSubscription> GetAgentsByRole(string role)
    {
        return _agents.Values.Where(a => a.IsActive && a.Role == role).ToArray();
    }

    /// <summary>
    /// Get agents in channel
    /// </summary>
    public IEnumerable<AgentSubscription> GetAgentsInChannel(string channel)
    {
        return _agents.Values.Where(a => a.IsActive && a.Channels.Contains(channel)).ToArray();
    }

    /// <summary>
    /// Get comprehensive bus statistics
    /// </summary>
    public BusStatistics GetBusStatistics()
    {
        return new BusStatistics
        {
            TotalAgents = _agents.Count(a => a.Value.IsActive),
            TotalChannels = _channelMembers.Count,
            TotalRoles = _roleMembers.Count,
            ScopeDistribution = _agents.Values.Where(a => a.IsActive)
                .GroupBy(a => a.Scope)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            TopChannels = _channelMembers.OrderByDescending(c => c.Value.Count)
                .Take(5)
                .ToDictionary(c => c.Key, c => c.Value.Count),
            TopRoles = _roleMembers.OrderByDescending(r => r.Value.Count)
                .Take(5)
                .ToDictionary(r => r.Key, r => r.Value.Count)
        };
    }

    /// <summary>
    /// Clear all agents and handlers (for testing)
    /// </summary>
    public void Reset()
    {
        _agents.Clear();
        _scopedHandlers.Clear();
        _channelMembers.Clear();
        _roleMembers.Clear();
        _logger?.LogInformation("Event Bus Service reset - all agents and handlers cleared");
    }

    #endregion
}

/// <summary>
/// Bus statistics for monitoring and debugging
/// </summary>
public class BusStatistics
{
    public int TotalAgents { get; set; }
    public int TotalChannels { get; set; }
    public int TotalRoles { get; set; }
    public Dictionary<string, int> ScopeDistribution { get; set; } = new();
    public Dictionary<string, int> TopChannels { get; set; } = new();
    public Dictionary<string, int> TopRoles { get; set; } = new();
}

/// <summary>
/// Static Event Bus Service Registry for CX Runtime
/// </summary>
public static class EventBusServiceRegistry
{
    private static EventBusService? _instance;
    private static readonly object _lock = new object();

    /// <summary>
    /// Get or create the global Event Bus Service instance
    /// </summary>
    public static EventBusService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new EventBusService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Reset the Event Bus Service (for testing)
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
