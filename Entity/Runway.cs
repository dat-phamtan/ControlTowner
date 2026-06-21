using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Entity
{
    public class Runway(int id, Action<bool>? onStateChanged = null)
    {
        public int id = id;
        public bool IsOccupied { get; set; } = false;
        public Flight? CurrentFlight { get; set; }
        public float RealDuration { get; set; }

        private readonly object lockObj = new();

        public Action<bool>? OnStateChanged = onStateChanged;

        public bool AssignFlight(Flight flight)
        {
            if (flight == null) return false;
            lock(lockObj)
            {
                if (IsOccupied) return false;
                CurrentFlight = flight;
                IsOccupied = true;
                OnStateChanged?.Invoke(IsOccupied);
                return true;
            }
        }

        public void Free()
        {
            lock(lockObj)
            {
                CurrentFlight = null;
                IsOccupied = false;
            }
            OnStateChanged?.Invoke(IsOccupied);
        }
    }
}
