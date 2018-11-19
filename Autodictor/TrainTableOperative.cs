using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Domain.Entitys;
using MainExample.Entites;
using System.Threading.Tasks;
using Domain.Entitys.Train;

namespace MainExample
{
    public partial class TrainTableOperative : Form
    {
        #region fields

        public const string TableFileName = "TableRecordsOperative.ini";
        public const string TableFileNameCis = "TableRecordsRemoteCisOper.ini";
        public static List<TrainTableRecord> TrainTableRecords = new List<TrainTableRecord>();
        private static int _id = 0;
        public static TrainTableOperative myMainForm = null;
        private readonly IDisposable _dispouseRemoteCisTableChangeRx;

        #endregion
        
        #region ctor

        public TrainTableOperative()
        {
            if (myMainForm != null)
                return;

            myMainForm = this;

            InitializeComponent();

            rbSourseSheduleCis.Checked = (TrainSheduleTable.SourceLoad == SourceData.RemoteCis);
            _dispouseRemoteCisTableChangeRx = TrainSheduleTable.RemoteCisTableChangeRx.Subscribe(data =>   //обновим данные в списке, при получении данных.
            {
                if (data == SourceData.RemoteCis)
                {
                    ОбновитьДанныеВСписке();
                }
            });
            btnLoad_Click(null, EventArgs.Empty);
        }

        #endregion
        
        #region EventHandlers

        private void btn_ДобавитьЗапись_Click(object sender, EventArgs e)
        {
            var form = new OperativeTableAddItemForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var tableRec = form.TableRec;
                TrainTableRecords.Add(tableRec);
                ОбновитьДанныеВСписке();
            }
        }
        
        private void btn_УдалитьЗапись_Click(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection sic = this.listView1.SelectedIndices;

            foreach (int item in sic)
            {
                int ID = 0;
                if (int.TryParse(this.listView1.Items[item].SubItems[0].Text, out ID) == true)
                {
                    for (int i = 0; i < TrainTableRecords.Count; i++)
                    {
                        if (TrainTableRecords[i].ID == ID)
                        {
                            TrainTableRecords.RemoveAt(i);
                            break;
                        }
                    }
                    ОбновитьДанныеВСписке();
                }
            }
        }
        
        private void btn_Сохранить_Click(object sender, EventArgs e)
        {
            СохранитьСписок();
        }
        
        //Загрузка расписание из выбранного источника
        private void btnLoad_Click(object sender, EventArgs e)
        {
            TrainSheduleTable.GetCisOperTimetableAsync().GetAwaiter();
            ОбновитьДанныеВСписке();
        }
        
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection sic = this.listView1.SelectedIndices;

