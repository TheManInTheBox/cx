using System;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Represents a structured event object passed to all CX event handlers.
    /// Replaces raw payload passing with a structured object containing event metadata.
    /// </summary>
    public class CxEvent
    {
        /// <summary>
        /// The full name of the event (e.g., "user.message.sent").
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// The data payload associated with the event.
        /// </summary>
        public object payload { get; set; } = new object();

        /// <summary>
        /// The UTC timestamp when the event was emitted.
        /// </summary>
        public DateTime timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Constructor for creating a CxEvent with all properties.
        /// </summary>
        public CxEvent(string eventName, object eventPayload)
        {
            name = eventName;
            payload = eventPayload ?? new object();
            timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Default constructor for CxEvent.
        /// </summary>
        public CxEvent()
        {
        }
    }
}

