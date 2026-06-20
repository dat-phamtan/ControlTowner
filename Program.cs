using System;
using System.Threading;
using ControlTowner.Config;
using ControlTowner.Controllers;
using ControlTowner.Entity;
using ControlTowner.IO;
using ControlTowner.Utility;

namespace ControlTowner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //logger 
            var config = new SimulationConfig();
            var controller = new Controller(config.runawayCount, config.landingDuration, config.takeoffDuration);
            controller.Init();
            SimpleClock.Instance.InitClock(4, 59,config.timeScale, config.maintenanceStartHour, config.maintenanceStartMinute, config.maintenanceEndHour, config.maintenanceEndMinute);
            //looger
            //SimpleClock.Instance.OnTick += 

            //clock thread
            Task.Run(async () =>
            {
                while (true)
                {
                    SimpleClock.Instance.UpdateClock();
                    await Task.Delay(1000);
                }
            });



            //while (true)
            //{

            //}
        }
    }
}
