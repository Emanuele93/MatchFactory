using UnityEngine.UI;
using UnityEngine;
using Services;
using Configs;

namespace Common
{
    public class BaseButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private SoundEffect soundEffect;

        private void OnEnable()
        {
            button.onClick.AddListener(() => SoundsManager.PlayEffect(soundEffect));
        }
        private void OnDisable()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
