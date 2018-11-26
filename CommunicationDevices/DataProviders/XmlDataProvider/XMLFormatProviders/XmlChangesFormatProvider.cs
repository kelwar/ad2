using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using CommunicationDevices.Settings.XmlDeviceSettings.XmlSpecialSettings;
using Library.Convertion;
using Domain.Entitys;
using Library.Logs;



namespace CommunicationDevices.DataProviders.XmlDataProvider.XMLFormatProviders
{

    //<? xml version="1.0" encoding="utf-8" standalone="yes"?>
    //<changes>
    //  <t>
    //    <TimeStamp>2017-08-07T19:16:00</TimeStamp>
    //    <UserInfo>Admin</UserInfo>
    //    <CauseOfChange>Admin</CauseOfChange>
    //    <TrainNumber>6309</TrainNumber>
    //    <TrainType>0</TrainType>
    //    <DirectionStation>Ряжское</DirectionStation>
    //    <DirectionStationNew>Ряжское</DirectionStationNew>
    //    <StartStation>Ленинградский</StartStation>
    //    <StartStationNew>Ленинградский</StartStationNew>
    //    <EndStation>Ленинградский</EndStation>
    //    <EndStationNew>Ленинградский</EndStationNew>
    //    <StartStationENG>ЛенинградскийEng</StartStationENG>
    //    <StartStationENGNew>ЛенинградскийEng</StartStationENGNew>
    //    <EndStationENG>ЛенинградскийEng</EndStationENG>
    //    <EndStationENGNew>ЛенинградскийEng</EndStationENGNew>
    //    <StartStationCH>ЛенинградскийCh</StartStationCH>
    //    <StartStationCHNew>ЛенинградскийCh</StartStationCHNew>
    //    <EndStationCH>ЛенинградскийCh</EndStationCH>
    //    <EndStationCHNew>ЛенинградскийCh</EndStationCHNew>
    //    <InDateTime>2017-08-07T19:16:00</InDateTime>
    //    <InDateTimeNew>2017-08-07T19:16:00</InDateTimeNew>
    //    <HereDateTime></HereDateTime>
    //    <HereDateTimeNew></HereDateTimeNew>
    //    <OutDateTime></OutDateTime>
    //    <OutDateTimeNew></OutDateTimeNew>
    //    <LateTime></LateTime>
    //    <LateTimeNew></LateTimeNew>
    //    <TrackNumber></TrackNumber>
    //    <TrackNumberNew>1</TrackNumberNew>
    //    <Direction>0</Direction>
    //    <VagonDirection>0</VagonDirection>
    //    <VagonDirectionNew>0</VagonDirectionNew>
    //    <Enabled>1</Enabled>
    //    <EnabledNew>1</EnabledNew>
    //    <Note></Note>
    //    <NoteNew></NoteNew>
    //  </t>
    //</changes>


    public class XmlChangesFormatProvider : IFormatProvider
    {
        private readonly DateTimeFormat _dateTimeFormat;





        public XmlChangesFormatProvider(DateTimeFormat dateTimeFormat)
        {
            _dateTimeFormat = dateTimeFormat;
        }





