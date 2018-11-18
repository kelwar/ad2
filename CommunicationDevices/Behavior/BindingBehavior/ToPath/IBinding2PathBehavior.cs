using System;
using System.Collections.Generic;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Devices;
using CommunicationDevices.Settings;

namespace CommunicationDevices.Behavior.BindingBehavior.ToPath
{
    public interface IBinding2PathBehavior
    {
        string GetDevicesName4Path(string pathNumber);

        IEnumerable<string> CollectionPathNumber { get; }

        bool IsPaging { get; }
        string GetDeviceName { get; }
        int GetDeviceId { get; }
        Langs Langs { get; }
        DeviceSetting GetDeviceSetting { get; }
        void InitializePagingBuffer(UniversalInputType inData, Func<UniversalInputType, bool> checkContrains, int? countDataTake = null);
        int? GetCountDataTake();

        void InitializeDevicePathInfo();

        void SendMessage4Path(UniversalInputType inData, string numberOfTrain, Func<UniversalInputType, bool> checkContrains, int? countDataTake = null);
        bool CheckContrains(UniversalInputType inData);
    }
}