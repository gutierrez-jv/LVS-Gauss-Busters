﻿using System;
using System.Threading.Tasks;
using Windows.Devices.Radios;
using Windows.Media.Playback;
using Windows.Storage;

namespace LVS_Gauss_Busters.Helpers
{
    public static class AudioHelper
    {
        private static MediaPlayer? bgPlayer;
        private static MediaPlayer? sfxPlayer;

        public static void PlayBackgroundMusic(string filePath, bool loop = true, double volume = 0.5)
        {
            bgPlayer ??= new MediaPlayer();
            bgPlayer.Source = Windows.Media.Core.MediaSource.CreateFromUri(new Uri($"ms-appx:///Assets/Audio/bg_music.mp3"));
            bgPlayer.IsLoopingEnabled = loop;
            bgPlayer.Volume = volume;
            bgPlayer.Play();
        }

        public static void StopBackgroundMusic()
        {
            bgPlayer?.Pause();
        }

        public static async Task PlaySoundEffect(string filePath, double volume = 1.0)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{filePath}"));
                sfxPlayer = new MediaPlayer();
                sfxPlayer.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                sfxPlayer.Volume = volume;
                sfxPlayer.Play();

                sfxPlayer.PlaybackSession.PlaybackStateChanged += (s, args) =>
                {
                    if (s.PlaybackState == MediaPlaybackState.None ||
                        s.PlaybackState == MediaPlaybackState.Paused)
                    {
                        sfxPlayer.Dispose();
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing sound {filePath}: {ex.Message}");
            }
        }
    }
}
