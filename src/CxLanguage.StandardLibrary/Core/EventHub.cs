using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Core
{
    /// <summary>
    /// Represents a decentralized event hub for a specific service instance.
    /// Each service will have its own EventHub to manage its own events.
    /// </summary>
    public class EventHub
    {
        private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _handlers = new();
        private readonly ILogger _logger;

        public EventHub(ILogger logger)
        {
            _logger = logger;
            _logger.LogInformation("✨ EventHub instance created.");
        }

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="handler">The handler to execute when the event is emitted.</param>
        public void Subscribe(string eventName, Func<object, Task> handler)
        {
            var eventHandlers = _handlers.GetOrAdd(eventName, new List<Func<object, Task>>());
            lock (eventHandlers)
            {
                eventHandlers.Add(handler);
            }
            _logger.LogDebug("Subscribed to event: {EventName}", eventName);
        }

        /// <summary>
        /// Emits an event, invoking all subscribed handlers.
        /// </summary>
        /// <param name="eventName">The name of the event to emit.</param>
        /// <param name="payload">The data to pass to the event handlers.</param>
        public async Task EmitAsync(string eventName, object payload)
        {
            _logger.LogInformation("🚀 Emitting event: {EventName}", eventName);
            if (_handlers.TryGetValue(eventName, out var eventHandlers))
            {
                List<Func<object, Task>> handlersToInvoke;
                lock (eventHandlers)
                {
                    handlersToInvoke = new List<Func<object, Task>>(eventHandlers);
                }

                foreach (var handler in handlersToInvoke)
                {
                    try
                    {
                        await handler(payload);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error executing handler for event: {EventName}", eventName);
                    }
                }
            }
        }
    }
}
