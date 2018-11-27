using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Linq;
using AutodictorBL.Entites;
using AutodictorBL.Sound;
using CommunicationDevices.Behavior.BindingBehavior.ToChange;
using CommunicationDevices.Behavior.BindingBehavior.ToGeneralSchedule;
using CommunicationDevices.Behavior.BindingBehavior.ToGetData;
using CommunicationDevices.Behavior.BindingBehavior.ToPath;
using CommunicationDevices.ClientWCF;
using CommunicationDevices.Devices;
using CommunicationDevices.DataProviders;
using Domain.Entitys;
using Domain.Entitys.Authentication;
using Library.Convertion;
using MainExample.Entites;
using MainExample.Extension;
using MainExample.Infrastructure;
using MainExample.Mappers;
using MainExample.Services;
using Library.Logs;
using MainExample.Services.FactoryServices;
using MainExample.Services.GetDataService;
using MoreLinq;
using Timer = System.Timers.Timer;
using CommunicationDevices.Model;
using Domain.Entitys.Train;

namespace MainExample
{
    public struct SoundRecord
    {
        public int ID { get; set; }
        public string НомерПоезда { get; set; }
        public string НомерПоезда2 { get; set; }
        public string НазваниеПоезда { get; set; }
        public string Направление { get; set; }
        public string СтанцияОтправления { get; set; }
        public string СтанцияНазначения { get; set; }
        public DateTime Время { get; set; }
        public DateTime ВремяПрибытия { get; set; }
        public DateTime ВремяОтправления { get; set; }
        public DateTime? ВремяЗадержки { get; set; }                      //время задержки в мин. относительно времени прибытия или отправелния
        public TimeSpan? DelayTime { get; set; }                          // переводит время задержки из Datetime в TimeSpan
        public DateTime ОжидаемоеВремя { get; set; }                      //вычисляется ВремяПрибытия или ВремяОтправления + ВремяЗадержки
        public DateTime? ВремяСледования { get; set; }                    //время в пути
        public TimeSpan? ВремяСтоянки { get; set; }                       //вычисляется для танзитов (ВремяОтправления - ВремяПрибытия)
        public DateTime? ФиксированноеВремяПрибытия { get; set; }         // фиксированное время
        public DateTime? ФиксированноеВремяОтправления { get; set; }      // фиксированное время + время стоянки
        public string Дополнение { get; set; }                            //свободная переменная для ввода  
        public string AdditionEng { get; set; }
        public Dictionary<string, bool> ИспользоватьДополнение { get; set; } //[звук] - использовать дополнение для звука.  [табло] - использовать дополнение для табло.
        public string ДниСледования { get; set; }
        public string ДниСледованияAlias { get; set; }                    // дни следования записанные в ручную
        public string DaysFollowingAliasEng { get; set; }
        public bool Активность { get; set; }
        public bool Автомат { get; set; }                                 // true - поезд обрабатывается в автомате.
        public string ШаблонВоспроизведенияСообщений { get; set; }
        public byte НумерацияПоезда { get; set; }                         // 1 - с головы,  2 - с хвоста
        public bool СменнаяНумерацияПоезда { get; set; }                  // для транзитов
        public string НомерПути { get; set; }
        public string НомерПутиБезАвтосброса { get; set; }                //выставленные пути не обнуляются через определенное время
        //public Pathways Track;
        public ТипПоезда ТипПоезда { get; set; }
        public string Примечание { get; set; }                            //С остановками....
        public string NoteEng { get; set; }
        public string Описание { get; set; }
        public SoundRecordStatus Состояние { get; set; }
        public SoundRecordType ТипСообщения { get; set; }
        public byte БитыАктивностиПолей { get; set; }
        public string[] НазванияТабло { get; set; }
        public TableRecordStatus СостояниеОтображения { get; set; }
        public PathPermissionType РазрешениеНаОтображениеПути { get; set; }
        public string[] ИменаФайлов { get; set; }
        public byte КоличествоПовторений { get; set; }
        public List<СостояниеФормируемогоСообщенияИШаблон> СписокФормируемыхСообщений { get; set; }
        public List<СостояниеФормируемогоСообщенияИШаблон> СписокНештатныхСообщений { get; set; }
        public byte СостояниеКарточки { get; set; }
        public string ОписаниеСостоянияКарточки { get; set; }
        public byte БитыНештатныхСитуаций { get; set; } // бит 0 - Отмена, бит 1 - задержка прибытия, бит 2 - задержка отправления, бит 4 - отправление по готовности
        public uint ТаймерПовторения { get; set; }

        public bool ВыводНаТабло { get; set; }     // Работает только при наличии Contrains "SendingDataLimit".
        public bool ВыводЗвука { get; set; }       //True - разрешен вывод звуковых шаблонов.

        public DateTime ActualArrivalTime { get; set; }
        public DateTime ActualDepartureTime { get; set; }
        public TimetableType TimetableType { get; set; }

        public Composition Composition
        {
            get
            {
                return _composition;
            }
            set
            {
                _composition = value;
                CompositionChanged?.Invoke(_composition);
            }
        }                       // Состав поезда


        public IdTrain IdTrain;
        public delegate void CompositionChangedHandler(Composition composition);
        public CompositionChangedHandler CompositionChanged;

        private Composition _composition;



        #region Methode

        public void AplyIdTrain()
        {
            //IdTrain.НомерПоезда = НомерПоезда;
            //IdTrain.НомерПоезда2 = НомерПоезда2;
            //IdTrain.СтанцияОтправления = СтанцияОтправления;
            //IdTrain.СтанцияНазначения = СтанцияНазначения;
            //IdTrain.ДеньПрибытия = ВремяПрибытия.Date;
            //IdTrain.ДеньОтправления = ВремяОтправления.Date;
        }

        #endregion
    };

    /// <summary>
    /// ИДЕНТИФИКАТОР ПОЕЗДА.
    /// для сопоставления поезда из распсиания.
    /// </summary>
    public struct IdTrain
    {
        public IdTrain(int scheduleId, int trnId = 0) : this()
        {
            ScheduleId = scheduleId;
            TrnId = trnId;
        }

        public int ScheduleId { get; }                   //Id поезда в распсиании
        public int TrnId { get; }
        //public DateTime ДеньПрибытия { get; set; }       //сутки в которые поезд ПРИБ.  
        //public DateTime ДеньОтправления { get; set; }    //сутки в которые поезд ОТПР.
        //public string НомерПоезда { get; set; }         //номер поезда 1
        //public string НомерПоезда2 { get; set; }        //номер поезда 2
        //public string СтанцияОтправления { get; set; }
        //public string СтанцияНазначения { get; set; }
    }

    public struct СостояниеФормируемогоСообщенияИШаблон
    {
        public int Id;                            // порядковый номер шаблона
        public int SoundRecordId;                 // строка расписания к которой принадлежит данный шаблон
        public bool Активность;
        public Priority ПриоритетГлавный;
        public PriorityPrecise ПриоритетВторостепенный;
        public bool Воспроизведен;                //???
        public SoundRecordStatus СостояниеВоспроизведения;
        public int ПривязкаКВремени;              // 0 - приб. 1- отпр
        public int ВремяСмещения;
        public string НазваниеШаблона;
        public string Шаблон;
        public List<NotificationLanguage> ЯзыкиОповещения;
    };

    public struct СтатическоеСообщение
    {
        public int ID;
        public DateTime Время;
        public string НазваниеКомпозиции;
        public string ОписаниеКомпозиции;
        public SoundRecordStatus СостояниеВоспроизведения;
        public bool Активность;
    };

    public struct ОписаниеСобытия
    {
        public DateTime Время;
        public string Описание;
        public byte НомерСписка;            // 0 - Динамические сообщения, 1 - статические звуковые сообщения
        public string Ключ;
        public byte СостояниеСтроки;        // 0 - Выключена, 1 - движение поезда (динамика), 2 - статическое сообщение, 3 - аварийное сообщение, 4 - воспроизведение, 5 - воспроизведЕН
        public string ШаблонИлиСообщение;   //текст стат. сообщения, или номер шаблона в динам. сообщении (для Субтитров)
    };

    enum MainWindowWorkMode
    {
        OnlyDictor, OnlyDispatcher, Both
    }

    public partial class MainWindowForm : Form
    {
        #region Fields

        public const string DATETIME_KEYFORMAT = "yy.MM.dd  HH:mm:ss";

        public static SortedDictionary<string, SoundRecord> SoundRecords = new SortedDictionary<string, SoundRecord>();
        public static SortedDictionary<string, SoundRecord> SoundRecordsOld = new SortedDictionary<string, SoundRecord>();
        public static SortedDictionary<string, СтатическоеСообщение> СтатическиеЗвуковыеСообщения = new SortedDictionary<string, СтатическоеСообщение>();
        public static List<SoundRecordChanges> SoundRecordChanges = new List<SoundRecordChanges>();  //Изменения на тек.сутки + изменения на пред. сутки для поездов ходящих в тек. сутки
        public static MainWindowForm myMainForm = null;
        public static QueueSoundService QueueSound = new QueueSoundService(Program.AutodictorModel.SoundPlayer);
        public static object SoundRecords_Lock = new object();
        public static bool ФлагОбновитьСписокЗвуковыхСообщений = false;
        public static byte РаботаПоНомеруДняНедели = 7;
        public static bool ФлагОбновитьСписокЖелезнодорожныхСообщенийПоДнюНедели = false;
        public static bool ФлагОбновитьСписокЖелезнодорожныхСообщенийВТаблице = false;

        //public TaskManagerService TaskManager = new TaskManagerService();
        public int ВремяЗадержкиМеждуСообщениями = 0;

        private const int ВремяЗадержкиВоспроизведенныхСобытий = 20;  //сек
        private readonly Timer _timerSoundHandler = new Timer(100);
        
        private bool ОбновлениеСписка = false;
        private int VisibleMode = 0;
        private int ТекущаяСекунда = 0;
        private string КлючВыбранныйМеню = "";
        private uint _tickCounter = 0;
        private ToolStripMenuItem[] СписокПолейПути;
        private bool _isbusyTimerSoundHandler;
        private bool _isBusytimer100Ms;
        private string currentPlayingTemplate = string.Empty;

        #endregion

        #region Properties

        public static BoardManager BoardManager { get; set; }
        public static SoundManager SoundManager { get; set; }

        public IDisposable DispouseCisClientIsConnectRx { get; set; }
        public IDisposable DispouseQueueChangeRx { get; set; }
        public IDisposable DispouseStaticChangeRx { get; set; }
        public IDisposable DispouseTemplateChangeRx { get; set; }
        public IDisposable DispouseApkDkVolgogradSheduleChangeRx { get; set; }
        public IDisposable DispouseApkDkVolgogradSheduleChangeConnectRx { get; set; }
        public IDisposable DispouseApkDkVolgogradSheduleDataExchangeSuccessChangeRx { get; set; }
        public GetSheduleAbstract GetSheduleAbstract { get; set; }                  //сервис получения данных от АпкДк Волгоград
        public GetSheduleAbstract DispatcherGetSheduleAbstract { get; set; }        //сервис получения данных от диспетчера
        public GetSheduleAbstract CisRegShAbstract { get; set; }                    //сервис получения данных от CIS (рег. расписание)
        public GetSheduleAbstract CisOperShAbstract { get; set; }                   //сервис получения данных от CIS (опер. расписание)
        public CisClient CisClient { get; }
        public Device SoundChanelManagment { get; }

        #endregion

        #region Ctor