        public string CreateDoc(IEnumerable<UniversalInputType> tables)
        {
            var universalInputTypes = tables as IList<UniversalInputType> ?? tables.ToList();
            if (!universalInputTypes.Any())
                return null;

            var xDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement("changes"));
            for (var i = 0; i < universalInputTypes.Count; i+=2)
            {
                var uit = universalInputTypes[i];
                var uitNew = universalInputTypes[i+1];

                string trainType;
                string typeName ;
                string typeNameShort;
                GetTypeTrain(uit.TypeTrain, out trainType, out typeName, out typeNameShort);


                string timeArrival;
                string timeDepart;
                byte direction;
                GetTypeEvent(uit, out timeArrival, out timeDepart, out direction);

                string timeArrivalNew;
                string timeDepartNew;
                byte directionNew;
                GetTypeEvent(uitNew, out timeArrivalNew, out timeDepartNew, out directionNew);


                var lateTime = uit.ВремяЗадержки?.ToString("mm:ss") ?? string.Empty;
                var lateTimeNew = uitNew.ВремяЗадержки?.ToString("mm:ss") ?? string.Empty;
                
                var stopTime = uit.StopTime.HasValue ?
                               uit.StopTime.Value.ToString("hh\\:mm") : 
                               string.Empty;
                var stopTimeNew = uitNew.StopTime.HasValue ?
                                  uitNew.StopTime.Value.ToString("hh\\:mm") : 
                                  string.Empty;

                // Время изменения
                string timeStamp = string.Empty;
                string timeStampNew = string.Empty;
                switch (_dateTimeFormat)
                {
                    case DateTimeFormat.None:
                    case DateTimeFormat.Sortable:
                        timeStamp = uit.ViewBag.ContainsKey("TimeStamp") ? ((DateTime)uit.ViewBag["TimeStamp"]).ToString("s") : string.Empty;
                        timeStampNew = uitNew.ViewBag.ContainsKey("TimeStamp") ? ((DateTime)uitNew.ViewBag["TimeStamp"]).ToString("s") : string.Empty;
                        break;

                    case DateTimeFormat.LinuxTimeStamp:
                        timeStamp = uit.ViewBag.ContainsKey("TimeStamp") ? DateTimeConvertion.ConvertToUnixTimestamp((DateTime)uit.ViewBag["TimeStamp"]).ToString(CultureInfo.InvariantCulture) : string.Empty;
                        timeStampNew = uitNew.ViewBag.ContainsKey("TimeStamp") ? DateTimeConvertion.ConvertToUnixTimestamp((DateTime)uitNew.ViewBag["TimeStamp"]).ToString(CultureInfo.InvariantCulture) : string.Empty;
                        break;
                }

                var userInfo = uit.ViewBag.ContainsKey("UserInfo") ? uit.ViewBag["UserInfo"] : string.Empty;
                var userInfoNew = uitNew.ViewBag.ContainsKey("UserInfo") ? uitNew.ViewBag["UserInfo"] : string.Empty;
                var causeOfChange = uit.ViewBag.ContainsKey("CauseOfChange") ? uit.ViewBag["CauseOfChange"] : string.Empty;
                var causeOfChangeNew = uitNew.ViewBag.ContainsKey("CauseOfChange") ? uitNew.ViewBag["CauseOfChange"] : string.Empty;


                Composition composition = null;
                XElement[] xVagons = null;
                try
                {
                    if (uit.ViewBag != null && uit.ViewBag.ContainsKey("Composition"))
                        composition = uit.ViewBag["Composition"];

                    if (composition != null && composition.Vagons != null && composition.Vagons.Any())
                    {
                        var vagons = composition.Vagons;
                        xVagons = new XElement[vagons.Count];
                        for (int j = 0; j < xVagons.Length; j++)
                        {
                            var v = vagons[j];
                            xVagons[i] = new XElement("v", new XElement("NppVag", v.VagonId),
                                                           new XElement("NomPS", v.UniqueVagonId),
                                                           new XElement("NomVagShem", v.VagonNumber),
                                                           new XElement("TipPS", (int)v.PsType),
                                                           new XElement("TipVag", (int)v.VagonType),
                                                           new XElement("Dlina", v.Length),
                                                           new XElement("NomVagShemAsupv", v.VagonNumberAsupv),
                                                           new XElement("NomVagShemAsoup", v.VagonNumberAsoup));
                        }
                    }
                    else
                    {
                        xVagons = new XElement[]
                        {
                            new XElement("v", "")
                        };
                    }
                }
                catch (Exception ex)
                {
                    Log.log.Fatal($"Ошибка при добавлении Состава поезда в XML {ex.Message}");
                }

                Pathways track = null;
                XElement[] xSectors = null;
                try
                {
                    track = uit.Track;

                    if (track != null && track.Platform != null && track.Platform.Sectors != null && track.Platform.Sectors.Any())
                    {
                        var sectors = track?.Platform?.Sectors;
                        xSectors = new XElement[sectors.Count];
                        for (int j = 0; j < xSectors.Length; j++)
                        {
                            var s = sectors[j];
                            xSectors[j] = new XElement("sector", new XElement("name", s.Name),
                                                                 new XElement("color", s.Color),
                                                                 new XElement("length", s.Length.ToString()),
                                                                 new XElement("Offset", s.Offset.ToString()));
                        }
                    }
                    else
                    {
                        xSectors = new XElement[]
                        {
                            new XElement("sector", "")
                        };
                    }
                }
                catch (Exception ex)
                {

                    Log.log.Fatal($"Ошибка при добавлении Платформы в XML { ex.Message}");
                }

                try
                {
                    xDoc.Root?.Add(

                        new XElement("t",
                        new XElement("TimeStamp", timeStamp ?? string.Empty),
                        new XElement("TimeStampNew", timeStampNew ?? string.Empty),
                        new XElement("UserInfo", userInfo ?? string.Empty),
                        new XElement("UserInfoNew", userInfoNew ?? string.Empty),
                        new XElement("CauseOfChange", causeOfChange ?? string.Empty),
                        new XElement("CauseOfChangeNew", causeOfChangeNew ?? string.Empty),
                        new XElement("TrnId", uit?.TrnId ?? 0),
                        new XElement("TrainNumber", uit?.NumberOfTrain ?? string.Empty),
                        new XElement("TrainType", trainType ?? string.Empty),

                        new XElement("DirectionStation", uit?.DirectionStation ?? string.Empty),
                        new XElement("DirectionStationNew", uitNew?.DirectionStation ?? string.Empty),

                        new XElement("StartStation", uit?.StationDeparture?.NameRu ?? string.Empty),
                        new XElement("StartStationNew", uitNew?.StationDeparture?.NameRu ?? string.Empty),
                        new XElement("EndStation", uit?.StationArrival?.NameRu ?? string.Empty),
                        new XElement("EndStationNew", uitNew?.StationArrival?.NameRu ?? string.Empty),

                        new XElement("StartStationENG", uit?.StationDeparture?.NameEng ?? string.Empty),
                        new XElement("StartStationENGNew", uitNew?.StationDeparture?.NameEng ?? string.Empty),
                        new XElement("EndStationENG", uit?.StationArrival?.NameEng ?? string.Empty),
                        new XElement("EndStationENGNew", uitNew?.StationArrival?.NameEng ?? string.Empty),

                        new XElement("StartStationCH", uit?.StationDeparture?.NameCh ?? string.Empty),
                        new XElement("StartStationCHNew", uitNew?.StationDeparture?.NameCh ?? string.Empty),
                        new XElement("EndStationCH", uit?.StationArrival?.NameCh ?? string.Empty),
                        new XElement("EndStationCHNew", uitNew?.StationArrival?.NameCh ?? string.Empty),

                        new XElement("WhereFrom", uit?.StationDeparture?.NearestStation ?? string.Empty),
                        new XElement("WhereTo", uit?.StationArrival?.NearestStation ?? string.Empty),


                        new XElement("InDateTime", timeArrival ?? string.Empty),                   //время приб
                        new XElement("InDateTimeNew", timeArrivalNew ?? string.Empty),             //
                        new XElement("HereDateTime", stopTime ?? string.Empty),                    //время стоянки
                        new XElement("HereDateTimeNew", stopTimeNew ?? string.Empty),              //
                        new XElement("OutDateTime", timeDepart ?? string.Empty),                   //время отпр
                        new XElement("OutDateTimeNew", timeDepartNew ?? string.Empty),             //

                        new XElement("LateTime", lateTime ?? string.Empty),                       //время задержки
                        new XElement("LateTimeNew", lateTimeNew ?? string.Empty),                 //время задержки

                        new XElement("TrackNumber", uit?.PathNumber ?? string.Empty),
                        new XElement("TrackNumberNew", uitNew?.PathNumber ?? string.Empty),
                        new XElement("Platform", uit?.Track?.Platform?.Name ?? string.Empty),
                        new XElement("PlatformNew", uitNew?.Track?.Platform?.Name ?? string.Empty),

                        new XElement("Direction", direction),

                        new XElement("VagonDirection", (byte)uit.VagonDirection),
                        new XElement("VagonDirectionNew", (byte)uitNew.VagonDirection),

                        new XElement("Enabled", (uit.EmergencySituation & 0x01) == 0x01 ? 0 : 1),
                        new XElement("EnabledNew", (uitNew.EmergencySituation & 0x01) == 0x01 ? 0 : 1),

                        new XElement("Note", uit?.Note ?? string.Empty),                               //станции следования
                        new XElement("NoteEng", uit?.NoteEng ?? string.Empty),
                        new XElement("NoteNew", uitNew?.Note ?? string.Empty),
                        new XElement("NoteNewEng", uitNew?.NoteEng ?? string.Empty),

                        new XElement("Kol_Vag", composition?.VagonCount ?? 0),
                        new XElement("Kol_Lok", composition?.LocomotiveCount ?? 0),
                        new XElement("UslDlPoezd", composition?.Length ?? 0),
                        new XElement("Vagons", xVagons),

                        new XElement("UslDlPerrona", track?.Platform?.Length ?? 0),
                        new XElement("PlatWhereFrom", track?.Platform?.WhereFrom?.NameRu ?? string.Empty),
                        new XElement("PlatWhereTo", track?.Platform?.WhereTo?.NameRu ?? string.Empty),
                        new XElement("Sectors", xSectors)
                    ));
                }
                catch (Exception ex)
                {
                    Log.log.Error(ex);
                }
            }

