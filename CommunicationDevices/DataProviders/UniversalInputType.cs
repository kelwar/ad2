﻿using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entitys;
using MoreLinq;



namespace CommunicationDevices.DataProviders
{
    public enum TypeTrain
    {
        None,                    //НеОпределен
        Passenger,               //Пассажирский
        Suburban,                //Пригородный
        Corporate,               //Фирменный
        Express,                 //Скорый
        HighSpeed,               //Скоростной
        Swallow,                 //Ласточка
        Rex,                     //РЭКС
        Tourist,                 //Туристический
    }

    public enum VagonDirection { None, FromTheHead, FromTheTail }
    public enum Command { None, View, Update, Delete, Clear, Restart }

    public enum InDataType
    {
        None, Trains, LocalTrains, TrainsOper, LocalTrainsOper, Users, Vagons, Command
    }

    public class UniversalInputType
    {
        public static readonly UniversalInputType DefaultUit = new UniversalInputType
        {
            IsActive = true,
            NumberOfTrain = "  ",
            PathNumber = "  ",
            Event = "   ",
            Time = DateTime.MinValue,
            Stations = "   ",
            Note = "   ",
            TypeTrain = TypeTrain.None,
            Command = Command.None,
            TableData = new List<UniversalInputType>() { new UniversalInputType() }
        };

        public int Id { get; set; }
        public int ScheduleId { get; set; }                          //Id поезда в распсиании
        public int TrnId { get; set; }
        public string AddressDevice { get; set; }                    //Адресс устройсва
        public bool IsActive { get; set; }                           //Флаг активности записи ("Отменен без объявления")

        public TypeTrain TypeTrain { get; set; }                     //тип поезда
        public string NumberOfTrain { get; set; }                    //Номер поезда
        public string PathNumber { get; set; }                       //Номер пути
        public Pathways Track { get; set; }                          // Объект пути. От строкового PathNumber необходимо избавиться
        public string PathNumberWithoutAutoReset { get; set; }       //Номер пути Без Автосброса
        public string Event { get; set; }                            //Событие (ОТПР./ПРИБ./СТОЯНКА)
        public string Addition { get; set; }                         //Дополнение (свободная строка)
        public string AdditionEng { get; set; }
        public string Stations { get; set; }                         // Станции Отправления-Прибытия. (название поезда)
        public string DirectionStation { get; set; }                 // Направление.
        public Station StationDeparture { get; set; }
        public Station StationArrival { get; set; }

        public string Note { get; set; }                             //Примечание.
        public string NoteEng { get; set; }
        public string DaysFollowing { get; set; }                    //Дни следования
        public string DaysFollowingAlias { get; set; }               //Дни следования, заданные в строке в нужном формате
        public string DaysFollowingAliasEng { get; set; }
        public DateTime Time { get; set; }                           //Время
        public Dictionary<string, DateTime> TransitTime { get; set; } //Транзитное время ["приб"]/["отпр"]
        public TimeSpan? DelayTime { get; set; }                     //Время задержки (прибытия или отправления поезда)
        public DateTime? ВремяЗадержки { get; set; } 
        public DateTime ExpectedTime { get; set; }                   //Ожидаемое время (Время + Время задержки)
        public TimeSpan? StopTime { get; set; }                      //время стоянки (для транзитов: Время отпр - время приб)
        public VagonDirection VagonDirection { get; set; }           //Нумерация вагона (с головы, с хвоста)
        public bool ChangeVagonDirection { get; set; }               //флаг смены нумерации вагонов
        public bool SendingDataLimit { get; set; }                   //Ограниение отправки данных (если Contrains="SendingDataLimit", то выводим только с галкой)
        public string Message { get; set; }                          //Сообщение

        public byte EmergencySituation { get; set; }                 //Нешатная ситуация (бит 0 - Отмена, бит 1 - задержка прибытия, бит 2 - задержка отправления, бит 3 - отправление по готовности)

        public Command Command { get; set; }                         //Команда (если указанна команда, то приоритет отдается выполнению команды.)

        public List<UniversalInputType> TableData { get; set; }      //Данные для табличного представления

        public List<bool> SoundChanels { get; set; }                 //Настройка звуковых каналов (по каким каналам передавать данное сообщение)

        public Dictionary<string, dynamic> ViewBag { get; set; }     //Не типизированный контейнер для передачи любых данных
        public InDataType InDataType { get; set; }                   // Тип входящих данных: поезда, пользователи или вагоны






        #region Methode

        public void Initialize(UniversalInputType initializeData)
        {
            Id = initializeData.Id;
            ScheduleId = initializeData.ScheduleId;
            AddressDevice = initializeData.AddressDevice;
            IsActive = initializeData.IsActive;
            TypeTrain = initializeData.TypeTrain;
            NumberOfTrain = initializeData.NumberOfTrain;
            PathNumber = initializeData.PathNumber;
            PathNumberWithoutAutoReset = initializeData.PathNumberWithoutAutoReset;
            Track = initializeData.Track;
            Event = initializeData.Event;
            Addition = initializeData.Addition;
            AdditionEng = initializeData.AdditionEng;
            Stations = initializeData.Stations;
            DirectionStation = initializeData.DirectionStation;
            StationArrival = initializeData.StationArrival;
            StationDeparture = initializeData.StationDeparture;
            Note = initializeData.Note;
            NoteEng = initializeData.NoteEng;
            DaysFollowing = initializeData.DaysFollowing;
            DaysFollowingAlias = initializeData.DaysFollowingAlias;
            DaysFollowingAliasEng = initializeData.DaysFollowingAliasEng;
            Time = initializeData.Time;
            TransitTime = initializeData.TransitTime;
            ВремяЗадержки = initializeData.ВремяЗадержки;
            ExpectedTime = initializeData.ExpectedTime;
            StopTime = initializeData.StopTime;
            Message = initializeData.Message;
            EmergencySituation = initializeData.EmergencySituation;
            Command = initializeData.Command;
            VagonDirection = initializeData.VagonDirection;
            ChangeVagonDirection= initializeData.ChangeVagonDirection;


            if (initializeData.TableData != null && initializeData.TableData.Any()) 
            {
                TableData = new List<UniversalInputType>(initializeData.TableData);
            }

            if (initializeData.SoundChanels != null && initializeData.SoundChanels.Any())
            {
                SoundChanels = new List<bool>(initializeData.SoundChanels);
            }

            if (initializeData.ViewBag != null && initializeData.ViewBag.Any())
            {
                ViewBag= new Dictionary<string, dynamic>(initializeData.ViewBag);
            }
        }


        public static List<UniversalInputType> GetFilteringByDateTimeTable(int outElement, IEnumerable<UniversalInputType> table)
        {
            if (outElement <= 0)
                return null;

            if (table.Count() < outElement)
                return null;


            var filtredCollection = new List<UniversalInputType>();
            var copyTableData = new List<UniversalInputType>(table);
            for (int i = 0; i < outElement; i++)
            {
                var nearVal = copyTableData.MinBy(d => (d.Time - DateTime.Now).Duration());
                filtredCollection.Add(nearVal);
                copyTableData.RemoveAt(copyTableData.IndexOf(nearVal));
            }

            return filtredCollection;
        }

        #endregion
    }
}