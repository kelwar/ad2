using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommunicationDevices.Converters;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Settings;


namespace CommunicationDevices.Rules.ExchangeRules
{
    public class MainRule
    {
        public List<BaseExchangeRule> ExchangeRules { get; set; }
        public ViewType ViewType { get; set; }
    }



    public class ViewType
    {
        public string Type { get; set; }

        public int? TableSize { get; set; }                   // Размер таблицы. выставляется если Type == Table
        public int? FirstTableElement { get; set; }          //Номер стартового элемента. выставляется если Type == Table
    }



    public class BaseExchangeRule
    {
        #region prop

        public string Format { get; set; }
        public Conditions Resolution { get; set; }

        public RequestRule RequestRule { get; set; }
        public ResponseRule ResponseRule { get; set; }
        public RepeatRule RepeatRule { get; set; }

        #endregion




        #region ctor

        public BaseExchangeRule(RequestRule requestRule, ResponseRule responseRule, RepeatRule repeatRule, string format, Conditions resolution)
        {
            RequestRule = requestRule;
            ResponseRule = responseRule;
            RepeatRule = repeatRule;
            Format = format;
            Resolution = resolution;
        }

        #endregion





        #region Methode

        /// <summary>
        /// Проверка условий разрешения выполнения правила.
        /// </summary>
        public bool CheckResolution(UniversalInputType inData)
        {
            if (Resolution == null)
                return true;

            return Resolution.CheckResolutions(inData);  //инверсия ограничения 
        }

        #endregion

    }





    public class RequestRule
    {
        public int? MaxLenght { get; set; }
        public string Body { get; set; }


        #region Method

        public virtual string GetFillBody(UniversalInputType uit, byte? currentRow)
        {
            var str = MakeIndependentInserts(uit, currentRow);
            str = MakeDependentInserts(str, uit);
            return str;
        }


