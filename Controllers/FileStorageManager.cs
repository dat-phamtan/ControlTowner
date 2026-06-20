using ControlTowner.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    public interface IStorage
    {
        public List<Flight> LoadDailySchedule(DateTime date);
        public void SaveDailyLog(string flightCode, int flightHour, int flightMinute, int runwayId, char flightType);
        public void GenerateDailySchedule();
    }

    public class FileStorageManager : IStorage
    {
        public void GenerateDailySchedule()
        {
            //return default;
        }

        public List<Flight> LoadDailySchedule(DateTime date)
        {
            // i will imple
            return default;
        }

        public void SaveDailyLog(string flightCode, int flightHour, int flightMinute, int runwayId, char flightType)
        {
            string log = flightCode + " " + flightHour + " " + flightMinute + " " + runwayId + " " + flightType;
            //IO.FlightDiaryIO.Save()
        }
    }
}
