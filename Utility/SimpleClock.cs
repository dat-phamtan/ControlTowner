using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using ControlTowner.Config;

namespace ControlTowner.Utility
{
    internal class SimpleClock
    {
        public static SimpleClock Instance { get; } = new SimpleClock();
        private SimpleClock() { }
        public DateTime SimulatedTime { get; private set; }
        public float TimeScale { get; set; } = 60f;

        public DateTime lastRealTime;
        private readonly object lockObject = new();

        public event Action<DateTime> OnTick;
        public Action OnMaintenanceStart;
        public Action OnNewDayStart;
        

        public void InitClock(int startHour, int startMinute)
        {
            SimulatedTime = DateTime.Today.AddHours(startHour).AddMinutes(startMinute);
            lastRealTime = DateTime.UtcNow;
        }

        
        public double UpdateClock()
        {
            DateTime currentRealTime = DateTime.UtcNow;
            double delta;

            lock (lockObject)
            {
                delta = (currentRealTime - lastRealTime).TotalSeconds;
                DateTime oldTime = SimulatedTime;
                SimulatedTime = SimulatedTime.AddSeconds(Math.Floor(delta * TimeScale));
                lastRealTime = currentRealTime;
                Console.WriteLine($"{SimulatedTime}");
                CheckSpecialEvent(oldTime, SimulatedTime);
            }
            
            OnTick?.Invoke(SimulatedTime);
            return delta;
        }


        private void CheckSpecialEvent(DateTime oldTime, DateTime newTime)
        {
            TimeSpan maintenanceStart = new(SimulationConfig.MaintenanceStartHour, SimulationConfig.MaintenanceStartMinute, 0);
            TimeSpan maintenanceEnd = new(SimulationConfig.MaintenanceEndHour, SimulationConfig.MaintenanceEndMinute, 0);
            //Console.Write($" {maintenanceEnd}");

            if (HasCrossedThreshold(oldTime, newTime, maintenanceStart))
            {
                OnMaintenanceStart.Invoke();
            }

            if (HasCrossedThreshold(oldTime, newTime, maintenanceEnd))
            {
                OnNewDayStart.Invoke();
            }
        }


        private bool HasCrossedThreshold(DateTime oldTime, DateTime newTime, TimeSpan threshold)
        {
            TimeSpan oldTimeOfDay = oldTime.TimeOfDay;
            TimeSpan newTimeOfDay = newTime.TimeOfDay;

            if (oldTimeOfDay <= newTimeOfDay) // not cross the midnight
            {
                return oldTimeOfDay < threshold && newTimeOfDay >= threshold;
            }
            else
            {
                return oldTimeOfDay < threshold || newTimeOfDay >= threshold;
            }

        }
    }
}
