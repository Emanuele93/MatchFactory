using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Services;
using Configs;
using System;

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
            SavesManager.OnMatchRestart += StartMatch;
            SavesManager.OnMatchContinue += OnMatchContinue;
            StartMatch();
            animator.Setup();
        }

        void OnDisable()
        {
            SavesManager.OnMatchRestart -= StartMatch;
            SavesManager.OnMatchContinue -= OnMatchContinue;
        }

        private void StartMatch()
        {
            foreach (var image in goalImageContainer.GetComponentsInChildren<GameGoalImage>())
                Destroy(image.gameObject);

            foreach (var item in itemsContainer.GetComponentsInChildren<GameItem>())
                Destroy(item.gameObject);

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
            animator.Init(goalImageReferences, _matchLogic.RemainingTime);
        }

        private async void GameItemClicked(GameItem item)
        {
            if (SavesManager.LastMatch.PauseStart != null || _matchLogic == null)
                return;

            var (pickedItems, goal, lose, win) = _matchLogic.PickItem(item);
            if (pickedItems != null)
                item.ItemClicked -= GameItemClicked;
            await animator.PickItem(item, pickedItems, goal);

            if (lose)
                OnMatchLose();

            var (mergedItems, remainingItems) = _matchLogic.MergeItems(item);
            if (mergedItems != null)
                await animator.MergeItems(mergedItems, remainingItems);

            if (win)
                OnMatchWin();
        }

        private void OnMatchWin()
        {
            SavesManager.SetMatchResult(true, _matchLogic.RemainingTime);
            NavigationManager.Open(Scenes.GameWinPopup);
            _matchLogic = null;
        }

        private void OnMatchLose()
        {
            SavesManager.SetMatchResult(false);
            NavigationManager.Open(Scenes.GameLosePopup);
            _matchLogic = null;
        }

        private void Update()
        {
            if (SavesManager.LastMatch.PauseStart != null || _matchLogic == null)
                return;
            if (_matchLogic.RemainingTime.TotalMilliseconds > 0)
                animator.UpdateTimer(_matchLogic.RemainingTime, _matchLogic.TotalTime);
            else if (_matchLogic.RemainingTime.TotalMilliseconds <= 0)
                OnMatchLose();
        }

        public void Pause()
        {
            SavesManager.PauseMatch();
            NavigationManager.Open(Scenes.GamePause);
        }

        public void OnMatchContinue(TimeSpan timeSpan) => _matchLogic.OnMatchContinue(timeSpan);

        UniTask ISceneController.Open() => animator.Open();

        UniTask ISceneController.Close() => animator.Close();
    }
}
