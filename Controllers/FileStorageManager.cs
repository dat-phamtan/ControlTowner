using ControlTowner.Config;
using ControlTowner.Entity;
using ControlTowner.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    public interface IStorage
    {
        public List<Flight> LoadDailySchedule();
        public void SaveDailyLog(string flightCode, int flightHour, int flightMinute, int runwayId, char flightType);
        public void GenerateDailySchedule(int mainStartHour, int mainStartMinute, int mainEndHour, int mainEndMinute, List<Flight>? delayedDailySchedule);
    }

    public class FileStorageManager : IStorage
    {
        int bufferMinute = 20;
        public void GenerateDailySchedule(int mainStartHour, int mainStartMinute, int mainEndHour, int mainEndMinute, List<Flight>? delayedDailySchedule)
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
            Console.WriteLine($"[IO] Generated flight schedule.");
        }


        public List<Flight> LoadDailySchedule()
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

                var flight = new Flight(code, FlightType.Takeoff)
                {
                    ScheduledTime = new DateTime(hour, minute, 0)
                };
                flights.Add(flight);
            }
            return flights;
        }

        public void SaveDailyLog(string flightCode, int flightHour, int flightMinute, int runwayId, char flightType)
        {
            string log = flightCode + " " + flightHour + " " + flightMinute + " " + runwayId + " " + flightType;
            FlightDiaryIO.Save(log);
            Console.WriteLine($"[LOG] Save daily log: {log}");
        }
    }
}
