using System;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.NeuroHub
{
    /// <summary>
    /// Represents a local compute pod for handling spinal reflexes.
    /// These pods can subscribe to the main NeuroHub and execute immediate, predefined actions.
    /// </summary>
    public abstract class GanglionicPod
    {
        protected readonly NeuroHub CentralNervousSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="GanglionicPod"/> class.
        /// </summary>
        /// <param name="hub">The central NeuroHub to connect to.</param>
        protected GanglionicPod(NeuroHub hub)
        {
            CentralNervousSystem = hub;
            InitializeReflexes();
        }

        /// <summary>
        /// Subclasses must implement this method to define their reflex actions
        /// by subscribing to specific topics on the central hub.
        /// </summary>
        protected abstract void InitializeReflexes();

        /// <summary>
        /// A helper method to publish an event back to the central nervous system.
        /// </summary>
        /// <param name="event">The event to publish.</param>
        protected Task Emerge(NeuroEvent @event)
        {
            return CentralNervousSystem.PublishAsync(@event);
        }
    }
}
