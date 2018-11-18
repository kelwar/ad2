using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Domain.Entitys;
using MainExample.Entites;
using Library;

namespace MainExample
{
    public partial class Оповещение : Form
    {
        #region Fields

        public TrainTableRecord РасписаниеПоезда;
        private TrainTableRecord tieTrain;
        private List<TrainTableRecord> Trains = new List<TrainTableRecord>();

        #endregion

        #region Properties

        private string[] СтанцииВыбранногоНаправления  { get; set; } = new string[0];
        public List<Pathways> НомераПутей { get; set; }

        #endregion

        #region Ctor

        public Оповещение(TrainTableRecord расписаниеПоезда)
        {
            this.РасписаниеПоезда = расписаниеПоезда;
            НомераПутей = Program.TrackRepository.List().ToList();

            InitializeComponent();

            cBПутьПоУмолчанию.Items.Add("Не определен");
            foreach (var путь in НомераПутей.Select(p => p.Name))
                cBПутьПоУмолчанию.Items.Add(путь);
       
            cBПутьПоУмолчанию.Text = this.РасписаниеПоезда.TrainPathNumber[WeekDays.Постоянно];
            InitializePathValues(расписаниеПоезда);


            cBОтсчетВагонов.SelectedIndex = this.РасписаниеПоезда.TrainPathDirection;
            chBox_сменнаяНумерация.Checked= this.РасписаниеПоезда.ChangeTrainPathDirection;
            chBoxВыводНаТабло.Checked = this.РасписаниеПоезда.IsScoreBoardOutput;
            chBoxВыводЗвука.Checked = this.РасписаниеПоезда.IsSoundOutput;

            var directions = Program.DirectionRepository.List().ToList();
            if (directions.Any())
            {
                string[] directionNames= directions.Select(d => d.Name).ToArray();
                cBНаправ.Items.AddRange(directionNames);

                //загрузили выбранное направление
                cBНаправ.Text = расписаниеПоезда.Direction;
            }


            string[] Станции = расписаниеПоезда.Name.Split('-');
            if (Станции.Length == 2)
            {
                cBОткуда.Text = Станции[0].Trim(new char[] { ' ' });
                cBКуда.Text = Станции[1].Trim(new char[] { ' ' });
            }
            else if (Станции.Length == 1 && расписаниеПоезда.Name != "")
            {
                cBКуда.Text = расписаниеПоезда.Name.Trim(new char[] { ' ' }); ;
            }



            rBВремяДействияС.Checked = false;
            rBВремяДействияПо.Checked = false;
            rBВремяДействияСПо.Checked = false;
            rBВремяДействияПостоянно.Checked = false;
            if ((расписаниеПоезда.ВремяНачалаДействияРасписания <= new DateTime(1901, 1, 1)) && (расписаниеПоезда.ВремяОкончанияДействияРасписания >= new DateTime(2099, 1, 1)))
                rBВремяДействияПостоянно.Checked = true;
            else if ((расписаниеПоезда.ВремяНачалаДействияРасписания > new DateTime(1901, 1, 1)) && (расписаниеПоезда.ВремяОкончанияДействияРасписания < new DateTime(2099, 1, 1)))
            {
                dTPВремяДействияС2.Value = расписаниеПоезда.ВремяНачалаДействияРасписания;
                dTPВремяДействияПо2.Value = расписаниеПоезда.ВремяОкончанияДействияРасписания;
                rBВремяДействияСПо.Checked = true;
            }
            else if ((расписаниеПоезда.ВремяНачалаДействияРасписания > new DateTime(1901, 1, 1)) && (расписаниеПоезда.ВремяОкончанияДействияРасписания >= new DateTime(2099, 1, 1)))
            {
                dTPВремяДействияС.Value = расписаниеПоезда.ВремяНачалаДействияРасписания;
                rBВремяДействияС.Checked = true;
            }
            else if ((расписаниеПоезда.ВремяНачалаДействияРасписания <= new DateTime(1901, 1, 1)) && (расписаниеПоезда.ВремяОкончанияДействияРасписания < new DateTime(2099, 1, 1)))
            {
                dTPВремяДействияПо.Value = расписаниеПоезда.ВремяОкончанияДействияРасписания;
                rBВремяДействияПо.Checked = true;
            }

            var ТекущийПланРасписанияПоезда = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(this.РасписаниеПоезда.Days);
            Расписание расписание = new Расписание(ТекущийПланРасписанияПоезда);
            tBОписаниеДнейСледования.Text = расписание.TrainSchedule.ПолучитьСтрокуОписанияРасписания();
            tb_ДниСледованияAlias.Text = расписаниеПоезда.DaysAlias;
            tb_DaysFollowingEng.Text = расписаниеПоезда.DaysAliasEng;


            this.Text = "Расписание движения для поезда: " + расписаниеПоезда.Num + " - " + расписаниеПоезда.Name;
            tBНомерПоезда.Text = расписаниеПоезда.Num;
            tBНомерПоездаДоп.Text = расписаниеПоезда.Num2;

            tb_Дополнение.Text = расписаниеПоезда.Addition;
            tbAdditionEng.Text = расписаниеПоезда.AdditionEng;
            cb_Дополнение_Табло.Checked = расписаниеПоезда.ИспользоватьДополнение["табло"];
            cb_Дополнение_Звук.Checked = расписаниеПоезда.ИспользоватьДополнение["звук"];

            rB_РежРабАвтомат.Checked = расписаниеПоезда.Автомат;
            rB_РежРабРучной.Checked = !расписаниеПоезда.Автомат;





            cBШаблонОповещения.Items.Add("Блокировка");

            foreach (var Item in DynamicSoundForm.DynamicSoundRecords)
                cBШаблонОповещения.Items.Add(Item.Name);

            string[] ШаблонОповещения = расписаниеПоезда.SoundTemplates.Split(':');
            int ТипОповещенияПути = 0;
            if ((ШаблонОповещения.Length % 3) == 0)
            {
                for (int i = 0; i < ШаблонОповещения.Length / 3; i++)
                {
                    if (cBШаблонОповещения.Items.Contains(ШаблонОповещения[3 * i + 0]))
                    {
                        int.TryParse(ШаблонОповещения[3 * i + 2], out ТипОповещенияПути);
                        ТипОповещенияПути %= 2;
                        ListViewItem lvi = new ListViewItem(new string[] { ШаблонОповещения[3 * i + 0], ШаблонОповещения[3 * i + 1], Program.ТипыВремени[ТипОповещенияПути] });
                        this.lVШаблоныОповещения.Items.Add(lvi);
                    }
                }
            }

            cBВремяОповещения.SelectedIndex = 0;


            string ВремяПрибытия = this.РасписаниеПоезда.ArrivalTime;
            string ВремяОтправления = this.РасписаниеПоезда.DepartureTime;
            string ВремяВПути = this.РасписаниеПоезда.FollowingTime;

            rBПрибытие.Checked = false;
            rBОтправление.Checked = false;
            rBТранзит.Checked = false;

            int Часы = 0, Минуты = 0;
            if (ВремяПрибытия == "")
            {
                rBОтправление.Checked = true;
                string[] SubStrings = ВремяОтправления.Split(':');
                if (int.TryParse(SubStrings[0], out Часы) && int.TryParse(SubStrings[1], out Минуты))
                    dTPОтправление.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Часы, Минуты, 0);
            }
            else if (ВремяОтправления == "")
            {
                rBПрибытие.Checked = true;
                string[] SubStrings = ВремяПрибытия.Split(':');
                if (int.TryParse(SubStrings[0], out Часы) && int.TryParse(SubStrings[1], out Минуты))
                    dTPОтправление.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Часы, Минуты, 0);
            }
            else
            {
                rBТранзит.Checked = true;
                string[] SubStrings;

                SubStrings = ВремяПрибытия.Split(':');
                if (int.TryParse(SubStrings[0], out Часы) && int.TryParse(SubStrings[1], out Минуты))
                    dTPОтправление.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Часы, Минуты, 0);

