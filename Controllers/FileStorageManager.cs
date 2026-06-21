using ControlTowner.Config;
using ControlTowner.Entity;
using ControlTowner.IO;
using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    public interface IStorage
    {
        public List<Flight> LoadDailySchedule(DateTime today, ILogger logger);
        public void SaveDailyLog(string flightCode, int flightHour, int flightMinute, int runwayId, char flightType, ILogger logger);
        public void GenerateDailySchedule(int mainStartHour, int mainStartMinute, int mainEndHour, int mainEndMinute, List<Flight>? delayedDailySchedule, ILogger logger);
    }

    public class FileStorageManager : IStorage
    {
        int bufferMinute = 20;
        public void GenerateDailySchedule(int mainStartHour, int mainStartMinute, int mainEndHour, int mainEndMinute, List<Flight>? delayedDailySchedule, ILogger logger)
        {
            var lines = new List<string>();
            int hour = mainEndHour;
            int minute = mainStartMinute + bufferMinute;
            //Random ranMinnute = new();
            if (delayedDailySchedule != null)
            {
                foreach (var delayedLine in delayedDailySchedule)
                {
                    if (hour > mainStartHour || (hour == mainStartHour && minute >= mainStartMinute)) break;
                    //int randomMintute = ranMinnute.Next(30, 60);
                    string line = delayedLine.Code + " " + hour + " " + minute;
                    lines.Add(line);
                    hour++;
                }
            }

            Random randomCodeNum = new();
            while (hour <= mainStartHour && minute < mainStartMinute - bufferMinute)
            {
                string randomCode = "MH" + randomCodeNum.Next(100, 999);
                string line = randomCode + " " + hour + " " + minute;
                lines.Add(line);
                hour++;
            }
            FlightScheduleIO.Save(string.Join('\n', lines));
            logger.Log($"[IO] Generated flight schedule.");
        }


        public List<Flight> LoadDailySchedule(DateTime today, ILogger logger)
        {
            string raw = FlightScheduleIO.Load();
            var flights = new List<Flight>();
            foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;

                string code = parts[0];
                if (!int.TryParse(parts[1], out int hour)) continue;
                if (!int.TryParse(parts[2], out int minute)) continue;

                var flight = new Flight(code, FlightType.Takeoff, logger)
                {
                    ScheduledTime = today.AddHours(hour).AddMinutes(minute)
                };
                flights.Add(flight);
            }
            return flights;
        }

        public void SaveDailyLog(string flightCode, int flightHour, int flightMinute, int runwayId, char flightType, ILogger logger)
        {
            string log = flightCode + " " + flightHour + " " + flightMinute + " " + runwayId + " " + flightType;
            FlightDiaryIO.Save(log);
            logger.Log($"[LOG] Save daily log: {log}");
        }
    }
}
