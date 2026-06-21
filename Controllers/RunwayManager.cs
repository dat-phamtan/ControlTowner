using ControlTowner.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    public class RunwayManager
    {
        public Runway[] runways;
        public Action? OnBecomeAvailable;


        public RunwayManager(int count)
        {
            runways = new Runway[count];
            for (int i = 0; i < count; i++)
            {
                runways[i] = new Runway(i, OnRunwayChanged);
            }
        }


        public Runway? GetAvailableRunway()
        {
            for (int i = 0; i < runways.Length; i++)
            {
                if (!runways[i].IsOccupied) return runways[i];
            }
            return null;
        }


        public bool AllRunwayEmpty()
        {
            for (int i = 0; i < runways.Length; i++)
                if (runways[i].IsOccupied) return false;
            return true;
        }


        public Runway[] GetRunways()
        {
            return runways;
        }


        private void OnRunwayChanged(bool isOccupied)
        {
            if (!isOccupied)
                OnBecomeAvailable?.Invoke();
        }
    }
}
