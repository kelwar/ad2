using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationDevices.Behavior.GetDataBehavior;
using CommunicationDevices.DataProviders;
using Library.Logs;
using Domain.Entitys;

namespace MainExample.Services.GetDataService
{
    class GetCisCarNavigation : GetSheduleAbstract
    {
        public GetCisCarNavigation(BaseGetDataBehavior baseGetDataBehavior, SortedDictionary<string, SoundRecord> soundRecords) 
            : base(baseGetDataBehavior, soundRecords)
        {
        }

        public override void GetaDataRxEventHandler(IEnumerable<UniversalInputType> data)
        {
            try
            {
                if (!Enable)
                    return;
                
                var universalInputTypes = data as IList<UniversalInputType> ?? data.ToList();
                if (universalInputTypes.Any())
                {
                    try
                    {
                        if (_soundRecords == null)
                            return;

                        for (var i = 0; i < _soundRecords.Count; i++)
                        {
                            KeyValuePair<string, SoundRecord> record;
                            lock (MainWindowForm.SoundRecords_Lock)
                            {
                                record = _soundRecords.ElementAt(i);
                            }
                            var sr = record.Value;

                            // Запись не проходит проверку на допустимые данные приёма повагонки (например поезд пригородного сообщения)
                            if (!CheckContrains(universalInputTypes.FirstOrDefault(u => CompareTrain(sr, u))))
                                continue;

                            bool isLogWrite = false;
                            foreach (var uit in universalInputTypes)
                            {
                                try
                                {
                                    // Ищем среди данных только соответствующие данному поезду данные
                                    if (!CompareTrain(sr, uit))
                                    {
                                        continue;
                                    }

                                    var oldComposition = sr.Composition;
                                    sr.Composition = uit.ViewBag.ContainsKey("Composition") ? uit.ViewBag["Composition"] : null;
                                    
                                    if (sr.Composition == null)
                                    {
                                        if (uit.VagonDirection == VagonDirection.None)
                                            Program.CarNavigationLog($"Состав поезда №{sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()} пуст", Program.AuthenticationService?.CurrentUser);
                                        continue;
                                    }

                                    sr.Composition.ResortByNppVag();

                                    //if (sr.НумерацияПоезда == 0)
                                    //{
                                    //}

                                    //byte carNumbering = (byte)uit.VagonDirection;
                                    //if (carNumbering == 0 && sr.Composition.Vagons != null && sr.Composition.Vagons.Count >= 2)
                                    //{
                                    //    carNumbering = (byte)GetCarNumbering(sr, uit);
                                    //}
                                    //else
                                    //{
                                    //    Program.CarNavigationLog($"Состав поезда №{sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()} не имеет вагонов. Вагоны: {sr.Composition}. Нумерация: {stringCarNumbering}", Program.AuthenticationService?.CurrentUser);
                                    //}

                                    //if (sr.НумерацияПоезда == 0 && carNumbering != 0)
                                    //{
                                    //    sr.НумерацияПоезда = carNumbering;
                                    //}

                                    sr.Composition.CarNumbering = (CarNumbering)uit.VagonDirection;
                                    if (sr.НумерацияПоезда == 0)
                                    {
                                        sr.НумерацияПоезда = (byte)sr.Composition.CarNumbering;
                                    }

                                    if (sr.НумерацияПоезда != (byte)sr.Composition.CarNumbering)
                                    {
                                        Program.CarNavigationLog($"Нумерация поезда №{sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()}. {sr.Composition} локальная не соответствует нумерации из ЦИС", Program.AuthenticationService?.CurrentUser);
                                    }

                                    // Состава не было и он появился
                                    if (oldComposition == null && sr.Composition != null)
                                    {
                                        Program.CarNavigationLog($"Принят состав поезда №{sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()}. {sr.Composition}", Program.AuthenticationService?.CurrentUser);
                                    }

                                    if (oldComposition != null && sr.Composition != null && !sr.Composition.Equals(oldComposition))
                                    {
                                        Program.CarNavigationLog($"Изменен состав поезда №{sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()}. {sr.Composition}", Program.AuthenticationService?.CurrentUser);
                                    }
                                    
                                    if (IsNeedReverse(sr, uit))
                                    {
                                        sr.Composition.Reverse();
                                        Program.CarNavigationLog($"Выполнено условие визуального разворота состава поезда №{sr.НомерПоезда}. {sr.Composition}", Program.AuthenticationService?.CurrentUser);
                                    }
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Log.log.Error($"Ошибка при первичном формировании состава поезда: {ex}");
                                }
                            }

                            if (sr.Composition == null)
                            {
                                if (!isLogWrite && (sr.Время - DateTime.Now < TimeSpan.FromMinutes(40)) && (DateTime.Now - sr.Время < TimeSpan.FromMinutes(40)))
                                {
                                    Program.CarNavigationLog($"Повагонная навигация для поезда {sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()} не найдена", Program.AuthenticationService?.CurrentUser);
                                    isLogWrite = true;
                                }
                                continue;
                            }

                            lock (MainWindowForm.SoundRecords_Lock)
                            {
                                _soundRecords[record.Key] = sr;
                            }
                        }
                        #region OldCode
                        /*
                        foreach (var uit in universalInputTypes)
                        {
                            if (!CheckContrains(uit))
                                continue;

                            var isDeparture = uit.StationDeparture?.Equals(_baseGetDataBehavior.ThisStation) ?? false;
                            var isArrival = uit.StationArrival?.Equals(_baseGetDataBehavior.ThisStation) ?? false;

                            
                            for (var i = 0; i < _soundRecords.Count; i++)
                            {
                                KeyValuePair<string, SoundRecord> record;
                                lock (MainWindowForm.SoundRecords_Lock)
                                {
                                    record = _soundRecords.ElementAt(i);
                                }
                                var sr = record.Value;

                                bool isLogWrite = false;
                                if (CompareTrain(sr, uit))
                                {
                                    try
                                    {
                                        sr.Composition = uit.ViewBag.ContainsKey("Composition") ? uit.ViewBag["Composition"] : null;
                                        if (sr.Composition == null)
                                            continue;

                                        sr.Composition.ResortByNppVag();
                                        Log.log.Info($"Принят состав поезда №{sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()}");

                                        var track = Program.TrackRepository.List().FirstOrDefault(t => t.Name == sr.НомерПути);
                                        var trackWhereFrom = track?.Platform?.WhereFrom?.NameRu ?? string.Empty;
                                        var trackWhereTo = track?.Platform?.WhereTo?.NameRu ?? string.Empty;
                                        var whereFrom = uit.StationDeparture?.NearestStation ?? string.Empty;
                                        var whereTo = uit.StationArrival.NearestStation ?? string.Empty;

                                        if (sr.НумерацияПоезда == 0)
                                        {
                                            sr.НумерацияПоезда = (byte)sr.Composition.CarNumbering;

                                            if (sr.НумерацияПоезда == (byte)CarNumbering.Undefined &&
                                                sr.НомерПоезда.Length == 3 &&
                                                sr.НомерПоезда.StartsWith("7"))
                                            {
                                                sr.НумерацияПоезда = (byte)(isDeparture ? CarNumbering.Head : CarNumbering.Rear);
                                            }
                                        }

                                        if (((!string.IsNullOrWhiteSpace(whereFrom) ||
                                            !string.IsNullOrWhiteSpace(whereTo)) &&
                                            whereFrom == trackWhereFrom &&
                                            whereTo == trackWhereTo) ||
                                            (track == null &&
                                            string.IsNullOrWhiteSpace(whereFrom) &&
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

                                    lock (MainWindowForm.SoundRecords_Lock)
                                    {
                                        _soundRecords[record.Key] = sr;
                                    }
                                }
                                else
                                {
                                    if (!isLogWrite && (sr.Время - DateTime.Now < TimeSpan.FromMinutes(40)) && (DateTime.Now - sr.Время < TimeSpan.FromMinutes(40)))
                                    {
                                        Log.log.Info($"Повагонная навигация для поезда {sr.НомерПоезда} {sr.СтанцияОтправления} {sr.СтанцияНазначения} {sr.Время.ToLongDateString()} не найдена");
                                        isLogWrite = true;
                                    }
                                }
                            }
                        }*/
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Log.log.Fatal($"Ошибка в методе принятия данных о составе поезда {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }

        private bool CompareTrain(SoundRecord record, UniversalInputType uit)
        {
            if (record.IdTrain.TrnId != 0 && uit.TrnId != 0)
                return record.IdTrain.TrnId == uit.TrnId;

            var num = record.НомерПоезда;
            if (!string.IsNullOrWhiteSpace(record.НомерПоезда2) && record.НомерПоезда != record.НомерПоезда2)
                num += $"/{record.НомерПоезда2}";

            if (uit.TransitTime != null)
            {
                if (uit.TransitTime.ContainsKey("отпр"))
                    return num == uit.NumberOfTrain && record.ВремяОтправления == uit.TransitTime["отпр"];
                else if (uit.TransitTime.ContainsKey("приб"))
                    return num == uit.NumberOfTrain && record.ВремяПрибытия == uit.TransitTime["приб"];
            }

            var startStations = Program.DirectionService?.GetStationsByCode(uit?.StationDeparture?.CodeEsr ?? 0) ?? null;
            var endStations = Program.DirectionService?.GetStationsByCode(uit?.StationArrival?.CodeEsr ?? 0) ?? null;

            if (startStations == null || endStations == null)
                return false;

            DateTime startDate = uit.ViewBag.ContainsKey("StartDate") ? uit.ViewBag["StartDate"] : DateTime.MinValue;
            var sd = GetStartDate(record, uit);


            return num == uit.NumberOfTrain &&
                   startStations.ToList().Exists(s => record.СтанцияОтправления.Contains(s.NameRu)) &&
                   endStations.ToList().Exists(s => record.СтанцияНазначения.Contains(s.NameRu)) &&

                   ((sd.Date == startDate.Date &&
                   sd != record.ВремяПрибытия && sd != DateTime.MinValue && startDate != DateTime.MinValue) ||
                   (record.ВремяПрибытия > DateTime.Now || record.ВремяОтправления > DateTime.Now));
        }

        private DateTime GetStartDate(SoundRecord record, UniversalInputType uit)
        {
            var time = record.ВремяСледования.HasValue ? record.ВремяСледования.Value : DateTime.Parse("00:00");
            return uit.StationArrival?.Equals(_baseGetDataBehavior.ThisStation) ?? false ? 
                   record.ВремяПрибытия - TimeSpan.FromDays(time.Hour) - TimeSpan.FromHours(time.Minute) - TimeSpan.FromMinutes(time.Second) :
                        uit.StationDeparture?.Equals(_baseGetDataBehavior.ThisStation) ?? false ? 
                        record.ВремяОтправления :
                   DateTime.MinValue; // если транзит - пока ничего не делать
        }

        private CarNumbering GetCarNumbering(SoundRecord record, UniversalInputType uit)
        {
            var result = CarNumbering.Undefined;
            if ((record.Composition.Vagons == null || record.Composition.Vagons.Count < 3) &&
                record.НумерацияПоезда == (byte)CarNumbering.Undefined &&
                record.НомерПоезда.Length == 3 &&
                record.НомерПоезда.StartsWith("7"))
            {
                if (_baseGetDataBehavior.ThisStation.CodeEsr == 6007)
                {
                    result = uit.StationDeparture?.Equals(_baseGetDataBehavior.ThisStation) ?? false ?
                           CarNumbering.Head :
                           CarNumbering.Rear;
                    Program.CarNavigationLog($"Обнаружен скоростной поезд {record.НомерПоезда} {record.СтанцияОтправления} - {record.СтанцияНазначения} {record.Время.ToLongDateString()}. Применено условие нумерации вагонов сообщением между Ленинградским и Московским вокзалами", Program.AuthenticationService?.CurrentUser);

                }
                else if (_baseGetDataBehavior.ThisStation.CodeEsr == 3181)
                {
                    result = uit.StationDeparture?.Equals(_baseGetDataBehavior.ThisStation) ?? false ?
                           CarNumbering.Rear :
                           CarNumbering.Head;
                    Program.CarNavigationLog($"Обнаружен скоростной поезд {record.НомерПоезда} {record.СтанцияОтправления} - {record.СтанцияНазначения} {record.Время.ToLongDateString()}. Применено условие нумерации вагонов сообщением между Ленинградским и Московским вокзалами", Program.AuthenticationService?.CurrentUser);

                }
            }
            else
            {
                result = record.Composition.CarNumbering;
            }

            return result;
        }

        private bool IsNeedReverse(SoundRecord record, UniversalInputType uit)
        {
            var track = Program.TrackRepository.List().FirstOrDefault(t => t.Name == record.НомерПути);
            var trackWhereFrom = track?.Platform?.WhereFrom?.NameRu ?? string.Empty;
            var trackWhereTo = track?.Platform?.WhereTo?.NameRu ?? string.Empty;
            var whereFrom = uit.StationDeparture?.NearestStation ?? string.Empty;
            var whereTo = uit.StationArrival.NearestStation ?? string.Empty;


            return (((!string.IsNullOrWhiteSpace(whereFrom) ||
                !string.IsNullOrWhiteSpace(whereTo)) &&
                whereFrom == trackWhereFrom &&
                whereTo == trackWhereTo) ||
                (track == null &&
                string.IsNullOrWhiteSpace(whereFrom) &&
                !string.IsNullOrWhiteSpace(whereTo)));
        }
    }
}
