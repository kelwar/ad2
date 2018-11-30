using System;
using System.Collections.Generic;
using System.Linq;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Model;
using Domain.Entitys;
using MainExample.Entites;


namespace MainExample.Mappers
{
    public static class Mapper
    {
        public static List<СтатическоеСообщение> MapSoundConfigurationRecord2СтатическоеСообщение(SoundConfigurationRecord scr, ref int newId)
        {
            СтатическоеСообщение statRecord;
            statRecord.СостояниеВоспроизведения = SoundRecordStatus.ОжиданиеВоспроизведения;
            List<СтатическоеСообщение> resultList = new List<СтатическоеСообщение>();

            if (scr.Enable == true)
            {
                if (scr.EnablePeriodic == true)
                {
                    statRecord.ОписаниеКомпозиции = scr.Name;
                    statRecord.НазваниеКомпозиции = scr.Name;

                    if (statRecord.НазваниеКомпозиции == string.Empty)
                        return null;

                    string[] Times = scr.MessagePeriodic.Split(',');
                    if (Times.Length != 3)
                        return null;

                    DateTime НачалоИнтервала2 = DateTime.Parse(Times[0]), КонецИнтервала2 = DateTime.Parse(Times[1]);
                    int Интервал = int.Parse(Times[2]);

                    while (НачалоИнтервала2 < КонецИнтервала2)
                    {
                        statRecord.ID = newId++;
                        statRecord.Время = НачалоИнтервала2;
                        statRecord.Активность = true;

                        resultList.Add(statRecord);
                        НачалоИнтервала2 = НачалоИнтервала2.AddMinutes(Интервал);
                    }
                }

                if (scr.EnableSingle == true)
                {
                    statRecord.ОписаниеКомпозиции = scr.Name;
                    statRecord.НазваниеКомпозиции = scr.Name;

                    if (statRecord.НазваниеКомпозиции == string.Empty)
                        return null;

                    string[] Times = scr.MessageSingle.Split(',');

                    foreach (string time in Times)
                    {
                        statRecord.ID = newId++;
                        statRecord.Время = DateTime.Parse(time);
                        statRecord.Активность = true;

                        resultList.Add(statRecord);
                    }
                }
            }

            return resultList;
        }
        
