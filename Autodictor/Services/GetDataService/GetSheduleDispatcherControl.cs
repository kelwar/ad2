using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CommunicationDevices.Behavior.GetDataBehavior;
using CommunicationDevices.DataProviders;
using Library.Logs;
using MainExample.Entites;
using Domain.Entitys.Train;

namespace MainExample.Services.GetDataService
{
    public class GetSheduleDispatcherControl : GetSheduleAbstract
    {
        #region ctor

        public GetSheduleDispatcherControl(BaseGetDataBehavior baseGetDataBehavior, SortedDictionary<string, SoundRecord> soundRecords) 
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
                foreach (var tr in data)
                {
                    #region Блок нового кода для приёма команды очистки данных
                    // Здесь разделяем данные на поезда и команды и если пришла команда сброса изменений - вызываем Program.SoundRecordChangesDbRepository.Delete(ch => true);
                    if (tr.InDataType == InDataType.Command && tr.ViewBag != null && tr.ViewBag.ContainsKey("Command") && tr.ViewBag["Command"] != null)
                    {
                        foreach (var cmd in (Dictionary<string, bool>)tr.ViewBag["Command"])
                        {
                            switch (cmd.Key)
                            {
                                case "RESET":
                                    if (cmd.Value)
                                    {
                                        Program.SoundRecordChangesDbRepository.Delete(ch => true);

                                        if (MainWindowForm.myMainForm != null)
                                        {
                                            MainWindowForm.myMainForm.RefreshMainList();
                                        }
                                    }
                                    break;
                                case "arrival":
                                    // TODO: описать логику кнопки ПРИБЫЛ
                                    break;
                                case "landing":
                                    // TODO: описать логику кнопки ПОСАДКА
                                    break;
                                case "departure":
                                    // TODO: описать логику кнопки ОТПРАВИЛСЯ
                                    break;
                            }
                        }
                        continue;
                    }

                    //DEBUG------------------------------------------------------
                    //var str = $" N= {tr.NumberOfTrain}  Путь= {tr.PathNumber}  Время отпр={dateTimeDepart:g}   Время приб={dateTimeArrival:g}  Ст.Приб {stationArrival}   Ст.Отпр {stationDepart}";
                    //Log.log.Trace("ПОЕЗД ИЗ ПОЛУЧЕННОГО СПСИКА" + str);
                    //DEBUG-----------------------------------------------------
                    #endregion

                    bool isExist = false;
                    for (int i = 0; i < _soundRecords.Count; i++)
                    {
                        #region Prepare
                        KeyValuePair<string, SoundRecord> record;
                        lock (MainWindowForm.SoundRecords_Lock)
                        {
                            record = _soundRecords.ElementAt(i);
                        }
                        var rec = record.Value;
                        var recOld = rec;

                        var idTrain = rec.IdTrain;
                        bool changeFlag = false;
                        #endregion

                        if (tr.NumberOfTrain == (string.IsNullOrWhiteSpace(rec.НомерПоезда2) ? rec.НомерПоезда : (rec.НомерПоезда + "/" + rec.НомерПоезда2)) &&
                            (tr.TransitTime != null && tr.TransitTime["приб"].Date == rec.ВремяПрибытия.Date || tr.TransitTime["отпр"].Date == rec.ВремяОтправления.Date) &&
                            IsContainsStationsName(tr.StationDeparture.NameRu, tr.StationArrival.NameRu, rec.СтанцияОтправления, rec.СтанцияНазначения))
                        {
                            isExist = true;
                            if (rec.БитыНештатныхСитуаций != tr.EmergencySituation)
                            {
                                rec.БитыНештатныхСитуаций = tr.EmergencySituation;
                                changeFlag = true;
                                Program.ЗаписьЛога("Действие диспетчера", "Выставлена нештатная ситуация", Program.AuthenticationService?.CurrentUser ?? null);
                                //Log.log.Trace("нашли изменения для ТРАНЗИТ. БитыНештатныхСитуаций: " + rec.БитыНештатныхСитуаций);//LOG    
                            }

                            #region Транзит
                            if (tr.TransitTime != null && tr.TransitTime["приб"] != DateTime.MinValue && tr.TransitTime["отпр"] != DateTime.MinValue &&
                                tr.TransitTime["приб"].Date == rec.ВремяПрибытия.Date && tr.TransitTime["отпр"].Date == rec.ВремяОтправления.Date)
                            {
                                if (rec.ВремяСтоянки != tr.StopTime)
                                {
                                    rec.ВремяСтоянки = tr.StopTime;
                                    changeFlag = true;
                                    //Log.log.Trace("нашли изменения для ТРАНЗИТ. ВремяСтоянки: " + rec.ВремяЗадержки);//LOG    
                                }

                                if (rec.ВремяПрибытия.ToString("yy.MM.dd  HH:mm") != tr.TransitTime["приб"].ToString("yy.MM.dd  HH:mm"))
                                {
                                    rec.ВремяПрибытия = tr.TransitTime["приб"];
                                    changeFlag = true;
                                    //Log.log.Trace("нашли изменения для ТРАНЗИТ. ВремяПрибытия: " + rec.ВремяПрибытия);//LOG    
                                }

                                if (rec.ВремяОтправления.ToString("yy.MM.dd  HH:mm") != tr.TransitTime["отпр"].ToString("yy.MM.dd  HH:mm"))
                                {
                                    rec.ВремяОтправления = tr.TransitTime["отпр"];
                                    rec.Время = rec.ВремяОтправления;
                                    changeFlag = true;
                                    //Log.log.Trace("нашли изменения для ТРАНЗИТ. ВремяОтправления: " + rec.ВремяОтправления);//LOG  
                                }
                                //Debug.WriteLine($"{rec.НазваниеПоезда} Время= {rec.Время} key= {key} ВремяПрибытия= {rec.ВремяПрибытия}  ВремяОтправления= {rec.ВремяОтправления}");     
                            }
                            #endregion

                            #region Прибытие
                            else
                            if (tr.TransitTime != null && tr.TransitTime["приб"] != DateTime.MinValue && tr.TransitTime["отпр"] == DateTime.MinValue &&
                                tr.TransitTime["приб"].Date == rec.ВремяПрибытия.Date)
                            {
                                if (rec.ВремяПрибытия.ToString("yy.MM.dd  HH:mm") != tr.TransitTime["приб"].ToString("yy.MM.dd  HH:mm"))
                                {
                                    rec.ВремяПрибытия = tr.TransitTime["приб"];
                                    rec.Время = rec.ВремяПрибытия;
                                    changeFlag = true;
                                    // Log.log.Trace("нашли изменения для ТРАНЗИТ. ВремяПрибытия: " + rec.ВремяПрибытия);//LOG  
                                }
                            }
                            #endregion

                            #region Отправление
                            else
                            if (tr.TransitTime != null && tr.TransitTime["отпр"] != DateTime.MinValue && tr.TransitTime["приб"] == DateTime.MinValue &&
                                tr.TransitTime["отпр"].Date == rec.ВремяОтправления.Date)
                            {
                                if (rec.ВремяОтправления.ToString("yy.MM.dd  HH:mm") != tr.TransitTime["отпр"].ToString("yy.MM.dd  HH:mm"))
                                {
                                    rec.ВремяОтправления = tr.TransitTime["отпр"];
                                    rec.Время = rec.ВремяОтправления;
                                    changeFlag = true;
                                    //Log.log.Trace("нашли изменения для ОТПР. ВремяОтправления: " + rec.ВремяОтправления);//LOG 
                                }
                            }


                            #endregion

                            var time = DateTime.Parse("00:00");
                            if ((rec.БитыНештатныхСитуаций & 0x02) != 0x00)
                            {
                                time = rec.ВремяПрибытия;
                            }
                            else if ((rec.БитыНештатныхСитуаций & 0x04) != 0x00 || (rec.БитыНештатныхСитуаций & 0x08) != 0x00)
                            {
                                time = rec.ВремяОтправления;
                            }
                            
                            if (rec.ВремяЗадержки != tr.ВремяЗадержки)
                            {
                                rec.ВремяЗадержки = tr.ВремяЗадержки;
                                var delay = rec.ВремяЗадержки.HasValue ? rec.ВремяЗадержки.Value : DateTime.Parse("00:00");
                                try
                                {
                                    rec.ОжидаемоеВремя = time.AddHours(delay.Minute).AddMinutes(delay.Second);
                                }
                                catch (Exception ex)
                                {
                                    Log.log.Error($"Исключение при попытке сохранить ожидаемое время от диспетчера. {ex}");
                                }
                                changeFlag = true;
                                //Log.log.Trace("нашли изменения для ТРАНЗИТ. ВремяЗадержки: " + rec.ВремяЗадержки);//LOG    
                            }
                            else
                            {

                            }

                            /*if (cBПрибытиеЗадерживается.Checked)
                            {
                                rec.ActualArrivalTime = rec.ОжидаемоеВремя != rec.ВремяПрибытия ? rec.ОжидаемоеВремя : rec.ОжидаемоеВремя.AddDays(1);
                                rec.ActualDepartureTime = rec.ActualArrivalTime + (rec.ВремяОтправления - rec.ВремяПрибытия);
                            }
                            else
                            {
                                rec.ActualArrivalTime = rec.ВремяПрибытия;
                                rec.ActualDepartureTime = cBОтправлениеЗадерживается.Checked || cBОтправлениеПоГотовности.Checked || cbLandingDelay.Checked ?
                                                              rec.ОжидаемоеВремя != rec.ВремяОтправления ? rec.ОжидаемоеВремя : rec.ОжидаемоеВремя.AddDays(1) :
                                                              rec.ВремяОтправления;
                            }*/

                            if (rec.НомерПути != tr.PathNumber)
                            {
                                rec.НомерПути = tr.PathNumber;
                                rec.НомерПутиБезАвтосброса = rec.НомерПути;
                                changeFlag = true;
                                //Log.log.Trace("нашли изменения для ТРАНЗИТ. Путь: " + rec.НомерПути);//LOG    
                            }

                            if (rec.Активность != tr.IsActive)
                            {
                                rec.Активность = tr.IsActive;
                                changeFlag = true;
                            }

                            rec.AplyIdTrain();

                        }

                        #region Сохранение изменений
                        if (changeFlag)
                        {
                            SoundRecordChangesRx.OnNext(new SoundRecordChanges { NewRec = rec, Rec = recOld, TimeStamp = DateTime.Now, UserInfo = "Удаленный диспетчер" });
                        }
                        #endregion
                    }

                    if (!isExist)
                    {
                        SoundRecord record = default(SoundRecord);

                        var trainNumber = new TrainNumber(tr.NumberOfTrain);
                        record.НомерПоезда = trainNumber.Num1 > 0 ? trainNumber.Num1.ToString() : string.Empty;
                        record.НомерПоезда2 = trainNumber.Num2 > 0 ? trainNumber.Num2.ToString() : string.Empty;

                        record.СтанцияОтправления = tr.StationDeparture.ToString();
                        record.СтанцияНазначения = tr.StationArrival.ToString();

                        record.ВремяПрибытия = tr.TransitTime["приб"];
                        record.ВремяОтправления = tr.TransitTime["отпр"];
                        record.Время = record.ВремяОтправления != DateTime.MinValue ? record.ВремяОтправления : record.ВремяПрибытия;

                        record.НомерПути = tr.PathNumber;
                        record.НомерПутиБезАвтосброса = tr.PathNumberWithoutAutoReset;

                        record.ВремяЗадержки = tr.ВремяЗадержки;
                        record.БитыНештатныхСитуаций = tr.EmergencySituation;
                        record.Активность = tr.IsActive;

                        var newKey = new SchedulingPipelineService().GetUniqueKey(_soundRecords.Keys, record.Время);
                        if (!string.IsNullOrWhiteSpace(newKey))
                        {
                            lock (MainWindowForm.SoundRecords_Lock)
                            {
                                _soundRecords.Add(newKey, record);
                            }
                            MainWindowForm.SoundRecordsOld.Add(newKey, record);
                        }

                        MainWindowForm.ФлагОбновитьСписокЖелезнодорожныхСообщенийВТаблице = true;
                    }
                }
            }
        }

        private bool IsContainsStationsName(string stationDepart1, string stationArrival1, string stationDepart2, string stationArrival2)
        {
            return (stationDepart1.ToLower().Contains(stationDepart2.ToLower()) || stationDepart2.ToLower().Contains(stationDepart1.ToLower())) &&
                   (stationArrival1.ToLower().Contains(stationArrival2.ToLower()) || stationArrival2.ToLower().Contains(stationArrival1.ToLower()));
        }

        #endregion
    }
}