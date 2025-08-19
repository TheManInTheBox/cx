using System;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.NeuroHub
{
    /// <summary>
    /// An example of a Ganglionic Pod that handles temperature alerts.
    /// This represents a "spinal reflex" for immediate action.
    /// </summary>
    public class TemperatureAlertPod : GanglionicPod
    {
        public TemperatureAlertPod(NeuroHub hub) : base(hub)
        {
        }

        protected override void InitializeReflexes()
        {
            // This pod is interested in all temperature readings.
            CentralNervousSystem.Subscribe("sensor.temperature.*", HandleTemperatureReading);
        }

        private async Task HandleTemperatureReading(NeuroEvent @event)
        {
            if (@event.Payload is double temperature)
            {
                Console.WriteLine($"[Reflex] Temperature reading received: {temperature}°C on topic {@event.Topic}");

                if (temperature > 30.0)
                {
                    // High temperature detected, trigger a high-priority alert.
                    var alertEvent = new NeuroEvent(
                        "alert.temperature.high",
                        new { Reading = temperature, Sensor = @event.Topic },
                        EventPriority.High
                    );
                    
                    Console.WriteLine($"[Reflex] High temperature detected! Emitting high-priority alert.");
                    await Emerge(alertEvent);
                }
            }
        }
    }
}

