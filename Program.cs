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
            var controller = new Controller();
            controller.Init();
            SimpleClock.Instance.InitClock(4, 59);
            //looger
            //SimpleClock.Instance.OnTick += 

            //clock thread
            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        SimpleClock.Instance.UpdateClock();
            //        await Task.Delay(1000);
            //    }
            //});



            //while (true)
            //{

            //}
        }
    }
}
