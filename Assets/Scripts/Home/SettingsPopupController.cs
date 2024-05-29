using UnityEngine;
using Services;
using Common;

namespace Home
{
    public class SettingsPopupController : MonoBehaviour
    {
        [Header("Settings")]
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

        public void ResetSaves() => SavesManager.Reset();

        public void CloseApp() => Application.Quit();
    }
}
