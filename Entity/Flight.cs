using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Entity
{
    public enum FlightType { Landing, Takeoff }
    public enum FlightState { Waiting, Operating, Completed }

    public class Flight(string code, FlightType type)
    {
        public string Code { get; set; } = code;
        public FlightType Type { get; set; } = type;
        public FlightState State { get; set; } = FlightState.Waiting;
        public DateTime ScheduledTime { get; set; } = default;

        public event Action<Runway, Flight>? OnActionCompleted;
        public event Func<Flight, Task>? OnRequestConfirmation;

        public async Task ExecuteActionAsync(Runway runway, float duration)
        {
            string action = (Type == FlightType.Landing) ? "landing" : "take off";
            Console.WriteLine($"[{Code}] Confirm {action} in runway number {runway.id}");

            await OnRequestConfirmation.Invoke(this);
            Console.WriteLine($"[{Code}] Confirmed. Start {action}.");

            await Task.Delay((int)duration * 1000);

            Console.WriteLine($"[{Code}] Completed {runway.id}");
            OnActionCompleted?.Invoke(runway, this);
        }
    }
}
