using System;
using System.Collections.Generic;
using System.Linq;
using CommunicationDevices.Behavior.GetDataBehavior;
using CommunicationDevices.DataProviders;

namespace MainExample.Services.GetDataService
{
    public class GetSheduleApkDk : GetSheduleAbstract
    {
        #region ctor

        public GetSheduleApkDk(BaseGetDataBehavior baseGetDataBehavior, SortedDictionary<string, SoundRecord> soundRecords) 
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
            if (!Enable)
                return;

            if (data != null && data.Any())
            {
                var trainWithPut = data.Where(sh => !(string.IsNullOrEmpty(sh.PathNumber) || string.IsNullOrWhiteSpace(sh.PathNumber))).ToList();
                foreach (var tr in trainWithPut)
                {
                    //DEBUG------------------------------------------------------
                    //var str = $"N= {tr.Ntrain}  Путь= {tr.Put}  Дата отпр={tr.DtOtpr:d}  Время отпр={tr.TmOtpr:g}  Дата приб={tr.DtPrib:d} Время приб={tr.TmPrib:g}  Ст.Приб {tr.StFinish}   Ст.Отпр {tr.StDeparture}";
                    //Log.log.Fatal("ПОЕЗД ИЗ ПОЛУЧЕННОГО СПСИКА" + str);
                    //DEBUG-----------------------------------------------------
                    
                    var dayArrival = tr.TransitTime != null ? tr.TransitTime["приб"].Date : DateTime.MinValue.Date;        //день приб.
                    var dayDepart = tr.TransitTime != null ? tr.TransitTime["отпр"].Date : DateTime.MinValue.Date;         //день отпр.
                    var stationArrival = tr.StationArrival.NameRu;       //станция приб.
                    var stationDepart = tr.StationDeparture.NameRu;      //станция отпр.

                    for (int i = 0; i < _soundRecords.Count; i++)
                    {
                        KeyValuePair<string, SoundRecord> record;
                        lock (MainWindowForm.SoundRecords_Lock)
                        {
                            record = _soundRecords.ElementAt(i);
                        }
                        var key = record.Key;
                        var rec = record.Value;

                        var idTrain = rec.IdTrain;

                        //ТРАНЗИТ
                        if (dayArrival != DateTime.MinValue && dayDepart != DateTime.MinValue)
                        {
                            var numberOfTrain = (string.IsNullOrEmpty(rec.НомерПоезда2) || string.IsNullOrWhiteSpace(rec.НомерПоезда2)) ? rec.НомерПоезда : (rec.НомерПоезда + "/" + rec.НомерПоезда2);
                            if (tr.NumberOfTrain == numberOfTrain &&
                                dayArrival == rec.ВремяПрибытия.Date &&
                                dayDepart == rec.ВремяОтправления.Date &&
                                (stationDepart.ToLower().Contains(rec.СтанцияОтправления.ToLower()) || rec.СтанцияОтправления.ToLower().Contains(stationArrival.ToLower())) &&
                                (stationArrival.ToLower().Contains(rec.СтанцияНазначения.ToLower()) || rec.СтанцияНазначения.ToLower().Contains(stationArrival.ToLower())))
                            {
                                // Log.log.Fatal("ТРАНЗИТ: " + numberOfTrain);//DEBUG
                                rec.НомерПути = tr.PathNumber;
                                lock (MainWindowForm.SoundRecords_Lock)
                                {
                                    _soundRecords[key] = rec;
                                }
                                break;
                            }
                        }
                        //ПРИБ.
                        else
                        if (dayArrival != DateTime.MinValue && dayDepart == DateTime.MinValue)
                        {
                            if (tr.NumberOfTrain == rec.НомерПоезда &&
                                dayArrival == rec.ВремяПрибытия.Date &&
                                (stationDepart.ToLower().Contains(rec.СтанцияОтправления.ToLower()) || rec.СтанцияОтправления.ToLower().Contains(stationArrival.ToLower())) &&
                                (stationArrival.ToLower().Contains(rec.СтанцияНазначения.ToLower()) || rec.СтанцияНазначения.ToLower().Contains(stationArrival.ToLower())))
                            {
                                //Log.log.Fatal("ПРИБ: " + rec.НомерПоезда);//DEBUG
                                rec.НомерПути = tr.PathNumber;
                                lock (MainWindowForm.SoundRecords_Lock)
                                {
                                    _soundRecords[key] = rec;
                                }
                                break;
                            }
                        }
                        //ОТПР.
                        else
                        if (dayDepart != DateTime.MinValue && dayArrival == DateTime.MinValue)
                        {
                            if (tr.NumberOfTrain == rec.НомерПоезда &&
                                dayDepart == rec.ВремяОтправления.Date &&
                                (stationDepart.ToLower().Contains(rec.СтанцияОтправления.ToLower()) || rec.СтанцияОтправления.ToLower().Contains(stationArrival.ToLower())) &&
                                (stationArrival.ToLower().Contains(rec.СтанцияНазначения.ToLower()) || rec.СтанцияНазначения.ToLower().Contains(stationArrival.ToLower())))
                            {
                                // Log.log.Fatal("ОТПР: " + rec.НомерПоезда);//DEBUG
                                rec.НомерПути = tr.PathNumber;
                                lock (MainWindowForm.SoundRecords_Lock)
                                {
                                    _soundRecords[key] = rec;
                                }
                                break;
                            }
                        }

                    }
                }
            }
        }

        #endregion
    }
}