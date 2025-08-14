using System;
using System.Collections.Generic;

namespace CxLanguage.Core.Events
{
    /// <summary>
    /// Payload class for events in the CX event system
    /// </summary>
    public class CxEventPayload
    {
        /// <summary>
        /// Gets the event name
        /// </summary>
        public string EventName { get; }
        
        /// <summary>
        /// Gets the event data
        /// </summary>
        public object Data { get; }
        
        /// <summary>
        /// Gets the timestamp when the event was created
        /// </summary>
        public DateTime Timestamp { get; }
        
        /// <summary>
        /// Initializes a new instance of the CxEventPayload class
        /// </summary>
        public CxEventPayload(string eventName, object data)
        {
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            Data = data;
            Timestamp = DateTime.UtcNow;
        }
    }
}
