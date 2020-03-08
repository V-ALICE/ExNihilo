using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExNihilo.Util;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace ExNihilo.Systems.Backend
{
    public static class AudioManager
    {
        private static readonly Dictionary<string, SoundEffect> Effects = new Dictionary<string, SoundEffect>();
        private static readonly Dictionary<string, Song> Songs = new Dictionary<string, Song>();

        private static Dictionary<string, List<SoundEffectInstance>> _soundLimiter;
        private static string _currentSong="none";
        private const int _maxSoundInstances = 2, _musicFadeTimeSec = 5;
        private static bool _paused, _transitioning, _killTransitionFlag;
        private static float _setVolume;
        private static int _timerID;

        public static float EffectVolume { get; set; }

        public static float MusicVolume
        {
            get => _setVolume;
            set
            {
                _setVolume = value;
                if (!_transitioning) MediaPlayer.Volume = _setVolume;
                
            }
        }

        public static void Initialize(ContentManager content)
        {
            _timerID = UniversalTime.NewTimer(true, _musicFadeTimeSec);

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

        private static void Transition(string key, bool repeating)
        {
            //Kill any other transition threads
            while (_transitioning)
            {
                _killTransitionFlag = true;
                Thread.Sleep(25);
            }

            //Prep for transition
            UniversalTime.ResetTimer(_timerID);
            UniversalTime.TurnOnTimer(_timerID);
            _killTransitionFlag = false;
            _transitioning = true;

            //Slowly adjust the volume down until the timer goes off
            if (_currentSong != "none")
            {
                while (!UniversalTime.GetAFire(_timerID))
                {
                    if (_killTransitionFlag) return;
                    var perc = UniversalTime.GetPercentageDone(_timerID);
                    MediaPlayer.Volume = (float) ((1.0 - perc) * _setVolume);
                    Thread.Sleep(100);
                }
                MediaPlayer.Stop();
            }

            //Slowly adjust the volume back up until the timer goes off again
            if (key != "none")
            {
                MediaPlayer.Volume = 0;
                _currentSong = key;
                MediaPlayer.Play(Songs[key]);
                MediaPlayer.IsRepeating = repeating;

                UniversalTime.ResetTimer(_timerID);
                while (!UniversalTime.GetAFire(_timerID))
                {
                    if (_killTransitionFlag) return;
                    var perc = UniversalTime.GetPercentageDone(_timerID);
                    MediaPlayer.Volume = (float) (perc * _setVolume);
                    Thread.Sleep(100);
                }
            }

            //Cleanup
            MediaPlayer.Volume = _setVolume;
            UniversalTime.TurnOffTimer(_timerID);
            _transitioning = false;
        }

        public static async void PlaySong(string key, bool repeating)
        {
            if (!_paused && _currentSong != key)
            {
                await Task.Run(() => Transition(key, repeating));
            }
        }

        public static void KillCurrentSong()
        {
            _currentSong = "";
            MediaPlayer.Stop();
        }
    }
}
