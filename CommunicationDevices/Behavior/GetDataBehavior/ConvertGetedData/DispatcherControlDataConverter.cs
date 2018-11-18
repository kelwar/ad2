using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CommunicationDevices.DataProviders;
using Domain.Entitys;
using Library.Logs;
using System.Globalization;
using Library;

namespace CommunicationDevices.Behavior.GetDataBehavior.ConvertGetedData
{

    //<? xml version="1.0" encoding="UTF-8"?>
    //<tlist>
    //<t>
    //<TrainNumber1>014</TrainNumber1>                //номер поезда до / (либо полный, если слэша не было)
    //<TrainNumber2>013</TrainNumber2>                //номер поезда после / (либо пустота, если слэша не было)
    //<StartStation>САРАТОВ</StartStation>            //станция отправки
    //<EndStation>АДЛЕР</EndStation>                  //станция прибытия
    //<RecDateTime>2017-10-01T16:09:00</RecDateTime>  //время прибытия - может быть изменено диспетчером
    //<SndDateTime>2017-10-01T16:54:00</SndDateTime>  //время отправления - может быть изменено диспетчером
    //<TrackNumber>1</TrackNumber>                    //путь - может быть изменено диспетчером
    //</t>
    //<t>
    //<TrainNumber1>368</TrainNumber1>
    //<TrainNumber2>367</TrainNumber2>
    //<StartStation>КИСЛОВОДСК</StartStation>
    //<EndStation>КИРОВ</EndStation>
    //<RecDateTime>2017-10-01T15:15:00</RecDateTime>
    //<SndDateTime>2017-10-01T15:55:00</SndDateTime>
    //<TrackNumber/>
    //</t>
    //<t>
    //<TrainNumber1>6805</TrainNumber1>
    //<TrainNumber2/>
    //<StartStation>ВОЛГОГРАД</StartStation>
    //<EndStation>АРЧЕДА</EndStation>
    //<RecDateTime/>
    //<SndDateTime>2017-10-01T17:23:00</SndDateTime>
    //<TrackNumber/>
    //</t>
    //</tlist>

