using System.Collections.Generic;
using CommunicationDevices.DataProviders;
using CommunicationDevices.Devices;
using CommunicationDevices.Settings;


namespace CommunicationDevices.Behavior.BindingBehavior.ToStatic
{
    public class Binding2StaticFormBehavior : IBinding2StaticFormBehavior
    {
        #region prop

        private readonly Device _device;
        public string GetDeviceName => _device.Name;
        public int GetDeviceId => _device.Id;
        public string GetDeviceAddress => _device.Address;
        public DeviceSetting GetDeviceSetting => _device.Setting;
        public Langs Langs { get; }

        #endregion




        #region ctor

        public Binding2StaticFormBehavior(Device device, Langs langs)
        {
            _device = device;
            Langs = langs;
        }

        #endregion





        #region Metode

        public void SendMessage(UniversalInputType inData)
        {
            _device.AddCycleFuncData(0, inData);
            _device.AddOneTimeSendData(_device.ExhBehavior.GetData4CycleFunc[0]);
        }

        #endregion
    }
}