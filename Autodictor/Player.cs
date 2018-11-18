﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.DirectX.AudioVideoPlayback;
using System.IO;


namespace MainExample
{
    public enum SoundFileStatus
    {
        Error,
        Stop,
        Playing,
        Paused,
    };

    public static class Player
    {
        [DllImportAttribute("winmm.dll")]
        public static extern long PlaySound(String lpszName, long hModule, long dwFlags);

        private static string TrackPath = "";
        private static Audio track_to_play = null;


        public static bool PlayFile(string File)
        {
            if (track_to_play != null)
            {
                track_to_play.Dispose();
                track_to_play = null;
            }

            TrackPath = File;
            if (TrackPath == "")
                return false;

            try
            {
                if (System.IO.File.Exists(TrackPath) == true)
                {
                    track_to_play = new Audio(TrackPath);
                    SetVolume(Program.Настройки.ВыборУровняГромкости());
                    track_to_play.Play();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }



        public static void Pause()
        {
            if (track_to_play == null)
               return;
            
            try
            {
                track_to_play.Pause();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }



        public static void Play()
        {
            if (track_to_play == null)
                return;

            try
            {
                track_to_play.Play();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }



        public static float GetDuration()
        {
            try
            {
                if (track_to_play != null)
                    return (float)track_to_play.Duration;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0f;
            };

            return 0f;
        }


        public static int GetCurrentPosition()
        {
            try
            {
                if (track_to_play != null)
                    return (int)track_to_play.CurrentPosition;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            };

            return 0;
        }



        //TODO: Exceptions при геннерации списка.
        public static SoundFileStatus GetFileStatus()
        {
            SoundFileStatus fileStatus = SoundFileStatus.Error;

            try
            {
                if (track_to_play != null)
                {
                    if (track_to_play.Playing)
                    {
                        if (track_to_play.CurrentPosition >= track_to_play.Duration)
                            return SoundFileStatus.Stop;

                        return SoundFileStatus.Playing;
                    }
                    else if (track_to_play.Paused)
                        return SoundFileStatus.Paused;
                    else
                        return SoundFileStatus.Stop;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            };

            return fileStatus;
        }



        public static int GetVolume()
        {
            if (track_to_play != null)
                return track_to_play.Volume;

            return 0;
        }

        public static void SetVolume(int Volume)
        {
            if (track_to_play != null)
            {
                track_to_play.Volume = Volume;
            }
        }
    }
}
