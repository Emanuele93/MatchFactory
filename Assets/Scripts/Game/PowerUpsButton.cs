using UnityEngine;
using Services;
using TMPro;
using UnityEngine.UI;

namespace Game
{
    public class PowerUpsButton : MonoBehaviour
    {
        [SerializeField] private GamePageController gamePageController;
        [SerializeField] private PowerUps powerUps;
        [SerializeField] private TextMeshProUGUI quantity;
        [SerializeField] private Button button;

        private void OnEnable()
        {
            quantity.text = SavesManager.PowerUps.ContainsKey(powerUps) ? SavesManager.PowerUps[powerUps].ToString() : "0";
        }

        public async void UsePowerUps()
        {
            button.interactable = false;
            quantity.text = (await gamePageController.UsePowerUps(powerUps)).ToString();
            button.interactable = true;
        }
    }
}
