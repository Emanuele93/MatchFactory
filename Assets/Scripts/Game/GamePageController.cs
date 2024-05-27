using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Services;
using Configs;

namespace Game
{
    public class GamePageController : MonoBehaviour, ISceneController
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private GameGoalImage goalImagePrefab;
        [SerializeField] private Transform goalImageContainer;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private Transform pickedItemContainer;

        [Header("Animator")]
        [SerializeField] private GamePageAnimator animator;

        private MatchLogic _matchLogic;

        private void OnEnable()
        {
            var levelItems = config.Levels[SavesManager.CurrentLevel].LevelItems;

            var gameItems = new List<GameItem>();
            var goalImageReferences = new Dictionary<string, GameGoalImage>();
            var itemsGoals = new Dictionary<string, int>();
            var rnd = new System.Random();

            foreach (var levelItem in levelItems)
            {
                GameGoalImage goalImageReference = null;
                var qty = levelItem.qty * 3;
                if (levelItem.isGoal)
                {
                    goalImageReference = Instantiate(goalImagePrefab, goalImageContainer.transform);
                    goalImageReference.Image.sprite = levelItem.item.Image;
                    goalImageReference.Text.text = qty.ToString();
                    goalImageReferences[levelItem.item.ID] = goalImageReference;
                    itemsGoals[levelItem.item.ID] = qty;
                }
                for (var i = 0; i < qty; i++)
                {
                    var gameItem = Instantiate(levelItem.item,
                        new Vector3(rnd.Next(-20, 20) / 100f, rnd.Next(10, 20) / 100f, rnd.Next(-20, 20) / 100f),
                        UnityEngine.Random.rotation, itemsContainer);
                    gameItem.ItemClicked += GameItemClicked;
                    gameItems.Add(gameItem);
                }
            }

            _matchLogic = new MatchLogic(gameItems, itemsGoals, config.Levels[SavesManager.CurrentLevel].TimerSeconds);
            animator.Setup(goalImageReferences, _matchLogic.RemainingTime);
        }

        private async void GameItemClicked(GameItem item)
        {
            var (pickedItems, goal, lose, win) = _matchLogic.PickItem(item);
            if (pickedItems != null)
                item.ItemClicked -= GameItemClicked;
            await animator.PickItem(item, pickedItems, goal);

            if (lose)
                Debug.LogError("Lose");

            var (mergedItems, remainingItems) = _matchLogic.MergeItems(item);
            if (mergedItems != null)
                await animator.MergeItems(mergedItems, remainingItems);

            if (win)
                Debug.LogError("Win");
        }

        private void Update()
        {
            if (_matchLogic != null)
                animator.UpdateTimer(_matchLogic.RemainingTime, _matchLogic.TotalTime);
        }

        public void Pause() => NavigationManager.Open(Scenes.GamePause);

        UniTask ISceneController.Open() => animator.Open();

        UniTask ISceneController.Close() => animator.Close();
    }
}
