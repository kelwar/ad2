using Library;
using Library.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys
{
    public enum ConnectionType { LongDistance, Local }

    public class OldTrain : EntityBase
    {
        #region Properties
        public int IdGdp { get; set; }
        public int IdTrn { get; set; }
        public int IdTieTrain { get; set; }
        public string FirstTrainNumber { get; set; }
        public string SecondTrainNumber { get; set; }
        public string Route { get; set; }
        public string StartStation { get; set; }
        public string EndStation { get; set; }
        public string Direction { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public TimeSpan StopTime { get; set; }
        public string TimeTable { get; set; }
        private IEnumerable<DateTime> _timeTable;
        public string TimeTableAlias { get; set; }
        public string TimeTableAliasEng { get; set; }
        public bool Active { get; set; }
        public string SoundTemplates { get; set; }
        public string CarNumbering { get; set; }
        public bool ChangeableCarNumbering { get; set; }
        public string DefaultTracks { get; set; }
        public string TrainType { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public string Note { get; set; }
        public string NoteEng { get; set; }
        public DateTime TimeTableStartDate { get; set; }
        public DateTime TimeTableEndDate { get; set; }
        public string CorpName { get; set; }
        public string CorpNameEng { get; set; }
        public bool CorpNameOnBoard { get; set; }
        public bool CorpNameOnSound { get; set; }
        public bool AutoSoundMode { get; set; }
        public TimeSpan TravelTime { get; set; }
        public bool OnBoard { get; set; }
        public bool OnSound { get; set; }
        #endregion

        public bool IsValid()
        {
            return Id != 0 &&
                   !string.IsNullOrWhiteSpace(FirstTrainNumber) &&
                   !string.IsNullOrWhiteSpace(Route) &&
                   (ArrivalTime != DateTime.MinValue ||
                   DepartureTime != DateTime.MinValue);
        }

        public void ShiftTime(TimeSpan offset)
        {
            try
            {
                if (ArrivalTime != DateTime.MinValue)
                    ArrivalTime += offset;
                if (DepartureTime != DateTime.MinValue)
                    DepartureTime += offset;
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }

        public void CalcStopTime()
        {
            try
            {
                StopTime = ArrivalTime != DateTime.MinValue && DepartureTime != DateTime.MinValue ?
                           DepartureTime < ArrivalTime ? DepartureTime.AddDays(1) - ArrivalTime : DepartureTime - ArrivalTime :
                           TimeSpan.MinValue;
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }

        public void ParseString(string[] data)
        {
            try
            {
                var parser = Parser.GetParser();
                Id = parser.ToInt(data[0]);
                FirstTrainNumber = data[1];
                ConnectionType = FirstTrainNumber.Length < 4 ? ConnectionType.LongDistance : ConnectionType.Local;
                Route = data[2];
                ArrivalTime = parser.ToDateTime(data[3]);
                StopTime = parser.ToTimeSpan(data[4]);
                DepartureTime = parser.ToDateTime(data[5]);
                TimeTable = data[6];
                Active = parser.ToBool(data[7]);
                SoundTemplates = data[8];
                CarNumbering = data[9];
                DefaultTracks = data[10];
                TrainType = data[11];
                Note = data[12];
                TimeTableStartDate = parser.ToDateTime(data[13]).Date;
                TimeTableEndDate = parser.ToDateTime(data[14]).Date;
                CorpName = data[15];
                CorpNameOnBoard = parser.ToBool(data[16]);
                CorpNameOnSound = parser.ToBool(data[17]);
                AutoSoundMode = parser.ToBool(data[18]);
                SecondTrainNumber = data[19];
                TravelTime = parser.ToTimeSpan(data[20]);
                TimeTableAlias = data[21];
                StartStation = data[22];
                EndStation = data[23];
                Direction = data[24];
                ChangeableCarNumbering = parser.ToBool(data[25]);
                OnBoard = parser.ToBool(data[26]);
                OnSound = parser.ToBool(data[27]);
                TimeTableAliasEng = data[28];
                CorpNameEng = data[29];
                NoteEng = data[30];
                IdGdp = parser.ToInt(data[31]);
                IdTrn = parser.ToInt(data[32]);
                IdTieTrain = parser.ToInt(data[33]);
            }
            catch (IndexOutOfRangeException)
            {
                Log.log.Error($"Параметра №{data.Length + 1} для поезда c ID = {Id} {ToShortString()} не существует");
            }
            catch (NullReferenceException ex)
            {
                Log.log.Error(ex);
            }
        }

        public override string ToString()
        {
            var result = string.Empty;
            try
            {
                result = $"{Id};" +
                       $"{FirstTrainNumber};" +
                       $"{Route};" +
                       $"{(ArrivalTime != DateTime.MinValue ? ArrivalTime.ToString("HH:mm") : string.Empty)};" +
                       $"{(StopTime != TimeSpan.MinValue ? StopTime.ToString("hh\\:mm") : string.Empty)};" +
                       $"{(DepartureTime != DateTime.MinValue ? DepartureTime.ToString("HH:mm") : string.Empty)};" +
                       $"{TimeTable};" +
                       $"{(Active ? "1" : "0")};" +
                       $"{SoundTemplates};" +
                       $"{CarNumbering};" +
                       $"{DefaultTracks};" +
                       $"{TrainType};" +
                       $"{Note};" +
                       $"{TimeTableStartDate.ToString("dd.MM.yyyy HH:mm:ss")};" +
                       $"{TimeTableEndDate.ToString("dd.MM.yyyy HH:mm:ss")};" +
                       $"{CorpName};" +
                       $"{(CorpNameOnBoard ? "1" : "0")};" +
                       $"{(CorpNameOnSound ? "1" : "0")};" +
                       $"{(AutoSoundMode ? "1" : "0")};" +
                       $"{SecondTrainNumber};" +
                       $"{TravelTime};" +
                       $"{TimeTableAlias};" +
                       $"{StartStation};" +
                       $"{EndStation};" +
                       $"{Direction};" +
                       $"{ChangeableCarNumbering};" +
                       $"{OnBoard};" +
                       $"{OnSound};" +
                       $"{TimeTableAliasEng};" +
                       $"{CorpNameEng};" +
                       $"{NoteEng};" +
                       $"{IdGdp};" +
                       $"{IdTrn};" +
                       $"{IdTieTrain}";
            }
            catch (Exception ex)
            {
                Log.log.Fatal(ex);
            }
            return result;
        }

        private string ToShortString()
        {
            return $"{(!string.IsNullOrWhiteSpace(SecondTrainNumber) ? $"{FirstTrainNumber}/{SecondTrainNumber}" : FirstTrainNumber)} " +
                   $"{Route} " +
                   $"{(ArrivalTime != DateTime.MinValue ? $"ПРИБ. {ArrivalTime.ToString("HH:mm")} " : string.Empty)}" +
                   $"{(DepartureTime != DateTime.MinValue ? $"ОТПР. {DepartureTime.ToString("HH:mm")} " : string.Empty)}";
        }
    }
}
