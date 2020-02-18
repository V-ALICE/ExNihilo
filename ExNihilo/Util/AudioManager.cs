using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace ExNihilo.Util
{
    public static class AudioManager
    {
        //TODO: this is all just temp
        private static readonly Dictionary<string, SoundEffect> Effects = new Dictionary<string, SoundEffect>();
        private static readonly Dictionary<string, Song> Songs = new Dictionary<string, Song>();

        private static Dictionary<string, List<SoundEffectInstance>> _soundLimiter;
        private static string _currentSong;
        private const int _maxSoundInstances = 2;
        private static bool _paused;

        public static float EffectVolume { get; set; }
        public static float MusicVolume
        {
            get => MediaPlayer.Volume;
            set => MediaPlayer.Volume = value;
        }

        public static void Initialize(ContentManager content)
        {
            _soundLimiter = new Dictionary<string, List<SoundEffectInstance>>();
            foreach (var word in Effects.Keys)
            {
                _soundLimiter.Add(word, new List<SoundEffectInstance>(_maxSoundInstances));
            }

            Songs.Add("Void", content.Load<Song>("Music/void"));
            Songs.Add("Title", content.Load<Song>("Music/title"));
            Songs.Add("Outerworld", content.Load<Song>("Music/outerworld"));
        }

        public static void Pause(bool on)
        {
            _paused = on;
            if (_paused) MediaPlayer.Pause();
            else MediaPlayer.Resume();
        }

        public static void PlayEffect(string key)
        {
            if (_soundLimiter[key].Count >= _maxSoundInstances)
            {
                _soundLimiter[key][0].Stop();
                _soundLimiter[key][0].Dispose();
                _soundLimiter[key].RemoveAt(0);
            }
            var g = Effects[key].CreateInstance();
            g.Volume = EffectVolume;
            _soundLimiter[key].Add(g);
            g.Play();
        }

        public static void PlaySong(string key, bool repeating)
        {
            if (_paused) return;
            if (_currentSong != key)
            {
                MediaPlayer.Stop();
                _currentSong = key;
                MediaPlayer.Play(Songs[key]);
                MediaPlayer.IsRepeating = repeating;
            }
        }

        public static void KillCurrentSong()
        {
            _currentSong = "";
            MediaPlayer.Stop();
        }
    }
}
