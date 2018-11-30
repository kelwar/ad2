using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using CommunicationDevices.Behavior.BindingBehavior.Helpers;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Devices;
using CommunicationDevices.Settings;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Library.Logs;

namespace CommunicationDevices.Behavior.BindingBehavior.ToPath
{

    /// <summary>
    /// привязка устройства к списку путей (1 табло может обслуживать несколько путей)
    /// Если список путей пуст, то привязка считается ко всем путям и обслуживается как вывод табличной информации.
    /// </summary>
    public class Binding2PathBehavior : IBinding2PathBehavior
    {
        private readonly Device _device;
        public IEnumerable<string> CollectionPathNumber { get; }
        public bool IsPaging { get; }
        public bool IsLangPaging { get; }
        public Conditions Conditions { get; }
        public Langs Langs { get; }
        public PaggingHelper PagingHelper { get; set; }
        public LangHelper LangHelper { get; set; }
        public IDisposable DispousePagingListSendRx { get; set; }
        public IDisposable DispouseLangDataSendRx { get; set; }
        public string GetDeviceName => _device.Name;
        public int GetDeviceId => _device.Id;
        public string GetDeviceAddress => _device.Address;
        public DeviceSetting GetDeviceSetting => _device.Setting;




        public Binding2PathBehavior(Device device, IEnumerable<string> pathNumbers, Conditions conditions, Langs langs, int countPage, int timePaging)
        {
            _device = device;
            CollectionPathNumber = pathNumbers;
            Conditions = conditions;
            Langs = langs;

            if (Langs != null && Langs.List.Any())
            {
                IsLangPaging = true;
                if (LangHelper == null)
                {
                    LangHelper = new LangHelper(Langs.List.FirstOrDefault());
                }
                DispouseLangDataSendRx = LangHelper.LangDataSend.Subscribe(OnNext);
            }

            //если указанны настройки пагинатора.
            if (countPage > 0 && timePaging > 0)
            {
                IsPaging = true;
                PagingHelper = new PaggingHelper(timePaging * 1000, countPage);
                DispousePagingListSendRx = PagingHelper.PagingListSend.Subscribe(OnNext);     //подписка на отправку сообщений пагинатором
            }
        }

        //private async void OnNext(LangList langList)
        private void OnNext(LangList langList)
        {
            var inData = new UniversalInputType
            {
                TableData = langList.List,
                Note = langList.CurrentLang.Name,
                AddressDevice = _device.Address
            };

            try
            {
                //await Task.Run(() => _device.ExhBehavior.AddOneTimeSendData(inData));
                _device.ExhBehavior.AddOneTimeSendData(inData);
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error($"Ошибка при отправке данных на путь. {ex}");
            }
        }

        private void OnNext(PagingList pagingList)
        {
            var inData = new UniversalInputType
            {
                TableData = pagingList.List,
                Note = pagingList.CurrentPage.ToString(),
                AddressDevice = _device.Address
            };

            _device.ExhBehavior.AddOneTimeSendData(inData);
        }


        public string GetDevicesName4Path(string pathNumber)
        {
            //привязка на все пути
            if (!CollectionPathNumber.Any())
                return $"{GetDeviceId}: {_device.Name}";

            //привязка на указанные пути
            var result = CollectionPathNumber.Contains(pathNumber) ? $"{GetDeviceId}: {_device.Name}" : null;
            return result;
        }




        public void SendMessage4Path(UniversalInputType inData, string numberOfTrain, Func<UniversalInputType, bool> checkContrains, int? countDataTake = null)
        {
            //проверка ограничения.
            if (inData.Command != Command.Delete)
            {
                if (!checkContrains(inData))
                    return;
            }

            if (CollectionPathNumber.Any())
            {
                switch (inData.Command)
                {
                    //ДОБАВИТЬ В ТАБЛ.
                    case Command.View:
                        _device.ExhBehavior.GetData4CycleFunc[0].TableData.Add(inData);                             // Изменили данные для циклического опроса
                        if (_device.ExhBehavior.GetData4CycleFunc[0].TableData.Count >= 2 &&                        // удалим пустой поезд (строку инициализации).
                            _device.ExhBehavior.GetData4CycleFunc[0].TableData.First().Time == DateTime.MinValue)
                        {
                            _device.ExhBehavior.GetData4CycleFunc[0].TableData.RemoveAt(0);
                        }
                        break;

                    //УДАЛИТЬ ИЗ ТАБЛ.
                    case Command.Delete:
                        var removeItem = _device.ExhBehavior.GetData4CycleFunc[0].TableData.FirstOrDefault(p => (p.Id == inData.Id) && (p.NumberOfTrain == numberOfTrain));
                        if (removeItem != null)
                        {
                            _device.ExhBehavior.GetData4CycleFunc[0].TableData.Remove(removeItem);
                            if (!_device.ExhBehavior.GetData4CycleFunc[0].TableData.Any())
                            {
                                InitializeDevicePathInfo();
                            }
                        }
                        break;

                    // ОБНОВИТЬ В ТАБЛ.
                    case Command.Update:
                        var updateItem = _device.ExhBehavior.GetData4CycleFunc[0].TableData.FirstOrDefault(p => (p.Id == inData.Id) && (p.NumberOfTrain == numberOfTrain));
                        if (updateItem != null)
                        {
                            var indexUpdateItem = _device.ExhBehavior.GetData4CycleFunc[0].TableData.IndexOf(updateItem);
                            _device.ExhBehavior.GetData4CycleFunc[0].TableData[indexUpdateItem] = inData;
                        }
                        break;
                }
            }

            InitializePagingBuffer(_device.ExhBehavior.GetData4CycleFunc[0], checkContrains, countDataTake);

            // Отправили однократный запрос (выставили запрос сразу на выполнение)
            //_device.AddOneTimeSendData(_device.ExhBehavior.GetData4CycleFunc[0]); 
        }

