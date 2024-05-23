using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using Services;

namespace Common
{
    public class PopupController : MonoBehaviour, ISceneController
    {
        [Header("Config")]
        [SerializeField] private float openDuration;
        [SerializeField] private float closeDuration;

        [Header("Popup")] 
        [SerializeField] private Image background;
        [SerializeField] private RectTransform popup;
        
        private Color _backgroundColor;

        private void OnEnable()
        {
            _backgroundColor = background.color;
            background.color = new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 0);
            popup.localScale = Vector3.zero;
        }

        public void ClosePopup() => NavigationManager.ClosePopup();
        
        UniTask ISceneController.Open()
        {
            // Open Animation
            var fadeBackground = background.DOColor(_backgroundColor, openDuration).ToUniTask();
            var scalePopup = popup.DOScale(Vector3.one, openDuration).ToUniTask();
            return UniTask.WhenAll(fadeBackground, scalePopup);
        }

        UniTask ISceneController.Close()
        {
            // Close Animation
            var fadeBackground = background.DOFade(0, openDuration).ToUniTask();
            var scalePopup = popup.DOScale(Vector3.zero, openDuration).ToUniTask();
            return UniTask.WhenAll(fadeBackground, scalePopup);
        }
    }
}
