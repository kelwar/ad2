using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AutodictorBL.Entites;
using Domain.Entitys;
using MainExample.Entites;
using MainExample.Services;
using MainExample.Services.FactoryServices;
using Library.Logs;


namespace MainExample
{
    public partial class КарточкаДвиженияПоезда : Form
    {
        private SoundRecord _record;
        private readonly SoundRecord _recordOld;
        private readonly string _key;
        private Pathways track;

        public bool ПрименитьКоВсемСообщениям = true;
        private bool _сделаныИзменения = false;
        private bool _разрешениеИзменений = false;
        private List<string> СтанцииВыбранногоНаправления { get; set; }
        public List<Pathways> НомераПутей { get; set; }
        
        private Composition _composition;
        private SectorsPanel sp;
        private CompositionPanel cp;

        #region ctor

        public КарточкаДвиженияПоезда(SoundRecord record, string key)
        {
            _record = record;
            _record.ИспользоватьДополнение = record.ИспользоватьДополнение != null ? 
                                             new Dictionary<string, bool>(record.ИспользоватьДополнение) : 
                                             new Dictionary<string, bool>()
                                             {
                                                 ["звук"] = true,
                                                 ["табло"] = true
                                             };

            _recordOld = record;
            _key = key;
            СтанцииВыбранногоНаправления = Program.DirectionRepository.GetByName(record.Направление)?.Stations?.Select(st => st.NameRu).ToList() ?? new List<string>();
            НомераПутей = Program.TrackRepository.List().ToList();
            
            InitializeComponent();

            if (sp == null)
            {
                sp = SectorsPanel.CreatePanel(pnlVagonNavigation);
                pnlVagonNavigation.Controls.Add(sp);
            }

            if (cp == null)
            {
                cp = CompositionPanel.CreatePanel(pnlVagonNavigation, this);
                pnlVagonNavigation.Controls.Add(cp);
            }

            Model2UiControls(_record);

            //_record.CompositionChanged += new SoundRecord.CompositionChangedHandler(_record_CompositionChanged);
        }

        #endregion




        #region Methode

        private void Model2UiControls(SoundRecord record)
        {
            cBОтменен.Checked = !record.Активность;

            cBПоездОтменен.Checked = false;
            cBПрибытиеЗадерживается.Checked = false;
            cbLandingDelay.Checked = false;
            cBОтправлениеЗадерживается.Checked = false;
            cBОтправлениеПоГотовности.Checked = false;
            if ((record.БитыНештатныхСитуаций & 0x01) == 0x01) cBПоездОтменен.Checked = true;
            else if ((record.БитыНештатныхСитуаций & 0x02) == 0x02) cBПрибытиеЗадерживается.Checked = true;
            else if ((record.БитыНештатныхСитуаций & 0x04) == 0x04) cBОтправлениеЗадерживается.Checked = true;
            else if ((record.БитыНештатныхСитуаций & 0x08) == 0x08) cBОтправлениеПоГотовности.Checked = true;
            else if ((record.БитыНештатныхСитуаций & 0x10) == 0x10) cbLandingDelay.Checked = true;

            cBПрибытие.Checked = ((record.БитыАктивностиПолей & 0x04) != 0x00) ? true : false;
            cBОтправление.Checked = ((record.БитыАктивностиПолей & 0x10) != 0x00) ? true : false;

            dTP_Прибытие.Enabled = cBПрибытие.Checked;
            btn_ИзменитьВремяПрибытия.Enabled = cBПрибытие.Checked;

            dTP_ВремяОтправления.Enabled = cBОтправление.Checked;
            btn_ИзменитьВремяОтправления.Enabled = cBОтправление.Checked;

            groupBox1.Enabled = (record.ТипПоезда == ТипПоезда.Пригородный) || (record.ТипПоезда == ТипПоезда.Ласточка) || (record.ТипПоезда == ТипПоезда.РЭКС);   //разблокируем только для пригорода

            cB_НомерПути.Items.Clear();
            cB_НомерПути.Items.Add("Не определен");
            var paths = НомераПутей.Select(p => p.Name).ToList();
            foreach (var путь in paths)
                cB_НомерПути.Items.Add(путь);


            cB_НомерПути.SelectedIndex = paths.IndexOf(record.НомерПути) + 1;
            



            dTP_Прибытие.Value = record.ВремяПрибытия;
            dTP_ВремяОтправления.Value = record.ВремяОтправления;
            dTP_Задержка.Value = record.ВремяЗадержки ?? DateTime.Parse("00:00");

            dTP_ОжидаемоеВремя.Value = record.ОжидаемоеВремя; // Заменил: было record.Время (непонятно почему)

            dTP_ВремяВПути.Value = (record.ВремяСледования.HasValue) ? record.ВремяСледования.Value : DateTime.Parse("00:00");

            switch (record.НумерацияПоезда)
            {
                case 0: rB_Нумерация_Отсутствует.Checked = true; break;
                case 1: rB_Нумерация_СГоловы.Checked = true; break;
                case 2: rB_Нумерация_СХвоста.Checked = true; break;
            }


            //контроллы для ТРАНЗИТОВ
            if (record.БитыАктивностиПолей == 31)
            {
                chbox_сменнаяНумерация.Checked = record.СменнаяНумерацияПоезда;
                cb_ВремяСтоянкиБудетИзмененно.Checked = (record.ВремяСтоянки == null);
            }
            else
            {
                chbox_сменнаяНумерация.Enabled = false;
                cb_ВремяСтоянкиБудетИзмененно.Enabled = false;
            }

    
            tb_Дополнение.Text = record.Дополнение;
            tbAdditionEng.Text = record.AdditionEng;
            cb_Дополнение_Звук.Checked = record.ИспользоватьДополнение != null && record.ИспользоватьДополнение["звук"];
            cb_Дополнение_Табло.Checked = record.ИспользоватьДополнение != null && record.ИспользоватьДополнение["табло"];

            ОбновитьТекстВОкне();

            string Text = "Карточка ";
            switch (record.ТипПоезда)
            {
                case ТипПоезда.Пассажирский: Text += "пассажирского поезда: "; break;
                case ТипПоезда.Пригородный: Text += "пригородного электропоезда: "; break;
                case ТипПоезда.Скоростной: Text += "скоростного поезда: "; break;
                case ТипПоезда.Скорый: Text += "скорого поезда: "; break;
                case ТипПоезда.Ласточка: Text += "скоростного поезда Ласточка: "; break;
                case ТипПоезда.РЭКС: Text += "скоростного поезда РЭКС: "; break;
                case ТипПоезда.Фирменный: Text += "фирменного поезда: "; break;
            }
            Text += record.НомерПоезда + ": " + record.СтанцияОтправления + " - " + record.СтанцияНазначения;
            this.Text = Text;

            txb_НомерПоезда.Text = record.НомерПоезда;
            txb_НомерПоезда2.Text = record.НомерПоезда2;


            var directions = Program.DirectionRepository.List().ToList();
            if (directions.Any())
            {
                var stationsNames = directions.FirstOrDefault(d => d.Name == record.Направление)?.Stations?.Select(st => st.NameRu).ToArray();
                if (stationsNames != null && stationsNames.Any())
                {
                    cBОткуда.Items.Clear();
                    cBКуда.Items.Clear();
                    cBОткуда.Items.AddRange(stationsNames);
                    cBКуда.Items.AddRange(stationsNames);
                }
            }

            cBОткуда.Text = record.СтанцияОтправления;
            cBКуда.Text = record.СтанцияНазначения;


            switch (record.КоличествоПовторений)
            {
                default:
                case 1:
                    btnПовторения.Text = "1 ПОВТОР";
                    break;

                case 2:
                    btnПовторения.Text = "2 ПОВТОРА";
                    break;

                case 3:
                    btnПовторения.Text = "3 ПОВТОРА";
                    break;
            };

            lVШаблоны.Items.Clear();
            for (int i = 0; i < record.СписокФормируемыхСообщений.Count(); i++)
            {
                var формируемоеСообщение = record.СписокФормируемыхСообщений[i];

                var времяАктивации = формируемоеСообщение.ПривязкаКВремени == 0 ? record.ВремяПрибытия.AddMinutes(формируемоеСообщение.ВремяСмещения) :
                                                                                  record.ВремяОтправления.AddMinutes(формируемоеСообщение.ВремяСмещения);

                string языки = String.Empty;
                формируемоеСообщение.ЯзыкиОповещения.ForEach(lang => языки += lang.ToString() + ", ");
                языки = языки.Remove(языки.Length - 2, 2);

                var variant = Enum.GetValues(typeof(PriorityPrecise)).Cast<PriorityPrecise>().ToList();
                var priorntyNum = variant.IndexOf(формируемоеСообщение.ПриоритетВторостепенный).ToString();
               


                ListViewItem lvi = new ListViewItem(new string[] { времяАктивации.ToString("HH:mm"), формируемоеСообщение.НазваниеШаблона, языки, priorntyNum });
                lvi.Checked = формируемоеСообщение.Активность;
                lvi.Tag = i;

                lVШаблоны.Items.Add(lvi);

                gBНастройкиПоезда.Enabled = record.Активность;
            }

            if (record.Автомат)
            {
                btn_Автомат.Text = "АВТОМАТ";
                btn_Автомат.BackColor = Color.Aquamarine;
                btn_Фиксировать.Enabled = false;
            }
            else
            {
                btn_Автомат.Text = "РУЧНОЙ";
                btn_Автомат.BackColor = Color.DarkSlateBlue;
                btn_Фиксировать.Enabled = true;
            }

            lb_фиксВрПриб.Text = record.ФиксированноеВремяПрибытия == null ? "--:--" : record.ФиксированноеВремяПрибытия.Value.ToString("t");
            lb_фиксВрОтпр.Text = record.ФиксированноеВремяОтправления == null ? "--:--" : record.ФиксированноеВремяОтправления.Value.ToString("t");
            lb_фиксВрПриб.BackColor = record.ФиксированноеВремяПрибытия == null ? Color.Empty : Color.Aqua;
            lb_фиксВрОтпр.BackColor = record.ФиксированноеВремяОтправления == null ? Color.Empty : Color.Aqua;

            chBoxВыводНаТабло.Checked = record.ВыводНаТабло;
            chBoxВыводЗвука.Checked = record.ВыводЗвука;
            
            ChangeBlock(Program.MainWindowWorkMode != MainWindowWorkMode.OnlyDispatcher);

            _composition = record.Composition;
            if (_composition != null)
            {
                DisplayComposition(_composition);
            }
            else
            {
                RemoveComposition();
            }
        }

