using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communication.SerialPort;
using CommunicationDevices.DataProviders;
using CommunicationDevices.DataProviders.DisplaySysDataProvider;


namespace CommunicationDevices.Behavior.ExhangeBehavior.SerialPortBehavior
{

    /// <summary>
    /// ПОВЕДЕНИЕ ОБМЕНА ДАННЫМИ ТАБЛО "ДИСПЛЕЙНЫХ СИСТЕМ" ПО ПОСЛЕД. ПОРТУ
    /// </summary>
    public class DisplSysExchangeBehavior : BaseExhangeSpBehavior
    {
        public string Address { get; set; }

        #region ctor

        public DisplSysExchangeBehavior(MasterSerialPort port, string address, ushort timeRespone, byte maxCountFaildRespowne)
            : base(port, timeRespone, maxCountFaildRespowne)
        {
            Address = address;

            //добавляем циклические функции
            Data4CycleFunc = new ReadOnlyCollection<UniversalInputType>(new List<UniversalInputType> { new UniversalInputType { Event = "  ", NumberOfTrain = "  ", PathNumber = "  ", Stations = "  ", Time = DateTime.MinValue, TableData = new List<UniversalInputType>() } });  //данные для 1-ой циклической функции
            ListCycleFuncs = new List<Func<MasterSerialPort, CancellationToken, Task>> { CycleExcangeService };                      // 1 циклическая функция
        }

        #endregion




        #region Methode

        private async Task CycleExcangeService(MasterSerialPort port, CancellationToken ct)
        {
            var inData = Data4CycleFunc[0];
            if (inData?.TableData != null && inData?.TableData.Count > 0)
            {
                //фильтрация по ближайшему времени к текущему времени.
                var filteredData = inData.TableData;
                var timeSamplingMessage = UniversalInputType.GetFilteringByDateTimeTable(1, filteredData)?.FirstOrDefault();

                //вывод пустой строки если в таблице нет данных
                var emptyMessage = new UniversalInputType
                {
                    Event = "  ",
                    NumberOfTrain = "  ",
                    PathNumber = "  ",
                    Stations = "  ",
                    Time = DateTime.MinValue,
                    Message = $"ПОЕЗД:{inData.NumberOfTrain}, ПУТЬ:{inData.PathNumber}, СОБЫТИЕ:{inData.Event}, СТАНЦИИ:{inData.Stations}, ВРЕМЯ:{inData.Time.ToShortTimeString()}"
                };

                var viewData = timeSamplingMessage ?? emptyMessage;
                viewData.AddressDevice = Address;

                //Вывод на путевое табло
                var writeProvider = new PanelDispSysWriteDataProvider { InputData = viewData };
                DataExchangeSuccess = await Port.DataExchangeAsync(TimeRespone, writeProvider, ct);

                LastSendData = writeProvider.InputData;

                if (writeProvider.IsOutDataValid)
                {
                    // Log.log.Trace(""); //TODO: возможно передавать в InputData ID устройства и имя.
                }

                //await Task.Delay(1000, ct);  //задержка для задания периода опроса. 
            }
        }

        #endregion





        #region OverrideMembers

        protected override sealed List<Func<MasterSerialPort, CancellationToken, Task>> ListCycleFuncs { get; set; }

        protected override async Task OneTimeExchangeService(MasterSerialPort port, CancellationToken ct)
        {
            UniversalInputType inData = null;
            if (InDataDict != null && !InDataDict.IsEmpty && InDataDict.TryRemove(Address, out inData))
            //if ((InDataQueue != null && !InDataQueue.IsEmpty && InDataQueue.TryDequeue(out inData)))
            {
                if (inData?.TableData != null && inData?.TableData.Count > 0)
                {
                    //фильтрация по ближайшему времени к текущему времени.
                    var filteredData = inData.TableData;
                    var timeSamplingMessage = UniversalInputType.GetFilteringByDateTimeTable(1, filteredData)?.FirstOrDefault();

                    //вывод пустой строки если в таблице нет данных
                    var emptyMessage = new UniversalInputType
                    {
                        Event = "  ",
                        NumberOfTrain = "  ",
                        PathNumber = "  ",
                        Stations = "  ",
                        Time = DateTime.MinValue,
                        Message = $"ПОЕЗД:{inData.NumberOfTrain}, ПУТЬ:{inData.PathNumber}, СОБЫТИЕ:{inData.Event}, СТАНЦИИ:{inData.Stations}, ВРЕМЯ:{inData.Time.ToShortTimeString()}"
                    };

                    var viewData = timeSamplingMessage ?? emptyMessage;
                    viewData.AddressDevice = Address;

                    //Вывод на путевое табло
                    var writeProvider = new PanelDispSysWriteDataProvider { InputData = viewData };
                    DataExchangeSuccess = await Port.DataExchangeAsync(TimeRespone, writeProvider, ct);

                    LastSendData = writeProvider.InputData;

                    if (writeProvider.IsOutDataValid)
                    {
                        // Log.log.Trace(""); //TODO: возможно передавать в InputData ID устройства и имя.
                    }

                    //await Task.Delay(1000, ct);  //задержка для задания периода опроса. 
                }
            }
        }

        #endregion
    }
}