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

        private float timeScale;
        private TimeSpan mainStart;
        private TimeSpan mainEnd;
        private DateTime lastRealTime;
        private readonly object lockObject = new();

        public event Action<DateTime> OnTick;
        public event Action OnMaintenanceStart;
        public event Action OnNewDayStart;
        

        public void InitClock(int startHour, int startMinute, float timeScale, int mainStartHour, int mainStartMinute, int mainEndHour, int mainEndMinute)
        {
            SimulatedTime = DateTime.Today.AddHours(startHour).AddMinutes(startMinute);
            lastRealTime = DateTime.UtcNow;
            mainStart = new TimeSpan(mainStartHour, mainStartMinute, 0);
            mainEnd = new TimeSpan(mainEndHour, mainEndMinute, 0);
            this.timeScale = timeScale;
        }

        
        public double UpdateClock()
        {
            DateTime currentRealTime = DateTime.UtcNow;
            double delta;
            DateTime oldTime, newTime;
            lock (lockObject)
            {
                delta = (currentRealTime - lastRealTime).TotalSeconds;
                oldTime = SimulatedTime;
                SimulatedTime = SimulatedTime.AddSeconds(Math.Floor(delta * timeScale));
                lastRealTime = currentRealTime;
                newTime = SimulatedTime;
                //Console.WriteLine($"{SimulatedTime}");
            }
            CheckSpecialEvent(oldTime, newTime);
            OnTick?.Invoke(newTime);
            return delta;
        }


        private void CheckSpecialEvent(DateTime oldTime, DateTime newTime)
        {
            TimeSpan maintenanceStart = new(mainStart.Hours, mainStart.Minutes, 0);
            TimeSpan maintenanceEnd = new(mainEnd.Hours, mainEnd.Minutes, 0);
            //Console.Write($" {maintenanceEnd}");

            if (HasCrossedThreshold(oldTime, newTime, maintenanceStart))
                OnMaintenanceStart.Invoke();
            if (HasCrossedThreshold(oldTime, newTime, maintenanceEnd))
                OnNewDayStart.Invoke();
        }


        private bool HasCrossedThreshold(DateTime oldTime, DateTime newTime, TimeSpan threshold)
        {
            TimeSpan oldTimeOfDay = oldTime.TimeOfDay;
            TimeSpan newTimeOfDay = newTime.TimeOfDay;

            if (oldTimeOfDay <= newTimeOfDay) // not cross the midnight
                return oldTimeOfDay < threshold && newTimeOfDay >= threshold;
            else
                return oldTimeOfDay < threshold || newTimeOfDay >= threshold;
        }
    }
}