            //DEBUG------------------------
            //string path = Application.StartupPath + @"/StaticTableDisplay" + @"/xDocChanges.info";
            //xDoc.Save(path);
            //-----------------------------

            return xDoc.ToString();
        }



        private void GetTypeTrain(TypeTrain typeTrain, out string typeTrainStr, out string typeNameStr, out string typeNameShortStr)
        {
            typeTrainStr = String.Empty;
            typeNameStr = String.Empty;
            typeNameShortStr = String.Empty;
            switch (typeTrain)
            {
                case TypeTrain.None:
                    typeTrainStr = String.Empty;
                    typeNameStr = String.Empty;
                    break;

                case TypeTrain.Suburban:
                    typeTrainStr = "0";
                    typeNameStr = "Пригородный";
                    typeNameShortStr = "приг";
                    break;

                case TypeTrain.Express:
                    typeTrainStr = "1";
                    typeNameStr = "Экспресс";
                    typeNameShortStr = "экспресс";
                    break;

                case TypeTrain.HighSpeed:
                    typeTrainStr = "2";
                    typeNameStr = "Скорый";
                    typeNameShortStr = "скор";
                    break;

                case TypeTrain.Corporate:
                    typeTrainStr = "3";
                    typeNameStr = "Фирменный";
                    typeNameShortStr = "фирм";
                    break;

                case TypeTrain.Passenger:
                    typeTrainStr = "4";
                    typeNameStr = "Пассажирский";
                    typeNameShortStr = "пасс";
                    break;

                case TypeTrain.Swallow:
                    typeTrainStr = "5";
                    typeNameStr = "Скоростной";
                    typeNameShortStr = "скоростной";
                    break;

                case TypeTrain.Rex:
                    typeTrainStr = "5";
                    typeNameStr = "Скоростной";
                    typeNameShortStr = "скоростной";
                    break;
            }
        }



