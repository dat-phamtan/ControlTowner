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

        public event Action<Flight> OnActionCompleted;

        public async Task ExecuteActionAsync(int runwayId, float duration)
        {
            //logger
            await Task.Delay((int)duration);
            OnActionCompleted?.Invoke(this);
        }
    }
}
