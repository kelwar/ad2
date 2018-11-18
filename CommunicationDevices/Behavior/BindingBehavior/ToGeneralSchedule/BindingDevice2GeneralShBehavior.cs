using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Timers;
using CommunicationDevices.Behavior.BindingBehavior.Helpers;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Devices;
using CommunicationDevices.Settings;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CommunicationDevices.Behavior.BindingBehavior.ToGeneralSchedule
{
    public class BindingDevice2GeneralShBehavior : IBinding2GeneralSchedule, IDisposable
    {
        #region fields

        private readonly Device _device;

        #endregion
        
        #region prop

        public bool IsPaging { get; }
        public bool IsLangPaging { get; }
        public SourceLoad SourceLoad { get; set; }
        public Conditions Conditions { get; }
        public Langs Langs { get; }
        public PaggingHelper PagingHelper { get; set; }
        public LangHelper LangHelper { get; set; }
        public IDisposable DispousePagingListSendRx { get; set; }
        public IDisposable DispouseLangDataSendRx { get; set; }

        public DeviceSetting GetDeviceSetting => _device.Setting;

        #endregion
        
        #region ctor

        public BindingDevice2GeneralShBehavior(Device device, SourceLoad source, Conditions conditions, Langs langs, int countPage, int timePaging)
        {
            Conditions = conditions;
            Langs = langs;
            _device = device;
            SourceLoad = source;

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

        #endregion

        public bool _sendLock = false;
        private void OnNext(LangList langList)
        {
            var inData = new UniversalInputType
            {
                TableData = langList.List,
                Note = langList.CurrentLang.Name,
                AddressDevice = _device.Address
            };

            _device.ExhBehavior.AddOneTimeSendData(inData);
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

        public void InitializePagingBuffer(UniversalInputType inData, Func<UniversalInputType, bool> checkContrains, int? countDataTake = null)
        {
            var query = inData.TableData.Where(checkContrains); // Запрашиваем uit.TableData, соответствующие Contrains
            if (countDataTake != null && countDataTake > 0)     // Если кол-во строк указано и больше нуля
            {
                query = query.Take(countDataTake.Value);        // Делаем выборку именно этих строк
            }

            var filteredTable = query.ToList();                 // Преобразуем их в список
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
                inData.Note = String.Empty;                     // Описание делаем пустым (?)
                inData.AddressDevice = _device.Address;
                _device.AddCycleFuncData(0, inData);            // Запускаем цикл отправки выбранных обрезанных данных
                _device.ExhBehavior.StartCycleExchange();
            }
        }
        
        /// <summary>
        /// Проверка ограничения привязки.
        /// </summary>
        public bool CheckContrains(UniversalInputType inData)
        {
            if (!inData.IsActive && SourceLoad != SourceLoad.Dispatcher)
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
            return Conditions?.LimitNumberRows;
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