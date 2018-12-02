using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using AutodictorBL;
using Domain.Abstract;
using Domain.Concrete;
using Domain.Concrete.NoSqlReposutory;
using Domain.Entitys;
using Domain.Entitys.Authentication;
using Library.Logs;
using Library.Xml;
using MainExample.Services;
using Domain.Service;

namespace MainExample
{

    static class Program
    {
        #region Fields
        public static Настройки Настройки;

        static Mutex m_mutex;

        private static List<string> NumbersFolder = null;
        private static List<string> СписокСтатическихСообщений = null;
        private static List<string> СписокДинамическихСообщений = null;

        #endregion

        #region Properties

        public static List<string> FilesFolder { get; set; } = null;

        public static AuthenticationService AuthenticationService { get; set; } = new AuthenticationService();
        public static DirectionService DirectionService { get; set; }
        public static TrackService TrackService { get; set; }

        public static IRepository<Pathways> TrackRepository { get; set; }
        public static IRepository<Direction> DirectionRepository { get; set; }
        public static IRepository<SoundRecordChangesDb> SoundRecordChangesDbRepository { get; set; }
        public static IRepository<User> UsersDbRepository { get; set; }
        
        public static AutodictorModel AutodictorModel { get; set; }
        public static MainWindowWorkMode MainWindowWorkMode { get; set; }

        public static DateTime StartTime { get; } = DateTime.Now;

        public static List<string> НомераПоездов { get; } = new List<string>();
        public static string[] ТипыВремени { get; } = new string[] { "Прибытие", "Отправление" };
        public static List<string> ШаблоныОповещения { get; } = new List<string>();
        public static string[] ШаблонОповещенияОбОтменеПоезда { get; } = new string[] { "", "Отмена пассажирского поезда", "Отмена пригородного электропоезда", "Отмена фирменного поезда", "Отмена скорого поезда", "Отмена скоростного поезда", "Отмена ласточки", "Отмена РЭКСа" };
        public static string[] ШаблонОповещенияОЗадержкеПрибытияПоезда { get; } = new string[] { "", "Задержка прибытия пассажирского поезда", "Задержка прибытия пригородного электропоезда", "Задержка прибытия фирменного поезда", "Задержка прибытия скорого поезда", "Задержка прибытия скоростного поезда", "Задержка прибытия ласточки", "Задержка прибытия РЭКСа" };
        public static string[] ШаблонОповещенияОЗадержкеОтправленияПоезда { get; } = new string[] { "", "Задержка отправления пассажирского поезда", "Задержка отправления пригородного электропоезда", "Задержка отправления фирменного поезда", "Задержка отправления скорого поезда", "Задержка отправления скоростного поезда", "Задержка отправления ласточки", "Задержка отправления РЭКСа" };
        public static string[] ШаблонОповещенияООтправлениеПоГотовностиПоезда { get; } = new string[] { "", "Отправление по готовности пассажирского поезда", "Отправление по готовности пригородного электропоезда", "Отправление по готовности фирменного поезда", "Отправление по готовности скорого поезда", "Отправление по готовности скоростного поезда", "Отправление по готовности ласточки", "Отправление по готовности РЭКСа" };
        public static string[] LandingDelaySoundTemplate { get; } = new string[] { "", "Задержка посадки пассажирского поезда", "Задержка посадки пригородного электропоезда", "Задержка посадки фирменного поезда", "Задержка посадки скорого поезда", "Задержка посадки скоростного поезда", "Задержка посадки ласточки", "Задержка посадки РЭКСа" };


        #endregion

        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (InstanceExists())
                    return;

                ЗагрузкаНазванийПутей();
                ЗагрузкаНазванийНаправлений();
                ОкноНастроек.ЗагрузитьНастройки();
                
                SoundRecordChangesDbRepository = new RepositoryNoSql<SoundRecordChangesDb>(@"NoSqlDb\Main.db");
                
                UsersDbRepository = new RepositoryNoSql<User>(@"NoSqlDb\Users.db");

                AuthenticationService.UsersDbInitialize();//не дожидаемся окончания Task-а загрузки БД
                
                var dir = Directory.CreateDirectory("Wav\\Sounds\\");
                //var dir = new DirectoryInfo(Application.StartupPath + @"\Wav\Sounds\");
                FilesFolder = new List<string>();
                foreach (FileInfo file in dir.GetFiles("*.wav"))
                    FilesFolder.Add(Path.GetFileNameWithoutExtension(file.FullName));

                //dir = new DirectoryInfo(Application.StartupPath + @"\Wav\Numbers\");
                dir = Directory.CreateDirectory("Wav\\Numbers\\");
                NumbersFolder = new List<string>();
                foreach (FileInfo file in dir.GetFiles("*.wav"))
                    NumbersFolder.Add(Path.GetFileNameWithoutExtension(file.FullName));

                //dir = new DirectoryInfo(Application.StartupPath + @"\Wav\Static message\");
                dir = Directory.CreateDirectory("Wav\\Static message\\");
                СписокСтатическихСообщений = new List<string>();
                foreach (FileInfo file in dir.GetFiles("*.wav"))
                    СписокСтатическихСообщений.Add(Path.GetFileNameWithoutExtension(file.FullName));

