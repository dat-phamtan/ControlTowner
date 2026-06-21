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
    public class Program
    {
        private const int START_HOUR = 2;
        private const int START_MINUTE = 29;

        static async Task Main(string[] args)
        {
            //logger init
            var logger = new EventLogger();
            //load config
            var config = SimulationConfig.Load(logger);

            //init controller
            var controller = new Controller(config, START_HOUR, START_MINUTE, logger, new RandomLandingGenerator(), new FileStorageManager());
            controller.Init();

            //clock init
            SimpleClock.Instance.InitClock(
                START_HOUR, 
                START_MINUTE,
                config.TimeScale,
                config.MaintenanceStartHour,
                config.MaintenanceStartMinute,
                config.MaintenanceEndHour,
                config.MaintenanceEndMinute
            );

            //display init
            var display = new DisplayManager(controller, logger);

            ////draw diary
            //await display.ShowYesterdayDiaryAsync(0.5f);

            //draw ui
            display.Start();

            //load schedule
            //controller.LoadSchedule(SimpleClock.Instance.SimulatedTime.Date);

            //main loop
            await MainLoop();
        }

        private static async Task MainLoop()
        {
            while (true)
            {
                SimpleClock.Instance.UpdateClock();
                await Task.Delay(100);
            }
        }
    }
}