    public class DispatcherControlDataConverter : IInputDataConverter
    {
        public IEnumerable<UniversalInputType> ParseXml2Uit(XDocument xDoc)
        {
            //Log.log.Trace("xDoc" + xDoc.ToString());//LOG;

            var shedules = new List<UniversalInputType>();
            
            #region Set datatype and root element
            var inDataType = InDataType.None;
            List<XElement> lines = null;
            if (xDoc.Element("command") != null)
            {
                inDataType = InDataType.Command;
                lines = xDoc.Element("command")?.Elements().ToList();
            }
            else if (xDoc.Element("tlist") != null)
            {
                inDataType = InDataType.Trains;
                lines = xDoc.Element("tlist")?.Elements().ToList();
            }
            #endregion
            
            try
            {
                if (lines != null)
                {
                    var parser = Parser.GetParser();
                    switch (inDataType)
                    {
                        case InDataType.Command:
                            var data = new UniversalInputType
                            {
                                ViewBag = new Dictionary<string, dynamic>(),
                                TransitTime = new Dictionary<string, DateTime>()
                            };
                            data.InDataType = inDataType;

                            var cmd = new Dictionary<string, bool>();
                            foreach (var line in lines)
                            {
                                switch (line.Name.LocalName)
                                {
                                    case "RESET":
                                        cmd[line.Name.LocalName] = parser.ToBool(line.Value);
                                        break;
                                    case "arrival":
                                    case "landing":
                                    case "departure":
                                        cmd[line.Name.LocalName] = true;
                                        var numberOfTrain1 = StringTrim(line, "TrainNumber1");
                                        var numberOfTrain2 = StringTrim(line, "TrainNumber2");
                                        data.NumberOfTrain =
                                            (string.IsNullOrEmpty(numberOfTrain2) || string.IsNullOrWhiteSpace(numberOfTrain2))
                                                ? numberOfTrain1
                                                : (numberOfTrain1 + "/" + numberOfTrain2);
                                        data.StationDeparture = new Station
                                        {
                                            NameRu = StringTrim(line, "StartStation")
                                        };

                                        data.StationArrival = new Station
                                        {
                                            NameRu = StringTrim(line, "EndStation")
                                        };
                                        data.TransitTime["приб"] = parser.ToDateTime(StringTrim(line, "RecDateTime"));
                                        data.TransitTime["отпр"] = parser.ToDateTime(StringTrim(line, "SndDateTime"));
                                        data.PathNumber = StringTrim(line, "TrackNumber");
                                        var dtLate = parser.ToDateTime(StringTrim(line, "LateTime"), "mm:ss");
                                        if (dtLate != DateTime.MinValue)
                                            data.ВремяЗадержки = dtLate;
                                        data.EmergencySituation = parser.ToByte(StringTrim(line, "EmergencySituation"));
                                        break;
                                }
                            }
                            data.ViewBag["Command"] = cmd;
                            shedules.Add(data);
                            break;

                        case InDataType.Trains:
                            for (var i = 0; i < lines.Count; i++)
                            {
                                var line = lines[i];

                                var uit = new UniversalInputType
                                {
                                    ViewBag = new Dictionary<string, dynamic>(),
                                    TransitTime = new Dictionary<string, DateTime>()
                                };
                                uit.InDataType = inDataType;

                                try
                                {
                                    var numberOfTrain1 = StringTrim(line, "TrainNumber1");
                                    var numberOfTrain2 = StringTrim(line, "TrainNumber2");
                                    uit.NumberOfTrain =
                                        (string.IsNullOrEmpty(numberOfTrain2) || string.IsNullOrWhiteSpace(numberOfTrain2))
                                            ? numberOfTrain1
                                            : (numberOfTrain1 + "/" + numberOfTrain2);
                                    
                                    uit.StationDeparture = new Station
                                    {
                                        NameRu = StringTrim(line, "StartStation")
                                    };

                                    uit.StationArrival = new Station
                                    {
                                        NameRu = StringTrim(line, "EndStation")
                                    };
                                    
                                    uit.TransitTime["приб"] = parser.ToDateTime(StringTrim(line, "RecDateTime"));
                                    uit.TransitTime["отпр"] = parser.ToDateTime(StringTrim(line, "SndDateTime"));
                                    
                                    uit.PathNumber = StringTrim(line, "TrackNumber");
                                    switch (uit.PathNumber)
                                    {
                                        case "11":
                                            uit.PathNumber = "1приг";
                                            break;
                                        case "12":
                                            uit.PathNumber = "3приг";
                                            break;
                                        case "13":
                                            uit.PathNumber = "2приг";
                                            break;
                                    }
                                    
                                    var dtLate = parser.ToDateTime(StringTrim(line, "LateTime"), "mm:ss");
                                    if (dtLate != DateTime.MinValue)
                                        uit.ВремяЗадержки = dtLate;
                                    
                                    var stopTime = parser.ToTimeSpan(StringTrim(line, "HereDateTime"));
                                    if (stopTime != TimeSpan.MinValue)
                                        uit.StopTime = stopTime;
                                    
                                    uit.EmergencySituation = parser.ToByte(StringTrim(line, "EmergencySituation"));
                                    uit.IsActive = parser.ToBool(StringTrim(line, "Enabled"));

                                    shedules.Add(uit);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Ошибка: {ex}");
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }

            return shedules;
        }
        private string StringTrim(XElement line, string s)
        {
            return Regex.Replace((line?.Element(s)?.Value.Replace("\\", "/") ?? string.Empty).TrimEnd(), "[\r\n\t]+", "");
        }
        private string StringTrimToTitleCase(XElement line, string s)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(StringTrim(line, s).ToLower());
        }
    }
}