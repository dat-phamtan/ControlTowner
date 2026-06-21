using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Entity
{
    public enum FlightType { Landing, Takeoff }
    public enum FlightState { Waiting, Operating, Completed }

    public class Flight(string code, FlightType type, ILogger logger)
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
            logger?.Log($"[{Code}] Confirm {action} in runway number {runway.id}");

            await OnRequestConfirmation.Invoke(this);
            logger?.Log($"[{Code}] Confirmed. Start {action}.");

            await Task.Delay((int)duration * 1000);

            logger?.Log($"[{Code}] Completed {runway.id}");
            OnActionCompleted?.Invoke(runway, this);
        }
    }
}
