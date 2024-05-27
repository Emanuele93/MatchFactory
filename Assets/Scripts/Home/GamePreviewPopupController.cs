using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Services;
using Common;
using TMPro;

namespace Home
{
    public class GamePreviewPopupController : MonoBehaviour
    {
        [Header("GamePreview")]
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI missileQty;
        [SerializeField] private ActiveButton missileButton;
        [SerializeField] private TextMeshProUGUI hourglassQty;
        [SerializeField] private ActiveButton hourglassButton;

        private HashSet<PowerUps> _initialPowerUps = new();

        private void OnEnable()
        {
            title.text = $"Level {SavesManager.CurrentLevel + 1}";

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
        }

        public void AddInitialPowerUps(PowerUps powerUps) => _initialPowerUps.Add(powerUps);

        public void RemoveInitialPowerUps(PowerUps powerUps) => _initialPowerUps.Remove(powerUps);

        public void PlayMatch()
        {
            SavesManager.StartNewMatch(_initialPowerUps.ToArray());
            NavigationManager.Open(Scenes.GamePage);
        }
    }
}
