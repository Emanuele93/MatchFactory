using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Services
{
    public enum Scenes
    {
        HomePage = 1,
        GamePage = 2
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

        private static SceneController? _currentSceneController;
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
            var closeTask = _currentSceneController?.Controller.Close();
            await SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive);

            var newScene = SceneManager.GetSceneByName(scene.ToString());
            var newSceneController = newScene.GetRootGameObjects()[0].GetComponent<ISceneController>();
            var openTask = newSceneController.Open();
            
            if (closeTask != null)
                await (UniTask)closeTask;

            UniTask? unloadSceneTask = null;
            var oldScene = _currentSceneController?.Scene;
            if (oldScene != null)
                unloadSceneTask = SceneManager.UnloadSceneAsync((Scene)oldScene).ToUniTask();

            _currentSceneController = new SceneController { Scene = newScene, Controller = newSceneController };
            await openTask;
            
            if (unloadSceneTask != null)
                await (UniTask)unloadSceneTask;
            _isOpening = false;
        }
    }
}
