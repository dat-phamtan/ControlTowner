using ControlTowner.Entity;
using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    public interface ILandingGenerator
    {
        public Flight? CheckGenerate(DateTime simulatedTime, ILogger logger);
        public void Reset();
    }

    public class RandomLandingGenerator : ILandingGenerator
    {
        private bool isWaiting = false;
        private DateTime generateTime;
        public Flight? CheckGenerate(DateTime simulatedTime, ILogger logger)
        {
            Random random = new();
            if (!isWaiting)
            {
                int randomPeriod = random.Next(180, 420);
                generateTime = simulatedTime.AddSeconds(randomPeriod);
                isWaiting = true;
                return null;
            }

            if (generateTime <= simulatedTime)
            {
                isWaiting = false;
                string code = "MH" + random.Next(100, 999).ToString();
                return new Flight(code, FlightType.Landing, logger);
            }
            return null;
        }

        public void Reset()
        {
            isWaiting = false;
        }
    }
}
