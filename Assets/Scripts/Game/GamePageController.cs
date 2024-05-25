using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using Services;
using Configs;
using System;
using TMPro;

namespace Game
{
    public class GamePageController : MonoBehaviour, ISceneController
    {
        [Header("Config")]
        [SerializeField] private float openDuration;
        [SerializeField] private float closeDuration;
        [SerializeField] private GameConfig config;

        [Header("UI")] 
        [SerializeField] private RectTransform topUI;
        [SerializeField] private RectTransform bottomUI;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private RectTransform timerBar;
        [SerializeField] private List<GameGoalImage> goalsImages;
        
        [Header("Board")]
        [SerializeField] private GameObject board;
        [SerializeField] private Transform itemsContainer;

        private Vector3 _topUIPosition;
        private Vector3 _bottomUIPosition;
        private DateTime _timerStart;
        private DateTime _timerEnd;
        
        private void OnEnable()
        {
            board.SetActive(false);
            _topUIPosition = topUI.position;
            topUI.position += Vector3.up * topUI.rect.height; 
            _bottomUIPosition = bottomUI.position;
            bottomUI.position += Vector3.down * topUI.rect.height;

            var items = config.Levels[SavesManager.CurrentLevel].LevelItems;
            var goals = items.Where(i => i.goalQty > 0).ToArray();
            var objects = items.SelectMany(i =>
            {
                var arr = new GameItem [i.qty * 3];
                for (var j = 0; j < arr.Length; j++)
                    arr[j] = i.item;
                return arr;
            }).ToArray();
            
            var rnd = new System.Random();
            foreach (var item in objects)
                Instantiate(item, new Vector3(rnd.Next(-20, 20) / 100f, rnd.Next(10, 20) / 100f, rnd.Next(-20, 20) / 100f), UnityEngine.Random.rotation, itemsContainer);

            _timerStart = DateTime.UtcNow;
            _timerEnd = DateTime.UtcNow.AddSeconds(config.Levels[SavesManager.CurrentLevel].TimerSeconds);
            var time = _timerEnd - _timerStart;
            timerText.text = $"{time.Minutes:00}:{time.Seconds:00}";

            for (var i = 0; i < goals.Length; i++)
            {
                if (i >= goalsImages.Count)
                    goalsImages.Add(Instantiate(goalsImages[0], goalsImages[0].transform.parent));
                goalsImages[i].Image.sprite = goals[i].item.Image;
            }
        }

        private void Update()
        {
            var missingTime = _timerEnd - DateTime.UtcNow;
            timerBar.localScale = new Vector3((float)(missingTime.TotalSeconds / (_timerEnd - _timerStart).TotalSeconds), 1, 1);
            timerText.text = $"{missingTime.Minutes:00}:{missingTime.Seconds:00}";
        }

        public void Pause () => NavigationManager.Open(Scenes.GamePause);
        
        UniTask ISceneController.Open()
        {
            // Open Animation
            var openTopUITask = topUI.DOMove(_topUIPosition, openDuration).ToUniTask();
            var openBottomUITask = bottomUI.DOMove(_bottomUIPosition, openDuration).ToUniTask();
            board.SetActive(true);
            return UniTask.WhenAll(openTopUITask, openBottomUITask);
        }

        UniTask ISceneController.Close()
        {
            // Close Animation
            var closeTopUITask = topUI.DOMove(topUI.position + Vector3.up * topUI.rect.height, closeDuration).ToUniTask();
            var closeBottomUITask = bottomUI.DOMove(bottomUI.position + Vector3.down * topUI.rect.height, closeDuration).ToUniTask();
            board.SetActive(false);
            return UniTask.WhenAll(closeTopUITask, closeBottomUITask);
        }
    }
}
