﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Domain.Entitys;
using MainExample.Services;
using Domain.Abstract;
using Domain.Concrete;
using Library;
using System.Globalization;

namespace MainExample.Entites
{
    public enum SourceData { Local, RemoteCis }
    public enum WeekDays { Постоянно, Пн, Вт, Ср, Чт, Пт, Сб, Вс }

    public struct TrainTableRecord
    {
        public int ID;                    //Id
        public string Num;                //Номер поезда
        public string Num2;               //Номер поезда 2 для транзита
        public string Name;               //Название поезда
        public string Direction;          //направление
        public string StationDepart;      //станция отправления
        public string StationArrival;     //станция прибытия
        public string ArrivalTime;        //время прибытие
        public string StopTime;           //время стоянка
        public string DepartureTime;      //время отправление
        public string FollowingTime;      //время следования (время в пути)
        public string Days;               //дни следования
        public string DaysAlias;          //дни следования (строка заполняется в ручную)
        public string DaysAliasEng;
        public string DaysDescription;
        public bool Active;               //активность, отмека галочкой
        public string SoundTemplates;     //
        public byte TrainPathDirection;   //Нумерация вагонов
        public bool ChangeTrainPathDirection;      //смена направления (для транзитов)
        public Dictionary<WeekDays, string> TrainPathNumber;      //Пути по дням недели или постоянно
        public bool PathWeekDayes;                               //true- установленны пути по дням недели, false - путь установленн постоянно
        public ТипПоезда ТипПоезда;
        public string Примечание;
        public string NoteEng;
        public DateTime ВремяНачалаДействияРасписания;
        public DateTime ВремяОкончанияДействияРасписания;
        public string Addition;                                   //Дополнение
        public string AdditionEng;
        public Dictionary<string, bool> ИспользоватьДополнение;   //[звук] - использовать дополнение для звука.  [табло] - использовать дополнение для табло.
        public bool Автомат;                                      // true - поезд обрабатывается в автомате.
        public bool IsScoreBoardOutput;                           // Вывод на табло. true. (Работает если указанн Contrains SendingDataLimit)
        public bool IsSoundOutput;                                // Вывод звука. true.
        public int ScheduleId;
        public int TrnId;
        public int TieTrainId;                                      // ИД_ГДП привязанного поезда
        public bool DenyAutoUpdate;

        public override string ToString()
        {
            var num = !string.IsNullOrWhiteSpace(Num2) && Num != Num2 ? $"{Num}/{Num2}" : Num;
            var time = !string.IsNullOrWhiteSpace(ArrivalTime) ?
                       !string.IsNullOrWhiteSpace(DepartureTime) ? $"ПРИБ. {ArrivalTime} ОТПР. {DepartureTime}" : $"ПРИБ. {ArrivalTime}" :
                       !string.IsNullOrWhiteSpace(DepartureTime) ? $"ОТПР. {DepartureTime}" : $"";
            return !string.IsNullOrWhiteSpace(num) ? $"{num} {StationDepart} - {StationArrival} {time}" : "Нет привязки";
        }
    };


    public class TrainSheduleTable
    {
        #region Field

        private static object _lockObj = new object();
        private const string FileNameLocalTableRec = @"TableRecords.ini";
        private const string FileNameRemoteCisTableRec = @"TableRecordsRemoteCis.ini";
        private const string FileNameRemoteCisTableRecOper = @"TableRecordsRemoteCisOper.ini";

        public static SourceData SourceLoad;
        public static List<TrainTableRecord> TrainTableRecords = new List<TrainTableRecord>(); // Содержит актуальное рабочее расписание

        #endregion




        #region Rx

        public static Subject<SourceData> RemoteCisTableChangeRx { get; } = new Subject<SourceData>();

        #endregion




        #region static ctor

        static TrainSheduleTable()
        {
            Enum.TryParse(Program.Настройки.SourceTrainTableRecordLoad, out SourceLoad);
        }

        #endregion





        #region Methode 