        private void ChangeBlock(bool isEnabled)
        {
            cB_НомерПути.Enabled = isEnabled;
            dTP_Прибытие.Enabled = isEnabled;
            dTP_ВремяОтправления.Enabled = isEnabled;
            cBПрибытие.Enabled = isEnabled;
            cBОтправление.Enabled = isEnabled;
            btn_ИзменитьВремяОтправления.Enabled = isEnabled;
            btn_ИзменитьВремяПрибытия.Enabled = isEnabled;
            cBПрибытиеЗадерживается.Enabled = isEnabled;
            cbLandingDelay.Enabled = isEnabled;
            cBОтправлениеЗадерживается.Enabled = isEnabled;
            cBОтправлениеПоГотовности.Enabled = isEnabled;

            // Блокируем изменение времени задержки, если не включена какая-либо нештатка
            isEnabled = isEnabled && (cBПрибытиеЗадерживается.Checked || cbLandingDelay.Checked || cBОтправлениеЗадерживается.Checked  || cBОтправлениеПоГотовности.Checked);
            dTP_Задержка.Enabled = isEnabled;
            dTP_ОжидаемоеВремя.Enabled = isEnabled;
            btn_ИзменитьВремяЗадержки.Enabled = isEnabled;
        }

        private void ОбновитьТекстВОкне()
        {
            if (_record.ТипСообщения == SoundRecordType.Обычное)
            {
                string ПутьКФайлу = Path.GetFileNameWithoutExtension(_record.ИменаФайлов[0]);
                rTB_Сообщение.Text = "Звуковой трек: " + ПутьКФайлу;
                rTB_Сообщение.SelectionStart = 15;
                rTB_Сообщение.SelectionLength = ПутьКФайлу.Length;
                rTB_Сообщение.SelectionColor = Color.DarkGreen;
                rTB_Сообщение.SelectionLength = 0;
            }
            else if ((_record.ТипСообщения == SoundRecordType.ДвижениеПоезда) || (_record.ТипСообщения == SoundRecordType.ДвижениеПоездаНеПодтвержденное))
            {
                #region Движение по станциям
                lB_ПоСтанциям.Items.Clear();
                rB_ПоРасписанию.Checked = false;
                rB_ПоСтанциям.Checked = false;
                rB_КромеСтанций.Checked = false;
                rB_СоВсемиОстановками.Checked = false;

                if ((this._record.ТипПоезда == ТипПоезда.Пригородный) || (this._record.ТипПоезда == ТипПоезда.Ласточка) || (this._record.ТипПоезда == ТипПоезда.РЭКС))
                {
                    string Примечание = this._record.Примечание;
                    var списокСтанцийParse = Примечание.Substring(Примечание.IndexOf(":", StringComparison.Ordinal) + 1).Split(',').Select(st => st.Trim()).ToList();

                    if (Примечание.Contains("С остановк"))
                    {
                        rB_ПоСтанциям.Checked = true;
                        foreach (var станция in СтанцииВыбранногоНаправления)
                        {
                            if (списокСтанцийParse.Contains(станция))
                                lB_ПоСтанциям.Items.Add(станция);
                        }

                        lB_ПоСтанциям.Enabled = true;
                        btnРедактировать.Enabled = true;
                    }
                    else if (Примечание.Contains("Со всеми остановками"))
                    {
                        rB_СоВсемиОстановками.Checked = true;
                        foreach (var станция in СтанцииВыбранногоНаправления)
                            lB_ПоСтанциям.Items.Add(станция);

                        lB_ПоСтанциям.Enabled = true;
                        btnРедактировать.Enabled = true;
                    }
                    else if (Примечание.Contains("Кроме"))
                    {
                        rB_КромеСтанций.Checked = true;
                        foreach (var станция in СтанцииВыбранногоНаправления)
                        {
                            if (списокСтанцийParse.Contains(станция))
                                lB_ПоСтанциям.Items.Add(станция);
                        }

                        lB_ПоСтанциям.Enabled = true;
                        btnРедактировать.Enabled = true;
                    }
                    else
                    {
                        rB_ПоРасписанию.Checked = true;
                        lB_ПоСтанциям.Enabled = false;
                        btnРедактировать.Enabled = false;
                    }
                }
                #endregion
            }

            var время = cBПрибытие.Checked && !(cBОтправлениеЗадерживается.Checked || cBОтправлениеПоГотовности.Checked || cbLandingDelay.Checked) ? 
                        _record.ВремяПрибытия : _record.ВремяОтправления;
            if (cBОтправлениеЗадерживается.Checked || cBПрибытиеЗадерживается.Checked)
            {
                dTP_Задержка.Enabled = true;
                dTP_ОжидаемоеВремя.Enabled = true;
                btn_ИзменитьВремяЗадержки.Enabled = true;
                dTP_Задержка.Value = (_record.ВремяЗадержки == null) ? DateTime.Parse("00:00") : _record.ВремяЗадержки.Value;
                
                dTP_ОжидаемоеВремя.Value = (_record.ВремяЗадержки == null) ? время : время.AddHours(_record.ВремяЗадержки.Value.Minute).AddMinutes(_record.ВремяЗадержки.Value.Second);;
            }
            else
            {
                dTP_Задержка.Enabled = false;
                dTP_ОжидаемоеВремя.Enabled = false;
                btn_ИзменитьВремяЗадержки.Enabled = false;

                dTP_Задержка.Value= DateTime.Parse("00:00");
                dTP_ОжидаемоеВремя.Value = время;
            }

            ChangeBlock(Program.MainWindowWorkMode != MainWindowWorkMode.OnlyDispatcher);


            //Обновить список табло
            comboBox_displayTable.Items.Clear();
            comboBox_displayTable.SelectedIndex = -1;
            if (_record.НазванияТабло != null && _record.НазванияТабло.Any())
            {
                foreach (var table in _record.НазванияТабло)
                {
                    comboBox_displayTable.Items.Add(table);
                }

                comboBox_displayTable.BackColor = Color.White;
            }
            else
            {
                comboBox_displayTable.BackColor = Color.DarkRed;
            }
        }