        /// <summary>
        /// Первоначальная вставка независимых переменных
        /// </summary>
        private string MakeIndependentInserts(UniversalInputType uit, byte? currentRow)
        {
            Lang lang = null;
            if (uit.ViewBag != null && uit.ViewBag.ContainsKey("Language"))
            {
                lang = uit.ViewBag["Language"];
            }

            if (Body.Contains("}"))                                                           //если указанны переменные подстановки
            {
                var subStr = Body.Split('}');
                StringBuilder resStr = new StringBuilder();
                int parseVal;
                foreach (var s in subStr)
                {
                    var replaseStr = (s.Contains("{")) ? (s + "}") : s;
                    var mathStr = Regex.Match(replaseStr, @"{(.*)}").Groups[1].Value;
                    var strpos = mathStr.Split(',').First();
                    var subvar = strpos.Split(':').First();

                    if (mathStr.Contains(','))
                    {
                        var strOffset = mathStr.Split(',').Last();
                        int offset;
                        if (int.TryParse(strOffset, out offset))
                        {
                            int length;
                            if (offset - 1 > resStr.Length)
                            {
                                length = offset - resStr.Length - 1;
                                for (int i = 0; i < length; i++)
                                {
                                    resStr.Append(' ');
                                }
                            }
                            else
                            {
                                length = resStr.Length - offset + 1;
                                if (resStr.Length > length)
                                {
                                    try
                                    {
                                        resStr = resStr.Remove(offset - 2, length + 1);
                                    }
                                    catch (Exception ex)
                                    {
                                        Library.Logs.Log.log.Error(ex);
                                    }
                                    resStr.Append(' ');
                                }
                            }
                        }
                    }
                    //if (subvar == string.Empty)
                    //{
                        //continue;
                    //}

                    if (replaseStr.Contains(','))
                    {
                        replaseStr = replaseStr.Split(',').First() + "}";
                    }

                    if (subvar == nameof(uit.AddressDevice))
                    { 
                        if (strpos.Contains(":")) //если указанн формат числа
                        {
                            if (int.TryParse(uit.AddressDevice, out parseVal))
                            {
                                var formatStr = string.Format(replaseStr.Replace(nameof(uit.AddressDevice), "0"), parseVal);
                                resStr.Append(formatStr);
                            }
                        }
                        else
                        {
                            var formatStr = string.Format(replaseStr.Replace(nameof(uit.AddressDevice), "0"), uit.AddressDevice);
                            resStr.Append(formatStr);
                        }
                        continue;
                    }
                    
                    if (subvar == "TypeName") 
                    {
                        string trainType;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                trainType = TypeConverters.TypeTrainEnum2EngString(uit.TypeTrain, TypeConverters.TypeTrainViewFormat.Long);
                                break;
                            default:
                                trainType = TypeConverters.TypeTrainEnum2RusString(uit.TypeTrain, TypeConverters.TypeTrainViewFormat.Long);
                                break;
                        }
                        var formatStr = string.Format(replaseStr.Replace("TypeName", "0"), !string.IsNullOrEmpty(trainType) ? trainType : " ");
                        resStr.Append(formatStr);
                        continue;
                    }
                    
                    if (subvar == "TypeAlias")
                    {
                        string trainType;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                trainType = TypeConverters.TypeTrainEnum2EngString(uit.TypeTrain, TypeConverters.TypeTrainViewFormat.Short);
                                break;
                            default:
                                trainType = TypeConverters.TypeTrainEnum2RusString(uit.TypeTrain, TypeConverters.TypeTrainViewFormat.Short);
                                break;
                        }
                        var formatStr = string.Format(replaseStr.Replace("TypeAlias", "0"), !string.IsNullOrEmpty(trainType) ? trainType : " ");
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == nameof(uit.NumberOfTrain))
                    {
                        string formatStr;
                        if (strpos.Contains(":")) //если указан формат числа
                        {
                            if (int.TryParse(uit.NumberOfTrain, out parseVal))
                            {
                                formatStr = string.Format(replaseStr.Replace(nameof(uit.NumberOfTrain), "0"), parseVal);
                            }
                            else
                            {
                                formatStr = string.Format(replaseStr.Replace(nameof(uit.NumberOfTrain), "0"), " ");
                            }
                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace(nameof(uit.NumberOfTrain), "0"), string.IsNullOrEmpty(uit.NumberOfTrain) ? " " : uit.NumberOfTrain);
                        }
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == nameof(uit.PathNumber))
                    {
                        string formatStr;
                        if (strpos.Contains(":")) //если указан формат числа
                        {
                            if (int.TryParse(uit.PathNumber, out parseVal))
                            {
                                formatStr = string.Format(replaseStr.Replace(nameof(uit.PathNumber), "0"), parseVal);
                            }
                            else
                            {
                                formatStr = string.Format(replaseStr.Replace(nameof(uit.PathNumber), "0"), " ");
                            }
                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace(nameof(uit.PathNumber), "0"), string.IsNullOrEmpty(uit.PathNumber) ? " " : uit.PathNumber);
                        }
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (subvar == "Platform")
                    {
                        string formatStr;
                        var platform = uit.Track?.Platform?.Name ?? string.Empty;
                        if (strpos.Contains(":")) //если указан формат числа
                        {
                            if (int.TryParse(platform, out parseVal))
                            {
                                formatStr = string.Format(replaseStr.Replace("Platform", "0"), parseVal);
                            }
                            else
                            {
                                formatStr = string.Format(replaseStr.Replace("Platform", "0"), " ");
                            }
                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace("Platform", "0"), string.IsNullOrEmpty(platform) ? " " : platform);
                        }
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (subvar == nameof(uit.Event))
                    {
                        string ev = uit.Event;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                switch (ev)
                                {
                                    case "ПРИБ.":
                                        ev = "ARR. ";
                                        break;
                                    case "ОТПР.":
                                        ev = "DEP. ";
                                        break;
                                    case "СТОЯНКА":
                                        ev = "STAY";
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                        var formatStr = string.Format(replaseStr.Replace(nameof(uit.Event), "0"), !string.IsNullOrEmpty(ev) ? ev : " ");
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == nameof(uit.Addition))
                    {
                        string addition;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                addition = uit.AdditionEng;
                                break;
                            default:
                                addition = uit.Addition;
                                break;
                        }
                        var formatStr = string.Format(replaseStr.Replace(nameof(uit.Addition), "0"), !string.IsNullOrEmpty(addition) ? addition : " ");
                        resStr.Append(formatStr);
                        continue;
                    }

                    
                    if (subvar == "StationsCut")
                    {
                        var stationDep = uit.StationDeparture;
                        var stationArr = uit.StationArrival;

                        string stationDeparture;
                        string stationArrival;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                stationDeparture = stationDep?.NameEng ?? null;
                                stationArrival = stationArr?.NameEng ?? null;
                                break;
                            default:
                                stationDeparture = stationDep?.NameRu ?? null;
                                stationArrival = stationArr?.NameRu ?? null;
                                break;
                        }

