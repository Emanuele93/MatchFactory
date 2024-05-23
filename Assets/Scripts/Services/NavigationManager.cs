using System.Linq;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Services
{
    public enum Scenes
    {
        HomePage = 1,
        Settings = 2,
        GamePreview = 3,
        GamePage = 4,
        GamePause = 5,
        GameGiveUp = 6
    }

    public interface ISceneController
    {
        protected internal UniTask Open();
        protected internal UniTask Close();
    }
    
    public class NavigationManager : Service
    {
        private struct SceneController
        {
            public Scene Scene;
            public ISceneController Controller;
        }

        private static readonly Scenes[] Popups = { Scenes.Settings, Scenes.GamePreview, Scenes.GamePause, Scenes.GameGiveUp };

        private static SceneController? _currentSceneController;
        private static SceneController? _currentPopupController;
        private static bool _isOpening;
        
        internal override void Init()
        {
            _currentSceneController = null;
            _isOpening = false;
        }

        public static async void Open(Scenes scene)
        {
            if (_isOpening)
                return;
            
            _isOpening = true;

            var isPopup = Popups.Contains(scene);
            
            var closePopupTask = _currentPopupController?.Controller.Close() ?? UniTask.CompletedTask;
            var loadNewSceneTask = SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive);
            
            await closePopupTask;
            var unloadPopupSceneTask = UniTask.CompletedTask;
            var popupScene = _currentPopupController?.Scene;
            if (popupScene != null)
            {
                unloadPopupSceneTask = SceneManager.UnloadSceneAsync((Scene)popupScene).ToUniTask();
                _currentPopupController = null;
            }
            
            var closePageTask = (isPopup ? null : _currentSceneController?.Controller.Close()) ?? UniTask.CompletedTask;
            await loadNewSceneTask;

            var newScene = SceneManager.GetSceneByName(scene.ToString());
            var newSceneController = newScene.GetRootGameObjects()[0].GetComponent<ISceneController>();
            
            await closePageTask;
            var unloadPageSceneTask = UniTask.CompletedTask;
            var oldScene = _currentSceneController?.Scene;
            if (!isPopup && oldScene != null)
                unloadPageSceneTask = SceneManager.UnloadSceneAsync((Scene)oldScene).ToUniTask();

            await newSceneController.Open();
            if(isPopup)
                _currentPopupController = new SceneController { Scene = newScene, Controller = newSceneController };
            else
                _currentSceneController = new SceneController { Scene = newScene, Controller = newSceneController };
            
            await UniTask.WhenAll(unloadPageSceneTask, unloadPopupSceneTask);
            
            _isOpening = false;
        }

        public static async void ClosePopup()
        {
            var controller = _currentPopupController?.Controller;
            if (_isOpening || controller == null)
                return;

            _isOpening = true;
            
            await controller.Close();
            var oldScene = _currentPopupController?.Scene;
            if (oldScene != null)
                await SceneManager.UnloadSceneAsync((Scene)oldScene);
            _currentPopupController = null;
            
            _isOpening = false;
        }
    }
}
