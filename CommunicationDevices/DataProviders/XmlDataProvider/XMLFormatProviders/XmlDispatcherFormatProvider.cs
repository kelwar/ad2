using CommunicationDevices.Settings.XmlDeviceSettings.XmlSpecialSettings;
using Library.Convertion;
using Library.Logs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommunicationDevices.DataProviders.XmlDataProvider.XMLFormatProviders
{
    public class XmlDispatcherFormatProvider : IFormatProvider
    {
        private readonly DateTimeFormat _dateTimeFormat;
        private readonly TransitSortFormat _transitSortFormat;
        
        public XmlDispatcherFormatProvider(DateTimeFormat dateTimeFormat, TransitSortFormat transitSortFormat)
        {
            _dateTimeFormat = dateTimeFormat;
            _transitSortFormat = transitSortFormat;
        }

        public string CreateDoc(IEnumerable<UniversalInputType> tables)
        {
            if (tables == null || !tables.Any())
                return null;

            var xDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement("tlist"));
            foreach (var uit in tables)
            {
                try
                {
                    xDoc.Root?.Add(
                            new XElement("t",
                            new XElement("ScheduleId", uit.ScheduleId),
                            new XElement("TrnId", uit.TrnId),
                            new XElement("TrainNumber", uit.NumberOfTrain),
                            new XElement("TrainType", GetTypeNumber(uit.TypeTrain)),
                            new XElement("DirectionStation", uit.DirectionStation),

                            new XElement("StartStation", uit.StationDeparture?.NameRu ?? string.Empty),
                            new XElement("EndStation", uit.StationArrival?.NameRu ?? string.Empty),
                            new XElement("StartStationENG", uit.StationDeparture?.NameEng ?? string.Empty),
                            new XElement("EndStationENG", uit.StationArrival?.NameEng ?? string.Empty),
                            new XElement("StartStationCH", uit.StationDeparture?.NameCh ?? string.Empty),
                            new XElement("EndStationCH", uit.StationArrival?.NameCh ?? string.Empty),
                            new XElement("WhereFrom", uit.StationDeparture?.NearestStation ?? string.Empty),
                            new XElement("WhereTo", uit.StationArrival?.NearestStation ?? string.Empty),


                            new XElement("RecDateTime", GetArrivalTimeString(uit)),                //время приб
                            new XElement("SndDateTime", GetDepartureTimeString(uit)),                 //время отпр
                            new XElement("EvRecTime", GetArrivalTimeString(uit)),
                            new XElement("EvSndTime", GetDepartureTimeString(uit)),
                            new XElement("LateTime", GetDelayTime(uit.ВремяЗадержки)),                      //время задержки
                            new XElement("HereDateTime", GetStopTime(uit)),                  //время стоянки
                            new XElement("ExpectedTime", (uit.ExpectedTime == uit.Time) ? string.Empty : uit.ExpectedTime.ToString("HH:mm")),    //ожидаемое время


                            new XElement("DaysOfGoing", uit.DaysFollowing),            //дни след
                            new XElement("DaysOfGoingAlias", uit.DaysFollowingAlias),  //дни след заданные в ручную
                            new XElement("DaysOfGoingAliasEng", uit.DaysFollowingAliasEng),

                            new XElement("TrackNumber", uit.PathNumber),
                            new XElement("TrackNumberWithoutAutoReset", uit.PathNumberWithoutAutoReset),
                            new XElement("Platform", uit.Track?.Platform?.Name ?? string.Empty),
                            new XElement("Direction", GetDirectionString(uit.Event)),
                            new XElement("EvTrackNumber", uit.PathNumber),
                            new XElement("State", 0),
                            new XElement("VagonDirection", (byte)uit.VagonDirection),
                            new XElement("Enabled", uit.IsActive ? 1 : 0), 
                            new XElement("EmergencySituation", uit.EmergencySituation),
                            new XElement("TypeName", GetTypeName(uit.TypeTrain)),
                            new XElement("TypeAlias", GetShortTypeName(uit.TypeTrain)),
                            new XElement("Addition", uit.Addition),
                            new XElement("AdditionEng", uit.AdditionEng),
                            new XElement("Note", uit.Note),
                            new XElement("NoteEng", uit.NoteEng)
                        ));
                }
                catch (Exception ex)
                {
                    Log.log.Error(ex);
                }
            }

            return xDoc.ToString();
        }

        private string GetTypeNumber(TypeTrain trainType)
        {
            switch (trainType)
            {
                case TypeTrain.Suburban:
                    return "0";

                case TypeTrain.Express:
                    return "1";

                case TypeTrain.HighSpeed:
                    return "2";

                case TypeTrain.Corporate:
                    return "3";

                case TypeTrain.Passenger:
                    return "4";

                case TypeTrain.Swallow:
                    return "5";

                case TypeTrain.Rex:
                    return "5";

                default:
                    return string.Empty;
            }
        }

        private string GetTypeName(TypeTrain trainType)
        {
            switch (trainType)
            {
                case TypeTrain.Suburban:
                    return "Пригородный";

                case TypeTrain.Express:
                    return "Скорый";

                case TypeTrain.HighSpeed:
                    return "Скоростной";

                case TypeTrain.Corporate:
                    return "Фирменный";

                case TypeTrain.Passenger:
                    return "Пассажирский";

                case TypeTrain.Swallow:
                    return "Экспресс";

                case TypeTrain.Rex:
                    return "Экспресс";

                default:
                    return string.Empty;
            }
        }

        private string GetShortTypeName(TypeTrain trainType)
        {
            switch (trainType)
            {
                case TypeTrain.Suburban:
                    return "приг";

                case TypeTrain.Express:
                    return "скор";

                case TypeTrain.HighSpeed:
                    return "скорост";

                case TypeTrain.Corporate:
                    return "фирм";

                case TypeTrain.Passenger:
                    return "пасс";

                case TypeTrain.Swallow:
                    return "эксп";

                case TypeTrain.Rex:
                    return "эксп";

                default:
                    return string.Empty;
            }
        }

        private string GetArrivalTimeString(UniversalInputType uit)
        {
            switch (uit.Event)
            {
                case "ПРИБ.":
                    switch (_dateTimeFormat)
                    {
                        case DateTimeFormat.LinuxTimeStamp:
                            return DateTimeConvertion.ConvertToUnixTimestamp(uit.Time).ToString(CultureInfo.InvariantCulture);

                        default:
                            return uit.Time.ToString("s");
                    }

                case "СТОЯНКА":
                    switch (_dateTimeFormat)
                    {
                        case DateTimeFormat.LinuxTimeStamp:
                            return uit.TransitTime != null && uit.TransitTime.ContainsKey("приб") ? DateTimeConvertion.ConvertToUnixTimestamp(uit.TransitTime["приб"]).ToString(CultureInfo.InvariantCulture) : string.Empty;
                            
                        default:
                            return uit.TransitTime != null && uit.TransitTime.ContainsKey("приб") ? uit.TransitTime["приб"].ToString("s") : string.Empty;
                    }

                default:
                    return string.Empty;
            }
        }

        private string GetDepartureTimeString(UniversalInputType uit)
        {
            switch (uit.Event)
            {
                case "ОТПР.":
                    switch (_dateTimeFormat)
                    {
                        case DateTimeFormat.LinuxTimeStamp:
                            return DateTimeConvertion.ConvertToUnixTimestamp(uit.Time).ToString(CultureInfo.InvariantCulture);

                        default:
                            return uit.Time.ToString("s");
                    }

                case "СТОЯНКА":
                    switch (_dateTimeFormat)
                    {
                        case DateTimeFormat.LinuxTimeStamp:
                            return uit.TransitTime != null && uit.TransitTime.ContainsKey("отпр") ? 
                                   DateTimeConvertion.ConvertToUnixTimestamp(uit.TransitTime["отпр"]).ToString(CultureInfo.InvariantCulture) : 
                                   string.Empty;

                        default:
                            return uit.TransitTime != null && uit.TransitTime.ContainsKey("отпр") ? uit.TransitTime["отпр"].ToString("s") : string.Empty;
                    }

                default:
                    return string.Empty;
            }
        }

        private byte GetDirectionString(string stringEvent)
        {
            switch (stringEvent)
            {
                case "ОТПР.":
                    return 1;

                case "СТОЯНКА":
                    return 2;

                default:
                    return 0;
            }
        }

        private string GetDelayTime(DateTime? delayTime)
        {
            var result = delayTime?.ToString("mm:ss") ?? string.Empty;
            if (result == "00:00")
            {
                result = string.Empty;
            }
            return result;
        }

        private string GetStopTime(UniversalInputType uit)
        {
            return uit.StopTime.HasValue ?
                   uit.StopTime.Value.ToString("hh\\:mm") :
                   uit.Event == "СТОЯНКА" ? "Время стоянки будет изменено" : string.Empty;
        }
    }
}
