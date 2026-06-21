using ControlTowner.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ControlTowner.IO
{
    public static class Path
    {
        public const string CONFIG_PATH = "D:\\Intern\\C# basic\\ControlTowner\\FileSave\\Config.json";
        public const string FLIGHT_DIARY_PATH = "D:\\Intern\\C# basic\\ControlTowner\\FileSave\\FlightDiary.txt";
        public const string FLIGHT_SCHEDULE_PATH = "D:\\Intern\\C# basic\\ControlTowner\\FileSave\\FlightSchedule.txt";
    }
  

    public static class ConfigIO
    {
        public static SimulationConfig? Load()
        {
            if (!File.Exists(Path.CONFIG_PATH)) throw new FileNotFoundException("File not found");
            string loadedJson = File.ReadAllText(Path.CONFIG_PATH);
            if (string.IsNullOrWhiteSpace(loadedJson)) return default;
            return JsonSerializer.Deserialize<SimulationConfig>(loadedJson);
        }
    }


    public static class FlightScheduleIO
    {
        public static string Load()
        {
            if (!File.Exists(Path.FLIGHT_SCHEDULE_PATH))
            {
                throw new FileNotFoundException("File not found");
            }
            return File.ReadAllText(Path.FLIGHT_SCHEDULE_PATH);
        }

        public static void Save(string data)
        {
            if (!File.Exists(Path.FLIGHT_SCHEDULE_PATH))
            {
                throw new FileNotFoundException("File not found");
            }
            File.WriteAllText(Path.FLIGHT_SCHEDULE_PATH, data);
        }
    }


    public static class FlightDiaryIO
    {
        public static string Load()
        {
            if (!File.Exists(Path.FLIGHT_DIARY_PATH))
            {
                throw new FileNotFoundException("File not found");
            }
            return File.ReadAllText(Path.FLIGHT_DIARY_PATH); 
        }

        public static void Save(string data)
        {
            if (!File.Exists(Path.FLIGHT_DIARY_PATH))
            {
                throw new FileNotFoundException("File not found");
            }
            string line = data + "\n";
            File.AppendAllText(Path.FLIGHT_DIARY_PATH, line);
        }
    }
}
