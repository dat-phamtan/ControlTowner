using ControlTowner.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    
    public interface IAirportState
    {
        void HandleLanding(Flight commingFlight);
        void HandleTakeoff(Flight takingoffFlight);
    }

    public class NormalAirportState : IAirportState
    {
        public void HandleLanding(Flight commingFlight)
        {
            Console.WriteLine($"[ATC] Confirmed landing ! Flight: {commingFlight.Code}");
        }

        public void HandleTakeoff(Flight takingoffFlight)
        {
            Console.WriteLine($"[ATC] Confirmed take off ! Flight: {takingoffFlight.Code}");
        }
    }

    public class MaintenanceAirportState : IAirportState
    {
        public void HandleLanding(Flight commingFlight)
        {
            Console.WriteLine($"[ATC-MAINTAIN] Landing completed ! Flight: {commingFlight.Code}");
        }

        public void HandleTakeoff(Flight takingoffFlight)
        {
            Console.WriteLine($"[ATC-MAINTAIN] Taking off completed ! Flight: {takingoffFlight.Code}");
        }
    }
}
