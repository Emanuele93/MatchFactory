using UnityEngine;
using Services;

namespace Game
{
    public class GameGiveUpPopupController : MonoBehaviour
    {
        private void OnEnable()
        {
        }

        public void GiveUpGame()
        {
            SavesManager.LoseLive();
            NavigationManager.Open(Scenes.HomePage);
        }
    }
}
