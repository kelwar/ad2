﻿using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AutodictorBL.Entites;
using AutodictorBL.Settings.XmlSound;
using AutodictorBL.Sound.Converters;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutodictorBL.Sound
{
    public enum SoundPlayerStatus
    {
        Idle,
        Error,
        Stop,
        Playing,
        Paused,
    };

    public interface ISoundPlayer : IDisposable
    {
        Task PlayMessage(Queue<ВоспроизводимоеСообщение> templateQueue);
        Task<bool> PlayFile(ВоспроизводимоеСообщение soundMessage, bool useFileNameConverter = true);
        Task<bool> Play();
        void Pause();
        void Stop();
        float GetDuration();
        int GetCurrentPosition();
        SoundPlayerStatus GetPlayerStatus();
        int GetVolume();
        void SetVolume(int volume);


        Task ReConnect();
        SoundPlayerType PlayerType { get; }
        bool IsConnect { get; }
        string GetInfo();
        string StatusString { get; }

        IFileNameConverter FileNameConverter { get; }  // конверетер имени проигрываемого файла в имя понятное конкретному плееру 

        Subject<string> StatusStringChangeRx { get; }  //Изменение StatusString
        Subject<bool> IsConnectChangeRx { get; }       //Изменение IsConnect
    }
}