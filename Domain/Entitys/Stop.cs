using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys
{
    public enum StopState { Stop = 0, NonStop = 1, TechNonStop = 3 }

    public class Stop
    {
        public Station Station { get; set; }
        public StopState StopState { get; set; }
        
        public Stop(Station station, StopState stopState)
        {
            Station = station;
            StopState = stopState;
        }
    }
}
