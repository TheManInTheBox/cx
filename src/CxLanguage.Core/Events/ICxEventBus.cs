using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.Core.Events
{
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
        void Subscribe(string eventName, CxEventHandler handler);
        
        /// <summary>
        /// Unsubscribe from an event in the CX event system
        /// </summary>
        /// <param name="eventName">The event name to unsubscribe from</param>
        /// <param name="handler">The handler function to remove</param>
        void Unsubscribe(string eventName, CxEventHandler handler);
        
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
}
