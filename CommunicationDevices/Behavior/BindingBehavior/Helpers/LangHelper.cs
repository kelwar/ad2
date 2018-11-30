using CommunicationDevices.DataProviders;
using CommunicationDevices.Settings;
using Library.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CommunicationDevices.Behavior.BindingBehavior.Helpers
{
    public class LangHelper : IDisposable
    {
        private readonly Timer _timer;
        private LangList _langList;
        private bool _isFirstSend;

        public List<UniversalInputType> Data { get; set; } = new List<UniversalInputType>();
        public ISubject<LangList> LangDataSend { get; } = new Subject<LangList>();

        public LangHelper(Lang currentLang)
        {
            _langList = new LangList();
            _langList.CurrentLang = currentLang;
            _isFirstSend = true;

            //Log.log.Info($"Конструктор, перед SendData");
            //SendData();
            //Log.log.Info($"Конструктор, после SendData");
            Task.Run(() => SendData()).GetAwaiter().GetResult();

            _timer = new Timer(_langList.CurrentLang.Period);
            _timer.Elapsed += OnTimedEvent;
            _timer.Start();
        }

        public async Task<List<UniversalInputType>> SwitchLangDataAsync(List<UniversalInputType> tableData)
        {
            await Task.Run(() => tableData.ForEach(t =>
            {
                if (t.ViewBag == null)
                {
                    t.ViewBag = new Dictionary<string, dynamic>();
                }

                if (t.ViewBag.ContainsKey("Language"))
                {
                    t.ViewBag["Language"] = _langList.CurrentLang;
                }
                else
                {
                    t.ViewBag.Add("Language", _langList.CurrentLang);
                }
            }));
            return tableData;
        }

        //private async void SendData()
        private async void SendData()
        {
            //if (_langList == null)
            //    _langList = new LangList();
            //var uit = Data.FirstOrDefault();
            //if (uit == null || uit.ViewBag == null || !uit.ViewBag.ContainsKey("Language") || !_langList.CurrentLang.Equals((Lang)uit.ViewBag["Language"]))
            //{
                _langList.List = await SwitchLangDataAsync(Data);
            //}
            //await Task.Run(() => LangDataSend.OnNext(_langList));
            LangDataSend.OnNext(_langList);
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            if (!_isFirstSend)
                _langList.CurrentLang = _langList.CurrentLang.TurnLang();
            else if (Data.Any())
                _isFirstSend = false;

            await Task.Run(() => SendData());
            //LangDataSend.OnNext(langList);

            _timer.Interval = _langList.CurrentLang.Period;
            _timer.Start();
        }

        #region Disposable

        public void Dispose()
        {
            _timer?.Dispose();
        }

        #endregion
    }

    public class LangList
    {
        public List<UniversalInputType> List { get; set; }
        public Lang CurrentLang { get; set; }
    }
}
