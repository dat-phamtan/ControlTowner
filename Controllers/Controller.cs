using ControlTowner.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using ControlTowner.Config;
using ControlTowner.Utility;
using System.Xml.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using ControlTowner.IO;

namespace ControlTowner.Controllers
{
    public class Controller
    {
        private readonly float landingDuration;
        private readonly float takeoffDuration;

        private readonly Queue<Flight> takeoffQueue = new();
        private readonly Queue<Flight> landingQueue = new();
        private readonly object queueLock = new();

        private readonly RunwayManager runwayManager;
        private readonly SimulationConfig simulationConfig;
        private readonly ILandingGenerator flightGenerator;
        private readonly IStorage storage;
        private IAirportState currentState;

        private List<Flight> todaySchedule = [];
        private readonly object scheduleLock = new();

        private bool maintenanceMode = false;
        private List<Flight> unfinishedFlights = new();

        public event Action<string>? OnLogEntry;
        public event Action<List<Flight>>? OnScheduleUpdated;


        public Controller(SimulationConfig config, ILandingGenerator? generator = null, IStorage? storage = null)
        {
            this.landingDuration = config.landingDuration;
            this.takeoffDuration = config.takeoffDuration;
            this.storage = storage;
            simulationConfig = config;
            runwayManager = new RunwayManager(config.runawayCount);
            currentState = new NormalAirportState();
            flightGenerator = generator;
        }


        public void Init()
        {
            runwayManager.OnBecomeAvailable += ProcessQueues;

            SimpleClock.Instance.OnTick += HandleClockTick;
            SimpleClock.Instance.OnMaintenanceStart += HandleMaintenanceStart;
            SimpleClock.Instance.OnNewDayStart += HandleNewDayStart;
        }


        public void LoadSchedule()
        {
            todaySchedule = storage.LoadDailySchedule();
            unfinishedFlights.Clear();
            Console.WriteLine($"[ATC] Loaded {todaySchedule.Count} diary's records");
            OnScheduleUpdated?.Invoke( todaySchedule );
        }


        private void DispathScheduledFights(DateTime now)
        {
            List<Flight> toDispath = [];
            for (int i = 0; i < todaySchedule.Count; i++)
            {
                if (todaySchedule[i].State == FlightState.Waiting && todaySchedule[i].ScheduledTime <= now)
                {
                    toDispath.Add(todaySchedule[i]);
                }
            }

            foreach (var flight in toDispath)
            {
                flight.State = FlightState.Operating;
                Console.WriteLine($"[ATC] Take off requirement: {flight.Code} ({flight.ScheduledTime:HH:mm})");
                EnqueueTakeoff(flight);
            }
        }


        public void EnqueueTakeoff(Flight flight)
        {
            takeoffQueue.Enqueue(flight);
            Console.WriteLine($"[QUEUE] Append flight {flight.Code} to Take off queue");
            ProcessQueues();
        }


        public void EnqueueLanding(Flight flight)
        {
            landingQueue.Enqueue(flight);
            Console.WriteLine($"[QUEUE] Append flight {flight.Code} to Landing queue");
            ProcessQueues();
        }


        public void ProcessQueues()
        {
            Runway? runway = runwayManager.GetAvailableRunway();
            if (runway == null) return;
            if (landingQueue.Count == 0 && takeoffQueue.Count == 0) return;

            if (landingQueue.TryDequeue(out Flight? landingFlight))
            {
                if (runway.AssignFlight(landingFlight))
                {
                    runway.RealDuration = landingDuration;
                    ExecuteFlight(runway, landingFlight);
                    ProcessQueues();
                }
            }
            else if (takeoffQueue.TryDequeue(out Flight? takeoffFlight))
            {
                if (runway.AssignFlight(takeoffFlight))
                {
                    runway.RealDuration = takeoffDuration;
                    ExecuteFlight(runway, takeoffFlight);
                    ProcessQueues();
                }
            }
        }


        private void ExecuteFlight(Runway runway, Flight flight)
        {
            flight.OnRequestConfirmation += ATCHandleFlightConfirm;
            flight.OnActionCompleted += ATCHandleFlightComplete;
            Task.Run(() => flight.ExecuteActionAsync(runway, runway.RealDuration));
        }


        private async Task ATCHandleFlightConfirm(Flight flight)
        {
            if (flight.Type == FlightType.Landing) currentState.HandleLanding(flight);
            else currentState.HandleTakeoff(flight);

            await Task.CompletedTask;
        }


        private void ATCHandleFlightComplete(Runway runway, Flight flight)
        {
            string action = (flight.Type == FlightType.Takeoff) ? "taking off" : "landing";
            Console.WriteLine($"[ATC] Completed: Flight {flight.Code} {action} in runway number {runway.id}");

            char flightType = (flight.Type == FlightType.Takeoff) ? 'T' : 'L';
            DateTime time = SimpleClock.Instance.SimulatedTime;
            storage.SaveDailyLog(flight.Code, time.Hour, time.Minute, runway.id, flightType);
            runway.Free();
        }


        private void HandleClockTick(DateTime simulatedTime)
        {
            if (!maintenanceMode)
            {
                var landing = flightGenerator.CheckGenerate(simulatedTime);
                if (landing != null)
                {
                    Console.WriteLine($"[ATC] Got a landing request from: {landing.Code}");
                    EnqueueLanding(landing);
                }
            }
            DispathScheduledFights(simulatedTime);
        }

        private void HandleMaintenanceStart()
        {
            maintenanceMode = true;
            currentState = new MaintenanceAirportState();
            for (int i = 0; i < todaySchedule.Count; i++)
            {
                if (todaySchedule[i].State == FlightState.Waiting)
                {
                    unfinishedFlights.Add(todaySchedule[i]);
                }
            }
            Task.Run(GenerateTomorrowSchedule);
            Task.Run(WaitForAllFlightsCompleted);
            Task.Run(WaitForAllFlightsCompleted);
        }

        private void HandleNewDayStart()
        {
            Console.WriteLine("Start new day");
            maintenanceMode = false;
            currentState = new NormalAirportState();
            flightGenerator.Reset();

            var today = SimpleClock.Instance.SimulatedTime.Date;
            Console.WriteLine($"[ATC] Start new day ({today:dd/MM/yyyy})");
            LoadSchedule();
        }


        private async void GenerateTomorrowSchedule()
        {
            storage.GenerateDailySchedule(
                simulationConfig.maintenanceStartHour, 
                simulationConfig.maintenanceStartMinute, 
                simulationConfig.maintenanceEndHour, 
                simulationConfig.maintenanceEndMinute, 
                unfinishedFlights
            );
            Console.WriteLine($"[ATC] Generated schedule for tomorrow");
        }

        
        private async Task WaitForAllFlightsCompleted()
        {
            while (true)
            {
                bool queueEmpty;
                queueEmpty = landingQueue.Count == 0 && takeoffQueue.Count == 0;
                if (queueEmpty && runwayManager.AllRunwayEmpty())
                {
                    Console.WriteLine($"[ATC] All flight completed !");
                    break;
                }
                await Task.Delay(500);
            }
        }


        public bool IsMaintenanceMode()
        {
            return maintenanceMode;
        }

        
        public Runway[] GetRunways()
        {
            return runwayManager.GetRunways();
        }
    }
}
