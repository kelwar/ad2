using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Xml;
using System.Xml.Linq;
using CommunicationDevices.Behavior.ExhangeBehavior;
using CommunicationDevices.Behavior.GetDataBehavior.ConvertGetedData;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Settings;
using Library.Logs;
using Domain.Entitys;

namespace CommunicationDevices.Behavior.GetDataBehavior
{
    public class BaseGetDataBehavior : IDisposable
    {
        #region prop
        //название поведения получения данных

        public string Name { get; set; }     

        //издатель события "данные получены и преобразованны в IEnumerable<UniversalInputType>"
        public ISubject<IEnumerable<UniversalInputType>> ConvertedDataChangeRx { get; } = new Subject<IEnumerable<UniversalInputType>>();

        //издатель события "изменения состояния соединения с сервером"
        public ISubject<IExhangeBehavior> ConnectChangeRx { get; }

        //издатель события "изменения состояния обмена данными"
        public ISubject<IExhangeBehavior> DataExchangeSuccessRx { get; }

        //конвертер в XDocument -> IEnumerable<UniversalInputType>
        public IInputDataConverter InputConverter { get; }

        public IDisposable GetStreamRxHandlerDispose { get; set; }

        private object locker= new object();
        public Station ThisStation { get; }
        public Conditions Conditions { get; } // Свойство добавлено для применения ограничений на уже полученные данные


        #endregion




        #region ctor

        public BaseGetDataBehavior(string name, ISubject<IExhangeBehavior> connectChangeRx,
                                                ISubject<IExhangeBehavior> dataExchangeSuccessRx,
                                                ISubject<Stream> getStreamRx,
                                                IInputDataConverter inputConverter,
                                                Station thisStation,
                                                Conditions conditions = null)
        {
            Name = name;
            ConnectChangeRx = connectChangeRx;
            DataExchangeSuccessRx = dataExchangeSuccessRx;
            GetStreamRxHandlerDispose = getStreamRx.Subscribe(GetStreamRxHandler);      //подписка на событие получения потока данных
            InputConverter = inputConverter;
            ThisStation = thisStation;
            Conditions = conditions;
        }

        #endregion




        /// <summary>
        /// Обработчик события получения потока данных от сервера
        /// </summary>
        private void GetStreamRxHandler(Stream stream)
        {
            var text = string.Empty;
            try
            {
                lock (locker)
                {
                    StreamReader reader = new StreamReader(stream);
                    text = (reader.ReadToEnd()).Trim();
                    if (!text.StartsWith("<?xml "))
                        return;

                    XDocument xDoc = XDocument.Parse(text);
                    var data = InputConverter.ParseXml2Uit(xDoc)?.ToList();
                    ConvertedDataChangeRx.OnNext(data);
                }
            }
            catch (XmlException ex)
            {
                Log.log.Error($"{ex}. Получены некорректные данные: {text}");
            }
            catch (Exception ex)
            {
                Log.log.Error($"метод GetStreamRxHandler:  {ex}.  InnerException: {ex.InnerException?.Message ?? string.Empty}");
            }
        }



        public void Dispose()
        {
            GetStreamRxHandlerDispose?.Dispose();
        }
    }
}