        // Конструктор
        public MainWindowForm(CisClient cisClient,
                              IEnumerable<IBinding2PathBehavior> binding2PathBehaviors,
                              IEnumerable<IBinding2GeneralSchedule> binding2GeneralScheduleBehaviors,
                              IEnumerable<IBinding2ChangesBehavior> binding2ChangesBehaviors,
                              IEnumerable<IBinding2ChangesEventBehavior> binding2ChangesEventBehaviors,
                              IEnumerable<IBinding2GetData> binding2GetDataBehaviors,
                              Device soundChanelManagment)
        {
            if (myMainForm != null)
                return;

            myMainForm = this;

            InitializeComponent();

            tableLayoutPanel1.Visible = false;

            CisClient = cisClient;

            BoardManager = new BoardManager(binding2PathBehaviors, binding2GeneralScheduleBehaviors, binding2ChangesBehaviors, binding2ChangesEventBehaviors, binding2GetDataBehaviors);
            SoundManager = new SoundManager();
            
            SoundChanelManagment = soundChanelManagment;

            MainForm.Пауза.Click += new System.EventHandler(this.btnПауза_Click);
            MainForm.Включить.Click += new System.EventHandler(this.btnБлокировка_Click);
            MainForm.ОбновитьСписок.Click += new System.EventHandler(this.btnОбновитьСписок_Click);


            СписокПолейПути = new ToolStripMenuItem[] { путь0ToolStripMenuItem, путь1ToolStripMenuItem, путь2ToolStripMenuItem, путь3ToolStripMenuItem, путь4ToolStripMenuItem, путь5ToolStripMenuItem, путь6ToolStripMenuItem, путь7ToolStripMenuItem, путь8ToolStripMenuItem, путь9ToolStripMenuItem, путь10ToolStripMenuItem, путь11ToolStripMenuItem, путь12ToolStripMenuItem, путь13ToolStripMenuItem, путь14ToolStripMenuItem, путь15ToolStripMenuItem, путь16ToolStripMenuItem, путь17ToolStripMenuItem, путь18ToolStripMenuItem, путь19ToolStripMenuItem, путь20ToolStripMenuItem, путь21ToolStripMenuItem, путь22ToolStripMenuItem, путь23ToolStripMenuItem, путь24ToolStripMenuItem, путь25ToolStripMenuItem };


            //if (CisClient.IsConnect)
            //{
            //    MainForm.СвязьСЦис.Text = "ЦИС на связи";
            //    MainForm.СвязьСЦис.BackColor = Color.LightGreen;
            //}
            //else
            //{
            //    MainForm.СвязьСЦис.Text = "ЦИС НЕ на связи";
            //    MainForm.СвязьСЦис.BackColor = Color.Orange;
            //}

            //DispouseCisClientIsConnectRx = CisClient.IsConnectChange.Subscribe(isConnect =>
            //{
            //    if (isConnect)
            //    {
            //        MainForm.СвязьСЦис.Text = "ЦИС на связи";
            //        MainForm.СвязьСЦис.BackColor = Color.LightGreen;
            //    }
            //    else
            //    {
            //        MainForm.СвязьСЦис.Text = "ЦИС НЕ на связи";
            //        MainForm.СвязьСЦис.BackColor = Color.Orange;
            //    }
            //});

            //
            DispouseQueueChangeRx = QueueSound.QueueChangeRx.Subscribe(status =>
            {
                switch (status)
                {
                    case StatusPlaying.Start:
                        SoundManager.СобытиеНачалоПроигрыванияОчередиЗвуковыхСообщений();
                        break;

                    case StatusPlaying.Stop:
                        SoundManager.СобытиеКонецПроигрыванияОчередиЗвуковыхСообщений();
                        break;
                }
            });
            DispouseStaticChangeRx = QueueSound.StaticChangeRx.Subscribe(StaticChangeRxEventHandler);
            DispouseTemplateChangeRx = QueueSound.TemplateChangeRx.Subscribe(TemplateChangeRxEventHandler);


            //ЗАПУСК ОЧЕРЕДИ ЗВУКА
            QueueSound.StartQueue();

            _timerSoundHandler.Elapsed += _timerSoundHandler_Elapsed;
            _timerSoundHandler.Start();

            MainForm.Включить.BackColor = Color.LightGreen;
            if (Program.AuthenticationService?.CurrentUser != null)
                Program.ЗаписьЛога("Системное сообщение", "Программный комплекс включен", Program.AuthenticationService.CurrentUser);
        }

        #endregion

        #region Methods

        #region Trains

        // Формирование списка воспроизведения
        private void ОбновитьСписокЗвуковыхСообщений()
        {
            lock (SoundRecords_Lock)
            {
                SoundRecords.Clear();
            }
            SoundRecordsOld.Clear();

            СтатическиеЗвуковыеСообщения.Clear();

            СозданиеРасписанияЖдТранспорта();
            SoundManager.СозданиеСтатическихЗвуковыхФайлов();
        }

        /// <summary>
        /// Созданире обобщенного списка из основного и оперативного расписания
        /// </summary>
        private void СозданиеРасписанияЖдТранспорта()
        {
            int id = 1;

            //загрузим список изменений на текущий день.
            var currentDay = DateTime.Now.Date;
            SoundRecordChanges = Program.SoundRecordChangesDbRepository.List()
                                                                       .Where(p => (p.TimeStamp.Date == currentDay) ||
                                                                                  ((p.TimeStamp.Date == currentDay.AddDays(-1)) && (p.Rec.Время.Date == currentDay)) ||
                                                                                  ((p.NewRec.БитыНештатныхСитуаций & 0x0F) != 0x00 && (p.NewRec.БитыНештатныхСитуаций & 0x01) == 0x00 &&
                                                                                  p.NewRec.ОжидаемоеВремя.Date >= DateTime.Now.Date))
                                                                       .Select(Mapper.SoundRecordChangesDb2SoundRecordChanges).ToList();


            //DEBUG--------------
            //ParticirovanieNoSqlRepositoryService<SoundRecordChangesDb> particirovanieNoSqlService = new ParticirovanieNoSqlRepositoryService<SoundRecordChangesDb>();
            //var soundRecordChangesCurrentDay = particirovanieNoSqlService.GetRepositoryOnCurrentDay().List();
            //var soundRecordChangesYesterdayDay = particirovanieNoSqlService.GetRepositoryOnYesterdayDay().List().Where(p=> p.Rec.Время.Date == currentDay);
            //SoundRecordChanges= soundRecordChangesCurrentDay.Union(soundRecordChangesYesterdayDay).Select(Mapper.SoundRecordChangesDb2SoundRecordChanges).ToList();
            //DEBUG-------------


            //Добавим весь список Оперативного расписания
            СозданиеЗвуковыхФайловРасписанияЖдТранспорта(TrainTableOperative.TrainTableRecords, DateTime.Now, null, ref id);
            СозданиеЗвуковыхФайловРасписанияЖдТранспорта(TrainTableOperative.TrainTableRecords, DateTime.Now.AddDays(1), hour => (hour >= 0 && hour <= 11), ref id);

            /*var differences = TrainSheduleTable.TrainTableRecords.Where(l2 =>
                  !SoundRecords.Any(l1 => 
                                        ((l2.ScheduleId != 0 && l1.Value.IdTrain.ScheduleId == l2.ScheduleId) ||
                                        l1.Value.НомерПоезда == l2.Num &&
                                                 l1.Value.НомерПоезда2 == l2.Num2 &&
                                                 (l1.Value.Направление == l2.Direction ||
                                                 (l1.Value.СтанцияОтправления != ExchangeModel.NameRailwayStation.NameRu && l1.Value.СтанцияОтправления == l2.StationDepart) ||
                                                 (l1.Value.СтанцияНазначения != ExchangeModel.NameRailwayStation.NameRu && l1.Value.СтанцияНазначения == l2.StationArrival))))).ToList();// &&
*/



            //Добавим оставшиеся записи
            //СозданиеЗвуковыхФайловРасписанияЖдТранспорта(differences, DateTime.Now, null, ref id);                                         // на тек. сутки
            //СозданиеЗвуковыхФайловРасписанияЖдТранспорта(differences, DateTime.Now.AddDays(1), hour => (hour >= 0 && hour <= 11), ref id); // на след. сутки на 2 первых часа  
            СозданиеЗвуковыхФайловРасписанияЖдТранспорта(TrainSheduleTable.TrainTableRecords, DateTime.Now, null, ref id);                                         // на тек. сутки
            СозданиеЗвуковыхФайловРасписанияЖдТранспорта(TrainSheduleTable.TrainTableRecords, DateTime.Now.AddDays(1), hour => (hour >= 0 && hour <= 11), ref id); // на след. сутки на 2 первых часа                                                                      //(DateTime.ParseExact(l1.Key, "yy.MM.dd  HH:mm:ss", new DateTimeFormatInfo()).Date >= DateTime.Now.Date))

            //var differences = TrainSheduleTable.TrainTableRecords.Where(l2 =>
            //    !SoundRecords.Values.Any(l1 =>
            //        l1.IdTrain.ScheduleId == l2.ID
            //        //|| l1.IdTrain.ScheduleId == l2.ScheduleId
            //    )).ToList();


            //Корректировка записей по изменениям
            КорректировкаЗаписейПоИзменениям();
        }

        private void СозданиеЗвуковыхФайловРасписанияЖдТранспорта(IList<TrainTableRecord> trainTableRecords, DateTime день, Func<int, bool> ограничениеВремениПоЧасам, ref int id)
        {
            var pipelineService = new SchedulingPipelineService();
            for (var index = 0; index < trainTableRecords.Count; index++)
            {
                var config = trainTableRecords[index];

                if (config.Active == false && Program.Настройки.РазрешениеДобавленияЗаблокированныхПоездовВСписок == false)
                    continue;

                if (!pipelineService.CheckTrainActuality(ref config, день, ограничениеВремениПоЧасам, РаботаПоНомеруДняНедели))
                    continue;

                var newId = id++;
                SoundRecord record = Mapper.MapTrainTableRecord2SoundRecord(config, день, newId);


                //выдать список привязанных табло
                record.НазванияТабло = record.НомерПути != "0" ? BoardManager.Binding2PathBehaviors.Select(beh => beh.GetDevicesName4Path(record.НомерПути)).Where(str => str != null).ToArray() : null;
                record.СостояниеОтображения = TableRecordStatus.Выключена;


                //СБРОСИТЬ НОМЕР ПУТИ, НА ВРЕМЯ МЕНЬШЕ ТЕКУЩЕГО
                if (record.Время < DateTime.Now)
                {
                    record.НомерПути = string.Empty;
                    record.НомерПутиБезАвтосброса = string.Empty;
                }

                //Добавление созданной записи

                var newkey = pipelineService.GetUniqueKey(SoundRecords.Keys, record.Время);

                if (!string.IsNullOrEmpty(newkey) && !SoundRecords.Any(r => r.Value.Время.Date == record.Время.Date &&
                                                                            (r.Value.IdTrain.ScheduleId == record.IdTrain.ScheduleId ||
                                                                            r.Value.НомерПоезда == record.НомерПоезда &&
                                                                            r.Value.НомерПоезда2 == record.НомерПоезда2 &&
                                                                            //(r.Value.Направление == record.Направление ||
                                                                            ((r.Value.СтанцияОтправления != ExchangeModel.NameRailwayStation.NameRu && r.Value.СтанцияОтправления == record.СтанцияОтправления) ||
                                                                            (r.Value.СтанцияНазначения != ExchangeModel.NameRailwayStation.NameRu && r.Value.СтанцияНазначения == record.СтанцияНазначения)))))
                {
                    record.Время = DateTime.ParseExact(newkey, DATETIME_KEYFORMAT, new DateTimeFormatInfo());
                    lock (SoundRecords_Lock)
                    {
                        SoundRecords.Add(newkey, record);
                    }
                    SoundRecordsOld.Add(newkey, record);
                }

                MainWindowForm.ФлагОбновитьСписокЖелезнодорожныхСообщенийВТаблице = true;
            }
        }

        private void КорректировкаЗаписейПоИзменениям()
        {
            //фильтрация по последним изменениям. среди элементов с одинаковым Названием поезда и сутками движения, выбрать элементы с большей датой.
            var filtredOnMaxDate = SoundRecordChanges.GroupBy(gr => new { gr.ScheduleId, gr.Rec.НомерПоезда, gr.Rec.Время.Date })
                .Select(elem => elem.MaxBy(b => b.TimeStamp))
                .ToList();

            foreach (var src in filtredOnMaxDate)
            {
                if ((src.NewRec.БитыНештатныхСитуаций & 0x0F) != 0x00 && (src.NewRec.БитыНештатныхСитуаций & 0x01) == 0x00 && src.NewRec.ОжидаемоеВремя.Date >= DateTime.Now.Date)
                {
                    var key = src.NewRec.Время.ToString(DATETIME_KEYFORMAT);
                    var rec = SoundManager.ЗаполнениеСпискаНештатныхСитуаций(src.NewRec, key);
                    rec.ВыводЗвука = true;
                    if (!SoundRecords.ContainsKey(key))
                    {
                        SoundRecords[key] = rec;
                        SoundRecordsOld[key] = rec;
                    }
                }
            }

            for (int i = 0; i < SoundRecords.Count; i++)
            {
                KeyValuePair<string, SoundRecord> record;
                lock (SoundRecords_Lock)
                {
                    record = SoundRecords.ElementAt(i);
                }
                //var key = record.Key;
                var rec = record.Value;

                var change = filtredOnMaxDate.FirstOrDefault(f => ((f.ScheduleId == rec.IdTrain.ScheduleId) &&
                                                                  ((f.Rec.Время.Date == rec.Время.Date))));// || (f.NewRec.Время.Date == rec.Время.Date) || 
                                                                                                           //!SoundRecords.ContainsKey(new SchedulingPipelineService().GetUniqueKey(SoundRecords.Keys, f.NewRec.Время)) ||
                                                                                                           //!SoundRecords.ContainsKey(new SchedulingPipelineService().GetUniqueKey(SoundRecords.Keys, f.Rec.Время)))));
                if (change != null)
                {
                    var keyOld = rec.Время.ToString(DATETIME_KEYFORMAT);

                    lock (SoundRecords_Lock)
                    {
                        SoundRecords.Remove(keyOld);
                    }
                    SoundRecordsOld.Remove(keyOld);

                    var keyNew = change.NewRec.Время.ToString(DATETIME_KEYFORMAT);
                    ПрименениеЗагруженныхИзменений(rec, change.NewRec, keyNew);
                    ФлагОбновитьСписокЖелезнодорожныхСообщенийВТаблице = true;
                }
            }
        }

