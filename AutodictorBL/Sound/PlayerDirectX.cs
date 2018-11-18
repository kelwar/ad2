using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using AutodictorBL.Entites;
using AutodictorBL.Settings.XmlSound;
using AutodictorBL.Sound.Converters;
using Domain.Entitys;
using Microsoft.DirectX.AudioVideoPlayback;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutodictorBL.Sound
{
    //TODO:Добавить lock для основных функций, т.к. очередь звука разматывается в потоке отдельного таймера, и могут быть паралеьно вызванны йункции из основного потока.
    public class PlayerDirectX : ISoundPlayer
    {
        #region fields

        private readonly Func<int> _выборУровняГромкостиFunc;
        private readonly Func<string, NotificationLanguage, string> _getFileNameFunc;
        private string _trackPath = "";
        private Audio _trackToPlay = null;
        private ConcurrentQueue<Audio> tracks = new ConcurrentQueue<Audio>();

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





        #region ctor

        public PlayerDirectX(SoundPlayerType playerType, Func<int> выборУровняГромкостиFunc, Func<string, NotificationLanguage, string> getFileNameFunc)
        {
            PlayerType = playerType;
            _выборУровняГромкостиFunc = выборУровняГромкостиFunc;
            _getFileNameFunc = getFileNameFunc;
            IsConnect = true;
        }

        #endregion




        #region RxEvent

        public Subject<string> StatusStringChangeRx { get; } = new Subject<string>(); //Изменение StatusString
        public Subject<bool> IsConnectChangeRx { get; } = new Subject<bool>(); //Изменение IsConnect

        #endregion




        #region Methode

        /// <summary>
        /// Получить информацию о плеере
        /// </summary>
        public string GetInfo()
        {
            return "Плеер DirectX, настройки стандартные....";
        }


        public async Task ReConnect()
        {
            IsConnect = true;
        }


        public async Task<bool> AddFile(ВоспроизводимоеСообщение soundMessage, bool useFileNameConverter = true)
        {
            var filePath = string.Empty;
            if (soundMessage != null)
            {
                filePath = soundMessage.ИмяВоспроизводимогоФайла ?? string.Empty;
                var язык = soundMessage.Язык;

                try
                {
                    if (!filePath.Contains(".wav"))
                    {
                        filePath = _getFileNameFunc(filePath, язык);
                    }
                }
                catch (Exception ex)
                {
                    Library.Logs.Log.log.Error($"Ошибка при получении пути к файлу. Путь: {filePath ?? "null"}. {ex}");
                }
            }

            _trackPath = filePath;
            if (string.IsNullOrWhiteSpace(_trackPath))
                return false;

            try
            {
                if (File.Exists(_trackPath))
                {
                    if (tracks != null)
                        tracks.Enqueue(new Audio(_trackPath));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error($"Ошибка при воспроизведении файла. Путь к файлу: {_trackPath ?? "null"}. {ex}");
            }
            return false;
        }

        public async Task<bool> PlayFile(ВоспроизводимоеСообщение soundMessage, bool useFileNameConverter = true)
        {
            var filePath = string.Empty;
            if (soundMessage != null)
            {
                filePath = soundMessage.ИмяВоспроизводимогоФайла ?? string.Empty;
                var язык = soundMessage.Язык;

                try
                {
                    if (!filePath.Contains(".wav"))
                    {
                        filePath = _getFileNameFunc(filePath, язык);
                    }
                }
                catch (Exception ex)
                {
                    Library.Logs.Log.log.Error($"Ошибка при получении пути к файлу. Путь: {filePath ?? "null"}. {ex}");
                }
            }

            if (_trackToPlay != null)
            {
                _trackToPlay.Dispose();
                _trackToPlay = null;
            }
            
            _trackPath = filePath;
            if (string.IsNullOrWhiteSpace(_trackPath))
                return false;

            try
            {
                if (File.Exists(_trackPath))
                {
                    _trackToPlay = new Audio(_trackPath);
                    _trackToPlay.Play();
                    SetVolume(_выборУровняГромкостиFunc());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error($"Ошибка при воспроизведении файла. Путь к файлу: {_trackPath ?? "null"}. {ex}");
            }
            return false;
        }

        public async Task PlayMessage(Queue<ВоспроизводимоеСообщение> templateQueue)
        {
            if (templateQueue == null)
                return;

            while (templateQueue.Count > 0)
            {
                var isExist = await AddFile(templateQueue.Dequeue());
            }

            try
            {
                var playTime = DateTime.MinValue;
                var isPauses = false;
                while (tracks != null && tracks.Count > 0)
                {
                    if (_trackToPlay == null || _trackToPlay.Disposed || (!_trackToPlay.Playing || _trackToPlay.CurrentPosition >= _trackToPlay.Duration))
                    {
                        Dispose();
                        if (tracks.TryDequeue(out _trackToPlay) && _trackToPlay != null)
                        {
                            if (!isPauses && playTime != DateTime.MinValue && DateTime.Now - playTime > TimeSpan.FromSeconds(1))
                                isPauses = true;

                            _trackToPlay.Play();
                            playTime = DateTime.Now;
                            SetVolume(_выборУровняГромкостиFunc());
                        }
                    }
                }
                if (isPauses)
                    Library.Logs.Log.log.Warn($"Пауза между словами больше 1 секунды");
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error($"Ошибка при воспроизведении сообщения: {ex}");
            }
        }

        public void Pause()
        {
            if (_trackToPlay == null)
                return;

            try
            {
                _trackToPlay.Pause();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                if (tracks != null && tracks.Count > 0 && _trackToPlay != null)
                {
                    _trackToPlay.Stop();
                    tracks = new ConcurrentQueue<Audio>();
                }
                else
                {
                    PlayFile(null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> Play()
        {
            if (_trackToPlay == null)
                return false;

            try
            {
                _trackToPlay.Play();
                return true;
            }
            catch (Exception e)
            {
                // ignored
            }
            return false;
        }


        public float GetDuration()
        {
            try
            {
                if (_trackToPlay != null)
                    return (float)_trackToPlay.Duration;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0f;
            }

            return 0f;
        }


        public int GetCurrentPosition()
        {
            try
            {
                if (_trackToPlay != null)
                    return (int)_trackToPlay.CurrentPosition;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            };

            return 0;
        }



        //TODO: Exceptions при геннерации списка.
        public SoundPlayerStatus GetPlayerStatus()
        {
            SoundPlayerStatus playerStatus = SoundPlayerStatus.Idle;

            try
            {
                if (_trackToPlay != null && !_trackToPlay.Disposed)
                {
                    if (_trackToPlay.State == StateFlags.Running || _trackToPlay.Playing)
                    {
                        if (_trackToPlay != null && !_trackToPlay.Disposed && _trackToPlay.CurrentPosition >= _trackToPlay.Duration)
                            return SoundPlayerStatus.Idle;

                        return SoundPlayerStatus.Playing;
                    }

                    if (_trackToPlay.Paused)
                        return SoundPlayerStatus.Paused;

                    if (_trackToPlay.Stopped)
                        return SoundPlayerStatus.Stop;

                    return SoundPlayerStatus.Error;
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }

            return playerStatus;
        }


        public int GetVolume()
        {
            if (_trackToPlay != null)
                return _trackToPlay.Volume;

            return 0;
        }


        public void SetVolume(int volume)
        {
            if (_trackToPlay != null)
            {
                _trackToPlay.Volume = volume;
            }
        }

        #endregion





        #region Dispouse

        public void Dispose()
        {
            if (_trackToPlay != null && !_trackToPlay.Disposed)
            {
                try
                {
                    _trackToPlay.Dispose();
                    _trackToPlay = null;
                }
                catch (Exception ex)
                {
                    Library.Logs.Log.log.Error(ex);
                }
            }
        }

        #endregion
    }
}