        public static SoundRecord MapTrainTableRecord2SoundRecord(TrainTableRecord config, DateTime day, int id)
        {
            var record = new SoundRecord();
            try
            {
                record.ID = id;
                record.IdTrain = new IdTrain(config.ID, config.TrnId);
                //record.IdTrain = new IdTrain(config.ScheduleId);
                record.НомерПоезда = config.Num;
                record.НомерПоезда2 = config.Num2;
                record.НазваниеПоезда = config.Name;
                record.Дополнение = config.Addition;
                record.AdditionEng = config.AdditionEng;
                record.ИспользоватьДополнение = new Dictionary<string, bool>
                {
                    ["звук"] = config.ИспользоватьДополнение["звук"],
                    ["табло"] = config.ИспользоватьДополнение["табло"]
                };
                record.Направление = config.Direction;
                record.СтанцияОтправления = "";
                record.СтанцияНазначения = "";
                record.ДниСледования = config.Days;
                record.ДниСледованияAlias = config.DaysAlias;
                record.DaysFollowingAliasEng = config.DaysAliasEng;
                record.Активность = config.Active;
                record.Автомат = config.Автомат;
                record.ШаблонВоспроизведенияСообщений = config.SoundTemplates;
                record.НомерПути = ПолучитьНомерПутиПоДнямНедели(config);
                record.НомерПутиБезАвтосброса = record.НомерПути;
                record.НумерацияПоезда = config.TrainPathDirection;
                record.СменнаяНумерацияПоезда = config.ChangeTrainPathDirection;
                record.Примечание = config.Примечание;
                record.NoteEng = config.NoteEng;
                record.ТипПоезда = config.ТипПоезда;
                record.Состояние = SoundRecordStatus.ОжиданиеВоспроизведения;
                record.ТипСообщения = SoundRecordType.ДвижениеПоездаНеПодтвержденное;
                record.Описание = config.DaysDescription;
                record.КоличествоПовторений = 1;
                record.СостояниеКарточки = 0;
                record.ОписаниеСостоянияКарточки = "";
                record.БитыНештатныхСитуаций = 0x00;
                record.ТаймерПовторения = 0;
                record.РазрешениеНаОтображениеПути = PathPermissionType.ИзФайлаНастроек;

                record.ИменаФайлов = new string[0];
                record.ФиксированноеВремяПрибытия = null;
                record.ФиксированноеВремяОтправления = null;

                record.СтанцияОтправления = config.StationDepart;
                record.СтанцияНазначения = config.StationArrival;

                record.ВыводНаТабло = config.IsScoreBoardOutput;
                record.ВыводЗвука = config.IsSoundOutput;


                int часы = 0;
                int минуты = 0;
                DateTime времяПрибытия = new DateTime(2000, 1, 1, 0, 0, 0);
                DateTime времяОтправления = new DateTime(2000, 1, 1, 0, 0, 0);
                record.ВремяПрибытия = DateTime.Now;
                record.ВремяОтправления = DateTime.Now;
                record.ОжидаемоеВремя = DateTime.Now;
                record.ActualArrivalTime = record.ВремяПрибытия;
                record.ActualDepartureTime = record.ВремяОтправления;
                record.ВремяСледования = null;
                record.ВремяЗадержки = null;
                //record.DelayTime = null;
                byte номерСписка = 0x00;


                if (config.ArrivalTime != "")
                {
                    string[] subStrings = config.ArrivalTime.Split(':');
                    if (int.TryParse(subStrings[0], out часы) && int.TryParse(subStrings[1], out минуты))
                    {
                        времяПрибытия = new DateTime(day.Year, day.Month, day.Day, часы, минуты, 0);
                        record.ВремяПрибытия = времяПрибытия;
                        record.ОжидаемоеВремя = времяПрибытия;
                        номерСписка |= 0x04;
                    }
                }

                if (config.DepartureTime != "")
                {
                    string[] subStrings = config.DepartureTime.Split(':');
                    if (int.TryParse(subStrings[0], out часы) && int.TryParse(subStrings[1], out минуты))
                    {
                        времяОтправления = new DateTime(day.Year, day.Month, day.Day, часы, минуты, 0);
                        record.ВремяОтправления = времяОтправления;
                        record.ОжидаемоеВремя = времяОтправления;
                        номерСписка |= 0x10;
                    }
                }
                record.ОжидаемоеВремя = !string.IsNullOrWhiteSpace(config.ArrivalTime) ? времяПрибытия : времяОтправления;

                if (!string.IsNullOrEmpty(config.FollowingTime))
                {
                    string[] subStrings = config.FollowingTime.Split(':');
                    if (subStrings.Length == 2 && int.TryParse(subStrings[0], out часы) && int.TryParse(subStrings[1], out минуты))
                    {
                        record.ВремяСледования = new DateTime(day.Year, day.Month, day.Day, часы / 24, часы % 24, минуты);
                    }
                }


                //ТРАНЗИТ
                record.ВремяСтоянки = null;
                //if (номерСписка == 0x14)
                if ((номерСписка & 0x14) == 0x14)
                {
                    if (времяОтправления < времяПрибытия)                              //??????????????
                    {
                        record.ВремяПрибытия = времяПрибытия.AddDays(-1);
                        //record.IdTrain.ДеньПрибытия = record.ВремяПрибытия.Date;
                    }

                    TimeSpan времяСтоянки;
                    if (TimeSpan.TryParse(config.StopTime, out времяСтоянки))
                    {
                        record.ВремяСтоянки = времяСтоянки;
                    }
                    номерСписка |= 0x08;                                              //TODO: ???
                }

                //DEBUG транзиты по ОТПР-------------------
                if ((номерСписка & 0x10) == 0x10 ||
                    (номерСписка & 0x14) == 0x14)
                {
                    record.Время = record.ВремяОтправления;
                    record.ОжидаемоеВремя = record.ВремяОтправления;
                }
                else
                {
                    record.Время = record.ВремяПрибытия;
                    record.ОжидаемоеВремя = record.ВремяПрибытия;
                }

                record.ActualArrivalTime = record.ВремяПрибытия;
                record.ActualDepartureTime = record.ВремяОтправления;

                record.БитыАктивностиПолей = номерСписка;
                //record.БитыАктивностиПолей |= 0x03;                                   //TODO: ???



                // Шаблоны оповещения
                record.СписокФормируемыхСообщений = new List<СостояниеФормируемогоСообщенияИШаблон>();
                string[] шаблонОповещения = record.ШаблонВоспроизведенияСообщений.Split(':');
                if ((шаблонОповещения.Length % 3) == 0)
                {
                    bool активностьШаблоновДанногоПоезда = record.ТипПоезда == ТипПоезда.Пассажирский && Program.Настройки.АвтФормСообщНаПассажирскийПоезд;
                    if (record.ТипПоезда == ТипПоезда.Пригородный && Program.Настройки.АвтФормСообщНаПригородныйЭлектропоезд) активностьШаблоновДанногоПоезда = true;
                    if (record.ТипПоезда == ТипПоезда.Скоростной && Program.Настройки.АвтФормСообщНаСкоростнойПоезд) активностьШаблоновДанногоПоезда = true;
                    if (record.ТипПоезда == ТипПоезда.Скорый && Program.Настройки.АвтФормСообщНаСкорыйПоезд) активностьШаблоновДанногоПоезда = true;
                    if (record.ТипПоезда == ТипПоезда.Ласточка && Program.Настройки.АвтФормСообщНаЛасточку) активностьШаблоновДанногоПоезда = true;
                    if (record.ТипПоезда == ТипПоезда.Фирменный && Program.Настройки.АвтФормСообщНаФирменный) активностьШаблоновДанногоПоезда = true;
                    if (record.ТипПоезда == ТипПоезда.РЭКС && Program.Настройки.АвтФормСообщНаРЭКС) активностьШаблоновДанногоПоезда = true;

                    int indexШаблона = 0;
                    for (int i = 0; i < шаблонОповещения.Length / 3; i++)
                    {
                        bool наличиеШаблона = false;
                        string шаблон = "";
                        PriorityPrecise приоритетШаблона = PriorityPrecise.Zero;
                        foreach (var item in DynamicSoundForm.DynamicSoundRecords)
                            if (item.Name == шаблонОповещения[3 * i + 0])
                            {
                                наличиеШаблона = true;
                                шаблон = item.Message;
                                приоритетШаблона = item.PriorityTemplate;
                                break;
                            }

                        if (наличиеШаблона == true)
                        {
                            var привязкаВремени = 0;
                            int.TryParse(шаблонОповещения[3 * i + 2], out привязкаВремени);

                            string[] времяАктивацииШаблона = шаблонОповещения[3 * i + 1].Replace(" ", "").Split(',');
                            foreach (var время in времяАктивацииШаблона)
                            {
                                int времяСмещения = 0;
                                if ((int.TryParse(время, out времяСмещения)) == true)
                                {
                                    СостояниеФормируемогоСообщенияИШаблон новыйШаблон;
                                    новыйШаблон.Id = indexШаблона++;
                                    новыйШаблон.SoundRecordId = record.ID;
                                    новыйШаблон.Активность = активностьШаблоновДанногоПоезда;
                                    новыйШаблон.ПриоритетГлавный = Priority.Midlle;
                                    новыйШаблон.ПриоритетВторостепенный = приоритетШаблона;
                                    новыйШаблон.Воспроизведен = false;
                                    новыйШаблон.СостояниеВоспроизведения = SoundRecordStatus.ОжиданиеВоспроизведения;
                                    новыйШаблон.ВремяСмещения = времяСмещения;
                                    новыйШаблон.НазваниеШаблона = шаблонОповещения[3 * i + 0];
                                    новыйШаблон.Шаблон = шаблон;
                                    новыйШаблон.ПривязкаКВремени = привязкаВремени;
                                    новыйШаблон.ЯзыкиОповещения = new List<NotificationLanguage> { NotificationLanguage.Ru, NotificationLanguage.Eng };  //TODO:Брать из ШаблонОповещения полученого из TrainTable.

                                    record.СписокФормируемыхСообщений.Add(новыйШаблон);
                                }
                            }
                        }
                    }
                }

                record.СписокНештатныхСообщений = new List<СостояниеФормируемогоСообщенияИШаблон>();
                record.TimetableType = config.TimetableType;
                record.AplyIdTrain();
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }

            return record;
        }
        
