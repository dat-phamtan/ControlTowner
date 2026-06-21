using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Entity
{
    public class MaintenancePeriod
    {
        public int maintainStartHour;
        public int maintainStartMinute;
        public int mmaintainEndHour;
        public int mmaintainEndMinute;

        public MaintenancePeriod(int maintainStartHour, int maintainStartMinute, int mmaintainEndHour, int mmaintainEndMinute)
        {
            this.maintainStartHour = maintainStartHour;
            this.maintainStartMinute = maintainStartMinute;
            this.mmaintainEndHour = mmaintainEndHour;
            this.mmaintainEndMinute = mmaintainEndMinute;
        }
    }
}
