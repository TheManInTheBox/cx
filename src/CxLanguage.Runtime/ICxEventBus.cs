using System;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Interface for the CX event bus supporting structured event objects.
    /// Implements issue #178 - Event Handlers Should Receive Literal CX Object as Parameter.
    /// </summary>
    public interface ICxEventBus
    {
        /// <summary>
        /// Subscribe to events with a global handler that receives CxEvent objects.
        /// </summary>
        void Subscribe(string eventName, Action<CxEvent> handler);
        
        /// <summary>
        /// Subscribe to events with an instance-scoped handler that receives CxEvent objects.
        /// </summary>
        void Subscribe(string eventName, object instance, Action<CxEvent> handler);
        
        /// <summary>
        /// Emit an event with a payload that will be wrapped in a CxEvent object.
        /// </summary>
        void Emit(string eventName, object payload);
    }
}
