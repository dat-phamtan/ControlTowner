using ControlTowner.Entity;
using ControlTowner.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Controllers
{
    
    public interface IAirportState
    {
        void HandleLanding(Flight commingFlight, ILogger logger);
        void HandleTakeoff(Flight takingoffFlight, ILogger logger);
    }

    public class NormalAirportState : IAirportState
    {
        public void HandleLanding(Flight commingFlight, ILogger logger)
        {
            logger?.Log($"[ATC] Confirmed landing ! Flight: {commingFlight.Code}");
        }

        public void HandleTakeoff(Flight takingoffFlight, ILogger logger)
        {
            logger?.Log($"[ATC] Confirmed take off ! Flight: {takingoffFlight.Code}");
        }
    }

    public class MaintenanceAirportState : IAirportState
    {
        public void HandleLanding(Flight commingFlight, ILogger logger)
        {
            logger?.Log($"[ATC-MAINTAIN] Landing completed ! Flight: {commingFlight.Code}");
        }

        public void HandleTakeoff(Flight takingoffFlight, ILogger logger)
        {
            logger?.Log($"[ATC-MAINTAIN] Taking off completed ! Flight: {takingoffFlight.Code}");
        }
    }
}
