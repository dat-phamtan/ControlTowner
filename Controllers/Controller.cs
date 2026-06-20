using ControlTowner.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using ControlTowner.Config;
using ControlTowner.Utility;

namespace ControlTowner.Controllers
{
    public class Controller(int runwayCount, float landingDuration, float takeoffDuration)
    {
        private readonly Queue<Flight> takeoffQueue = new();
        private readonly Queue<Flight> landingQueue = new();

        private RunwayManager runwayManager;
        private IAirportState currentState;
        private ILandingGenerator flightGenerator;
        private bool isDayFinalized = false;

        public void Init()
        {
            runwayManager = new RunwayManager(runwayCount);
            currentState = new NormalAirportState();

            SimpleClock.Instance.OnMaintenanceStart += HandleMaintenanceStart;
            SimpleClock.Instance.OnNewDayStart += HandleNewDayStart;

            runwayManager.OnBecomeAvailable += ProcessQueues; //maybe better
            //Task.Run((StartDispatchLoop));
        }

        public void RequestLanding()
        {
            Flight? flight = flightGenerator.CheckGenerate(SimpleClock.Instance.SimulatedTime);
            if (flight == null) return;
      
            landingQueue.Enqueue(flight);
            ProcessQueues();
        }

        public void RequestTakeoff(Flight flight)
        {
            takeoffQueue.Enqueue(flight);
            ProcessQueues();
        }

        public void ProcessQueues()
        {
            Runway? runway = runwayManager.GetAvailableRunway();
            if (runway == null) return;
            if (landingQueue.Count == 0 && takeoffQueue.Count == 0) return;
            

            //need refactor
            if (landingQueue.TryDequeue(out Flight? landingFlight))
            {
                if (runway.AssignFlight(landingFlight))
                {
                    runway.RealDuration = landingDuration;
                    ExecuteFlight(runway, landingFlight);
                }
            }
            else if (takeoffQueue.TryDequeue(out Flight? takeoffFlight))
            {
                if (runway.AssignFlight(takeoffFlight))
                {
                    runway.RealDuration = takeoffDuration;
                    ExecuteFlight(runway, takeoffFlight);
                }
            }
        }


        private void ExecuteFlight(Runway runway, Flight flight)
        {
            //logger
            flight.OnActionCompleted += (completedFlight) =>
            {
                runway.Free();
                //logger
            };

            if (flight.Type == FlightType.Landing) currentState.HandleLanding(this, flight);
            else currentState?.HandleTakeoff(this, flight);
        }

        public void FinalizeDayAndPrepareNext()
        {
            if (!isDayFinalized) return;
            isDayFinalized = true;
            //logger
            //generate new plan 4 tommorow
        }

        public bool AreAllRunwayEmpty()
        {
            return runwayManager.AllRunwayEmpty();
        }

        private void HandleMaintenanceStart()
        {
            Console.WriteLine("Maintenance");
            currentState = new MaintenanceAirportState();
        }

        private void HandleNewDayStart()
        {
            Console.WriteLine("Start new day");
            isDayFinalized = false;
            currentState = new NormalAirportState();
        }
    }
}
