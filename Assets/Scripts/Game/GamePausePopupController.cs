using UnityEngine;
using Services;
using Common;

namespace Game
{
    public class GamePausePopupController : MonoBehaviour
    {
        [Header("GamePause")] 
        [SerializeField] private OnOffButton musicButton;
        [SerializeField] private OnOffButton soundsButton;
        
        private void OnEnable()
        {
            musicButton.SetState(SavesManager.IsMusicActive);
            musicButton.OnStateChange += SetMusicActive;
            soundsButton.SetState(SavesManager.IsSoundEffectsActive);
            soundsButton.OnStateChange += SetSoundEffectsActive;
        }

        private void OnDisable()
        {
            musicButton.OnStateChange -= SetMusicActive;
            soundsButton.OnStateChange -= SetSoundEffectsActive;
        }

        private static void SetMusicActive(bool value) => SavesManager.IsMusicActive = value;
        private static void SetSoundEffectsActive(bool value) => SavesManager.IsSoundEffectsActive = value;

        public void GiveUpGame() => NavigationManager.Open(Scenes.GameGiveUp);
    }
}
