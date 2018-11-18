using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace MainExample
{
    public partial class Расписание : Form
    {
        #region Fields

        private byte ИндексТекущегоМесяца = 0;
        private byte ПредыдущийИндексТекущегоМесяца = 0;
        private DateTime ВремяПоследнегоНажатияКлавиатуры = DateTime.Now;
        private bool ПризнакИзмененияСтроки = false;
        private ScheduleMode СтарыйРежимРасписания = ScheduleMode.Отсутствует;
        private bool ИнициализацияЗавершена = false;

        private string[] stringMonths = new string[14];

        public int[] intMonths = new int[14];

        #endregion

        #region Properties

        public TrainSchedule TrainSchedule { get; set; }

        #endregion

        #region Ctor

        public Расписание(TrainSchedule trainSchedule)
        {
            InitializeComponent();

            TrainSchedule = trainSchedule;

            for (int i = 0; i < intMonths.Length; i++) // Цикл по количеству прописанных месяцев в календаре
            {
                int monthIndex = i + DateTime.Now.Month - 1;
                int currentYear = DateTime.Now.Year + monthIndex / 12;
                intMonths[i] = monthIndex % 12 + 1;
                int currentMonth = intMonths[i];
                stringMonths[i] = CultureInfo.CreateSpecificCulture("ru-RU").DateTimeFormat.GetMonthName(currentMonth) + " " + currentYear;
             
                string[] ШаблонСтроки = new string[38]; // Массив в 38 строк (максимальное количество элементов в длину)
                for (int j = 0; j < ШаблонСтроки.Length; j++)
                    ШаблонСтроки[j] = " ";

                ШаблонСтроки[0] = stringMonths[i]; // Заполняем колонку месяцев
                int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                
                int dayOfWeek = ((byte) ((new DateTime(currentYear, currentMonth, 1)).DayOfWeek) + 6) % 7;
                for (int j = 1; j <= daysInMonth; j++) // от 1 до количества дней
                    ШаблонСтроки[j + dayOfWeek] = j.ToString(); // Пишем в клетки числа дней месяца в зависимости от того, на какой день недели выпадает его начало

                ListViewItem item = new ListViewItem(ШаблонСтроки, 0); // Добавляем колонки на ListView
                item.UseItemStyleForSubItems = false;

                for (int j = 1; j <= daysInMonth; j++)
                    item.SubItems[j + dayOfWeek].Tag = j + dayOfWeek; // тэгу задаем положение первого дня месяца в строке

                
                listView1.Items.Add(item); // Рисуем дни месяца на форме
            }

            this.Text = "Расписание движения поезда: " + TrainSchedule.TrainNumber + " - " + TrainSchedule.TrainName; // Заголовок
            
            int АктивностьДняДвижения = TrainSchedule.BoundaryDaysActivity;
            if ((АктивностьДняДвижения & 0x40000) != 0x00000) cBНаГрМес.Checked = true; // 0x40000
            if ((АктивностьДняДвижения & 0x00001) != 0x00000) cBГр26.Checked = true; // 0x00001
            if ((АктивностьДняДвижения & 0x00002) != 0x00000) cBГр27.Checked = true; // 0x00002
            if ((АктивностьДняДвижения & 0x00004) != 0x00000) cBГр28.Checked = true; // 0x00004
            if ((АктивностьДняДвижения & 0x00008) != 0x00000) cBГр29.Checked = true; // 0x00008
            if ((АктивностьДняДвижения & 0x00010) != 0x00000) cBГр30.Checked = true; // 0x00010
            if ((АктивностьДняДвижения & 0x00020) != 0x00000) cBГр31.Checked = true; // 0x00020
            if ((АктивностьДняДвижения & 0x00040) != 0x00000) cBГр1.Checked = true; // 0x00040
            if ((АктивностьДняДвижения & 0x00080) != 0x00000) cBГр2.Checked = true; // 0x00080
            if ((АктивностьДняДвижения & 0x00100) != 0x00000) cBГр3.Checked = true; // 0x00100
            if ((АктивностьДняДвижения & 0x00200) != 0x00000) cBГр4.Checked = true; // 0x00200
            if ((АктивностьДняДвижения & 0x00400) != 0x00000) cBГр5.Checked = true; // 0x00400
            if ((АктивностьДняДвижения & 0x00800) != 0x00000) cBГр6.Checked = true; // 0x00800
            if ((АктивностьДняДвижения & 0x01000) != 0x00000) cBГр7.Checked = true; // 0x01000
            if ((АктивностьДняДвижения & 0x02000) != 0x00000) cBГр8.Checked = true; // 0x02000
            if ((АктивностьДняДвижения & 0x04000) != 0x00000) cBГр9.Checked = true; // 0x04000
            if ((АктивностьДняДвижения & 0x08000) != 0x00000) cBГр10.Checked = true; // 0x08000
            if ((АктивностьДняДвижения & 0x10000) != 0x00000) cBГр11.Checked = true; // 0x10000
            if ((АктивностьДняДвижения & 0x20000) != 0x00000) cBГр12.Checked = true; // 0x20000

            ushort ДниНедели = TrainSchedule.WeekDaysActivity;
            if ((ДниНедели & 0x0001) != 0x0000) cBПн.Checked = true;
            if ((ДниНедели & 0x0002) != 0x0000) cBВт.Checked = true;
            if ((ДниНедели & 0x0004) != 0x0000) cBСр.Checked = true;
            if ((ДниНедели & 0x0008) != 0x0000) cBЧт.Checked = true;
            if ((ДниНедели & 0x0010) != 0x0000) cBПт.Checked = true;
            if ((ДниНедели & 0x0020) != 0x0000) cBСб.Checked = true;
            if ((ДниНедели & 0x0040) != 0x0000) cBВск.Checked = true;
            if ((ДниНедели & 0x0100) != 0x0000) cBКрПн.Checked = true;
            if ((ДниНедели & 0x0200) != 0x0000) cBКрВт.Checked = true;
            if ((ДниНедели & 0x0400) != 0x0000) cBКрСр.Checked = true;
            if ((ДниНедели & 0x0800) != 0x0000) cBКрЧт.Checked = true;
            if ((ДниНедели & 0x1000) != 0x0000) cBКрПт.Checked = true;
            if ((ДниНедели & 0x2000) != 0x0000) cBКрСб.Checked = true;
            if ((ДниНедели & 0x4000) != 0x0000) cBКрВск.Checked = true;

            ЗадатьКодПереключателейРежима(TrainSchedule.ScheduleMode);

            ИнициализацияЗавершена = true;

            ОбновитьЦветовуюМаркировкуРасписания();
        }

        #endregion

        #region Methods

        public void УстановитьВремяДействия(string ВремяДействия)
        {
            lblВремяДействия.Text = ВремяДействия;
        }

        private ScheduleMode ПолучитьКодПереключателейРежима()
        {
            if (radioButton1.Checked == true)
                return ScheduleMode.Ежедневно;
            else if (radioButton2.Checked == true)
                return ScheduleMode.ПоЧетным;
            else if (radioButton3.Checked == true)
                return ScheduleMode.ПоНечетным;
            else if (radioButton4.Checked == true)
                return ScheduleMode.Выборочно;
            else if (radioButton6.Checked == true)
                return ScheduleMode.ПоДням;

            return ScheduleMode.Отсутствует;
        }

        private void ЗадатьКодПереключателейРежима(ScheduleMode КодПереключателейРежима)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = false;
            radioButton5.Checked = false;
            radioButton6.Checked = false;
            switch (КодПереключателейРежима)
            {
                case ScheduleMode.Ежедневно:
                    radioButton1.Checked = true; break;
                case ScheduleMode.ПоЧетным:
                    radioButton2.Checked = true; break;
                case ScheduleMode.ПоНечетным:
                    radioButton3.Checked = true; break;
                case ScheduleMode.Выборочно:
                    radioButton4.Checked = true; break;
                case ScheduleMode.ПоДням:
                    radioButton6.Checked = true; break;
                case ScheduleMode.Отсутствует:
                    radioButton5.Checked = true; break;
            }

        }

        private void ОбновитьЦветовуюМаркировкуРасписания()
        {
            byte НомерСтолбца = 0;
            try
            {
                for (byte i = 0; i < intMonths.Length; i++)
                {
                    int monthIndex = i + DateTime.Now.Month - 1;

                    var firstDay = new DateTime(DateTime.Now.Year + monthIndex / 12, monthIndex % 12 + 1, 1);
                    intMonths[i] = firstDay.Month;

                    for (var j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                    {
                        НомерСтолбца = (byte)(j.Day + (((byte)(firstDay.DayOfWeek)) + 6) % 7);

                        ListViewItem.ListViewSubItem SubItem = listView1.Items[i].SubItems[НомерСтолбца];
                        SubItem.BackColor = TrainSchedule.ПолучитьАктивностьДняДвижения(j.Date) ? Color.LightGreen : Color.White;
                        SubItem.ForeColor = ((НомерСтолбца % 7) == 6) || ((НомерСтолбца % 7) == 0) ? Color.Red : Color.Black;
                        listView1.Items[i].SubItems[НомерСтолбца] = SubItem;
                    }
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error($"Номер Столбца: {НомерСтолбца}. {ex}");
            }
        }

        private void ЗадатьАктивностьРасписанияПоДням()
        {
            if (ИнициализацияЗавершена == false)
                return;

            bool[] МассивАктивныхДней = new bool[32];
            bool[] МассивАктивныхМесяцев = new bool[14];
            bool РаботаПоВыбраннымМесяцам = false;

            CheckBox[] МассивЭлементовАктивныхМесяцев = new CheckBox[12] { cBЯнв, cBФев, cBМар, cBАпр, cBМай, cBИюн, cBИюл, cBАвг, cBСен, cBОкт, cBНоя, cBДек };

            int count = 0;
            for (int i = 0; i < intMonths.Length; i++)
            {
                int currentMonth = intMonths[i];
                if (cBРаспространитьНаВсе.Checked || МассивЭлементовАктивныхМесяцев[intMonths[i] - 1].Checked)
                {
                    МассивАктивныхМесяцев[currentMonth - 1] = true;
                    count++;
                }
            }
            // РаботаПоВыбраннымМесяцам
            if (count == 0)
                for (int i = 0; i < intMonths.Length; i++)
                    МассивАктивныхМесяцев[intMonths[i] - 1] = true; // Заменить ИндексТекущегоМесяца

            //for (byte i = 0; i < intMonths.Length; i++)

            //    int currentMonth = intMonths[i];
            //    if (МассивАктивныхМесяцев[currentMonth - 1] == false) // И здесь
            //        continue;




            byte ПоДням = 0x00;
            if (cBПн.Checked) ПоДням |= 0x01;
            if (cBВт.Checked) ПоДням |= 0x02;
            if (cBСр.Checked) ПоДням |= 0x04;
            if (cBЧт.Checked) ПоДням |= 0x08;
            if (cBПт.Checked) ПоДням |= 0x10;
            if (cBСб.Checked) ПоДням |= 0x20;
            if (cBВск.Checked) ПоДням |= 0x40;
            if (cBВ.Checked) ПоДням |= 0x60;

            byte КромеДней = 0x00;
            if (cBКрПн.Checked) КромеДней |= 0x01;
            if (cBКрВт.Checked) КромеДней |= 0x02;
            if (cBКрСр.Checked) КромеДней |= 0x04;
            if (cBКрЧт.Checked) КромеДней |= 0x08;
            if (cBКрПт.Checked) КромеДней |= 0x10;
            if (cBКрСб.Checked) КромеДней |= 0x20;
            if (cBКрВск.Checked) КромеДней |= 0x40;
            if (cBКрВ.Checked) КромеДней |= 0x60;

            TrainSchedule.WeekDaysActivity = (ushort)((КромеДней << 8) | ПоДням);
            TrainSchedule.ScheduleMode = ScheduleMode.ПоДням;

            for (int i = 0; i < intMonths.Length; i++)
            {

                int monthIndex = i + DateTime.Now.Month - 1;
                int currentYear = DateTime.Now.Year + monthIndex / 12;
                int currentMonth = intMonths[i];

                int dayOfWeek = (byte)((new DateTime(currentYear, currentMonth, 1)).DayOfWeek) + 6;

                if (МассивАктивныхМесяцев[currentMonth - 1])
                {
                    byte НомерПервогоДня = 0;
                    byte НомерПоследнегоДня = 30;

                    for (int j = 0; j < 32; j++)
                        МассивАктивныхДней[j] = false;

                    int lastMonth = currentMonth - 1;
                    int lastMonthYear = currentYear;
                    if (lastMonth == 0)
                    {
                        lastMonthYear -= 1;
                        lastMonth = 12;
                    }
                    int daysInLastMonth = DateTime.DaysInMonth(lastMonthYear, lastMonth);

                    if ((cBНаГрМес.Checked == true) && (daysInLastMonth % 2 != 0))
                    {
                        МассивАктивныхДней[25] = cBГр26.Checked;
                        МассивАктивныхДней[26] = cBГр27.Checked;
                        МассивАктивныхДней[27] = cBГр28.Checked;
                        МассивАктивныхДней[28] = cBГр29.Checked;
                        if (daysInLastMonth > 29)
                        {
                            МассивАктивныхДней[29] = cBГр30.Checked;
                            МассивАктивныхДней[30] = cBГр31.Checked;
                        }
                        else
                        {
                            МассивАктивныхДней[29] = false;
                            МассивАктивныхДней[30] = false;
                        }
                        МассивАктивныхДней[0] = cBГр1.Checked;
                        МассивАктивныхДней[1] = cBГр2.Checked;
                        МассивАктивныхДней[2] = cBГр3.Checked;
                        МассивАктивныхДней[3] = cBГр4.Checked;
                        МассивАктивныхДней[4] = cBГр5.Checked;
                        МассивАктивныхДней[5] = cBГр6.Checked;
                        МассивАктивныхДней[6] = cBГр7.Checked;
                        МассивАктивныхДней[7] = cBГр8.Checked;
                        МассивАктивныхДней[8] = cBГр9.Checked;
                        МассивАктивныхДней[9] = cBГр10.Checked;
                        МассивАктивныхДней[10] = cBГр11.Checked;
                        МассивАктивныхДней[11] = cBГр12.Checked;
                        //НомерПервогоДня = 12;
                        //НомерПоследнегоДня = 24;
                    }
                    //else if ((cBНаГрМес.Checked == true) && (daysInLastMonth % 2 == 0))
                    //{
                    //  НомерПервогоДня = 0;
                    //НомерПоследнегоДня = (byte)(DateTime.DaysInMonth(currentYear, currentMonth) - 1);
                    //for (int j = НомерПервогоДня; j <= НомерПоследнегоДня; j++)
                    //{
                    //    МассивАктивныхДней[j] = ; // Найти откуда брать значения на основе типа расписания
                    //}
                    //}

                    if (ПоДням != 0x00)
                    {
                        for (int j = НомерПервогоДня; j <= НомерПоследнегоДня; j++)
                            if ((ПоДням & (0x01 << ((j + dayOfWeek) % 7))) != 0x00)
                                МассивАктивныхДней[j] = true;
                    }
                    else if (КромеДней != 0x00)
                    {
                        for (int j = НомерПервогоДня; j <= НомерПоследнегоДня; j++)
                            if ((КромеДней & (0x01 << ((j + dayOfWeek) % 7))) == 0x00)
                                МассивАктивныхДней[j] = true;
                    }

                    ;
                    for (DateTime firstDay = new DateTime(currentYear, monthIndex % 12 + 1, 1), j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                        TrainSchedule.DayDictionary.AddOrUpdate(j.Date, МассивАктивныхДней[j.Day - 1]);
                }
            }

            ОбновитьЦветовуюМаркировкуРасписания();
        }

        #endregion

        #region Events

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void РежимРасписания_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked == false)
                return;

            bool[] МассивАктивныхМесяцев = new bool[14];
            // bool РаботаПоВыбраннымМесяцам = false;

            CheckBox[] МассивЭлементовАктивныхМесяцев = new CheckBox[12] { cBЯнв, cBФев, cBМар, cBАпр, cBМай, cBИюн, cBИюл, cBАвг, cBСен, cBОкт, cBНоя, cBДек };

            int count = 0;
            for (int i = 0; i < intMonths.Length; i++)
            {
                if (cBРаспространитьНаВсе.Checked || МассивЭлементовАктивныхМесяцев[intMonths[i] - 1].Checked)
                {
                    МассивАктивныхМесяцев[intMonths[i] - 1] = true;
                    count++;
                }
            }
            // РаботаПоВыбраннымМесяцам
            if (count == 0)
                for (int i = 0; i < intMonths.Length; i++)
                    МассивАктивныхМесяцев[intMonths[i] - 1] = true; // Необходимо сделать выборку активных месяцев раздельно для заданных и раздельно для остальных
            
            for (byte i = 0; i < intMonths.Length; i++)
            {
                if (МассивАктивныхМесяцев[intMonths[i] - 1] == false) // И здесь
                    continue;
                int monthIndex = i + DateTime.Now.Month - 1;
                int currentYear = DateTime.Now.Year + monthIndex / 12;
                int lastMonth = intMonths[i] - 1;
                int lastMonthYear = currentYear;
                if (lastMonth == 0)
                {
                    lastMonthYear -= 1;
                    lastMonth = 12;
                }
                int daysInLastMonth = DateTime.DaysInMonth(lastMonthYear, lastMonth);

                var новыйРежимРасписания = ПолучитьКодПереключателейРежима();
                
                var firstDay = new DateTime(currentYear, monthIndex % 12 + 1, 1);
                switch (новыйРежимРасписания)
                {
                    case ScheduleMode.Отсутствует:
                        for (var j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                            TrainSchedule.DayDictionary.AddOrUpdate(j.Date, false);
                        break;

                    case ScheduleMode.Ежедневно:
                        for (var j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                            TrainSchedule.DayDictionary.AddOrUpdate(j.Date, true);
                        break;

                    case ScheduleMode.ПоЧетным:
                        for (var j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                            TrainSchedule.DayDictionary.AddOrUpdate(j.Date, ((j.Day % 2) == 0) ? true : false);
                        break;

                    case ScheduleMode.ПоНечетным:
                        for (var j = firstDay; j < firstDay.AddMonths(1); j = j.AddDays(1))
                            TrainSchedule.DayDictionary.AddOrUpdate(j.Date, ((j.Day % 2) != 0) ? true : false);
                        break;

                    case ScheduleMode.Выборочно:
                        break;

                    case ScheduleMode.ПоДням:
                        ЗадатьАктивностьРасписанияПоДням();
                        break;
                }


                if (cBНаГрМес.Checked && (daysInLastMonth % 2 != 0) && (новыйРежимРасписания == ScheduleMode.ПоНечетным || новыйРежимРасписания == ScheduleMode.ПоЧетным))
                {
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(lastMonthYear, lastMonth, 26).Date, cBГр26.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(lastMonthYear, lastMonth, 27).Date, cBГр27.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(lastMonthYear, lastMonth, 28).Date, cBГр28.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(lastMonthYear, lastMonth, 29).Date, cBГр29.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(lastMonthYear, lastMonth, 30).Date, cBГр30.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(lastMonthYear, lastMonth, 31).Date, cBГр31.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 1).Date, cBГр1.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 2).Date, cBГр2.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 3).Date, cBГр3.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 4).Date, cBГр4.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 5).Date, cBГр5.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 6).Date, cBГр6.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 7).Date, cBГр7.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 8).Date, cBГр8.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 9).Date, cBГр9.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 10).Date, cBГр10.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 11).Date, cBГр11.Checked);
                    TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, 12).Date, cBГр12.Checked);
                }

                TrainSchedule.ScheduleMode = новыйРежимРасписания;
                СтарыйРежимРасписания = новыйРежимРасписания;
            }

            ОбновитьЦветовуюМаркировкуРасписания();
        }
        
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Y >= 24)
            {
                int rowIndex = (e.Y - 24) / 24;

                if (rowIndex < intMonths.Length)
                {
                    lblВыбранМесяц.Text = stringMonths[rowIndex];
                    ИндексТекущегоМесяца = (byte)rowIndex;
                    var РежимРасписанияПоезда = TrainSchedule.ScheduleMode;
                    ЗадатьКодПереключателейРежима(РежимРасписанияПоезда);

                    if (ПредыдущийИндексТекущегоМесяца != ИндексТекущегоМесяца)
                    {
                        ПредыдущийИндексТекущегоМесяца = ИндексТекущегоМесяца;
                        ПризнакИзмененияСтроки = true;
                    }
                    else
                        ПризнакИзмененияСтроки = false;

                    int monthIndex = ИндексТекущегоМесяца + DateTime.Now.Month - 1;
                    int currentYear = DateTime.Now.Year + monthIndex / 12;
                    // && ((DateTime.Now - ВремяПоследнегоНажатияКлавиатуры).Seconds < 1)
                    if ((ПризнакИзмененияСтроки == false) && (РежимРасписанияПоезда == ScheduleMode.Выборочно))
                    {
                        ListViewHitTestInfo lvhti;
                        lvhti = listView1.HitTest(new Point(e.X, e.Y));
                        ListViewItem.ListViewSubItem my4 = lvhti.SubItem;

                        int Число;
                        if (int.TryParse(my4.Text, out Число) == true)
                        {
                            int Tag = (int)my4.Tag;

                            if ((Tag >= 1) && (Tag <= 38) && (Число >= 1) && (Число <= DateTime.DaysInMonth(currentYear, monthIndex % 12 + 1)))
                            {
                                bool ТекущаяАктивность = TrainSchedule.ПолучитьАктивностьДняДвижения(new DateTime(currentYear, monthIndex % 12 + 1, Число).Date);
                                TrainSchedule.DayDictionary.AddOrUpdate(new DateTime(currentYear, monthIndex % 12 + 1, Число).Date, !ТекущаяАктивность);
                                //ОбновитьЦветовуюМаркировкуРасписания();
                            }
                        }
                    }

                    ВремяПоследнегоНажатияКлавиатуры = DateTime.Now;
                }
            }
            ОбновитьЦветовуюМаркировкуРасписания();
        }

        private void ИзмененоПоДням(object sender, EventArgs e)
        {
            if (ИнициализацияЗавершена == false)
                return; 

            CheckBox cb = sender as CheckBox;

            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = false;
            radioButton5.Checked = false;
            radioButton6.Checked = true;

            if (cb.Checked == true)
                switch (cb.Name)
                {
                    case "cBПн": 
                    case "cBВт":
                    case "cBСр":
                    case "cBЧт":
                    case "cBПт":
                    case "cBСб":
                    case "cBВск":
                        cBКрПн.Checked = false;
                        cBКрВт.Checked = false;
                        cBКрСр.Checked = false;
                        cBКрЧт.Checked = false;
                        cBКрПт.Checked = false;
                        cBКрСб.Checked = false;
                        cBКрВск.Checked = false;
                        cBКрВ.Checked = false;
                        break;

                    case "cBКрПн":
                    case "cBКрВт":
                    case "cBКрСр":
                    case "cBКрЧт":
                    case "cBКрПт":
                    case "cBКрСб":
                    case "cBКрВск":
                        cBПн.Checked = false;
                        cBВт.Checked = false;
                        cBСр.Checked = false;
                        cBЧт.Checked = false;
                        cBПт.Checked = false;
                        cBСб.Checked = false;
                        cBВск.Checked = false;
                        cBВ.Checked = false;
                        break;
                }

            if ((cBСб.Checked == true) && (cBВск.Checked == true))
                cBВ.Checked = true;

            if ((cBКрСб.Checked == true) && (cBКрВск.Checked == true))
                cBКрВ.Checked = true;

            ЗадатьАктивностьРасписанияПоДням();
        }
        
        private void ИзмененоНаГраницеМесяца(object sender, EventArgs e)
        {
            int АктивностьРасписанияВКрайниеДни = 0x00000;
            if (cBНаГрМес.Checked) АктивностьРасписанияВКрайниеДни |= 0x40000;
            if (cBГр26.Checked) АктивностьРасписанияВКрайниеДни |= 0x00001;
            if (cBГр27.Checked) АктивностьРасписанияВКрайниеДни |= 0x00002;
            if (cBГр28.Checked) АктивностьРасписанияВКрайниеДни |= 0x00004;
            if (cBГр29.Checked) АктивностьРасписанияВКрайниеДни |= 0x00008;
            if (cBГр30.Checked) АктивностьРасписанияВКрайниеДни |= 0x00010;
            if (cBГр31.Checked) АктивностьРасписанияВКрайниеДни |= 0x00020;
            if (cBГр1.Checked) АктивностьРасписанияВКрайниеДни |= 0x00040;
            if (cBГр2.Checked) АктивностьРасписанияВКрайниеДни |= 0x00080;
            if (cBГр3.Checked) АктивностьРасписанияВКрайниеДни |= 0x00100;
            if (cBГр4.Checked) АктивностьРасписанияВКрайниеДни |= 0x00200;
            if (cBГр5.Checked) АктивностьРасписанияВКрайниеДни |= 0x00400;
            if (cBГр6.Checked) АктивностьРасписанияВКрайниеДни |= 0x00800;
            if (cBГр7.Checked) АктивностьРасписанияВКрайниеДни |= 0x01000;
            if (cBГр8.Checked) АктивностьРасписанияВКрайниеДни |= 0x02000;
            if (cBГр9.Checked) АктивностьРасписанияВКрайниеДни |= 0x04000;
            if (cBГр10.Checked) АктивностьРасписанияВКрайниеДни |= 0x08000;
            if (cBГр11.Checked) АктивностьРасписанияВКрайниеДни |= 0x10000;
            if (cBГр12.Checked) АктивностьРасписанияВКрайниеДни |= 0x20000;
            TrainSchedule.BoundaryDaysActivity = АктивностьРасписанияВКрайниеДни;

            if (radioButton6.Checked)
                ЗадатьАктивностьРасписанияПоДням();
        }

        #endregion
    }
}
