using UnityEngine;
using Services;

namespace Game
{
    public class GameGiveUpPopupController : MonoBehaviour
    {
        public void GiveUpGame()
        {
            SavesManager.SetMatchResult(false);
            NavigationManager.Open(Scenes.GameLosePopup);
        }

        public void Coninue()
        {
            SavesManager.ContineMatch();
            NavigationManager.ClosePopup();
        }
    }
}
