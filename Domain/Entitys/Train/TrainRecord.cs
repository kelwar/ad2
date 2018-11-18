using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys.Train
{
    public class StationDateTime
    {
        public DateTime ArrivalDateTime { get; set; }
        public DateTime DepartureDateTime { get; set; }

        public DateTime ActualArrivalDateTime { get; set; }
        public DateTime ActualDepartureDateTime { get; set; }
    }

    public class TimetableDate
    {
        public DateTime StartTimetableDate { get; set; }
        public DateTime EndTimetableDate { get; set; }
    }

    public class TrainRecord
    {
        public TrainId TrainId { get; set; }
        public TrainNumber TrainNumber { get; set; }
        public StationDateTime StationDateTime { get; set; }
    }
}
