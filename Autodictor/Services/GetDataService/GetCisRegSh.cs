using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationDevices.Behavior.GetDataBehavior;
using CommunicationDevices.DataProviders;
using MainExample.Entites;
using MainExample.Mappers;
using Domain.Entitys;
using Domain.Entitys.Authentication;
using Domain.Abstract;
using Library.Logs;
using System.Windows.Forms;

namespace MainExample.Services.GetDataService
{
    class GetCisRegSh : GetSheduleAbstract
    {
        #region ctor

        public GetCisRegSh(BaseGetDataBehavior baseGetDataBehavior, SortedDictionary<string, SoundRecord> soundRecords) 
            : base(baseGetDataBehavior, soundRecords)
        {
        }

        #endregion




        #region Methode

        /// <summary>
        /// Обработка полученных данных
        /// </summary>
        public override void GetaDataRxEventHandler(IEnumerable<UniversalInputType> data)
        {
            try
            {
                if (!Enable)
                    return;

                var universalInputTypes = data as IList<UniversalInputType> ?? data.ToList();
                if (universalInputTypes.Any())
                {
                    #region UsersDb
                    if (universalInputTypes.FirstOrDefault(u => u.InDataType == InDataType.Users) != null)
                    {
                        //if (_getCisUsersDb == null)
                        //    _getCisUsersDb = new GetCisUsersDb(_baseGetDataBehavior, _soundRecords);
                        //_getCisUsersDb.GetaDataRxEventHandler(data);
                        //return;

                        var usersDb = Program.UsersDbRepository.List().ToList();
                        if (usersDb == null)
                            return;
                        usersDb.Clear();
                        Program.UsersDbRepository.Delete(u => true);
                        foreach (var uit in universalInputTypes)
                        {
                            try
                            {
                                User user = new User();

                                user.Id = uit.Id;
                                user.Login = uit.ViewBag["login"];
                                user.Password = uit.ViewBag["hash_salt_pass"];
                                int role_id = uit.ViewBag["role"];
                                switch (role_id)
                                {
                                    case 9:
                                        user.Role = Role.Диктор; break; // users - Основные пользователи
                                    case 1:
                                        user.Role = Role.Администратор; break; // imperator - Основная роль root админа
                                    case 7:
                                        user.Role = Role.Администратор; break; // administrator - Администратор ЦТС ПАСС
                                    case 8:
                                        user.Role = Role.Инженер; break; // sysadmin - Администратор ПТК
                                    case 3:
                                        user.Role = Role.Инженер; break; // apiReaders - Для чтения с API
                                    case 4:
                                        user.Role = Role.Инженер; break; // system - system
                                    case 5:
                                        user.Role = Role.Инженер; break; // daemon - Демоны
                                    default:
                                        user.Role = Role.Наблюдатель; break; // любой недокументированный id
                                }
                                user.IsEnabled = uit.ViewBag["status"];
                                user.StartDate = uit.ViewBag["start_date"];
                                user.EndDate = uit.ViewBag["end_date"];

                                usersDb.Add(user);
                            }
                            catch (Exception ex)
                            {
                                System.Windows.Forms.MessageBox.Show("Не получилось обновить репозиторий. Ошибка: " + ex);
                            }
                        }
                        Program.UsersDbRepository.AddRange(usersDb);
                    }
                    #endregion

                    #region CarNavigation
                    else if (universalInputTypes.FirstOrDefault(u => u.InDataType == InDataType.Vagons) != null)
                    {
                        //if (_getCisCarNavigation == null)
                        //    _getCisCarNavigation = new GetCisCarNavigation(_baseGetDataBehavior, _soundRecords);
                        //_getCisCarNavigation.GetaDataRxEventHandler(data);
                        //return;

                        try
                        {
                            if (_soundRecords == null)
                                return;

                            foreach (var uit in universalInputTypes)
                            {
                                //List<Direction> startDirs;
                                //List<Direction> endDirs;
                                //uit.StationDeparture = Program.GetStationByCode(uit.StationDeparture.CodeEsr, uit.StationDeparture.CodeExpress, uit.StationDeparture.NameRu, out startDirs);
                                //uit.StationArrival = Program.GetStationByCode(uit.StationArrival.CodeEsr, uit.StationArrival.CodeExpress, uit.StationArrival.NameRu, out endDirs);
                                var startStations = Program.DirectionService.GetStationsByCode(uit.StationDeparture.CodeEsr);
                                var endStations = Program.DirectionService.GetStationsByCode(uit.StationArrival.CodeEsr);
                                
                                if (startStations == null || endStations == null)
                                    continue;

                                var isDeparture = startStations.FirstOrDefault().CodeEsr == _baseGetDataBehavior.ThisStation.CodeEsr;
                                var isArrival = endStations.FirstOrDefault().CodeEsr == _baseGetDataBehavior.ThisStation.CodeEsr;


                                KeyValuePair<string, SoundRecord> record = default(KeyValuePair<string, SoundRecord>);
                                List<KeyValuePair<string, SoundRecord>> records = new List<KeyValuePair<string, SoundRecord>>();
                                for (var i = 0; i < _soundRecords.Count; i++)
                                {
                                    KeyValuePair<string, SoundRecord> rec;
                                    lock (MainWindowForm.SoundRecords_Lock)
                                    {
                                        rec = _soundRecords.ElementAt(i);
                                    }
                                    var r = rec.Value;
                                    var time = r.ВремяСледования.HasValue ? r.ВремяСледования.Value : DateTime.Parse("00:00");
                                    DateTime startDate = uit.ViewBag.ContainsKey("StartDate") ? uit.ViewBag["StartDate"] : DateTime.MinValue;
                                    var sd = isArrival ? r.ВремяПрибытия - TimeSpan.FromDays(time.Hour) - TimeSpan.FromHours(time.Minute) - TimeSpan.FromMinutes(time.Second) : 
                                             isDeparture ? r.ВремяОтправления : 
                                             DateTime.MinValue; // если транзит - пока ничего не делать

                                    bool isLogWrite = false;
                                    if (startStations.ToList().Exists(s => r.СтанцияОтправления.Contains(s.NameRu)) &&
                                        endStations.ToList().Exists(s => r.СтанцияНазначения.Contains(s.NameRu)) &&
                                        r.НомерПоезда == uit.NumberOfTrain && 
                                        ((sd.Date == startDate.Date &&
                                        sd != r.ВремяПрибытия && sd != DateTime.MinValue && startDate != DateTime.MinValue) ||
                                        (r.ВремяПрибытия > DateTime.Now || r.ВремяОтправления > DateTime.Now)))
                                    {
                                        record = rec;
                                        if (record.Key != null)
                                        {
                                            var sr = record.Value;
                                            if (sr.Composition == null)
                                            {
                                                try
                                                {
                                                    Log.log.Info($"Принят состав поезда №{sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()}");
                                                    var track = Program.TrackRepository.List().FirstOrDefault(t => t.Name == sr.НомерПути);
                                                    var trackWhereFrom = track?.Platform?.WhereFrom?.NameRu ?? string.Empty;
                                                    var trackWhereTo = track?.Platform?.WhereTo?.NameRu ?? string.Empty;
                                                    var whereFrom = startStations.FirstOrDefault()?.NearestStation ?? string.Empty;
                                                    var whereTo = endStations.FirstOrDefault()?.NearestStation ?? string.Empty;

                                                    sr.Composition = uit.ViewBag.ContainsKey("Composition") ? uit.ViewBag["Composition"] : null;
                                                    if (sr.Composition != null)
                                                        sr.Composition.ResortByNppVag();
                                                    if ((sr.Composition != null && (!string.IsNullOrWhiteSpace(whereFrom) || !string.IsNullOrWhiteSpace(whereTo)) &&
                                                        whereFrom == trackWhereFrom && whereTo == trackWhereTo) || (track == null && string.IsNullOrWhiteSpace(whereFrom) &&
                                                        !string.IsNullOrWhiteSpace(whereTo)))
                                                    {
                                                        Log.log.Info($"Выполнено условие разворота состава поезда №{sr.НомерПоезда}");
                                                        sr.Composition.Reverse();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.log.Error($"Ошибка при первичном формировании состава поезда: {ex}");
                                                }
                                            }

                                            lock (MainWindowForm.SoundRecords_Lock)
                                            {
                                                _soundRecords[record.Key] = sr;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!isLogWrite && (r.Время - DateTime.Now < TimeSpan.FromMinutes(40)) && (DateTime.Now - r.Время < TimeSpan.FromMinutes(40)))
                                        {
                                            Log.log.Info($"Повагонная навигация для поезда {r.НомерПоезда} {r.СтанцияОтправления} {r.СтанцияНазначения} {r.Время.ToLongDateString()} не найдена");
                                            isLogWrite = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.log.Fatal($"Ошибка в методе принятия данных о составе поезда {ex}");
                        }
                    }
                    #endregion

                    #region Trains
                    else if (universalInputTypes.FirstOrDefault(u => u.InDataType == InDataType.Trains) != null)
                    {

                        var tableRecords = new List<TrainTableRecord>(TrainSheduleTable.ЗагрузитьРасписаниеЦисAsync().GetAwaiter().GetResult());
                        foreach (var uit in universalInputTypes)
                        {
                            if (!CheckContrains(uit))
                                continue;

                            try
                            {
                                ApplyFilter(uit);
                                
                                uit.StationDeparture = Program.DirectionService.GetStationByCode(uit.StationDeparture.CodeEsr, uit.StationDeparture.CodeExpress, uit.StationDeparture.NameRu);
                                uit.StationArrival = Program.DirectionService.GetStationByCode(uit.StationArrival.CodeEsr, uit.StationArrival.CodeExpress, uit.StationArrival.NameRu);

                                if (uit.StationDeparture == null && uit.StationArrival == null)
                                {
                                    Log.log.Warn($"Поезд {uit.NumberOfTrain} не принадлежит этой станции либо станции не найдены");
                                }

                                //var tableRec = tableRecords.FirstOrDefault(tr => TrainRecordAndUitCompare(tr, uit));
                                var tableRecs = tableRecords.Where(tr => tr.ScheduleId != 0 && uit.ScheduleId == tr.ScheduleId);

                                //CreateOrUpdateTrainRecord(tableRec, tableRecords, uit, _baseGetDataBehavior.ThisStation.CodeEsr);
                                if (tableRecs != null && tableRecs.Any())
                                {
                                    if (tableRecs.ToList().Exists(tr => tr.DenyAutoUpdate))
                                        continue;

                                    DateTime date;
                                    if (DateTime.TryParse(uit.DaysFollowing, out date))
                                    {
                                        /*var trec = tableRecs.FirstOrDefault(tr => GetCisOperSh.IsContainsDate(tr, date));
                                        if (!trec.Equals(default(TrainTableRecord)) &&
                                            !TrainRecordAndUitCompare(trec, uit) && 
                                            trec.ArrivalTime == TimeToString(uit, "приб") && 
                                            trec.DepartureTime == TimeToString(uit, "отпр"))
                                        {
                                            // Склеить несовпадающие станции поезда в одну через пробел
                                        }
                                        else if (TrainRecordAndUitCompare(trec, uit))
                                        {
                                            tableRecords[tableRecords.IndexOf(trec)] = GetCisOperSh.UpdateData(trec, uit, _baseGetDataBehavior.ThisStation.CodeEsr);
                                        }
                                        else if (trec.Equals(default(TrainTableRecord)) &&
                                            !TrainRecordAndUitCompare(trec, uit))
                                        {
                                            if (!IsCreateRecord(tableRecords, uit, _baseGetDataBehavior.ThisStation.CodeEsr))
                                                continue;

                                            tableRec = tableRecs.FirstOrDefault(tr => GetCisOperSh.IsContainsDate(tableRec, date));
                                            if (!tableRec.Equals(default(TrainTableRecord)))
                                            {
                                                tableRecords[tableRecords.IndexOf(tableRec)] = GetCisOperSh.DeleteDateFromSchedule(tableRec, date);
                                            }
                                        }*/

                                        var tableRec = tableRecs.FirstOrDefault(tr => TrainRecordAndUitCompare(tr, uit));
                                        if (tableRec.Equals(default(TrainTableRecord)))
                                        {
                                            if (!IsCreateRecord(tableRecords, uit, _baseGetDataBehavior.ThisStation.CodeEsr))
                                                continue;

                                            tableRec = tableRecs.FirstOrDefault(tr => GetCisOperSh.IsContainsDate(tableRec, date));
                                            if (!tableRec.Equals(default(TrainTableRecord)))
                                            {
                                                tableRecords[tableRecords.IndexOf(tableRec)] = GetCisOperSh.DeleteDateFromSchedule(tableRec, date);
                                            }
                                        }
                                        else
                                        {
                                            //if (!GetCisOperSh.IsContainsDate(tableRec, date))
                                                tableRecords[tableRecords.IndexOf(tableRec)] = GetCisOperSh.UpdateData(tableRec, uit, _baseGetDataBehavior.ThisStation.CodeEsr);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!IsCreateRecord(tableRecords, uit, _baseGetDataBehavior.ThisStation.CodeEsr))
                                        continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.log.Warn("Поезд № " + uit.NumberOfTrain + " " + uit.StationDeparture.NameRu + " - " + uit.StationArrival.NameRu + " " + uit.Addition + "; Ошибка: " + ex);
                            }
                        }
                        if (tableRecords.Any())
                        {
                            var remoteCisTable = TrainSheduleTable.ЗагрузитьРасписаниеЦисAsync().GetAwaiter().GetResult();
                            if (GetCisOperSh.IsChangeInRemote(remoteCisTable, tableRecords))// || GetCisOperSh.IsChangeInLocal(tableRecords))
                            {
                                TrainSheduleTable.СохранитьИПрименитьСписокРегулярноеРасписаниеЦис(tableRecords).GetAwaiter();
                                if (Program.Настройки.IsSuggestMainListUpdating &&
                                    MessageBox.Show("Данные нормативного расписания изменились. Обновить данные в Основном окне?",
                                                    "Обнаружены новые данные расписания", MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes &&
                                   MainWindowForm.myMainForm != null)
                                {
                                    MainWindowForm.myMainForm.RefreshMainList();
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }

        public static bool IsCreateRecord(ICollection<TrainTableRecord> tableRecords, UniversalInputType uit, int codeEsr)
        {
            var tableRec = GetCisOperSh.CreateRecord(uit, codeEsr);
            if (tableRec.Equals(default(TrainTableRecord)))
                return false;

            int maxId = getMaxId(tableRecords);
            tableRec.ID = ++maxId;
            tableRecords.Add(tableRec);
            return true;
        }

        private bool TrainRecordAndUitCompare(TrainTableRecord tr, UniversalInputType uit)
        {
            var startStation = uit.StationDeparture?.NameRu ?? string.Empty;
            var endStation = uit.StationArrival?.NameRu ?? string.Empty;

            return tr.ScheduleId != 0 &&
                   tr.ScheduleId == uit.ScheduleId &&
                   tr.StationDepart == startStation &&
                   tr.StationArrival == endStation &&
                   tr.ArrivalTime == TimeToString(uit, "приб") &&
                   tr.DepartureTime == TimeToString(uit, "отпр");
        }

        private static string TimeToString(UniversalInputType uit, string moveEvent)
        {
            return uit.TransitTime != null && uit.TransitTime.ContainsKey(moveEvent) &&
                                      uit.TransitTime[moveEvent] != DateTime.MinValue ?
                                      uit.TransitTime[moveEvent].ToString("HH:mm") :
                                      string.Empty;
        }

        /*private static void CreateOrUpdateTrainRecord(TrainTableRecord tableRec, List<TrainTableRecord> tableRecords, UniversalInputType uit, int codeEsr)
        {
            if (!tableRec.Equals(default(TrainTableRecord)))
            {
                var index = tableRecords.IndexOf(tableRec);
                tableRec = GetCisOperSh.UpdateData(tableRec, uit, codeEsr);

                var dayStr = GetCisOperSh.GetScheduleString(uit.DaysFollowing, tableRec);
                if (dayStr != null)
                    tableRec.Days = dayStr;

                tableRecords[index] = tableRec;
            }
            else
            {
                var cisTable = TrainSheduleTable.ЗагрузитьРасписаниеЦисAsync().GetAwaiter().GetResult();
                var cisRec = cisTable?.FirstOrDefault(ct => ct.ScheduleId != 0 && ct.ScheduleId == uit.ScheduleId) ?? default(TrainTableRecord);

                int maxId = getMaxId(tableRecords);
                tableRec = GetCisOperSh.CreateRecord(uit, codeEsr, cisRec);
                tableRec.ID = ++maxId;

                var dayStr = GetCisOperSh.GetScheduleString(uit.DaysFollowing, tableRec, tableRecords);
                if (dayStr != null)
                    tableRec.Days = dayStr;

                tableRecords.Add(tableRec);
            }
        }*/

        private static int getMaxId(ICollection<TrainTableRecord> tableRecords)
        {
            int maxId = TrainSheduleTable.TrainTableRecords.Any() ? TrainSheduleTable.TrainTableRecords.Max(t => t.ID) : 0;
            int maxNewId = tableRecords != null && tableRecords.Any() ? tableRecords.Max(t => t.ID) : 0;
            return maxId > maxNewId ? maxId : maxNewId;
        }

        #endregion
    }
}
