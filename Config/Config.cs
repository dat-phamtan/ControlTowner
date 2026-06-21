using ControlTowner.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Config
{
    public class SimulationConfig
    {
        public int runawayCount = 3;
        public float timeScale = 1.5f;
        public float takeoffDuration = 20;
        public float landingDuration = 25;
        public int maintenanceStartHour = 2;
        public int maintenanceStartMinute = 30;
        public int maintenanceEndHour = 5;
        public int maintenanceEndMinute = 15;

        public static SimulationConfig Load()
        {
            var cfg = ConfigIO.Load();
            if (cfg == null) return new SimulationConfig();
            else return cfg;
        }
    }
}
