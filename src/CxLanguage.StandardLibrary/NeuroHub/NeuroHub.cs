using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.NeuroHub
{
    /// <summary>
    /// Represents a single "neuron" or event handler subscription.
    /// </summary>
    public interface ISubscription
    {
        /// <summary>
        /// Unsubscribes the handler from the event hub.
        /// </summary>
        void Unsubscribe();
    }

    /// <summary>
    /// The central event hub, analogous to the Central Nervous System.
    /// It manages subscriptions and routes events.
    /// </summary>
    public class NeuroHub
    {
        private readonly ConcurrentDictionary<string, List<Func<NeuroEvent, Task>>> _subscriptions = new();
        private readonly ConcurrentDictionary<string, List<Func<NeuroEvent, Task>>> _wildcardSubscriptions = new();

        /// <summary>
        /// Subscribes an event handler to a specific topic.
        /// </summary>
        /// <param name="topic">The topic to subscribe to (e.g., "sensor.temperature.reading"). 
        /// Can include wildcards (*).</param>
        /// <param name="handler">The asynchronous event handler.</param>
        /// <returns>An ISubscription object to manage the subscription.</returns>
        public ISubscription Subscribe(string topic, Func<NeuroEvent, Task> handler)
        {
            var topicList = topic.Contains('*') ? _wildcardSubscriptions : _subscriptions;
            
            topicList.AddOrUpdate(topic, 
                new List<Func<NeuroEvent, Task>> { handler }, 
                (key, list) => { list.Add(handler); return list; });

            return new Subscription(this, topic, handler);
        }

        /// <summary>
        /// Publishes an event to the hub, which then routes it to all relevant subscribers.
        /// </summary>
        /// <param name="event">The event to publish.</param>
        public async Task PublishAsync(NeuroEvent @event)
        {
            // Direct subscribers
            if (_subscriptions.TryGetValue(@event.Topic, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    await handler(@event);
                }
            }

            // Wildcard subscribers
            foreach (var subscription in _wildcardSubscriptions)
            {
                if (IsMatch(@event.Topic, subscription.Key))
                {
                    foreach (var handler in subscription.Value)
                    {
                        await handler(@event);
                    }
                }
            }
        }

        private void Unsubscribe(string topic, Func<NeuroEvent, Task> handler)
        {
            var topicList = topic.Contains('*') ? _wildcardSubscriptions : _subscriptions;

            if (topicList.TryGetValue(topic, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        private static bool IsMatch(string topic, string pattern)
        {
            // Simple wildcard matching, can be expanded to be more sophisticated
            var patternParts = pattern.Split('*');
            if (patternParts.Length == 1)
            {
                return topic == pattern;
            }

            var lastIndex = -1;
            foreach (var part in patternParts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                var currentIndex = topic.IndexOf(part, lastIndex + 1, StringComparison.Ordinal);
                if (currentIndex == -1) return false;

                lastIndex = currentIndex;
            }
            return true;
        }

        private class Subscription : ISubscription
        {
            private readonly NeuroHub _hub;
            private readonly string _topic;
            private readonly Func<NeuroEvent, Task> _handler;

            public Subscription(NeuroHub hub, string topic, Func<NeuroEvent, Task> handler)
            {
                _hub = hub;
                _topic = topic;
                _handler = handler;
            }

            public void Unsubscribe()
            {
                _hub.Unsubscribe(_topic, _handler);
            }
        }
    }
}

