using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Entity
{
    public enum FlightType { Landing, Takeoff }
    public enum FlightState { Waiting, Operating, Completed }

    public class Flight
    {
        public string Code { get; set; }
        public FlightType Type { get; set; }
        public FlightState State { get; set; } = FlightState.Waiting;
        public DateTime ScheduledTime { get; set; } = default;
        public ILogger logger;
        private FlightType takeoff;

        public event Action<Runway, Flight>? OnActionCompleted;
        public event Func<Flight, Task>? OnRequestConfirmation;


        public Flight(string code, FlightType type, FlightState state, DateTime scheduledTime, ILogger logger)
        {
            Code = code;
            Type = type;
            State = state;
            ScheduledTime = scheduledTime;
            this.logger = logger;
        }

        public async Task ExecuteActionAsync(Runway runway, float duration)
        {
            //State = FlightState.Operating;
            string action = (Type == FlightType.Landing) ? "land" : "take off";
            logger?.Log($"[{Code}] I will {action} in runway: {runway.id}");

            await OnRequestConfirmation.Invoke(this);
            string logAction = (Type == FlightType.Landing) ? "landing" : "taking off";
            logger?.Log($"[{Code}] Confirmed!! Start {logAction}.");

            await Task.Delay((int)duration * 1000);

            State = FlightState.Completed;
            logger?.Log($"[{Code}] Completed {logAction} in runway: {runway.id}");
            OnActionCompleted?.Invoke(runway, this);
        }
    }
}
