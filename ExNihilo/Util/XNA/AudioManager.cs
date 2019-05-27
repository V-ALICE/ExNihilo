using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace ExNihilo.Util.XNA
{
    public class AudioManager
    {
        private readonly Dictionary<string, SoundEffect> _sounds;
        private readonly Dictionary<string, List<SoundEffectInstance>> _soundLimiter;
        private readonly Dictionary<string, Song> _songs;
        private string currentSong;
        private const int MaxSoundInstances = 2;
        private bool paused;

        public float EffectVolume { private get; set; }
        public float MusicVolume
        {
            private get => MediaPlayer.Volume;
            set => MediaPlayer.Volume = value;
        }

        public AudioManager(ContentManager content, Dictionary<string, Song> songs, Dictionary<string, SoundEffect> effects)
        {
            _songs = songs;
            _sounds = effects;
            _soundLimiter = new Dictionary<string, List<SoundEffectInstance>>();
            foreach (var word in _sounds.Keys)
            {
                _soundLimiter.Add(word, new List<SoundEffectInstance>(MaxSoundInstances));
            }
            EffectVolume = MusicVolume = 0.75f;
        }

        public void Pause()
        {
            paused = !paused;
            if (paused) MediaPlayer.Pause();
            else MediaPlayer.Resume();
        }

        public void PlayEffect(string key)
        {
            if (_soundLimiter[key].Count >= MaxSoundInstances)
            {
                _soundLimiter[key][0].Stop();
                _soundLimiter[key][0].Dispose();
                _soundLimiter[key].RemoveAt(0);
            }
            var g = _sounds[key].CreateInstance();
            g.Volume = EffectVolume;
            _soundLimiter[key].Add(g);
            g.Play();
        }

        public void PlaySong(string key, bool repeating)
        {
            if (paused) return;
            if (currentSong != key)
            {
                MediaPlayer.Stop();
                currentSong = key;
                MediaPlayer.Play(_songs[key]);
                MediaPlayer.IsRepeating = repeating;
            }
        }

        public void KillCurrentSong()
        {
            currentSong = "";
            MediaPlayer.Stop();
        }
    }
}
