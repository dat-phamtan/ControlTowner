using ControlTowner.Config;
using ControlTowner.Entity;
using ControlTowner.IO;
using ControlTowner.Utility;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ControlTowner.Controllers
{
    public interface IStorage
    {
        public List<Flight> LoadDailySchedule(DateTime today, ILogger logger);
        public void SaveDailyLog(string flightCode, DateTime time, int runwayId, char flightType, ILogger logger);
        public void GenerateDailySchedule(int mainStartHour, int mainStartMinute, int mainEndHour, int mainEndMinute, List<Flight>? delayedDailySchedule, ILogger logger);
    }

    public class FileStorageManager : IStorage
    {
        int bufferMinute = 20;
        private string[] header = { "MH", "VN", "SK", "FA", "OL" };

        public void GenerateDailySchedule(int mainStartHour, int mainStartMinute, int mainEndHour, int mainEndMinute, List<Flight>? delayedDailySchedule, ILogger logger)
        {
            var lines = new List<string>();
            int hour = mainEndHour + 1;
            int minute = mainStartMinute + bufferMinute;
            DateTime date = SimpleClock.Instance.SimulatedTime.Date;

            Random random = new();
            if (delayedDailySchedule != null)
            {
                foreach (var delayedLine in delayedDailySchedule)
                {
                    int randomMintute = random.Next(60);
                    if (hour > mainStartHour || (hour == mainStartHour && randomMintute >= mainStartMinute)) break;
                    string line = delayedLine.Code + " " + hour + " " + randomMintute + " " + date.ToString("dd/MM/yyyy");
                    lines.Add(line);
                    hour++;
                    if (hour > 23)
                    {
                        hour = 0;
                        date = date.AddDays(1);
                    }
                }
            }

            if (mainStartHour < mainEndHour) //maintain at same day
            {
                for (int i = mainEndHour + 1; i < 24; i++)
                {
                    int headerIndex = random.Next(header.Length);
                    string randomCode = header[headerIndex] + random.Next(100, 999);
                    minute = random.Next(0, 60);
                    string line = randomCode + " " + hour + " " + minute + " " + date.ToString("dd/MM/yyyy");
                    lines.Add(line);
                    hour++;
                }
                hour = 0;
                date = date.AddDays(1);
                for (int i = 0; i < mainStartHour; i++)
                {
                    int headerIndex = random.Next(header.Length);
                    string randomCode = header[headerIndex] + random.Next(100, 999);
                    minute = random.Next(0, 60);
                    string line = randomCode + " " + hour + " " + minute + " " + date.ToString("dd/MM/yyyy");
                    lines.Add(line);
                    hour++;
                }
            }
            else
            {
                while (hour < mainStartHour)
                {
                    int headerIndex = random.Next(header.Length);
                    string randomCode = header[headerIndex] + random.Next(100, 999);
                    minute = random.Next(0, 60);
                    string line = randomCode + " " + hour + " " + minute + " " + date.ToString("dd/MM/yyyy");
                    lines.Add(line);
                    hour++;
                }
            }

            FlightScheduleIO.Save(string.Join('\n', lines));
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
                if (!DateTime.TryParseExact(parts[3], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) continue;

                DateTime scheduledTime = date.AddHours(hour).AddMinutes(minute);
                var flight = new Flight(code, FlightType.Takeoff, FlightState.Waiting, scheduledTime, logger);
                flights.Add(flight);
            }
            return flights;
        }

        public void SaveDailyLog(string flightCode, DateTime time, int runwayId, char flightType, ILogger logger)
        {
            string log = time.ToString("dd/MM/yyyy HH:mm") + " " + flightCode + " " + runwayId.ToString() + " " + flightType;
            FlightDiaryIO.Save(log);
            logger.Log($"[SYSTEM] Saved daily log: [{flightCode}]");
        }
    }
}