        private void ПрименениеЗагруженныхИзменений(SoundRecord rec, SoundRecord newRec, string key)
        {
            //ПРИМЕНЕНИЕ ИЗМЕНЕНИЙ
            rec.Время = newRec.Время;
            rec.ВремяЗадержки = newRec.ВремяЗадержки;
            //rec.DelayTime = newRec.DelayTime;
            rec.ВремяОтправления = newRec.ВремяОтправления;
            rec.ВремяПрибытия = newRec.ВремяПрибытия;
            rec.ВремяСтоянки = newRec.ВремяСтоянки;
            rec.ВремяСледования = newRec.ВремяСледования;
            rec.ОжидаемоеВремя = newRec.ОжидаемоеВремя;
            rec.ActualArrivalTime = newRec.ActualArrivalTime;
            rec.ActualDepartureTime = newRec.ActualDepartureTime;
            rec.ФиксированноеВремяОтправления = newRec.ФиксированноеВремяОтправления;
            rec.ФиксированноеВремяПрибытия = newRec.ФиксированноеВремяПрибытия;
            rec.ФиксированноеВремяОтправления = newRec.ФиксированноеВремяОтправления;

            rec.Автомат = newRec.Автомат;
            rec.Активность = newRec.Активность;
            rec.БитыАктивностиПолей = newRec.БитыАктивностиПолей;
            rec.БитыНештатныхСитуаций = newRec.БитыНештатныхСитуаций;

            rec.Дополнение = newRec.Дополнение;
            rec.AdditionEng = newRec.AdditionEng;
            rec.ИменаФайлов = newRec.ИменаФайлов;  //???
            rec.ИспользоватьДополнение = newRec.ИспользоватьДополнение;//???
            rec.КоличествоПовторений = newRec.КоличествоПовторений;
            rec.НазванияТабло = newRec.НазванияТабло;
            rec.НомерПути = newRec.НомерПути;
            rec.НомерПутиБезАвтосброса = newRec.НомерПутиБезАвтосброса;
            rec.Описание = newRec.Описание;
            rec.ОписаниеСостоянияКарточки = newRec.ОписаниеСостоянияКарточки;
            rec.Примечание = newRec.Примечание;
            rec.NoteEng = newRec.NoteEng;
            rec.РазрешениеНаОтображениеПути = newRec.РазрешениеНаОтображениеПути;
            rec.НумерацияПоезда = newRec.НумерацияПоезда;
            rec.СтанцияНазначения = newRec.СтанцияНазначения;
            rec.СтанцияОтправления = newRec.СтанцияОтправления;
            rec.НазваниеПоезда = newRec.НазваниеПоезда;
            rec.СостояниеОтображения = newRec.СостояниеОтображения;
            rec.ТипСообщения = newRec.ТипСообщения;//???

            //rec.СписокНештатныхСообщений = newRec.СписокНештатныхСообщений;


            //Заполнение СписокНештатныхСообщений.
            if ((rec.БитыНештатныхСитуаций & 0x0F) != 0x00)
            {
                rec = SoundManager.ЗаполнениеСпискаНештатныхСитуаций(rec, null);
            }

            rec.AplyIdTrain();

            //СОХРАНЕНИЕ
            lock (SoundRecords_Lock)
            {
                SoundRecords[key] = rec;
            }
            SoundRecordsOld[key] = rec;
        }

        private SoundRecord ПрименитьИзмененияSoundRecord(SoundRecord данные, SoundRecord старыеДанные, string key, string keyOld, ListView listView)
        {
            //Найдем индекс элемента "item" в listView, по ключу 
            listView.InvokeIfNeeded(() =>
            {
                int item = 0;
                for (int i = 0; i < listView.Items.Count - 1; item++)
                {
                    if (listView.Items[item].SubItems[0].Text == keyOld)
                        break;
                }

                //примем изменения
                данные = ИзменениеДанныхВКарточке(старыеДанные, данные, key);
                if (DateTime.ParseExact(key, DATETIME_KEYFORMAT, new DateTimeFormatInfo()) != данные.Время)
                {
                    key = данные.Время.ToString(DATETIME_KEYFORMAT);
                    listView.Items[item].SubItems[0].Text = key;
                }

                switch (listView.Name)
                {
                    case "listView1":
                        if (listView.Items[item].SubItems[3].Text != данные.НазваниеПоезда)      //Изменение названия поезда.
                            listView.Items[item].SubItems[3].Text = данные.НазваниеПоезда;
                        if (listView.Items[item].SubItems[1].Text != данные.НомерПоезда)        //Изменение номера поезда
                            listView.Items[item].SubItems[1].Text = данные.НомерПоезда;
                        if (listView.Items[item].SubItems[7].Text != данные.Дополнение)         //Изменение ДОПОЛНЕНИЯ
                            listView.Items[item].SubItems[7].Text = данные.ИспользоватьДополнение != null && данные.ИспользоватьДополнение["звук"] ? данные.Дополнение : String.Empty;
                        break;

                    case "lVПрибытие":
                    case "lVОтправление":
                        if (listView.Items[item].SubItems[4].Text != данные.НазваниеПоезда)
                            listView.Items[item].SubItems[4].Text = данные.НазваниеПоезда;
                        if (listView.Items[item].SubItems[1].Text != данные.НомерПоезда)
                            listView.Items[item].SubItems[1].Text = данные.НомерПоезда;
                        if (listView.Items[item].SubItems[5].Text != данные.Дополнение)
                            listView.Items[item].SubItems[5].Text = данные.ИспользоватьДополнение != null && данные.ИспользоватьДополнение["звук"] ? данные.Дополнение : String.Empty;
                        break;

                    case "lVТранзит":
                        if (listView.Items[item].SubItems[5].Text != данные.НазваниеПоезда)
                            listView.Items[item].SubItems[5].Text = данные.НазваниеПоезда;
                        if (listView.Items[item].SubItems[1].Text != данные.НомерПоезда)
                            listView.Items[item].SubItems[1].Text = данные.НомерПоезда;
                        if (listView.Items[item].SubItems[6].Text != данные.Дополнение)
                            listView.Items[item].SubItems[6].Text = данные.ИспользоватьДополнение != null && данные.ИспользоватьДополнение["звук"] ? данные.Дополнение : String.Empty;
                        break;
                }

                //if (данные.БитыНештатныхСитуаций != старыеДанные.БитыНештатныхСитуаций)
                //{
                данные = SoundManager.ЗаполнениеСпискаНештатныхСитуаций(данные, key);
                //}

                //Обновить Время ПРИБ
                var actStr = "";
                if (((данные.БитыАктивностиПолей & 0x04) != 0x00) && (старыеДанные.ВремяПрибытия != данные.ВремяПрибытия))
                {
                    данные = SoundManager.ЗаполнениеСпискаНештатныхСитуаций(данные, key);
                    actStr = //DateTime.ParseExact(key, "yy.MM.dd  HH:mm:ss", new DateTimeFormatInfo()).Date < DateTime.Now.Date ? 
                             //данные.ВремяПрибытия.ToString("dd.MM.yyyy HH:mm") : 
                             данные.ВремяПрибытия.ToString("HH:mm");
                    switch (listView.Name)
                    {
                        case "listView1":
                            if (listView.Items[item].SubItems[4].Text != actStr)
                                listView.Items[item].SubItems[4].Text = actStr;
                            break;

                        case "lVПрибытие":
                        case "lVТранзит":
                            if (listView.Items[item].SubItems[3].Text != actStr)
                                listView.Items[item].SubItems[3].Text = actStr;
                            break;
                    }
                }

                //Обновить Время ОТПР
                if (((данные.БитыАктивностиПолей & 0x10) != 0x00) && (старыеДанные.ВремяОтправления != данные.ВремяОтправления))
                {
                    данные = SoundManager.ЗаполнениеСпискаНештатныхСитуаций(данные, key);
                    actStr = //DateTime.ParseExact(key, "yy.MM.dd  HH:mm:ss", new DateTimeFormatInfo()).Date < DateTime.Now.Date ?
                             //данные.ВремяОтправления.ToString("dd.MM.yyyy HH:mm") : 
                             данные.ВремяОтправления.ToString("HH:mm");
                    switch (listView.Name)
                    {
                        case "listView1":
                            if (listView.Items[item].SubItems[5].Text != actStr)
                                listView.Items[item].SubItems[5].Text = actStr;
                            break;

                        case "lVТранзит":
                            if (listView.Items[item].SubItems[4].Text != actStr)
                                listView.Items[item].SubItems[4].Text = actStr;
                            break;

                        case "lVОтправление":
                            if (listView.Items[item].SubItems[3].Text != actStr)
                                listView.Items[item].SubItems[3].Text = actStr;
                            break;
                    }
                }

                //Смена Режима Работы.
                if (старыеДанные.Автомат != данные.Автомат)
                {
                    MainForm.РежимРаботы.BackColor = Color.LightGray;
                    MainForm.РежимРаботы.Text = @"Пользовательский";
                }

                if (!SoundRecords.ContainsKey(keyOld))  // поменяли время приб. или отпр. т.е. изменили ключ записи. Т.е. удалили запись под старым ключем.
                {
                    ОбновитьСписокЗвуковыхСообщенийВТаблице(); //Перерисуем список на UI.
                }

                ОбновитьСостояниеЗаписейТаблицы();
            });

            return данные;
        }