        /// <summary>
        /// Выбор источника загрузки и загрузка
        /// </summary>
        public static async Task SourceLoadMainListAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                var trainTableRec = ЗагрузитьСписок(SourceLoad == SourceData.Local ? FileNameLocalTableRec : FileNameRemoteCisTableRec);
                if (trainTableRec != null)
                {
                    TrainTableRecords.Clear();
                    TrainTableRecords.AddRange(trainTableRec);
                }
            });
        }

        /// <summary>
        /// Выбор источника загрузки и загрузка
        /// </summary>
        public static async Task SourceLoadOperListAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                var trainTableRec = ЗагрузитьСписок(SourceLoad == SourceData.Local ? TrainTableOperative.TableFileName : TrainTableOperative.TableFileNameCis);
                if (trainTableRec != null)
                {
                    TrainTableOperative.TrainTableRecords.Clear();
                    TrainTableOperative.TrainTableRecords.AddRange(trainTableRec);
                }
            });
        }

        /// <summary>
        /// Выбор источника загрузки и загрузка
        /// </summary>
        public static void SourceLoadMainList()
        {
            var trainTableRec = ЗагрузитьСписок(SourceLoad == SourceData.Local ? FileNameLocalTableRec : FileNameRemoteCisTableRec);
            if (trainTableRec != null)
            {
                TrainTableRecords.Clear();
                TrainTableRecords.AddRange(trainTableRec);
            }
        }


        /// <summary>
        /// Выбор источника сохранения и сохранение
        /// </summary>
        public static async Task SourceSaveMainListAsync()
        {
           await Task.Factory.StartNew(()=>
            {
                СохранитьСписок(TrainTableRecords, SourceLoad == SourceData.Local ? FileNameLocalTableRec : FileNameRemoteCisTableRec);
            });
        }

        public static async Task SourceSaveOperListAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                СохранитьСписок(TrainTableOperative.TrainTableRecords, SourceLoad == SourceData.Local ? FileNameLocalTableRec : FileNameRemoteCisTableRec);
            });
        }

        /// <summary>
        /// Сохранить список от ЦИС
        /// Начнет асинхронно сохранять и обновлять список UI.
        /// </summary>
        public static async Task СохранитьИПрименитьСписокРегулярноеРасписаниеЦис(IList<TrainTableRecord> trainTableRecords)
        {
            await Task.Factory.StartNew(() =>
            {
                СохранитьСписок(trainTableRecords, FileNameRemoteCisTableRec);
            });

            switch (SourceLoad)
            {
                case SourceData.RemoteCis:
                    TrainTableRecords = trainTableRecords as List<TrainTableRecord>;
                    break;
            }
            RemoteCisTableChangeRx.OnNext(SourceLoad);
        }

        public static async Task SaveOperTimetable(IList<TrainTableRecord> trainTableRecords)
        {
            await Task.Factory.StartNew(() =>
            {
                СохранитьСписок(trainTableRecords, FileNameRemoteCisTableRecOper);
            });

            switch (SourceLoad)
            {
                case SourceData.RemoteCis:
                    TrainTableOperative.TrainTableRecords = trainTableRecords as List<TrainTableRecord>;
                    break;
            }
            RemoteCisTableChangeRx.OnNext(SourceLoad);
        }

        /// <summary>
        /// загрузить локальное распсиание
        /// </summary>
        public static async Task<List<TrainTableRecord>> ЗагрузитьРасписаниеЛокальноеAsync()
        {
            return await Task<List<TrainTableRecord>>.Factory.StartNew(()=> ЗагрузитьСписок(FileNameLocalTableRec));
        }



        /// <summary>
        /// загрузить локальное распсиание
        /// </summary>
        public static async Task<List<TrainTableRecord>> ЗагрузитьРасписаниеЦисAsync()
        {
            return await Task<List<TrainTableRecord>>.Factory.StartNew(()=> ЗагрузитьСписок(FileNameRemoteCisTableRec));
        }


        /// <summary>
        /// загрузить оперативное распсиание ЦИС
        /// </summary>
        public static async Task<List<TrainTableRecord>> GetCisOperTimetableAsync()
        {
            return await Task<List<TrainTableRecord>>.Factory.StartNew(() => ЗагрузитьСписок(FileNameRemoteCisTableRecOper));
        }

        public static async void ChangeTimeZone(int offset)
        {
            ChangeTimeZoneInTableRecords(TrainTableRecords, "HH:mm", offset);
            await SourceSaveMainListAsync();

            ChangeTimeZoneInTableRecords(TrainTableOperative.TrainTableRecords, "HH:mm", offset);
            await SourceSaveOperListAsync();
        }

        /// <summary>
        /// Сохранить список в файл
        /// </summary>
        private static void СохранитьСписок(IList<TrainTableRecord> trainTableRecords, string fileName)
        {
            /*var firstDate = DateTime.MaxValue.Date;
            var lastDate = DateTime.MinValue.Date;
            try
            {
                foreach (var tr in trainTableRecords)
                {
                    DateTime tempDate;
                    if ((tempDate = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tr.Days, tr.ВремяНачалаДействияРасписания, tr.ВремяОкончанияДействияРасписания)?.FirstDayOfGoing() ?? DateTime.MinValue) < firstDate)
                    {
                        firstDate = tempDate;
                    }

                    if (firstDate == DateTime.Now.Date)
                        break;
                }

                foreach (var tr in trainTableRecords)
                {
                    DateTime tempDate;
                    if ((tempDate = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tr.Days, tr.ВремяНачалаДействияРасписания, tr.ВремяОкончанияДействияРасписания)?.LastDayOfGoing() ?? DateTime.MinValue) > lastDate)
                    {
                        lastDate = tempDate;
                    }

                    if (lastDate == tr.ВремяОкончанияДействияРасписания.Date)
                        break;
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error($"Начало: {firstDate.ToString("yyyy-MM-dd")}, конец: {lastDate.ToString("yyyy-MM-dd")}. \n{ex}");
            }*/

            try
            {
                lock (_lockObj)
                {
                    using (StreamWriter dumpFile = new StreamWriter(fileName))
                    {
                        for (int i = 0; i < trainTableRecords.Count; i++)
                        {
                            var tableRec = trainTableRecords[i];
                            //tableRec.ID = i + 1;

                            var daysAlias = //string.IsNullOrWhiteSpace(trainTableRecords[i].DaysAlias) ?
                                            //ПланРасписанияПоезда.ПолучитьИзСтрокиПланРасписанияПоезда(tableRec.Days, tableRec.ВремяНачалаДействияРасписания, tableRec.ВремяОкончанияДействияРасписания)?.GetAlias(firstDate, lastDate) ?? string.Empty;// : 
                                            trainTableRecords[i].DaysAlias;
                            if (daysAlias.Contains("\r\n"))
                            {
                                daysAlias = daysAlias.Replace("\r\n", "{rn}");
                            }

                            var noteEng = trainTableRecords[i].NoteEng;

                            /*var isScoreBoardOutput = !trainTableRecords.ToList().Exists(tr => tableRec.Num == tr.Num &&
                                                                                              tableRec.StationDepart == tr.StationDepart &&
                                                                                              tableRec.StationArrival == tr.StationArrival) ||
                                                     trainTableRecords.ToList().Exists(tr => tableRec.Num == tr.Num &&
                                                                                             tableRec.StationDepart == tr.StationDepart &&
                                                                                             tableRec.StationArrival == tr.StationArrival &&
                                                                                             (TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tableRec.Days, tableRec.ВремяНачалаДействияРасписания, tableRec.ВремяОкончанияДействияРасписания).DayDictionary.Where(d => d.Value).Count() >
                                                                                             TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(tr.Days, tr.ВремяНачалаДействияРасписания, tr.ВремяОкончанияДействияРасписания).DayDictionary.Where(d => d.Value).Count())) ?
                                                     true : false;*/

                            string line = tableRec.ID + ";" +
                                  trainTableRecords[i].Num + ";" +
                                  trainTableRecords[i].Name + ";" +
                                  trainTableRecords[i].ArrivalTime + ";" +
                                  trainTableRecords[i].StopTime + ";" +
                                  trainTableRecords[i].DepartureTime + ";" +
                                  (!string.IsNullOrWhiteSpace(trainTableRecords[i].Days) ? trainTableRecords[i].Days : $"Отс:0:0:0:0:0:0:0:0:0:0:0:0:0:0:0:0") + ";" +
                                  (trainTableRecords[i].Active ? "1" : "0") + ";" +
                                  trainTableRecords[i].SoundTemplates + ";" +
                                  trainTableRecords[i].TrainPathDirection.ToString() + ";" +
                                  SavePath2File(trainTableRecords[i].TrainPathNumber,
                                      trainTableRecords[i].PathWeekDayes) + ";" +
                                  trainTableRecords[i].ТипПоезда.ToString() + ";" +
                                  trainTableRecords[i].Примечание + ";" +
                                  trainTableRecords[i]
                                      .ВремяНачалаДействияРасписания.ToString("dd.MM.yyyy HH:mm:ss") + ";" +
                                  trainTableRecords[i]
                                      .ВремяОкончанияДействияРасписания.ToString("dd.MM.yyyy HH:mm:ss") +
                                  ";" +
                                  trainTableRecords[i].Addition + ";" +
                                  (trainTableRecords[i].ИспользоватьДополнение["табло"] ? "1" : "0") + ";" +
                                  (trainTableRecords[i].ИспользоватьДополнение["звук"] ? "1" : "0") + ";" +
                                  (trainTableRecords[i].Автомат ? "1" : "0") + ";" +

                                  trainTableRecords[i].Num2 + ";" +
                                  trainTableRecords[i].FollowingTime + ";" +
                                  daysAlias + ";" +

                                  trainTableRecords[i].StationDepart + ";" +
                                  trainTableRecords[i].StationArrival + ";" +
                                  trainTableRecords[i].Direction + ";" +
                                  trainTableRecords[i].ChangeTrainPathDirection + ";" +
                                  trainTableRecords[i].IsScoreBoardOutput + ";" +
                                  trainTableRecords[i].IsSoundOutput + ";" +
                                  trainTableRecords[i].DaysAliasEng + ";" +
                                  trainTableRecords[i].AdditionEng + ";" +
                                  (!string.IsNullOrWhiteSpace(noteEng) ? noteEng : TranslateNote(trainTableRecords[i], trainTableRecords[i].Примечание)) + ";" +
                                  trainTableRecords[i].ScheduleId + ";" +
                                  trainTableRecords[i].TrnId + ";" +
                                  trainTableRecords[i].DenyAutoUpdate;
                            
                            dumpFile.WriteLine(line);
                        }

                        dumpFile.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string TranslateNote(TrainTableRecord train, string note)
        {
            var result = note.Replace("Со всеми остановками", "With all stops")
                             .Replace("Без остановок", "Without stops")
                             .Replace("С остановками: ", "With stops: ")
                             .Replace("Кроме: ", "Except: ");

            var str = note.Split(':');
            if (str != null && str.Length > 1)
            {
                var s = str[1].Split(',');
                if (s != null)
                {
                    var count = s.Length;
                    for (int i = 0; i < count; i++)
                    {
                        var nameRu = s[i].Trim();
                        result = result.Replace($"{nameRu}", $"{Program.DirectionRepository.GetByName(train.Direction)?.GetStationInDirectionByName(nameRu)?.NameEng ?? string.Empty}");
                    }
                }
            }
            return result;
        }
        

        /// <summary>
        /// загрузить список из файл
        /// </summary>
        private static List<TrainTableRecord> ЗагрузитьСписок(string fileName)
        {
            lock (_lockObj)
            {
                var trainTableRecords = new List<TrainTableRecord>();
                try
                {
                    using (StreamReader file = new StreamReader(fileName))
                    {
                        string line;
                        while ((line = file.ReadLine()) != null)
                        {
                            string[] settings = line.Split(';');
                            if ((settings.Length == 13) || (settings.Length == 15) || (settings.Length >= 16))
                            {
                                TrainTableRecord данные;

                                var parser = Parser.GetParser();
                                данные.ID = int.Parse(settings[0]);
                                данные.Num = settings[1];

                                if (string.IsNullOrWhiteSpace(данные.Num))
                                    continue;

                                данные.Name = settings[2];
                                данные.ArrivalTime = settings[3];
                                //данные.StopTime = settings[4];
                                данные.DepartureTime = settings[5];
                                var arrivalTime = parser.ToDateTime(данные.ArrivalTime);
                                var departureTime = parser.ToDateTime(данные.DepartureTime);
                                данные.StopTime = arrivalTime != DateTime.MinValue && departureTime != DateTime.MinValue ?
                                                    (departureTime < arrivalTime ?
                                                    departureTime.AddDays(1) - arrivalTime :
                                                    departureTime - arrivalTime).ToString("hh\\:mm") :
                                                  string.Empty;
                                данные.Days = !string.IsNullOrWhiteSpace(settings[6]) ? settings[6] : $"Отс:0:0:0:0:0:0:0:0:0:0:0:0:0:0:0:0";
                                данные.DaysDescription = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(данные.Days).ПолучитьСтрокуОписанияРасписания();
                                данные.Active = settings[7] == "1" ? true : false;
                                данные.SoundTemplates = settings[8];
                                данные.TrainPathDirection = byte.Parse(settings[9]);
                                данные.TrainPathNumber = LoadPathFromFile(settings[10], out данные.PathWeekDayes);
                                данные.ИспользоватьДополнение = new Dictionary<string, bool>()
                                {
                                    ["звук"] = false,
                                    ["табло"] = false
                                };


                                ТипПоезда типПоезда = ТипПоезда.НеОпределен;
                                try
                                {
                                    типПоезда = (ТипПоезда)Enum.Parse(typeof(ТипПоезда), settings[11]);
                                }
                                catch (ArgumentException) { }
                                данные.ТипПоезда = типПоезда;

                                данные.Примечание = settings[12];

                                if (данные.TrainPathDirection > 2)
                                    данные.TrainPathDirection = 0;

                                var path = Program.TrackRepository.List().FirstOrDefault(p => p.Name == данные.TrainPathNumber[WeekDays.Постоянно]);
                                if (path == null)
                                    данные.TrainPathNumber[WeekDays.Постоянно] = "";

                                DateTime началоДействия = new DateTime(DateTime.Now.Year, 1, 1);
                                DateTime конецДействия = new DateTime(2100, 1, 1);
                                if (settings.Length >= 15)
                                {
                                    DateTime.TryParse(settings[13], out началоДействия);
                                    DateTime.TryParse(settings[14], out конецДействия);
                                }
                                данные.ВремяНачалаДействияРасписания = началоДействия;
                                данные.ВремяОкончанияДействияРасписания = конецДействия;


                                var addition = "";
                                if (settings.Length >= 16)
                                {
                                    addition = settings[15];
                                }
                                данные.Addition = addition;


                                if (settings.Length >= 18)
                                {
                                    данные.ИспользоватьДополнение["табло"] = settings[16] == "1";
                                    данные.ИспользоватьДополнение["звук"] = settings[17] == "1";
                                }

                                данные.Автомат = true;
                                if (settings.Length >= 19)
                                {
                                    данные.Автомат = (string.IsNullOrEmpty(settings[18]) || settings[18] == "1"); // по умолчанию true
                                }


                                данные.Num2 = String.Empty;
                                данные.FollowingTime = String.Empty;
                                данные.DaysAlias = String.Empty;
                                if (settings.Length >= 22)
                                {
                                    данные.Num2 = settings[19];
                                    данные.FollowingTime = settings[20];
                                    данные.DaysAlias = settings[21]?.Replace("{rn}", "\r\n");
                                }


                                данные.StationDepart = String.Empty;
                                данные.StationArrival = String.Empty;
                                if (settings.Length >= 23)
                                {
                                    данные.StationDepart = settings[22];
                                    данные.StationArrival = settings[23];
                                }

                                данные.Direction = String.Empty;
                                if (settings.Length >= 25)
                                {
                                    данные.Direction = settings[24];
                                }

                                данные.ChangeTrainPathDirection = false;
                                if (settings.Length >= 26)
                                {
                                    bool changeDirection;
                                    bool.TryParse(settings[25], out changeDirection);
                                    данные.ChangeTrainPathDirection = changeDirection;
                                }

                                данные.IsScoreBoardOutput = false;
                                if (settings.Length >= 27)
                                {
                                    bool выводНаТабло;
                                    bool.TryParse(settings[26], out выводНаТабло);
                                    данные.IsScoreBoardOutput = выводНаТабло;
                                }
                                
                                данные.IsSoundOutput = true;
                                if (settings.Length >= 28)
                                {
                                    bool выводЗвука;
                                    bool.TryParse(settings[27], out выводЗвука);
                                    данные.IsSoundOutput = выводЗвука;
                                }

                                данные.DaysAliasEng = string.Empty;
                                if (settings.Length >= 29)
                                {
                                    данные.DaysAliasEng = settings[28];
                                }

                                данные.AdditionEng = string.Empty;
                                if (settings.Length >= 30)
                                {
                                    данные.AdditionEng = settings[29];
                                }

                                данные.NoteEng = string.Empty;
                                if (settings.Length >= 31)
                                {
                                    данные.NoteEng = settings[30];
                                }

                                int id;
                                данные.ScheduleId = 0;
                                if (settings.Length >= 32 && int.TryParse(settings[31], out id))
                                {
                                    данные.ScheduleId = id;
                                }

                                данные.TrnId = 0;
                                if (settings.Length >= 33 && int.TryParse(settings[32], out id))
                                {
                                    данные.TrnId = id;
                                }

                                данные.TieTrainId = 0;
                                if (settings.Length >= 34 && int.TryParse(settings[33], out id))
                                {
                                    данные.TieTrainId = id;
                                }

                                bool denyAutoUpdate;
                                данные.DenyAutoUpdate = false;
                                if (settings.Length >= 35 && bool.TryParse(settings[34], out denyAutoUpdate))
                                {
                                    данные.DenyAutoUpdate = denyAutoUpdate;
                                }

                                trainTableRecords.Add(данные);
                                Program.НомераПоездов.Add(данные.Num);
                                if (!string.IsNullOrEmpty(данные.Num2))
                                    Program.НомераПоездов.Add(данные.Num2);
                            }
                        }
                    }
                    return trainTableRecords;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                    return null;
                }
            }
        }



        private static string SavePath2File(Dictionary<WeekDays, string> pathDictionary, bool pathWeekDayes)
        {
            StringBuilder strBuild = new StringBuilder();
            foreach (var keyVal in pathDictionary)
            {
                var value = (keyVal.Value == "Не определен") ? string.Empty : keyVal.Value;
                strBuild.Append(keyVal.Key).Append(":").Append(value).Append("|");
            }
            strBuild.Append("ПутиПоДням").Append(":").Append(pathWeekDayes ? "1" : "0");

            return strBuild.ToString();
        }



        private static Dictionary<WeekDays, string> LoadPathFromFile(string str, out bool pathWeekDayes)
        {
            Dictionary<WeekDays, string> pathDictionary = new Dictionary<WeekDays, string>
            {
                [WeekDays.Постоянно] = String.Empty,
                [WeekDays.Пн] = String.Empty,
                [WeekDays.Вт] = String.Empty,
                [WeekDays.Ср] = String.Empty,
                [WeekDays.Ср] = String.Empty,
                [WeekDays.Чт] = String.Empty,
                [WeekDays.Пт] = String.Empty,
                [WeekDays.Сб] = String.Empty,
                [WeekDays.Вс] = String.Empty
            };
            pathWeekDayes = false;

            if (!string.IsNullOrEmpty(str) && str.Contains("|") && str.Contains(":"))
            {
                var pairs = str.Split('|');
                if (pairs.Length == 9)
                {
                    foreach (var pair in pairs)
                    {
                        var keyVal = pair.Split(':');

                        var value = (keyVal[1] == "Не определен") ? string.Empty : keyVal[1];
                        switch (keyVal[0])
                        {
                            case "Постоянно":
                                pathDictionary[WeekDays.Постоянно] = value;
                                break;

                            case "Пн":
                                pathDictionary[WeekDays.Пн] = value;
                                break;

                            case "Вт":
                                pathDictionary[WeekDays.Вт] = value;
                                break;

                            case "Ср":
                                pathDictionary[WeekDays.Ср] = value;
                                break;

                            case "Чт":
                                pathDictionary[WeekDays.Чт] = value;
                                break;

                            case "Пт":
                                pathDictionary[WeekDays.Пт] = value;
                                break;

                            case "Сб":
                                pathDictionary[WeekDays.Сб] = value;
                                break;

                            case "Вс":
                                pathDictionary[WeekDays.Вс] = value;
                                break;

                            case "ПутиПоДням":
                                pathWeekDayes = (keyVal[1] == "1");
                                break;
                        }
                    }
                }
            }

            return pathDictionary;
        }

        private static void ChangeTimeZoneInTableRecords(IList<TrainTableRecord> table, string format, int offset)
        {
            for (var i = 0; i < table.Count; i++)
            {
                var tr = table[i];
                tr.ArrivalTime = AddHours(tr.ArrivalTime, format, offset);
                tr.DepartureTime = AddHours(tr.DepartureTime, format, offset);
                table[i] = tr;
            }
        }

        private static string AddHours(string stringTime, string format, int hours)
        {
            DateTime time;
            return !string.IsNullOrWhiteSpace(stringTime) &&
                   DateTime.TryParseExact(stringTime, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out time) ?
                   time.AddHours(hours).ToString(format) :
                   stringTime;
        }

        #endregion
    }
}
