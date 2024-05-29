using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Services;
using Configs;
using System;
using TMPro;

namespace Game
{
    public class GamePageController : MonoBehaviour, ISceneController
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private GameGoalImage goalImagePrefab;
        [SerializeField] private Transform goalImageContainer;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private Transform pickedItemContainer;
        [SerializeField] private TextMeshProUGUI level;

        [Header("Animator")]
        [SerializeField] private GamePageAnimator animator;

        private MatchLogic _matchLogic;
        private const string keyID = "Key";

        private void OnEnable()
        {
            level.text = $"Level {SavesManager.CurrentLevel + 1}";
            SavesManager.OnMatchRestart += Restart;
            SavesManager.OnMatchContinue += OnMatchContinue;
            StartMatch();
            animator.Setup();
        }

        void OnDisable()
        {
            SavesManager.OnMatchRestart -= Restart;
            SavesManager.OnMatchContinue -= OnMatchContinue;
        }

        private async void Restart()
        {
            StartMatch();
            animator.ContinueMatch(_matchLogic.PickedWithBombDates());
            var tasks = new List<UniTask>();
            foreach (var powerUps in SavesManager.LastMatch.InitialPowerUps)
                tasks.Add(UsePowerUps(powerUps));
            await UniTask.WhenAll(tasks);
        }

        private void StartMatch()
        {
            foreach (var image in goalImageContainer.GetComponentsInChildren<GameGoalImage>())
                Destroy(image.gameObject);

            foreach (var item in itemsContainer.GetComponentsInChildren<GameItem>())
                Destroy(item.gameObject);

            var level = config.Levels.Length > SavesManager.CurrentLevel ? config.Levels[SavesManager.CurrentLevel] : config.LevelDefault;

            var gameItems = new List<GameItem>();
            var goalImageReferences = new Dictionary<string, GameGoalImage>();
            var itemsGoals = new Dictionary<string, int>();
            var rnd = new System.Random();
            var keysCount = 0;

            foreach (var levelItem in level.LevelItems)
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
                if (levelItem.item.ID == keyID)
                    keysCount += levelItem.qty;
            }

            _matchLogic = new MatchLogic(gameItems, itemsGoals, level.TimerSeconds, keysCount);
            animator.Init(goalImageReferences, _matchLogic.RemainingTime, keysCount);
        }

        private async void GameItemClicked(GameItem item)
        {
            if (SavesManager.LastMatch.PauseStart != null || _matchLogic == null)
                return;

            item.ItemClicked -= GameItemClicked;
            SoundsManager.PlayEffect(SoundEffect.PickObject);
            var (pickedItems, keys, goal, lose, win) = _matchLogic.PickItem(item);
            await animator.PickItem(item, pickedItems, goal, keys);

            if (_matchLogic == null)
                return;

            if (lose)
            {
                OnMatchLose();
                return;
            }

            var (mergedItems, remainingItems) = _matchLogic.MergeItems(item);
            if (mergedItems != null)
                await animator.MergeItems(mergedItems, remainingItems);

            if (_matchLogic == null)
                return;

            if (win)
                OnMatchWin();
            item.ItemClicked += GameItemClicked;
        }

        private void OnMatchWin()
        {
            SavesManager.SetMatchResult(true, _matchLogic.RemainingTime);
            NavigationManager.Open(Scenes.GameWinPopup);
            _matchLogic = null;
        }

        private void OnMatchLose()
        {
            animator.PauseMatch();
            SavesManager.SetMatchResult(false);
            NavigationManager.Open(Scenes.GameLosePopup);
            _matchLogic = null;
        }

        private void Update()
        {
            if (SavesManager.LastMatch.PauseStart != null || _matchLogic == null)
                return;
            if (_matchLogic.RemainingTime.TotalMilliseconds > 0)
            {
                if (!_matchLogic.IsBombExploded())
                    animator.UpdateTimers(_matchLogic.RemainingTime, _matchLogic.TotalTime);
                else
                    OnMatchLose();
            }
            else if (_matchLogic.RemainingTime.TotalMilliseconds <= 0)
                OnMatchLose();
        }

        public void Pause()
        {
            animator.PauseMatch();
            SavesManager.PauseMatch();
            NavigationManager.Open(Scenes.GamePause);
        }

        public void OnMatchContinue(TimeSpan timeSpan)
        {
            _matchLogic.OnMatchContinue(timeSpan);
            animator.ContinueMatch(_matchLogic.PickedWithBombDates());
        }

        async UniTask ISceneController.Open()
        {
            await animator.Open();
            var tasks = new List<UniTask>();
            foreach (var powerUps in SavesManager.LastMatch.InitialPowerUps)
                tasks.Add(UsePowerUps(powerUps));

            await UniTask.WhenAll(tasks);
        }

        internal async UniTask<int> UsePowerUps(PowerUps powerUps)
        {
            if (!SavesManager.PowerUps.ContainsKey(powerUps) || SavesManager.PowerUps[powerUps] <= 0)
                return 0;
            if (SavesManager.LastMatch.PauseStart != null || _matchLogic == null || !_matchLogic.CanUsePowerUps(powerUps))
                return SavesManager.PowerUps[powerUps];
            if (!SavesManager.UsePowerUps(powerUps))
                return SavesManager.PowerUps[powerUps];

            switch (powerUps)
            {
                case PowerUps.Missile:
                    await animator.UseMissilePowerUps(_matchLogic.UseMissilePowerUps);
                    break;
                case PowerUps.Hourglass:
                    await animator.UseHourglassPowerUps(_matchLogic.UseHourglassPowerUps);
                    break;
                case PowerUps.Fan:
                    await animator.UseFanPowerUps(_matchLogic.UseFanPowerUps);
                    break;
                case PowerUps.Vacuum:
                    await animator.UseVacuumPowerUps(_matchLogic.UseVacuumPowerUps);
                    break;
                case PowerUps.Piston:
                    await animator.UsePistonPowerUps(_matchLogic.UsePistonPowerUps);
                    break;
                case PowerUps.LaserGun:
                    await animator.UseLaserGunPowerUps(_matchLogic.UseLaserGunPowerUps);
                    break;
            }
            return SavesManager.PowerUps[powerUps];
        }

        UniTask ISceneController.Close() => animator.Close();
    }
}