        private void ОбновитьСостояниеТаблицыШаблонов()
        {
            for (int item = 0; item < this.lVШаблоны.Items.Count; item++)
            {
                if (item <= _record.СписокФормируемыхСообщений.Count)
                {
                    var формируемоеСообщение = _record.СписокФормируемыхСообщений[item];

                    var активность = lVШаблоны.Items[item].Checked;

                    var ручноШаблон = формируемоеСообщение.НазваниеШаблона.StartsWith("@");
                    //var времяПриб = (_record.ФиксированноеВремяПрибытия == null || !ручноШаблон) ? _record.ВремяПрибытия : _record.ФиксированноеВремяПрибытия.Value;
                    //var времяОтпр = (_record.ФиксированноеВремяОтправления == null || !ручноШаблон) ? _record.ВремяОтправления : _record.ФиксированноеВремяОтправления.Value;
                    var времяПриб = (_record.ФиксированноеВремяПрибытия == null || !ручноШаблон) ? _record.ActualArrivalTime : _record.ФиксированноеВремяПрибытия.Value;
                    var времяОтпр = (_record.ФиксированноеВремяОтправления == null || !ручноШаблон) ? _record.ActualDepartureTime : _record.ФиксированноеВремяОтправления.Value;
                    var time = формируемоеСообщение.ПривязкаКВремени == 0 ? времяПриб : времяОтпр;

                    var времяАктивации = DateTime.Parse("00:00");
                    if (time != DateTime.MinValue || формируемоеСообщение.ВремяСмещения > 0)
                        времяАктивации = time.AddMinutes(формируемоеСообщение.ВремяСмещения);

                    string текстовоеПредставлениеВремениАктивации = времяАктивации.ToString("HH:mm");

                    if (this.lVШаблоны.Items[item].Text != текстовоеПредставлениеВремениАктивации)
                        this.lVШаблоны.Items[item].Text = текстовоеПредставлениеВремениАктивации;

                    if (формируемоеСообщение.Воспроизведен == true)
                        this.lVШаблоны.Items[item].BackColor = Color.LightGray;
                    else
                    {
                        this.lVШаблоны.Items[item].BackColor = активность ? Color.LightGreen : Color.White;
                    }


                    if (chbox_сменнаяНумерация.Checked)
                    {
                        if (формируемоеСообщение.НазваниеШаблона.StartsWith("[ПРИБ]") ||
                            формируемоеСообщение.НазваниеШаблона.StartsWith("[ОТПР]"))
                        {
                            if (формируемоеСообщение.Воспроизведен == true)
                                this.lVШаблоны.Items[item].BackColor = Color.LightGray;
                            else
                            {
                                this.lVШаблоны.Items[item].BackColor = активность ? Color.CornflowerBlue : Color.White;
                            }
                        }
                    }
                }
            }
        }


        public void ОтобразитьШаблонОповещенияНаRichTb(string шаблонОповещения, ref СостояниеФормируемогоСообщенияИШаблон? сообшение, RichTextBox rTb)
        {
            var option = new Dictionary<string, dynamic>
            {
                {"формируемоеСообщение", сообшение }
            };
            var record = _record;
            var soundRecordPreprocessingService = PreprocessingOutputFactory.CreateSoundRecordPreprocessingService(option);
            soundRecordPreprocessingService.StartPreprocessing(ref record);


            rTb.Text = "";
            string Text;

            string[] НазваниеФайловНумерацииПутей = new string[] { "", "Нумерация поезда с головы состава", "Нумерация поезда с хвоста состава" };

            List<int> УказательВыделенныхФрагментов = new List<int>();

            string[] ЭлементыШаблона = шаблонОповещения.Split('|');
            foreach (string шаблон in ЭлементыШаблона)
            {
                string текстПодстановки = String.Empty;
                Pathways путь;
                switch (шаблон)
                {
                    case "НА НОМЕР ПУТЬ":
                    case "НА НОМЕРом ПУТИ":
                    case "С НОМЕРого ПУТИ":
                        путь = НомераПутей.FirstOrDefault(p => p.Name == record.НомерПути);
                        if (путь == null)
                            break;
                        if (шаблон == "НА НОМЕР ПУТЬ") текстПодстановки = путь.НаНомерПуть;
                        if (шаблон == "НА НОМЕРом ПУТИ") текстПодстановки = путь.НаНомерОмПути;
                        if (шаблон == "С НОМЕРого ПУТИ") текстПодстановки = путь.СНомерОгоПути;

                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = текстПодстановки;
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ПУТЬ ДОПОЛНЕНИЕ":
                        путь = НомераПутей.FirstOrDefault(p => p.Name == record.НомерПути);
                        текстПодстановки = путь?.Addition ?? string.Empty;
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = текстПодстановки;
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "СТ.ОТПРАВЛЕНИЯ":
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = record.СтанцияОтправления;
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "НОМЕР ПОЕЗДА":
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = record.НомерПоезда;
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "НОМЕР ПОЕЗДА ТРАНЗИТ ОТПР":
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = record.НомерПоезда2;
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ДОПОЛНЕНИЕ":
                        if (string.IsNullOrEmpty(record.Дополнение))
                            break;
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = record.Дополнение;
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "СТ.ПРИБЫТИЯ":
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = record.СтанцияНазначения;
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ВРЕМЯ ПРИБЫТИЯ":
                        rTb.Text += "Время прибытия: ";
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = record.ВремяПрибытия.ToString("HH:mm");
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ВРЕМЯ ПРИБЫТИЯ UTC":
                        rTb.Text += "Время прибытия UTC: ";
                        var времяUtc = record.ВремяПрибытия.AddMinutes(Program.Настройки.UTC);
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = времяUtc.ToString("HH:mm");
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ВРЕМЯ СТОЯНКИ":
                        rTb.Text += "Стоянка: ";
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = string.Empty;
                        if (record.ВремяСтоянки.HasValue)
                        {
                            //Text = (record.ВремяСтоянки.Value.Hours.ToString("D2") + ":" + record.ВремяСтоянки.Value.Minutes.ToString("D2"));
                            Text = (record.ВремяСтоянки.Value.ToString("hh\\:mm"));
                        }
                        else
                        if (record.БитыАктивностиПолей == 31)
                        {
                            Text = "Время стоянки будет измененно";
                        }
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ВРЕМЯ ОТПРАВЛЕНИЯ":
                        rTb.Text += "Время отправления: ";
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = record.ВремяОтправления.ToString("HH:mm");
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ВРЕМЯ ОТПРАВЛЕНИЯ UTC":
                        rTb.Text += "Время отправления UTC: ";
                        времяUtc = record.ВремяОтправления.AddMinutes(Program.Настройки.UTC);
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = времяUtc.ToString("HH:mm");
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ВРЕМЯ ЗАДЕРЖКИ":
                        rTb.Text += "Время задержки: ";
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = (record.ВремяЗадержки == null) ? "00:00" : record.ВремяЗадержки.Value.ToString("mm:ss");
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;

                    case "ОЖИДАЕМОЕ ВРЕМЯ":
                        rTb.Text += "Ожидаемое время: ";
                        УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                        Text = record.ОжидаемоеВремя.ToString("HH:mm");
                        УказательВыделенныхФрагментов.Add(Text.Length);
                        rTb.AppendText(Text + " ");
                        break;


                    case "НУМЕРАЦИЯ СОСТАВА":
                        if ((record.НумерацияПоезда > 0) && (record.НумерацияПоезда <= 2))
                        {
                            УказательВыделенныхФрагментов.Add(rTb.Text.Length);
                            Text = НазваниеФайловНумерацииПутей[record.НумерацияПоезда];
                            УказательВыделенныхФрагментов.Add(Text.Length);
                            rTb.AppendText(Text + " ");
                        }
                        break;


                    case "СТАНЦИИ":
                        if ((record.ТипПоезда == ТипПоезда.Пригородный) || (record.ТипПоезда == ТипПоезда.Ласточка) || (record.ТипПоезда == ТипПоезда.РЭКС))
                        {
                            if (rB_СоВсемиОстановками.Checked == true)
                            {
                                rTb.AppendText("Электропоезд движется со всеми остановками");
                            }
                            else if (rB_ПоСтанциям.Checked == true)
                            {
                                rTb.AppendText("Электропоезд движется с остановками на станциях: ");
                                foreach (var станция in СтанцииВыбранногоНаправления)
                                    if (lB_ПоСтанциям.Items.Contains(станция))
                                    {
                                        rTb.AppendText(станция + " ");
                                    }
                            }
                            else if (rB_КромеСтанций.Checked == true)
                            {
                                rTb.AppendText("Электропоезд движется с остановками кроме станций: ");
                                foreach (var станция in СтанцииВыбранногоНаправления)
                                    if (lB_ПоСтанциям.Items.Contains(станция))
                                    {
                                        rTb.AppendText(станция + " ");
                                    }
                            }
                        }
                        break;


                    default:
                        rTb.AppendText(шаблон + " ");
                        break;
                }
            }

            for (int i = 0; i < УказательВыделенныхФрагментов.Count / 2; i++)
            {
                rTb.SelectionStart = УказательВыделенныхФрагментов[2 * i];
                rTb.SelectionLength = УказательВыделенныхФрагментов[2 * i + 1];
                rTb.SelectionColor = Color.Red;
            }

            rTb.SelectionLength = 0;
        }


