using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CxLanguage.Core.Events
{
    /// <summary>
    /// Event bus interface for CX Language event system integration
    /// </summary>
    public interface ICxEventBus
    {
        /// <summary>
        /// Emit an event to the CX event system
        /// </summary>
        /// <param name="eventName">The event name (e.g., "realtime.session.started")</param>
        /// <param name="payload">The event payload object</param>
        Task EmitAsync(string eventName, object payload);
        
        /// <summary>
        /// Subscribe to an event in the CX event system
        /// </summary>
        /// <param name="eventName">The event name to subscribe to</param>
        /// <param name="handler">The handler function to call when the event is emitted</param>
        void Subscribe(string eventName, Func<object, Task> handler);
        
        /// <summary>
        /// Unsubscribe from an event in the CX event system
        /// </summary>
        /// <param name="eventName">The event name to unsubscribe from</param>
        void Unsubscribe(string eventName);
        
        /// <summary>
        /// Get statistics about the event bus
        /// </summary>
        /// <returns>Dictionary containing event statistics</returns>
        Dictionary<string, object> GetStatistics();
        
        /// <summary>
        /// Clear all event handlers and statistics
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Get the count of handlers for a specific event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <returns>Number of handlers registered for the event</returns>
        int GetHandlerCount(string eventName);
    }
    
    /// <summary>
    /// Statistics data for the event bus
    /// </summary>
    public class EventBusStatistics
    {
        public int TotalEventTypes { get; set; }
        public int TotalHandlers { get; set; }
        public int TotalEventsEmitted { get; set; }
        public Dictionary<string, int> EventCounts { get; set; } = new();
        public Dictionary<string, int> SubscriptionsByScope { get; set; } = new();
        public Dictionary<string, int> SubscriptionsByChannel { get; set; } = new();
        public Dictionary<string, int> SubscriptionsByRole { get; set; } = new();
        public Dictionary<string, int> TopEventPatterns { get; set; } = new();
        public DateTime LastEventEmitted { get; set; }
        public DateTime LastStatisticsUpdate { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Event payload wrapper for CX event system
    /// </summary>
    public class CxEventPayload
    {
        public object Data { get; set; } = new();
        public string EventName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = "ICxEventBus";
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        public CxEventPayload() { }
        
        public CxEventPayload(object data, string eventName, string source = "ICxEventBus")
        {
            Data = data;
            EventName = eventName;
            Source = source;
        }
    }
    
    /// <summary>
    /// Event handler delegate for CX events  
    /// </summary>
    public delegate Task CxEventHandler(CxEventPayload payload);
    
    /// <summary>
    /// Standard event handler delegate
    /// </summary>
    public delegate Task EventHandler(object payload);
    
    /// <summary>
    /// Event registration result
    /// </summary>
    public class EventRegistrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SubscriptionId { get; set; }
        public int HandlerCount { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Event emission result
    /// </summary>
    public class EventEmissionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int HandlersNotified { get; set; }
        public List<string> Errors { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
        public DateTime EmittedAt { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Event scoping enumeration for event bus systems
    /// </summary>
    public enum CxEventScope
    {
        /// <summary>
        /// Global scope - events available to all subscribers
        /// </summary>
        Global,
        
        /// <summary>
        /// Agent scope - events scoped to specific agent instances
        /// </summary>
        Agent,
        
        /// <summary>
        /// Channel scope - events scoped to specific channels
        /// </summary>
        Channel,
        
        /// <summary>
        /// Role scope - events scoped to specific roles
        /// </summary>
        Role,
        
        /// <summary>
        /// Namespace scope - events scoped to specific namespaces
        /// </summary>
        Namespace
    }
    
    /// <summary>
    /// Event subscription information
    /// </summary>
    public class CxEventSubscription
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventName { get; set; } = string.Empty;
        public string EventPattern { get; set; } = string.Empty;
        public bool IsPattern { get; set; }
        public CxEventScope Scope { get; set; } = CxEventScope.Global;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "agent";
        public List<string> Channels { get; set; } = new();
        public string NamespacePrefix { get; set; } = string.Empty;
        public object? Instance { get; set; }
        public Func<object, Task> Handler { get; set; } = _ => Task.CompletedTask;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CallCount { get; set; }
        public DateTime LastCalledAt { get; set; }
    }
}