            foreach (int item in sic)
            {
                int ID = 0;
                if (int.TryParse(this.listView1.Items[item].SubItems[0].Text, out ID) == true)
                {
                    for (int i = 0; i < TrainTableRecords.Count; i++)
                    {
                        if (TrainTableRecords[i].ID == ID)
                        {
                            TrainTableRecord Данные;

                            Данные = TrainTableRecords[i];
                            var ТекущийПланРасписанияПоезда = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(Данные.Days);
                            ТекущийПланРасписанияПоезда.TrainNumber = Данные.Num;
                            ТекущийПланРасписанияПоезда.TrainName = Данные.Name;

                            Оповещение оповещение = new Оповещение(Данные);
                            оповещение.ShowDialog();
                            Данные.Active = !оповещение.cBБлокировка.Checked;
                            if (оповещение.DialogResult == System.Windows.Forms.DialogResult.OK)
                            {
                                Данные = оповещение.РасписаниеПоезда;
                                if (string.IsNullOrWhiteSpace(Данные.Num))
                                    return;

                                this.listView1.Items[item].SubItems[1].Text = Данные.Num;
                                this.listView1.Items[item].SubItems[2].Text = Данные.Name;
                                this.listView1.Items[item].SubItems[3].Text = Данные.ArrivalTime;
                                this.listView1.Items[item].SubItems[4].Text = Данные.StopTime;
                                this.listView1.Items[item].SubItems[5].Text = Данные.DepartureTime;

                                string СтрокаОписанияРасписания = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(Данные.Days).ПолучитьСтрокуОписанияРасписания();
                                this.listView1.Items[item].SubItems[6].Text = СтрокаОписанияРасписания;

                                this.listView1.Items[item].BackColor = Данные.Active ? Color.LightGreen : Color.LightGray;
                            }

                            TrainTableRecords[i] = Данные;
                            //ОбновитьСостояниеАктивностиВТаблице();
                            break;
                        }
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (myMainForm == this)
                myMainForm = null;

            _dispouseRemoteCisTableChangeRx.Dispose();
            base.OnClosing(e);
        }

        #endregion

        #region Methode

        public static void ОбновитьДанныеВСписке()
        {
            myMainForm.listView1.Items.Clear();

            foreach (var данные in TrainTableRecords)
            {
                string строкаОписанияРасписания = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(данные.Days).ПолучитьСтрокуОписанияРасписания();

                ListViewItem lvi = new ListViewItem(new string[] { данные.ID.ToString(), данные.Num, данные.Name, данные.ArrivalTime, данные.StopTime, данные.DepartureTime, строкаОписанияРасписания });
                lvi.Tag = данные;
                lvi.BackColor = данные.Active ? Color.LightGreen : Color.LightGray;
                myMainForm.listView1.Items.Add(lvi);
            }
        }
        private static async Task ОбновитьДанныеВСпискеAsync()
        {
            ОбновитьДанныеВСписке();
        }

        public static void ЗагрузитьСписок()
        {
            TrainTableRecords.Clear();

            try
            {
                using (StreamReader file = new StreamReader(TrainSheduleTable.SourceLoad == SourceData.Local ? TableFileName : TableFileNameCis))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] Settings = line.Split(';');
                        if ((Settings.Length == 13) || (Settings.Length == 15) || (Settings.Length >= 16))
                        {
                            TrainTableRecord Данные;

                            Данные.ID = int.Parse(Settings[0]);
                            Данные.Num = Settings[1];
                            Данные.Name = Settings[2];
                            Данные.ArrivalTime = Settings[3];
                            Данные.StopTime = Settings[4];
                            Данные.DepartureTime = Settings[5];
                            Данные.Days = Settings[6];

                            var trainSchedule = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(Данные.Days);
                            trainSchedule.TrainNumber = Данные.Num;
                            trainSchedule.TrainName = Данные.Name;
                            Данные.DaysDescription = trainSchedule.ПолучитьСтрокуОписанияРасписания();
                            Данные.Active = Settings[7] == "1" ? true : false;
                            Данные.SoundTemplates = Settings[8];
                            Данные.TrainPathDirection = byte.Parse(Settings[9]);
                            Данные.TrainPathNumber = LoadPathFromFile(Settings[10], out Данные.PathWeekDayes);
                            Данные.ИспользоватьДополнение = new Dictionary<string, bool>()
                            {
                                ["звук"] = false,
                                ["табло"] = false
                            };
                            Данные.Автомат = true;

                            ТипПоезда типПоезда = ТипПоезда.НеОпределен;
                            try
                            {
                                типПоезда = (ТипПоезда)Enum.Parse(typeof(ТипПоезда), Settings[11]);
                            }
                            catch (ArgumentException) { }
                            Данные.ТипПоезда = типПоезда;

                            Данные.Примечание = Settings[12];

                            if (Данные.TrainPathDirection > 2)
                                Данные.TrainPathDirection = 0;


                            var path = Program.TrackRepository.List().FirstOrDefault(p=> p.Name == Данные.TrainPathNumber[WeekDays.Постоянно]);
                            if (path == null)
                                Данные.TrainPathNumber[WeekDays.Постоянно] = "";

                            DateTime НачалоДействия = new DateTime(1900, 1, 1);
                            DateTime КонецДействия = new DateTime(2100, 1, 1);
                            if (Settings.Length >= 15)
                            {
                                DateTime.TryParse(Settings[13], out НачалоДействия);
                                DateTime.TryParse(Settings[14], out КонецДействия);
                            }
                            Данные.ВремяНачалаДействияРасписания = НачалоДействия;
                            Данные.ВремяОкончанияДействияРасписания = КонецДействия;


                            var addition = "";
                            if (Settings.Length >= 16)
                            {
                                addition = Settings[15];
                            }
                            Данные.Addition = addition;


                            if (Settings.Length >= 18)
                            {
                                Данные.ИспользоватьДополнение["табло"] = Settings[16] == "1";
                                Данные.ИспользоватьДополнение["звук"] = Settings[17] == "1";
                            }

                            if (Settings.Length >= 19)
                            {
                                Данные.Автомат = (string.IsNullOrEmpty(Settings[18]) || Settings[18] == "1"); // по умолчанию true
                            }


                            Данные.Num2 = String.Empty;
                            Данные.FollowingTime = String.Empty;
                            Данные.DaysAlias = String.Empty;
                            if (Settings.Length >= 22)
                            {
                                Данные.Num2 = Settings[19];
                                Данные.FollowingTime = Settings[20];
                                Данные.DaysAlias = Settings[21];
                            }


                            Данные.StationDepart = String.Empty;
                            Данные.StationArrival = String.Empty;
                            if (Settings.Length >= 23)
                            {
                                Данные.StationDepart = Settings[22];
                                Данные.StationArrival = Settings[23];
                            }

                            Данные.Direction = String.Empty;
                            if (Settings.Length >= 25)
                            {
                                Данные.Direction = Settings[24];
                            }

                            Данные.ChangeTrainPathDirection = false;
                            if (Settings.Length >= 26)
                            {
                                bool changeDirection;
                                bool.TryParse(Settings[25], out changeDirection);
                                Данные.ChangeTrainPathDirection = changeDirection;
                            }

                            Данные.IsScoreBoardOutput = false;
                            if (Settings.Length >= 27)
                            {
                                bool ограничениеОтправки;
                                bool.TryParse(Settings[26], out ограничениеОтправки);
                                Данные.IsScoreBoardOutput = ограничениеОтправки;
                            }

                            Данные.IsSoundOutput = true;
                            if (Settings.Length >= 28)
                            {
                                bool выводЗвука;
                                bool.TryParse(Settings[27], out выводЗвука);
                                Данные.IsSoundOutput = выводЗвука;
                            }

                            Данные.DaysAliasEng = string.Empty;
                            if (Settings.Length >= 29)
                            {
                                Данные.DaysAliasEng = Settings[28];
                            }

                            Данные.AdditionEng = string.Empty;
                            if (Settings.Length >= 30)
                            {
                                Данные.AdditionEng = Settings[29];
                            }

                            Данные.NoteEng = string.Empty;
                            if (Settings.Length >= 31)
                            {
                                Данные.NoteEng = Settings[30];
                            }

                            int id;
                            Данные.ScheduleId = 0;
                            if (Settings.Length >= 32 && int .TryParse(Settings[31], out id))
                            {
                                Данные.ScheduleId = id;
                            }

                            Данные.TrnId = 0;
                            if (Settings.Length >= 33 && int.TryParse(Settings[32], out id))
                            {
                                Данные.TrnId = id;
                            }

                            Данные.TieTrainId = 0;
                            if (Settings.Length >= 34 && int.TryParse(Settings[33], out id))
                            {
                                Данные.TieTrainId = id;
                            }

                            bool denyAutoUpdate;
                            Данные.DenyAutoUpdate = false;
                            if (Settings.Length >= 35 && bool.TryParse(Settings[34], out denyAutoUpdate))
                            {
                                Данные.DenyAutoUpdate = denyAutoUpdate;
                            }

                            TimetableType timetableType;
                            Данные.TimetableType = TimetableType.Extra;
                            if (Settings.Length >= 36 && Enum.TryParse(Settings[35], out timetableType))
                            {
                                Данные.TimetableType = timetableType;
                            }

                            TrainTableRecords.Add(Данные);
                            Program.НомераПоездов.Add(Данные.Num);
                            if (!string.IsNullOrEmpty(Данные.Num2))
                                Program.НомераПоездов.Add(Данные.Num2);

                            if (Данные.ID > _id)
                                _id = Данные.ID;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }



        private void СохранитьСписок()
        {
            try
            {
                using (StreamWriter DumpFile = new StreamWriter(TrainSheduleTable.SourceLoad == SourceData.Local ? TableFileName : TableFileNameCis))
                {
                    for (int i = 0; i < TrainTableRecords.Count; i++)
                    {
                        var tableRec = TrainTableRecords[i];
                        //tableRec.ID = i + 1;
                        var noteEng = TrainTableRecords[i].NoteEng;

                        string line =
                            tableRec.ID + ";" +
                            TrainTableRecords[i].Num + ";" +
                            TrainTableRecords[i].Name + ";" +
                            TrainTableRecords[i].ArrivalTime + ";" +
                            TrainTableRecords[i].StopTime + ";" +
                            TrainTableRecords[i].DepartureTime + ";" +
                            TrainTableRecords[i].Days + ";" +
                            (TrainTableRecords[i].Active ? "1" : "0") + ";" +
                            TrainTableRecords[i].SoundTemplates + ";" +
                            TrainTableRecords[i].TrainPathDirection.ToString() + ";" +
                            SavePath2File(TrainTableRecords[i].TrainPathNumber, TrainTableRecords[i].PathWeekDayes) + ";" +
                            TrainTableRecords[i].ТипПоезда.ToString() + ";" +
                            TrainTableRecords[i].Примечание + ";" +
                            TrainTableRecords[i].ВремяНачалаДействияРасписания.ToString("dd.MM.yyyy HH:mm:ss") + ";" +
                            TrainTableRecords[i].ВремяОкончанияДействияРасписания.ToString("dd.MM.yyyy HH:mm:ss") + ";" +
                            TrainTableRecords[i].Addition + ";" +
                            (TrainTableRecords[i].ИспользоватьДополнение["табло"] ? "1" : "0") + ";" +
                            (TrainTableRecords[i].ИспользоватьДополнение["звук"] ? "1" : "0") + ";" +
                            (TrainTableRecords[i].Автомат ? "1" : "0") + ";" +

                            TrainTableRecords[i].Num2 + ";" +
                            TrainTableRecords[i].FollowingTime + ";" +
                            TrainTableRecords[i].DaysAlias + ";" +

                            TrainTableRecords[i].StationDepart + ";" +
                            TrainTableRecords[i].StationArrival + ";" +
                            TrainTableRecords[i].Direction + ";" +
                            TrainTableRecords[i].ChangeTrainPathDirection + ";" +
                            TrainTableRecords[i].IsScoreBoardOutput + ";" +
                            TrainTableRecords[i].IsSoundOutput + ";" +
                            TrainTableRecords[i].DaysAliasEng + ";" +
                            TrainTableRecords[i].AdditionEng + ";" +
                            (!string.IsNullOrWhiteSpace(noteEng) ? noteEng : TranslateNote(TrainTableRecords[i], TrainTableRecords[i].Примечание)) + ";" +
                            TrainTableRecords[i].ScheduleId + ";" +
                            TrainTableRecords[i].TrnId + ";" +
                            TrainTableRecords[i].TieTrainId + ";" +
                            TrainTableRecords[i].DenyAutoUpdate + ";" +
                            TrainTableRecords[i].TimetableType;

                        DumpFile.WriteLine(line);
                    }

                    DumpFile.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

        #endregion
    }
}