        private void СброситьФиксированноеВремяВШаблонах()
        {
            _record.ФиксированноеВремяПрибытия = null;
            _record.ФиксированноеВремяОтправления = null;
            lb_фиксВрПриб.Text = @"--:--";
            lb_фиксВрОтпр.Text = @"--:--";
            lb_фиксВрПриб.BackColor = Color.Empty;
            lb_фиксВрОтпр.BackColor = Color.Empty;
        }



        private void ДобавитьШаблонВОчередьЗвуковыхСообщений(int? привязкаКоВремени)
        {
            for (int i = 0; i < _record.СписокФормируемыхСообщений.Count; i++)
            {
                var формируемоеСообщение = _record.СписокФормируемыхСообщений[i];
                if (привязкаКоВремени != null &&
                    формируемоеСообщение.ПривязкаКВремени != привязкаКоВремени.Value)
                {
                    continue;
                }

                if (формируемоеСообщение.НазваниеШаблона.StartsWith("@") && формируемоеСообщение.ВремяСмещения == 0)
                {
                    формируемоеСообщение.Воспроизведен = true;
                    формируемоеСообщение.СостояниеВоспроизведения = SoundRecordStatus.ДобавленВОчередьРучное;
                    формируемоеСообщение.ПриоритетГлавный = Priority.Hight;
                    _record.СписокФормируемыхСообщений[i] = формируемоеСообщение;

                    MainWindowForm.SoundManager.ВоспроизвестиШаблонОповещения("Воспроизведение шаблона в ручном режиме при фиксации времени", _record, формируемоеСообщение, ТипСообщения.Динамическое);
                }
            }
        }


        /// <summary>
        /// Применить изменения
        /// </summary>
        private void ApplyChange()
        {
            bool ПерваяСтанция = true;
            string Примечание = "";
            string NoteEng = "";

            _record.НомерПоезда = txb_НомерПоезда.Text;
            _record.НомерПоезда2 = txb_НомерПоезда2.Text;

            _record.СменнаяНумерацияПоезда = chbox_сменнаяНумерация.Checked;
            _record.НумерацияПоезда = rB_Нумерация_СГоловы.Checked ? (byte)1 : rB_Нумерация_СХвоста.Checked ? (byte)2 : (byte)0;


            _record.ВыводНаТабло = chBoxВыводНаТабло.Checked;
            _record.ВыводЗвука = chBoxВыводЗвука.Checked;

            if (rB_СоВсемиОстановками.Checked == true)
            {
                Примечание = "Со всеми остановками";
                NoteEng = "With all stops";
            }
            else if (rB_ПоСтанциям.Checked == true)
            {
                Примечание = "С остановками: ";
                NoteEng = "With stops: ";

                foreach (var станция in СтанцииВыбранногоНаправления)
                    if (lB_ПоСтанциям.Items.Contains(станция))
                    {
                        if (ПерваяСтанция == true)
                            ПерваяСтанция = false;
                        else
                        {
                            Примечание += ", ";
                            NoteEng += ", ";
                        }

                        Примечание += станция;

                        var station = Program.DirectionRepository.GetByName(_record.Направление)?.GetStationInDirectionByName(станция);
                        var nameEng = station?.NameEng ?? "";
                        NoteEng += (!string.IsNullOrWhiteSpace(nameEng) ? nameEng : станция);
                    }
            }
            else if (rB_КромеСтанций.Checked == true)
            {
                Примечание = "Кроме: ";
                NoteEng = "Except: ";
                foreach (var станция in СтанцииВыбранногоНаправления)
                    if (lB_ПоСтанциям.Items.Contains(станция))
                    {
                        if (ПерваяСтанция == true)
                            ПерваяСтанция = false;
                        else
                        {
                            Примечание += ", ";
                            NoteEng += ", ";
                        }

                        Примечание += станция;

                        var station = Program.DirectionRepository.GetByName(_record.Направление)?.GetStationInDirectionByName(станция);
                        var nameEng = station?.NameEng ?? "";
                        NoteEng += (!string.IsNullOrWhiteSpace(nameEng) ? nameEng : станция);
                    }
            }
            _record.Примечание = Примечание;
            _record.NoteEng = NoteEng;

            _record.СтанцияОтправления = cBОткуда.Text;
            _record.СтанцияНазначения = cBКуда.Text;

            _record.Дополнение = tb_Дополнение.Text;
            _record.AdditionEng = tbAdditionEng.Text;
            _record.ИспользоватьДополнение["звук"] = cb_Дополнение_Звук.Checked;
            _record.ИспользоватьДополнение["табло"] = cb_Дополнение_Табло.Checked;

            _record.НазваниеПоезда = _record.СтанцияОтправления == "" ? _record.СтанцияНазначения : _record.СтанцияОтправления + " - " + _record.СтанцияНазначения;


            //корректировка суток--------------------------------
            //выставили время на СЛЕД сутки
            if ((_recordOld.ВремяОтправления - _record.ВремяОтправления).Hours > 12)
            {
                _record.ВремяОтправления = _record.ВремяОтправления.AddDays(1);
            }
            if ((_recordOld.ВремяПрибытия - _record.ВремяПрибытия).Hours > 12)
            {
                _record.ВремяПрибытия = _record.ВремяПрибытия.AddDays(1);
            }

            //выставили время на ПРЕД сутки
            if ((_record.ВремяОтправления - _recordOld.ВремяОтправления).Hours > 12)
            {
                _record.ВремяОтправления = _record.ВремяОтправления.AddDays(-1);
            }
            if ((_record.ВремяПрибытия - _recordOld.ВремяПрибытия).Hours > 12)
            {
                _record.ВремяПрибытия = _record.ВремяПрибытия.AddDays(-1);
            }



            //Применение битов нештатных ситуаций------------------------------
            _record.БитыНештатныхСитуаций &= 0x00;
            if (cBПоездОтменен.Checked)
            {
                _record.БитыНештатныхСитуаций |= 0x01;
            }
            else
            if (cBПрибытиеЗадерживается.Checked)
            {
                _record.БитыНештатныхСитуаций |= 0x02;
            }
            else
            if (cBОтправлениеЗадерживается.Checked)
            {
                _record.БитыНештатныхСитуаций |= 0x04;
            }
            else
            if (cBОтправлениеПоГотовности.Checked)
            {
                _record.БитыНештатныхСитуаций |= 0x08;
            }
            else
            if (cbLandingDelay.Checked)
            {
                _record.БитыНештатныхСитуаций |= 0x10;
            }


            //Время стоянки для транзитов----------------------------------------
            _record.ВремяСтоянки = (TimeSpan?)(cBПрибытие.Checked && cBОтправление.Checked && !cb_ВремяСтоянкиБудетИзмененно.Checked ?
                                        _record.ВремяОтправления < _record.ВремяПрибытия ? 
                                        _record.ВремяОтправления.AddDays(1) - _record.ВремяПрибытия : 
                                        _record.ВремяОтправления - _record.ВремяПрибытия :
                                   (ValueType)null);

            //если полле ввода времени задержки неактивно, то
            //if (!dTP_Задержка.Enabled)
            //{
                _record.ВремяЗадержки = dTP_Задержка.Value;

                _record.ОжидаемоеВремя = dTP_ОжидаемоеВремя.Value;
            /*if (delay.Hour == 0 && delay.Minute == 0)
            {
                cBПрибытиеЗадерживается.Checked = false;
                cBОтправлениеЗадерживается.Checked = false;
                _record.БитыНештатныхСитуаций = 0x00;
            }*/
            //}

            if (cBПрибытиеЗадерживается.Checked)
            {
                _record.ActualArrivalTime = _record.ОжидаемоеВремя != _record.ВремяПрибытия ? _record.ОжидаемоеВремя : _record.ОжидаемоеВремя.AddDays(1);
                _record.ActualDepartureTime = _record.ActualArrivalTime + (_record.ВремяОтправления - _record.ВремяПрибытия);
            }
            else
            {
                _record.ActualArrivalTime = _record.ВремяПрибытия;
                _record.ActualDepartureTime = cBОтправлениеЗадерживается.Checked || cBОтправлениеПоГотовности.Checked || cbLandingDelay.Checked ? 
                                              _record.ОжидаемоеВремя != _record.ВремяОтправления ? _record.ОжидаемоеВремя : _record.ОжидаемоеВремя.AddDays(1) : 
                                              _record.ВремяОтправления;
            }

            //Применение активности шаблонов--------------------------------------
            for (int i = 0; i < this.lVШаблоны.Items.Count; i++)
            {
                if (i <= _record.СписокФормируемыхСообщений.Count)
                {
                    var формируемоеСообщение = _record.СписокФормируемыхСообщений[i];
                    формируемоеСообщение.Активность = this.lVШаблоны.Items[i].Checked;
                    _record.СписокФормируемыхСообщений[i] = формируемоеСообщение;
                }
            }

            _record.Composition = _composition;

            _record.AplyIdTrain();
        }


