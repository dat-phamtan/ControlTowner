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
        private readonly List<string> logBuffer = new();
        private List<Flight> schedule = new();

        private static readonly object logLock = new();
        private static readonly object consoleLock = new();
        private static readonly object scheduleLock = new();

        private const int MAX_LOG_LINES = 15;
        private const int ROW_CLOCK = 0;
        private const int ROW_STATUS = 1;
        private const int ROW_RUNWAY_HEADER = 3;
        private const int ROW_QUEUE = 6;
        private const int ROW_SCHEDULE_HEADER = 9;
        private const int ROW_SCHEDULE_START = 10;
        private const int ROW_LOG_HEADER = 21;
        private const int ROW_LOG_START = 22;


        public DisplayManager(Controller controller, ILogSource? logSource = null)
        {
            logSource?.OnLog += AddLog;
            this.controller = controller;
            this.controller.OnScheduleUpdated += UpdateSchedule;
        }

        public void Start()
        {
            Console.Clear();
            Console.CursorVisible = false;
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
            WriteAtRow(ROW_RUNWAY_HEADER, "----------RUNWAY----------");
            WriteAtRow(ROW_SCHEDULE_HEADER, "----------SCHEDULE----------");
            WriteAtRow(ROW_LOG_HEADER, "----------DIARY----------");
        }

        
        private void RenderClock()
        {
            var time = SimpleClock.Instance.SimulatedTime;
            WriteAtRow(ROW_CLOCK, $"Clock: {time:dd/MM/yyyy HH:mm:ss}");
        }


        private void RenderStatus()
        {
            string status = (controller.IsMaintenanceMode()) ? "Status: Maintenance" : "Status: Working";
            WriteAtRow(ROW_STATUS, status);
        }


        private void RenderRunways()
        {
            var runways = controller.GetRunways();
            for(int i = 0; i < runways.Length; i++) 
            {
                string info;
                var runway = runways[i];
                if (runway.IsOccupied) 
                    info = $"Runway {runway.id}: [USED] {runway.CurrentFlight?.Code} {runway.CurrentFlight?.Type}";
                else 
                    info = $"Runway {runway.id}: [EMPTY]";

                WriteAtRow(ROW_RUNWAY_HEADER + 1 + i, info);
            }
        }


        private void RenderSchedule()
        {
            List<Flight> scheduleList;
            lock (scheduleLock)
            {
                scheduleList = new(schedule);
            }
            
            for (int i = 0; i < 10; i++)
            {
                string line = "";
                if (i < scheduleList.Count) line = $"{scheduleList[i].Code} {scheduleList[i].ScheduledTime:HH/mm} {scheduleList[i].State}";
                WriteAtRow(ROW_SCHEDULE_START + i, line.PadRight(50));
            }
        }


        private void UpdateSchedule(List<Flight> flights)
        {
            lock (scheduleLock)
            {
                schedule = flights;
            }
        }


        private void RenderLog()
        {
            List<string> log;
            lock (logLock)
            {
                log = new(logBuffer);
            }
            
            for (int i = 0; i < MAX_LOG_LINES; i++)
            {
                string line = " ";
                if (i < log.Count)
                    line += log[log.Count - 1 - i];
                WriteAtRow(ROW_LOG_START + i, line.PadRight(80));
            }
        }


        private void AddLog(string newLog)
        {
            lock (logLock)
            {
                logBuffer.Add(newLog);
                if (logBuffer.Count > 50) logBuffer.RemoveAt(0);
            }
        }


        public async Task ShowYesterdayDiaryAsync(float intervalSeconds = 1)
        {
            string diary = FlightDiaryIO.Load();
            if (string.IsNullOrWhiteSpace(diary))
            {
                AddLog("[ATC] No diary found");
                return;
            }
            AddLog("---YESTERDAY DIARY---");
            foreach (string line in diary.Split('\n'))
            {
                AddLog(" " + line);
                await Task.Delay((int)(intervalSeconds * 1000));
            }
            AddLog("---END DIARY---");
        }


        private static void WriteAtRow(int row, string text)
        {
            lock (consoleLock)
            {
                Console.SetCursorPosition(0, row);
                Console.Write(text.PadRight(Console.WindowWidth - 1));
            }
        }
    }
}
