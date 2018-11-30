using System;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Devices;
using CommunicationDevices.Settings;

namespace CommunicationDevices.Behavior.BindingBehavior.ToGeneralSchedule
{   
    public enum SourceLoad                       // источник загрузки
    {
        None,
        MainWindow,                              // Из главного окна (расписание на сутки)
        Shedule,                                 // Из окна Расписание
        SheduleOperative,                         // Из окна Оперативное Расписание
        Dispatcher,
        Stations
    }

    public interface IBinding2GeneralSchedule
    {
        bool IsPaging { get; }
        bool IsLangPaging { get; }
        SourceLoad SourceLoad { get; set; }
        void InitializePagingBuffer(UniversalInputType inData, Func<UniversalInputType, bool> checkContrains, int? countDataTake = null);
        int? GetCountDataTake();
        bool CheckContrains(UniversalInputType inData);
        Langs Langs { get; }

        DeviceSetting GetDeviceSetting { get; }
    }
}