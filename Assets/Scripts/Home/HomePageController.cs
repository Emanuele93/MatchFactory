using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Services;
using System;
using TMPro;

namespace Home
{
    public class HomePageController : MonoBehaviour, ISceneController
    {
        [Header("Config")]
        [SerializeField] private float openDuration;
        [SerializeField] private float closeDuration;

        [Header("Home")]
        [SerializeField] private RectTransform header;
        [SerializeField] private RectTransform playButton;
        [SerializeField] private RectTransform levelImage;
        [SerializeField] private TextMeshProUGUI livesTimer;
        [SerializeField] private TextMeshProUGUI livesCount;
        [SerializeField] private TextMeshProUGUI coins;
        [SerializeField] private TextMeshProUGUI currentLevel;

        private Vector3 _headerPosition;
        private Vector3 _playButtonPosition;

        private void SetLivesValues()
        {
            var (lCount, lTimer) = SavesManager.Lives;
            if (lTimer == 0)
            {
                livesCount.text = lCount.ToString();
                livesTimer.text = "Max";
                return;
            }
            var minutes = Convert.ToInt32(Math.Floor(lTimer / 60f));
            var seconds = lTimer - minutes * 60;
            livesCount.text = lCount.ToString();
            livesTimer.text = $"{minutes:00}:{seconds:00}";
        }
        
        private void OnEnable()
        {
            // SetUp Initial Position 
            _headerPosition = header.position;
            header.position += Vector3.up * header.rect.height * 2;
            _playButtonPosition = playButton.position;
            playButton.position += Vector3.down * playButton.rect.height * 2;
            levelImage.localScale = Vector3.zero;
            
            // SetUp values
            SetLivesValues();
            coins.text = SavesManager.Coins.ToString();
            currentLevel.text = (SavesManager.CurrentLevel + 1).ToString();
        }

        public void OpenSettings () => NavigationManager.Open(Scenes.Settings);
        public void PlayMatch () => NavigationManager.Open(Scenes.GamePreview);
        
        private void Update()
        {
            SetLivesValues();
        }

        UniTask ISceneController.Open()
        {
            // Open Animation
            var moveHeader = header.DOMove(_headerPosition, openDuration).ToUniTask();
            var movePlayButton = playButton.DOMove(_playButtonPosition, openDuration).ToUniTask();
            var scaleLevelImage = levelImage.DOScale(Vector3.one, openDuration).ToUniTask();
            return UniTask.WhenAll(moveHeader, movePlayButton, scaleLevelImage);
        }

        UniTask ISceneController.Close()
        {
            // Close Animation
            var moveHeader = header.DOMove(header.position + Vector3.up * header.position.y * 2, closeDuration).ToUniTask();
            var movePlayButton = playButton.DOMove(playButton.position + Vector3.down * playButton.position.y * 2, closeDuration).ToUniTask();
            var scaleLevelImage = levelImage.DOScale(Vector3.zero, closeDuration).ToUniTask();
            return UniTask.WhenAll(moveHeader, movePlayButton, scaleLevelImage);
        }
    }
}
