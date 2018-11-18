﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using CommunicationDevices.Model;
using System.Drawing;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using CommunicationDevices.Behavior.BindingBehavior;
using CommunicationDevices.Behavior.BindingBehavior.ToPath;
using CommunicationDevices.ClientWCF;
using CommunicationDevices.Quartz.Jobs;
using CommunicationDevices.Quartz.Shedules;
using CommunicationDevices.Verification;
using Domain.Entitys;
using Domain.Entitys.Authentication;
using Domain.Service;
using MainExample.Entites;
using MainExample.Extension;
using MainExample.Services;
using CommunicationDevices.DataProviders;
using Domain.Concrete;
using System.Threading.Tasks;

namespace MainExample
{
    public partial class MainForm : Form
    {
        public ExchangeModel ExchangeModel { get; set; }
        public IDisposable DispouseCisClientIsConnectRx { get; set; }
        public VerificationActivation VerificationActivationService { get; set; } = new VerificationActivation();

        public IDisposable DispouseActivationWarningInvokeRx { get; set; }

        public static int VisibleStyle = 0;

        public static MainForm mainForm = null;
        public static ToolStripButton Пауза = null;
        public static ToolStripButton Включить = null;
        public static ToolStripButton ОбновитьСписок = null;
        public static ToolStripButton РежимРаботы = null;
        public static ToolStripButton AutoPilot = null;

        private AuthenticationService autenServ = Program.AuthenticationService;
        private List<UniversalInputType> table = new List<UniversalInputType>();




        public MainForm()
        {
            InitializeComponent();

            StaticSoundForm.ЗагрузитьСписок();
            DynamicSoundForm.ЗагрузитьСписок();
            SoundConfiguration.ЗагрузитьСписок();
            TrainSheduleTable.SourceLoadMainListAsync().GetAwaiter();
            TrainSheduleTable.SourceLoadOperListAsync().GetAwaiter();

            // Player.PlayFile("");                          //TODO: ???? включить

            ExchangeModel = new ExchangeModel();

            if (mainForm == null)
                mainForm = this;

            Пауза = tSBПауза;

            Включить = tSBВключить;
            ОбновитьСписок = tSBОбновитьСписок;
            РежимРаботы = tSBРежимРаботы;
            AutoPilot = tsbAutoPilot;

            Включить.BackColor = Color.LightGreen;

            //QuartzVerificationActivation.Start(VerificationActivationService);
        }



        /// <summary>
        /// Проверка аутентификации.
        /// </summary>
        /// <param name="flagApplicationExit">ВЫХОД из приложения</param>
        private void CheckAuthentication(bool flagApplicationExit)
        {
            tSBAdmin.Visible = false;
            while (!autenServ?.IsAuthentication ?? false)
            {
                var autenForm = new AuthenticationForm();
                var result = autenForm.ShowDialog();
                Program.ЗаписьЛога("Системное сообщение", "Ожидается авторизация пользователя", autenServ?.OldUser);
                if (result == DialogResult.OK)
                {
                    if (autenServ?.IsAuthentication ?? false)
                    {
                        //ОТОБРАЗИТЬ ВОШЕДШЕГО ПОЛЬЗОВАТЕЛЯ
                        var user = autenServ.CurrentUser;
                        Program.ЗаписьЛога("Системное сообщение", "Авторизация успешна", user);
                        tSBLogOut.Text = user.Login;
                        if (ExchangeModel?.Binding2ChangesEvent?.Any() ?? false)
                            SendAuthenticationChanges(user, "Вход в систему");
                    }
                }
                else
                {        
                    if (flagApplicationExit)
                    {
                        Application.Exit();                  //ВЫХОД
                        Program.ЗаписьЛога("Системное сообщение", "Программа закрыта без авторизации", null);
                    }

                    //ПОЛЬЗОВАТЕЛЬ - Предыдущий пользователь
                    autenServ?.SetOldUser();
                    var user = autenServ?.CurrentUser;
                    if (user != null)
                    {
                        Program.ЗаписьЛога("Системное сообщение", "Смена пользователя отменена", user);
                        tSBLogOut.Text = user.Login;
                        //SendAuthenticationChanges(user, "Вход в систему");
                    }
                    break;
                }
            }

            //Отрисовать вход в админку
            switch (autenServ?.CurrentUser?.Role)
            {
                case Role.Администратор:
                    tSBAdmin.Visible = true;
                    break;
            }
        }

