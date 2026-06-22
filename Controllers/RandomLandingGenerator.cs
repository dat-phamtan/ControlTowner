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
        private const int minGapMinute = 10;
        private const int maxGapMinute = 25;
        private bool isWaiting = false;
        private DateTime generateTime;
        private string[] header = { "MH", "VN", "SK", "FA", "OL" };
        public Flight? CheckGenerate(DateTime simulatedTime, ILogger logger)
        {
            Random random = new();
            if (!isWaiting)
            {
                int randomPeriod = random.Next(minGapMinute * 60, maxGapMinute * 60);
                generateTime = simulatedTime.AddSeconds(randomPeriod);
                isWaiting = true;
                return null;
            }

            if (generateTime <= simulatedTime)
            {
                isWaiting = false;
                int headerIndex = random.Next(header.Length);
                string code = header[headerIndex] + random.Next(100, 999).ToString();
                return new Flight(code, FlightType.Landing, FlightState.Waiting, SimpleClock.Instance.SimulatedTime, logger);
            }
            return null;
        }

        public void Reset()
        {
            isWaiting = false;
        }
    }
}
