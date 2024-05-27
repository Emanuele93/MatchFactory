using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using Services;
using Common;
using TMPro;

namespace Game
{
    public class GameLosePopupController : MonoBehaviour
    {
        [Header("GameLose")]
        [SerializeField] private RectTransform popup;
        [SerializeField] private Button retryButton;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private TextMeshProUGUI missileQty;
        [SerializeField] private ActiveButton missileButton;
        [SerializeField] private TextMeshProUGUI hourglassQty;
        [SerializeField] private ActiveButton hourglassButton;

        private HashSet<PowerUps> _initialPowerUps = new();

        private void OnEnable()
        {
            var powerUps = SavesManager.PowerUps;
            missileQty.text = (powerUps.ContainsKey(PowerUps.Missile) ? powerUps[PowerUps.Missile] : 0).ToString();
            hourglassQty.text = (powerUps.ContainsKey(PowerUps.Hourglass) ? powerUps[PowerUps.Hourglass] : 0).ToString();

            if (!powerUps.ContainsKey(PowerUps.Missile) || powerUps[PowerUps.Missile] <= 0)
                missileButton.Button.interactable = false;
            if (!powerUps.ContainsKey(PowerUps.Hourglass) || powerUps[PowerUps.Hourglass] <= 0)
                hourglassButton.Button.interactable = false;

            hourglassButton.OnStateChange += (value) =>
            {
                if (value)
                    AddInitialPowerUps(PowerUps.Hourglass);
                else
                    RemoveInitialPowerUps(PowerUps.Hourglass);
            };
            missileButton.OnStateChange += (value) =>
            {
                if (value)
                    AddInitialPowerUps(PowerUps.Missile);
                else
                    RemoveInitialPowerUps(PowerUps.Missile);
            };

            var (lives, _) = SavesManager.Lives;
            if (lives <= 0)
            {
                retryButton.gameObject.SetActive(false);
                popup.sizeDelta = new Vector2(popup.sizeDelta.x, popup.sizeDelta.y - 200);
                message.text = "You lost the match";
                missileButton.gameObject.SetActive(false);
                hourglassButton.gameObject.SetActive(false);
            }
        }

        public void AddInitialPowerUps(PowerUps powerUps) => _initialPowerUps.Add(powerUps);

        public void RemoveInitialPowerUps(PowerUps powerUps) => _initialPowerUps.Remove(powerUps);

        public void RestartMatch()
        {
            SavesManager.RestartMatch(_initialPowerUps.ToArray());
            NavigationManager.ClosePopup();
        }

        public void Exit()
        {
            NavigationManager.Open(Scenes.HomePage);
        }
    }
}