        private void SendAuthenticationChanges(User user, string causeOfChange)
        {
            if (table.Count < 1)
            {
                // Первичные изменения записываем в список изменений
                table.Add(new UniversalInputType
                {
                    ViewBag = new Dictionary<string, dynamic>
                    {
                        { "TimeStamp", Program.StartTime },
                        { "UserInfo", "" },
                        { "CauseOfChange", "Запуск программы" }
                    }
                });
            }
            else if (table.Count > 1)
            {
                table.RemoveAt(0);
            }

            // Пишем новые изменения на позицию (1)
            table.Add(new UniversalInputType
            {
                ViewBag = new Dictionary<string, dynamic>
                {
                    { "TimeStamp", DateTime.Now },
                    { "UserInfo", user?.Login ?? string.Empty },
                    { "CauseOfChange", causeOfChange }
                }
            });
            
            if (table.Count != 2)
            {
                Library.Logs.Log.log.Warn("Отправка аутентификации в ЦИС: неверное количество таблиц данных изменений");
            }
            var uit = new UniversalInputType { TableData = table };
            if (ExchangeModel.Binding2ChangesEvent.Any())
            {
                ExchangeModel.Binding2ChangesEvent.Last().SendMessage(uit);
            }
        }

        private void SendChangedMode(User user, string causeOfChange)
        {
            if (table.Count < 1)
            {
                // Первичные изменения записываем в список изменений
                table.Add(new UniversalInputType
                {
                    ViewBag = new Dictionary<string, dynamic>
                    {
                        { "TimeStamp", Program.StartTime },
                        { "UserInfo", "" },
                        { "CauseOfChange", "Запуск программы" }
                    }
                });
            }
            else if (table.Count > 1)
            {
                table.RemoveAt(0);
            }

            // Пишем новые изменения на позицию (1)
            table.Add(new UniversalInputType
            {
                ViewBag = new Dictionary<string, dynamic>
                {
                    { "TimeStamp", DateTime.Now },
                    { "UserInfo", user?.Login ?? "Неизвестный пользователь" },
                    { "CauseOfChange", causeOfChange ?? "Неизвестный тип управления" }
                }
            });

            if (table.Count != 2)
            {
                Library.Logs.Log.log.Warn("Отправка типа управления в диспетчерскую: неверное количество таблиц данных изменений");
            }
            var uit = new UniversalInputType { TableData = table };
            if (ExchangeModel.Binding2ChangesEvent.Any())
            {
                ExchangeModel.Binding2ChangesEvent.Last().SendMessage(uit);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckAuthentication(true); // переместил сюда, т.к. иначе данные о первом логине не отправляются по причине незагруженной модели обмена
                                       // это выключило возможность включения/отключения галки получения данных из ЦИС на нижней панели программы
                                       //ExchangeModel.StartCisClient();

            ExchangeModel.LoadSetting();

            SendAuthenticationChanges(autenServ?.CurrentUser, "Вход в систему");

            ExchangeModel.InitializeDeviceSoundChannelManagement();

            DispouseCisClientIsConnectRx = ExchangeModel.CisClient.IsConnectChange.Subscribe(isConnect =>
            {
                //TODO: вызывать через Invoke
                //if (isConnect)
                //{
                //    СвязьСЦис = tSLСостояниеСвязиСЦИС;
                //    СвязьСЦис.BackColor = Color.LightGreen;
                //    СвязьСЦис.Text = "ЦИС на связи";
                //}
                //else
                //{
                //    СвязьСЦис = tSLСостояниеСвязиСЦИС;
                //    СвязьСЦис .BackColor = Color.Orange;
                //    СвязьСЦис.Text = "ЦИС НЕ на связи";
                //}
            });

            DispouseActivationWarningInvokeRx = VerificationActivationService.WarningInvokeRx.Subscribe(verAct =>
            {
               this.InvokeIfNeeded(() =>
               {
                   if (BlockingForm.MyMainForm == null)
                   {
                       var blockingForm = new BlockingForm(verAct);
                       blockingForm.WindowState = FormWindowState.Normal;
                       blockingForm.ShowDialog();
                   }
               });
            });

            btnMainWindowShow_Click(null, EventArgs.Empty);
        }



        private void btnMainWindowShow_Click(object sender, EventArgs e)
        {
            if (MainWindowForm.myMainForm != null)
            {
                MainWindowForm.myMainForm.Show();
                MainWindowForm.myMainForm.WindowState = FormWindowState.Maximized;
            }
            else
            {
                MainWindowForm mainform = new MainWindowForm(ExchangeModel.CisClient,
                                                             ExchangeModel.Binding2PathBehaviors,
                                                             ExchangeModel.Binding2GeneralSchedules,
                                                             ExchangeModel.Binding2ChangesSchedules,
                                                             ExchangeModel.Binding2ChangesEvent,
                                                             ExchangeModel.Binding2GetData,
                                                             ExchangeModel.DeviceSoundChannelManagement)
                {
                    MdiParent = this,
                    WindowState = FormWindowState.Maximized
                };
                mainform.Show();
                mainform.btnОбновитьСписок_Click(null, EventArgs.Empty);
            }
        }



        //Расписание движения поездов
        private void listExample_Click(object sender, EventArgs e)
        {
            if (TrainTableGrid.MyMainForm != null)
            {
                TrainTableGrid.MyMainForm.Show();
                TrainTableGrid.MyMainForm.WindowState = FormWindowState.Normal;
            }
            else
            {
                TrainTableGrid form = new TrainTableGrid() { MdiParent = this };
                form.Show();
            }
        }



        private void validationExample_Click(object sender, EventArgs e)
        {
            if (SoundConfiguration.thisForm != null)
            {
                SoundConfiguration.thisForm.Show();
                SoundConfiguration.thisForm.WindowState = FormWindowState.Maximized;
            }
            else
            {
                SoundConfiguration soundConfiguration = new SoundConfiguration();
                soundConfiguration.MdiParent = this;
                soundConfiguration.Show();
            }
        }



        private void textBoxExample_Click(object sender, EventArgs e)
        {
        }



        private void dataSetExample_Click(object sender, EventArgs e)
        {
            if (StaticSoundForm.thisForm != null)
            {
                StaticSoundForm.thisForm.Show();
                StaticSoundForm.thisForm.WindowState = FormWindowState.Maximized;
            }
            else
            {
                StaticSoundForm form = new StaticSoundForm(Program.AutodictorModel.SoundPlayer);
                form.MdiParent = this;
                form.Show();
            }
        }
        
        private void arrayDataSourceExample_Click(object sender, EventArgs e)
        {
            if (DynamicSoundForm.thisForm != null)
            {
                DynamicSoundForm.thisForm.Show();
                DynamicSoundForm.thisForm.WindowState = FormWindowState.Maximized;
            }
            else
            {
                DynamicSoundForm form = new DynamicSoundForm(Program.AutodictorModel.SoundPlayer);
                form.MdiParent = this;
                form.Show();
            }
        }
        
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm form = new AboutForm(VerificationActivationService);
            form.ShowDialog();
        }
        
