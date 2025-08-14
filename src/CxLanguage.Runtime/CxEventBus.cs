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
        private readonly ConcurrentDictionary<string, List<Func<object?, string, IDictionary<string, object>?, Task<bool>>>> _eventHandlers;
        private readonly ILogger<CxEventBus> _logger;

        public CxEventBus(ILogger<CxEventBus> logger)
        {
            _eventHandlers = new ConcurrentDictionary<string, List<Func<object?, string, IDictionary<string, object>?, Task<bool>>>>();
            _logger = logger;
        }

        /// <summary>
        /// Emit an event with data payload
        /// </summary>
        public async Task<bool> EmitAsync(string eventName, IDictionary<string, object>? payload = null, object? sender = null)
        {
            _logger.LogDebug("Emitting event: {EventName}", eventName);
            var processed = false;

            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                var tasks = new List<Task<bool>>();
                
                foreach (var handler in handlers.ToArray())
                {
                    try
                    {
                        tasks.Add(handler(sender, eventName, payload));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing handler for event {EventName}", eventName);
                    }
                }
                
                var results = await Task.WhenAll(tasks);
                processed = results.Any(r => r);
            }
            return processed;
        }

        /// <summary>
        /// Subscribe to an event with a handler function
        /// </summary>
        public bool Subscribe(string eventPattern, Func<object?, string, IDictionary<string, object>?, Task<bool>> handler)
        {
            try
            {
                _eventHandlers.AddOrUpdate(eventPattern,
                    new List<Func<object?, string, IDictionary<string, object>?, Task<bool>>> { handler },
                    (key, existingHandlers) =>
                    {
                        existingHandlers.Add(handler);
                        return existingHandlers;
                    });
                _logger.LogDebug("Subscribed to event pattern: {EventPattern}", eventPattern);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to event pattern: {EventPattern}", eventPattern);
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        public bool Unsubscribe(string eventPattern, Func<object?, string, IDictionary<string, object>?, Task<bool>> handler)
        {
            try
            {
                if (_eventHandlers.TryGetValue(eventPattern, out var handlers))
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                    {
                        _eventHandlers.TryRemove(eventPattern, out _);
                    }
                    _logger.LogDebug("Unsubscribed from event pattern: {EventPattern}", eventPattern);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unsubscribe from event pattern: {EventPattern}", eventPattern);
                return false;
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
