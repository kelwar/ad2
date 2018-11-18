using CommunicationDevices.DataProviders;
using CommunicationDevices.Settings;
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
        private Lang _currentLang;
        private LangList langList;
        private bool _isFirstSend;

        public List<UniversalInputType> Data { get; set; } = new List<UniversalInputType>();
        public ISubject<LangList> LangDataSend { get; } = new Subject<LangList>();

        public LangHelper(Lang currentLang)
        {
            _currentLang = currentLang;
            _isFirstSend = true;

            Task.Run(() => SendData()).GetAwaiter().GetResult();

            _timer = new Timer(_currentLang.Period);
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
                    t.ViewBag["Language"] = _currentLang;
                }
                else
                {
                    t.ViewBag.Add("Language", _currentLang);
                }
            }));
            return tableData;
        }

        private async void SendData()
        {
            if (langList == null)
                langList = new LangList();
            langList.CurrentLang = _currentLang;
            langList.List = await SwitchLangDataAsync(Data);
            await Task.Run(() => LangDataSend.OnNext(langList));
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            if (!_isFirstSend)
                _currentLang = _currentLang.TurnLang();
            else if (Data.Any())
                _isFirstSend = false;

            await Task.Run(() => SendData());
            //LangDataSend.OnNext(langList);

            _timer.Interval = _currentLang.Period;
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