        private void просмотрСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = Application.StartupPath + @"\Manuals\Manual.pdf";
                Process.Start(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Ошибка открытия файла: ""{ex.Message}""  ""{ex.InnerException?.Message}""");
            }
        }
        
        private void OperativeShedules_Click(object sender, EventArgs e)
        {
            if (OperativeSheduleForm.MyOperativeSheduleForm != null)                                     //Открытие окна повторно, при открытом первом экземпляре.
            {
                OperativeSheduleForm.MyOperativeSheduleForm.Show();
                OperativeSheduleForm.MyOperativeSheduleForm.WindowState = FormWindowState.Normal;
            }
            else                                                                                         //Открытие окна
            {
                OperativeSheduleForm operativeSheduleForm = new OperativeSheduleForm(ExchangeModel.CisClient);
                operativeSheduleForm.MdiParent = this;
                operativeSheduleForm.Show();
            }
        }
        
        private void RegulatoryShedules_Click(object sender, EventArgs e)
        {
            if (RegulatorySheduleForm.MyRegulatorySheduleForm != null)                                     
            {
                RegulatorySheduleForm.MyRegulatorySheduleForm.Show();
                RegulatorySheduleForm.MyRegulatorySheduleForm.WindowState = FormWindowState.Normal;
            }
            else                                                                                         
            {
                RegulatorySheduleForm regulatorySheduleForm = new RegulatorySheduleForm(ExchangeModel.CisClient);
                regulatorySheduleForm.MdiParent = this;
                regulatorySheduleForm.Show();
            }
        }
        
        private void Boards_Click(object sender, EventArgs e)
        {
            if (BoardForm.MyBoardForm != null)                                     //Открытие окна повторно, при открытом первом экземпляре.
            {
                BoardForm.MyBoardForm.Show();
                BoardForm.MyBoardForm.WindowState = FormWindowState.Normal;
            }
            else                                                                   //Открытие окна
            {
                ExchangeModel.UpdateSetting();
                BoardForm boardForm = new BoardForm(ExchangeModel.DeviceTables);
                boardForm.MdiParent = this;
                boardForm.Show();
            }
        }
        
        private  void коммуникацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CommunicationForm.MyCommunicationForm != null)                                     //Открытие окна повторно, при открытом первом экземпляре.
            {
                CommunicationForm.MyCommunicationForm.Show();
                CommunicationForm.MyCommunicationForm.WindowState = FormWindowState.Normal;
            }
            else                                                                   //Открытие окна
            {
                CommunicationForm boardForm = new CommunicationForm(ExchangeModel.MasterSerialPorts, ExchangeModel.ReOpenMasterSerialPorts);
                boardForm.MdiParent = this;
                boardForm.Show();
            }
        }
        
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.Checked = true;
            toolStripMenuItem2.Checked = false;
            VisibleStyle = 0;
        }
        
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.Checked = false;
            toolStripMenuItem2.Checked = true;
            VisibleStyle = 1;
        }
        
        private void добавитьСтатическоеСообщениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //проверка ДОСТУПА
            if (!autenServ.CheckRoleAcsess(new List<Role> { Role.Администратор, Role.Диктор, Role.Инженер }))
            {
                MessageBox.Show($@"Нет прав!!!   С вашей ролью ""{autenServ.CurrentUser.Role}"" нельзя совершать  это действие.");
                return;
            }

            СтатическоеСообщение Сообщение;
            Сообщение.ID = 0;
            Сообщение.Активность = true;
            Сообщение.Время = DateTime.Now;
            Сообщение.НазваниеКомпозиции = "";
            Сообщение.ОписаниеКомпозиции = "";
            Сообщение.СостояниеВоспроизведения = SoundRecordStatus.ОжиданиеВоспроизведения;
            КарточкаСтатическогоЗвуковогоСообщения ОкноСообщения = new КарточкаСтатическогоЗвуковогоСообщения(Сообщение);
            if (ОкноСообщения.ShowDialog() == DialogResult.OK)
            {
                Сообщение = ОкноСообщения.ПолучитьИзмененнуюКарточку();

                int ПопыткиВставитьСообщение = 5;
                while (ПопыткиВставитьСообщение-- > 0)
                {
                    string Key = Сообщение.Время.ToString("HH:mm:ss");
                    string[] SubKeys = Key.Split(':');
                    if (SubKeys[0].Length == 1)
                        Key = "0" + Key;

                    if (MainWindowForm.СтатическиеЗвуковыеСообщения.ContainsKey(Key))
                    {
                        Сообщение.Время = Сообщение.Время.AddSeconds(1);
                        continue;
                    }

                    MainWindowForm.СтатическиеЗвуковыеСообщения.Add(Key, Сообщение);
                    MainWindowForm.СтатическиеЗвуковыеСообщения.OrderBy(key => key.Value);
                    MainWindowForm.ФлагОбновитьСписокЗвуковыхСообщений = true;
                    break;
                }
            }
        }
        
        private void TSMIПоКалендарю_Click(object sender, EventArgs e)
        {
            TSMIПоПонедельнику.Checked = false;
            TSMIПоВторнику.Checked = false;
            TSMIПоСреде.Checked = false;
            TSMIПоЧетвергу.Checked = false;
            TSMIПоПятнице.Checked = false;
            TSMIПоСубботе.Checked = false;
            TSMIПоВоскресенью.Checked = false;
            TSMIПоКалендарю.Checked = false;

            (sender as ToolStripMenuItem).Checked = true;

            tSDDBРаботаПоДням.BackColor = TSMIПоКалендарю.Checked == true ? Color.LightGray : Color.Yellow;
            switch ((sender as ToolStripMenuItem).Name)
            {
                case "TSMIПоПонедельнику":
                    tSDDBРаботаПоДням.Text = "РАБОТА ПО ПОНЕДЕЛЬНИКУ";
                    MainWindowForm.РаботаПоНомеруДняНедели = 0;
                    break;

                case "TSMIПоВторнику":
                    tSDDBРаботаПоДням.Text = "РАБОТА ПО ВТОРНИКУ";
                    MainWindowForm.РаботаПоНомеруДняНедели = 1;
                    break;

                case "TSMIПоСреде":
                    tSDDBРаботаПоДням.Text = "РАБОТА ПО СРЕДЕ";
                    MainWindowForm.РаботаПоНомеруДняНедели = 2;
                    break;

                case "TSMIПоЧетвергу":
                    tSDDBРаботаПоДням.Text = "РАБОТА ПО ЧЕТВЕРГУ";
                    MainWindowForm.РаботаПоНомеруДняНедели = 3;
                    break;

                case "TSMIПоПятнице":
                    tSDDBРаботаПоДням.Text = "РАБОТА ПО ПЯТНИЦЕ";
                    MainWindowForm.РаботаПоНомеруДняНедели = 4;
                    break;

                case "TSMIПоСубботе":
                    tSDDBРаботаПоДням.Text = "РАБОТА ПО СУББОТЕ";
                    MainWindowForm.РаботаПоНомеруДняНедели = 5;
                    break;

                case "TSMIПоВоскресенью":
                    tSDDBРаботаПоДням.Text = "РАБОТА ПО ВОСКРЕСЕНЬЮ";
                    MainWindowForm.РаботаПоНомеруДняНедели = 6;
                    break;

                case "TSMIПоКалендарю":
                    tSDDBРаботаПоДням.Text = "РАБОТА ПО КАЛЕНДАРЮ";
                    MainWindowForm.РаботаПоНомеруДняНедели = 7;
                    break;
            }

            MainWindowForm.ФлагОбновитьСписокЖелезнодорожныхСообщенийПоДнюНедели = true;
        }
        
        private void настройкиToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ОкноНастроек окно = new ОкноНастроек();
            окно.ShowDialog();
        }
        
        private void добавитьВнештатныйПоездToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //проверка ДОСТУПА
            if (!autenServ.CheckRoleAcsess(new List<Role> { Role.Администратор, Role.Диктор, Role.Инженер }))
            {
                MessageBox.Show($@"Нет прав!!!   С вашей ролью ""{autenServ.CurrentUser.Role}"" нельзя совершать  это действие.");
                return;
            }

            var newRecId = MainWindowForm.SoundRecords.Max(rec => rec.Value.ID) + 1;

            ОкноДобавленияПоезда окно = new ОкноДобавленияПоезда(newRecId);
            if (окно.ShowDialog() == DialogResult.OK)
            {
                var record = окно.Record;

                //Добавление созданной записи                
                var newkey = new SchedulingPipelineService().GetUniqueKey(MainWindowForm.SoundRecords.Keys, record.Время);

                if (!string.IsNullOrEmpty(newkey))
                {
                    record.Время = DateTime.ParseExact(newkey, "yy.MM.dd  HH:mm:ss", new DateTimeFormatInfo());

                    lock (MainWindowForm.SoundRecords_Lock)
                    {
                        MainWindowForm.SoundRecords.Add(newkey, record);
                    }
                    MainWindowForm.SoundRecordsOld.Add(newkey, record);
                }

                MainWindowForm.ФлагОбновитьСписокЖелезнодорожныхСообщенийВТаблице = true;
            }
        }
        
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            СписокВоспроизведения список = new СписокВоспроизведения();
            список.Show();
        }
        
        private void оперативноеРасписаниеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TrainTableOperative.myMainForm != null)
            {
                TrainTableOperative.myMainForm.Show();
                TrainTableOperative.myMainForm.WindowState = FormWindowState.Normal;
            }
            else
            {
                TrainTableOperative listFormOper = new TrainTableOperative { MdiParent = this };
                listFormOper.Show();
            }
        }
        
        private void ИзмененияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ОкноАрхиваИзменений.myMainForm != null)
            {
                ОкноАрхиваИзменений.myMainForm.Show();
                ОкноАрхиваИзменений.myMainForm.WindowState = FormWindowState.Normal;
            }
            else
            {
                ОкноАрхиваИзменений listFormOper = new ОкноАрхиваИзменений { MdiParent = this };
                listFormOper.Show();
            }
        }
        
        private void timer_Clock_Tick(object sender, EventArgs e)
        {
            toolClockLabel.Text = DateTime.Now.ToString("dd.MM  HH:mm:ss");
        }
        
        private void статическаяИнформацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (StaticDisplayForm.MyStaticDisplayForm != null)                                     //Открытие окна повторно, при открытом первом экземпляре.
            {
                StaticDisplayForm.MyStaticDisplayForm.Show();
                StaticDisplayForm.MyStaticDisplayForm.WindowState = FormWindowState.Normal;
            }
            else                                                                   //Открытие окна
            {
                StaticDisplayForm staticDisplayForm = new StaticDisplayForm(ExchangeModel.Binding2StaticFormBehaviors);
                //staticDisplayForm.MdiParent = this;
                staticDisplayForm.Show();
            }
        }
        
        private void tsb_ТехническоеСообщение_Click(object sender, EventArgs e)
        {
            //проверка ДОСТУПА
            if (!autenServ.CheckRoleAcsess(new List<Role> { Role.Администратор, Role.Диктор, Role.Инженер }))
            {
                MessageBox.Show($@"Нет прав!!!   С вашей ролью ""{autenServ.CurrentUser.Role}"" нельзя совершать  это действие.");
                return;
            }

            TechnicalMessageForm techForm = new TechnicalMessageForm();
            techForm.ShowDialog();
        }
        
        /// <summary>
        /// "Пользовательский" -> "Автомат" -> "Ручной" -> "Пользовательский"
        /// </summary>
        private void tSBРежимРаботы_Click(object sender, EventArgs e)
        {
            //var sr_lock = MainWindowForm.SoundRecords_Lock;

            //проверка ДОСТУПА
            if (!autenServ.CheckRoleAcsess(new List<Role> {Role.Администратор, Role.Диктор, Role.Инженер}))
            {
                MessageBox.Show($@"Нет прав!!!   С вашей ролью ""{autenServ.CurrentUser.Role}"" нельзя совершать  это действие.");
                return;
            }

            if (MessageBox.Show(@"Сменить режим работы?", @"Смена режима работы", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            
            var records = MainWindowForm.SoundRecords;

            switch (РежимРаботы.Text)
            {
                case @"Пользовательский":
                    SetMode(records, true);
                    РежимРаботы.Text = @"Автомат";
                    РежимРаботы.BackColor = Color.CornflowerBlue;
                    break;


                case @"Автомат":
                    SetMode(records, false);
                    РежимРаботы.Text = @"Ручной";
                    РежимРаботы.BackColor = Color.Coral;
                    break;


                case @"Ручной":
                    SetMode(records, true);
                    РежимРаботы.Text = @"Автомат";
                    РежимРаботы.BackColor = Color.Coral;
                    break;
            }
        }

        private void SetMode(IDictionary<string, SoundRecord> records, bool isAuto)
        {
            if (records != null)
            {
                for (int i = 0; i < records.Count; i++)
                {
                    var key = records.Keys.ElementAt(i);
                    var value = records.Values.ElementAt(i);
                    value.Автомат = isAuto;
                    lock (MainWindowForm.SoundRecords_Lock)
                    {
                        //if (records.ContainsKey(key))
                            records[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Смена пользователя
        /// </summary>
        private void tSBLogOut_Click(object sender, EventArgs e)
        {
            autenServ.LogOut();
            SendAuthenticationChanges(autenServ.OldUser, "Выход из системы");
            CheckAuthentication(false);
        }


        /// <summary>
        /// Админка. Управление пользователями.
        /// </summary>
        private void tSBAdmin_Click(object sender, EventArgs e)
        {
            var form= new AdminForm();
            form.ShowDialog();
        }


        protected override void OnClosed(EventArgs e)
        {
            Application.Exit();
            try
            {
                Program.ЗаписьЛога("Системное сообщение", "Программный комплекс выключен", autenServ?.CurrentUser ?? null);
                if (autenServ != null)
                {
                    autenServ.LogOut();
                    SendAuthenticationChanges(autenServ.OldUser, "Выход из системы");
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            DispouseCisClientIsConnectRx.Dispose();
            ExchangeModel.Dispose();

            DispouseActivationWarningInvokeRx.Dispose();
            //QuartzVerificationActivation.Shutdown();

            base.OnClosed(e);
        }

        private void tsbAutoPilot_CheckedChanged(object sender, EventArgs e)
        {
            SelectControlMode();
        }

        private void tsbAutoPilot_EnabledChanged(object sender, EventArgs e)
        {
            AutoPilot.Checked = !AutoPilot.Enabled;
        }

        private void tsbAutoPilot_CheckStateChanged(object sender, EventArgs e)
        {
            AutoPilot.BackColor = AutoPilot.CheckState == CheckState.Unchecked ? Color.LimeGreen : Color.Orange;
        }

        public static void SelectControlMode()
        {
            var causeOfChange = "";
            if (!AutoPilot.Enabled)
            {
                Program.MainWindowWorkMode = MainWindowWorkMode.OnlyDictor;
                causeOfChange = "Отключена диспетчерская";
                return;
            }

            if (AutoPilot.Checked)
            {
                Program.MainWindowWorkMode = MainWindowWorkMode.Both;
                causeOfChange = "Включено ручное управление";
            }
            else
            {
                Program.MainWindowWorkMode = MainWindowWorkMode.OnlyDispatcher;
                causeOfChange = "Включено автоматическое управление";
            }
            mainForm.SendChangedMode(mainForm.autenServ.CurrentUser, causeOfChange);
        }

        private void stationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StationsForm stationsForm = new StationsForm(Program.DirectionRepository);
            stationsForm.Show();
        }

        private void resetChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Вы действительно хотите сбросить все изменения поездов Основного окна на текущие сутки?",
                            $"Предупреждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning) == DialogResult.No)
                return;

            Program.SoundRecordChangesDbRepository.Delete(ch => true);

            if (MainWindowForm.myMainForm != null)
            {
                MainWindowForm.myMainForm.RefreshMainList();
            }
        }

        private void changeTimeZoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TrainSheduleTable.ChangeTimeZone(1);

            //if (MainWindowForm.myMainForm != null)
            //{
            //    MainWindowForm.myMainForm.RefreshMainList();
            //}
        }
    }
}
