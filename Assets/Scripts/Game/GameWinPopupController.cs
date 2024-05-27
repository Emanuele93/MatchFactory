using UnityEngine;
using Services;
using Configs;
using TMPro;

namespace Game
{
    public class GameWinPopupController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        [Header("GameWin")]
        [SerializeField] private TextMeshProUGUI timer;
        [SerializeField] private TextMeshProUGUI winMessage;

        private void OnEnable()
        {
            timer.text = $"{SavesManager.LastMatch.RemainingTimer.Minutes.ToString("00")}:{SavesManager.LastMatch.RemainingTimer.Seconds.ToString("00")}";
            winMessage.text = $"Congrats you won {config.MatchWinCoinsReward} coins";
        }

        public void Contine() => NavigationManager.Open(Scenes.HomePage);
    }
}
