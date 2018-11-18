using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CommunicationDevices.DataProviders;
using Domain.Entitys;
using System;
using System.Globalization;

namespace CommunicationDevices.Behavior.GetDataBehavior.ConvertGetedData
{
    class CisOperativeShDataConverter : IInputDataConverter
    {
        public IEnumerable<UniversalInputType> ParseXml2Uit(XDocument xDoc)
        {
            //Log.log.Trace("xDoc" + xDoc.ToString());//LOG;
            var shedules = new List<UniversalInputType>();

            InDataType inDataType = InDataType.None;
            List<XElement> lines = null;
            if (xDoc.Element("changes") != null)
            {
                lines = xDoc.Element("changes")?.Elements().ToList();
            }

            if (lines != null)
            {
                for (var i = 0; i < lines.Count; i++)
                //foreach (var line in lines)
                {
                    var line = lines[i];
                    if (line.Element("IdGdp") != null)
                        inDataType = InDataType.TrainsOper;
                    else
                        inDataType = InDataType.LocalTrainsOper;

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
                            case InDataType.TrainsOper:
                                //TransitTime["приб"]-----
                                var prib = StringTrim(line, "InDateTime");
                                DateTime dtPrib;
                                if (prib != "-1" && DateTime.TryParse(prib, out dtPrib))
                                {
                                    dtPrib = TimeZoneInfo.ConvertTime(dtPrib, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), TimeZoneInfo.Local);
                                }
                                else
                                {
                                    dtPrib = DateTime.MinValue;
                                    uit.Event = "ОТПР.";
                                }
                                uit.TransitTime["приб"] = dtPrib;

                                //TransitTime["отпр"]-----
                                var otpr = StringTrim(line, "OutDateTime");
                                DateTime dtOtpr;
                                if (otpr != "-1" && DateTime.TryParse(otpr, out dtOtpr))
                                {
                                    dtOtpr = TimeZoneInfo.ConvertTime(dtOtpr, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), TimeZoneInfo.Local);
                                }
                                else
                                {
                                    dtOtpr = DateTime.MinValue;
                                    uit.Event = "ПРИБ.";
                                }
                                uit.TransitTime["отпр"] = dtOtpr;

                                uit.DaysFollowing = dtOtpr != DateTime.MinValue ? dtOtpr.Date.ToString("yyyy.MM.dd") : dtPrib.ToString("yyyy.MM.dd");

                                //StopTime--------------- 
                                //TimeSpan stopTime;
                                //if (TimeSpan.TryParse(StringTrim(line, "HereDateTime"), out stopTime))
                                //{
                                //    uit.StopTime = stopTime;
                                //}
                                //else
                                //{
                                //uit.StopTime = uit.TransitTime["отпр"] > uit.TransitTime["приб"] ? uit.TransitTime["отпр"] - uit.TransitTime["приб"] : default(TimeSpan);
                                //}

                                //Id----------
                                int id;
                                uit.TrnId = int.TryParse(StringTrim(line, "ID"), out id) ? id : 0;
                                uit.ScheduleId = int.TryParse(StringTrim(line, "IdGdp"), out id) ? id : 0;
                                uit.ViewBag["GdpIdR"] = int.TryParse(StringTrim(line, "IdrGdp"), out id) ? id : 0;


                                //NumberOfTrain------ NumberOfTrain - проверить, куда пишет значения в файл TableRecords
                                //var numberOfTrain1 = StringTrim(line, "TrainNumber");
                                //var numberOfTrain2 = StringTrim(line, "SecondTrainNumber");
                                var numberOfTrain1 = StringTrim(line, "Num1");
                                var numberOfTrain2 = StringTrim(line, "Num2");

                                int num1;
                                int num2;
                                uit.NumberOfTrain = prib == "-1" ? numberOfTrain2 :
                                                        otpr == "-1" ? numberOfTrain1 :
                                                        numberOfTrain1 != numberOfTrain2 ? $"{numberOfTrain1}/{numberOfTrain2}" : numberOfTrain2;

                                /*uit.NumberOfTrain = string.IsNullOrWhiteSpace(numberOfTrain2)
                                                    // Защита от некорректных данных из ЦИС
                                                    || numberOfTrain1 == numberOfTrain2 // Если равны - игнорим второй
                                                    || ((numberOfTrain1.Length < 4 && numberOfTrain2.Length < 4) // Только для дальних
                                                    && int.TryParse(numberOfTrain1, out num1) && int.TryParse(numberOfTrain2, out num2)
                                                    && Math.Abs(num1 - num2) > 1) // Если номера разнятся более чем на 1 игнорим второй
                                                    ? numberOfTrain1
                                                    : (prib == "-1" || otpr == "-1") // Защита от неверного номера поезда формирования или прибытия
                                                    ? numberOfTrain2
                                                    : numberOfTrain1 + "/" + numberOfTrain2;*/

                                //DaysFollowing------ самое сложное - для дальнего следования циклом проходим каждый день и записываем это в "АктивностьДня"
                                // для пригорода - преобразуем вариант "По рабочим" и т.д. в РаботаПоДням
                                //uit.DaysFollowing = uit.Event == "ПРИБ." ? uit.TransitTime["приб"].ToString("yyyy.MM.dd") : uit.TransitTime["отпр"].ToString("yyyy.MM.dd");
                                //uit.DaysFollowingAlias = StringTrim(line, "DaysOfGoingAlias");
                                uit.DaysFollowingAlias = string.Empty;
                                uit.DaysFollowingAliasEng = string.Empty;

                                //Enabled------------
                                uit.ViewBag["Enabled"] = StringTrim(line, "Enabled");
                                uit.IsActive = uit.ViewBag["Enabled"] == "1";

                                //SoundTemplate------
                                //uit.ViewBag["SoundTemplate"] = StringTrim(line, "SoundTemplate");
                                uit.ViewBag["SoundTemplate"] = string.Empty;

                                //VagonDirection------ нет возможности собирать эту информацию из ЦИС
                                //int vagonDirection;
                                //if (int.TryParse(StringTrim(line, "VagonDirection"), out vagonDirection))
                                //{
                                //    uit.VagonDirection = (VagonDirection)vagonDirection;
                                //}

                                //DefaultsPaths------------- нет возможности собирать эту информацию из ЦИС
                                //uit.ViewBag["DefaultsPaths"] = StringTrim(line, "DefaultsPaths");

                                //TrainType---------
                                ТипПоезда typeOfTrain;
                                //var s = StringTrimToTitleCase(line, "TrainType");
                                var s = StringTrimToTitleCase(line, "Type");
                                if (s == "Экспресс")
                                    s = "Ласточка"; // условие подойдёт не для всех вокзалов, хардкод
                                if (s == "Высокоскоростной")
                                    s = "Скоростной";

                                if (Enum.TryParse(s, out typeOfTrain))
                                {
                                    uit.TypeTrain = (TypeTrain)typeOfTrain;
                                }

                                //Stops------
                                uit.Note = string.Empty;
                                uit.NoteEng = string.Empty;

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

                                //AdditionSend--------------- не принимаем из ЦИС, по умолчанию сделать 1
                                //uit.ViewBag["AdditionSend"] = StringTrim(line, "AdditionSend");

                                //AdditionSendSound------------- не принимаем из ЦИС, по умолчанию сделать 1
                                //uit.ViewBag["AdditionSendSound"] = StringTrim(line, "AdditionSendSound");

                                //SoundsType------------------- не принимаем из ЦИС, по умолчанию Автомат

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

                                //DirectionStation----------------------- из ЦИС принимаем станции следования. 
                                // Либо проверить соответствие конечных станций, либо игнорировать поле
                                //uit.DirectionStation = StringTrim(line, "Direction");
                                uit.DirectionStation = string.Empty;

                                //VagonDirectionChanging----------- не принимаем из ЦИС, по умолчанию сделать 0
                                //bool changeVagonDirection;
                                //if (bool.TryParse(StringTrim(line, "VagonDirectionChanging"), out changeVagonDirection))
                                //{
                                //    uit.ChangeVagonDirection = changeVagonDirection;
                                //}
                                break;
                            case InDataType.LocalTrainsOper:
                                //TransitTime["приб"]-----
                                prib = StringTrim(line, "InDateTime");
                                if (prib != "-1" && DateTime.TryParse(prib, out dtPrib))
                                {
                                    dtPrib = TimeZoneInfo.ConvertTime(dtPrib, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), TimeZoneInfo.Local);
                                }
                                else
                                {
                                    dtPrib = DateTime.MinValue;
                                    uit.Event = "ОТПР.";
                                }
                                uit.TransitTime["приб"] = dtPrib;

