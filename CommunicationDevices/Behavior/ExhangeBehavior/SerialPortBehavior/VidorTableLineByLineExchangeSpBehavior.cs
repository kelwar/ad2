using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communication.SerialPort;
using Communication.TcpIp;
using CommunicationDevices.DataProviders;
using CommunicationDevices.DataProviders.VidorDataProvider;

namespace CommunicationDevices.Behavior.ExhangeBehavior.SerialPortBehavior
{

    /// <summary>
    /// ПОВЕДЕНИЕ ОБМЕНА ДАННЫМИ МНОГОСТРОЧНОГО ТАБЛО "ДИСПЛЕЙНЫХ СИСТЕМ" ПО ПОСЛЕД. ПОРТУ
    /// </summary>
    public class VidorTableLineByLineExchangeSpBehavior : BaseExhangeSpBehavior
    {
        #region fields

        private readonly byte _countRow; //кол-во строк на табло

        #endregion




        #region prop

        public ILineByLineDrawingTableDataProvider ForTableViewDataProvider { get; set; }
        public string Address { get; set; }
        public bool IsSyncTime { get; set; }
        public int PeriodTimer { get; set; }                              //Период опроса в мСек.
        #endregion




        #region ctor

        public VidorTableLineByLineExchangeSpBehavior(MasterSerialPort port, string address, ushort timeRespone, byte maxCountFaildRespowne, byte countRow, bool isSyncTime, int periodTimer)
            : base(port, timeRespone, maxCountFaildRespowne)
        {
            _countRow = countRow;
            IsSyncTime = isSyncTime;
            PeriodTimer = periodTimer;
            Address = address;

            //добавляем циклические функции
            Data4CycleFunc = new ReadOnlyCollection<UniversalInputType>(new List<UniversalInputType> { new UniversalInputType { TableData = new List<UniversalInputType>() } });  //данные для 1-ой циклической функции
            ListCycleFuncs = new List<Func<MasterSerialPort, CancellationToken, Task>> { CycleExcangeService };                      // 1 циклическая функция
        }

        #endregion




        #region Methode

        private async Task CycleExcangeService(MasterSerialPort port, CancellationToken ct)
        {
            var inData = Data4CycleFunc[0];

            //Вывод на табличное табло построчной информации
            if (inData?.TableData != null && inData?.TableData.Count > 0)
            {
                //фильтрация по ближайшему времени к текущему времени.
                var filteredData = inData.TableData;
                var timeSampling = inData.TableData.Count > _countRow ? UniversalInputType.GetFilteringByDateTimeTable(_countRow, filteredData) : filteredData;
                var orderSampling = timeSampling.Where(date => date.Time > DateTime.MinValue).OrderBy(date => date.Time).ToList();//TODO:фильтровать при заполнении TableData.

                var count = timeSampling.Count() - orderSampling.Count;
                for (int j = 0; j < count; j++)
                    orderSampling.Add(UniversalInputType.DefaultUit);

                orderSampling.ForEach(t => t.AddressDevice = Address);
                for (byte i = 0; i < _countRow; i++)
                {
                    ForTableViewDataProvider.CurrentRow = (byte)(i + 1);                                                                                                        
                    ForTableViewDataProvider.InputData = (i < orderSampling.Count) ? orderSampling[i] : new UniversalInputType { AddressDevice = Address };

                    DataExchangeSuccess = await Port.DataExchangeAsync(TimeRespone, ForTableViewDataProvider, ct);
                    LastSendData = ForTableViewDataProvider.InputData;

                    await Task.Delay(500, ct);
                }


                //Запрос синхронизации времени
                if (IsSyncTime)
                {
                    ForTableViewDataProvider.CurrentRow = 0xFF;
                    ForTableViewDataProvider.InputData = new UniversalInputType {AddressDevice = Address };
                    DataExchangeSuccess = await Port.DataExchangeAsync(TimeRespone, ForTableViewDataProvider, ct);
                }
            }

            await Task.Delay(PeriodTimer, ct);  //задержка для задания периода опроса.    
        }

        #endregion




        #region OverrideMembers

        protected sealed override List<Func<MasterSerialPort, CancellationToken, Task>> ListCycleFuncs { get; set; }

        protected override async Task OneTimeExchangeService(MasterSerialPort port, CancellationToken ct)
        {
            UniversalInputType inData = null;
            if (InDataDict != null && !InDataDict.IsEmpty && InDataDict.TryRemove(Address, out inData))
            //if ((InDataQueue != null && !InDataQueue.IsEmpty && InDataQueue.TryDequeue(out inData)))
            {
                //Вывод на табличное табло построчной информации
                if (inData?.TableData != null && inData?.TableData.Count > 0)
                {
                    //фильтрация по ближайшему времени к текущему времени.
                    var filteredData = inData.TableData;
                    var timeSampling = inData.TableData.Count > _countRow ? UniversalInputType.GetFilteringByDateTimeTable(_countRow, filteredData) : filteredData;
                    var orderSampling = timeSampling.Where(date => date.Time > DateTime.MinValue).OrderBy(date => date.Time).ToList();//TODO:фильтровать при заполнении TableData.

                    var count = timeSampling.Count() - orderSampling.Count;
                    for (int j = 0; j < count; j++)
                        orderSampling.Add(UniversalInputType.DefaultUit);

                    orderSampling.ForEach(t => t.AddressDevice = Address);
                    for (byte i = 0; i < _countRow; i++)
                    {
                        ForTableViewDataProvider.CurrentRow = (byte)(i + 1);                                                                                                        // Отрисовка строк
                        ForTableViewDataProvider.InputData = (i < orderSampling.Count) ? orderSampling[i] : new UniversalInputType { AddressDevice = Address };          // Обнуление строк

                        DataExchangeSuccess = await Port.DataExchangeAsync(TimeRespone, ForTableViewDataProvider, ct);
                        LastSendData = ForTableViewDataProvider.InputData;

                        await Task.Delay(500, ct);
                    }

                    //Запрос синхронизации времени
                    if (IsSyncTime)
                    {
                        ForTableViewDataProvider.CurrentRow = 0xFF;
                        ForTableViewDataProvider.InputData = new UniversalInputType { AddressDevice = Address };
                        DataExchangeSuccess = await Port.DataExchangeAsync(TimeRespone, ForTableViewDataProvider, ct);
                    }
                }
            }

            await Task.Delay(PeriodTimer, ct);  //задержка для задания периода опроса. 
        }

        #endregion
    }
}