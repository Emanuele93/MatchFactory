using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using Services;
using Configs;
using System;
using TMPro;

namespace Game
{
    public class GamePageAnimator : MonoBehaviour
    {
        [Serializable]
        private class PowerUpsSprite
        {
            public PowerUps powerUps;
            public Sprite sprite;
        }

        [Header("Config")]
        [SerializeField] private float openDuration;
        [SerializeField] private float closeDuration;
        [SerializeField] private PowerUpsSprite[] powerUpsSprite;

        [Header("UI")]
        [SerializeField] private RectTransform topUI;
        [SerializeField] private RectTransform bottomUI;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private RectTransform timerBar;
        [SerializeField] private RectTransform goalImageContainer;
        [SerializeField] private RectTransform powerUpsContainer;
        [SerializeField] private Image powerUpsPrefab;

        [Header("Board")]
        [SerializeField] private GameObject board;
        [SerializeField] private Transform pickedItemContainer;
        [SerializeField] private Transform[] pickedItemStands;

        private Vector3 _topUIPosition;
        private Vector3 _bottomUIPosition;
        private Vector3 _pickedItemsPosition;

        private Dictionary<string, GameGoalImage> _goalImages;

        internal void Init(Dictionary<string, GameGoalImage> goalImages, TimeSpan time)
        {
            _goalImages = goalImages;
            timerText.text = $"{time.Minutes:00}:{time.Seconds:00}";
        }

        internal void Setup()
        {
            board.SetActive(false);
            _topUIPosition = topUI.position;
            topUI.position += Vector3.up * topUI.rect.height;
            _bottomUIPosition = bottomUI.position;
            bottomUI.position += Vector3.down * topUI.rect.height;
            _pickedItemsPosition = pickedItemContainer.position;
            pickedItemContainer.position += Vector3.up;
        }

        internal UniTask Open()
        {
            var openTopUITask = topUI.DOMove(_topUIPosition, openDuration).ToUniTask();
            var openBottomUITask = bottomUI.DOMove(_bottomUIPosition, openDuration).ToUniTask();
            board.SetActive(true);
            pickedItemContainer.DOMove(_pickedItemsPosition, openDuration);
            return UniTask.WhenAll(openTopUITask, openBottomUITask);
        }

        internal UniTask Close()
        {
            var closeTopUITask = topUI.DOMove(topUI.position + Vector3.up * topUI.rect.height, closeDuration).ToUniTask();
            var closeBottomUITask = bottomUI.DOMove(bottomUI.position + Vector3.down * topUI.rect.height, closeDuration).ToUniTask();
            board.SetActive(false);
            return UniTask.WhenAll(closeTopUITask, closeBottomUITask);
        }

        internal void UpdateTimer(TimeSpan missingTime, TimeSpan totalTime)
        {
            timerBar.localScale = new Vector3((float)(missingTime.TotalSeconds / totalTime.TotalSeconds), 1, 1);
            timerText.text = $"{missingTime.Minutes:00}:{missingTime.Seconds:00}";
        }

        internal async UniTask PickItem(GameItem item, GameItem[] pickedItems, int goal)
        {
            if (pickedItems == null)
            {
                await item.transform.DOMove(Vector3.one * 0.002f, .4f);
                return;
            }

            await item.transform.DOScale(item.transform.localScale * 1.2f, .2f);
            item.Collider.enabled = false;
            Destroy(item.RigidBody);

            var movements = new List<UniTask>()
            {
                item.transform.DOScale(item.PickedScale, .4f).ToUniTask(),
                item.transform.DORotate(item.PickedRotation, .4f).ToUniTask()
            };
            for (var i = 0; i < pickedItems.Length; i++)
                movements.Add(pickedItems[i].transform.DOMove(new Vector3(pickedItemStands[i].position.x, pickedItems[i].PickedPosition.y, pickedItems[i].PickedPosition.z), .4f).ToUniTask());

            await UniTask.WhenAll(movements);

            if (_goalImages.ContainsKey(item.ID))
            {
                _goalImages[item.ID].Text.text = goal.ToString();
                if (goal <= 0)
                    DestroyGoalImage(item.ID);
            }
        }

        private async void DestroyGoalImage(string itemID)
        {
            await UniTask.Delay(300);
            await _goalImages[itemID].transform.DOScale(Vector3.zero, 1f);
            Destroy(_goalImages[itemID].gameObject);
        }

