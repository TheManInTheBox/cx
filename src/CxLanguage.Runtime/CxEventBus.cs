using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CxLanguage.Core.Events;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Core Event Bus for CX autonomous programming language
    /// Enables agent-to-agent communication through event-driven architecture
    /// </summary>
    public class CxEventBus : ICxEventBus
    {
        private readonly ConcurrentDictionary<string, List<CxEventHandler>> _eventHandlers;
        private readonly ILogger<CxEventBus> _logger;

        public CxEventBus(ILogger<CxEventBus> logger)
        {
            _eventHandlers = new ConcurrentDictionary<string, List<CxEventHandler>>();
            _logger = logger;
        }

        /// <summary>
        /// Emit an event with data payload
        /// </summary>
        public async Task EmitAsync(string eventName, object payload)
        {
            _logger.LogDebug("Emitting event: {EventName}", eventName);

            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                var eventPayload = new CxEventPayload
                {
                    EventName = eventName,
                    Data = payload,
                    Timestamp = DateTime.UtcNow,
                    Source = "CxEventBus"
                };

                var tasks = new List<Task>();
                
                foreach (var handler in handlers.ToArray())
                {
                    try
                    {
                        tasks.Add(handler(eventPayload));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing handler for event {EventName}", eventName);
                    }
                }
                
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Subscribe to an event with a handler function
        /// </summary>
        public void Subscribe(string eventName, CxEventHandler handler)
        {
            var handlers = _eventHandlers.GetOrAdd(eventName, _ => new List<CxEventHandler>());
            lock (handlers)
            {
                handlers.Add(handler);
            }
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        public void Unsubscribe(string eventName, CxEventHandler handler)
        {
            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                lock (handlers)
                {
                    handlers.Remove(handler);
                }
            }
        }

        /// <summary>
        /// Get statistics about the event bus
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "HandlerCount", _eventHandlers.Count },
                { "TotalSubscriptions", _eventHandlers.Values.SelectMany(v => v).Count() }
            };
        }

        /// <summary>
        /// Clear all event handlers
        /// </summary>
        public void Clear()
        {
            _eventHandlers.Clear();
        }

        /// <summary>
        /// Get handler count for an event
        /// </summary>
        public int GetHandlerCount(string eventName)
        {
            return _eventHandlers.TryGetValue(eventName, out var handlers) ? handlers.Count : 0;
        }
    }
}
