using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Display
{
    public class DisplayManager
    {
        private SimpleClock clock;
        private int clockRow = 0;

        public void DisplayTime()
        {
            Task.Run(TimeThread);
        }

        private void TimeThread()
        {
            while (true)
            {
                Console.SetCursorPosition(0, clockRow);
                Console.WriteLine($"{clock.SimulatedTime}");
                Thread.Sleep(16);
            }
        }
    }
}