        public void InitializePagingBuffer(UniversalInputType inData, Func<UniversalInputType, bool> checkContrains, int? countDataTake = null)
        {
            var query = inData.TableData.Where(checkContrains); // Запрашиваем uit.TableData, соответствующие Contrains

            List<UniversalInputType> byCountList = null;
            if (countDataTake != null && countDataTake > 0)     // Если кол-во строк указано и больше нуля
            {
                byCountList = new List<UniversalInputType>();
                foreach (var track in CollectionPathNumber)
                {
                    byCountList.AddRange(query
                                         .Where(d => d.PathNumber == track &&
                                                     d.Time != DateTime.MinValue)
                                         .OrderBy(t => t.Time)
                                         .Take(countDataTake.Value));
                }
            }

            var filteredTable = byCountList ?? query.ToList();                 // Преобразуем их в список
            if (!filteredTable.Any())                           // Если список пуст
            {
                filteredTable.Add(UniversalInputType.DefaultUit);               // Заполняем его данными по умолчанию
            }

            if (IsLangPaging)
            {
                LangHelper.Data = filteredTable;
                _device.ExhBehavior.StopCycleExchange();
                return;
            }

            if (IsPaging)                                       // Если пейджинг включен
            {
                PagingHelper.PagingBuffer = filteredTable;      // Передаем выбранный список данных в буфер листателя
                _device.ExhBehavior.StopCycleExchange();
            }
            else
            {
                inData.TableData = filteredTable;               // Иначе меняет все данные на отфильтрованные
                inData.Note = string.Empty;                     // Описание делаем пустым (?)
                inData.AddressDevice = _device.Address;
                _device.ExhBehavior.StartCycleExchange();
                _device.AddCycleFuncData(0, inData);            // Запускаем цикл отправки выбранных обрезанных данных
            }
        }


        /// <summary>
        /// Проверка ограничения првязки.
        /// </summary>
        public bool CheckContrains(UniversalInputType inData)
        {
            if (!inData.IsActive)
                return false;

            if (Conditions == null)
                return true;

            return Conditions.CheckContrains(inData);
        }


        /// <summary>
        /// Вернуть сколко первых элементов таблицы нужно взять
        /// </summary>
        public int? GetCountDataTake()
        {
            return Conditions?.LimitNumberRowsOnTrack;
        }


        /// <summary>
        /// Инициализация начальной строки вывода на дисплей.
        /// Из всех привязанных путей берется первый путь для отображения.
        /// </summary>
        public void InitializeDevicePathInfo()
        {
            var inData = UniversalInputType.DefaultUit;
            //inData.PathNumber = (CollectionPathNumber != null && CollectionPathNumber.Any()) ? CollectionPathNumber.First().ToString() : "   ";
            //inData.PathNumber = CollectionPathNumber?.First() ?? "   ";
            inData.Message = $"ПОЕЗД:{inData.NumberOfTrain}, ПУТЬ:{inData.PathNumber}, СОБЫТИЕ:{inData.Event}, СТАНЦИИ:{inData.Stations}, ВРЕМЯ:{inData.Time.ToShortTimeString()}";

            InitializePagingBuffer(inData, CheckContrains);
            //_device.AddCycleFuncData(0, inData);
        }

        #region Disposable

        public void Dispose()
        {
            DispousePagingListSendRx?.Dispose();
            DispouseLangDataSendRx?.Dispose();
            PagingHelper?.Dispose();
            LangHelper?.Dispose();
        }

        #endregion
    }
}