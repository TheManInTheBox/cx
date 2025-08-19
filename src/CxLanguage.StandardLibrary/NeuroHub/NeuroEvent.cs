using System;
using System.Collections.Generic;

namespace CxLanguage.StandardLibrary.NeuroHub
{
    /// <summary>
    /// Represents the priority of a neuro-event, analogous to neurotransmitter types.
    /// </summary>
    public enum EventPriority
    {
        /// <summary>
        /// High-value alerts, critical system messages (Dopamine).
        /// </summary>
        High,
        /// <summary>
        /// Standard operational events.
        /// </summary>
        Standard,
        /// <summary>
        /// Bulk telemetry, low-priority logs.
        /// </summary>
        Bulk
    }

    /// <summary>
    /// Base class for all events flowing through the NeuroHub.
    /// Analogous to an action potential.
    /// </summary>
    public class NeuroEvent
    {
        /// <summary>
        /// Unique identifier for the event.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Timestamp of when the event was created.
        /// </summary>
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The type or topic of the event (e.g., "sensor.temperature.reading").
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Priority of the event, guiding routing and processing.
        /// </summary>
        public EventPriority Priority { get; }

        /// <summary>
        /// The data payload of the event.
        /// </summary>
        public object Payload { get; }

        /// <summary>
        /// Optional vector embedding for semantic routing.
        /// </summary>
        public float[]? Vector { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuroEvent"/> class.
        /// </summary>
        /// <param name="topic">The event topic.</param>
        /// <param name="payload">The event data.</param>
        /// <param name="priority">The event priority.</param>
        public NeuroEvent(string topic, object payload, EventPriority priority = EventPriority.Standard)
        {
            Topic = topic;
            Payload = payload;
            Priority = priority;
        }
    }
}

