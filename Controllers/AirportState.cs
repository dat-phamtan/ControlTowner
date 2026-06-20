using ControlTowner.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    
    public interface IAirportState
    {
        void HandleLanding(Controller controller, Flight commingFlight);
        void HandleTakeoff(Controller controller, Flight takingoffFlight);
    }

    public class NormalAirportState : IAirportState
    {
        public void HandleLanding(Controller controller, Flight commingFlight)
        {
            
        }

        public void HandleTakeoff(Controller controller, Flight takingoffFlight)
        {
            // i will imple
        }
    }

    public class MaintenanceAirportState : IAirportState
    {
        public void HandleLanding(Controller controller, Flight commingFlight)
        {
            // i will imple
        }

        public void HandleTakeoff(Controller controller, Flight takingoffFlight)
        {
            // i will imple
        }
    }
}
