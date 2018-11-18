using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationDevices.Behavior.GetDataBehavior;
using CommunicationDevices.DataProviders;
using Domain.Entitys;
using Library.Logs;
using MainExample.Entites;
using System.Windows.Forms;

namespace MainExample.Services.GetDataService
{
    class GetCisOperSh : GetSheduleAbstract
    {
        #region ctor

        public GetCisOperSh(BaseGetDataBehavior baseGetDataBehavior, SortedDictionary<string, SoundRecord> soundRecords) 
            : base(baseGetDataBehavior, soundRecords)
        {

        }

        #endregion




        #region Methode

        public override void GetaDataRxEventHandler(IEnumerable<UniversalInputType> data)
        {
            try
            {
                if (!Enable)
                    return;

                var universalInputTypes = data as IList<UniversalInputType> ?? data.ToList();
                var cisOperTable = TrainSheduleTable.GetCisOperTimetableAsync().GetAwaiter().GetResult();

                var tableRecords = new List<TrainTableRecord>(cisOperTable.Where(tr =>
                                   DateTime.Now - ParseDate(tr.Days, tr.ВремяНачалаДействияРасписания, tr.ВремяОкончанияДействияРасписания) < TimeSpan.FromDays(2)));
                
                if (universalInputTypes.Any())
                {
                    foreach (var uit in universalInputTypes)
                    {
                        if (!CheckContrains(uit))
                            continue;

                        try
                        {
                            ApplyFilter(uit);

                            uit.StationDeparture = Program.DirectionService.GetStationByCode(uit.StationDeparture.CodeEsr, uit.StationDeparture.CodeExpress, uit.StationDeparture.NameRu);
                            uit.StationArrival = Program.DirectionService.GetStationByCode(uit.StationArrival.CodeEsr, uit.StationArrival.CodeExpress, uit.StationArrival.NameRu);

                            if (uit.StationDeparture == null || uit.StationArrival == null)
                            {
                                Log.log.Warn($"Поезд {uit.NumberOfTrain} не принадлежит этой станции либо станции не найдены");
                                continue;
                            }

                            //var tableRec = tableRecords.FirstOrDefault(tr => TrainRecordAndUitCompare(tr, uit));
                            var tableRecs = GetTrainRecordByDataType(tableRecords, uit);
                            //var tableRec = GetTrainRecordByDataType(tableRecords, uit);
                            if (tableRecs != null && tableRecs.Any())
                            {
                                if (tableRecs.ToList().Exists(tr => tr.DenyAutoUpdate))
                                    continue;

                                DateTime date;
                                if (DateTime.TryParse(uit.DaysFollowing, out date))
                                {
                                    var tableRec = tableRecs.FirstOrDefault(tr => TrainRecordAndUitCompare(tr, uit));
                                    if (tableRec.Equals(default(TrainTableRecord)))
                                    {
                                        if (!GetCisRegSh.IsCreateRecord(tableRecords, uit, _baseGetDataBehavior.ThisStation.CodeEsr))
                                            continue;

                                        tableRec = tableRecs.FirstOrDefault(tr => IsContainsDate(tableRec, date));
                                        if (!tableRec.Equals(default(TrainTableRecord)))
                                        {
                                            tableRecords[tableRecords.IndexOf(tableRec)] = DeleteDateFromSchedule(tableRec, date);
                                        }
                                    }
                                    else
                                    {
                                        //if (!IsContainsDate(tableRec, date))
                                            tableRecords[tableRecords.IndexOf(tableRec)] = UpdateData(tableRec, uit, _baseGetDataBehavior.ThisStation.CodeEsr);
                                    }
                                }
                            }
                            else
                            {
                                if (!GetCisRegSh.IsCreateRecord(tableRecords, uit, _baseGetDataBehavior.ThisStation.CodeEsr))
                                    continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.log.Error($"Ошибка при обработке входных данных: {ex}");
                        }
                    }
                }

                if (tableRecords.Any())
                {                    
                     if (IsChangeInRemote(cisOperTable, tableRecords))// || IsChangeInLocal(tableRecords))
                    {
                        
                        TrainSheduleTable.SaveOperTimetable(tableRecords).GetAwaiter();
                        if (Program.Настройки.IsSuggestMainListUpdating &&
                            MessageBox.Show("Данные оперативного расписания изменились. Обновить данные в Основном окне?", 
                                            "Обнаружены новые данные расписания", MessageBoxButtons.YesNo, 
                                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes &&
                           MainWindowForm.myMainForm != null)
                        {
                            MainWindowForm.myMainForm.RefreshMainList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log.Error($"Ошибка в основном методе получения оперативного расписания: {ex}");
            }
        }

        public static bool IsContainsDate(TrainTableRecord tableRec, DateTime date)
        {
            bool result = false;
            if (!tableRec.Equals(default(TrainTableRecord)))
            {
                //var trainSchedule = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tableRec.Days).DayDictionary[date];
                //result = trainSchedule.ПолучитьАктивностьДняДвижения(date.Date);
                result = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tableRec.Days).DayDictionary[date];
            }
            return result;
        }

        public static TrainTableRecord DeleteDateFromSchedule(TrainTableRecord tableRec, DateTime date)
        {
            var schedule = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tableRec.Days);
            schedule.DayDictionary.AddOrUpdate(date.Date, false);
            schedule.ScheduleMode = ScheduleMode.Выборочно;
            tableRec.Days = schedule.ПолучитьСтрокуРасписания();
            return tableRec;
        }

        private static void CreateOrUpdateTrainRecord(TrainTableRecord tableRec, List<TrainTableRecord> tableRecords, UniversalInputType uit, int codeEsr)
        {
            if (!tableRec.Equals(default(TrainTableRecord)))
            {
                var index = tableRecords.IndexOf(tableRec);
                tableRec = UpdateData(tableRec, uit, codeEsr);

                var dayStr = GetScheduleString(uit.DaysFollowing, tableRec);
                if (dayStr != null)
                    tableRec.Days = dayStr;

                tableRecords[index] = tableRec;
            }
            else
            {
                //var cisTable = TrainSheduleTable.ЗагрузитьРасписаниеЦисAsync().GetAwaiter().GetResult();
                //var cisRec = cisTable?.FirstOrDefault(ct => ct.ScheduleId != 0 && ct.ScheduleId == uit.ScheduleId) ?? default(TrainTableRecord);

                int maxId = getMaxId(tableRecords);
                tableRec = CreateRecord(uit, codeEsr);
                if (tableRec.Equals(default(TrainTableRecord)))
                    return;

                tableRec.ID = ++maxId;

                var dayStr = GetScheduleString(uit.DaysFollowing, tableRec, tableRecords);
                if (dayStr != null)
                    tableRec.Days = dayStr;

                tableRecords.Add(tableRec);
            }
        }
        public static bool IsChangeInRemote(List<TrainTableRecord> remoteCisTable, List<TrainTableRecord> tableRecords)
        {
            //КОНТРОЛЬ ИЗМЕНЕНИЙ
            bool changeFlag = false;

            if (remoteCisTable != null)
            {
                if (remoteCisTable.Count == tableRecords.Count)
                {
                    foreach (var trCis in remoteCisTable)
                    {
                        var findElem = tableRecords.FirstOrDefault(t =>
                        {
                            var notChange = (t.Name == trCis.Name) &&
                                         (t.ArrivalTime == trCis.ArrivalTime) &&
                                         (t.DepartureTime == trCis.DepartureTime) &&
                                         //(t.StopTime == trCis.StopTime) &&
                                         (t.ВремяНачалаДействияРасписания == trCis.ВремяНачалаДействияРасписания) &&
                                         (t.ВремяОкончанияДействияРасписания == trCis.ВремяОкончанияДействияРасписания) &&
                                         (t.Addition == trCis.Addition) &&
                                         (t.StationArrival == trCis.StationArrival) &&
                                         (t.StationDepart == trCis.StationDepart) &&
                                         (t.Direction == trCis.Direction) &&
                                         (t.SoundTemplates == trCis.SoundTemplates);

                            return notChange;
                        });

                        //если такого элемента нет.
                        if (string.IsNullOrEmpty(findElem.Num))
                        {
                            changeFlag = true;
                            break;
                        }
                    }
                }
                else
                {
                    changeFlag = true;
                }
            }
            else
            {
                //удаленная таблица не созданна
                changeFlag = true;
            }
            return changeFlag;
        }

        public static bool IsChangeInLocal(List<TrainTableRecord> tableRecords)
        {
            bool changeFlag = false;
            var localTable = TrainSheduleTable.ЗагрузитьРасписаниеЛокальноеAsync().GetAwaiter().GetResult();
            for (var i = 0; i < tableRecords.Count; i++)
            {
                var tr = tableRecords[i];
                var localRec = localTable?.FirstOrDefault(t => t.Num == tr.Num &&
                                                               t.Num2 == tr.Num2 &&
                                                               (t.StationDepart == tr.StationDepart ||
                                                               t.StationArrival.Contains(tr.StationArrival)) &&
                                                               t.ArrivalTime == tr.ArrivalTime &&
                                                               t.DepartureTime == tr.DepartureTime) ?? new TrainTableRecord();

                tr.ID = i + 1;
                if (!localRec.Equals(default(TrainTableRecord)))
                {
                    tr.TrainPathDirection = localRec.TrainPathDirection;
                    changeFlag = true;
                }
                tableRecords[i] = tr;
            }
            return changeFlag;
        }

        public static TrainTableRecord CreateRecord(UniversalInputType uit, int codeEsr, TrainTableRecord tableRec = default(TrainTableRecord))
        {
            try
            {
                if (tableRec.Equals(default(TrainTableRecord)))
                {
                    tableRec.ScheduleId = uit.ScheduleId;

                    string num1, num2;
                    ParseTrainNumber(uit.NumberOfTrain, out num1, out num2);
                    tableRec.Num = num1 ?? string.Empty;
                    tableRec.Num2 = num2 ?? string.Empty;

                    tableRec.FollowingTime = uit.ViewBag.ContainsKey("ItenaryTime") ? uit.ViewBag["ItenaryTime"] : string.Empty;
                    tableRec.Active = true;
                    tableRec.TrainPathDirection = (byte)uit.VagonDirection;
                    tableRec.ChangeTrainPathDirection = false;
                    tableRec.ТипПоезда = (ТипПоезда)uit.TypeTrain;
                    //tableRec.Примечание = uit.Note ?? string.Empty;
                    Route route = Program.DirectionService.GetRouteWithNormalStopNames(uit.ViewBag.ContainsKey("Route") ? uit.ViewBag["Route"] : null);
                    tableRec.Примечание = route?.GetString(NotificationLanguage.Ru) ?? string.Empty;
                    tableRec.NoteEng = route?.GetString(NotificationLanguage.Eng) ?? string.Empty;
                    tableRec.ВремяНачалаДействияРасписания = uit.ViewBag.ContainsKey("ScheduleStartDateTime") ?
                                                             uit.ViewBag["ScheduleStartDateTime"] : DateTime.MinValue;
                    tableRec.ВремяОкончанияДействияРасписания = uit.ViewBag.ContainsKey("ScheduleEndDateTime") ?
                                                             uit.ViewBag["ScheduleEndDateTime"] : DateTime.MinValue;
                    tableRec.Addition = uit.Addition ?? string.Empty;
                    tableRec.AdditionEng = uit.AdditionEng ?? string.Empty;

                    tableRec.TrainPathNumber = new Dictionary<WeekDays, string>
                    {
                        [WeekDays.Постоянно] = string.Empty,
                        [WeekDays.Пн] = string.Empty,
                        [WeekDays.Вт] = string.Empty,
                        [WeekDays.Ср] = string.Empty,
                        [WeekDays.Чт] = string.Empty,
                        [WeekDays.Пт] = string.Empty,
                        [WeekDays.Сб] = string.Empty,
                        [WeekDays.Вс] = string.Empty
                    };
                    tableRec.PathWeekDayes = false;

                    tableRec.ИспользоватьДополнение = new Dictionary<string, bool>
                    {
                        ["звук"] = true,
                        ["табло"] = true
                    };

                    tableRec.Автомат = true;
                    tableRec.IsScoreBoardOutput = true;
                    tableRec.IsSoundOutput = true;
                }
                
                tableRec.TrnId = uit.TrnId;
                tableRec.DenyAutoUpdate = false;


                //try
                //{
                //uit.StationDeparture = Program.DirectionService.GetStationByCode(uit.StationDeparture.CodeEsr, uit.StationDeparture.CodeExpress, uit.StationDeparture.NameRu);
                //uit.StationArrival = Program.DirectionService.GetStationByCode(uit.StationArrival.CodeEsr, uit.StationArrival.CodeExpress, uit.StationArrival.NameRu);
                //}
                //catch (Exception ex)
                //{
                //    Log.log.Error($"Ошибка при получении данных о станциях: {ex}");
                //}


                tableRec.StationDepart = uit.StationDeparture?.NameRu ?? string.Empty;
                tableRec.StationArrival = uit.StationArrival?.NameRu ?? string.Empty;

                int num;
                tableRec.Direction = Program.DirectionService.GetDirection(uit.StationDeparture, uit.StationArrival, int.TryParse(tableRec.Num, out num) ? num : 0) ?? string.Empty;

                tableRec.Name = $"{tableRec.StationDepart} - {tableRec.StationArrival}" ?? string.Empty;

                var arrivalTime = uit.TransitTime != null && uit.TransitTime.ContainsKey("приб") ? uit.TransitTime["приб"] : DateTime.MinValue;
                var departureTime = uit.TransitTime != null && uit.TransitTime.ContainsKey("отпр") ? uit.TransitTime["отпр"] : DateTime.MinValue;
                tableRec.ArrivalTime = arrivalTime != DateTime.MinValue ? arrivalTime.ToString("HH:mm") : string.Empty;
                tableRec.DepartureTime = departureTime != DateTime.MinValue ? departureTime.ToString("HH:mm") : string.Empty;
                //tableRec.StopTime = arrivalTime != DateTime.MinValue && departureTime > arrivalTime ?
                //                    (arrivalTime - departureTime).Minutes.ToString() : 
                //                    string.Empty;
                tableRec.StopTime = arrivalTime != DateTime.MinValue && departureTime != DateTime.MinValue ?
                                        (departureTime < arrivalTime ? 
                                        departureTime.AddDays(1) - arrivalTime : 
                                        departureTime - arrivalTime).ToString("hh\\:mm") :
                                    string.Empty;
                tableRec.SoundTemplates = GenerateTemplate(uit, tableRec, codeEsr) ?? string.Empty;

                var schedule = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(string.Empty);
                DateTime date;
                if (DateTime.TryParse(uit.DaysFollowing, out date))
                {
                    schedule.DayDictionary.AddOrUpdate(date.Date, true);
                    schedule.ScheduleMode = ScheduleMode.Выборочно;
                }
                tableRec.Days = schedule.ПолучитьСтрокуРасписания();
                tableRec.DaysAlias = uit.DaysFollowingAlias ?? string.Empty;
                tableRec.DaysAliasEng = uit.DaysFollowingAliasEng ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log.log.Error($"Ошибка при создании новой записи поезда: {ex}");
                tableRec = default(TrainTableRecord);
            }

            return tableRec;
        }
        
        private bool TrainRecordAndUitCompare(TrainTableRecord tr, UniversalInputType uit)
        {
            var startStation = uit.StationDeparture?.NameRu ?? string.Empty;
            var endStation = uit.StationArrival?.NameRu ?? string.Empty;
            var arrivalTimeString = uit.TransitTime != null && uit.TransitTime.ContainsKey("приб") &&
                                      uit.TransitTime["приб"] != DateTime.MinValue ?
                                      uit.TransitTime["приб"].ToString("HH:mm") :
                                      string.Empty;
            var departureTimeString = uit.TransitTime != null && uit.TransitTime.ContainsKey("отпр") &&
                                      uit.TransitTime["отпр"] != DateTime.MinValue ?
                                      uit.TransitTime["отпр"].ToString("HH:mm") :
                                      string.Empty;
            Route route = Program.DirectionService.GetRouteWithNormalStopNames(uit.ViewBag.ContainsKey("Route") ? uit.ViewBag["Route"] : null);
            var routeString = route?.GetString(NotificationLanguage.Ru) ?? string.Empty;

            var isCommon = tr.ScheduleId != 0 &&
                           tr.ScheduleId == uit.ScheduleId &&
                           tr.StationDepart == startStation &&
                           tr.StationArrival == endStation &&
                           tr.ArrivalTime == arrivalTimeString &&
                           tr.DepartureTime == departureTimeString;

            return isCommon && (uit.InDataType == InDataType.TrainsOper ||
                   (uit.InDataType == InDataType.LocalTrainsOper && tr.Примечание == routeString));
        }

        private static bool TrainTableRecordCompare(TrainTableRecord tr1, TrainTableRecord tr2)
        {
            return tr1.ScheduleId != 0 && 
                   tr1.ScheduleId == tr2.ScheduleId &&
                   tr1.StationDepart == tr2.StationDepart &&
                   tr1.StationArrival == tr2.StationArrival &&
                   tr1.ArrivalTime == tr2.ArrivalTime &&
                   tr1.DepartureTime == tr2.DepartureTime &&
                   tr1.Примечание == tr2.Примечание;
        }
        

        public static TrainTableRecord UpdateData(TrainTableRecord tableRec, UniversalInputType uit, int codeEsr)
        {
            try
            {
                tableRec.Addition = uit.Addition ?? string.Empty;
                //uit.StationDeparture = Program.DirectionService.GetStationByCode(uit.StationDeparture.CodeEsr, uit.StationDeparture.CodeExpress);
                //uit.StationArrival = Program.DirectionService.GetStationByCode(uit.StationArrival.CodeEsr, uit.StationArrival.CodeExpress);

                tableRec.StationDepart = uit.StationDeparture?.NameRu ?? string.Empty;
                tableRec.StationArrival = uit.StationArrival?.NameRu ?? string.Empty;
                tableRec.Name = $"{tableRec.StationDepart} - {tableRec.StationArrival}";

                var arrivalTime = uit.TransitTime != null && uit.TransitTime.ContainsKey("приб") ? uit.TransitTime["приб"] : DateTime.MinValue;
                var departureTime = uit.TransitTime != null && uit.TransitTime.ContainsKey("отпр") ? uit.TransitTime["отпр"] : DateTime.MinValue;
                tableRec.ArrivalTime = arrivalTime != DateTime.MinValue ? arrivalTime.ToString("HH:mm") : string.Empty;
                tableRec.DepartureTime = departureTime != DateTime.MinValue ? departureTime.ToString("HH:mm") : string.Empty;
                //tableRec.StopTime = arrivalTime != DateTime.MinValue && departureTime > arrivalTime ?
                //                    (arrivalTime - departureTime).Minutes.ToString() : 
                //                    string.Empty;
                tableRec.StopTime = arrivalTime != DateTime.MinValue && departureTime != DateTime.MinValue ?
                                        (departureTime < arrivalTime ? 
                                        departureTime.AddDays(1) - arrivalTime : 
                                        departureTime - arrivalTime).ToString("hh\\:mm") :
                                    string.Empty;
                tableRec.SoundTemplates = GenerateTemplate(uit, tableRec, codeEsr) ?? string.Empty;

                Route route = Program.DirectionService.GetRouteWithNormalStopNames(uit.ViewBag.ContainsKey("Route") ? uit.ViewBag["Route"] : null);
                tableRec.Примечание = route?.GetString(NotificationLanguage.Ru) ?? string.Empty;
                tableRec.NoteEng = route?.GetString(NotificationLanguage.Eng) ?? string.Empty;
                tableRec.Days = GetScheduleString(uit.DaysFollowing, tableRec);
            }
            catch (Exception ex)
            {
                Log.log.Error($"Ошибка при попытке обновления записи: {ex}");
            }
            return tableRec;
        }
        
        public static string GetScheduleString(string source, TrainTableRecord tableRec, List<TrainTableRecord> tableRecords = null)
        {
            TrainSchedule trainSchedule;// = new ПланРасписанияПоезда();
            DateTime date;
            if (tableRecords != null && tableRecords.Exists(tr => TrainTableRecordCompare(tr, tableRec)))
            {
                var tRecs = new List<TrainTableRecord>(tableRecords); // копируем уже сформированный tableRecords
                foreach (var tr in tRecs) // проходимся по копии сформированного tableRecords
                {
                    if (TrainTableRecordCompare(tr, tableRec)) // находим совпадающий поезд
                    {
                        trainSchedule = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tr.Days); // получаем расписание этого поезда
                                                                                                            ///// одинаковый код
                        if (DateTime.TryParse(source, out date))
                        {
                            trainSchedule.DayDictionary.AddOrUpdate(date.Date, true); // добавляем дату
                            trainSchedule.ScheduleMode = ScheduleMode.Выборочно; // задаем выборочный режим (продумать что-то поинтересней)
                        }
                        tableRec.Days = trainSchedule.ПолучитьСтрокуРасписания(); // получаем строку из совмещенного
                                                                                  ///// одинаковый код
                        var index = tableRecords.IndexOf(tr);
                        tableRecords.RemoveAt(index);
                        tableRecords.Insert(index, tableRec); // обновляем запись новой информацией
                    }
                }
                return null; // ничего дальше не делаем
            }
            else
            {
                trainSchedule = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tableRec.Days);
            }

            // повтор кода их предыдущего условия
            if (DateTime.TryParse(source, out date))
            {

                trainSchedule.DayDictionary.AddOrUpdate(date.Date, true);
                trainSchedule.ScheduleMode = ScheduleMode.Выборочно;
            }
            return trainSchedule.ПолучитьСтрокуРасписания();
        }

              
        private static int getMaxId(List<TrainTableRecord> tableRecords)
        {
            int maxId = TrainTableOperative.TrainTableRecords.Any() ? TrainTableOperative.TrainTableRecords.Max(t => t.ID) : 0;
            int maxNewId = tableRecords != null && tableRecords.Any() ? tableRecords.Max(t => t.ID) : 0;
            return maxId > maxNewId ? maxId : maxNewId;
        }
        
        private static void ParseTrainNumber(string trainNumber, out string num1, out string num2)
        {
            string[] num;
            if (!string.IsNullOrWhiteSpace(trainNumber) && trainNumber.Contains('/'))
            {
                num = trainNumber.Split('/');
            }
            else
            {
                num = new string[] { trainNumber, string.Empty };
            }
            num1 = num[0];
            num2 = num[1];
        }
        
        private ICollection<TrainTableRecord> GetTrainRecordByDataType(List<TrainTableRecord> tableRecords, UniversalInputType uit)
        {
            List<TrainTableRecord> tableRecs = new List<TrainTableRecord>();
            if (tableRecords != null && tableRecords.Any())
            {
                if (uit.InDataType == InDataType.TrainsOper)
                {
                    tableRecs.AddRange(tableRecords.Where(tr => tr.TrnId != 0 && tr.TrnId == uit.TrnId));
                }
                else if (uit.InDataType == InDataType.LocalTrainsOper)
                {
                    tableRecs.AddRange(tableRecords.Where(tr => tr.ScheduleId == uit.ScheduleId));
                    //tableRec = tableRecords.FirstOrDefault(tr => TrainRecordAndUitCompare(tr, uit));
                }
            }
            return tableRecs;
        }

        private DateTime ParseDate(string str, DateTime startTime, DateTime endTime)
        {
            var trainSchedule = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(str, startTime, endTime); // получаем расписание этого поезда
            return trainSchedule.LastDayOfGoing();
        }

        
        #endregion

        #region Sound Templates
        public static string GenerateTemplate(UniversalInputType uit, TrainTableRecord tableRec, int codeEsr)
        {
            var temp = string.Empty;
            var comp = StringComparison.OrdinalIgnoreCase;
            List<string> templates = new List<string>();
            string source;
            if (Program.ШаблоныОповещения.Exists(t => !string.IsNullOrWhiteSpace(tableRec.Addition) && t.IndexOf(tableRec.Addition, comp) >= 0))
            {
                source = tableRec.Addition;
            }
            else
            {
                source = tableRec.ТипПоезда.ToString();
            }

            foreach (var t in Program.ШаблоныОповещения)
            {
                if (t.IndexOf(source, comp) >= 0)
                {
                    // игнорим шаблоны смены номера если номер не меняется
                    if (t.ToLower().Contains("смена номера") && (string.IsNullOrWhiteSpace(tableRec.Num2) || tableRec.Num == tableRec.Num2))
                    {
                        continue;
                    }
                    templates.Add(t);
                }
            }

            var resultTemplates = !string.IsNullOrWhiteSpace(tableRec.Num2) && tableRec.Num != tableRec.Num2 && tableRec.Num.Length < 4 ?
                                  templates.Where(t => t.ToLower().Contains("ожидается прибытием") ||
                                                       t.ToLower().Contains("прибывает") ||
                                                       t.ToLower().Contains("смена номера")) :
                                  templates.Where(t => !t.ToLower().Contains("смена номера"));

            //var templates = Program.ШаблоныОповещения.Where(t => !string.IsNullOrWhiteSpace(tableRec.Addition) && t.IndexOf(tableRec.Addition, comp) >= 0 || t.IndexOf(tableRec.ТипПоезда.ToString(), comp) >= 0);
            if (!string.IsNullOrWhiteSpace(tableRec.ArrivalTime))
            {
                if (!string.IsNullOrWhiteSpace(tableRec.DepartureTime))
                {
                    TimeSpan stopTime;
                    stopTime = (TimeSpan.TryParse(tableRec.StopTime, out stopTime)) ? stopTime : TimeSpan.MinValue;
                    temp = codeEsr != 6007 ? 
                        tableRec.Num.Length < 4 ? GetStringTransitTemplate(resultTemplates, (int)stopTime.TotalMinutes) : GetStringLocalTrainTransitTemplate(resultTemplates, (int)stopTime.TotalMinutes) : 
                        GetStringTransitTemplateOld(resultTemplates, (int)stopTime.TotalMinutes);
                }
                else
                {
                    temp = codeEsr != 6007 ? GetStringArrivalTemplate(resultTemplates) : GetStringArrivalTemplateOld(resultTemplates);
                }
            }
            else
            {
                temp = codeEsr != 6007 ? 
                    tableRec.Num.Length < 4 ? GetStringDepartureTemplate(resultTemplates) : GetStringLocalTrainDepartureTemplate(resultTemplates) : 
                    GetStringDepartureTemplateOld(resultTemplates);
            }
            return temp;
        }

        private static string GetStringArrivalTemplate(IEnumerable<string> collection)
        {
            var str = string.Empty;
            foreach (var template in collection)
            {
                var s = template.ToLower();
                if (s.Contains("ожидается прибытием"))
                {
                    str += $"{template}:-10:0";
                }
                else if (s.Contains("прибывает"))
                {
                    str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-5:0";
                }
                else if (s.Contains("прибыл"))
                {
                    str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:0:0";
                }
            }
            return str;
        }

        private static string GetStringDepartureTemplate(IEnumerable<string> collection)
        {
            var str = string.Empty;
            foreach (var template in collection)
            {
                var s = template.ToLower();
                if (s.Contains("начинается посадка") || s.Contains("начало посадки"))
                {
                    str += $"{template}:-30:1";
                }
                else if (s.Contains("продолжается посадка") || s.Contains("продолжение посадки"))
                {
                    str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-20,-10:1";
                }
                else if (s.Contains("заканчивается посадка") && s.Contains("5 минут"))
                {
                    str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-5:1";
                }
                else if (s.Contains("отправляется"))
                {
                    str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-2:1";
                }
            }
            return str;
        }

        private static string GetStringLocalTrainDepartureTemplate(IEnumerable<string> collection)
        {
            var str = string.Empty;
            foreach (var template in collection)
            {
                var s = template.ToLower();
                if (s.Contains("отправится"))
                {
                    str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-20,-15,-10,-7,-4:1";
                }
                else if (s.Contains("отправляется"))
                {
                    str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-1:1";
                }
            }
            return str;
        }

        private static string GetStringLocalTrainTransitTemplate(IEnumerable<string> collection, int stopTime)
        {
            var str = string.Empty;
            if (stopTime > 20)
            {
                str += GetStringArrivalTemplate(collection);
                str += GetStringLocalTrainDepartureTemplate(collection);
            }
            else if (stopTime > 15)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("отправится"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-15,-10,-7,-4:1";
                    }
                    else if (s.Contains("отправляется"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-1:1";
                    }
                }
            }
            else if (stopTime > 10)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("отправится"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-10,-7,-4:1";
                    }
                    else if (s.Contains("отправляется"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-1:1";
                    }
                }
            }
            else if (stopTime > 7)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("отправится"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-7,-4:1";
                    }
                    else if (s.Contains("отправляется"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-1:1";
                    }
                }
            }
            else if (stopTime > 4)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("отправится"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-4:1";
                    }
                    else if (s.Contains("отправляется"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-1:1";
                    }
                }
            }
            else if (stopTime > 1)
            {
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("ожидается прибытием"))
                    {
                        str += $"{template}:-10:0";
                    }
                    else if (s.Contains("прибывает"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-5:0";
                    }
                    else if (s.Contains("отправится"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:0:0";
                    }
                    else if (s.Contains("отправляется"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-1:1";
                    }
                }
            }
            else
            {
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("ожидается прибытием"))
                    {
                        str += $"{template}:-10:0";
                    }
                    else if (s.Contains("прибывает"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-5:0";
                    }
                    else if (s.Contains("отправится"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:0:0";
                    }
                }
            }
            return str;
        }

        private static string GetStringTransitTemplate(IEnumerable<string> collection, int stopTime)
        {
            var str = string.Empty;
            if (stopTime >= 15)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("начинается посадка") || s.Contains("начало посадки"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:1:0";
                    }
                    else if (s.Contains("продолжается посадка") || s.Contains("продолжение посадки"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:";
                        var i = (stopTime - 3) / 10;
                        while (i > 0)
                        {
                            str += $"-{i * 10}{(--i > 0 ? "," : "")}";
                        }
                        str += ":1";
                    }
                    else if (s.Contains("заканчивается посадка") && s.Contains("5 минут"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-5:1";
                    }
                    else if (s.Contains("отправляется"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-2:1";
                    }
                }
            }
            else if (stopTime >= 10)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("начинается посадка") || s.Contains("начало посадки"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:1:0";
                    }
                    else if (s.Contains("заканчивается посадка") && s.Contains("5 минут"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-5:1";
                    }
                    else if (s.Contains("отправляется"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-2:1";
                    }
                }
            }
            else if (stopTime >= 5)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("начинается посадка") || s.Contains("начало посадки"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:1:0";
                    }
                    else if (s.Contains("заканчивается посадка") && !s.Contains("5 минут"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-2:1";
                    }
                }
            }
            else if (stopTime > 1)
            {
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("ожидается прибытием"))
                    {
                        str += $"{template}:-10:0";
                    }
                    else if (s.Contains("прибывает"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-5:0";
                    }
                    if (s.Contains("начинается посадка") || s.Contains("начало посадки"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:0:0";
                    }
                    else if (s.Contains("заканчивается посадка") && !s.Contains("5 минут"))
                    {
                        str += $"{(!string.IsNullOrWhiteSpace(str) ? ":" : "")}{template}:-1:1";
                    }
                }
            }
            else
            {
                str += GetStringArrivalTemplate(collection);
            }
            return str;
        }

        private static string GetStringArrivalTemplateOld(IEnumerable<string> collection)
        {
            var str = string.Empty;
            foreach (var template in collection)
            {
                var s = template.ToLower();
                if (s.Contains("ожидается прибытием"))
                {
                    str += $"{template}:-10:0:";
                }
                else if (s.Contains("прибывает"))
                {
                    str += $"{template}:-5:0:";
                }
                else if (s.Contains("прибыл"))
                {
                    str += $"{template}:0:0";
                }
            }
            return str;
        }

        private static string GetStringDepartureTemplateOld(IEnumerable<string> collection)
        {
            var str = string.Empty;
            foreach (var template in collection)
            {
                var s = template.ToLower();
                if (s.Contains("будет подан под посадку"))
                {
                    str += $"{template}:-40:1:";
                }
                else if (s.Contains("начинается посадка"))
                {
                    str += $"{template}:-30:1:";
                }
                else if (s.Contains("продолжается посадка"))
                {
                    str += $"{template}:-20,-10:1:";
                }
                else if (s.Contains("заканчивается посадка") && s.Contains("5 минут"))
                {
                    str += $"{template}:-5:1:";
                }
                else if (s.Contains("заканчивается посадка"))
                {
                    str += $"{template}:-2:1";
                }
            }
            return str;
        }

        private static string GetStringTransitTemplateOld(IEnumerable<string> collection, int stopTime)
        {
            var str = string.Empty;
            if (stopTime > 40)
            {
                str += GetStringArrivalTemplate(collection);
                str += GetStringDepartureTemplate(collection);
            }
            else if (stopTime > 30)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("начинается посадка"))
                    {
                        str += $"{template}:-30:1:";
                    }
                    else if (s.Contains("продолжается посадка"))
                    {
                        str += $"{template}:-20,-10:1:";
                    }
                    else if (s.Contains("заканчивается посадка") && s.Contains("5 минут"))
                    {
                        str += $"{template}:-5:1:";
                    }
                    else if (s.Contains("заканчивается посадка"))
                    {
                        str += $"{template}:-2:1";
                    }
                }
            }
            else if (stopTime >= 25)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("начинается посадка"))
                    {
                        str += $"{template}:1:0:";
                    }
                    else if (s.Contains("продолжается посадка"))
                    {
                        str += $"{template}:-20,-10:1:";
                    }
                    else if (s.Contains("заканчивается посадка") && s.Contains("5 минут"))
                    {
                        str += $"{template}:-5:1:";
                    }
                    else if (s.Contains("заканчивается посадка"))
                    {
                        str += $"{template}:-2:1";
                    }
                }
            }
            else if (stopTime >= 15)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("начинается посадка"))
                    {
                        str += $"{template}:1:0:";
                    }
                    else if (s.Contains("продолжается посадка"))
                    {
                        str += $"{template}:-10:1:";
                    }
                    else if (s.Contains("заканчивается посадка") && s.Contains("5 минут"))
                    {
                        str += $"{template}:-5:1:";
                    }
                    else if (s.Contains("заканчивается посадка"))
                    {
                        str += $"{template}:-2:1";
                    }
                }
            }
            else if (stopTime >= 10)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("начинается посадка"))
                    {
                        str += $"{template}:1:0:";
                    }
                    else if (s.Contains("заканчивается посадка") && s.Contains("5 минут"))
                    {
                        str += $"{template}:-5:1:";
                    }
                    else if (s.Contains("заканчивается посадка"))
                    {
                        str += $"{template}:-2:1";
                    }
                }
            }
            else if (stopTime >= 5)
            {
                str += GetStringArrivalTemplate(collection);
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("начинается посадка"))
                    {
                        str += $"{template}:1:0:";
                    }
                    else if (s.Contains("заканчивается посадка") && !s.Contains("5 минут"))
                    {
                        str += $"{template}:-2:1";
                    }
                }
            }
            else if (stopTime > 1)
            {
                foreach (var template in collection)
                {
                    var s = template.ToLower();
                    if (s.Contains("ожидается прибытием"))
                    {
                        str += $"{template}:-10:0:";
                    }
                    else if (s.Contains("прибывает"))
                    {
                        str += $"{template}:-5:0:";
                    }
                    if (s.Contains("начинается посадка"))
                    {
                        str += $"{template}:0:0:";
                    }
                    else if (s.Contains("заканчивается посадка") && !s.Contains("5 минут"))
                    {
                        str += $"{template}:-1:1";
                    }
                }
            }
            else
            {
                str += GetStringArrivalTemplate(collection);
            }
            return str;
        }
        #endregion
    }
}
