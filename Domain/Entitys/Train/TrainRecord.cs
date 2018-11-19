using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys.Train
{
    public enum TimetableType
    {
        [Description("Дополнительное")]
        Extra,
        [Description("Основное")]
        Main
    }
    public static class TimetableTypeExtensions
    {
        public static string ToStringTimetableType(this Enum enumerate)
        {
            var type = enumerate.GetType();
            var fieldInfo = type.GetField(enumerate.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : enumerate.ToString();
        }
    }

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