                //dir = new DirectoryInfo(Application.StartupPath + @"\Wav\Dynamic message\");
                dir = Directory.CreateDirectory("Wav\\Dynamic message\\");
                СписокДинамическихСообщений = new List<string>();
                foreach (FileInfo file in dir.GetFiles("*.wav"))
                    СписокДинамическихСообщений.Add(Path.GetFileNameWithoutExtension(file.FullName));


                AutodictorModel = new AutodictorModel();
                AutodictorModel.LoadSetting(Настройки.ВыборУровняГромкости, GetFileName);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //ОБРАБОТКА НЕ ПЕРЕХВАЧЕННЫХ ИСКЛЮЧЕНИЙ
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                Application.Run(new MainForm());

                Dispose();
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }
        
        public static string GetFileName(string track, NotificationLanguage lang = NotificationLanguage.Ru)
        {
            string langPostfix = String.Empty;
            switch (lang)
            {
                case NotificationLanguage.Eng:
                    langPostfix = "_" + NotificationLanguage.Eng;
                    break;
            }
            track += langPostfix;
            string Path = Application.StartupPath + @"\";


            if (FilesFolder != null && FilesFolder.Contains(track))
                return Path + @"Wav\Sounds\" + track + ".wav";

            if (NumbersFolder != null && NumbersFolder.Contains(track))
                return Path + @"Wav\Numbers\" + track + ".wav";

            if (СписокСтатическихСообщений != null && СписокСтатическихСообщений.Contains(track))
                return Path + @"Wav\Static message\" + track + ".wav";

            if (СписокДинамическихСообщений != null && СписокДинамическихСообщений.Contains(track))
                return Path + @"Wav\Dynamic message\" + track + ".wav";

            foreach (var sound in StaticSoundForm.StaticSoundRecords)
                if (sound.Name == track)
                    return sound.Path;

            return "";
        }

        public static void ЗаписьЛога(string ТипСообщения, string Сообщение, User user, string format = null)
        {
            try
            {
                var path = "Logs\\";
                Directory.CreateDirectory(path);
                using (StreamWriter sw = new StreamWriter(File.Open(path + DateTime.Now.ToString("yy.MM.dd") + ".log", FileMode.Append)))
                {
                    sw.WriteLine(DateTime.Now.ToString(!string.IsNullOrWhiteSpace(format) ? format : "HH:mm:ss") + ": \t" + (user?.Login ?? "Неизвестный пользователь") + ": \t" + ТипСообщения + "\t" + Сообщение);
                    sw.Close();
                }
            }
            catch (Exception ex) { };
        }

        public static void CarNavigationLog(string Сообщение, User user)
        {
            try
            {
                var path = "Logs\\CarNavigation\\";
                Directory.CreateDirectory(path);
                using (StreamWriter sw = new StreamWriter(File.Open(path + DateTime.Now.ToString("yy.MM.dd") + ".log", FileMode.Append)))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": \t" + (user?.Login ?? "ЦИС") + ": \t" + "Повагонная навигация" + "\t" + Сообщение);
                    sw.Close();
                }
            }
            catch (Exception ex) { };
        }

        public static void DispatcherLog(string Сообщение, User user)
        {
            try
            {
                var path = "Logs\\Dispatcher\\";
                Directory.CreateDirectory(path);
                using (StreamWriter sw = new StreamWriter(File.Open(path + DateTime.Now.ToString("yy.MM.dd") + ".log", FileMode.Append)))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": \t" + (user?.Login ?? "Удаленный диспетчер") + ": \t" + "Диспетчерская" + "\t" + Сообщение);
                    sw.Close();
                }
            }
            catch (Exception ex) { };
        }

        private static bool InstanceExists()
        {
            bool createdNew;
            m_mutex = new Mutex(false, "AutodictorOneInstanceApplication", out createdNew);
            return (!createdNew);
        }

        private static void ЗагрузкаНазванийНаправлений()
        {
            try
            {
                var xmlFile = XmlWorker.LoadXmlFile("Stations.xml"); //все настройки в одном файле
                if (xmlFile == null)
                    throw new FileNotFoundException("Файл Stations.xml не найден или не соответствует формату xml. Откорректируйте файл и повторите попытку");

                DirectionRepository = new RepositoryXmlDirection(xmlFile, "Stations.xml");                 //хранилище XML
                DirectionService = new DirectionService(DirectionRepository);
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
                MessageBox.Show($"файл \"Stations.xml\" не загружен. Исключение: {ex.Message}");
                Application.Exit();
            }
        }

        private static void ЗагрузкаНазванийПутей()
        {
            try
            {
                var xmlFile = XmlWorker.LoadXmlFile("PathNames.xml"); //все настройки в одном файле
                if (xmlFile == null)
                    throw new FileNotFoundException("Файл PathNames.xml не найден или не соответствует формату xml. Откорректируйте файл и повторите попытку");

                TrackRepository = new RepositoryXmlPathways(xmlFile);                 //хранилище XML
                //TrackService = new TrackService("PathNames.xml");
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
                MessageBox.Show($"файл \"PathNames.xml\" не загружен. Исключение: {ex.Message}");
                Application.Exit();
            }
        }
        
        #endregion

        #region Events

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.log.Fatal($"Исключение из не UI потока {e.Exception}");
        }
        
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.log.Fatal($"Исключение основного UI потока {(e.ExceptionObject as Exception)}");
        }

        #endregion

        #region Dispouse

        private static void Dispose()
        {
            AutodictorModel?.Dispose();
        }

        #endregion
    }
}
