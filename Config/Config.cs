using ControlTowner.IO;
using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Config
{
    public class SimulationConfig
    {
        public int RunwayCount { get; set; } = 4;
        public float TimeScale { get; set; } = 1.5f;
        public float TakeoffDuration { get; set; } = 20;
        public float LandingDuration { get; set; } = 25;
        public int MaintenanceStartHour { get; set; } = 2;
        public int MaintenanceStartMinute { get; set; } = 30;
        public int MaintenanceEndHour { get; set; } = 5;
        public int MaintenanceEndMinute { get; set; } = 15;

        public static SimulationConfig Load(ILogger logger)
        {
            var cfg = ConfigIO.Load();
            if (cfg == null)
            {
                logger?.Log($"[SYSTEM] No file to load, load default!! Runway count: {3}, time scale: {1.5}");
                return new SimulationConfig();
            }
            else 
            {
                logger?.Log($"[SYSTEM] Load config success!! Runway count: {cfg.RunwayCount}, time scale: {cfg.TimeScale}");
                return cfg; 
            }
        }
    }
}
