using System.Linq;
using UnityEngine;
using System;

namespace Services
{
    [Serializable] public enum SoundEffect
    {
        // Common
        Button,
    
        //Game
        PickObject
    }

    [CreateAssetMenu(fileName = "SoundsConfig", menuName = "ScriptableObjects/SoundsConfig", order = 1)]
    public class SoundsConfig : ScriptableObject
    {
        [Serializable] private class NamedSoundEffect
        {
            public SoundEffect soundEffect;
            public AudioClip audioClip;
        }
    
        [SerializeField] private NamedSoundEffect[] soundEffects;

        public AudioClip GetAudioClip(SoundEffect soundEffect) =>
            soundEffects.FirstOrDefault(x => x.soundEffect == soundEffect)?.audioClip;
    }
}