        public SoundRecord ПолучитьИзмененнуюКарточку()
        {
            return _record;
        }

        public void DisplaySectors(Pathways track, Composition composition = null)
        {
            sp?.Display(track, composition != null ? composition : _record.Composition);
        }

        public void RemoveSectors()
        {
            sp?.Remove();
        }

        public void DisplayComposition(Composition composition)
        {
            cp?.Display(composition);
        }

        public void RemoveComposition()
        {
            cp?.Remove();
        }

        private Pathways getTrackByName(string name)
        {
            return НомераПутей.FirstOrDefault(t => t.Name == _record.НомерПути);
        }

        #endregion





        #region EventHandler

        private void cB_НомерПути_SelectedIndexChanged(object sender, EventArgs e)
        {
            int номерПути = cB_НомерПути.SelectedIndex;
            //_record.Track = cB_НомерПути.SelectedIndex == 0 ? null : (Pathways)cB_НомерПути.SelectedItem;
            _record.НомерПути = cB_НомерПути.SelectedIndex != 0 ? cB_НомерПути.Text : string.Empty;
            _record.НомерПутиБезАвтосброса = _record.НомерПути;
            _record.НазванияТабло = номерПути != 0 ? MainWindowForm.BoardManager.Binding2PathBehaviors.Select(beh => beh.GetDevicesName4Path(_record.НомерПути)).Where(str => str != null).ToArray() : null;
            ОбновитьТекстВОкне();
            if (_разрешениеИзменений == true) _сделаныИзменения = true;

            track = getTrackByName(_record.НомерПути);
            var sectors = track?.Platform?.Sectors;
            if (sectors == null || !sectors.Any())
            {
                RemoveSectors();
            }
            else
            {
                DisplaySectors(track);
            }
        }



        private void rB_Нумерация_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_Нумерация_Отсутствует.Checked)
                _record.НумерацияПоезда = 0;
            else if (rB_Нумерация_СГоловы.Checked)
                _record.НумерацияПоезда = 1;
            else if (rB_Нумерация_СХвоста.Checked)
                _record.НумерацияПоезда = 2;