        private SoundRecord ИзменениеДанныхВКарточке(SoundRecord старыеДанные, SoundRecord данные, string key)
        {
            данные.ТипСообщения = SoundRecordType.ДвижениеПоезда;

            if (данные.НомерПути != старыеДанные.НомерПути)
            {
                данные.НазванияТабло = (данные.НомерПути != "0" && !string.IsNullOrEmpty(данные.НомерПути)) ? BoardManager.Binding2PathBehaviors.Select(beh => beh.GetDevicesName4Path(данные.НомерПути)).Where(str => str != null).ToArray() : null;
            }

            //если Поменяли время--------------------------------------------------------
            if ((старыеДанные.ВремяПрибытия != данные.ВремяПрибытия) ||
                (старыеДанные.ВремяОтправления != данные.ВремяОтправления))
            {
                данные.Время = ((данные.БитыАктивностиПолей & 0x10) == 0x10 ||
                                (данные.БитыАктивностиПолей & 0x14) == 0x14) ? данные.ВремяОтправления : данные.ВремяПрибытия;

                var keyOld = старыеДанные.Время.ToString(DATETIME_KEYFORMAT);
                lock (SoundRecords_Lock)
                {
                    SoundRecords.Remove(keyOld);           //удалим старую запись
                }
                SoundRecordsOld.Remove(keyOld);

                var pipelineService = new SchedulingPipelineService();

                var newkey = pipelineService.GetUniqueKey(SoundRecords.Keys, данные.Время);

                if (!string.IsNullOrEmpty(newkey))
                {
                    данные.Время = DateTime.ParseExact(newkey, DATETIME_KEYFORMAT, new DateTimeFormatInfo());

                    lock (SoundRecords_Lock)
                    {
                        SoundRecords.Add(newkey, данные);   //Добавим запись под новым ключем
                    }
                    SoundRecordsOld.Add(newkey, старыеДанные);
                }
            }
            else
            {
                lock (SoundRecords_Lock)
                {
                    SoundRecords[key] = данные;
                }
            }

            string сообщениеОбИзменениях = "";
            if (старыеДанные.НазваниеПоезда != данные.НазваниеПоезда) сообщениеОбИзменениях += "Поезд: " + старыеДанные.НазваниеПоезда + " -> " + данные.НазваниеПоезда + "; ";
            if (старыеДанные.НомерПоезда != данные.НомерПоезда) сообщениеОбИзменениях += "№Поезда: " + старыеДанные.НомерПоезда + " -> " + данные.НомерПоезда + "; ";
            if (старыеДанные.НомерПути != данные.НомерПути) сообщениеОбИзменениях += "Путь: " + старыеДанные.НомерПути + " -> " + данные.НомерПути + "; ";
            if (старыеДанные.НумерацияПоезда != данные.НумерацияПоезда) сообщениеОбИзменениях += "Нум.вагонов: " + старыеДанные.НумерацияПоезда.ToString() + " -> " + данные.НумерацияПоезда.ToString() + "; ";
            if (старыеДанные.СменнаяНумерацияПоезда != данные.СменнаяНумерацияПоезда) сообщениеОбИзменениях += " сменная Нум.вагонов: " + старыеДанные.СменнаяНумерацияПоезда.ToString() + " -> " + данные.СменнаяНумерацияПоезда.ToString() + "; ";
            if (старыеДанные.СтанцияОтправления != данные.СтанцияОтправления) сообщениеОбИзменениях += "Ст.Отпр.: " + старыеДанные.СтанцияОтправления + " -> " + данные.СтанцияОтправления + "; ";
            if (старыеДанные.СтанцияНазначения != данные.СтанцияНазначения) сообщениеОбИзменениях += "Ст.Назн.: " + старыеДанные.СтанцияНазначения + " -> " + данные.СтанцияНазначения + "; ";
            if ((старыеДанные.БитыАктивностиПолей & 0x04) != 0x00) if (старыеДанные.ВремяПрибытия != данные.ВремяПрибытия) сообщениеОбИзменениях += "Прибытие: " + старыеДанные.ВремяПрибытия.ToString("HH:mm") + " -> " + данные.ВремяПрибытия.ToString("HH:mm") + "; ";
            if ((старыеДанные.БитыАктивностиПолей & 0x10) != 0x00) if (старыеДанные.ВремяОтправления != данные.ВремяОтправления) сообщениеОбИзменениях += "Отправление: " + старыеДанные.ВремяОтправления.ToString("HH:mm") + " -> " + данные.ВремяОтправления.ToString("HH:mm") + "; ";
            if (старыеДанные.Автомат != данные.Автомат) сообщениеОбИзменениях += "Режим работы измененн: " +
                    (старыеДанные.Автомат ? "Автомат" : "Ручное") + " -> " +
                    (данные.Автомат ? "Автомат" : "Ручное") + "; ";
            if (старыеДанные.ФиксированноеВремяПрибытия != данные.ФиксированноеВремяПрибытия) сообщениеОбИзменениях += "Фиксированное время ПРИБЫТИЯ измененно: " +
                    ((старыеДанные.ФиксированноеВремяПрибытия == null) ? "--:--" : старыеДанные.ФиксированноеВремяПрибытия.Value.ToString("HH:mm")) + " -> " +
                    ((данные.ФиксированноеВремяПрибытия == null) ? "--:--" : данные.ФиксированноеВремяПрибытия.Value.ToString("HH:mm")) + "; ";
            if (старыеДанные.ФиксированноеВремяОтправления != данные.ФиксированноеВремяОтправления) сообщениеОбИзменениях += "Фиксированное время ОТПРАВЛЕНИЯ измененно: " +
                    ((старыеДанные.ФиксированноеВремяОтправления == null) ? "--:--" : старыеДанные.ФиксированноеВремяОтправления.Value.ToString("HH:mm")) + " -> " +
                    ((данные.ФиксированноеВремяОтправления == null) ? "--:--" : данные.ФиксированноеВремяОтправления.Value.ToString("HH:mm")) + "; ";

            if (сообщениеОбИзменениях != "")
                Program.ЗаписьЛога("Действие оператора", "Изменение настроек поезда: " + старыеДанные.НомерПоезда + " " + старыеДанные.НазваниеПоезда + ": " + сообщениеОбИзменениях, Program.AuthenticationService.CurrentUser);

            return данные;
        }

        public void СохранениеИзмененийДанныхКарточкеБД(SoundRecord старыеДанные, SoundRecord данные, string источникИзменений = "Текущий пользователь")
        {
            var recChange = new SoundRecordChanges
            {
                ScheduleId = данные.IdTrain.ScheduleId,
                TimeStamp = DateTime.Now,
                Rec = старыеДанные,
                NewRec = данные,
                UserInfo = $"{Program.AuthenticationService.CurrentUser.Login}  ({Program.AuthenticationService.CurrentUser.Role})",
                CauseOfChange = источникИзменений
            };

            if (recChange == null)
            {
                Log.log.Warn($"Объект изменений не создан. Сохранение отменено");
                return;
            }

            SoundRecordChanges.Add(recChange);
            //var hh = Mapper.SoundRecordChanges2SoundRecordChangesDb(recChange);//DEBUG

            //Сохранить в БД
            Program.SoundRecordChangesDbRepository.Add(Mapper.SoundRecordChanges2SoundRecordChangesDb(recChange));

            //Отправить на устройства с привязкой "Binding2ChangesEvent"
            BoardManager.SendData4Binding2ChangesEvent(recChange);
        }

        private void HttpDispatcherSoundRecordChanges(SoundRecordChanges soundRecordChanges)
        {
            var данные = soundRecordChanges.NewRec;
            var старыеДанные = soundRecordChanges.Rec;
            string key = данные.Время.ToString(DATETIME_KEYFORMAT);
            string keyOld = старыеДанные.Время.ToString(DATETIME_KEYFORMAT);

            //DEBUG------------------------------------------------------
            //var str = $"N= {данные.НомерПоезда}  Путь= {данные.НомерПути}  Время отпр={данные.ВремяОтправления:g}   Время приб={данные.ВремяПрибытия:g}  Ст.Приб {данные.СтанцияНазначения}   Ст.Отпр {данные.СтанцияОтправления}  key = {key}  keyOld= {keyOld}";
            // Log.log.Trace("Применить данные" + str);
            //DEBUG-----------------------------------------------------

            данные = ПрименитьИзмененияSoundRecord(данные, старыеДанные, key, keyOld, listView1);
            if (!StructCompare.SoundRecordComparer(ref данные, ref старыеДанные))
            {
                СохранениеИзмененийДанныхКарточкеБД(старыеДанные, данные, "Удаленный диспетчер");
            }
        }

        // Отображение сформированного списка воспроизведения в таблицу
        private void ОбновитьСписокЗвуковыхСообщенийВТаблице()
        {
            ОбновлениеСписка = true;

            listView1.InvokeIfNeeded(() =>
            {
                listView1.Items.Clear();
                lVПрибытие.Items.Clear();
                lVТранзит.Items.Clear();
                lVОтправление.Items.Clear();

                for (int i = 0; i < SoundRecords.Count; i++)
                {
                    KeyValuePair<string, SoundRecord> Данные;
                    lock (SoundRecords_Lock)
                    {
                        Данные = SoundRecords.ElementAt(i);
                    }

                    string ВремяОтправления = "";
                    string ВремяПрибытия = "";
                    if ((Данные.Value.БитыАктивностиПолей & 0x04) != 0x00) ВремяПрибытия = Данные.Value.ВремяПрибытия.ToString("HH:mm");
                    if ((Данные.Value.БитыАктивностиПолей & 0x10) != 0x00) ВремяОтправления = Данные.Value.ВремяОтправления.ToString("HH:mm");

                    //var track = Данные.Value.Track?.ToString() ?? string.Empty;
                    ListViewItem lvi1 = new ListViewItem(new string[] {Данные.Value.Время.ToString(DATETIME_KEYFORMAT),
                                                                       Данные.Value.НомерПоезда.Replace(':', ' '),
                                                                       Данные.Value.НомерПути,
                                                                       Данные.Value.НазваниеПоезда,
                                                                       ВремяПрибытия,
                                                                       ВремяОтправления,
                                                                       Данные.Value.Примечание,
                                                                       Данные.Value.ИспользоватьДополнение != null && Данные.Value.ИспользоватьДополнение["звук"] ? Данные.Value.Дополнение : String.Empty});
                    lvi1.Tag = Данные.Value.ID;
                    lvi1.Checked = Данные.Value.Состояние != SoundRecordStatus.Выключена;
                    this.listView1.Items.Add(lvi1);

                    if ((Данные.Value.БитыАктивностиПолей & 0x14) == 0x04)
                    {
                        ListViewItem lvi2 = new ListViewItem(new string[] {Данные.Value.Время.ToString(DATETIME_KEYFORMAT),
                                                                       Данные.Value.НомерПоезда.Replace(':', ' '),
                                                                       Данные.Value.НомерПути,
                                                                       ВремяПрибытия,
                                                                       Данные.Value.НазваниеПоезда,
                                                                       Данные.Value.ИспользоватьДополнение != null && Данные.Value.ИспользоватьДополнение["звук"] ? Данные.Value.Дополнение : String.Empty});
                        lvi2.Tag = Данные.Value.ID;
                        lvi2.Checked = Данные.Value.Состояние != SoundRecordStatus.Выключена;
                        this.lVПрибытие.Items.Add(lvi2);
                    }

                    if ((Данные.Value.БитыАктивностиПолей & 0x14) == 0x14)
                    {
                        ListViewItem lvi3 = new ListViewItem(new string[] {Данные.Value.Время.ToString(DATETIME_KEYFORMAT),
                                                                       Данные.Value.НомерПоезда.Replace(':', ' '),
                                                                       Данные.Value.НомерПути,
                                                                       ВремяПрибытия,
                                                                       ВремяОтправления,
                                                                       Данные.Value.НазваниеПоезда,
                                                                       Данные.Value.ИспользоватьДополнение != null && Данные.Value.ИспользоватьДополнение["звук"] ? Данные.Value.Дополнение : String.Empty});
                        lvi3.Tag = Данные.Value.ID;
                        lvi3.Checked = Данные.Value.Состояние != SoundRecordStatus.Выключена;
                        this.lVТранзит.Items.Add(lvi3);
                    }

                    if ((Данные.Value.БитыАктивностиПолей & 0x14) == 0x10)
                    {
                        ListViewItem lvi4 = new ListViewItem(new string[] {Данные.Value.Время.ToString(DATETIME_KEYFORMAT),
                                                                       Данные.Value.НомерПоезда.Replace(':', ' '),
                                                                       Данные.Value.НомерПути,
                                                                       ВремяОтправления,
                                                                       Данные.Value.НазваниеПоезда,
                                                                       Данные.Value.Дополнение});
                        lvi4.Tag = Данные.Value.ID;
                        lvi4.Checked = Данные.Value.Состояние != SoundRecordStatus.Выключена;
                        this.lVОтправление.Items.Add(lvi4);
                    }
                }
            });

            ОбновлениеСписка = false;
        }

        // Раскрасить записи в соответствии с состоянием
        public void ОбновитьСостояниеЗаписейТаблицы()
        {
            this.InvokeIfNeeded(() =>
            {
                #region Обновление списков поездов
                ОбновлениеРаскраскиСписка(this.listView1);
                ОбновлениеРаскраскиСписка(this.lVПрибытие);
                ОбновлениеРаскраскиСписка(this.lVТранзит);
                ОбновлениеРаскраскиСписка(this.lVОтправление);
                #endregion

                #region Обновление списка окна статических звуковых сообщений
                for (int item = 0; item < this.lVСтатическиеСообщения.Items.Count; item++)
                {
                    string Key = this.lVСтатическиеСообщения.Items[item].SubItems[0].Text;

                    if (СтатическиеЗвуковыеСообщения.Keys.Contains(Key) == true)
                    {
                        СтатическоеСообщение Данные = СтатическиеЗвуковыеСообщения[Key];

                        if (Данные.Активность == false)
                        {
                            if (this.lVСтатическиеСообщения.Items[item].BackColor != Color.LightGray)
                                this.lVСтатическиеСообщения.Items[item].BackColor = Color.LightGray;
                        }
                        else
                        {
                            switch (Данные.СостояниеВоспроизведения)
                            {
                                default:
                                case SoundRecordStatus.Выключена:
                                case SoundRecordStatus.Воспроизведена:
                                    if (this.lVСтатическиеСообщения.Items[item].BackColor != Color.LightGray)
                                        this.lVСтатическиеСообщения.Items[item].BackColor = Color.LightGray;
                                    break;

                                case SoundRecordStatus.ОжиданиеВоспроизведения:
                                    if (this.lVСтатическиеСообщения.Items[item].BackColor != Color.LightGreen)
                                        this.lVСтатическиеСообщения.Items[item].BackColor = Color.LightGreen;
                                    break;

                                case SoundRecordStatus.ВоспроизведениеАвтомат:
                                    if (this.lVСтатическиеСообщения.Items[item].BackColor != Color.LightBlue)
                                        this.lVСтатическиеСообщения.Items[item].BackColor = Color.LightBlue;
                                    break;
                            }
                        }
                    }
                }
                #endregion
            });
        }

