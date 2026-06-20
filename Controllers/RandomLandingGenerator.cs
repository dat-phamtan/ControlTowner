using ControlTowner.Entity;
using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    public interface ILandingGenerator
    {
        public Flight? CheckGenerate(DateTime time);
    }

    public class RandomLandingGenerator : ILandingGenerator
    {
        private bool isWaiting = false;
        private DateTime generateTime;
        public Flight? CheckGenerate(DateTime time)
        {
            Random random = new();
            if (!isWaiting)
            {
                int randomPeriod = random.Next(3, 7) * 1000;
                generateTime = SimpleClock.Instance.SimulatedTime.AddSeconds(randomPeriod);
                isWaiting = true;
                return null;
            }

            if (isWaiting && (generateTime < SimpleClock.Instance.SimulatedTime))
            {
                isWaiting = false;
                string code = "MH" + random.Next(100, 999).ToString();
                return new Flight(code, FlightType.Landing);
            }
            return null;
        }
    }
}
