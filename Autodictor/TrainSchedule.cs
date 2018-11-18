using Library;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MainExample
{
    public enum ScheduleMode
    {
        Отсутствует = 0,
        Ежедневно = 1,
        ПоЧетным = 2,
        ПоНечетным = 3,
        Выборочно = 4,
        ПоДням = 5,
    };

    public class TrainSchedule
    {
        #region Fields

        private DateTime? _startDate;
        private DateTime? _endDate;

        #endregion

        #region Properties

        public ConcurrentSortedDict<DateTime, bool> DayDictionary { get; }
        public string TrainNumber { get; set; }
        public string TrainName { get; set; }
        public int BoundaryDaysActivity { get; set; }
        public ushort WeekDaysActivity { get; set; }
        public ScheduleMode ScheduleMode { get; set; }

        #endregion

        #region Ctor

        private TrainSchedule()
        {
            DayDictionary = new ConcurrentSortedDict<DateTime, bool>();
            ScheduleMode = ScheduleMode.Отсутствует;
            TrainNumber = string.Empty;
            TrainName = string.Empty;
            BoundaryDaysActivity = 0x00000;
            WeekDaysActivity = 0x0000;
        }

        #endregion

        #region Methods

        public static TrainSchedule ПолучитьИзСтрокиПланРасписанияПоезда(string beatsString, DateTime? startDate = null, DateTime? endDate = null)
        {
            var trainSchedule = new TrainSchedule();
            trainSchedule._startDate = startDate;
            trainSchedule._endDate = endDate;

            if (string.IsNullOrWhiteSpace(beatsString))
            {
                Library.Logs.Log.log.Error($"Строка расписания поезда {trainSchedule.TrainNumber} {trainSchedule.TrainName} пуста");
                beatsString = $"Отс:0:0:0:0:0:0:0:0:0:0:0:0:0:0:0:0";
                //return trainSchedule;
            }

            string[] ПланПоМесяцам = beatsString.Split(':');
            if (ПланПоМесяцам.Length == 17)
            {
                if (ПланПоМесяцам[0].Contains("Отс"))
                    trainSchedule.ScheduleMode = ScheduleMode.Отсутствует;
                else if (ПланПоМесяцам[0].Contains("Еж"))
                    trainSchedule.ScheduleMode = ScheduleMode.Ежедневно;
                else if (ПланПоМесяцам[0].Contains("ПоЧет"))
                    trainSchedule.ScheduleMode = ScheduleMode.ПоЧетным;
                else if (ПланПоМесяцам[0].Contains("ПоНеч"))
                    trainSchedule.ScheduleMode = ScheduleMode.ПоНечетным;
                else if (ПланПоМесяцам[0].Contains("Выб"))
                    trainSchedule.ScheduleMode = ScheduleMode.Выборочно;
                else if (ПланПоМесяцам[0].Contains("ПоДням"))
                    trainSchedule.ScheduleMode = ScheduleMode.ПоДням;
                else
                    return trainSchedule;

                uint TempUInt32 = 0x00000000;
                for (byte i = 0; i < 12; i++)
                {
                    if (uint.TryParse(ПланПоМесяцам[(DateTime.Now.Month + i - 1) % 12 + 1], out TempUInt32) == true)
                    {
                        var firstDay = new DateTime(DateTime.Now.Year + (DateTime.Now.Month + i - 1) / 12, (DateTime.Now.Month + i - 1) % 12 + 1, 1);
                        for (var j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                        {
                            if (!trainSchedule.DayDictionary.ContainsKey(j.Date))
                            {
                                trainSchedule.DayDictionary.Add(j.Date, (TempUInt32 & (1 << (j.Day - 1))) != 0);
                            }
                            else
                            {
                                trainSchedule.DayDictionary[j.Date] = (TempUInt32 & (1 << (j.Day - 1))) != 0;
                            }
                        }
                    }
                }

                ushort TempUInt16 = 0x0000;
                if (ushort.TryParse(ПланПоМесяцам[15], out TempUInt16) == true)
                    trainSchedule.WeekDaysActivity = TempUInt16;

                int TempInt = 0x00000;
                if (int.TryParse(ПланПоМесяцам[16], out TempInt) == true)
                    trainSchedule.BoundaryDaysActivity = TempInt;
            }
            else
            {
                if (beatsString.Contains("Январь"))
                {
                    trainSchedule.ScheduleMode = ScheduleMode.Выборочно;
                    if (ПланПоМесяцам.Length == 14)
                    {
                        for (byte i = 0; i < 14; i++)
                        {
                            int monthIndex = i + DateTime.Now.Month - 1;
                            var currentYear = DateTime.Now.Year + monthIndex / 12;
                            string[] ПоляМесячногоПлана = ПланПоМесяцам[i].Split(',');
                            if (ПоляМесячногоПлана.Contains("Отсутствует")) { }
                            else if (ПоляМесячногоПлана.Contains("Ежедневно")) { }
                            else if (ПоляМесячногоПлана.Contains("ПоЧетным")) { }
                            else if (ПоляМесячногоПлана.Contains("ПоНечетным")) { }
                            else
                            {
                                foreach (string item in ПоляМесячногоПлана)
                                {
                                    int День = 0;
                                    if (int.TryParse(item, out День))
                                    {
                                        if ((День >= 1) && (День <= DateTime.DaysInMonth(currentYear, monthIndex % 12 + 1)))
                                            trainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, День).Date, true);
                                    }
                                }
                            }
                        }
                    }

                }
            }

            return trainSchedule;
        }

        public string ПолучитьСтрокуРасписания()
        {
            var result = string.Empty;
            switch (ScheduleMode)
            {
                case ScheduleMode.Отсутствует:
                    result += "Отс:";
                    break;

                case ScheduleMode.Ежедневно:
                    result += "Еж:";
                    break;

                case ScheduleMode.ПоЧетным:
                    result += "ПоЧет:";
                    break;

                case ScheduleMode.ПоНечетным:
                    result += "ПоНеч:";
                    break;

                case ScheduleMode.Выборочно:
                    result += "Выб:";
                    break;

                case ScheduleMode.ПоДням:
                    result += "ПоДням:";
                    break;
            }

            var beats = GetBeats();
            for (var i = 0; i < beats.Length; i++)
                result += beats[i] + ":";

            result += WeekDaysActivity.ToString() + ":" + BoundaryDaysActivity.ToString();

            return result;
        }

        public bool ПолучитьАктивностьДняДвижения(DateTime date, DateTime? деньГенерации = null)
        {
            try
            {
                //Проверить активность расписания
                if (_startDate.HasValue && _endDate.HasValue && деньГенерации.HasValue &&
                   (_startDate.Value.Date > деньГенерации.Value.Date || _endDate.Value.Date < деньГенерации.Value.Date))
                    return false;

                if ((BoundaryDaysActivity & 0x40000) != 0x00000 &&
                    (DateTime.DaysInMonth(date.AddMonths(-1).Year, date.AddMonths(-1).Month) % 2 != 0) &&
                    (ScheduleMode == ScheduleMode.ПоНечетным || ScheduleMode == ScheduleMode.ПоЧетным))
                {
                    switch (date.Day)
                    {
                        case 26: return ((BoundaryDaysActivity & 0x00001) != 0x00000);
                        case 27: return ((BoundaryDaysActivity & 0x00002) != 0x00000);
                        case 28: return ((BoundaryDaysActivity & 0x00004) != 0x00000);
                        case 29: return ((BoundaryDaysActivity & 0x00008) != 0x00000);
                        case 30: return ((BoundaryDaysActivity & 0x00010) != 0x00000);
                        case 31: return ((BoundaryDaysActivity & 0x00020) != 0x00000);
                        case 1: return ((BoundaryDaysActivity & 0x00040) != 0x00000);
                        case 2: return ((BoundaryDaysActivity & 0x00080) != 0x00000);
                        case 3: return ((BoundaryDaysActivity & 0x00100) != 0x00000);
                        case 4: return ((BoundaryDaysActivity & 0x00200) != 0x00000);
                        case 5: return ((BoundaryDaysActivity & 0x00400) != 0x00000);
                        case 6: return ((BoundaryDaysActivity & 0x00800) != 0x00000);
                        case 7: return ((BoundaryDaysActivity & 0x01000) != 0x00000);
                        case 8: return ((BoundaryDaysActivity & 0x02000) != 0x00000);
                        case 9: return ((BoundaryDaysActivity & 0x04000) != 0x00000);
                        case 10: return ((BoundaryDaysActivity & 0x08000) != 0x00000);
                        case 11: return ((BoundaryDaysActivity & 0x10000) != 0x00000);
                        case 12: return ((BoundaryDaysActivity & 0x20000) != 0x00000);
                    }
                }

                switch (ScheduleMode)
                {
                    case ScheduleMode.Отсутствует:
                        return false;


                    case ScheduleMode.Ежедневно:
                        return true;


                    case ScheduleMode.ПоЧетным:
                        return date.Day % 2 == 0 ? true : false;


                    case ScheduleMode.ПоНечетным:
                        return date.Day % 2 != 0 ? true : false;


                    case ScheduleMode.Выборочно:
                        return DayDictionary.ContainsKey(date.Date) && DayDictionary[date.Date];


                    case ScheduleMode.ПоДням:
                        byte ДеньНедели = (byte)(((byte)date.DayOfWeek + 6) % 7);
                        if ((WeekDaysActivity & 0x00FF) != 0x0000) // По дням
                        {
                            if ((WeekDaysActivity & (0x0001 << ДеньНедели)) != 0x0000)
                                return true;
                        }
                        else // Кроме дней
                        {
                            if ((WeekDaysActivity & (0x0100 << ДеньНедели)) == 0x0000)
                                return true;
                        }
                        return false;
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }

            return false;
        }

        public bool IsActive(DateTime date, DateTime? деньГенерации = null)
        {
            if (_startDate.HasValue && _endDate.HasValue && деньГенерации.HasValue &&
               (_startDate.Value.Date > деньГенерации.Value.Date || _endDate.Value.Date < деньГенерации.Value.Date))
                return false;

            bool result;
            return DayDictionary.TryGetValue(date.Date, out result);
        }

        public string ПолучитьСтрокуОписанияРасписания()
        {
            string СтрокаРасписания = "Режим работы: ";

            switch (ScheduleMode)
            {
                case ScheduleMode.Отсутствует:
                    СтрокаРасписания += "Движение отсутствует";
                    break;

                case ScheduleMode.Ежедневно:
                    СтрокаРасписания += "Ежедневно";
                    break;

                case ScheduleMode.ПоЧетным:
                    СтрокаРасписания += "По четным дням";
                    break;

                case ScheduleMode.ПоНечетным:
                    СтрокаРасписания += "По нечетным дням";
                    break;

                case ScheduleMode.Выборочно:
                    СтрокаРасписания += "Выборочные дни: ";
                    for (int i = 0; i < 12; i++)
                    {
                        int monthIndex = i + DateTime.Now.Month - 1;
                        var firstDay = new DateTime(DateTime.Now.Year + monthIndex / 12, monthIndex % 12 + 1, 1);
                        if (DayDictionary.Where(d => d.Key.Date >= firstDay &&
                                                      d.Key.Date < firstDay.AddMonths(1) &&
                                                      d.Value).Any())
                        {
                            СтрокаРасписания += CultureInfo.CreateSpecificCulture("ru-RU").DateTimeFormat.GetMonthName(monthIndex % 12 + 1) + ":";
                            for (var j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                                if (DayDictionary[j.Date])
                                    СтрокаРасписания += (j.Day).ToString() + ",";
                        }
                    }

                    if (СтрокаРасписания.Length > 1)
                        if (СтрокаРасписания[СтрокаРасписания.Length - 1] == ',')
                            СтрокаРасписания = СтрокаРасписания.Remove(СтрокаРасписания.Length - 1);
                    break;

                case ScheduleMode.ПоДням:
                    string[] НазваниеДнейНедели = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
                    if ((WeekDaysActivity & 0x007F) != 0x0000)
                    {
                        СтрокаРасписания += "По дням недели: ";
                        for (int i = 0; i < 7; i++)
                            if ((WeekDaysActivity & (0x0001 << i)) != 0x0000)
                                СтрокаРасписания += НазваниеДнейНедели[i] + ",";
                    }
                    else if ((WeekDaysActivity & 0x7F00) != 0x0000)
                    {
                        СтрокаРасписания += "Кроме дней недели: ";
                        for (int i = 0; i < 7; i++)
                            if ((WeekDaysActivity & (0x0100 << i)) != 0x0000)
                                СтрокаРасписания += НазваниеДнейНедели[i] + ",";
                    }

                    if (СтрокаРасписания.Length > 1)
                        if (СтрокаРасписания[СтрокаРасписания.Length - 1] == ',')
                            СтрокаРасписания = СтрокаРасписания.Remove(СтрокаРасписания.Length - 1);
                    break;
            }

            if (((BoundaryDaysActivity & 0x40000) != 0x00000) && ((BoundaryDaysActivity & 0x3FFFF) != 0x00000))
            {
                СтрокаРасписания += ". На границе месяца активные числа: ";
                if ((BoundaryDaysActivity & 0x00001) != 0x00000) СтрокаРасписания += "26,";
                if ((BoundaryDaysActivity & 0x00002) != 0x00000) СтрокаРасписания += "27,";
                if ((BoundaryDaysActivity & 0x00004) != 0x00000) СтрокаРасписания += "28,";
                if ((BoundaryDaysActivity & 0x00008) != 0x00000) СтрокаРасписания += "29,";
                if ((BoundaryDaysActivity & 0x00010) != 0x00000) СтрокаРасписания += "30,";
                if ((BoundaryDaysActivity & 0x00020) != 0x00000) СтрокаРасписания += "31,";
                if ((BoundaryDaysActivity & 0x00040) != 0x00000) СтрокаРасписания += "1,";
                if ((BoundaryDaysActivity & 0x00080) != 0x00000) СтрокаРасписания += "2,";
                if ((BoundaryDaysActivity & 0x00100) != 0x00000) СтрокаРасписания += "3,";
                if ((BoundaryDaysActivity & 0x00200) != 0x00000) СтрокаРасписания += "4,";
                if ((BoundaryDaysActivity & 0x00400) != 0x00000) СтрокаРасписания += "5,";
                if ((BoundaryDaysActivity & 0x00800) != 0x00000) СтрокаРасписания += "6,";
                if ((BoundaryDaysActivity & 0x01000) != 0x00000) СтрокаРасписания += "7,";
                if ((BoundaryDaysActivity & 0x02000) != 0x00000) СтрокаРасписания += "8,";
                if ((BoundaryDaysActivity & 0x04000) != 0x00000) СтрокаРасписания += "9,";
                if ((BoundaryDaysActivity & 0x08000) != 0x00000) СтрокаРасписания += "10,";
                if ((BoundaryDaysActivity & 0x10000) != 0x00000) СтрокаРасписания += "11,";
                if ((BoundaryDaysActivity & 0x20000) != 0x00000) СтрокаРасписания += "12,";
                СтрокаРасписания = СтрокаРасписания.Remove(СтрокаРасписания.Length - 1);
            }

            return СтрокаРасписания;
        }
        
        public DateTime FirstDayOfGoing()
        {
            return DayDictionary?.FirstOrDefault(d => d.Value && d.Key.Date >= DateTime.Now.Date).Key ?? (_startDate.HasValue ? _startDate.Value : DateTime.MinValue);
        }

        public DateTime LastDayOfGoing()
        {
            return DayDictionary?.LastOrDefault(d => d.Value && (_endDate.HasValue && d.Key.Date <= _endDate.Value || true)).Key ?? (_endDate.HasValue ? _endDate.Value : DateTime.MinValue);
        }

        // firstDate - общая начальная дата расписания текущего года не раньше текущего дня
        // lastDate - общая конечная дата расписания текущего года
        public string GetAlias(DateTime firstDate, DateTime lastDate)
        {
            var result = string.Empty;
            var list = DayDictionary.Where(d => d.Value).ToList();
            var dict = new SortedDictionary<DateTime, bool>();

            for (var i = firstDate; i <= lastDate; i = i.AddDays(1))
            {
                dict.Add(i.Date, list.Exists(d => d.Key.Date == i.Date));
            }

            // 0. Ежедневно
            result += GetByEveryDayString(dict, firstDate, lastDate);

            // 1. С привязкой к дням недели
            result += GetByDayOfWeekString(list, firstDate, lastDate);

            // 2. Без привязки к дням недели
            result += GetByParityString(list, firstDate, lastDate);

            // 3. Без привязки к чему-либо
            result += GetByDatesString(dict, firstDate, lastDate);

            return result;
        }


        private string GetByEveryDayString(IDictionary<DateTime, bool> dict, DateTime firstDate, DateTime lastDate)
        {
            var result = string.Empty;

            var trueCount = 0;
            var falseCount = 0;
            foreach (var d in dict)
            {
                if (d.Value)
                {
                    trueCount++;
                }
                else
                {
                    falseCount++;
                }
            }

            var fd = dict.FirstOrDefault(d => d.Value).Key;
            var ld = dict.LastOrDefault(d => d.Value).Key;
            if (falseCount == 0 || (int)((ld.Date - fd.Date).TotalDays) / falseCount >= 30)
            {
                result += "Ежедневно";

                if (fd.Date > firstDate)
                {
                    Library.Logs.Log.log.Info($"Поезд {TrainNumber} {TrainName}. fd > firstDate: {fd.ToShortDateString()} > {firstDate.ToShortDateString()}");
                    result += $" с {fd.ToString("dd/MM")}";
                }

                if (ld.Date < lastDate)
                {
                    Library.Logs.Log.log.Info($"Поезд {TrainNumber} {TrainName}. fd > firstDate: {ld.ToShortDateString()} > {lastDate.ToShortDateString()}");
                    result += $" по {ld.ToString("dd/MM")}";
                }

                if (((int)((ld - fd).TotalDays) == trueCount))
                {
                    Library.Logs.Log.log.Info($"Поезд {TrainNumber} {TrainName}. trueCount = {trueCount}, ld-fd = {(ld - fd).TotalDays}");
                    return result;
                }
                else
                {
                    Library.Logs.Log.log.Info($"Поезд {TrainNumber} {TrainName}. trueCount = {trueCount}, ld-fd = {(ld - fd).TotalDays}");
                    result += $", кроме ";

                    var isFirstTime = true;
                    var isExist = false;
                    foreach (var d in dict)
                    {
                        if (!d.Value && d.Key.Date >= fd.Date && d.Key.Date <= ld.Date)
                        {
                            if (!isFirstTime)
                            {
                                result += ",";
                                isFirstTime = false;

                                if (!isExist)
                                    result += " ";
                            }

                            isExist = true;
                            result += $"{d.Key.ToString("d")}";
                        }

                        if (isExist && d.Key.Day == DateTime.DaysInMonth(d.Key.Year, d.Key.Month))
                        {
                            result += $"{d.Key.ToString("/M")}";
                            isExist = false;
                        }
                    }
                }
            }
            return result;
        }

        private string GetByDayOfWeekString(List<KeyValuePair<DateTime, bool>> list, DateTime firstDate, DateTime lastDate)
        {
            var result = string.Empty;

            var mondays = new Dictionary<DateTime, bool>();
            var tuesdays = new Dictionary<DateTime, bool>();
            var wednesdays = new Dictionary<DateTime, bool>();
            var thursdays = new Dictionary<DateTime, bool>();
            var fridays = new Dictionary<DateTime, bool>();
            var saturdays = new Dictionary<DateTime, bool>();
            var sundays = new Dictionary<DateTime, bool>();

            for (var i = firstDate; i <= lastDate; i = i.AddDays(1))
            {
                switch (i.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        mondays.Add(i, list.Exists(d => d.Key.Date == i.Date));
                        break;
                    case DayOfWeek.Tuesday:
                        tuesdays.Add(i, list.Exists(d => d.Key.Date == i.Date));
                        break;
                    case DayOfWeek.Wednesday:
                        wednesdays.Add(i, list.Exists(d => d.Key.Date == i.Date));
                        break;
                    case DayOfWeek.Thursday:
                        thursdays.Add(i, list.Exists(d => d.Key.Date == i.Date));
                        break;
                    case DayOfWeek.Friday:
                        fridays.Add(i, list.Exists(d => d.Key.Date == i.Date));
                        break;
                    case DayOfWeek.Saturday:
                        saturdays.Add(i, list.Exists(d => d.Key.Date == i.Date));
                        break;
                    case DayOfWeek.Sunday:
                        sundays.Add(i, list.Exists(d => d.Key.Date == i.Date));
                        break;
                }
            }

            result += GetDayOfWeekString(mondays, firstDate, lastDate);
            result += GetDayOfWeekString(tuesdays, firstDate, lastDate);
            result += GetDayOfWeekString(wednesdays, firstDate, lastDate);
            result += GetDayOfWeekString(thursdays, firstDate, lastDate);
            result += GetDayOfWeekString(fridays, firstDate, lastDate);
            result += GetDayOfWeekString(saturdays, firstDate, lastDate);
            result += GetDayOfWeekString(sundays, firstDate, lastDate);

            return result;
        }

        private string GetDayOfWeekString(IDictionary<DateTime, bool> dictionary, DateTime firstDate, DateTime lastDate)
        {
            var dateFrom = firstDate;
            var dateTo = lastDate;

            var result = string.Empty;
            if (dictionary.Where(d => d.Value).Count() > dictionary.Where(d => !d.Value).Count())
            {
                result += dictionary.FirstOrDefault().Key.ToString("ddd");

                if (!dictionary.FirstOrDefault().Value)
                {
                    dateFrom = dictionary.FirstOrDefault(d => d.Value).Key;
                    result += $" с {dateFrom.ToString("dd/MM")}";
                }

                if (!dictionary.LastOrDefault().Value)
                {
                    dateTo = dictionary.LastOrDefault(d => d.Value).Key;
                    result += $" по {dateTo.ToString("dd/MM")}";
                }

                if (dictionary.Where(d => !d.Value && d.Key.Date > dateFrom.Date && d.Key.Date < dateTo.Date).Any())
                {
                    result += " кроме";
                    foreach (var d in dictionary)
                    {
                        if (!d.Value)
                        {
                            result += $" {d.Key.ToString("dd/MM")}";
                        }
                    }
                }
            }
            else
            {
                foreach (var d in dictionary)
                {
                    if (d.Value)
                    {
                        result += $"{d.Key.ToString("dd/MM")}";
                    }
                }
            }
            return result;
        }

        private string GetByParityString(List<KeyValuePair<DateTime, bool>> list, DateTime firstDate, DateTime lastDate)
        {
            var result = string.Empty;

            var months = new Dictionary<int, string>();

            for (var i = firstDate; i <= lastDate; i = i.AddMonths(1))
            {
                if (list.LastOrDefault(d => d.Key.Month == i.Month).Key.Day % 2 != 0 &&
                                    !list.Exists(t => list.LastOrDefault(d => d.Key.Month == i.Month).Key.AddDays(-1).Date == t.Key.Date) &&
                                    list.Exists(t => list.LastOrDefault(d => d.Key.Month == i.Month).Key.AddDays(-2).Date == t.Key.Date))
                {
                    months.Add(i.Month, "Odd");
                }
                else if (list.LastOrDefault(d => d.Key.Month == i.Month).Key.Day % 2 == 0 &&
                                    list.Exists(t => list.LastOrDefault(d => d.Key.Month == i.Month).Key.AddDays(-1).Date == t.Key.Date) &&
                                    !list.Exists(t => list.LastOrDefault(d => d.Key.Month == i.Month).Key.AddDays(-2).Date == t.Key.Date))
                {
                    months.Add(i.Month, "Even");
                }
                else
                {
                    months.Add(i.Month, string.Empty);
                }
            }

            foreach (var m in months)
            {
                switch (m.Value)
                {
                    case "Odd":
                        result += "Нечет";
                        break;
                    case "Even":
                        result += "Четн";
                        break;
                }
            }

            return result;
        }

        private string GetByDatesString(IDictionary<DateTime, bool> dict, DateTime firstDate, DateTime lastDate)
        {
            var result = string.Empty;

            var trueCount = 0;
            foreach (var d in dict)
            {
                if (d.Value)
                    trueCount++;
            }

            var isFirstTime = true;
            var isExist = false;
            if (trueCount < 5)
            {
                foreach (var d in dict)
                {
                    if (!isFirstTime)
                    {
                        result += ",";
                        isFirstTime = false;

                        if (!isExist)
                            result += " ";
                    }

                    if (d.Value)
                        result += $"{d.Key.ToString("d")}";

                    if (isExist && d.Key.Day == DateTime.DaysInMonth(d.Key.Year, d.Key.Month))
                    {
                        result += $"{d.Key.ToString("/M")}";
                        isExist = false;
                    }
                }
            }
            else
            {
                result += "По датам";
            }

            return result;
        }

        private uint[] GetBeats()
        {
            var result = new uint[14];

            if (DayDictionary != null)
            {
                for (byte i = 0; i < 12; i++)
                {
                    uint tempUint = 0x00;
                    var firstDay = new DateTime(DateTime.Now.Year + (DateTime.Now.Month + i - 1) / 12, (DateTime.Now.Month + i - 1) % 12 + 1, 1);
                    for (var j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                    {
                        if (DayDictionary.ContainsKey(j.Date) && DayDictionary[j.Date])
                        {
                            tempUint |= (uint)(1 << (j.Day - 1));
                        }
                        else
                        {
                            tempUint &= (uint)((1 << (j.Day - 1)) ^ 0xFFFFFFFF);
                        }
                    }
                    result[(DateTime.Now.Month + i - 1) % 12] = tempUint;
                }
            }

            return result;
        }

        #endregion
    }
}
