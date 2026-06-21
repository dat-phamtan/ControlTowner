using System;
using System.Threading;
using ControlTowner.Config;
using ControlTowner.Controllers;
using ControlTowner.Display;
using ControlTowner.Entity;
using ControlTowner.IO;
using ControlTowner.Utility;

namespace ControlTowner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //load config
            var config = SimulationConfig.Load();
            Console.WriteLine($"[LOADED] Runway count: {config.runawayCount}, time scale: {config.timeScale}");

            //init controller
            float standardLandDuration = config.landingDuration * 1000f / config.timeScale;
            float standardTakeoffDuration = config.takeoffDuration * 1000f / config.timeScale;
            var controller = new Controller(config);
            controller.Init();

            //clock init
            SimpleClock.Instance.InitClock(
                4, 
                59,
                config.timeScale,
                config.maintenanceStartHour,
                config.maintenanceStartMinute,
                config.maintenanceEndHour,
                config.maintenanceEndMinute
            );

            //display init
            var display = new DisplayManager(controller);

            //draw diary
            await display.ShowYesterdayDiaryAsync(0.5f);

            //draw ui
            display.Start();

            //load schedule
            controller.LoadSchedule();

            //main loop
            _ = Task.Run(MainLoop);

            //waiting
            await Task.Delay(Timeout.Infinite);
        }

        private static async void MainLoop()
        {
            while (true)
            {
                SimpleClock.Instance.UpdateClock();
                await Task.Delay(1000);
            }
        }
    }
}