            ОбновитьТекстВОкне();
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void btn_ИзменитьВремяПрибытия_Click(object sender, EventArgs e)
        {
            var oldArrivalTime = _record.ВремяПрибытия;
            _record.ВремяПрибытия = dTP_Прибытие.Value;
            
            if (cBПрибытиеЗадерживается.Checked)
            {
                _record.ОжидаемоеВремя += _record.ВремяПрибытия - oldArrivalTime;
                _record.ActualArrivalTime = _record.ОжидаемоеВремя;
            }
            else
            {
                _record.ActualArrivalTime = _record.ВремяПрибытия;
            }
            
            ОбновитьТекстВОкне();
            ОбновитьСостояниеТаблицыШаблонов();
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void btn_ИзменитьВремяОтправления_Click(object sender, EventArgs e)
        {
            var oldDepartureTime = _record.ВремяОтправления;
            _record.ВремяОтправления = dTP_ВремяОтправления.Value;

            if (cBОтправлениеЗадерживается.Checked)
            {
                _record.ОжидаемоеВремя += _record.ВремяОтправления - oldDepartureTime;
                _record.ActualDepartureTime = _record.ОжидаемоеВремя;
            }
            else
            {
                _record.ActualDepartureTime = _record.ВремяОтправления;
            }

            ОбновитьТекстВОкне();
            ОбновитьСостояниеТаблицыШаблонов();
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void btn_ИзменитьВремяЗадержки_Click(object sender, EventArgs e)
        {
            //не стоят обе галочки приб. и отпр.
            if (!(cBПрибытие.Checked || cBОтправление.Checked))
                return;

            _record.ВремяЗадержки = dTP_Задержка.Value;
            ОбновитьТекстВОкне();
            ОбновитьСостояниеТаблицыШаблонов();
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void btn_ИзменитьВремяВПути_Click(object sender, EventArgs e)
        {
            _record.ВремяСледования = dTP_ВремяВПути.Value;
            ОбновитьТекстВОкне();
            ОбновитьСостояниеТаблицыШаблонов();
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void button2_Click(object sender, EventArgs e)
        {
            string СписокВыбранныхСтанций = "";
            for (int i = 0; i < lB_ПоСтанциям.Items.Count; i++)
                СписокВыбранныхСтанций += lB_ПоСтанциям.Items[i] + ",";

            var direction = Program.DirectionRepository.List().FirstOrDefault(d => d.Name == _record.Направление);
            var станцииНаправления = direction?.Stations.Select(st => st.NameRu).ToArray();

            СписокСтанций списокСтанций = new СписокСтанций(СписокВыбранныхСтанций, станцииНаправления);

            if (списокСтанций.ShowDialog() == DialogResult.OK)
            {
                List<string> РезультирующиеСтанции = списокСтанций.ПолучитьСписокВыбранныхСтанций();
                lB_ПоСтанциям.Items.Clear();
                foreach (var res in РезультирующиеСтанции)
                    lB_ПоСтанциям.Items.Add(res);

                bool ПерваяСтанция = true;

                string Примечание = "";
                var NoteEng = "";

                if (rB_СоВсемиОстановками.Checked == true)
                {
                    Примечание = "Со всеми остановками";
                    NoteEng = "With all stops";
                }
                else if (rB_ПоСтанциям.Checked == true)
                {
                    Примечание = "С остановками: ";
                    NoteEng = "With stops: ";

                    foreach (var станция in СтанцииВыбранногоНаправления)
                        if (lB_ПоСтанциям.Items.Contains(станция))
                        {
                            if (ПерваяСтанция == true)
                                ПерваяСтанция = false;
                            else
                            {
                                Примечание += ", ";
                                NoteEng += ", ";
                            }

                            Примечание += станция;

                            var station = Program.DirectionRepository.GetByName(_record.Направление)?.GetStationInDirectionByName(станция);
                            var nameEng = station?.NameEng ?? "";
                            NoteEng += (!string.IsNullOrWhiteSpace(nameEng) ? nameEng : станция);
                        }
                }
                else if (rB_КромеСтанций.Checked == true)
                {
                    Примечание = "Кроме: ";
                    NoteEng = "Except: ";
                    foreach (var станция in СтанцииВыбранногоНаправления)
                        if (lB_ПоСтанциям.Items.Contains(станция))
                        {
                            if (ПерваяСтанция == true)
                                ПерваяСтанция = false;
                            else
                            {
                                Примечание += ", ";
                                NoteEng += ", ";
                            }

                            Примечание += станция;

                            var station = Program.DirectionRepository.GetByName(_record.Направление)?.GetStationInDirectionByName(станция);
                            var nameEng = station?.NameEng ?? "";
                            NoteEng += (!string.IsNullOrWhiteSpace(nameEng) ? nameEng : станция);
                        }
                }
                _record.Примечание = Примечание;
                _record.NoteEng = NoteEng;

                ОбновитьТекстВОкне();
                if (_разрешениеИзменений == true) _сделаныИзменения = true;
            }
        }



        private void rB_ПоСтанциям_CheckedChanged(object sender, EventArgs e)
        {
            if ((rB_ПоСтанциям.Checked == true) || (rB_КромеСтанций.Checked == true) || (rB_СоВсемиОстановками.Checked == true))
            {
                lB_ПоСтанциям.Enabled = true;
                btnРедактировать.Enabled = true;
            }
            else
            {
                lB_ПоСтанциям.Enabled = false;
                btnРедактировать.Enabled = false;
            }
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void btnПовторения_Click(object sender, EventArgs e)
        {
            if (btnПовторения.Text == "1 ПОВТОР")
            {
                btnПовторения.Text = "2 ПОВТОРА";
                _record.КоличествоПовторений = 2;
            }
            else if (btnПовторения.Text == "2 ПОВТОРА")
            {
                btnПовторения.Text = "3 ПОВТОРА";
                _record.КоличествоПовторений = 3;
            }
            else
            {
                btnПовторения.Text = "1 ПОВТОР";
                _record.КоличествоПовторений = 1;
            }
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void cBПрибытие_CheckedChanged(object sender, EventArgs e)
        {
            dTP_Прибытие.Enabled = cBПрибытие.Checked;
            btn_ИзменитьВремяПрибытия.Enabled = cBПрибытие.Checked;
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void cBОтправление_CheckedChanged(object sender, EventArgs e)
        {
            dTP_ВремяОтправления.Enabled = cBОтправление.Checked;
            btn_ИзменитьВремяОтправления.Enabled = cBОтправление.Checked;
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void lVШаблоны_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection sic = this.lVШаблоны.SelectedIndices;

            foreach (int item in sic)
            {
                string Key = this.lVШаблоны.Items[item].SubItems[1].Text;
                string Шаблон = "";

                bool наличиеШаблона = false;
                СостояниеФормируемогоСообщенияИШаблон? сообшение = null;
                foreach (var Item in DynamicSoundForm.DynamicSoundRecords)
                    if (Item.Name == Key)
                    {
                        наличиеШаблона = true;
                        Шаблон = Item.Message;
                        сообшение= _record.СписокФормируемыхСообщений.FirstOrDefault(t => t.НазваниеШаблона == Key);
                        break;
                    }

                if (наличиеШаблона == true)
                    ОтобразитьШаблонОповещенияНаRichTb(Шаблон, ref сообшение, rTB_Сообщение);
            }
        }



        private void lVШаблоны_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ОбновитьСостояниеТаблицыШаблонов();
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }



        private void btnВоспроизвестиВыбранныйШаблон_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_record.НомерПути))
            {
                if (MessageBox.Show($"Для поезда не выбран путь, сообщение будет воспроизведено без указания пути. Всё равно продолжить?",
                                $"Предупреждение",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            }

            ListView.SelectedIndexCollection sic = this.lVШаблоны.SelectedIndices;

            foreach (int item in sic)
            {
                int номерШаблона = (int)this.lVШаблоны.Items[item].Tag;
                if (номерШаблона < _record.СписокФормируемыхСообщений.Count())
                {
                    var формируемоеСообщение = _record.СписокФормируемыхСообщений[номерШаблона];
                    формируемоеСообщение.Воспроизведен = true;
                    формируемоеСообщение.СостояниеВоспроизведения = SoundRecordStatus.ДобавленВОчередьРучное;
                    формируемоеСообщение.ПриоритетГлавный = Priority.Hight;
                    _record.СписокФормируемыхСообщений[item] = формируемоеСообщение;

                    MainWindowForm.SoundManager.ВоспроизвестиШаблонОповещения("Действие оператора", _record, формируемоеСообщение, ТипСообщения.Динамическое);
                    break;
                }
            }

            ОбновитьСостояниеТаблицыШаблонов();
        }



        private void КарточкаДвиженияПоезда_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_сделаныИзменения == false)
                {
                    Close();
                }
                else
                {
                    DialogResult Результат = MessageBox.Show("Вы желаете сохранить изменения?", "Внимание !!!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (Результат == DialogResult.Yes)
                    {
                        ApplyChange();
                    }
                    else if (Результат == DialogResult.No)
                    {
                        Close();
                    }
                }
            }
        }



        private void КарточкаДвиженияПоезда_Load(object sender, EventArgs e)
        {
            _разрешениеИзменений = true;
        }



        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _record.Активность = !cBОтменен.Checked;
            _сделаныИзменения = true;
            gBНастройкиПоезда.Enabled = _record.Активность;
        }



        private void btnНештаткаПоезда_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите воспроизвести данное сообщение в эфир?", "Внимание !!!", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                string ФормируемоеСообщение = "";
                int ТипПоезда = (int)_record.ТипПоезда;

                switch ((sender as Button).Name)
                {
                    case "btnОтменаПоезда":
                        ФормируемоеСообщение = Program.ШаблонОповещенияОбОтменеПоезда[ТипПоезда];
                        break;

                    case "btnЗадержкаПрибытия":
                        ФормируемоеСообщение = Program.ШаблонОповещенияОЗадержкеПрибытияПоезда[ТипПоезда];
                        break;

                    case "btnLandingDelay":
                        ФормируемоеСообщение = Program.LandingDelaySoundTemplate[ТипПоезда];
                        break;

                    case "btnЗадержкаОтправления":
                        ФормируемоеСообщение = Program.ШаблонОповещенияОЗадержкеОтправленияПоезда[ТипПоезда];
                        break;

                    case "btnОтправлениеПоГотовности":
                        ФормируемоеСообщение = Program.ШаблонОповещенияООтправлениеПоГотовностиПоезда[ТипПоезда];
                        break;
                }

                bool НаличиеШаблона = false;
                foreach (var Item in DynamicSoundForm.DynamicSoundRecords)
                    if (Item.Name == ФормируемоеСообщение)
                    {
                        НаличиеШаблона = true;
                        ФормируемоеСообщение = Item.Message;
                        break;
                    }


                if (НаличиеШаблона == true)
                {
                    СостояниеФормируемогоСообщенияИШаблон шаблонФормируемогоСообщения = new СостояниеФормируемогоСообщенияИШаблон
                    {
                        Id = 2000,
                        ПриоритетГлавный = Priority.Hight,
                        SoundRecordId = _record.ID,
                        Шаблон = ФормируемоеСообщение,
                        ЯзыкиОповещения = new List<NotificationLanguage> { NotificationLanguage.Ru, NotificationLanguage.Eng }, //TODO: вычислять языки оповещения 
                        НазваниеШаблона = "Авария"
                    };
                    
                    MainWindowForm.SoundManager.ВоспроизвестиШаблонОповещения("Действие оператора нештатная ситуация", _record, шаблонФормируемогоСообщения, ТипСообщения.ДинамическоеАварийное);
                }
                else
                {
                    Log.log.Warn($"Шаблон {ФормируемоеСообщение} отсутствует");
                }
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }



        private void cBНештатки_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (((CheckBox) sender).Checked == true)
                {
                    switch (((CheckBox) sender).Name)
                    {
                        case "cBПоездОтменен":
                            if (cBПрибытиеЗадерживается.Checked)
                                cBПрибытиеЗадерживается.Checked = false;
                            if (cbLandingDelay.Checked)
                                cbLandingDelay.Checked = false;
                            if (cBОтправлениеЗадерживается.Checked)
                                cBОтправлениеЗадерживается.Checked = false;
                            if (cBОтправлениеПоГотовности.Checked)
                                cBОтправлениеПоГотовности.Checked = false;
                            break;

                        case "cBПрибытиеЗадерживается":
                            if (cBПоездОтменен.Checked)
                                cBПоездОтменен.Checked = false;
                            if (cbLandingDelay.Checked)
                                cbLandingDelay.Checked = false;
                            if (cBОтправлениеЗадерживается.Checked)
                                cBОтправлениеЗадерживается.Checked = false;
                            if (cBОтправлениеПоГотовности.Checked)
                                cBОтправлениеПоГотовности.Checked = false;
                            break;

                        case "cBОтправлениеЗадерживается":
                            if (cBПоездОтменен.Checked)
                                cBПоездОтменен.Checked = false;
                            if (cBПрибытиеЗадерживается.Checked)
                                cBПрибытиеЗадерживается.Checked = false;
                            if (cbLandingDelay.Checked)
                                cbLandingDelay.Checked = false;
                            if (cBОтправлениеПоГотовности.Checked)
                                cBОтправлениеПоГотовности.Checked = false;
                            break;

                        case "cBОтправлениеПоГотовности":
                            if (cBПоездОтменен.Checked)
                                cBПоездОтменен.Checked = false;
                            if (cBПрибытиеЗадерживается.Checked)
                                cBПрибытиеЗадерживается.Checked = false;
                            if (cbLandingDelay.Checked)
                                cbLandingDelay.Checked = false;
                            if (cBОтправлениеЗадерживается.Checked)
                                cBОтправлениеЗадерживается.Checked = false;
                            break;
                        case "cbLandingDelay":
                            if (cBПоездОтменен.Checked)
                                cBПоездОтменен.Checked = false;
                            if (cBПрибытиеЗадерживается.Checked)
                                cBПрибытиеЗадерживается.Checked = false;
                            if (cBОтправлениеЗадерживается.Checked)
                                cBОтправлениеЗадерживается.Checked = false;
                            if (cBОтправлениеПоГотовности.Checked)
                                cBОтправлениеПоГотовности.Checked = false;
                            break;

                    }
                    //ОбновитьТекстВОкне();
                }
                ОбновитьТекстВОкне();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }



