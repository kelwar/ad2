using CommunicationDevices.Behavior.BindingBehavior.ToChange;
using CommunicationDevices.Behavior.BindingBehavior.ToGeneralSchedule;
using CommunicationDevices.Behavior.BindingBehavior.ToGetData;
using CommunicationDevices.Behavior.BindingBehavior.ToPath;
using CommunicationDevices.DataProviders;
using Domain.Entitys;
using Library.Logs;
using MainExample.Entites;
using MainExample.Infrastructure;
using MainExample.Mappers;
using MainExample.Services.FactoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainExample
{
    public class BoardManager
    {
        private uint _tickCounter = 0;

        public IEnumerable<IBinding2PathBehavior> Binding2PathBehaviors { get; set; }
        public IEnumerable<IBinding2GeneralSchedule> Binding2GeneralScheduleBehaviors { get; set; }
        public IEnumerable<IBinding2ChangesBehavior> Binding2ChangesBehaviors { get; set; }
        public IEnumerable<IBinding2ChangesEventBehavior> Binding2ChangesEventBehaviors { get; set; }
        public IEnumerable<IBinding2GetData> Binding2GetDataBehaviors { get; set; }

        public BoardManager(IEnumerable<IBinding2PathBehavior> binding2PathBehaviors,
                            IEnumerable<IBinding2GeneralSchedule> binding2GeneralScheduleBehaviors,
                            IEnumerable<IBinding2ChangesBehavior> binding2ChangesBehaviors,
                            IEnumerable<IBinding2ChangesEventBehavior> binding2ChangesEventBehaviors,
                            IEnumerable<IBinding2GetData> binding2GetDataBehaviors)
        {
            Binding2PathBehaviors = binding2PathBehaviors;
            Binding2GeneralScheduleBehaviors = binding2GeneralScheduleBehaviors;
            Binding2ChangesBehaviors = binding2ChangesBehaviors;
            Binding2ChangesEventBehaviors = binding2ChangesEventBehaviors;
            Binding2GetDataBehaviors = binding2GetDataBehaviors;
        }
        
        public void InitializeTrackBoards()
        {
            try
            {
                foreach (var beh in Binding2PathBehaviors)
                {
                    beh.InitializeDevicePathInfo();
                }
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }

        public void SetDisplayStateAndMessageTypeForAllRecordsAndSend2TrackBoards()
        {
            for (var i = 0; i < MainWindowForm.SoundRecords.Count; i++)
            {
                KeyValuePair<string, SoundRecord> rec;
                lock (MainWindowForm.SoundRecords_Lock)
                {
                    rec = MainWindowForm.SoundRecords.ElementAt(i);
                }
                var данные = rec.Value;
                if (!string.IsNullOrWhiteSpace(данные.НомерПути))
                {
                    данные.СостояниеОтображения = данные.НомерПути != "0" ? TableRecordStatus.Отображение : TableRecordStatus.Очистка;
                    данные.ТипСообщения = SoundRecordType.ДвижениеПоезда;

                    lock (MainWindowForm.SoundRecords_Lock)
                    {
                        MainWindowForm.SoundRecords[rec.Key] = данные;
                    }
                    SendOnPathTable(данные);
                }
            }
        }

        public void DefineAndSendInfo2Board()
        {
            #region ВЫВОД РАСПИСАНИЯ НА ТАБЛО (из главного окна или из окна расписания)

            if (_tickCounter++ > 50)
            {
                _tickCounter = 0;

                var uitPreprocessingService = PreprocessingOutputFactory.CreateUitPreprocessingOutputService();

                if (Binding2GeneralScheduleBehaviors != null && Binding2GeneralScheduleBehaviors.Any())
                {
                    var binding2MainWindow = Binding2GeneralScheduleBehaviors
                        .Where(b => b.SourceLoad == SourceLoad.MainWindow || b.SourceLoad == SourceLoad.Dispatcher)
                        .ToList();
                    var binding2Shedule = Binding2GeneralScheduleBehaviors
                        .Where(b => b.SourceLoad == SourceLoad.Shedule)
                        .ToList();
                    var binding2OperativeShedule = Binding2GeneralScheduleBehaviors
                        .Where(b => b.SourceLoad == SourceLoad.SheduleOperative)
                        .ToList();
                    var binding2Stations = Binding2GeneralScheduleBehaviors
                        .Where(b => b.SourceLoad == SourceLoad.Stations)
                        .ToList();

                    //Отправить расписание из окна РАСПИСАНИЕ
                    if (binding2Shedule.Any())
                    {
                        if (TrainSheduleTable.TrainTableRecords != null)
                        {
                            foreach (var beh in binding2Shedule)
                            {
                                var table = TrainSheduleTable.TrainTableRecords
                                    .Select(Mapper.MapTrainTableRecord2UniversalInputType)
                                    .ToList();


                                table.ForEach(t =>
                                {
                                    uitPreprocessingService.StartPreprocessing(t);
                                    t.Command = Command.View;
                                    t.Message = $"ПОЕЗД:{t.NumberOfTrain}, ПУТЬ:{t.PathNumber}, СОБЫТИЕ:{t.Event}, СТАНЦИИ:{t.Stations}, ВРЕМЯ:{t.Time.ToShortTimeString()}";
                                });

                                var inData = new UniversalInputType { TableData = table };
                                beh.InitializePagingBuffer(inData, beh.CheckContrains, beh.GetCountDataTake());
                            }
                        }
                    }


                    //Отправить расписание из окна ОПЕРАТИВНОГО РАСПИСАНИЕ
                    if (binding2OperativeShedule.Any())
                    {
                        if (TrainTableOperative.TrainTableRecords != null)
                        {
                            foreach (var beh in binding2OperativeShedule)
                            {
                                var table = TrainTableOperative.TrainTableRecords
                                    .Select(Mapper.MapTrainTableRecord2UniversalInputType)
                                    .ToList();

                                table.ForEach(t =>
                                {
                                    uitPreprocessingService.StartPreprocessing(t);
                                    t.Command = Command.View;
                                    t.Message = $"ПОЕЗД:{t.NumberOfTrain}, ПУТЬ:{t.PathNumber}, СОБЫТИЕ:{t.Event}, СТАНЦИИ:{t.Stations}, ВРЕМЯ:{t.Time.ToShortTimeString()}";
                                });

                                var inData = new UniversalInputType { TableData = table };
                                beh.InitializePagingBuffer(inData, beh.CheckContrains, beh.GetCountDataTake());
                            }
                        }
                    }

                    //Отправить расписание из ГЛАВНОГО окна  
                    if (binding2MainWindow.Any())
                    {
                        if (MainWindowForm.SoundRecords != null && MainWindowForm.SoundRecords.Any())
                        {
                            foreach (var beh in binding2MainWindow)
                            {
                                var table = new List<UniversalInputType>();

                                for (int i = 0; i < MainWindowForm.SoundRecords.Count; i++)
                                {
                                    KeyValuePair<string, SoundRecord> rec;
                                    lock (MainWindowForm.SoundRecords_Lock)
                                    {
                                        rec = MainWindowForm.SoundRecords.ElementAt(i);
                                    }
                                    table.Add(Mapper.MapSoundRecord2UniveralInputType(rec.Value, beh.GetDeviceSetting.PathPermission, false));
                                }

                                table.ForEach(t =>
                                {
                                    uitPreprocessingService.StartPreprocessing(t);
                                    t.Command = Command.View;
                                    t.Message = $"ПОЕЗД:{t.NumberOfTrain}, ПУТЬ:{t.PathNumber}, СОБЫТИЕ:{t.Event}, СТАНЦИИ:{t.Stations}, ВРЕМЯ:{t.Time.ToShortTimeString()}";
                                });
                                var inData = new UniversalInputType { TableData = table };                                      // Прописать ещё один таймер. Если пейджинга нет, меняем язык по его истечении
                                                                                                                                // Если есть - меняем язык по истечении времени пейджинга последней страницы
                                beh.InitializePagingBuffer(inData, beh.CheckContrains, beh.GetCountDataTake());
                            }
                        }
                    }

                    // Отправка станций из файла Stations.xml
                    if (binding2Stations.Any())
                    {
                        if (Program.DirectionRepository != null && Program.DirectionRepository.List().Any())
                        {
                            foreach (var beh in binding2Stations)
                            {
                                var inData = new UniversalInputType() { TableData = new List<UniversalInputType>() };
                                var uit = new UniversalInputType();
                                uit.ViewBag = new Dictionary<string, dynamic>() { ["Directions"] = Program.DirectionRepository.List() };
                                inData.TableData.Add(uit);
                                beh.InitializePagingBuffer(inData, u => true, beh.GetCountDataTake());
                            }
                        }
                    }
                }


                //ОТПРАВИТЬ ИЗМЕНЕНИЯ
                if (Binding2ChangesBehaviors != null && Binding2ChangesBehaviors.Any())
                {
                    foreach (var beh in Binding2ChangesBehaviors)
                    {
                        //загрузим список изменений на глубину beh.HourDepth.
                        var min = DateTime.Now.AddHours(beh.HourDepth * (-1));
                        var changes = Program.SoundRecordChangesDbRepository.List()
                            .Where(p => p.TimeStamp >= min)
                            .Select(Mapper.SoundRecordChangesDb2SoundRecordChanges)
                            .ToList();


                        List<UniversalInputType> table = new List<UniversalInputType>();
                        foreach (var change in changes)
                        {
                            var uit = Mapper.MapSoundRecord2UniveralInputType(change.Rec, beh.GetDeviceSetting.PathPermission, false);
                            uit.ViewBag = new Dictionary<string, dynamic>
                            {
                                { "TimeStamp", change.TimeStamp },
                                { "UserInfo", change.UserInfo },
                                { "CauseOfChange", change.CauseOfChange }
                            };
                            uit.Command = Command.View;


                            var uitNew = Mapper.MapSoundRecord2UniveralInputType(change.NewRec, beh.GetDeviceSetting.PathPermission, false);
                            uitNew.ViewBag = new Dictionary<string, dynamic>
                            {
                                { "TimeStamp", change.TimeStamp },
                                { "UserInfo", change.UserInfo },
                                { "CauseOfChange", change.CauseOfChange }
                            };
                            uit.Command = Command.View;

                            table.Add(uit);
                            table.Add(uitNew);
                        }

                        table.ForEach(t => t.Message = $"ПОЕЗД:{t.NumberOfTrain}, ПУТЬ:{t.PathNumber}, СОБЫТИЕ:{t.Event}, СТАНЦИИ:{t.Stations}, ВРЕМЯ:{t.Time.ToShortTimeString()}");
                        var inData = new UniversalInputType { TableData = table };
                        beh.InitializePagingBuffer(inData, beh.CheckContrains, beh.GetCountDataTake());
                    }
                }
            }

            #endregion

            #region ВЫВОД НА ПУТЕВЫЕ ТАБЛО
            for (var i = 0; i < MainWindowForm.SoundRecords.Count; i++)                     // Перебираем все записи
            {
                try
                {
                    KeyValuePair<string, SoundRecord> record;
                    lock (MainWindowForm.SoundRecords_Lock)
                    {                                                        // Получаем значение текущей записи
                        record = MainWindowForm.SoundRecords.ElementAt(i);
                    }                                                        // Получаем ключ текущей записи
                    var данные = record.Value;
                    var данныеOld = MainWindowForm.SoundRecordsOld.ElementAt(i).Value;      // Получаем значение записи с таким же индексом из репозитория старых записей       

                    if (!данные.Автомат)                                     // Пропускаем запись, которая не автомат
                        continue;


                    // Вводим условие проверки, не является ли запись "Выключенной" (не ушёл ли поезд)
                    // Если это не так, и тип сообщения "Движение"
                    if (данные.Состояние != SoundRecordStatus.Выключена && данные.ТипСообщения == SoundRecordType.ДвижениеПоезда)
                    {
                        //ВЫВОД НА ПУТЕВЫЕ ТАБЛО
                        var номераПутей = Program.TrackRepository.List().Select(p => p.Name).ToList(); // Получаем список путей
                        var номерПути = номераПутей.IndexOf(данные.НомерПути) + 1;                        // Если такой путь не был найден (индекс равен -1), номер пути считать индексом
                        var номерПутиOld = номераПутей.IndexOf(данныеOld.НомерПути) + 1;                  // -//-

                        if (номерПути > 0 || номерПутиOld > 0)                        // Если номер пути выставлен или был выставлен и ушёл самостоятельно (не был снят)
                        {
                            //ПОМЕНЯЛИ ПУТЬ
                            if (номерПути != номерПутиOld)                                                // Если номера путей при этом не совпадают
                            {
                                //очистили старый путь, если он не "0";
                                if (номерПутиOld > 0)                                                     // Если старый путь не "0"
                                {
                                    данныеOld.СостояниеОтображения = TableRecordStatus.Очистка;           // 
                                    SendOnPathTable(данныеOld);
                                }

                                //вывод на новое табло
                                данные.СостояниеОтображения = TableRecordStatus.Отображение;
                                SendOnPathTable(данные);
                            }
                            else
                            {
                                //ИЗДАНИЕ СОБЫТИЯ ИЗМЕНЕНИЯ ДАННЫХ В ЗАПИСИ SoundRecords.
                                if (!StructCompare.SoundRecordComparer(ref данные, ref данныеOld))
                                {
                                    данные.СостояниеОтображения = TableRecordStatus.Обновление;
                                    SendOnPathTable(данные);
                                }
                            }



                            //ОТПРАВЛЕНИЕ, ТРАНЗИТЫ
                            if ((данные.БитыАктивностиПолей & 0x10) == 0x10 ||
                                (данные.БитыАктивностиПолей & 0x14) == 0x14)
                            {
                                //ОЧИСТИТЬ если нет нештатных ситуаций на момент отправления
                                if ((DateTime.Now >= данные.ВремяОтправления.AddMinutes(1) &&
                                   (DateTime.Now <= данные.ВремяОтправления.AddMinutes(1.02))) && //1
                                   ((данные.БитыНештатныхСитуаций & 0x0F) == 0x00) &&
                                   (данные.СостояниеОтображения == TableRecordStatus.Отображение ||
                                   данные.СостояниеОтображения == TableRecordStatus.Обновление))
                                {
                                    данные.СостояниеОтображения = TableRecordStatus.Очистка;
                                    данные.НомерПути = "0";

                                    var данныеОчистки = данные;
                                    данныеОчистки.НомерПути = данныеOld.НомерПути;
                                    SendOnPathTable(данныеОчистки);

                                    if (MainWindowForm.myMainForm != null)
                                        MainWindowForm.myMainForm.СохранениеИзмененийДанныхКарточкеБД(данныеOld, данные); //DEBUG
                                }

                                //ОЧИСТИТЬ если убрали нештатные ситуации
                                if (((данные.БитыНештатныхСитуаций & 0x0F) == 0x00)
                                    && ((данныеOld.БитыНештатныхСитуаций & 0x0F) != 0x00)
                                    && (DateTime.Now >= данные.ВремяОтправления.AddMinutes(1)))
                                {
                                    if (данные.СостояниеОтображения == TableRecordStatus.Отображение ||
                                        (данные.СостояниеОтображения == TableRecordStatus.Обновление))
                                    {
                                        данные.СостояниеОтображения = TableRecordStatus.Очистка;
                                        данные.НомерПути = "0";

                                        var данныеОчистки = данные;
                                        данныеОчистки.НомерПути = данныеOld.НомерПути;
                                        SendOnPathTable(данныеОчистки);

                                        if (MainWindowForm.myMainForm != null)
                                            MainWindowForm.myMainForm.СохранениеИзмененийДанныхКарточкеБД(данныеOld, данные); //DEBUG
                                    }
                                }
                            }
                            //ПРИБЫТИЕ
                            else if ((данные.БитыАктивностиПолей & 0x04) == 0x04)
                            {
                                //ОЧИСТИТЬ если нет нештатных ситуаций на момент прибытия
                                if ((DateTime.Now >= данные.ВремяПрибытия.AddMinutes(10) && //10
                                     (DateTime.Now <= данные.ВремяПрибытия.AddMinutes(10.02))))
                                {
                                    if ((данные.БитыНештатныхСитуаций & 0x0F) == 0x00)
                                        if (данные.СостояниеОтображения == TableRecordStatus.Отображение ||
                                            (данные.СостояниеОтображения == TableRecordStatus.Обновление))
                                        {
                                            данные.СостояниеОтображения = TableRecordStatus.Очистка;
                                            данные.НомерПути = "0";

                                            var данныеОчистки = данные;
                                            данныеОчистки.НомерПути = данныеOld.НомерПути;
                                            SendOnPathTable(данныеОчистки);

                                            if (MainWindowForm.myMainForm != null)
                                                MainWindowForm.myMainForm.СохранениеИзмененийДанныхКарточкеБД(данныеOld, данные); //DEBUG
                                        }
                                }


                                //ОЧИСТИТЬ если убрали нештатные ситуации
                                if (((данные.БитыНештатныхСитуаций & 0x0F) == 0x00)
                                    && ((данныеOld.БитыНештатныхСитуаций & 0x0F) != 0x00)
                                    && (DateTime.Now >= данные.ВремяПрибытия.AddMinutes(10)))
                                {
                                    if (данные.СостояниеОтображения == TableRecordStatus.Отображение ||
                                        (данные.СостояниеОтображения == TableRecordStatus.Обновление))
                                    {
                                        данные.СостояниеОтображения = TableRecordStatus.Очистка;
                                        данные.НомерПути = "0";

                                        var данныеОчистки = данные;
                                        данныеОчистки.НомерПути = данныеOld.НомерПути;
                                        SendOnPathTable(данныеОчистки);

                                        if (MainWindowForm.myMainForm != null)
                                            MainWindowForm.myMainForm.СохранениеИзмененийДанныхКарточкеБД(данныеOld, данные); //DEBUG
                                    }
                                }
                            }

                        }
                    }

                    lock (MainWindowForm.SoundRecords_Lock)
                    {
                        MainWindowForm.SoundRecords[record.Key] = данные;
                    }
                    MainWindowForm.SoundRecordsOld[record.Key] = данные;
                }
                catch (Exception ex)
                {
                    Log.log.Error($"Ошибка при изменении данных: {ex}");
                }
            }

            #endregion
        }

        /// <summary>
        /// Отправка изменения.
        /// </summary>
        public void SendData4Binding2ChangesEvent(SoundRecordChanges recChange)
        {
            if (Binding2ChangesEventBehaviors != null && Binding2ChangesEventBehaviors.Any())
            {
                foreach (var beh in Binding2ChangesEventBehaviors)
                {
                    List<UniversalInputType> table = new List<UniversalInputType>();
                    var uit = Mapper.MapSoundRecord2UniveralInputType(recChange.Rec, true, false);
                    uit.ViewBag = new Dictionary<string, dynamic>
                    {
                        { "TimeStamp", recChange.TimeStamp },
                        { "UserInfo", recChange.UserInfo },
                        { "CauseOfChange", recChange.CauseOfChange }
                    };

                    var uitNew = Mapper.MapSoundRecord2UniveralInputType(recChange.NewRec, true, false);
                    uitNew.ViewBag = new Dictionary<string, dynamic>
                    {
                        { "TimeStamp", recChange.TimeStamp },
                        { "UserInfo", recChange.UserInfo },
                        { "CauseOfChange", recChange.CauseOfChange }
                    };

                    table.Add(uit);
                    table.Add(uitNew);
                    table.ForEach(t => t.Message = $"ПОЕЗД:{t.NumberOfTrain}, ПУТЬ:{t.PathNumber}, СОБЫТИЕ:{t.Event}, СТАНЦИИ:{t.Stations}, ВРЕМЯ:{t.Time.ToShortTimeString()}");
                    var inData = new UniversalInputType { TableData = table };
                    beh.SendMessage(inData);
                }
            }
        }

        // Здесь отправлять на табло только uitData, вызывать из методов инициализации, добавления и удаления данных с табло
        private void SendOnPathTable(SoundRecord data)
        {
            if (data.СостояниеОтображения == TableRecordStatus.Выключена || data.СостояниеОтображения == TableRecordStatus.ОжиданиеОтображения)
                return;

            if (data.НазванияТабло == null || !data.НазванияТабло.Any())
                return;



            // Формируем список табло, на которые отправлять данные
            var devicesId = data.НазванияТабло.Select(s => new string(s.TakeWhile(c => c != ':').ToArray())).Select(int.Parse).ToList();
            foreach (var devId in devicesId)
            {
                // Находим первое совпавшее
                var beh = Binding2PathBehaviors.FirstOrDefault(b => b.GetDeviceId == devId);

                // Если такое есть
                if (beh != null)
                {
                    var uit = Mapper.MapSoundRecord2UniveralInputType(data, beh.GetDeviceSetting.PathPermission, true);
                    //uitData.TableData.Add(uit);
                    var uitPreprocessingService = PreprocessingOutputFactory.CreateUitPreprocessingOutputService();
                    uitPreprocessingService.StartPreprocessing(uit);
                    uit.Message = $"ПОЕЗД:{uit.NumberOfTrain}, ПУТЬ:{uit.PathNumber}, СОБЫТИЕ:{uit.Event}, СТАНЦИИ:{uit.Stations}, ВРЕМЯ:{uit.Time.ToShortTimeString()}";

                    var numberOfTrain = (string.IsNullOrEmpty(data.НомерПоезда2) || string.IsNullOrWhiteSpace(data.НомерПоезда2)) ? data.НомерПоезда : (data.НомерПоезда + "/" + data.НомерПоезда2);

                    beh.SendMessage4Path(uit, numberOfTrain, beh.CheckContrains, beh.GetCountDataTake());
                    //Debug.WriteLine($" ТАБЛО= {beh.GetDeviceName}: {beh.GetDeviceId} для ПУТИ {data.НомерПути}.  Сообшение= {inData.Message}  ");
                }
            }
        }

    }
}
