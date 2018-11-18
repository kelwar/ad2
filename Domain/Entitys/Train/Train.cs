using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys.Train
{
    public class TrainId
    {
        public int IdGdp { get; set; }
        public int IdTrn { get; set; }
    }


    public class TrainTime
    {
    }

    public class TrainSchedule
    {
    }

    public class Train
    {
        public TrainId TrainId { get; set; }
        public TrainNumber TrainNumber { get; set; }
        public List<TrainSchedule> TrainSchedules { get; set; }
        public string CorpName { get; set; }
    }
}