        public void RefreshMainList()
        {
            try
            {
                ОбновитьСписокЗвуковыхСообщений();
                ОбновитьСписокЗвуковыхСообщенийВТаблицеСтатическихСообщений();
                ОбновитьСостояниеЗаписейТаблицы();

                BoardManager.InitializeTrackBoards();
                BoardManager.SetDisplayStateAndMessageTypeForAllRecordsAndSend2TrackBoards();

                MainForm.РежимРаботы.BackColor = Color.LightGray;
                MainForm.РежимРаботы.Text = @"Пользовательский";
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }

        private void DisplayAutopilotButton(bool isChecked)
        {
            MainForm.AutoPilot.Visible = isChecked;
            MainForm.AutoPilot.Enabled = isChecked;
            MainForm.SelectControlMode();
        }

        private void SetHeight(ListView listView, int height)
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, height);
            listView.SmallImageList = imgList;
        }

        private void ОбновлениеРаскраскиСписка(ListView lv)
        {
            for (int item = 0; item < lv.Items.Count; item++)
            {
                if (item <= SoundRecords.Count)
                {
                    try
                    {
                        string Key = lv.Items[item].SubItems[0].Text;

                        if (SoundRecords.Keys.Contains(Key))
                        {
                            SoundRecord данные;
                            lock (SoundRecords_Lock)
                            {
                                данные = SoundRecords[Key];
                            }

                            Color foreColor;
                            Font font;
                            if (данные.ТипПоезда == ТипПоезда.Пассажирский ||
                                данные.ТипПоезда == ТипПоезда.Скорый ||
                                данные.ТипПоезда == ТипПоезда.Фирменный ||
                                данные.ТипПоезда == ТипПоезда.Скоростной)
                            {
                                foreColor = Program.Настройки.НастройкиЦветов[17];
                                font = Program.Настройки.FontДальние;
                            }
                            else
                            {
                                foreColor = Program.Настройки.НастройкиЦветов[16];
                                font = Program.Настройки.FontПригород;
                            }

                            if (font == null)
                                font = lv.Items[item].Font;

                            switch (данные.СостояниеКарточки)
                            {
                                default:
                                case 0: // Выключен или не актуален
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[0] : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[0] : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[1])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[1];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.25) ||
                                        (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;

                                case 1: // Отсутствую шаблоны оповещения
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[2] : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[2] : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[3])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[3];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.25) || (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;

                                case 2: // Время не подошло (за 30 минут)
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[4] : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[4] : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[5])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[5];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.25) || (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;

                                case 3: // Не установлен путь
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[6] : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[6] : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[7])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[7];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.25) || (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;

                                case 4: // Не полностью включены все галочки
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[8] : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[8] : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[9])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[9];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.25) || (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;

                                case 5: // Полностью включены все галочки
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[10] : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[10] : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[11])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[11];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.25) || (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;

                                case 6: // Нештатная ситуация "Отмена"
                                case 16: // Нештатная ситуация "Задержка приб"
                                case 26: // Нештатная ситуация "Задержка отпр"
                                case 36: // Нештатная ситуация "Отпр по готов"
                                case 46: // Нештатная ситуация "Задержка посадки"
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[12] : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[12] : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[13])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[13];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.25) || (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;

                                case 7: // Ручной режим за 30 мин до самого ранего события или если не выставленн ПУТЬ
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[14] : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Program.Настройки.НастройкиЦветов[14] : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[15])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[15];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.25) || (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;

                                case 8: // Ручной режим
                                    if (lv.Items[item].ForeColor != ((foreColor == Color.Black) ? Color.White : foreColor))
                                        lv.Items[item].ForeColor = ((foreColor == Color.Black) ? Color.White : foreColor);
                                    if (lv.Items[item].BackColor != Program.Настройки.НастройкиЦветов[15])
                                        lv.Items[item].BackColor = Program.Настройки.НастройкиЦветов[15];
                                    if ((Math.Abs(lv.Items[item].Font.Size - font.Size) > 0.2) || (font.Name != lv.Items[item].Font.Name))
                                    {
                                        lv.Items[item].Font = font;
                                        SetHeight(listView1, (int)(font.Size * 2));
                                    }
                                    break;
                            }

                            //Обновить номер пути (текущий номер / предыдущий, до автосброса)
                            var номерПути = (данные.НомерПути != данные.НомерПутиБезАвтосброса) ?
                                             $"{данные.НомерПути} ({данные.НомерПутиБезАвтосброса})" :
                                             данные.НомерПути;
                            //var track = данные.Track?.ToString() ?? string.Empty;
                            if (lv.Items[item].SubItems[2].Text != номерПути)
                            {
                                lv.Items[item].SubItems[2].Text = номерПути;
                            }

                            if (lv.Name == "listView1")
                            {
                                string нумерацияПоезда = String.Empty;
                                switch (данные.НумерацияПоезда)
                                {
                                    case 1:
                                        нумерацияПоезда = "Нумерация поезда с ГОЛОВЫ состава";
                                        break;

                                    case 2:
                                        нумерацияПоезда = "Нумерация поезда с ХВОСТА состава";
                                        break;
                                }


                                if (lv.Items[item].SubItems[6].Text != данные.Примечание + нумерацияПоезда)
                                    lv.Items[item].SubItems[6].Text = данные.Примечание + нумерацияПоезда;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        
        private void ChangeBlock(bool isEnabled)
        {
            foreach (var tsm in СписокПолейПути)
            {
                tsm.Enabled = isEnabled;
            }
        }

        #endregion

        #region Sound
        #region Prepare

        private void ОбновитьСписокЗвуковыхСообщенийВТаблицеСтатическихСообщений()
        {
            ОбновлениеСписка = true;

            int НомерСтроки = 0;
            foreach (var Данные in СтатическиеЗвуковыеСообщения)
            {
                if (НомерСтроки >= lVСтатическиеСообщения.Items.Count)
                {
                    ListViewItem lvi1 = new ListViewItem(new string[] {Данные.Value.Время.ToString(DATETIME_KEYFORMAT),
                                                                       Данные.Value.НазваниеКомпозиции });
                    lvi1.Tag = НомерСтроки;
                    lvi1.Checked = Данные.Value.Активность;
                    lVСтатическиеСообщения.Items.Add(lvi1);
                }
                else
                {
                    if (lVСтатическиеСообщения.Items[НомерСтроки].SubItems[0].Text != Данные.Value.Время.ToString(DATETIME_KEYFORMAT))
                        lVСтатическиеСообщения.Items[НомерСтроки].SubItems[0].Text = Данные.Value.Время.ToString(DATETIME_KEYFORMAT);
                    if (lVСтатическиеСообщения.Items[НомерСтроки].SubItems[1].Text != Данные.Value.НазваниеКомпозиции)
                        lVСтатическиеСообщения.Items[НомерСтроки].SubItems[1].Text = Данные.Value.НазваниеКомпозиции;
                }

                НомерСтроки++;
            }

            while (НомерСтроки < lVСтатическиеСообщения.Items.Count)
                lVСтатическиеСообщения.Items.RemoveAt(НомерСтроки);

            ОбновлениеСписка = false;
        }
        
        public void RefreshTableStateAndDisplaySubtitles()
        {
            this.InvokeIfNeeded(() =>
            {
                lVСобытия_ОбновитьСостояниеТаблицы();
                ОтобразитьСубтитры();
            });
        }

        public void lVСобытия_ОбновитьСостояниеТаблицы()
        {
            int НомерСтроки = 0;
            foreach (var taskSound in SoundManager.TaskManager.Tasks)
            {
                if (НомерСтроки >= lVСобытия.Items.Count)
                {
                    ListViewItem lvi1 = new ListViewItem(new string[] { taskSound.Key, taskSound.Value.Описание });
                    switch (taskSound.Value.СостояниеСтроки)
                    {
                        case 0: lvi1.BackColor = Color.LightGray; break;
                        case 1: lvi1.BackColor = Color.White; break;
                        case 2: lvi1.BackColor = Color.LightGreen; break;
                        case 3: lvi1.BackColor = Color.Orange; break;
                        case 4: lvi1.BackColor = Color.CadetBlue; break;
                    }
                    lVСобытия.Items.Add(lvi1);
                }
                else
                {
                    if (lVСобытия.Items[НомерСтроки].SubItems[0].Text != taskSound.Key)
                        lVСобытия.Items[НомерСтроки].SubItems[0].Text = taskSound.Key;

                    if (lVСобытия.Items[НомерСтроки].SubItems[1].Text != taskSound.Value.Описание)
                        lVСобытия.Items[НомерСтроки].SubItems[1].Text = taskSound.Value.Описание;

                    switch (taskSound.Value.СостояниеСтроки)
                    {
                        case 0: if (lVСобытия.Items[НомерСтроки].BackColor != Color.LightGray) lVСобытия.Items[НомерСтроки].BackColor = Color.LightGray; break;
                        case 1: if (lVСобытия.Items[НомерСтроки].BackColor != Color.White) lVСобытия.Items[НомерСтроки].BackColor = Color.White; break;
                        case 2: if (lVСобытия.Items[НомерСтроки].BackColor != Color.LightGreen) lVСобытия.Items[НомерСтроки].BackColor = Color.LightGreen; break;
                        case 3: if (lVСобытия.Items[НомерСтроки].BackColor != Color.Orange) lVСобытия.Items[НомерСтроки].BackColor = Color.Orange; break;
                        case 4: if (lVСобытия.Items[НомерСтроки].BackColor != Color.CadetBlue) lVСобытия.Items[НомерСтроки].BackColor = Color.CadetBlue; break;
                    }
                }

                НомерСтроки++;
            }

            while (НомерСтроки < lVСобытия.Items.Count)
                lVСобытия.Items.RemoveAt(НомерСтроки);
        }

        public void ОтобразитьСубтитры()
        {
            var subtaitles = SoundManager.TaskManager.GetElements.FirstOrDefault(ev => ev.СостояниеСтроки == 4);
            if (subtaitles != null && subtaitles.СостояниеСтроки == 4)
            {
                if (subtaitles.НомерСписка == 1) //статические звуковые сообщения
                {
                    if (СтатическиеЗвуковыеСообщения.Keys.Contains(subtaitles.Ключ))
                    {
                        currentPlayingTemplate = subtaitles.ШаблонИлиСообщение;
                        rtb_subtaitles.Text = currentPlayingTemplate;
                    }
                }
                else
                if (subtaitles.НомерСписка == 0) //динамические звуковые сообщения
                {
                    if (subtaitles.ШаблонИлиСообщение != currentPlayingTemplate)
                    {
                        currentPlayingTemplate = subtaitles.ШаблонИлиСообщение;

                        SoundRecord record;
                        lock (SoundRecords_Lock)
                        {
                            record = SoundRecords[subtaitles.Ключ];
                        }

                        var card = new КарточкаДвиженияПоезда(record, subtaitles.Ключ);
                        СостояниеФормируемогоСообщенияИШаблон? сообшение = null;
                        card.ОтобразитьШаблонОповещенияНаRichTb(currentPlayingTemplate, ref сообшение, rtb_subtaitles);
                    }
                }
            }
            else
            {
                rtb_subtaitles.Text = string.Empty;
                currentPlayingTemplate = string.Empty;
            }
        }

        #endregion
        
        #endregion
        
        #endregion

        #region Events

        // Обновление списка вопроизведения сообщений при нажатии кнопки на панели
        public void btnОбновитьСписок_Click(object sender, EventArgs e)
        {
            RefreshMainList();
        }


        /// <summary>
        /// Загрузка формы
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            //ИНИЦИАЛИЗАЦИЯ УСТРОЙСТВ ПОЛУЧЕНИЯ ДАННЫХ--------------------------
            chbox_apkDk.Visible = false;
            chbox_DispatcherControl.Visible = false;
            chbox_CisRegShControl.Visible = false;
            chbox_CisOperShControl.Visible = false;
            foreach (var beh in BoardManager.Binding2GetDataBehaviors)
            {
                //ВКЛ/ОТКЛ элементы UI данного девайса
                switch (beh.GetDeviceName)
                {
                    case "HttpApkDkVolgograd":
                        chbox_apkDk.Visible = true;
                        chbox_apkDk.Checked = Program.Настройки.GetDataApkDkStart;

                        GetSheduleAbstract = new GetSheduleApkDk(beh.BaseGetDataBehavior, SoundRecords);
                        GetSheduleAbstract.SubscribeAndStart(chbox_apkDk);
                        GetSheduleAbstract.Enable = chbox_apkDk.Checked;
                        break;

                    case "HttpDispatcher":
                        chbox_DispatcherControl.Visible = true;
                        chbox_DispatcherControl.Checked = Program.Настройки.GetDataDispatcherControlStart;

                        DispatcherGetSheduleAbstract = new GetSheduleDispatcherControl(beh.BaseGetDataBehavior, SoundRecords);
                        DispatcherGetSheduleAbstract.SoundRecordChangesRx.Subscribe(HttpDispatcherSoundRecordChanges);
                        DispatcherGetSheduleAbstract.SubscribeAndStart(chbox_DispatcherControl);
                        DispatcherGetSheduleAbstract.Enable = chbox_DispatcherControl.Checked;
                        DisplayAutopilotButton(chbox_DispatcherControl.Checked);
                        break;

                    case "HttpCisRegSh":
                        chbox_CisRegShControl.Visible = true;
                        chbox_CisRegShControl.Checked = Program.Настройки.GetDataCisRegShStart;

                        CisRegShAbstract = new GetCisRegSh(beh.BaseGetDataBehavior, SoundRecords);
                        CisRegShAbstract.SubscribeAndStart(chbox_CisRegShControl);
                        CisRegShAbstract.Enable = chbox_CisRegShControl.Checked;
                        break;

                    case "HttpCisOperSh":
                        chbox_CisOperShControl.Visible = true;
                        chbox_CisOperShControl.Checked = Program.Настройки.GetDataCisOperShStart;

                        CisOperShAbstract = new GetCisOperSh(beh.BaseGetDataBehavior, SoundRecords);
                        // CisOperShAbstract.SoundRecordChangesRx.Subscribe(HttpDispatcherSoundRecordChanges);
                        CisOperShAbstract.SubscribeAndStart(chbox_CisOperShControl);
                        CisOperShAbstract.Enable = chbox_CisOperShControl.Checked;
                        break;

                    case "HttpCisUsersDb":
                        chbox_CisRegShControl.Visible = true;
                        chbox_CisRegShControl.Checked = Program.Настройки.GetDataCisRegShStart;

                        CisRegShAbstract = new GetCisUsersDb(beh.BaseGetDataBehavior, SoundRecords);
                        CisRegShAbstract.SubscribeAndStart(chbox_CisRegShControl);
                        CisRegShAbstract.Enable = chbox_CisRegShControl.Checked;
                        break;

                    case "HttpCisCarNavigation":
                        chbox_CisRegShControl.Visible = true;
                        chbox_CisRegShControl.Checked = Program.Настройки.GetDataCisRegShStart;

                        CisRegShAbstract = new GetCisCarNavigation(beh.BaseGetDataBehavior, SoundRecords);
                        CisRegShAbstract.SubscribeAndStart(chbox_CisRegShControl);
                        CisRegShAbstract.Enable = chbox_CisRegShControl.Checked;
                        break;
                }

                //инициализируем таблицу, для прохождения условия в функции отправки "AddOneTimeSendData".
                var uit = new UniversalInputType
                {
                    TableData = new List<UniversalInputType> { new UniversalInputType() }
                };
                beh.SendMessage(uit);
            }
            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            DispouseCisClientIsConnectRx?.Dispose();
            DispouseQueueChangeRx?.Dispose();
            DispouseStaticChangeRx?.Dispose();

            DispouseApkDkVolgogradSheduleChangeRx?.Dispose();
            DispouseApkDkVolgogradSheduleChangeConnectRx?.Dispose();
            DispouseApkDkVolgogradSheduleDataExchangeSuccessChangeRx?.Dispose();

            GetSheduleAbstract?.Dispose();
            DispatcherGetSheduleAbstract?.Dispose();

            _timerSoundHandler.Close();
            _timerSoundHandler.Dispose();

            base.OnClosed(e);
        }


        private async void _timerSoundHandler_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isbusyTimerSoundHandler)
                return;

            _isbusyTimerSoundHandler = true;

            // var sw = new Stopwatch();
            //  sw.Start();
            await QueueSound.Invoke();

            //   sw.Stop();
            //    TimeSpan ts = sw.Elapsed;
            //    Debug.WriteLine(ts.ToString());

            _isbusyTimerSoundHandler = false;
        }

        // Обработка таймера 100 мс для воспроизведения звуковых сообщений
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_isBusytimer100Ms)
                return;

            _isBusytimer100Ms = true;

            try
            {
                SoundManager.ОбработкаЗвуковогоПотка();
                BoardManager.DefineAndSendInfo2Board();

                if (VisibleMode != MainForm.VisibleStyle)
                {
                    VisibleMode = MainForm.VisibleStyle;
                    if (VisibleMode == 0)
                    {
                        listView1.Visible = true;
                        tableLayoutPanel1.Visible = false;
                    }
                    else
                    {
                        listView1.Visible = false;
                        tableLayoutPanel1.Visible = true;
                    }
                }

                if (ФлагОбновитьСписокЗвуковыхСообщений == true)
                {
                    ФлагОбновитьСписокЗвуковыхСообщений = false;
                    ОбновитьСписокЗвуковыхСообщенийВТаблицеСтатическихСообщений();
                }

                if (ФлагОбновитьСписокЖелезнодорожныхСообщенийПоДнюНедели == true)
                {
                    ФлагОбновитьСписокЖелезнодорожныхСообщенийПоДнюНедели = false;
                    btnОбновитьСписок_Click(null, null);
                    ОбновитьСписокЗвуковыхСообщенийВТаблице();
                }

                if (ФлагОбновитьСписокЖелезнодорожныхСообщенийВТаблице == true)
                {
                    ФлагОбновитьСписокЖелезнодорожныхСообщенийВТаблице = false;
                    ОбновитьСписокЗвуковыхСообщенийВТаблице();
                }
            }
            catch (Exception ex)
            {
                Log.log.Fatal($"Ошибка в работе таймера воспроизведения звуковых сообщений: {ex}");
            }
            finally
            {
                _isBusytimer100Ms = false;
            }
            
        }

        private void chbox_apkDk_CheckedChanged(object sender, EventArgs e)
        {
            var chBox = sender as CheckBox;
            if (chBox != null)
            {
                if (GetSheduleAbstract != null)
                    GetSheduleAbstract.Enable = chbox_apkDk.Checked;

                Program.Настройки.GetDataApkDkStart = chbox_apkDk.Checked;
                ОкноНастроек.СохранитьНастройки();
            }
        }

        private void chbox_DispatcherControl_CheckedChanged(object sender, EventArgs e)
        {
            var chBox = sender as CheckBox;
            if (chBox != null)
            {
                var isChecked = chbox_DispatcherControl.Checked;
                if (DispatcherGetSheduleAbstract != null)
                    DispatcherGetSheduleAbstract.Enable = isChecked;

                DisplayAutopilotButton(isChecked);
                Program.Настройки.GetDataDispatcherControlStart = isChecked;
                ОкноНастроек.СохранитьНастройки();
            }
        }

        private void chbox_CisRegShControl_CheckedChanged(object sender, EventArgs e)
        {
            var chBox = sender as CheckBox;
            if (chBox != null)
            {
                if (CisRegShAbstract != null)
                    CisRegShAbstract.Enable = chbox_CisRegShControl.Checked;

                Program.Настройки.GetDataCisRegShStart = chbox_CisRegShControl.Checked;
                ОкноНастроек.СохранитьНастройки();
            }
        }

        private void chbox_CisOperShControl_CheckedChanged(object sender, EventArgs e)
        {
            var chBox = sender as CheckBox;
            if (chBox != null)
            {
                if (CisOperShAbstract != null)
                    CisOperShAbstract.Enable = chbox_CisOperShControl.Checked;

                Program.Настройки.GetDataCisOperShStart = chbox_CisOperShControl.Checked;
                ОкноНастроек.СохранитьНастройки();
            }
        }

        private void StaticChangeRxEventHandler(StaticChangeValue staticChangeValue)
        {
            switch (staticChangeValue.StatusPlaying)
            {
                case StatusPlaying.Start:
                    //Debug.WriteLine($"Статическое СТАРТ");//DEBUG
                    SoundManager.СобытиеНачалоПроигрыванияОчередиЗвуковыхСообщений();
                    break;

                case StatusPlaying.Stop:
                    //Debug.WriteLine($"Статическое СТОП");//DEBUG
                    SoundManager.СобытиеКонецПроигрыванияОчередиЗвуковыхСообщений();
                    break;
            }

            this.InvokeIfNeeded(() =>
            {
                for (int i = 0; i < СтатическиеЗвуковыеСообщения.Count(); i++)
                {
                    string Key = СтатическиеЗвуковыеСообщения.ElementAt(i).Key;
                    СтатическоеСообщение сообщение = СтатическиеЗвуковыеСообщения.ElementAt(i).Value;

                    if (сообщение.ID == staticChangeValue.SoundMessage.RootId)
                    {
                        switch (staticChangeValue.StatusPlaying)
                        {
                            case StatusPlaying.Start:
                                сообщение.СостояниеВоспроизведения = SoundRecordStatus.ВоспроизведениеАвтомат;
                                break;

                            case StatusPlaying.Stop:
                                сообщение.СостояниеВоспроизведения = SoundRecordStatus.Выключена;
                                break;
                        }
                        СтатическиеЗвуковыеСообщения[Key] = сообщение;
                    }
                }
            });
        }
        
        private void TemplateChangeRxEventHandler(TemplateChangeValue templateChangeValue)
        {
            //DEBUG QUEUE-----------------------------------------
            switch (templateChangeValue.StatusPlaying)
            {
                case StatusPlaying.Start:
                    //Debug.WriteLine($"ДИНАМИЧЕСКОЕ СТАРТ");//DEBUG
                    SoundManager.СобытиеНачалоПроигрыванияОчередиЗвуковыхСообщений();
                    break;

                case StatusPlaying.Stop:
                    //Debug.WriteLine($"ДИНАМИЧЕСКОЕ СТОП");//DEBUG
                    SoundManager.СобытиеКонецПроигрыванияОчередиЗвуковыхСообщений();
                    break;
            }

            this.InvokeIfNeeded(() =>
            {
                //ШАБЛОН технического сообщения
                if (templateChangeValue.SoundMessage.ТипСообщения == ТипСообщения.ДинамическоеТехническое)
                {
                    var soundRecordTech = TechnicalMessageForm.SoundRecords.FirstOrDefault(rec => rec.ID == templateChangeValue.Template.SoundRecordId);
                    if (soundRecordTech.ID > 0)
                    {
                        int index = TechnicalMessageForm.SoundRecords.IndexOf(soundRecordTech);
                        var template = soundRecordTech.СписокФормируемыхСообщений.FirstOrDefault(i => i.Id == templateChangeValue.Template.Id);
                        switch (templateChangeValue.StatusPlaying)
                        {
                            case StatusPlaying.Start:
                                template.СостояниеВоспроизведения = SoundRecordStatus.ВоспроизведениеРучное;
                                break;

                            case StatusPlaying.Stop:
                                template.СостояниеВоспроизведения = SoundRecordStatus.Выключена;
                                break;
                        }
                        soundRecordTech.СписокФормируемыхСообщений[0] = template;
                        TechnicalMessageForm.SoundRecords[index] = soundRecordTech;
                    }
                    return;
                }

                KeyValuePair<string, SoundRecord> soundRecord = default(KeyValuePair<string, SoundRecord>);
                for (int i = 0; i < SoundRecords.Count; i++)
                {
                    KeyValuePair<string, SoundRecord> rec;
                    lock (SoundRecords_Lock)
                    {
                        rec = SoundRecords.ElementAt(i);
                    }

                    if (rec.Value.ID == templateChangeValue.Template.SoundRecordId)
                    {
                        soundRecord = rec;
                        break;
                    }
                }

                //шаблон АВАРИЯ
                if (templateChangeValue.SoundMessage.ТипСообщения == ТипСообщения.ДинамическоеАварийное)
                {
                    for (int i = 0; i < soundRecord.Value.СписокНештатныхСообщений.Count; i++)
                    {
                        if (soundRecord.Value.СписокНештатныхСообщений[i].Id == templateChangeValue.Template.Id)
                        {
                            var template = soundRecord.Value.СписокНештатныхСообщений[i];
                            switch (templateChangeValue.StatusPlaying)
                            {
                                case StatusPlaying.Start:
                                    template.СостояниеВоспроизведения = SoundRecordStatus.ВоспроизведениеАвтомат;
                                    break;

                                case StatusPlaying.Stop:
                                    template.СостояниеВоспроизведения = SoundRecordStatus.Выключена;
                                    break;
                            }
                            soundRecord.Value.СписокНештатныхСообщений[i] = template;
                        }
                    }
                }
                //шаблон ДИНАМИКИ
                else
                {
                    for (int i = 0; i < soundRecord.Value.СписокФормируемыхСообщений.Count; i++)
                    {
                        if (soundRecord.Value.СписокФормируемыхСообщений[i].Id == templateChangeValue.Template.Id)
                        {
                            var template = soundRecord.Value.СписокФормируемыхСообщений[i];
                            switch (templateChangeValue.StatusPlaying)
                            {
                                case StatusPlaying.Start:
                                    template.СостояниеВоспроизведения = (template.СостояниеВоспроизведения == SoundRecordStatus.ДобавленВОчередьРучное) ? SoundRecordStatus.ВоспроизведениеРучное : SoundRecordStatus.ВоспроизведениеАвтомат;
                                    break;

                                case StatusPlaying.Stop:
                                    template.СостояниеВоспроизведения = SoundRecordStatus.Выключена;
                                    break;
                            }
                            soundRecord.Value.СписокФормируемыхСообщений[i] = template;
                        }
                    }
                }

                lock (SoundRecords_Lock)
                {
                    if (SoundRecords.ContainsKey(soundRecord.Key))
                        SoundRecords[soundRecord.Key] = soundRecord.Value;
                }
            });
        }
        
        // Обработка нажатия кнопки блокировки/разрешения работы
        private void btnБлокировка_Click(object sender, EventArgs e)
        {
            //проверка ДОСТУПА
            if (!Program.AuthenticationService.CheckRoleAcsess(new List<Role> { Role.Администратор, Role.Диктор, Role.Инженер }))
            {
                MessageBox.Show($@"Нет прав!!!   С вашей ролью ""{Program.AuthenticationService.CurrentUser.Role}"" нельзя совершать  это действие.");
                return;
            }

            SoundManager.РазрешениеРаботы = !SoundManager.РазрешениеРаботы;
            if (SoundManager.РазрешениеРаботы == true)
            {
                MainForm.Включить.Text = "ОТКЛЮЧИТЬ";
                MainForm.Включить.BackColor = Color.LightGreen;
                QueueSound.StartAndPlayedCurrentMessage();
                Program.ЗаписьЛога("Действие оператора", "Работа разрешена", Program.AuthenticationService.CurrentUser);
            }
            else
            {
                MainForm.Включить.Text = "ВКЛЮЧИТЬ";
                MainForm.Включить.BackColor = Color.Red;
                QueueSound.StopAndPlayedCurrentMessage();
                Program.ЗаписьЛога("Действие оператора", "Работа запрещена", Program.AuthenticationService.CurrentUser);
            }
        }
        
        // ВоспроизведениеАвтомат выбраной в таблице записи
        private void btnПауза_Click(object sender, EventArgs e)
        {
            //проверка ДОСТУПА
            if (!Program.AuthenticationService.CheckRoleAcsess(new List<Role> { Role.Администратор, Role.Диктор, Role.Инженер }))
            {
                MessageBox.Show($@"Нет прав!!!   С вашей ролью ""{Program.AuthenticationService.CurrentUser.Role}"" нельзя совершать  это действие.");
                return;
            }

            //SoundPlayerStatus status = Program.AutodictorModel.SoundPlayer.GetPlayerStatus();//PlayerDirectX.GetPlayerStatus();
            //switch (status)
            //{
            //    case SoundPlayerStatus.Playing:
            //        QueueSound.Erase();
            //        break;
            //}

            QueueSound.Erase();
        }
        
        private void listView6_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                ListView.SelectedIndexCollection sic = this.lVСтатическиеСообщения.SelectedIndices;

                foreach (int item in sic)
                {
                    if (item <= СтатическиеЗвуковыеСообщения.Count)
                    {
                        string Key = this.lVСтатическиеСообщения.Items[item].SubItems[0].Text;

                        if (СтатическиеЗвуковыеСообщения.Keys.Contains(Key) == true)
                        {
                            СтатическоеСообщение Данные = СтатическиеЗвуковыеСообщения[Key];

                            КарточкаСтатическогоЗвуковогоСообщения Карточка = new КарточкаСтатическогоЗвуковогоСообщения(Данные);
                            if (Карточка.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                Данные = Карточка.ПолучитьИзмененнуюКарточку();

                                string Key2 = Данные.Время.ToString(DATETIME_KEYFORMAT);
                                string[] SubKeys = Key.Split(':');
                                if (SubKeys[0].Length == 1)
                                    Key2 = "0" + Key2;

                                if (Key == Key2)
                                {
                                    СтатическиеЗвуковыеСообщения[Key] = Данные;
                                    this.lVСтатическиеСообщения.Items[item].SubItems[1].Text = Данные.НазваниеКомпозиции;
                                }
                                else
                                {
                                    СтатическиеЗвуковыеСообщения.Remove(Key);

                                    int ПопыткиВставитьСообщение = 5;
                                    while (ПопыткиВставитьСообщение-- > 0)
                                    {
                                        Key2 = Данные.Время.ToString(DATETIME_KEYFORMAT);
                                        SubKeys = Key2.Split(':');
                                        if (SubKeys[0].Length == 1)
                                            Key2 = "0" + Key2;

                                        if (СтатическиеЗвуковыеСообщения.ContainsKey(Key2))
                                        {
                                            Данные.Время = Данные.Время.AddSeconds(20);
                                            continue;
                                        }

                                        СтатическиеЗвуковыеСообщения.Add(Key2, Данные);
                                        break;
                                    }

                                    ОбновитьСписокЗвуковыхСообщенийВТаблицеСтатическихСообщений();
                                }
                            }

                            ОбновитьСостояниеЗаписейТаблицы();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log.Error($"Ошибка при попытке открыть статическое сообщение: {ex}");
            }
        }
        
        // Обработка двойного нажатия на сообщение (вызов формы сообщения)
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView listView = sender as ListView;
            try
            {
                ListView.SelectedIndexCollection sic = listView.SelectedIndices;
                foreach (int item in sic)
                {
                    if (item <= SoundRecords.Count)
                    {
                        string key = listView.Items[item].SubItems[0].Text;
                        string keyOld = key;

                        if (SoundRecords.Keys.Contains(key))
                        {
                            SoundRecord данные;
                            lock (SoundRecords_Lock)
                            {
                                данные = SoundRecords[key];
                            }

                            КарточкаДвиженияПоезда карточка = new КарточкаДвиженияПоезда(данные, key);
                            if (карточка.ShowDialog() == DialogResult.OK)
                            {
                                SoundRecord старыеДанные = данные;
                                данные = карточка.ПолучитьИзмененнуюКарточку();

                                данные = ПрименитьИзмененияSoundRecord(данные, старыеДанные, key, keyOld, listView);
                                if (!StructCompare.SoundRecordComparer(ref данные, ref старыеДанные))
                                {
                                    СохранениеИзмененийДанныхКарточкеБД(старыеДанные, данные);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }

        private void lVПрибытие_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.ContextMenuStrip != null)
                this.ContextMenuStrip = null;

            if (e.Button == MouseButtons.Right)
            {
                ListView list = sender as ListView;
                if ((list.Name == "lVПрибытие") || (list.Name == "lVТранзит") || (list.Name == "lVОтправление") || (list.Name == "listView1"))
                {
                    if (list.SelectedIndices.Count > 0)
                    {
                        this.ContextMenuStrip = this.contextMenuStrip1;
                        try
                        {
                            ListView.SelectedIndexCollection sic = list.SelectedIndices;

                            foreach (int item in sic)
                            {
                                if (item <= SoundRecords.Count)
                                {
                                    string Key = list.Items[item].SubItems[0].Text;

                                    if (SoundRecords.Keys.Contains(Key))
                                    {
                                        SoundRecord Данные;
                                        lock (SoundRecords_Lock)
                                        {
                                            Данные = SoundRecords[Key];
                                        }

                                        КлючВыбранныйМеню = Key;

                                        var paths = Program.TrackRepository.List().Select(p => p.Name).ToList();
                                        //var paths = Program.TrackRepository.List().ToList();
                                        for (int i = 0; i < СписокПолейПути.Length - 1; i++)
                                        {
                                            if (i < paths.Count)
                                            {
                                                СписокПолейПути[i + 1].Text = paths[i].ToString();
                                                СписокПолейПути[i + 1].Visible = true;
                                            }
                                            else
                                            {
                                                СписокПолейПути[i + 1].Visible = false;
                                            }
                                        }

                                        foreach (ToolStripMenuItem t in СписокПолейПути)
                                            t.Checked = false;

                                        //int номерПути = paths.IndexOf(paths.FirstOrDefault(t => t.Name == Данные.НомерПути)) + 1;
                                        int номерПути = paths.IndexOf(Данные.НомерПути) + 1;
                                        if (номерПути >= 1 && номерПути < СписокПолейПути.Length)
                                            СписокПолейПути[номерПути].Checked = true;
                                        else
                                            СписокПолейПути[0].Checked = true;


                                        ToolStripMenuItem[] СписокНумерацииВагонов = new ToolStripMenuItem[] { отсутсвуетToolStripMenuItem, сГоловыСоставаToolStripMenuItem, сХвостаСоставаToolStripMenuItem };
                                        for (int i = 0; i < СписокНумерацииВагонов.Length; i++)
                                            СписокНумерацииВагонов[i].Checked = false;

                                        if (Данные.НумерацияПоезда <= 2)
                                            СписокНумерацииВагонов[Данные.НумерацияПоезда].Checked = true;


                                        ToolStripMenuItem[] СписокКоличестваПовторов = new ToolStripMenuItem[] { null, повтор1ToolStripMenuItem, повтор2ToolStripMenuItem, повтор3ToolStripMenuItem };
                                        for (int i = 1; i < СписокКоличестваПовторов.Length; i++)
                                            СписокКоличестваПовторов[i].Checked = false;

                                        if (Данные.КоличествоПовторений >= 1 && Данные.КоличествоПовторений <= 3)
                                            СписокКоличестваПовторов[Данные.КоличествоПовторений].Checked = true;


                                        var вариантыОтображенияПути = Табло_отображениеПутиToolStripMenuItem.DropDownItems;
                                        for (int i = 0; i < вариантыОтображенияПути.Count; i++)
                                        {
                                            var menuItem = вариантыОтображенияПути[i] as ToolStripMenuItem;
                                            if (menuItem != null)
                                            {
                                                menuItem.Checked = (i == (int)Данные.РазрешениеНаОтображениеПути);
                                            }
                                        }



                                        шаблоныОповещенияToolStripMenuItem1.DropDownItems.Clear();
                                        for (int i = 0; i < Данные.СписокФормируемыхСообщений.Count(); i++)
                                        {
                                            var Сообщение = Данные.СписокФормируемыхСообщений[i];
                                            ToolStripMenuItem tsmi = new ToolStripMenuItem(Сообщение.НазваниеШаблона);
                                            tsmi.Size = new System.Drawing.Size(165, 22);
                                            tsmi.Name = "ШаблонОповещения" + i.ToString();
                                            tsmi.Checked = Сообщение.Активность;
                                            tsmi.Click += new System.EventHandler(this.путь1ToolStripMenuItem_Click);
                                            шаблоныОповещенияToolStripMenuItem1.DropDownItems.Add(tsmi);
                                        }

                                        bool isEnabled;
                                        switch (Program.MainWindowWorkMode)
                                        {
                                            case MainWindowWorkMode.OnlyDispatcher:
                                                isEnabled = false;
                                                break;
                                            default:
                                                isEnabled = true;
                                                break;
                                        }
                                        ChangeBlock(isEnabled);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                else if (list.Name == "lVСтатическиеСообщения")
                {
                    if (list.SelectedIndices.Count > 0)
                    {
                        this.ContextMenuStrip = this.contextMenuStrip2;
                        try
                        {
                            ListView.SelectedIndexCollection sic = this.lVСтатическиеСообщения.SelectedIndices;

                            foreach (int item in sic)
                            {
                                if (item <= СтатическиеЗвуковыеСообщения.Count)
                                {
                                    string Key = this.lVСтатическиеСообщения.Items[item].SubItems[0].Text;

                                    if (СтатическиеЗвуковыеСообщения.Keys.Contains(Key) == true)
                                    {
                                        СтатическоеСообщение Данные = СтатическиеЗвуковыеСообщения[Key];
                                        включитьToolStripMenuItem.Text = Данные.Активность == true ? "Отключить" : "Включить";
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                //                contextMenuStrip1.Items.Add(list.Name);
            }
        }

        private void listView5_Enter(object sender, EventArgs e)
        {
            if (this.ContextMenuStrip != null)
                this.ContextMenuStrip = null;
        }

        private void воспроизвестиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ListView.SelectedIndexCollection sic = this.lVСтатическиеСообщения.SelectedIndices;

                foreach (int item in sic)
                {
                    if (item <= СтатическиеЗвуковыеСообщения.Count)
                    {
                        string Key = this.lVСтатическиеСообщения.Items[item].SubItems[0].Text;

                        if (СтатическиеЗвуковыеСообщения.Keys.Contains(Key) == true)
                        {
                            СтатическоеСообщение Данные = СтатическиеЗвуковыеСообщения[Key];
                            foreach (var Sound in StaticSoundForm.StaticSoundRecords)
                            {
                                if (Sound.Name == Данные.НазваниеКомпозиции)
                                {
                                    Program.ЗаписьЛога("Действие оператора", "ВоспроизведениеАвтомат статического звукового сообщения: " + Sound.Name, Program.AuthenticationService.CurrentUser);
                                    var воспроизводимоеСообщение = new ВоспроизводимоеСообщение
                                    {
                                        ParentId = null,
                                        RootId = Данные.ID,
                                        ИмяВоспроизводимогоФайла = Sound.Name,
                                        ПриоритетГлавный = Priority.Low,
                                        Язык = NotificationLanguage.Ru,
                                        ОчередьШаблона = null
                                    };
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void включитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ListView.SelectedIndexCollection sic = this.lVСтатическиеСообщения.SelectedIndices;

                foreach (int item in sic)
                {
                    if (item <= СтатическиеЗвуковыеСообщения.Count)
                    {
                        string Key = this.lVСтатическиеСообщения.Items[item].SubItems[0].Text;

                        if (СтатическиеЗвуковыеСообщения.Keys.Contains(Key) == true)
                        {
                            СтатическоеСообщение Данные = СтатическиеЗвуковыеСообщения[Key];
                            Данные.Активность = !Данные.Активность;
                            Program.ЗаписьЛога("Действие оператора", (Данные.Активность ? "Включение " : "Отключение ") + "звукового сообщения: \"" + Данные.НазваниеКомпозиции + "\" (" + Данные.Время.ToString("HH:mm") + ")", Program.AuthenticationService.CurrentUser);
                            СтатическиеЗвуковыеСообщения[Key] = Данные;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void путь1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;

            try
            {
                if (SoundRecords.Keys.Contains(КлючВыбранныйМеню))
                {
                    SoundRecord данные;
                    lock (SoundRecords_Lock)
                    {
                        данные = SoundRecords[КлючВыбранныйМеню];
                    }
                    var paths = Program.TrackRepository.List().Select(p => p.Name).ToList();
                    //var paths = Program.TrackRepository.List().ToList();

                    for (int i = 0; i < СписокПолейПути.Length; i++)
                        if (СписокПолейПути[i].Name == tsmi.Name)
                        {
                            //var oldTrack = данные.Track;
                            string старыйНомерПути = данные.НомерПути;
                            //данные.Track = i == 0 ? null : paths[i - 1];
                            //данные.НомерПути = данные.Track?.Name ?? string.Empty;
                            данные.НомерПути = i != 0 ? paths[i - 1] : string.Empty;
                            if (старыйНомерПути != данные.НомерПути) Program.ЗаписьЛога("Действие оператора", "Изменение настроек поезда: " + данные.НомерПоезда + " " + данные.НазваниеПоезда + ": " + "Путь: " + старыйНомерПути + " -> " + данные.НомерПути + "; ", Program.AuthenticationService.CurrentUser);

                            данные.ТипСообщения = SoundRecordType.ДвижениеПоезда;
                            данные.НазванияТабло = данные.НомерПути != "0" ? BoardManager.Binding2PathBehaviors.Select(beh => beh.GetDevicesName4Path(данные.НомерПути)).Where(str => str != null).ToArray() : null;

                            данные.НомерПутиБезАвтосброса = данные.НомерПути;
                            lock (SoundRecords_Lock)
                            {
                                SoundRecords[КлючВыбранныйМеню] = данные;
                            }

                            var старыеДанные = данные;
                            //старыеДанные.Track = oldTrack;
                            старыеДанные.НомерПути = старыйНомерПути;
                            if (!StructCompare.SoundRecordComparer(ref данные, ref старыеДанные))
                            {
                                СохранениеИзмененийДанныхКарточкеБД(старыеДанные, данные);
                            }
                            return;
                        }


                    ToolStripMenuItem[] СписокНумерацииВагонов = new ToolStripMenuItem[] { отсутсвуетToolStripMenuItem, сГоловыСоставаToolStripMenuItem, сХвостаСоставаToolStripMenuItem };
                    string[] СтроковыйСписокНумерацииВагонов = new string[] { "отсутсвуетToolStripMenuItem", "сГоловыСоставаToolStripMenuItem", "сХвостаСоставаToolStripMenuItem" };
                    if (СтроковыйСписокНумерацииВагонов.Contains(tsmi.Name))
                        for (int i = 0; i < СтроковыйСписокНумерацииВагонов.Length; i++)
                            if (СтроковыйСписокНумерацииВагонов[i] == tsmi.Name)
                            {
                                byte СтараяНумерацияПоезда = данные.НумерацияПоезда;
                                данные.НумерацияПоезда = (byte)i;
                                if (СтараяНумерацияПоезда != данные.НумерацияПоезда) Program.ЗаписьЛога("Действие оператора", "Изменение настроек поезда: " + данные.НомерПоезда + " " + данные.НазваниеПоезда + ": " + "Нум.вагонов: " + СтараяНумерацияПоезда.ToString() + " -> " + данные.НумерацияПоезда.ToString() + "; ", Program.AuthenticationService.CurrentUser);
                                lock (SoundRecords_Lock)
                                {
                                    SoundRecords[КлючВыбранныйМеню] = данные;
                                }

                                var старыеДанные = данные;
                                старыеДанные.НумерацияПоезда = СтараяНумерацияПоезда;
                                if (!StructCompare.SoundRecordComparer(ref данные, ref старыеДанные))
                                {
                                    СохранениеИзмененийДанныхКарточкеБД(старыеДанные, данные);
                                }
                                return;
                            }


                    ToolStripMenuItem[] СписокКоличестваПовторов = new ToolStripMenuItem[] { повтор1ToolStripMenuItem, повтор2ToolStripMenuItem, повтор3ToolStripMenuItem };
                    string[] СтроковыйСписокКоличестваПовторов = new string[] { "повтор1ToolStripMenuItem", "повтор2ToolStripMenuItem", "повтор3ToolStripMenuItem" };
                    if (СтроковыйСписокКоличестваПовторов.Contains(tsmi.Name))
                        for (int i = 0; i < СтроковыйСписокКоличестваПовторов.Length; i++)
                            if (СтроковыйСписокКоличестваПовторов[i] == tsmi.Name)
                            {
                                byte СтароеКоличествоПовторений = данные.КоличествоПовторений;
                                данные.КоличествоПовторений = (byte)(i + 1);
                                if (СтароеКоличествоПовторений != данные.КоличествоПовторений) Program.ЗаписьЛога("Действие оператора", "Изменение настроек поезда: " + данные.НомерПоезда + " " + данные.НазваниеПоезда + ": " + "Кол.повт.: " + СтароеКоличествоПовторений.ToString() + " -> " + данные.КоличествоПовторений.ToString() + "; ", Program.AuthenticationService.CurrentUser);
                                lock (SoundRecords_Lock)
                                {
                                    SoundRecords[КлючВыбранныйМеню] = данные;
                                }

                                var старыеДанные = данные;
                                старыеДанные.КоличествоПовторений = СтароеКоличествоПовторений;
                                if (!StructCompare.SoundRecordComparer(ref данные, ref старыеДанные))
                                {
                                    СохранениеИзмененийДанныхКарточкеБД(старыеДанные, данные);
                                }
                                return;
                            }


                    if (шаблоныОповещенияToolStripMenuItem1.DropDownItems.Contains(tsmi))
                    {
                        int ИндексШаблона = шаблоныОповещенияToolStripMenuItem1.DropDownItems.IndexOf(tsmi);
                        if (ИндексШаблона >= 0 && ИндексШаблона < 10 && ИндексШаблона < данные.СписокФормируемыхСообщений.Count)
                        {
                            var ФормируемоеСообщение = данные.СписокФормируемыхСообщений[ИндексШаблона];
                            ФормируемоеСообщение.Активность = !tsmi.Checked;
                            данные.СписокФормируемыхСообщений[ИндексШаблона] = ФормируемоеСообщение;
                            lock (SoundRecords_Lock)
                            {
                                SoundRecords[КлючВыбранныйМеню] = данные;
                            }
                            return;
                        }
                    }


                    if (Табло_отображениеПутиToolStripMenuItem.DropDownItems.Contains(tsmi))
                    {
                        int индексВарианта = Табло_отображениеПутиToolStripMenuItem.DropDownItems.IndexOf(tsmi);
                        if (индексВарианта >= 0)
                        {
                            данные.РазрешениеНаОтображениеПути = (PathPermissionType)индексВарианта;
                            lock (SoundRecords_Lock)
                            {
                                SoundRecords[КлючВыбранныйМеню] = данные;
                            }
                            return;
                        }
                    }


                    ОбновитьСостояниеЗаписейТаблицы();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void lVСобытия_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                string Key = lVСобытия.SelectedItems[0].SubItems[0].Text;
                if (SoundManager.TaskManager.Tasks.ContainsKey(Key))
                {
                    var данныеСтроки = SoundManager.TaskManager.Tasks[Key];
                    if (данныеСтроки.НомерСписка == 1)
                    {
                        Key = данныеСтроки.Ключ;
                        if (СтатическиеЗвуковыеСообщения.Keys.Contains(Key))
                        {
                            СтатическоеСообщение Данные = СтатическиеЗвуковыеСообщения[Key];
                            КарточкаСтатическогоЗвуковогоСообщения Карточка = new КарточкаСтатическогоЗвуковогоСообщения(Данные);
                            if (Карточка.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                Данные = Карточка.ПолучитьИзмененнуюКарточку();

                                string Key2 = Данные.Время.ToString(DATETIME_KEYFORMAT);
                                string[] SubKeys = Key.Split(':');
                                if (SubKeys[0].Length == 1)
                                    Key2 = "0" + Key2;

                                if (Key == Key2)
                                {
                                    СтатическиеЗвуковыеСообщения[Key] = Данные;
                                    for (int i = 0; i < lVСтатическиеСообщения.Items.Count; i++)
                                        if (lVСтатическиеСообщения.Items[i].SubItems[0].Text == Key)
                                            if (lVСтатическиеСообщения.Items[i].SubItems[1].Text != Данные.НазваниеКомпозиции)
                                            {
                                                lVСтатическиеСообщения.Items[i].SubItems[1].Text = Данные.НазваниеКомпозиции;
                                                break;
                                            }
                                }
                                else
                                {
                                    СтатическиеЗвуковыеСообщения.Remove(Key);

                                    int ПопыткиВставитьСообщение = 5;
                                    while (ПопыткиВставитьСообщение-- > 0)
                                    {
                                        Key2 = Данные.Время.ToString(DATETIME_KEYFORMAT);
                                        SubKeys = Key2.Split(':');
                                        if (SubKeys[0].Length == 1)
                                            Key2 = "0" + Key2;

                                        if (СтатическиеЗвуковыеСообщения.ContainsKey(Key2))
                                        {
                                            Данные.Время = Данные.Время.AddSeconds(20);
                                            continue;
                                        }

                                        СтатическиеЗвуковыеСообщения.Add(Key2, Данные);
                                        break;
                                    }

                                    ОбновитьСписокЗвуковыхСообщенийВТаблицеСтатическихСообщений();
                                }
                            }
                        }
                    }
                    else // Динамические сообщения
                    {
                        Key = данныеСтроки.Ключ;

                        if (SoundRecords.Keys.Contains(Key))
                        {
                            SoundRecord Данные;
                            lock (SoundRecords_Lock)
                            {
                                Данные = SoundRecords[Key];
                            }
                            КарточкаДвиженияПоезда Карточка = new КарточкаДвиженияПоезда(Данные, Key);
                            if (Карточка.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                SoundRecord СтарыеДанные = Данные;
                                Данные = Карточка.ПолучитьИзмененнуюКарточку();
                                ИзменениеДанныхВКарточке(СтарыеДанные, Данные, Key);
                                ОбновитьСостояниеЗаписейТаблицы();
                            }
                        }
                    }

                    ОбновитьСостояниеЗаписейТаблицы();
                }
            }
            catch (Exception ex)
            {
                Log.log.Error($"Ошибка при двойном щелчке на событии: {ex}");
            }
        }
        
        // Обработка закрытия основной формы
        private void MainWindowForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myMainForm == this)
                myMainForm = null;
        }
        
        #endregion
    }
}
