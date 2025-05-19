using Windows.Media.Playback;
using Windows.Storage;
using System;
using System.Threading.Tasks;

namespace LVS_Gauss_Busters.Helpers
{
    public static class AudioHelper
    {
        private static MediaPlayer? bgPlayer;

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

        public static void SetBackgroundMusicMuted(bool muted)
        {
            if (bgPlayer != null)
            {
                bgPlayer.Volume = muted ? 0.0 : 0.5;
            }
        }

        public static async Task PlaySoundEffectAsync(string filePath, double volume = 5.0)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/Audio/bust_it.mp3"));
                MediaPlayer sfxPlayer = new MediaPlayer();
                sfxPlayer.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                sfxPlayer.Volume = volume;
                sfxPlayer.Play();
                System.Diagnostics.Debug.WriteLine($"Playing sound: {"Assets/Audio/bust_it.mp3"}, Volume: {volume}"); // Add this line
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing sound {"Assets/Audio/bust_it.mp3"}: {ex.Message}"); // Keep this
            }
        }
    }
}