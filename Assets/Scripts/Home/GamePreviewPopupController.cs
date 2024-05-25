using UnityEngine;
using Services;
using TMPro;

namespace Home
{
    public class GamePreviewPopupController : MonoBehaviour
    {
        [Header("GamePreview")] 
        [SerializeField] private TextMeshProUGUI title;
        
        private void OnEnable()
        {
            title.text = $"Level {SavesManager.CurrentLevel + 1}";
        }

        public void PlayMatch() => NavigationManager.Open(Scenes.GamePage);
    }
}