        public static UniversalInputType MapTrainTableRecord2UniversalInputType(TrainTableRecord t)
        {
            UniversalInputType uit = null;
            try
            {
                Func<string, string, DateTime> timePars = (arrival, depart) =>
                {
                    DateTime outData;
                    if (DateTime.TryParse(arrival, out outData))
                        return outData;

                    if (DateTime.TryParse(depart, out outData))
                        return outData;

                    return DateTime.MinValue;
                };

                Func<string, string, string> eventPars = (arrivalTime, departTime) =>
                {
                    if ((!string.IsNullOrEmpty(arrivalTime)) && (!string.IsNullOrEmpty(departTime)))
                    {
                        return "СТОЯНКА";
                    }

                    if (!string.IsNullOrEmpty(arrivalTime))
                    {
                        return "ПРИБ.";
                    }

                    if (!string.IsNullOrEmpty(departTime))
                    {
                        return "ОТПР.";
                    }

                    return String.Empty;
                };


                Func<string, string, Dictionary<string, DateTime>> transitTimePars = (arrivalTime, departTime) =>
                {
                    var transitTime = new Dictionary<string, DateTime>();
                    if (!string.IsNullOrEmpty(arrivalTime))
                    {
                        transitTime["приб"] = timePars(arrivalTime, String.Empty);
                    }

                    if (!string.IsNullOrEmpty(departTime))
                    {
                        transitTime["отпр"] = timePars(departTime, String.Empty);
                    }

                    return transitTime;
                };


                Func<string, string, Station> stationsPars2 = (station, direction) =>
                {
                    var emptyStation = new Station { NameRu = string.Empty, NameEng = string.Empty, NameCh = string.Empty };
                    if (string.IsNullOrEmpty(direction) || string.IsNullOrEmpty(station))
                    {
                        return emptyStation;
                    }

                    var stationDir = Program.DirectionRepository.GetByName(direction)?.GetStationInDirectionByName(station);
                    if (stationDir == null)
                        return emptyStation;

                    return stationDir;
                };


                TimeSpan stopTime;
                uit = new UniversalInputType
                {
                    IsActive = t.Active,
                    Id = t.ID,
                    ScheduleId = t.ScheduleId,
                    TrnId = t.TrnId,
                    Event = eventPars(t.ArrivalTime, t.DepartureTime),
                    TypeTrain = (t.ТипПоезда == ТипПоезда.Пассажирский) ? TypeTrain.Passenger :
                                                (t.ТипПоезда == ТипПоезда.Пригородный) ? TypeTrain.Suburban :
                                                (t.ТипПоезда == ТипПоезда.Фирменный) ? TypeTrain.Corporate :
                                                (t.ТипПоезда == ТипПоезда.Скорый) ? TypeTrain.Express :
                                                (t.ТипПоезда == ТипПоезда.Скоростной) ? TypeTrain.HighSpeed :
                                                (t.ТипПоезда == ТипПоезда.Ласточка) ? TypeTrain.Swallow :
                                                (t.ТипПоезда == ТипПоезда.РЭКС) ? TypeTrain.Rex : TypeTrain.None,
                    Note = t.Примечание, //C остановками: ...
                    NoteEng = t.NoteEng,
                    PathNumber = ПолучитьНомерПутиПоДнямНедели(t),
                    VagonDirection = (VagonDirection)t.TrainPathDirection,
                    NumberOfTrain = string.IsNullOrWhiteSpace(t.Num2) ? t.Num : t.Num + "/" + t.Num2,
                    Stations = t.Name,
                    DirectionStation = t.Direction,
                    StationDeparture = stationsPars2(t.StationDepart, t.Direction),
                    StationArrival = stationsPars2(t.StationArrival, t.Direction),
                    Time = timePars(t.ArrivalTime, t.DepartureTime),
                    TransitTime = transitTimePars(t.ArrivalTime, t.DepartureTime),
                    ВремяЗадержки = null,
                    StopTime = (TimeSpan?)(TimeSpan.TryParse(t.StopTime, out stopTime) ? (ValueType)stopTime : null),
                    ExpectedTime = timePars(t.ArrivalTime, t.DepartureTime),
                    DaysFollowing = t.DaysDescription,
                    DaysFollowingAlias = t.DaysAlias,
                    DaysFollowingAliasEng = t.DaysAliasEng,
                    Addition = t.Addition,
                    AdditionEng = t.AdditionEng,
                    SendingDataLimit = t.IsScoreBoardOutput,
                    Command = Command.None,
                    EmergencySituation = 0x00,
                    TimetableType = t.TimetableType
                };
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }

            return uit;
        }
        
