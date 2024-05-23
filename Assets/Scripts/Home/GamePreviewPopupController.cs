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
            title.text = SavesManager.CurrentLevel.ToString();
        }

        public void PlayMatch() => NavigationManager.Open(Scenes.GamePage);
    }
}
