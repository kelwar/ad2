using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using AutodictorBL.Entites;
using AutodictorBL.Settings.XmlSound;
using AutodictorBL.Sound.Converters;
using System.Collections.Concurrent;
using System.Media;
using Domain.Entitys;

namespace AutodictorBL.Sound
{
    class PlayerSoundMedia : ISoundPlayer
    {
        #region fields

        private readonly Func<int> _выборУровняГромкостиFunc;
        private readonly Func<string, NotificationLanguage, string> _getFileNameFunc;
        private string _trackPath = "";
        private SoundPlayer _trackToPlay = null;
        private ConcurrentQueue<SoundPlayer> tracks = new ConcurrentQueue<SoundPlayer>();

        private object _locker = new object();

        #endregion

        #region prop
        public SoundPlayerType PlayerType { get; }
        private string _statusString;
        public string StatusString
        {
            get { return _statusString; }
            set
            {
                if (value == _statusString) return;
                _statusString = value;
                StatusStringChangeRx.OnNext(_statusString);
            }
        }

        private bool _isConnect;
        public bool IsConnect
        {
            get { return _isConnect; }
            set
            {
                if (value == _isConnect) return;
                _isConnect = value;
                IsConnectChangeRx.OnNext(_isConnect);
            }
        }
        public IFileNameConverter FileNameConverter => null; //Отстутсвует конвертор имени файлов
        #endregion

        #region RxEvent

        public Subject<string> StatusStringChangeRx { get; } = new Subject<string>(); //Изменение StatusString
        public Subject<bool> IsConnectChangeRx { get; } = new Subject<bool>(); //Изменение IsConnect

        #endregion

        #region Methode

        public int GetCurrentPosition()
        {
            throw new NotImplementedException();
        }

        public float GetDuration()
        {
            throw new NotImplementedException();
        }

        public string GetInfo()
        {
            throw new NotImplementedException();
        }

        public SoundPlayerStatus GetPlayerStatus()
        {
            throw new NotImplementedException();
        }

        public int GetVolume()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Play()
        {
            throw new NotImplementedException();
        }

        public Task<bool> PlayFile(ВоспроизводимоеСообщение soundMessage, bool useFileNameConverter = true)
        {
            throw new NotImplementedException();
        }

        public Task ReConnect()
        {
            throw new NotImplementedException();
        }

        public void SetVolume(int volume)
        {
            throw new NotImplementedException();
        }
        #endregion





        #region Dispouse

        public void Dispose()
        {
            if (_trackToPlay != null)
            {
                _trackToPlay.Dispose();
            }
        }

        public Task PlayMessage(Queue<ВоспроизводимоеСообщение> templateQueue)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