                                //TransitTime["отпр"]-----
                                otpr = StringTrim(line, "OutDateTime");
                                if (otpr != "-1" && DateTime.TryParse(otpr, out dtOtpr))
                                {
                                    dtOtpr = TimeZoneInfo.ConvertTime(dtOtpr, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), TimeZoneInfo.Local);
                                }
                                else
                                {
                                    dtOtpr = DateTime.MinValue;
                                    uit.Event = "ПРИБ.";
                                }
                                uit.TransitTime["отпр"] = dtOtpr;


                                uit.DaysFollowing = dtOtpr != DateTime.MinValue ? dtOtpr.Date.ToString("yyyy.MM.dd") : dtPrib.ToString("yyyy.MM.dd");
                                //StopTime--------------- 
                                //if (TimeSpan.TryParse(StringTrim(line, "HereDateTime"), out stopTime))
                                //{
                                //    uit.StopTime = stopTime;
                                //}
                                //else
                                //{
                                //uit.StopTime = uit.TransitTime["отпр"] > uit.TransitTime["приб"] ? uit.TransitTime["отпр"] - uit.TransitTime["приб"] : default(TimeSpan);
                                //}
                                
                                uit.ScheduleId = int.TryParse(StringTrim(line, "ID"), out id) ? id : 0;
                                uit.TrnId = int.TryParse(StringTrim(line, "TrnId"), out id) ? id : 0;



