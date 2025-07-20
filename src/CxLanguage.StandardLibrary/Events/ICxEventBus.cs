using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Events
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
        /// Subscribe to events from the CX event system
        /// </summary>
        /// <param name="eventName">The event name to subscribe to</param>
        /// <param name="handler">The handler function to call when event is received</param>
        void Subscribe(string eventName, Func<object, Task> handler);
        
        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        /// <param name="eventName">The event name to unsubscribe from</param>
        void Unsubscribe(string eventName);
    }
    
    /// <summary>
    /// Basic implementation of the CX event bus
    /// </summary>
    public class CxEventBus : ICxEventBus
    {
        private readonly Dictionary<string, List<Func<object, Task>>> _handlers = new();
        
        public async Task EmitAsync(string eventName, object payload)
        {
            if (_handlers.TryGetValue(eventName, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler(payload);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other handlers
                        Console.WriteLine($"Error in event handler for {eventName}: {ex.Message}");
                    }
                }
            }
            
            // For demo purposes, also print to console
            Console.WriteLine($"🔥 CX EVENT EMITTED: {eventName}");
        }
        
        public void Subscribe(string eventName, Func<object, Task> handler)
        {
            if (!_handlers.ContainsKey(eventName))
            {
                _handlers[eventName] = new List<Func<object, Task>>();
            }
            
            _handlers[eventName].Add(handler);
        }
        
        public void Unsubscribe(string eventName)
        {
            _handlers.Remove(eventName);
        }
    }
}