        private void btn_Автомат_Click(object sender, EventArgs e)
        {
            if (this._record.Автомат)
            {
                this._record.Автомат = false;
                btn_Автомат.Text = "РУЧНОЙ";
                btn_Автомат.BackColor = Color.DarkSlateBlue;
                btn_Фиксировать.Enabled = true;
            }
            else
            {
                this._record.Автомат = true;
                btn_Автомат.Text = "АВТОМАТ";
                btn_Автомат.BackColor = Color.Aquamarine;
                btn_Фиксировать.Enabled = false;

                СброситьФиксированноеВремяВШаблонах();
                ОбновитьСостояниеТаблицыШаблонов();
            }
        }



        private void btn_Фиксировать_Click(object sender, EventArgs e)
        {
            DateTime текВремя = DateTime.Now;
            текВремя = текВремя.AddSeconds(-текВремя.Second);

            _record.ФиксированноеВремяПрибытия = текВремя;
            _record.ФиксированноеВремяОтправления = (_record.ВремяСтоянки != null) ? (текВремя + _record.ВремяСтоянки.Value) : текВремя;

            lb_фиксВрПриб.Text = _record.ФиксированноеВремяПрибытия.Value.ToString("t");
            lb_фиксВрОтпр.Text = _record.ФиксированноеВремяОтправления.Value.ToString("t");
            lb_фиксВрПриб.BackColor = Color.Aqua;
            lb_фиксВрОтпр.BackColor = Color.Aqua;

            int? привязкаКоВремени = null;
            if (_record.ФиксированноеВремяПрибытия == _record.ФиксированноеВремяОтправления)
            {
                привязкаКоВремени = null;     //шаблоны привязанные к ПРИБ и ОТПР с 0 смещением добавятся в очередь
            }
            else
            if (_record.ФиксированноеВремяПрибытия == текВремя)
            {
                привязкаКоВремени = 0;     //шаблоны привязанные к ПРИБ с 0 смещением добавятся в очередь
            }
            else
            if (_record.ФиксированноеВремяОтправления == текВремя)
            {
                привязкаКоВремени = 1;   //шаблоны привязанные к ОТПР с 0 смещением добавятся в очередь
            }

            ДобавитьШаблонВОчередьЗвуковыхСообщений(привязкаКоВремени);
            ОбновитьСостояниеТаблицыШаблонов();
        }



        /// <summary>
        /// раскрасить шаблоны, у которых префикс "[ПРИБ]" или "[ОТПР]"
        /// </summary>
        private void chbox_сменнаяНумерация_CheckedChanged(object sender, EventArgs e)
        {
            _record.СменнаяНумерацияПоезда = chbox_сменнаяНумерация.Checked;
            ОбновитьСостояниеТаблицыШаблонов();
        }



        private void cb_ВремяСтоянкиБудетИзмененно_CheckedChanged(object sender, EventArgs e)
        {
            //Время стоянки для транзитов----------------------------------------
            _record.ВремяСтоянки = (TimeSpan?)((!cb_ВремяСтоянкиБудетИзмененно.Checked) ? 
                                        _record.ВремяОтправления < _record.ВремяПрибытия ?
                                        _record.ВремяОтправления.AddDays(1) - _record.ВремяПрибытия :
                                        _record.ВремяОтправления - _record.ВремяПрибытия :
                                   (ValueType)null);
        }


        /// <summary>
        /// Уборали фокус с контрола задания времени ожидания
        /// </summary>
        private void dTP_ОжидаемоеВремя_Leave(object sender, EventArgs e)
        {
            var время = (cBПрибытие.Checked) ? _record.ВремяПрибытия : _record.ВремяОтправления;
            _record.ОжидаемоеВремя = dTP_ОжидаемоеВремя.Value;
            DateTime dt = DateTime.Now.Date;

            var differenceTime = _record.ОжидаемоеВремя - время;
            var newDelayTime = dt + differenceTime;

            //_record.ВремяЗадержки = newDelayTime;
            dTP_Задержка.Value = _record.ВремяЗадержки.Value;
        }


        private void btn_Ok_Click(object sender, EventArgs e)
        {
            ApplyChange();
            DialogResult = DialogResult.OK;
            this.Close();
        }


        private void btn_ПрименитьClick(object sender, EventArgs e)
        {
            ApplyChange();
        }


        private void btn_отмена_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void _record_CompositionChanged(Composition composition)
        {
            /*if (composition != null)
            {
                DisplayComposition(composition);
            }
            else
            {
                RemoveComposition();
            }*/
        }

        private void dTP_Задержка_ValueChanged(object sender, EventArgs e)
        {
            DateTime time;
            if (cBПрибытиеЗадерживается.Checked)
            {
                time = _record.ВремяПрибытия;
            }
            else if (cBОтправлениеЗадерживается.Checked)
            {
                time = _record.ВремяОтправления;
            }
            else
                return;

            _record.ВремяЗадержки = dTP_Задержка.Value;
            var delay = _record.ВремяЗадержки.Value;
            _record.ОжидаемоеВремя = time.AddHours(delay.Minute).AddMinutes(delay.Second);
            dTP_ОжидаемоеВремя.Value = _record.ОжидаемоеВремя;

            if (cBПрибытиеЗадерживается.Checked)
            {
                _record.ActualArrivalTime = _record.ОжидаемоеВремя;
                _record.ActualDepartureTime = _record.ActualArrivalTime + (_record.ВремяОтправления - _record.ВремяПрибытия);
            }
            else
            {
                _record.ActualArrivalTime = _record.ВремяПрибытия;
                _record.ActualDepartureTime = cBОтправлениеЗадерживается.Checked ? _record.ОжидаемоеВремя : _record.ВремяОтправления;
            }

            ОбновитьТекстВОкне();
            ОбновитьСостояниеТаблицыШаблонов();
            if (_разрешениеИзменений == true) _сделаныИзменения = true;
        }

