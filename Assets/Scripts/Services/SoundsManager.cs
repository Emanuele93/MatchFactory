using Configs;
using UnityEngine;

namespace Services
{
    public class SoundsManager : Service
    {
        [SerializeField] private SoundsConfig config;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource[] effectSources;
    
        private static SoundsConfig _config;
        private static AudioSource _musicSource;
        private static AudioSource[] _effectSources;
        private static int _audioSourceIndex;

        internal override void Init()
        {
            _config = config;
            _musicSource = musicSource;
            _effectSources = effectSources;
            _audioSourceIndex = 0;
        }

        public static void PlayMusic()
        {
            if (SavesManager.IsMusicActive)
                _musicSource.Play();
        }

        public static void PlayEffect(SoundEffect soundEffect)
        {
            if (!SavesManager.IsSoundEffectsActive)
                return;
            
            _audioSourceIndex = (_audioSourceIndex + 1) % _effectSources.Length;
            _effectSources[_audioSourceIndex].clip = _config.GetAudioClip(soundEffect);
            _effectSources[_audioSourceIndex].Play();
        }

        private static void OnSoundActiveChange(bool isSoundActive)
        {
            if (isSoundActive)
                _musicSource.Play();
            else
                _musicSource.Stop();
        }
        
        private void OnEnable()
        {
            SavesManager.OnMusicActiveChange += OnSoundActiveChange;
        }

        private void OnDisable()
        {
            SavesManager.OnMusicActiveChange -= OnSoundActiveChange;
        }
    }
}
