using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunicationDevices.DataProviders;
using Domain.Entitys;
using MoreLinq;
using System.Globalization;
using Library;

namespace CommunicationDevices.Behavior.GetDataBehavior.ConvertGetedData
{
    class CisRegularShDataConverter : IInputDataConverter
    {
        private CisUsersDbDataConverter _cisUsersDbDataConverter;
        private CisCarNavigationDataConverter _cisCarNavigationDataConverter;
        

        public IEnumerable<UniversalInputType> ParseXml2Uit(XDocument xDoc)
        {
            //Log.log.Trace("xDoc" + xDoc.ToString());//LOG;
            var shedules = new List<UniversalInputType>();

            #region Set datatype and root element
            InDataType inDataType = InDataType.None;
            List<XElement> lines = null;
            if (xDoc.Element("changes") != null)
            {
                inDataType = InDataType.Trains;
                lines = xDoc.Element("changes")?.Elements().ToList();
            }
            else if (xDoc.Element("users") != null)
            {
                //if (_cisUsersDbDataConverter == null)
                //    _cisUsersDbDataConverter = new CisUsersDbDataConverter();
                //return _cisUsersDbDataConverter.ParseXml2Uit(xDoc);
                inDataType = InDataType.Users;
                lines = xDoc.Element("users")?.Elements("user")?.ToList();
            }
            else if (xDoc.Element("trains") != null)
            {
                //if (_cisCarNavigationDataConverter == null)
                //    _cisCarNavigationDataConverter = new CisCarNavigationDataConverter();
                //return _cisCarNavigationDataConverter.ParseXml2Uit(xDoc);
                inDataType = InDataType.Vagons;
                lines = xDoc.Element("trains")?.Elements("train")?.ToList();
            }
            #endregion

            if (lines != null)
            {
                var parser = Parser.GetParser();
                for (var i = 0; i < lines.Count; i++)
                //foreach (var line in lines)
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
                        switch (uit.InDataType)
                        {
                            #region Trains
                            case InDataType.Trains:

                                uit.ScheduleId = parser.ToInt(StringTrim(line, "ID"));

                                //TransitTime["приб"]-----
                                var prib = StringTrim(line, "InDateTime");
                                DateTime dtPrib;
                                if (prib != "-1" && DateTime.TryParse(prib, out dtPrib))
                                {
                                    //DateTime.TryParse(ParseIntTimeToTextTime(prib), out dtPrib);
                                    //DateTime.TryParseExact(prib, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtPrib);
                                    dtPrib = TimeZoneInfo.ConvertTime(dtPrib, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), TimeZoneInfo.Local);
                                }
                                else
                                {
                                    dtPrib = new DateTime();
                                    uit.Event = "ОТПР.";
                                }
                                uit.TransitTime["приб"] = dtPrib;
                                
                                //TransitTime["отпр"]-----
                                var otpr = StringTrim(line, "OutDateTime");
                                DateTime dtOtpr;
                                if (otpr != "-1" && DateTime.TryParse(otpr, out dtOtpr))
                                {
                                    //DateTime.TryParse(ParseIntTimeToTextTime(otpr), out dtOtpr);
                                    //DateTime.TryParseExact(otpr, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtOtpr);
                                    dtOtpr = TimeZoneInfo.ConvertTime(dtOtpr, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), TimeZoneInfo.Local);
                                }
                                else
                                {
                                    dtOtpr = new DateTime();
                                    uit.Event = "ПРИБ.";
                                }
                                uit.TransitTime["отпр"] = dtOtpr;

                                uit.DaysFollowing = dtOtpr != DateTime.MinValue ? dtOtpr.Date.ToString("yyyy.MM.dd") : dtPrib.ToString("yyyy.MM.dd");

                                var num1 = StringTrim(line, "Num1");
                                var num2 = StringTrim(line, "Num2");

                                if (num1.Length < 4 && num2.Length < 4)
                                {
                                    uit.NumberOfTrain = prib == "-1" ? num2 :
                                                            otpr == "-1" ? num1 :
                                                            num1 != num2 ? $"{num1}/{num2}" :
                                                            num2;
                                }
                                else
                                {
                                    uit.NumberOfTrain = string.IsNullOrWhiteSpace(num2) || otpr == "-1" ?
                                                        num1 : num2;
                                }
                                
                                var str = StringTrim(line, "Date").Split('-');
                                var dayStr = string.Empty;
                                if (str.Length == 3)
                                {
                                    dayStr = $"{str[2]}.{str[1]}.{str[0]}";
                                }
                                uit.DaysFollowingAlias = string.Empty;
                                uit.DaysFollowingAliasEng = string.Empty;
                                
                                uit.ViewBag["Enabled"] = StringTrim(line, "Enabled");
                                uit.IsActive = parser.ToBool(uit.ViewBag["Enabled"]);
                                
                                uit.ViewBag["SoundTemplate"] = string.Empty;

                                //TrainType---------
                                ТипПоезда typeOfTrain;
                                var s = StringTrimToTitleCase(line, "Type");
                                if (s == "Экспресс")
                                    s = "Ласточка"; // условие подойдёт не для всех вокзалов, хардкод
                                if (s == "Высокоскоростной")
                                    s = "Скоростной";
                                if (string.IsNullOrWhiteSpace(s))
                                {
                                    s = num1.Length < 4 ? "Пассажирский" : "Пригородный";
                                }

                                if (Enum.TryParse(s, out typeOfTrain))
                                {
                                    uit.TypeTrain = (TypeTrain)typeOfTrain;
                                }

                                //Stops------
                                uit.Note = string.Empty;
                                uit.NoteEng = string.Empty;

                                var trainUnderLines = line?.Element("Stops")?.Elements("stop")?.ToList() ?? null;
                                var route = new Route();
                                if (trainUnderLines != null)
                                {
                                    foreach (var underLine in trainUnderLines)
                                    {
                                        try
                                        {
                                            var stopStation = new Station
                                            {
                                                NameRu = StringTrim(underLine, "name"),
                                                CodeEsr = parser.ToInt(StringTrim(underLine, "esr"))
                                            };

                                            int state;
                                            StopState stopState = StopState.TechNonStop;
                                            if (int.TryParse(StringTrim(underLine, "state"), out state))
                                            {
                                                stopState = (StopState)state;
                                            }
                                            else if (int.TryParse(StringTrim(underLine, "isStop"), out state))
                                            {
                                                stopState = state == 1 ? StopState.Stop : stopStation.CodeEsr != 6023 ? StopState.NonStop : StopState.TechNonStop;
                                            }

                                            route.Stops.Add(parser.ToInt(StringTrim(underLine, "seq")), new Stop
                                            (
                                                stopStation,
                                                stopState
                                            ));
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Ошибка: {ex}");
                                        }
                                    }
                                }
                                uit.ViewBag["Route"] = route;

                                //ScheduleStartDateTime---------------
                                DateTime dtStartDateTime = DateTime.MinValue;
                                //DateTime.TryParse(StringTrim(line, "ScheduleStartDateTime"), out dtStartDateTime);
                                DateTime.TryParse(StringTrim(line, "StartDate"), out dtStartDateTime);
                                uit.ViewBag["ScheduleStartDateTime"] = dtStartDateTime;

                                //ScheduleEndDateTime---------------
                                DateTime dtEndDateTime = new DateTime(2099, 12, 31);
                                //DateTime.TryParse(StringTrim(line, "ScheduleEndDateTime"), out dtEndDateTime);
                                DateTime.TryParse(StringTrim(line, "EndDate"), out dtEndDateTime);
                                uit.ViewBag["ScheduleEndDateTime"] = dtEndDateTime;

                                //Addition---------------
                                //uit.Addition = StringTrimToTitleCase(line, "Addition");
                                uit.Addition = StringTrimToTitleCase(line, "Name");
                                uit.AdditionEng = string.Empty;
                                

                                //ItenaryTime--------------
                                var itenaryTime = StringTrim(line, "ItenaryTime");
                                int intTime;
                                int.TryParse(itenaryTime, out intTime);
                                intTime %= 24 * 60;
                                itenaryTime = intTime / 60 + ":" + (intTime % 60 < 10 ? "0" : string.Empty) + (intTime % 60);
                                //DateTime itenTime;
                                //DateTime.TryParse(itenaryTime, out itenTime);
                                uit.ViewBag["ItenaryTime"] = itenaryTime;

                                //StartStation--------------------
                                //elem = line?.Element("StartStation")?.Value.Replace("\\", "/") ?? string.Empty;
                                int expressStart;
                                int.TryParse(StringTrim(line, "ExpressStart"), out expressStart);

                                int esrStart;
                                int.TryParse(StringTrim(line, "EsrStart"), out esrStart);

                                //EndStation-----------------------
                                int expressEnd;
                                int.TryParse(StringTrim(line, "ExpressEnd"), out expressEnd);

                                //EndStation-----------------------
                                int esrEnd;
                                int.TryParse(StringTrim(line, "EsrEnd"), out esrEnd);

                                //string startStation = Regex.Replace(stationCisToLocal(startCode), "[\r\n\t]+", "");
                                uit.StationDeparture = new Station
                                {
                                    NameRu = StringTrim(line, "StartStation"),
                                    CodeExpress = expressStart,
                                    CodeEsr = esrStart
                                };

                                //string endStation = Regex.Replace(stationCisToLocal(endCode), "[\r\n\t]+", "");
                                uit.StationArrival = new Station
                                {
                                    NameRu = StringTrim(line, "EndStation"),
                                    CodeExpress = expressEnd,
                                    CodeEsr = esrEnd
                                };
                                
                                uit.DirectionStation = string.Empty;
                                break;
                            #endregion

                            #region Users
                            case InDataType.Users:
                                //login------------
                                uit.ViewBag["login"] = StringTrim(line, "login");

                                //password------------
                                uit.ViewBag["hash_salt_pass"] = StringTrim(line, "hash_salt_pass");

                                //role id------------
                                uit.ViewBag["role"] = parser.ToInt(StringTrim(line, "role"));

                                //ФИО ------------
                                uit.ViewBag["FullName"] = $"{StringTrim(line, "surname")} {StringTrim(line, "name")?.FirstOrDefault()}.{StringTrim(line, "patronymic")?.FirstOrDefault()}.";

                                //Status------------
                                int status_id;
                                uit.ViewBag["status"] = int.TryParse(StringTrim(line, "status"), out status_id) ? status_id == 2 : false;

                                //Start_date------------
                                DateTime date;
                                uit.ViewBag["start_date"] = DateTime.TryParse(StringTrim(line, "start_date"), out date) ? date : new DateTime(1900, 01, 01);
                                //ent_date------------
                                uit.ViewBag["end_date"] = DateTime.TryParse(StringTrim(line, "end_date"), out date) ? date : new DateTime(2100, 12, 31);
                                break;
                            #endregion

                            #region CarNavigation
                            case InDataType.Vagons:
                                uit.ViewBag["StartDate"] = parser.ToDateTime(StringTrim(line, "DateFrom"));

                                uit.NumberOfTrain = StringTrim(line, "NomPoezd");

                                uit.StationDeparture = new Station
                                {
                                    CodeEsr = parser.ToInt(StringTrim(line, "KsnmPoezd"))
                                };

                                uit.StationArrival = new Station
                                {
                                    CodeEsr = parser.ToInt(StringTrim(line, "KskmPoezd"))
                                };

                                uit.ViewBag["UniqueId"] = parser.ToInt(StringTrim(line, "IdPoezd"));
                                
                                PsType psType;
                                VagonType vagonType;
                                var vagonUnderLines = line?.Element("vagons")?.Elements("v")?.ToList();
                                List<Vagon> vagons = new List<Vagon>();
                                foreach (var underLine in vagonUnderLines)
                                {
                                    try
                                    {
                                        vagons.Add(new Vagon
                                        (
                                            parser.ToInt(StringTrim(underLine, "NppVag")),
                                            StringTrim(underLine, "NomPS"),
                                            parser.ToInt(StringTrim(underLine, "NomVagShem")),
                                            Enum.TryParse(StringTrim(underLine, "TipPS"), out psType) ? psType : PsType.Other,
                                            Enum.TryParse(StringTrim(underLine, "TipVag"), out vagonType) ? vagonType : VagonType.Other,
                                            parser.ToInt(StringTrim(underLine, "Dlina")),
                                            parser.ToInt(StringTrim(underLine, "NomVagShemAsupv")),
                                            parser.ToInt(StringTrim(underLine, "NomVagShemAsoup"))
                                        ));
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Ошибка: {ex.Message}");
                                    }
                                }

                                uit.ViewBag["Composition"] = new Composition(
                                    parser.ToInt(StringTrim(line, "KolVag")),
                                    parser.ToInt(StringTrim(line, "KolLok")),
                                    parser.ToInt(StringTrim(line, "UslDlPoezd")),
                                    vagons);
                                break;
                                #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex}");
                    }

                    shedules.Add(uit);
                }
            }


            return shedules;
        }

        /*private string ParseIntTimeToTextTime(string time)
        {
            if (time.Contains(':'))
                return time;

            var lengthIn = time.Length;
            // Вставляем двоеточие в середину, чтобы можно было распарсить в число
            return $"{time.Substring(0, lengthIn / 2)}:{time.Substring(lengthIn / 2, lengthIn - lengthIn / 2)}";
        }*/

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