                                //NumberOfTrain------ NumberOfTrain - проверить, куда пишет значения в файл TableRecords
                                //numberOfTrain1 = StringTrim(line, "TrainNumber");
                                //numberOfTrain2 = StringTrim(line, "SecondTrainNumber");
                                numberOfTrain1 = StringTrim(line, "Num1");
                                numberOfTrain2 = StringTrim(line, "Num2");

                                uit.NumberOfTrain = string.IsNullOrWhiteSpace(numberOfTrain2) || otpr == "-1" ?
                                                    numberOfTrain1 : numberOfTrain2;

                                /*uit.NumberOfTrain = string.IsNullOrWhiteSpace(numberOfTrain2)
                                                    // Защита от некорректных данных из ЦИС
                                                    || numberOfTrain1 == numberOfTrain2 // Если равны - игнорим второй
                                                    || ((numberOfTrain1.Length < 4 && numberOfTrain2.Length < 4) // Только для дальних
                                                    && int.TryParse(numberOfTrain1, out num1) && int.TryParse(numberOfTrain2, out num2)
                                                    && Math.Abs(num1 - num2) > 1) // Если номера разнятся более чем на 1 игнорим второй
                                                    ? numberOfTrain1
                                                    : (prib == "-1" || otpr == "-1") // Защита от неверного номера поезда формирования или прибытия
                                                    ? numberOfTrain2
                                                    : numberOfTrain1 + "/" + numberOfTrain2;*/

                                //DaysFollowing------ самое сложное - для дальнего следования циклом проходим каждый день и записываем это в "АктивностьДня"
                                // для пригорода - преобразуем вариант "По рабочим" и т.д. в РаботаПоДням
                                //var str = StringTrim(line, "DaysOfGoing").Split('-');
                                var str = StringTrim(line, "Date").Split('-');
                                var dayStr = string.Empty;
                                if (str.Length == 3)
                                {
                                    dayStr = $"{str[2]}.{str[1]}.{str[0]}";
                                }
                                //uit.DaysFollowing = dayStr;
                                uit.DaysFollowingAlias = StringTrim(line, "DaysOfGoingAlias");
                                uit.DaysFollowingAlias = string.Empty;
                                uit.DaysFollowingAliasEng = string.Empty;

                                //Enabled------------
                                uit.ViewBag["Enabled"] = StringTrim(line, "Enabled");
                                uit.IsActive = uit.ViewBag["Enabled"] == "1";

                                //SoundTemplate------
                                //uit.ViewBag["SoundTemplate"] = StringTrim(line, "SoundTemplate");
                                uit.ViewBag["SoundTemplate"] = string.Empty;

                                //VagonDirection------ нет возможности собирать эту информацию из ЦИС
                                //if (int.TryParse(StringTrim(line, "VagonDirection"), out vagonDirection))
                                //{
                                //    uit.VagonDirection = (VagonDirection)vagonDirection;
                                //}

                                //DefaultsPaths------------- нет возможности собирать эту информацию из ЦИС
                                //uit.ViewBag["DefaultsPaths"] = StringTrim(line, "DefaultsPaths");

                                //TrainType---------
                                //s = StringTrimToTitleCase(line, "TrainType");
                                s = StringTrimToTitleCase(line, "Type");
                                if (s == "Экспресс")
                                    s = "Ласточка"; // условие подойдёт не для всех вокзалов, хардкод
                                if (s == "Высокоскоростной")
                                    s = "Скоростной";

                                if (Enum.TryParse(s, out typeOfTrain))
                                {
                                    uit.TypeTrain = (TypeTrain)typeOfTrain;
                                }

                                //Stops------
                                uit.Note = string.Empty;
                                uit.NoteEng = string.Empty;