        public static string ПолучитьНомерПутиПоДнямНедели(TrainTableRecord record)
        {
            if (!record.PathWeekDayes)
            {
                return record.TrainPathNumber[WeekDays.Постоянно] != "Не определен" ? record.TrainPathNumber[WeekDays.Постоянно] : string.Empty;
            }

            DayOfWeek dayOfWeek= DateTime.Now.DayOfWeek;
            switch (MainWindowForm.РаботаПоНомеруДняНедели)  //TODO: РаботаПоНомеруДняНедели внедрять через DI
            {
                case 0:
                    dayOfWeek = DayOfWeek.Monday;
                    break;

                case 1:
                    dayOfWeek = DayOfWeek.Tuesday;
                    break;

                case 2:
                    dayOfWeek = DayOfWeek.Wednesday;
                    break;

                case 3:
                    dayOfWeek = DayOfWeek.Thursday;
                    break;

                case 4:
                    dayOfWeek = DayOfWeek.Friday;
                    break;

                case 5:
                    dayOfWeek = DayOfWeek.Saturday;
                    break;

                case 6:
                    dayOfWeek = DayOfWeek.Sunday;
                    break;
            }

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return record.TrainPathNumber[WeekDays.Пн];

                case DayOfWeek.Tuesday:
                    return record.TrainPathNumber[WeekDays.Вт];

                case DayOfWeek.Wednesday:
                    return record.TrainPathNumber[WeekDays.Ср];

                case DayOfWeek.Thursday:
                    return record.TrainPathNumber[WeekDays.Чт];

                case DayOfWeek.Friday:
                    return record.TrainPathNumber[WeekDays.Пт];

                case DayOfWeek.Saturday:
                    return record.TrainPathNumber[WeekDays.Сб];

                case DayOfWeek.Sunday:
                    return record.TrainPathNumber[WeekDays.Вс];
            }

