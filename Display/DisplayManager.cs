using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using ControlTowner.Controllers;
using ControlTowner.Entity;
using ControlTowner.IO;

namespace ControlTowner.Display
{
    public class DisplayManager
    {
        private Controller controller;
        private List<string> logBuffer = new();
        private List<Flight> schedule = new();

        private readonly object logLock = new();

        private const int MAX_LOG_LINES = 15;
        private const int ROW_CLOCK = 0;
        private const int ROW_STATUS = 1;
        private const int ROW_RUNWAY_HEADER = 3;
        private const int ROW_QUEUE = 6;
        private const int ROW_SCHEDULE_HEADER = 8;
        private const int ROW_SCHEDULE_START = 9;
        private const int ROW_LOG_HEADER = 20;
        private const int ROW_LOG_START = 21;


        public DisplayManager(Controller controller)
        {
            this.controller = controller;
            this.controller.OnLogEntry += AddLog;
            this.controller.OnScheduleUpdated += UpdateSchedule;
        }

        public void Start()
        {
            DrawStaticLabels();
            Task.Run(RenderLoop);
        }

        
        private async Task RenderLoop()
        {
            while (true)
            {
                RenderClock();
                RenderStatus();
                RenderRunways();
                RenderSchedule();
                RenderLog();
                await Task.Delay(200);
            }
        }


        private void DrawStaticLabels()
        {
            WriteAtPosition(ROW_RUNWAY_HEADER, 0, "----------RUNWAY----------");
            WriteAtPosition(ROW_SCHEDULE_HEADER, 0, "----------SCHEDULE----------");
            WriteAtPosition(ROW_LOG_HEADER, 0, "----------DIARY----------");
        }

        
        private void RenderClock()
        {
            var time = SimpleClock.Instance.SimulatedTime;
            WriteAtPosition(ROW_CLOCK, 0, $"Clock: {time:dd/MM/yyyy HH:mm:ss}");
        }


        private void RenderStatus()
        {
            string status = (controller.IsMaintenanceMode()) ? "Status: Maintenance" : "Status: Working";
            WriteAtPosition(ROW_STATUS, 0, status);
        }


        private void RenderRunways()
        {
            var runways = controller.GetRunways();
            foreach (var runway in runways)
            {
                string info;
                int row = 1;
                if (runway.IsOccupied) 
                    info = $"Runway {runway.id}: [USED] {runway.CurrentFlight?.Code} {runway.CurrentFlight?.Type}";
                else 
                    info = $"Runway {runway.id}: [EMPTY]";

                WriteAtPosition(ROW_RUNWAY_HEADER + row, 2, info);
            }
        }


        private void RenderSchedule()
        {
            var scheduleList = new List<Flight>(schedule);
            for (int i = 0; i < 10; i++)
            {
                string line = "";
                if (i < scheduleList.Count) line = $"{scheduleList[i].Code} {scheduleList[i].ScheduledTime:HH/mm} {scheduleList[i].State}";
                WriteAtPosition(ROW_SCHEDULE_START + i, 2, line);
            }
        }


        private void UpdateSchedule(List<Flight> flights)
        {
            schedule = flights;
        }


        private void RenderLog()
        {
            var log = new List<string>(logBuffer);
            for (int i = 0; i < MAX_LOG_LINES; i++)
            {
                string line = (i < log.Count) ? log[log.Count - 1 - i] : string.Empty;
                WriteAtPosition(ROW_LOG_START + i, 2, line);
            }
        }


        private void AddLog(string newLog)
        {
            logBuffer.Add(newLog);
            if (logBuffer.Count > 50) logBuffer.RemoveAt(0);
        }


        public async Task ShowYesterdayDiaryAsync(float intervalSeconds = 1)
        {
            string diary = FlightDiaryIO.Load();
            if (string.IsNullOrWhiteSpace(diary))
            {
                Console.WriteLine("[ATC] No diary found");
                return;
            }
            Console.WriteLine("YESTERDAY DIARY");
            foreach (string line in diary.Split('\n'))
            {
                Console.WriteLine(" " + line);
                await Task.Delay((int)(intervalSeconds * 1000));
            }
            Console.WriteLine("END DIARY");
        }


        private static void WriteAtPosition(int row, int col, string text)
        {
            Console.SetCursorPosition(row, col);
            Console.Write(text);
        }
    }
}
