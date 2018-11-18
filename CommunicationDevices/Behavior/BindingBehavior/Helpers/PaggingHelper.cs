using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Timers;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Devices;



namespace CommunicationDevices.Behavior.BindingBehavior.Helpers
{
    public class PaggingHelper : IDisposable
    {
        private readonly Timer _timer;
        private int _currentPage = 0;



        public int CountPaging { get; }                                    // кол-во страниц.      
        public List<UniversalInputType> PagingBuffer { get; set; } = new List<UniversalInputType>();



        public ISubject<PagingList> PagingListSend { get; } = new Subject<PagingList>();



        public PaggingHelper(int timeDispalayPage, int countPaging)
        {
            CountPaging = countPaging;

            _timer = new Timer(timeDispalayPage);
            _timer.Elapsed += OnTimedEvent;
            _timer.Enabled = true;
        }





        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            var pagingList= new PagingList();                                                                 // Инициализировали список
 
            if (CountPaging >= PagingBuffer.Count)                                                            // Если кол-во строк равно кол-ву элементов
            {
                pagingList.CurrentPage = 0;                                                                   // Ничего не перелистываем
                pagingList.List = PagingBuffer;                                                               // Пишем в список весь буфер
                PagingListSend.OnNext(pagingList);                                                            // И отправляем его
                return;
            }

            var numberOfPage = PagingBuffer.Count / CountPaging;                                              // Кол-во страниц
            if (_currentPage < numberOfPage)                                                                  // Если страницы не закончились
            {
                var page = PagingBuffer.Skip(_currentPage * CountPaging).Take(CountPaging).ToList();          // Пропускаем уже пролистанные данные и выдаём ближайшие N не выведенных
                pagingList.List = page;                                                                       // Пишем в список
            }
            else
            {
                var remainingElem = PagingBuffer.Count - (_currentPage * CountPaging);                        // Иначе если страница последняя то: кол-во оставшихся элементов всего
                var page = PagingBuffer.Skip(_currentPage * CountPaging).Take(remainingElem).ToList();        // Выбираем их
                pagingList.List = page;                                                                       // Добавляем в список
            }


            pagingList.CurrentPage = _currentPage;                                                            // Задаем текущую страницу
            PagingListSend.OnNext(pagingList);                                                                // Отправляем порцию данных


            if (++_currentPage > numberOfPage)                                                                // Если доходим до конца данных
                _currentPage = 0;                                                                             // Возвращаемся на начальную страницу
        }




        #region Disposable

        public void Dispose()
        {
            _timer?.Dispose();
        }

        #endregion
    }




    public class PagingList
    {
        public List<UniversalInputType> List { get; set; }
        public int CurrentPage { get; set; }
    }
}