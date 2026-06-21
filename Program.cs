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
            var logger = new EventLogger();

            //load config
            var config = SimulationConfig.Load();
            logger?.Log($"[LOADED] Runway count: {config.runawayCount}, time scale: {config.timeScale}");

            //init controller
            var controller = new Controller(config, logger, new RandomLandingGenerator(), new FileStorageManager());
            controller.Init();

            //clock init
            SimpleClock.Instance.InitClock(
                6, 
                0,
                config.timeScale,
                config.maintenanceStartHour,
                config.maintenanceStartMinute,
                config.maintenanceEndHour,
                config.maintenanceEndMinute
            );

            //display init
            var display = new DisplayManager(controller, logger);

            //draw diary
            //await display.ShowYesterdayDiaryAsync(0.5f);

            //draw ui
            display.Start();

            //load schedule
            controller.LoadSchedule(SimpleClock.Instance.SimulatedTime.Date);

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