            return string.Empty;
        }
        
        public static UniversalInputType MapSoundRecord2UniveralInputType(SoundRecord data, bool pathPermission, bool isShow)
        {
            UniversalInputType mapData = null;
            try
            {
                mapData = new UniversalInputType
                {
                    Id = data.ID,
                    ScheduleId = data.IdTrain.ScheduleId,
                    TrnId = data.IdTrain.TrnId,
                    IsActive = data.Активность,
                    VagonDirection = (VagonDirection)data.НумерацияПоезда,
                    ChangeVagonDirection = data.СменнаяНумерацияПоезда,
                    PathNumber = TrackNumber2DisplayString(data.РазрешениеНаОтображениеПути, pathPermission, data.НомерПути),
                    PathNumberWithoutAutoReset = data.НомерПутиБезАвтосброса,
                    Track = Program.TrackRepository.List().FirstOrDefault(t => t.Name == data.НомерПути),
                    Time = FilterTime(data.БитыАктивностиПолей, data.ВремяПрибытия, data.ВремяОтправления),
                    TransitTime = Times2TransitTimes(data.БитыАктивностиПолей, data.ВремяПрибытия, data.ВремяОтправления),
                    ВремяЗадержки = data.ВремяЗадержки,
                    ExpectedTime = data.ОжидаемоеВремя,
                    StopTime = data.ВремяСтоянки,
                    DirectionStation = data.Направление,
                    TypeTrain = (TypeTrain)data.ТипПоезда,
                    DaysFollowing = data.Описание,
                    DaysFollowingAlias = data.ДниСледованияAlias,
                    DaysFollowingAliasEng = data.DaysFollowingAliasEng,
                    Addition = data.ИспользоватьДополнение != null && data.ИспользоватьДополнение["табло"] ? data.Дополнение : string.Empty,
                    AdditionEng = data.ИспользоватьДополнение != null && data.ИспользоватьДополнение["табло"] ? data.AdditionEng : string.Empty,
                    SendingDataLimit = data.ВыводНаТабло,
                    Command = TableRecordStatus2Command(data.СостояниеОтображения),
                    EmergencySituation = data.БитыНештатныхСитуаций,
                    ViewBag = new Dictionary<string, dynamic>
                    {
                        ["Composition"] = data.Composition
                    },
                    TimetableType = data.TimetableType
                };

                if (!isShow || data.СостояниеОтображения != TableRecordStatus.Очистка)
                {
                    mapData.NumberOfTrain = TrainNumber2String(data.БитыАктивностиПолей, data.НомерПоезда, data.НомерПоезда2);
                    mapData.Event = ActivityBeats2Event(data.БитыАктивностиПолей);
                    mapData.Stations = data.НазваниеПоезда;
                    mapData.StationDeparture = Program.DirectionRepository.GetByName(data.Направление)?.GetStationInDirectionByName(data.СтанцияОтправления) ?? ExchangeModel.NameRailwayStation;
                    mapData.StationArrival = Program.DirectionRepository.GetByName(data.Направление)?.GetStationInDirectionByName(data.СтанцияНазначения) ?? ExchangeModel.NameRailwayStation;
                    mapData.Note = data.Примечание;
                    mapData.NoteEng = data.NoteEng;
                }
                else
                {
                    mapData.NumberOfTrain = "   ";
                    mapData.Event = "   ";
                    mapData.Stations = "   ";
                    mapData.StationDeparture = new Station();
                    mapData.StationArrival = new Station();
                    mapData.Note = "   ";
                    mapData.NoteEng = "   ";
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }

            return mapData;
        }
        
        public static SoundRecordDb MapSoundRecord2SoundRecordDb(SoundRecord data)
        {
            return new SoundRecordDb
            {
                Id = data.ID,
                Автомат = data.Автомат,
                Активность = data.Активность,
                БитыАктивностиПолей = data.БитыАктивностиПолей,
                БитыНештатныхСитуаций = data.БитыНештатныхСитуаций,
                Время = data.Время,
                ВремяЗадержки = data.ВремяЗадержки,
                ВремяОтправления = data.ВремяОтправления,
                ВремяПрибытия = data.ВремяПрибытия,
                ВремяСледования = data.ВремяСледования,
                ВремяСтоянки = data.ВремяСтоянки,
                ДниСледования = data.ДниСледования,
                ДниСледованияAlias = data.ДниСледованияAlias ?? string.Empty,
                DaysFollowingAliasEng = data.DaysFollowingAliasEng ?? string.Empty,
                Дополнение = data.Дополнение,
                AdditionEng = data.AdditionEng,
                ИменаФайлов = data.ИменаФайлов,
                ИспользоватьДополнение = data.ИспользоватьДополнение,
                КоличествоПовторений = data.КоличествоПовторений,
                НазваниеПоезда = data.НазваниеПоезда ?? string.Empty,
                НазванияТабло = data.НазванияТабло,
                Направление = data.Направление,
                НомерПоезда = data.НомерПоезда ?? string.Empty,
                НомерПоезда2 = data.НомерПоезда2 ?? string.Empty,
                НомерПути = data.НомерПути ?? string.Empty,
                НомерПутиБезАвтосброса = data.НомерПутиБезАвтосброса ?? string.Empty,
                НумерацияПоезда = data.НумерацияПоезда,
                ОжидаемоеВремя = data.ОжидаемоеВремя,
                Описание = data.Описание ?? string.Empty,
                ОписаниеСостоянияКарточки = data.ОписаниеСостоянияКарточки,
                Примечание = data.Примечание ?? string.Empty,
                NoteEng = data.NoteEng ?? string.Empty,
                РазрешениеНаОтображениеПути = data.РазрешениеНаОтображениеПути,
                Состояние = data.Состояние,
                СостояниеКарточки = data.СостояниеКарточки,
                СостояниеОтображения = data.СостояниеОтображения,
                СтанцияНазначения = data.СтанцияНазначения ?? string.Empty,
                СтанцияОтправления = data.СтанцияОтправления ?? string.Empty,
                ТаймерПовторения = data.ТаймерПовторения,
                ТипПоезда = data.ТипПоезда,
                ТипСообщения = data.ТипСообщения,
                ФиксированноеВремяОтправления = data.ФиксированноеВремяОтправления,
                ФиксированноеВремяПрибытия = data.ФиксированноеВремяПрибытия,
                ШаблонВоспроизведенияСообщений = data.ШаблонВоспроизведенияСообщений,
                СписокФормируемыхСообщений = data.СписокФормируемыхСообщений.Select(СостояниеФормируемогоСообщенияИШаблон2СостояниеФормируемогоСообщенияИШаблонDb).ToList(),
                Composition = data.Composition,
                IsDisplayOnBoard = data.ВыводНаТабло,
                IsPlaySound = data.ВыводЗвука,
                TimetableType = data.TimetableType
                //СписокНештатныхСообщений = data.СписокНештатныхСообщений.Select(СостояниеФормируемогоСообщенияИШаблон2СостояниеФормируемогоСообщенияИШаблонDb).ToList()
            };
        }
        
        public static SoundRecord MapSoundRecordDb2SoundRecord(SoundRecordDb data)
        {
            return new SoundRecord
            {
                ID = data.Id,
                Автомат = data.Автомат,
                Активность = data.Активность,
                БитыАктивностиПолей = data.БитыАктивностиПолей,
                БитыНештатныхСитуаций = data.БитыНештатныхСитуаций,
                Время = data.Время,
                ВремяЗадержки = data.ВремяЗадержки,
                ВремяОтправления = data.ВремяОтправления,
                ВремяПрибытия = data.ВремяПрибытия,
                ВремяСледования = data.ВремяСледования,
                ВремяСтоянки = data.ВремяСтоянки,
                ДниСледования = data.ДниСледования,
                ДниСледованияAlias = data.ДниСледованияAlias ?? string.Empty,
                DaysFollowingAliasEng = data.DaysFollowingAliasEng ?? string.Empty,
                Дополнение = data.Дополнение,
                AdditionEng = data.AdditionEng,
                ИменаФайлов = data.ИменаФайлов,
                ИспользоватьДополнение = data.ИспользоватьДополнение,
                КоличествоПовторений = data.КоличествоПовторений,
                НазваниеПоезда = data.НазваниеПоезда ?? string.Empty,
                НазванияТабло = data.НазванияТабло,
                Направление = data.Направление,
                НомерПоезда = data.НомерПоезда ?? string.Empty,
                НомерПоезда2 = data.НомерПоезда2 ?? string.Empty,
                НомерПути = data.НомерПути ?? string.Empty,
                НомерПутиБезАвтосброса = data.НомерПутиБезАвтосброса ?? string.Empty,
                НумерацияПоезда = data.НумерацияПоезда,
                ОжидаемоеВремя = data.ОжидаемоеВремя,
                Описание = data.Описание ?? string.Empty,
                ОписаниеСостоянияКарточки = data.ОписаниеСостоянияКарточки,
                Примечание = data.Примечание ?? string.Empty,
                NoteEng = data.NoteEng ?? string.Empty,
                РазрешениеНаОтображениеПути = data.РазрешениеНаОтображениеПути,
                Состояние = data.Состояние,
                СостояниеКарточки = data.СостояниеКарточки,
                СостояниеОтображения = data.СостояниеОтображения,
                СтанцияНазначения = data.СтанцияНазначения ?? string.Empty,
                СтанцияОтправления = data.СтанцияОтправления ?? string.Empty,
                ТаймерПовторения = data.ТаймерПовторения,
                ТипПоезда = data.ТипПоезда,
                ТипСообщения = data.ТипСообщения,
                ФиксированноеВремяОтправления = data.ФиксированноеВремяОтправления,
                ФиксированноеВремяПрибытия = data.ФиксированноеВремяПрибытия,
                ШаблонВоспроизведенияСообщений = data.ШаблонВоспроизведенияСообщений,
                СписокФормируемыхСообщений = data.СписокФормируемыхСообщений.Select(СостояниеФормируемогоСообщенияИШаблонDb2СостояниеФормируемогоСообщенияИШаблон).ToList(),
                ActualArrivalTime = (data.БитыНештатныхСитуаций & 0x02) != 0x00 ?
                                    data.ОжидаемоеВремя != data.ВремяПрибытия ? data.ОжидаемоеВремя : data.ОжидаемоеВремя.AddDays(1) : data.ВремяПрибытия,
                ActualDepartureTime = (data.БитыНештатныхСитуаций & 0x04) != 0x00 || (data.БитыНештатныхСитуаций & 0x08) != 0x00 || (data.БитыНештатныхСитуаций & 0x10) != 0x00 ? 
                                      data.ОжидаемоеВремя != data.ВремяОтправления ? data.ОжидаемоеВремя : data.ОжидаемоеВремя.AddDays(1) : data.ВремяОтправления,
                Composition = data.Composition,
                ВыводНаТабло = data.IsDisplayOnBoard,
                ВыводЗвука = data.IsPlaySound,
                TimetableType = data.TimetableType
                //СписокНештатныхСообщений = data.СписокНештатныхСообщений.Select(СостояниеФормируемогоСообщенияИШаблонDb2СостояниеФормируемогоСообщенияИШаблон).ToList()
            };
        }
        
        public static СостояниеФормируемогоСообщенияИШаблонDb СостояниеФормируемогоСообщенияИШаблон2СостояниеФормируемогоСообщенияИШаблонDb(СостояниеФормируемогоСообщенияИШаблон data)
        {
            return new СостояниеФормируемогоСообщенияИШаблонDb
            {
                Id = data.Id,
                SoundRecordId = data.SoundRecordId,
                Активность = data.Активность,
                НазваниеШаблона = data.НазваниеШаблона,
                ВремяСмещения = data.ВремяСмещения,
                ЯзыкиОповещения = data.ЯзыкиОповещения,
                Воспроизведен = data.Воспроизведен,
                ПривязкаКВремени = data.ПривязкаКВремени,
                Приоритет = data.ПриоритетГлавный,
                СостояниеВоспроизведения = data.СостояниеВоспроизведения,
                Шаблон = data.Шаблон
            };
        }
        
        public static СостояниеФормируемогоСообщенияИШаблон СостояниеФормируемогоСообщенияИШаблонDb2СостояниеФормируемогоСообщенияИШаблон(СостояниеФормируемогоСообщенияИШаблонDb data)
        {
            return new СостояниеФормируемогоСообщенияИШаблон
            {
                Id = data.Id,
                SoundRecordId = data.SoundRecordId,
                Активность = data.Активность,
                НазваниеШаблона = data.НазваниеШаблона,
                ВремяСмещения = data.ВремяСмещения,
                ЯзыкиОповещения = data.ЯзыкиОповещения,
                Воспроизведен = data.Воспроизведен,
                ПривязкаКВремени = data.ПривязкаКВремени,
                ПриоритетГлавный = data.Приоритет,
                СостояниеВоспроизведения = data.СостояниеВоспроизведения,
                Шаблон = data.Шаблон
            };
        }
        
        public static SoundRecordChanges SoundRecordChangesDb2SoundRecordChanges(SoundRecordChangesDb data)
        {      
             return new SoundRecordChanges
             {
                 ScheduleId = data.ScheduleId,
                 Rec = MapSoundRecordDb2SoundRecord(data.Rec),
                 NewRec = MapSoundRecordDb2SoundRecord(data.NewRec),
                 TimeStamp = data.TimeStamp,
                 UserInfo= data.UserInfo,
                 CauseOfChange = data.CauseOfChange
             };
        }
        
        public static SoundRecordChangesDb SoundRecordChanges2SoundRecordChangesDb(SoundRecordChanges data)
        {
            return new SoundRecordChangesDb
            {
                ScheduleId = data.ScheduleId,
                Rec = MapSoundRecord2SoundRecordDb(data.Rec),
                NewRec = MapSoundRecord2SoundRecordDb(data.NewRec),
                TimeStamp = data.TimeStamp,
                UserInfo= data.UserInfo,
                CauseOfChange = data.CauseOfChange
            };
        }

        private static Command TableRecordStatus2Command(TableRecordStatus status)
        {
            switch (status)
            {
                case TableRecordStatus.Отображение:
                    return Command.View;

                case TableRecordStatus.Очистка:
                    return Command.Delete;

                case TableRecordStatus.Обновление:
                    return Command.Update;

                default:
                    return Command.None;
            }
        }

        private static string TrackNumber2DisplayString(PathPermissionType permissionType, bool permission, string trackNumber)
        {
            return permissionType == PathPermissionType.Отображать || (permissionType == PathPermissionType.ИзФайлаНастроек && permission) ? trackNumber : "   ";
        }

        private static string TrainNumber2String(byte activityBeats, string num1, string num2 = null)
        {
            return (activityBeats & 0x14) == 0x14 && !string.IsNullOrWhiteSpace(num2) ? $"{num1}/{num2}" : num1;
        }

        private static string ActivityBeats2Event(byte activityBeats)
        {
            if ((activityBeats & 0x14) == 0x14)
                return "СТОЯНКА";
            else if ((activityBeats & 0x04) == 0x04)
                return "ПРИБ.";
            else if ((activityBeats & 0x10) == 0x10)
                return "ОТПР.";
            else
                return "   ";
        }

        private static DateTime FilterTime(byte activityBeats, DateTime arrivalTime, DateTime departureTime)
        {
            return (activityBeats & 0x04) == 0x04 ? arrivalTime : departureTime;
        }

        private static Dictionary<string, DateTime> Times2TransitTimes(byte activityBeats, DateTime arrivalTime, DateTime departureTime)
        {
            var result = new Dictionary<string, DateTime>();

            if ((activityBeats & 0x04) == 0x04 || (activityBeats & 0x14) == 0x14)
                result["приб"] = arrivalTime;

            if ((activityBeats & 0x10) == 0x10 || (activityBeats & 0x14) == 0x14)
                result["отпр"] = departureTime;

            return result;
        }
    }
}