using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Communication.SerialPort;
using CommunicationDevices.DataProviders;
using System.Collections.Concurrent;
using Library.Logs;

namespace CommunicationDevices.Behavior.ExhangeBehavior.SerialPortBehavior
{
    /// <summary>
    /// АБСТРАКТНЫЙ КЛАСС ПОВЕДЕНИЯ ОБМЕНА ПО ПОСЛЕДОВАТЕЛЬНОМУ ПОРТУ.
    /// </summary>
    public abstract class BaseExhangeSpBehavior : IExhangeBehavior
    {
        #region Fields

        private readonly byte _maxCountFaildRespowne;
        private byte _countFaildRespowne;
        protected readonly ushort TimeRespone;

        #endregion




        #region Prop

        protected MasterSerialPort Port { get; }

       // public Queue<UniversalInputType> InDataQueue { get; set; } = new Queue<UniversalInputType>();
        public ConcurrentQueue<UniversalInputType> InDataQueue { get; set; } = new ConcurrentQueue<UniversalInputType>();
        public ConcurrentDictionary<string, UniversalInputType> InDataDict { get; set; } = new ConcurrentDictionary<string, UniversalInputType>();

        public ReadOnlyCollection<UniversalInputType> Data4CycleFunc { get; set; }        // для каждой циклической функции свои данные. 

        public int NumberPort => Port.PortNumber;
        public bool IsOpen => Port.IsOpen;

        private bool _isConnect;
        public bool IsConnect
        {
            get { return _isConnect; }
            set
            {
                if(_isConnect == value)
                    return;

                _isConnect = value;
                IsConnectChange.OnNext(this);
            }
        }


        private bool _dataExchangeSuccess;
        public bool DataExchangeSuccess
        {
            get { return _dataExchangeSuccess; }
            set
            {
                _dataExchangeSuccess = value;
                if (_dataExchangeSuccess)
                {
                    _countFaildRespowne = 0;
                    IsConnect = true;
                }
                else
                {
                    if (_countFaildRespowne++ >= _maxCountFaildRespowne)
                    {
                        _countFaildRespowne = 0;
                        IsConnect = false;
                    }
                }

                IsDataExchangeSuccessChange.OnNext(this);
            }
        }


        private UniversalInputType _lastSendData;
        public UniversalInputType LastSendData
        {
            get { return _lastSendData; }
            set
            {
                _lastSendData = value;
                LastSendDataChange.OnNext(this);
            }
        }

        #endregion




        #region ctor

        protected BaseExhangeSpBehavior(MasterSerialPort port, ushort timeRespone, byte maxCountFaildRespowne)
        {
            Port = port;
            TimeRespone = timeRespone;
            _maxCountFaildRespowne = maxCountFaildRespowne;
        }

        #endregion




        #region Rx

        public ISubject<IExhangeBehavior> IsDataExchangeSuccessChange { get; } = new Subject<IExhangeBehavior>();
        public ISubject<IExhangeBehavior> IsConnectChange { get; } = new Subject<IExhangeBehavior>();
        public ISubject<IExhangeBehavior> LastSendDataChange { get; } = new Subject<IExhangeBehavior>();
        public string ProviderName { get; set; }

        #endregion




        #region Methode

        public void CycleReConnect(ICollection<Task> backGroundTasks = null)
        {
            try
            {
                if (Port != null)
                {
                    var taskSerialPort = Task.Factory.StartNew(async () =>
                    {
                        if (await Port.CycleReConnect())
                        {
                            var taskPortEx = Port.RunExchange();
                            backGroundTasks?.Add(taskPortEx);
                        }
                    });
                    backGroundTasks?.Add(taskSerialPort);
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }
        }


        /// <summary>
        /// Добавление однократно вызываемых функций
        /// </summary>
        public void AddOneTimeSendData(UniversalInputType inData)
        {
            try
            {
                if (inData != null)
                {
                    InDataDict.AddOrUpdate(inData.AddressDevice, inData, (k, v) => inData);
                    if (InDataDict.Count > 150)
                    {
                        Log.log.Warn($"Очередь больше 150 элементов: {InDataDict.Count}");
                    }
                    Port.AddOneTimeSendData(OneTimeExchangeService);
                }
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }


        /// <summary>
        /// Изменение данных для циклических функций
        /// </summary>
        public ReadOnlyCollection<UniversalInputType> GetData4CycleFunc => Data4CycleFunc;


        /// <summary>
        /// Добавление циклических функций.
        /// Поведение устройства определяет нужное количество циклических функций. Добавляются все функции в очередь порта
        /// </summary>
        public void StartCycleExchange()
        {
            try
            {
                ListCycleFuncs?.ForEach(func => Port.AddCycleFunc(func));
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }


        /// <summary>
        /// Удаление циклических функций.
        /// Удаляются все циклические функции из очереди порта.
        /// </summary>
        public void StopCycleExchange()
        {
            try
            {
                ListCycleFuncs?.ForEach(func => Port.RemoveCycleFunc(func));
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }
        }


        protected abstract List<Func<MasterSerialPort, CancellationToken, Task>> ListCycleFuncs { get; set; }
        protected abstract Task OneTimeExchangeService(MasterSerialPort port, CancellationToken ct);

        #endregion
    }
}