                        string stationsCut = null;
                        switch (uit.Event)
                        {
                            case "ПРИБ.":
                                stationsCut = stationDeparture;
                                break;

                            case "ОТПР.":
                                stationsCut = stationArrival;
                                break;

                            case "СТОЯНКА":
                                if (!string.IsNullOrEmpty(stationArrival))
                                {
                                    stationsCut = !string.IsNullOrEmpty(stationDeparture) ? $"{stationDeparture}-{stationArrival}" : stationArrival;
                                }
                                break;
                        }

                        var formatStr = string.Format(replaseStr.Replace("StationsCut", "0"), !string.IsNullOrEmpty(stationsCut) ? stationsCut : " ");
                        resStr.Append(formatStr);
                        continue;
                    }
                    
                    if (subvar == nameof(uit.Stations))
                    {
                        string stations = uit.Stations;

                        var stationDep = uit.StationDeparture;
                        var stationArr = uit.StationArrival;

                        string stationDeparture;
                        string stationArrival;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                stationDeparture = stationDep?.NameEng ?? null;
                                stationArrival = stationArr?.NameEng ?? null;
                                if (!string.IsNullOrEmpty(stationArrival))
                                {
                                    stations = !string.IsNullOrEmpty(stationDeparture) ? $"{stationDeparture}-{stationArrival}" : stationArrival;
                                }
                                break;
                            default:
                                break;
                        }
                        var formatStr = string.Format(replaseStr.Replace(nameof(uit.Stations), "0"), !string.IsNullOrEmpty(stations) ? stations : " ");
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == nameof(uit.StationArrival))
                    {
                        var station = uit.StationArrival;
                        var stationArrival = " ";
                        if (station != null)
                        {
                            switch (lang?.Name)
                            {
                                case "Eng":
                                    stationArrival = station.NameEng;
                                    break;
                                default:
                                    stationArrival = station.NameRu;
                                    break;
                            }
                        }
                        var formatStr = string.Format(replaseStr.Replace(nameof(uit.StationArrival), "0"), stationArrival);
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == nameof(uit.StationDeparture))
                    {
                        var station = uit.StationDeparture;
                        var stationDeparture = " ";
                        if (station != null)
                        {
                            switch (lang?.Name)
                            {
                                case "Eng":
                                    stationDeparture = station.NameEng;
                                    break;
                                default:
                                    stationDeparture = station.NameRu;
                                    break;
                            }
                        }
                        var formatStr = string.Format(replaseStr.Replace(nameof(uit.StationDeparture), "0"), stationDeparture);
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == nameof(uit.Note))
                    {
                        string note;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                note = uit.NoteEng;
                                break;
                            default:
                                note = uit.Note;
                                break;
                        }
                        var formatStr = string.Format(replaseStr.Replace(nameof(uit.Note), "0"), !string.IsNullOrEmpty(note) ? note : " ");
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (subvar == nameof(uit.DaysFollowingAlias))
                    {
                        string days;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                days = uit.DaysFollowingAliasEng;
                                break;
                            default:
                                days = uit.DaysFollowingAlias;
                                break;
                        }
                        var formatStr = string.Format(replaseStr.Replace(nameof(uit.DaysFollowingAlias), "0"), !string.IsNullOrEmpty(days) ? days : " ");
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (subvar == nameof(uit.DaysFollowing))
                    {
                        string days;
                        switch (lang?.Name)
                        {
                            case "Eng":
                                days = uit.DaysFollowingAliasEng; // Пока нет соответствующего свойства, используем его алиас
                                break;
                            default:
                                days = uit.DaysFollowing;
                                break;
                        }
                        var formatStr = string.Format(replaseStr.Replace(nameof(uit.DaysFollowing), "0"), !string.IsNullOrEmpty(days) ? days : " ");
                        resStr.Append(formatStr);
                        continue;
                    }

                    //if (subvar == nameof(uit.DelayTime))
                    if (subvar == "DelayTime")
                    {
                        string formatStr;
                        if (uit.ВремяЗадержки == null || uit.ВремяЗадержки.Value.TimeOfDay == TimeSpan.Zero)
                        {
                            //formatStr = string.Format(replaseStr.Replace(nameof(uit.ВремяЗадержки), "0"), " ");
                            formatStr = string.Format(replaseStr.Replace("DelayTime", "0"), " ");
                            resStr.Append(formatStr);
                            continue;
                        }

                        if (strpos.Contains(":")) //если указзанн формат времени
                        {
                            var dateFormat = strpos.Split(':')[1]; //без закр. скобки
                            if (dateFormat == "HH:mm")
                                dateFormat = dateFormat.Replace("HH:mm", "mm:ss");

                            //formatStr = string.Format(replaseStr.Replace(nameof(uit.DelayTime), "0"), (uit.DelayTime == DateTime.MinValue) ? " " : uit.DelayTime.Value.ToString(dateFormat));
                            formatStr = string.Format(replaseStr.Replace("DelayTime", "0"), (uit.ВремяЗадержки == DateTime.MinValue) ? " " : uit.ВремяЗадержки.Value.ToString(dateFormat));
                        }
                        else                         //вывод в минутах
                        {
                            //formatStr = string.Format(replaseStr.Replace(nameof(uit.DelayTime), "0"), (uit.DelayTime == DateTime.MinValue) ? " " : ((uit.DelayTime.Value.Hour * 60) + (uit.DelayTime.Value.Minute)).ToString());
                            formatStr = string.Format(replaseStr.Replace("DelayTime", "0"), (uit.ВремяЗадержки == DateTime.MinValue) ? " " : ((uit.ВремяЗадержки.Value.Minute * 60) + (uit.ВремяЗадержки.Value.Second)).ToString());
                        }
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == nameof(uit.ExpectedTime))
                    {
                        string formatStr;
                        if (strpos.Contains(":")) //если указзанн формат времени
                        {
                            var dateFormat = strpos.Split(':')[1]; //без закр. скобки
                            formatStr = string.Format(replaseStr.Replace(nameof(uit.ExpectedTime), "0"), (uit.ExpectedTime == DateTime.MinValue) ? " " : uit.ExpectedTime.ToString(dateFormat));
                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace(nameof(uit.ExpectedTime), "0"), (uit.ExpectedTime == DateTime.MinValue) ? " " : uit.ExpectedTime.ToString(CultureInfo.InvariantCulture));
                        }
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == nameof(uit.Time))
                    {
                        string formatStr;
                        if (strpos.Contains(":")) //если указанн формат времени
                        {
                            var dateFormat = strpos.Split(':')[1]; //без закр. скобки
                            if (dateFormat.Contains("Sec"))   //формат задан в секундах
                            {
                                var intFormat = dateFormat.Substring(3, 2);
                                var intValue = (uit.Time.Hour * 3600 + uit.Time.Minute * 60);
                                formatStr = string.Format(replaseStr.Replace(nameof(uit.Time), "0"), (intValue == 0) ? " " : intValue.ToString(intFormat));
                            }
                            else
                            {
                                formatStr = string.Format(replaseStr.Replace(nameof(uit.Time), "0"), (uit.Time == DateTime.MinValue) ? " " : uit.Time.ToString(dateFormat));
                            }

                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace(nameof(uit.Time), "0"), (uit.Time == DateTime.MinValue) ? " " : uit.Time.ToString(CultureInfo.InvariantCulture));
                        }
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == "TDepart")
                    {
                        DateTime timeDepart = DateTime.MinValue;
                        switch (uit.Event)
                        {
                            case "СТОЯНКА":
                                timeDepart = (uit.TransitTime != null && uit.TransitTime.ContainsKey("отпр")) ? uit.TransitTime["отпр"] : DateTime.MinValue;
                                break;

                            case "ОТПР.":
                                timeDepart = uit.Time;
                                break;
                        }

                        string formatStr;
                        if (strpos.Contains(":")) //если указанн формат времени
                        {
                            var dateFormat = strpos.Split(':')[1]; //без закр. скобки
                            if (dateFormat.Contains("Sec"))   //формат задан в секундах
                            {
                                var intFormat = dateFormat.Substring(3, 2);
                                var intValue = (uit.Time.Hour * 3600 + uit.Time.Minute * 60);
                                formatStr = string.Format(replaseStr.Replace("TDepart", "0"), (intValue == 0) ? " " : intValue.ToString(intFormat));
                            }
                            else
                            {
                                formatStr = string.Format(replaseStr.Replace("TDepart", "0"), (timeDepart == DateTime.MinValue) ? " " : timeDepart.ToString(dateFormat));
                            }
                            
                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace("TDepart", "0"), (timeDepart == DateTime.MinValue) ? " " : timeDepart.ToString(CultureInfo.InvariantCulture));
                        }
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == "TArrival")
                    {
                        DateTime timeArrival = DateTime.MinValue;
                        switch (uit.Event)
                        {
                            case "СТОЯНКА":
                                timeArrival = (uit.TransitTime != null && uit.TransitTime.ContainsKey("приб")) ? uit.TransitTime["приб"] : DateTime.MinValue;
                                break;

                            case "ПРИБ.":
                                timeArrival = uit.Time;
                                break;
                        }

                        string formatStr;
                        if (strpos.Contains(":")) //если указанн формат времени
                        {
                            var dateFormat = strpos.Split(':')[1]; //без закр. скобки
                            if (dateFormat.Contains("Sec"))   //формат задан в секундах
                            {
                                var intFormat = dateFormat.Substring(3, 2);
                                var intValue = (uit.Time.Hour * 3600 + uit.Time.Minute * 60);
                                formatStr = string.Format(replaseStr.Replace("TArrival", "0"), (intValue == 0) ? " " : intValue.ToString(intFormat));
                            }
                            else
                            {
                                formatStr = string.Format(replaseStr.Replace("TArrival", "0"), (timeArrival == DateTime.MinValue) ? " " : timeArrival.ToString(dateFormat));
                            }

                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace("TArrival", "0"), (timeArrival == DateTime.MinValue) ? " " : timeArrival.ToString(CultureInfo.InvariantCulture));
                        }
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == "Hour")
                    {
                        var formatStr = string.Format(replaseStr.Replace("Hour", "0"), DateTime.Now.Hour);
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == "Minute")
                    {
                        var formatStr = string.Format(replaseStr.Replace("Minute", "0"), DateTime.Now.Minute);
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar == "Second")
                    {
                        var formatStr = string.Format(replaseStr.Replace("Second", "0"), DateTime.Now.Second);
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (subvar.Contains("Year"))
                    {
                        var formatStr = CalculateMathematicFormat(replaseStr, DateTime.Now.Year); // конструкция не работает, необходимо откорректировать условие
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (subvar == "Month")
                    {
                        var formatStr = string.Format(replaseStr.Replace("Month", "0"), DateTime.Now.Month);
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (subvar == "Day")
                    {
                        var formatStr = string.Format(replaseStr.Replace("Day", "0"), DateTime.Now.Day);
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (subvar == "SyncTInSec")
                    {
                        var secTime = DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second;
                        string formatStr;
                        if (strpos.Contains(":")) //если указан формат времени
                        {
                            var dateFormat = strpos.Split(':')[1]; //без закр. скобки
                            formatStr = string.Format(replaseStr.Replace("SyncTInSec", "0"), (secTime == 0) ? " " : secTime.ToString(dateFormat));
                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace("SyncTInSec", "0"), (secTime == 0) ? " " : secTime.ToString(CultureInfo.InvariantCulture));
                        }
                        resStr.Append(formatStr);
                        continue;
                    }


                    if (subvar.Contains("rowNumber"))
                    {
                        if (currentRow.HasValue)
                        {
                            var formatStr = CalculateMathematicFormat(replaseStr, currentRow.Value);
                            resStr.Append(formatStr);
                            continue;
                        }
                    }

                    if (subvar == "Text")
                    {
                        string text = null;
                        var defaultText = " ";

                        var value = strpos.Split(':').LastOrDefault();
                        if (value != null)
                        {
                            var dict = value.Split('|');
                            if (dict != null)
                            {
                                for (int i = 0; i < dict.Length; i++)
                                {
                                    var key = dict[i].Split('_');
                                    if (key != null && key.Length > 0)
                                    {
                                        if (key.Length > 1)
                                        {
                                            if (key[0] == lang?.Name)
                                            {
                                                text = key[1];
                                                break;
                                            }
                                            // Игнорим все остальные возможные значения
                                        }
                                        else
                                        {
                                            defaultText = key[0];
                                        }
                                    }
                                }
                            }
                            
                        }

                        var formatStr = string.Format(replaseStr.Replace("Text", "0"), !string.IsNullOrEmpty(text) ? text : defaultText);
                        resStr.Append(formatStr);
                        continue;
                    }

                    if (int.TryParse(subvar, out parseVal))
                    {
                        string formatStr;
                        if (strpos.Contains(":")) //если указанн формат числа
                        {
                            formatStr = string.Format(replaseStr.Replace(subvar, "0"), parseVal);
                        }
                        else
                        {
                            formatStr = string.Format(replaseStr.Replace(subvar, "0"), subvar);
                        }
                        resStr.Append(formatStr);
                        continue;
                    }

                    replaseStr = replaseStr.Replace("{}", "");

                    //Добавим в неизменном виде спецификаторы байтовой информации.
                    resStr.Append(replaseStr);
                }

                return resStr.ToString();
            }

            return Body;
        }



        /// <summary>
        /// Первоначальная вставка ЗАВИСИМЫХ переменных
        /// </summary>
        private string MakeDependentInserts(string body, UniversalInputType uit)
        {
            if (body.Contains("}"))                                                           //если указанны переменные подстановки
            {
                var subStr = body.Split('}');
                StringBuilder resStr = new StringBuilder();
                for (var index = 0; index < subStr.Length; index++)
                {
                    var s = subStr[index];
                    var replaseStr = (s.Contains("{")) ? (s + "}") : s;
                    //Подсчет кол-ва символов
                    if (replaseStr.Contains("NumberOfCharacters"))
                    {
                        var targetStr = (subStr.Length > (index + 1)) ? subStr[index + 1] : string.Empty;
                        if (Regex.Match(targetStr, "\\\"(.*)\"").Success) //
                        {
                            var matchString = Regex.Match(targetStr, "\\\"(.*)\\\"").Groups[1].Value;
                            if (!string.IsNullOrEmpty(matchString))
                            {
                                var lenght = matchString.TrimEnd('\\').Length;

                                var dateFormat = Regex.Match(replaseStr, "\\{NumberOfCharacters:(.*)\\}").Groups[1].Value;
                                if (!string.IsNullOrEmpty(dateFormat)) //если указан формат числа
                                {
                                    var formatStr = string.Format(replaseStr.Replace("NumberOfCharacters", "0"), lenght.ToString(dateFormat));
                                    resStr.Append(formatStr);
                                }
                                else
                                {
                                    var formatStr = string.Format(replaseStr.Replace(nameof(uit.AddressDevice), "0"), uit.AddressDevice);
                                    resStr.Append(formatStr);
                                }
                            }
                        }
                        continue;
                    }

                    //Добавим в неизменном виде спецификаторы байтовой информации.
                    resStr.Append(replaseStr);
                }

                return resStr.ToString().Replace("\\\"", string.Empty);
            }

            return body;
        }




        public static string CalculateMathematicFormat(string str, int row)
        {
            var matchString = Regex.Match(str, "\\{\\((.*)\\)\\:(.*)\\}").Groups[1].Value;

            var calc = new Sprache.Calc.XtensibleCalculator();
            var expr = calc.ParseExpression(matchString, rowNumber => row);
            var func = expr.Compile();
            var arithmeticResult = (int)func();

            var reultStr = str.Replace("(" + matchString + ")", "0");
            reultStr = String.Format(reultStr, arithmeticResult);

            return reultStr;
        }

        #endregion
    }


    public class ResponseRule : RequestRule
    {
        public int Time { get; set; }
    }


    public class RepeatRule
    {
        public int Count { get; set; }
        public int? DeltaX { get; set; }
        public int? DeltaY { get; set; }
    }

}