                                var underLines = line?.Element("Stops")?.Elements("stop")?.ToList() ?? null;
                                var route = new Route();
                                if (underLines != null)
                                {
                                    foreach (var underLine in underLines)
                                    {
                                        try
                                        {
                                            int esr;
                                            var stopStation = new Station
                                            {
                                                NameRu = StringTrim(underLine, "name"),
                                                CodeEsr = int.TryParse(StringTrim(underLine, "esr"), out esr) ? esr : 0
                                            };

                                            int seq, state;
                                            StopState stopState = StopState.TechNonStop;
                                            //if (int.TryParse(StringTrim(underLine, "StopState"), out state))
                                            if (int.TryParse(StringTrim(underLine, "state"), out state))
                                            {
                                                stopState = (StopState)state;
                                            }
                                            else if (int.TryParse(StringTrim(underLine, "isStop"), out state))
                                            {
                                                stopState = state == 1 ? StopState.Stop : esr != 6023 ? StopState.NonStop : StopState.TechNonStop;
                                            }

                                            route.Stops.Add(int.TryParse(StringTrim(underLine, "seq"), out seq) ? seq : 0, new Stop
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
                                dtStartDateTime = DateTime.MinValue;
                                //DateTime.TryParse(StringTrim(line, "ScheduleStartDateTime"), out dtStartDateTime);
                                DateTime.TryParse(StringTrim(line, "StartDate"), out dtStartDateTime);
                                uit.ViewBag["ScheduleStartDateTime"] = dtStartDateTime;

                                //ScheduleEndDateTime---------------
                                dtEndDateTime = new DateTime(2099, 12, 31);
                                //DateTime.TryParse(StringTrim(line, "ScheduleEndDateTime"), out dtEndDateTime);
                                DateTime.TryParse(StringTrim(line, "EndDate"), out dtEndDateTime);
                                uit.ViewBag["ScheduleEndDateTime"] = dtEndDateTime;

                                //Addition---------------
                                //uit.Addition = StringTrimToTitleCase(line, "Addition");
                                uit.Addition = StringTrimToTitleCase(line, "Name");
                                uit.AdditionEng = string.Empty;

                                //AdditionSend--------------- не принимаем из ЦИС, по умолчанию сделать 1
                                //uit.ViewBag["AdditionSend"] = StringTrim(line, "AdditionSend");

                                //AdditionSendSound------------- не принимаем из ЦИС, по умолчанию сделать 1
                                //uit.ViewBag["AdditionSendSound"] = StringTrim(line, "AdditionSendSound");

                                //SoundsType------------------- не принимаем из ЦИС, по умолчанию Автомат

                                //ItenaryTime--------------
                                itenaryTime = StringTrim(line, "ItenaryTime");
                                int.TryParse(itenaryTime, out intTime);
                                intTime %= 24 * 60;
                                itenaryTime = intTime / 60 + ":" + (intTime % 60 < 10 ? "0" : string.Empty) + (intTime % 60);
                                //DateTime itenTime;
                                //DateTime.TryParse(itenaryTime, out itenTime);
                                uit.ViewBag["ItenaryTime"] = itenaryTime;

                                //StartStation--------------------
                                //elem = line?.Element("StartStation")?.Value.Replace("\\", "/") ?? string.Empty;
                                int.TryParse(StringTrim(line, "ExpressStart"), out expressStart);
                                
                                int.TryParse(StringTrim(line, "EsrStart"), out esrStart);

                                //EndStation-----------------------
                                int.TryParse(StringTrim(line, "ExpressEnd"), out expressEnd);

                                //EndStation-----------------------
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

                                //DirectionStation----------------------- из ЦИС принимаем станции следования. 
                                // Либо проверить соответствие конечных станций, либо игнорировать поле
                                //uit.DirectionStation = StringTrim(line, "Direction");
                                uit.DirectionStation = string.Empty;

                                //VagonDirectionChanging----------- не принимаем из ЦИС, по умолчанию сделать 0
                                //if (bool.TryParse(StringTrim(line, "VagonDirectionChanging"), out changeVagonDirection))
                                //{
                                //    uit.ChangeVagonDirection = changeVagonDirection;
                                //}
                                break;
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

        private string ParseIntTimeToTextTime(string time)
        {
            var lengthIn = time.Length;
            // Вставляем двоеточие в середину, чтобы можно было распарсить в число
            return $"{time.Substring(0, lengthIn / 2)}:{time.Substring(lengthIn / 2, lengthIn - lengthIn / 2)}";
        }

        private string StringTrim(XElement line, string s)
        {
            var elem = line?.Element(s)?.Value.Replace("\\", "/") ?? string.Empty;
            return Regex.Replace(elem.TrimEnd(), "[\r\n\t]+", "");
        }
        private string StringTrimToTitleCase(XElement line, string s)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(StringTrim(line, s).ToLower());
        }
    }
}
