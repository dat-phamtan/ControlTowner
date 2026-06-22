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

        private const int SECOND_COL_NUMS = 55;
        private const int MAX_LOG_LINES = 15;
        private const int ROW_CLOCK = 0;
        private const int ROW_STATUS = 1;
        private const int ROW_RUNWAY_HEADER = 3;
        private const int ROW_SCHEDULE_HEADER = 20;
        private const int ROW_SCHEDULE_START = 21;
        private const int ROW_LOG_HEADER = 2;
        private const int ROW_LOG_START = 3;
        private const int ROW_DIARY_HEADER = 20;
        private const int ROW_DIARY_START = 21;


        public DisplayManager(Controller controller, ILogSource? logSource = null)
        {
            logSource?.OnLog += AddLog;
            this.controller = controller;
            this.controller.OnScheduleUpdated += UpdateSchedule;
            this.controller.OnLogDiary += HandleYesterdayDiary;
        }

        public void Start()
        {
            if (OperatingSystem.IsWindows() && Console.WindowHeight < 40)
                Console.WindowHeight = 100;

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
                //Test();
                await Task.Delay(200);
            }
        }


        private void DrawStaticLabels()
        {
            WriteAtPosition("----------RUNWAY----------", ROW_RUNWAY_HEADER);
            WriteAtPosition("----------DIARY----------", ROW_DIARY_HEADER);
            WriteAtPosition("----------SCHEDULE----------", ROW_SCHEDULE_HEADER, SECOND_COL_NUMS);
            WriteAtPosition("----------LOG----------", ROW_LOG_HEADER, SECOND_COL_NUMS);
        }

        
        private void RenderClock()
        {
            var time = SimpleClock.Instance.SimulatedTime;
            WriteAtPosition($"Clock: {time:dd/MM/yyyy HH:mm:ss}", ROW_CLOCK);
        }


        private void RenderStatus()
        {
            string status = (controller.IsMaintenanceMode()) ? "Status: Maintenance" : "Status: Working";
            WriteAtPosition(status, ROW_STATUS);
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

                WriteAtPosition(info, ROW_RUNWAY_HEADER + 1 + i);
            }
        }


        private void RenderSchedule()
        {
            List<Flight> scheduleList;
            lock (scheduleLock)
            {
                scheduleList = new(schedule);
            }
            
            int numSchedule = scheduleList.Count;
            for (int i = 0; i < numSchedule; i++)
            {
                string line = "";
                if (i < scheduleList.Count) line = $"{scheduleList[i].ScheduledTime.ToString("dd/MM/yyyy HH:mm")} {scheduleList[i].Code} {scheduleList[i].State}";
                WriteAtPosition(line, ROW_SCHEDULE_START + i, SECOND_COL_NUMS);
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
                WriteAtPosition(line.PadRight(50), ROW_LOG_START + i, SECOND_COL_NUMS);

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


        private async void HandleYesterdayDiary()
        {
            await ShowYesterdayDiaryAsync(1);
        }


        private async Task ShowYesterdayDiaryAsync(float intervalSeconds = 1)
        {
            string diary = FlightDiaryIO.Load().Trim();
            FlightDiaryIO.ClearDiary();
            if (string.IsNullOrWhiteSpace(diary))
            {
                AddLog("[ATC] Tommorrow's diary count: 0");
                return;
            }
            string[] diaryList = diary.Split('\n');
            AddLog($"[ATC] Tommorrow's diary count: {diaryList.Length}");
            for (int i = 0; i < diaryList.Length; i++)
            {
                WriteAtPosition(diaryList[i], ROW_DIARY_START + i);
                await Task.Delay((int)(intervalSeconds * 1000));
            }
        }

        //public void Test()
        //{
        //    WriteAtPosition("000000000000000000", 20, 55);
        //    WriteAtPosition("000000000000000000", 21, 55);
        //    WriteAtPosition("000000000000000000", 22, 55);
        //    WriteAtPosition("000000000000000000", 23, 55);
        //    WriteAtPosition("000000000000000000", 24, 55);
        //    WriteAtPosition("000000000000000000", 25, 55);
        //}

        private static void WriteAtPosition(string text, int row, int col = 0)
        {
            lock (consoleLock)
            {
                Console.SetCursorPosition(col, row);
                int availableSpace = Console.WindowWidth - col - 1;
                if (availableSpace <= 0) return;

                if (text.Length > availableSpace)
                {
                    text = text.Substring(0, availableSpace - 3);
                    text += "...";
                }
                Console.Write(text.PadRight(availableSpace));
            }
        }
    }
}
