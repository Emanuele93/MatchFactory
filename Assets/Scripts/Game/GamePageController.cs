using Cysharp.Threading.Tasks;
using UnityEngine;
using Services;

namespace Game
{
    public class GamePageController : MonoBehaviour, ISceneController
    {
        [Header("Config")]
        [SerializeField] private float openDuration;
        [SerializeField] private float closeDuration;

        private void OnEnable()
        {
            
        }

        private void Update()
        {
            
        }

        public void Pause () => NavigationManager.Open(Scenes.GamePause);
        
        UniTask ISceneController.Open()
        {
            // Open Animation
            return UniTask.CompletedTask;
        }

        UniTask ISceneController.Close()
        {
            // Close Animation
            return UniTask.CompletedTask;
        }
    }
}