        private void GetTypeEvent(UniversalInputType uit, out string timeArrival, out string timeDepart, out byte direction)
        {
             timeArrival = string.Empty;
             timeDepart = string.Empty;
             direction = 0;

            switch (uit.Event)
            {
                case "ПРИБ.":
                    switch (_dateTimeFormat)
                    {
                        case DateTimeFormat.None:
                            timeArrival = uit.Time.ToString("s");
                            break;

                        case DateTimeFormat.Sortable:
                            timeArrival = uit.Time.ToString("s");
                            break;

                        case DateTimeFormat.LinuxTimeStamp:
                            timeArrival = DateTimeConvertion.ConvertToUnixTimestamp(uit.Time).ToString(CultureInfo.InvariantCulture);
                            break;
                    }
                    direction = 0;
                    break;

                case "ОТПР.":
                    switch (_dateTimeFormat)
                    {
                        case DateTimeFormat.None:
                            timeDepart = uit.Time.ToString("s");
                            break;

                        case DateTimeFormat.Sortable:
                            timeDepart = uit.Time.ToString("s");
                            break;

                        case DateTimeFormat.LinuxTimeStamp:
                            timeDepart = DateTimeConvertion.ConvertToUnixTimestamp(uit.Time).ToString(CultureInfo.InvariantCulture);
                            break;
                    }
                    direction = 1;
                    break;

                case "СТОЯНКА":
                    switch (_dateTimeFormat)
                    {
                        case DateTimeFormat.None:
                            timeDepart = uit.Time.ToString("s");
                            break;

                        case DateTimeFormat.Sortable:
                            timeArrival = uit.TransitTime != null && uit.TransitTime.ContainsKey("приб") ? uit.TransitTime["приб"].ToString("s") : String.Empty;
                            timeDepart = uit.TransitTime != null && uit.TransitTime.ContainsKey("отпр") ? uit.TransitTime["отпр"].ToString("s") : String.Empty;
                            break;

                        case DateTimeFormat.LinuxTimeStamp:
                            timeArrival = uit.TransitTime != null && uit.TransitTime.ContainsKey("приб") ? DateTimeConvertion.ConvertToUnixTimestamp(uit.TransitTime["приб"]).ToString(CultureInfo.InvariantCulture) : String.Empty;
                            timeDepart = uit.TransitTime != null && uit.TransitTime.ContainsKey("отпр") ? DateTimeConvertion.ConvertToUnixTimestamp(uit.TransitTime["отпр"]).ToString(CultureInfo.InvariantCulture) : String.Empty;
                            break;
                    }
                    direction = 2;
                    break;
            }

        }
     }
}