                SubStrings = ВремяОтправления.Split(':');
                if (int.TryParse(SubStrings[0], out Часы) && int.TryParse(SubStrings[1], out Минуты))
                    dTPПрибытие.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Часы, Минуты, 0);
            }

            if (!string.IsNullOrEmpty(ВремяВПути))
            {
                string[] SubStrings = ВремяВПути.Split(':');
                if (int.TryParse(SubStrings[0], out Часы) && int.TryParse(SubStrings[1], out Минуты))
                {
                    dTPСледования.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Часы / 24, Часы % 24, Минуты);
                }
            }
            else
            {
                dTPСледования.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }


            cBБлокировка.Checked = !расписаниеПоезда.Active;
            cBКатегория.SelectedIndex = (int)расписаниеПоезда.ТипПоезда;




            rBНеОповещать.Checked = false;
            rBСоВсемиОстановками.Checked = false;
            rBБезОстановок.Checked = false;
            rBСОстановкамиНа.Checked = false;
            rBСОстановкамиКроме.Checked = false;

            if (расписаниеПоезда.Примечание.Contains("Со всеми остановками"))
            {
                rBСоВсемиОстановками.Checked = true;
            }
            else if (расписаниеПоезда.Примечание.Contains("Без остановок"))
            {
                rBБезОстановок.Checked = true;
            }
            else if (расписаниеПоезда.Примечание.Contains("С остановками: "))
            {
                rBСОстановкамиНа.Checked = true;
                string Примечание = расписаниеПоезда.Примечание.Replace("С остановками: ", "");
                string[] СписокСтанций = Примечание.Split(',');
                foreach (var Станция in СписокСтанций)
                    if (СтанцииВыбранногоНаправления.Contains(Станция))
                        lVСписокСтанций.Items.Add(Станция);
            }
            else if (расписаниеПоезда.Примечание.Contains("Кроме: "))
            {
                rBСОстановкамиКроме.Checked = true;
                string Примечание = расписаниеПоезда.Примечание.Replace("Кроме: ", "");
                string[] СписокСтанций = Примечание.Split(',');
                foreach (var Станция in СписокСтанций)
                    if (СтанцииВыбранногоНаправления.Contains(Станция))
                        lVСписокСтанций.Items.Add(Станция);
            }
            else
            {
                rBНеОповещать.Checked = true;
            }
        }

        #endregion

        #region Methods

        public string ПолучитьШаблоныОповещения()
        {
            string РезультирующийШаблонОповещения = "";

            for (int item = 0; item < this.lVШаблоныОповещения.Items.Count; item++)
            {
                РезультирующийШаблонОповещения += this.lVШаблоныОповещения.Items[item].SubItems[0].Text + ":";
                РезультирующийШаблонОповещения += this.lVШаблоныОповещения.Items[item].SubItems[1].Text + ":";
                РезультирующийШаблонОповещения += (this.lVШаблоныОповещения.Items[item].SubItems[2].Text == "Отправление") ? "1:" : "0:";
            }

            if (РезультирующийШаблонОповещения.Length > 0)
                if (РезультирующийШаблонОповещения[РезультирующийШаблонОповещения.Length - 1] == ':')
                    РезультирующийШаблонОповещения = РезультирующийШаблонОповещения.Remove(РезультирующийШаблонОповещения.Length - 1);

            return РезультирующийШаблонОповещения;
        }

        private void InitializePathValues(TrainTableRecord rec)
        {
            if (!rec.PathWeekDayes)
            {
                dgv_ПутиПоДнямНедели.Enabled = false;
                cBПутьПоУмолчанию.Enabled = true;
                cBПутьПоУмолчанию.Text = rec.TrainPathNumber[WeekDays.Постоянно];
                rb_Постоянно.Checked = true;
            }
            else
            {
                rb_ПоДнямНедели.Checked = true;
                dgv_ПутиПоДнямНедели.Enabled = true;
                cBПутьПоУмолчанию.Enabled = false;
            }

            DataGridViewComboBoxColumn cmb = (DataGridViewComboBoxColumn)dgv_ПутиПоДнямНедели.Columns[1];
            foreach (var путь in НомераПутей.Select(p => p.Name))
            {
                cmb.Items.Add(путь);
            }

            int rowNumber = 0;
            foreach (var path in rec.TrainPathNumber)
            {
                if (path.Key == WeekDays.Постоянно)
                    continue;

                object[] row = { path.Key.ToString() };
                dgv_ПутиПоДнямНедели.Rows.Add(row);

                // Выставить значения путей 
                dgv_ПутиПоДнямНедели.Rows[rowNumber].Cells["cmb_Путь"].Value = string.IsNullOrEmpty(path.Value) ? string.Empty : path.Value;
                dgv_ПутиПоДнямНедели.Rows[rowNumber].Cells["cmb_Путь"].Tag = path.Key;
                rowNumber++;
            }

        }
        
        private void ChangePathValues(TrainTableRecord rec)
        {
            if (!rec.PathWeekDayes)
            {
                dgv_ПутиПоДнямНедели.Enabled = false;
                cBПутьПоУмолчанию.Enabled = true;
                rb_Постоянно.Checked = true;
                cBПутьПоУмолчанию.Text = rec.TrainPathNumber[WeekDays.Постоянно];
            }
            else
            {
                if (dgv_ПутиПоДнямНедели.Rows.Count == 0)
                    return;

                rb_ПоДнямНедели.Checked = true;
                dgv_ПутиПоДнямНедели.Enabled = true;
                cBПутьПоУмолчанию.Enabled = false;

                int rowNumber = 0;
                foreach (var path in rec.TrainPathNumber)
                {
                    if (path.Key == WeekDays.Постоянно)
                        continue;

                    // Выставить значения путей
                    dgv_ПутиПоДнямНедели.Rows[rowNumber].Cells["cmb_Путь"].Value = string.IsNullOrEmpty(path.Value) ? string.Empty : path.Value;
                    rowNumber++;
                }
            }
        }
        
        private void SavePathValues(ref TrainTableRecord rec)
        {
            rec.TrainPathNumber[WeekDays.Постоянно] = cBПутьПоУмолчанию.Text;

            for (int i = 0; i < dgv_ПутиПоДнямНедели.Rows.Count; i++)
            {
                var key = (WeekDays)dgv_ПутиПоДнямНедели.Rows[i].Cells["cmb_Путь"].Tag;
                rec.TrainPathNumber[key] = (string)((dgv_ПутиПоДнямНедели.Rows[i].Cells["cmb_Путь"].Value == null) ? string.Empty : dgv_ПутиПоДнямНедели.Rows[i].Cells["cmb_Путь"].Value);
            }
        }

        #endregion

        #region Events

        private void Оповещение_Load(object sender, EventArgs e)
        {
            var isArrival = string.IsNullOrEmpty(РасписаниеПоезда.DepartureTime);
            cmbTieTrains.Enabled = isArrival;

            var defaultRec = default(TrainTableRecord);
            Trains.Add(defaultRec);
            Trains.AddRange(TrainSheduleTable.TrainTableRecords.Where(tr => tr.ТипПоезда.Equals(РасписаниеПоезда.ТипПоезда) &&
                                                                            tr.ID != РасписаниеПоезда.ID &&
                                                                            (isArrival ? string.IsNullOrWhiteSpace(tr.ArrivalTime) : string.IsNullOrWhiteSpace(tr.DepartureTime))).ToList());

            cmbTieTrains.DataSource = Trains;
            tieTrain = Trains.FirstOrDefault(tr => tr.ID == РасписаниеПоезда.TieTrainId);
            cmbTieTrains.SelectedItem = tieTrain;
        }

        private void btnПрименить_Click(object sender, EventArgs e)
        {
            РасписаниеПоезда.Num = tBНомерПоезда.Text;
            РасписаниеПоезда.Num2 = tBНомерПоездаДоп.Text;

            РасписаниеПоезда.Addition = tb_Дополнение.Text;
            РасписаниеПоезда.AdditionEng = tbAdditionEng.Text;
            РасписаниеПоезда.ИспользоватьДополнение["табло"] = cb_Дополнение_Табло.Checked;
            РасписаниеПоезда.ИспользоватьДополнение["звук"] = cb_Дополнение_Звук.Checked;

            РасписаниеПоезда.Автомат = rB_РежРабАвтомат.Checked;


            if (cBОткуда.Text != "")
                РасписаниеПоезда.Name = cBОткуда.Text + " - " + cBКуда.Text;
            else
                РасписаниеПоезда.Name = cBКуда.Text;


            РасписаниеПоезда.Direction = cBНаправ.Text;

            //if (rBТранзит.Checked)
            //{
                РасписаниеПоезда.StationDepart = cBОткуда.Text;
                РасписаниеПоезда.StationArrival = cBКуда.Text;
            //}
            //else
            //if (rBОтправление.Checked)
            //{
            //    РасписаниеПоезда.StationArrival = cBКуда.Text;
            //    РасписаниеПоезда.StationDepart = String.Empty;
            //}
            //else
            //if (rBПрибытие.Checked)
            //{
            //    РасписаниеПоезда.StationDepart = cBОткуда.Text;
            //    РасписаниеПоезда.StationArrival = String.Empty;
            //}


            if (rBВремяДействияС.Checked == true)
            {
                РасписаниеПоезда.ВремяНачалаДействияРасписания = dTPВремяДействияС.Value;
                РасписаниеПоезда.ВремяОкончанияДействияРасписания = new DateTime(2100, 1, 1);
            }
            else if (rBВремяДействияПо.Checked == true)
            {
                РасписаниеПоезда.ВремяНачалаДействияРасписания = new DateTime(1900, 1, 1);
                РасписаниеПоезда.ВремяОкончанияДействияРасписания = dTPВремяДействияПо.Value;
            }
            else if (rBВремяДействияСПо.Checked == true)
            {
                РасписаниеПоезда.ВремяНачалаДействияРасписания = dTPВремяДействияС2.Value;
                РасписаниеПоезда.ВремяОкончанияДействияРасписания = dTPВремяДействияПо2.Value;
            }
            else if (rBВремяДействияПостоянно.Checked == true)
            {
                РасписаниеПоезда.ВремяНачалаДействияРасписания = new DateTime(1900, 1, 1);
                РасписаниеПоезда.ВремяОкончанияДействияРасписания = new DateTime(2100, 1, 1);
            }

            РасписаниеПоезда.Active = !cBБлокировка.Checked;
            РасписаниеПоезда.SoundTemplates = ПолучитьШаблоныОповещения();

            SavePathValues(ref РасписаниеПоезда);


            РасписаниеПоезда.TrainPathDirection = (byte)cBОтсчетВагонов.SelectedIndex;
            РасписаниеПоезда.ТипПоезда = (ТипПоезда)cBКатегория.SelectedIndex;

            РасписаниеПоезда.ChangeTrainPathDirection = chBox_сменнаяНумерация.Checked;
            РасписаниеПоезда.IsScoreBoardOutput = chBoxВыводНаТабло.Checked;
            РасписаниеПоезда.IsSoundOutput = chBoxВыводЗвука.Checked;


            if (rBПрибытие.Checked == true)
            {
                РасписаниеПоезда.ArrivalTime = dTPОтправление.Value.ToString("HH:mm");
                РасписаниеПоезда.StopTime = "";
                РасписаниеПоезда.DepartureTime = "";
            }
            else if (rBОтправление.Checked == true)
            {
                РасписаниеПоезда.ArrivalTime = "";
                РасписаниеПоезда.StopTime = "";
                РасписаниеПоезда.DepartureTime = dTPОтправление.Value.ToString("HH:mm");
            }
            else
            {
                РасписаниеПоезда.ArrivalTime = dTPОтправление.Value.ToString("HH:mm");
                РасписаниеПоезда.DepartureTime = dTPПрибытие.Value.ToString("HH:mm");

                //var времяПрибытия = dTPПрибытие.Value;
                //if (dTPОтправление.Value < времяПрибытия)
                //{
                //    времяПрибытия = времяПрибытия.AddDays(-1);
                //}
                //var stopTime = (dTPОтправление.Value - времяПрибытия);

                var parser = Parser.GetParser();
                var arrivalTime = parser.ToDateTime(РасписаниеПоезда.ArrivalTime);
                var departureTime = parser.ToDateTime(РасписаниеПоезда.DepartureTime);
                РасписаниеПоезда.StopTime = arrivalTime != DateTime.MinValue && departureTime != DateTime.MinValue ?
                                    (departureTime < arrivalTime ?
                                    departureTime.AddDays(1) - arrivalTime :
                                    departureTime - arrivalTime).ToString("hh\\:mm") :
                                  string.Empty;

            }
            
            РасписаниеПоезда.FollowingTime = $"{dTPСледования.Value.Hour * 24 + dTPСледования.Value.Minute}:{dTPСледования.Value.Second}";
            
            if (rBНеОповещать.Checked)
            {
                РасписаниеПоезда.Примечание = "";
                РасписаниеПоезда.NoteEng = "";
            }
            else if (rBСоВсемиОстановками.Checked)
            {
                РасписаниеПоезда.Примечание = "Со всеми остановками";
                РасписаниеПоезда.NoteEng = "With all stops";
            }
            else if (rBБезОстановок.Checked)
            {
                РасписаниеПоезда.Примечание = "Без остановок";
                РасписаниеПоезда.NoteEng = "Without stops";
            }
            else if (rBСОстановкамиНа.Checked)
            {
                РасписаниеПоезда.Примечание = "С остановками: ";
                РасписаниеПоезда.NoteEng = "With stops: ";
                var count = lVСписокСтанций.Items.Count;
                for (int i = 0; i < count; i++)
                {
                    var nameRu = lVСписокСтанций.Items[i].SubItems[0].Text;
                    РасписаниеПоезда.Примечание += nameRu + ",";

                    var station = Program.DirectionRepository.GetByName(РасписаниеПоезда.Direction)?.GetStationInDirectionByName(nameRu);
                    var nameEng = station?.NameEng ?? "";
                    РасписаниеПоезда.NoteEng += (!string.IsNullOrWhiteSpace(nameEng) ? nameEng : nameRu) + (i < count - 1 ? "," : "");
                }

                if (РасписаниеПоезда.Примечание.Length > 10)
                    if (РасписаниеПоезда.Примечание[РасписаниеПоезда.Примечание.Length - 1] == ',')
                        РасписаниеПоезда.Примечание = РасписаниеПоезда.Примечание.Remove(РасписаниеПоезда.Примечание.Length - 1);
            }
            else if (rBСОстановкамиКроме.Checked)
            {
                РасписаниеПоезда.Примечание = "Кроме: ";
                РасписаниеПоезда.NoteEng = "Except: ";
                var count = lVСписокСтанций.Items.Count;
                for (int i = 0; i < count; i++)
                {
                    var nameRu = lVСписокСтанций.Items[i].SubItems[0].Text;
                    РасписаниеПоезда.Примечание += nameRu + ",";

                    var station = Program.DirectionRepository.GetByName(РасписаниеПоезда.Direction)?.GetStationInDirectionByName(nameRu);
                    var nameEng = station?.NameEng ?? "";
                    РасписаниеПоезда.NoteEng += (!string.IsNullOrWhiteSpace(nameEng) ? nameEng : nameRu) + (i < count - 1 ? "," : "");
                }

                if (РасписаниеПоезда.Примечание.Length > 10)
                    if (РасписаниеПоезда.Примечание[РасписаниеПоезда.Примечание.Length - 1] == ',')
                        РасписаниеПоезда.Примечание = РасписаниеПоезда.Примечание.Remove(РасписаниеПоезда.Примечание.Length - 1);
            }

            РасписаниеПоезда.DaysAlias = tb_ДниСледованияAlias.Text;
            РасписаниеПоезда.DaysAliasEng = tb_DaysFollowingEng.Text;

            РасписаниеПоезда.TieTrainId = tieTrain.ID;
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        
        private void rBОтправление_CheckedChanged(object sender, EventArgs e)
        {
            if (rBПрибытие.Checked)
            {
                dTPПрибытие.Visible = false;
                tBНомерПоездаДоп.Visible = false;

                chBox_сменнаяНумерация.Checked = false;
                chBox_сменнаяНумерация.Enabled = false;
            }
            else if (rBОтправление.Checked)
            {
                dTPПрибытие.Visible = false;
                tBНомерПоездаДоп.Visible = false;

                chBox_сменнаяНумерация.Checked = false;
                chBox_сменнаяНумерация.Enabled = false;
            }
            else
            {
                dTPПрибытие.Visible = true;
                tBНомерПоездаДоп.Visible = true;
                chBox_сменнаяНумерация.Enabled = true;
            }
        }
        
        private void btnРедактировать_Click(object sender, EventArgs e)
        {
            string списокВыбранныхСтанций = "";
            for (int i = 0; i < lVСписокСтанций.Items.Count; i++)
                списокВыбранныхСтанций += lVСписокСтанций.Items[i].Text + ",";

            СписокСтанций списокСтанций = new СписокСтанций(списокВыбранныхСтанций, СтанцииВыбранногоНаправления);

            if (списокСтанций.ShowDialog() == DialogResult.OK)
            {
                List<string> результирующиеСтанции = списокСтанций.ПолучитьСписокВыбранныхСтанций();
                lVСписокСтанций.Items.Clear();
                foreach (var res in результирующиеСтанции)
                    lVСписокСтанций.Items.Add(res);
            }
        }
        
        private void cBБлокировка_CheckedChanged(object sender, EventArgs e)
        {
            if (cBБлокировка.Checked)
            {
                tBНомерПоезда.Enabled = false;
                cBКатегория.Enabled = false;
                gBНаправление.Enabled = false;
                gBОстановки.Enabled = false;
                gBДниСледования.Enabled = false;
                cBПутьПоУмолчанию.Enabled = false;
                cBОтсчетВагонов.Enabled = false;
                gBШаблонОповещения.Enabled = false;
                chBox_сменнаяНумерация.Enabled = false;
            }
            else
            {
                tBНомерПоезда.Enabled = true;
                cBКатегория.Enabled = true;
                gBНаправление.Enabled = true;
                gBОстановки.Enabled = true;
                gBДниСледования.Enabled = true;
                cBПутьПоУмолчанию.Enabled = true;
                cBОтсчетВагонов.Enabled = true;
                gBШаблонОповещения.Enabled = true;
                chBox_сменнаяНумерация.Enabled = true;
            }
        }
        
        private void btnДниСледования_Click(object sender, EventArgs e)
        {
            var ТекущийПланРасписанияПоезда = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(this.РасписаниеПоезда.Days);
            ТекущийПланРасписанияПоезда.TrainNumber = РасписаниеПоезда.Num;
            ТекущийПланРасписанияПоезда.TrainName = РасписаниеПоезда.Name;

            Расписание расписание = new Расписание(ТекущийПланРасписанияПоезда);


            string ВремяДействия = "";
            if (rBВремяДействияС.Checked)
                ВремяДействия = "c " + dTPВремяДействияС.Value.ToString("dd.MM.yyyy");
            else if (rBВремяДействияПо.Checked)
                ВремяДействия = "по " + dTPВремяДействияПо.Value.ToString("dd.MM.yyyy");
            else if (rBВремяДействияСПо.Checked)
                ВремяДействия = "c " + dTPВремяДействияС2.Value.ToString("dd.MM.yyyy") + " по " + dTPВремяДействияПо2.Value.ToString("dd.MM.yyyy");
            else
                ВремяДействия = "постоянно";

            расписание.УстановитьВремяДействия(ВремяДействия);
            расписание.ShowDialog();
            if (расписание.DialogResult == DialogResult.OK)
            {
                РасписаниеПоезда.Days = расписание.TrainSchedule.ПолучитьСтрокуРасписания();
                РасписаниеПоезда.DaysDescription = расписание.TrainSchedule.ПолучитьСтрокуОписанияРасписания();
                tBОписаниеДнейСледования.Text = РасписаниеПоезда.DaysDescription;
            }
        }
        
        private void btnДобавитьШаблон_Click(object sender, EventArgs e)
        {
            if (cBШаблонОповещения.SelectedIndex >= 0)
            {
                string ВремяОповещения = tBВремяОповещения.Text.Replace(" ", "");
                string[] Времена = ВремяОповещения.Split(',');

                int TempInt = 0;
                bool Result = true;

                foreach (var ВременнойИнтервал in Времена)
                    Result &= int.TryParse(ВременнойИнтервал, out TempInt);

                if (Result == true)
                {
                    ListViewItem lvi = new ListViewItem(new string[] { cBШаблонОповещения.Text, tBВремяОповещения.Text, cBВремяОповещения.Text });
                    this.lVШаблоныОповещения.Items.Add(lvi);
                }
                else
                {
                    MessageBox.Show(this, "Строка должна содержать время смещения шаблона оповещения, разделенного запятыми", "Внимание !!!");
                }
            }
        }
        
        private void btnУдалитьШаблон_Click(object sender, EventArgs e)
        {
            while (lVШаблоныОповещения.SelectedItems.Count > 0)
            {
                lVШаблоныОповещения.Items.Remove(lVШаблоныОповещения.SelectedItems[0]);
            }
        }
        
        private void rb_Постоянно_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.Checked)
            {
                РасписаниеПоезда.PathWeekDayes = false;
                ChangePathValues(РасписаниеПоезда);
            }
        }
        
        private void rb_ПоДнямНедели_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.Checked)
            {
                РасписаниеПоезда.PathWeekDayes = true;
                ChangePathValues(РасписаниеПоезда);
            }
        }
        
        private void cBНаправ_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var selectedIndex = comboBox.SelectedIndex;

                if(selectedIndex < 0)
                    return;

                var directions = Program.DirectionRepository.List().ToList();
                if (directions.Any())
                {
                    СтанцииВыбранногоНаправления = directions[selectedIndex].Stations?.Select(st => st.NameRu).ToArray();
                    if (СтанцииВыбранногоНаправления != null && СтанцииВыбранногоНаправления.Any())
                    {
                        cBОткуда.Items.Clear();
                        cBКуда.Items.Clear();
                        cBОткуда.Items.AddRange(СтанцииВыбранногоНаправления);
                        cBКуда.Items.AddRange(СтанцииВыбранногоНаправления);
                    }
                }

                rBНеОповещать.Checked = true;             
                РасписаниеПоезда.Примечание = "";
                РасписаниеПоезда.NoteEng = "";
                lVСписокСтанций.Items.Clear();
            }
        }
        
        private void rBВремяДействияС_CheckedChanged(object sender, EventArgs e)
        {
            var radioBtn = sender as RadioButton;
            if (radioBtn != null)
            {
                if (radioBtn.Checked)
                {
                    grbВремяДействия.BackColor = Color.LightCoral;
                }
            }
        }

        private void rBВремяДействияПо_CheckedChanged(object sender, EventArgs e)
        {
            var radioBtn = sender as RadioButton;
            if (radioBtn != null)
            {
                if (radioBtn.Checked)
                {
                    grbВремяДействия.BackColor = Color.LightCoral;
                }
            }
        }

        private void rBВремяДействияСПо_CheckedChanged(object sender, EventArgs e)
        {
            var radioBtn = sender as RadioButton;
            if (radioBtn != null)
            {
                if (radioBtn.Checked)
                {
                    grbВремяДействия.BackColor = Color.LightCoral;
                }
            }
        }

        private void rBВремяДействияПостоянно_CheckedChanged(object sender, EventArgs e)
        {
            var radioBtn = sender as RadioButton;
            if (radioBtn != null)
            {
                if (radioBtn.Checked)
                {
                    grbВремяДействия.BackColor = DefaultBackColor;
                }
            }
        }

        // Запрет на прием данных из ЦИС
        private void cbDenyAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            РасписаниеПоезда.DenyAutoUpdate = cbDenyAutoUpdate.Checked;
        }
        
        private void cmbTieTrains_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tr = (TrainTableRecord)cmbTieTrains.SelectedItem;
            if (tieTrain.TieTrainId != 0 && tr.TieTrainId != tieTrain.TieTrainId)
            {
                var index = TrainSheduleTable.TrainTableRecords.IndexOf(tieTrain);
                tieTrain.TieTrainId = 0;
                TrainSheduleTable.TrainTableRecords[index] = tieTrain;
            }
            tieTrain = tr;
        }

        #endregion
    }
}