        private void dTP_ОжидаемоеВремя_ValueChanged(object sender, EventArgs e)
        {/*
            DateTime time;
            if (cBПрибытие.Checked)
                time = _record.ВремяПрибытия;
            else if (cBОтправление.Checked)
                time = _record.ВремяОтправления;
            else
                return;

            _record.ОжидаемоеВремя = dTP_ОжидаемоеВремя.Value;

            var lateTimeMinutes = _record.ОжидаемоеВремя - time;
            var lateTime = new DateTime().AddHours(;
            dTP_Задержка.Value = */
        }

        #endregion

        class SectorsPanel : FlowLayoutPanel
        {
            private static SectorsPanel instance;

            private SectorsPanel(object sender)
            {
                var parent = sender as Panel;
                FlowDirection = FlowDirection.LeftToRight;
                WrapContents = false;
                Width = parent.Width;
                Height = parent.Height / 2;
            }

            public static SectorsPanel CreatePanel(object sender)
            {
                if (instance == null)
                    instance = new SectorsPanel(sender);
                return instance;
            }

            public void Display(Pathways track, Composition composition = null)
            {
                if (track == null || track.Platform == null || track.Platform.Sectors == null)
                    return;

                Controls.Clear();
                foreach (var sector in track?.Platform?.Sectors)
                {
                    Label lbl = new Label();
                    lbl.Text = sector.Name;
                    var length = composition == null ? track.Platform.Length : composition.Length;
                    lbl.Width = (int)(Width / (double)length * sector.Length);
                    lbl.Margin = new Padding(sector.Offset, 0, 0, 0);
                    try
                    {
                        int[] rgb = getRgbFromString(sector.Color);
                        if (rgb != null && rgb.Length == 3)
                            lbl.BackColor = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                    }
                    catch (Exception ex)
                    {
                        Log.log.Warn($"Задан неверный формат цвета сектора {sector.Name} платформы {track.Platform.Name} пути {track.Name}. Выставлен цвет по умолчанию. {ex.Message}");
                    }
                    Controls.Add(lbl);
                }
            }

            public void Remove()
            {
                Controls.Clear();
            }

            private int[] getRgbFromString(string color)
            {
                int[] array = null;

                if (color.StartsWith("#"))
                {
                    array = new int[3];
                    var s = color.Substring(1);
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = Convert.ToInt32(s.Substring(i * 2, 2), 16);
                    }
                }
                return array;
            }
        }

        class CompositionPanel : FlowLayoutPanel
        {
            private static CompositionPanel instance;


            private Label moveableLabel;                // Элемент, который требуется переместить
            private int moveableIndex;
            //private Point moveablePosition;
            //private Point mousePosition;
            //bool isDrag;
            private Label contextLabel;                 // Элемент, для которого вызвано контекстное меню
            private int contextIndex;                   // Индекс элемента, для которого вызвано контекстное меню

            private ContextMenuStrip contextMenu;       // Контекстное меню для вагонов
            private ToolStripMenuItem tsmInsert, tsmEdit, tsmCopy, tsmMove, tsmRemove;

            private Composition _composition;
            private КарточкаДвиженияПоезда _frm;

            private CompositionPanel(object sender, КарточкаДвиженияПоезда frm)
            {
                var parent = sender as Panel;
                FlowDirection = FlowDirection.LeftToRight;
                WrapContents = false;
                Width = parent.Width;
                Height = parent.Height;
                _frm = frm;

                contextMenu = new ContextMenuStrip();
                tsmInsert = new ToolStripMenuItem("Добавить вагон");
                tsmEdit = new ToolStripMenuItem("Редактировать данные вагона");
                tsmCopy = new ToolStripMenuItem("Копировать вагон");
                tsmMove = new ToolStripMenuItem("Переместить вагон");
                tsmRemove = new ToolStripMenuItem("Удалить вагон");
                contextMenu.Items.AddRange(new[] { tsmInsert, tsmEdit, tsmCopy, tsmMove, tsmRemove });
                ContextMenuStrip = contextMenu;

                tsmInsert.Click += Insert_Click;
                tsmEdit.Click += Edit_Click;
                tsmCopy.Click += Copy_Click;
                tsmMove.Click += Move_Click;
                tsmRemove.Click += Remove_Click;
            }

            public static CompositionPanel CreatePanel(object sender, КарточкаДвиженияПоезда frm)
            {
                if (instance == null)
                    instance = new CompositionPanel(sender, frm);
                return instance;
            }

            public void Display(Composition composition)
            {
                Controls.Clear();
                _composition = composition;
                if (_composition == null || _composition.Vagons == null)
                    return;

                foreach (var vagon in _composition?.Vagons)
                {
                    Label lbl = new Label();
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.Text = vagon.PsType != PsType.Locomotive ? vagon.VagonNumber.ToString() : "лок";
                    lbl.Width = (int)(Width / (double)_composition.Length * vagon.Length);
                    lbl.Margin = new Padding(0);
                    lbl.BackColor = Color.White;
                    lbl.BorderStyle = BorderStyle.FixedSingle;
                    lbl.AllowDrop = true;
                    Controls.Add(lbl);

                    lbl.MouseDown += Lbl_MouseDown;
                    lbl.MouseMove += Lbl_MouseMove;
                    lbl.MouseUp += Lbl_MouseUp;
                    //lbl.DoubleClick += Lbl_DoubleClick;
                }
                _frm.DisplaySectors(_frm.track, _composition);
            }

            public void Remove()
            {
                _composition = null;
                Controls.Clear();
            }

            private void InsertVagon()
            {
                var vagon = new Vagon(contextIndex);
                EditVagonDialog dialog = new EditVagonDialog(vagon);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (_composition == null)
                        _composition = new Composition();
                    _composition.AddVagon(vagon, vagon.VagonId);
                    Display(_composition);
                }
            }

            private void EditVagon()
            {
                var vagon = _composition.Vagons[contextIndex];
                EditVagonDialog dialog = new EditVagonDialog(vagon);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _composition.EditVagon(vagon);
                    Display(_composition);
                }
            }

            private void CopyVagon()
            {
                var vagon = new Vagon(_composition.Vagons[contextIndex]);
                EditVagonDialog dialog = new EditVagonDialog(vagon);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _composition.AddVagon(vagon, vagon.VagonId);
                    Display(_composition);
                }
            }

            private void MoveVagon()
            {
                if (moveableLabel == null)
                {
                    moveableLabel = contextLabel;
                    moveableIndex = Controls.IndexOf(moveableLabel);
                    moveableLabel.BackColor = Color.Aqua;
                    tsmMove.Text = "Переместить сюда";
                }
                else
                {
                    _composition.Shuffle(moveableIndex, contextIndex);
                    moveableLabel = null;
                    moveableIndex = -1;
                    Display(_composition);
                    tsmMove.Text = "Переместить вагон";
                }
            }

            private void RemoveVagon()
            {
                _composition.RemoveVagon(contextIndex);
                moveableLabel = null;
                moveableIndex = -1;
                Display(_composition);
            }

            private void Lbl_MouseDown(object sender, MouseEventArgs e)
            {
                var lbl = sender as Label;
                moveableLabel = lbl;
                moveableIndex = Controls.IndexOf(moveableLabel);

                //moveablePosition = e.Location;
                //mousePosition = MousePosition;
                //isDrag = true;

                if (e.Button == MouseButtons.Right)
                {
                    contextLabel = lbl;
                    contextIndex = Controls.IndexOf(lbl);
                }
            }

            private void Lbl_MouseUp(object sender, MouseEventArgs e)
            {
                //isDrag = false;
            }

            private void Lbl_MouseMove(object sender, MouseEventArgs e)
            {
                //if (isDrag)
                //{
                //    moveableLabel.Location = new Point(moveablePosition.X + (MousePosition.X - mousePosition.X), moveablePosition.Y + (MousePosition.Y - mousePosition.Y));
                //}
            }

            private void Insert_Click(object sender, EventArgs e)
            {
                InsertVagon();
            }

            private void Edit_Click(object sender, EventArgs e)
            {
                EditVagon();
            }

            private void Copy_Click(object sender, EventArgs e)
            {
                CopyVagon();
            }

            private void Move_Click(object sender, EventArgs e)
            {
                MoveVagon();
            }

            private void Remove_Click(object sender, EventArgs e)
            {
                RemoveVagon();
            }
        }

        private void КарточкаДвиженияПоезда_FormClosed(object sender, FormClosedEventArgs e)
        {
            track = null;
        }
    }
}