        internal async UniTask MergeItems(GameItem[] mergedItems, GameItem[] remainingItems)
        {
            var movements = new List<UniTask>();
            var mergePosition = new Vector3(
                mergedItems.Sum(x => x.transform.position.x) / mergedItems.Length,
                mergedItems.Sum(x => x.transform.position.y) / mergedItems.Length,
                mergedItems.Sum(x => x.transform.position.z) / mergedItems.Length);

            foreach (var mergedItem in mergedItems)
                movements.Add(mergedItem.transform.DOMove(mergePosition, .4f).ToUniTask());
            await UniTask.WhenAll(movements);
            foreach (var mergedItem in mergedItems)
                Destroy(mergedItem.gameObject);

            for (var i = 0; i < remainingItems.Length; i++)
                movements.Add(remainingItems[i].transform.DOMoveX(pickedItemStands[i].position.x, .4f).ToUniTask());
        }

        private async UniTask ActivePowerUps(PowerUps powerUps, Func<UniTask> func)
        {
            var openTasks = new List<UniTask>();
            var image = Instantiate(powerUpsPrefab, powerUpsContainer).GetComponentsInChildren<Image>()[1];
            image.sprite = powerUpsSprite.First(p => p.powerUps == powerUps).sprite;
            image.rectTransform.parent.localScale = Vector3.zero;
            openTasks.Add(image.rectTransform.parent.DOScale(Vector3.one, .4f).ToUniTask());
            openTasks.Add(UniTask.Delay(800));
            await UniTask.Delay(300);
            var funcTask = func.Invoke();
            await UniTask.WhenAll(openTasks);
            await funcTask;
            await image.rectTransform.parent.DOScale(Vector3.zero, .4f);
            Destroy(image.rectTransform.parent.gameObject);
        }

        private async UniTask DestroyItem(GameItem item)
        {
            await item.transform.DOScale(item.transform.localScale * 1.2f, .2f);
            Destroy(item.gameObject.GetComponent<Rigidbody>());
            item.Collider.enabled = false;
            var task = item.transform.DOMove(new Vector3(-0.001f, 0.655f, 0.035f), .6f).ToUniTask();
            await item.transform.DOScale(Vector3.zero, .3f);
            await task;
            Destroy(item);
        }

        internal UniTask UseMissilePowerUps(Func<GameItem[]> destroyItemsFunc) => ActivePowerUps(PowerUps.Missile, async () =>
        {
            var items = destroyItemsFunc.Invoke();
            var tasks = new List<UniTask>();
            foreach (var item in items)
                tasks.Add(DestroyItem(item));
            await UniTask.WhenAll(tasks);
        });
        internal UniTask UseHourglassPowerUps(Action changeTimerFunc) => ActivePowerUps(PowerUps.Hourglass, async () =>
        {
            await timerText.transform.DOScale(Vector3.one * 1.2f, .3f);
            changeTimerFunc.Invoke();
            await timerText.transform.DOScale(Vector3.one, .3f);
        });
        internal UniTask UseFanPowerUps(Func<GameItem[]> selectRandomItems) => ActivePowerUps(PowerUps.Fan, async () =>
        {
            var items = selectRandomItems();
            var tasks = new List<UniTask>();
            foreach (var item in items)
            {
                tasks.Add(item.transform.DOMove(Vector3.one * 0.002f, .4f).ToUniTask());
                await UniTask.Delay(100);
            }
            await UniTask.WhenAll(tasks);
        });
        internal UniTask UseVacuumPowerUps(Func<GameItem[]> destroyItemsFunc) => ActivePowerUps(PowerUps.Vacuum, async () =>
        {
            var items = destroyItemsFunc.Invoke();
            var tasks = new List<UniTask>();
            foreach (var item in items)
                tasks.Add(DestroyItem(item));
            await UniTask.WhenAll(tasks);
        });
        internal UniTask UsePistonPowerUps(Func<GameItem[]> removeItemsFunc) => ActivePowerUps(PowerUps.Piston, async () =>
        {
            var items = removeItemsFunc.Invoke();
            foreach (var item in items)
            {
                var scaleTask = item.transform.DOScale(item.MainScale, .4f);
                await item.transform.DOMove(new Vector3(-0.001f, 0.655f, 0.035f), .4f);
                await scaleTask;
                item.Collider.enabled = true;
                item.RigidBody = item.gameObject.AddComponent<Rigidbody>();
            }
        });
        internal UniTask UseLaserGunPowerUps(Action changeTimerFunc) => ActivePowerUps(PowerUps.LaserGun, async () =>
        {
            await timerText.transform.DOScale(Vector3.one * 1.2f, .3f);
            changeTimerFunc.Invoke();
            await timerText.transform.DOScale(Vector3.one, .3f);
        });
    }
}