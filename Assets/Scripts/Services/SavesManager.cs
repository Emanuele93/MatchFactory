using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using Configs;

namespace Services
{
    public class SavesManager : Service
    {
        [Serializable] private class SavesData
        {
            public bool isMusicActive = true;
            public bool isSoundEffectsActive = true;

            public int currentLevel;
            public int starsCollected;

            public int coins;
            public int lives;
            public string recoverLiveStart;
        }

        [SerializeField] private GameConfig config;
        
        private static GameConfig _config;
        private static SavesData _savesData;
        private static string Path => Application.persistentDataPath + "MatchFactory.saves";

        public static Action<bool> OnMusicActiveChange;
        public static bool IsMusicActive
        {
            get => _savesData.isMusicActive;
            set { _savesData.isMusicActive = value; Save(); OnMusicActiveChange.Invoke(value); }
        }
        public static bool IsSoundEffectsActive
        {
            get => _savesData.isSoundEffectsActive;
            set { _savesData.isSoundEffectsActive = value; Save(); }
        }

        public static int CurrentLevel => _savesData.currentLevel;
        public static int StarsCollected => _savesData.starsCollected;
        public static void CompleteLevel(int newStarsCollected)
        {
            _savesData.currentLevel++;
            _savesData.starsCollected += newStarsCollected;
            Save();
        }

        public static int Coins
        {
            get => _savesData.coins;
            set { _savesData.coins = value; Save(); }
        }

        public static (int, int) Lives
        {
            get
            {
                if (_savesData.recoverLiveStart == null)
                    return (_savesData.lives, 0);

                var recoverLiveStart = DateTime.Parse(_savesData.recoverLiveStart, CultureInfo.InvariantCulture);
                var timeSpan = (DateTime.UtcNow - recoverLiveStart).TotalSeconds;
                var recoveredLives = Convert.ToInt32(Math.Floor(timeSpan / _config.RecoverLiveSecondsDuration));

                if (recoveredLives == 0)
                    return ((int, int))(_savesData.lives, _config.RecoverLiveSecondsDuration - timeSpan);

                var consumedSeconds = recoveredLives * _config.RecoverLiveSecondsDuration;
                _savesData.lives = Math.Min(_config.MaxLives, _savesData.lives + recoveredLives);
                _savesData.recoverLiveStart = _savesData.lives < _config.MaxLives
                    ? recoverLiveStart.AddSeconds(consumedSeconds).ToString(CultureInfo.InvariantCulture)
                    : null;
                
                Save();
                
                return ((int, int))(_savesData.lives, _savesData.lives < _config.MaxLives ? timeSpan - consumedSeconds : 0);
            }
        }

        public static void LoseLive()
        {
            _savesData.lives--;
            _savesData.recoverLiveStart ??= DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            Save();
        }
        
        private static void Save()
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(Path, FileMode.Create);
            formatter.Serialize(stream, _savesData);
            stream.Close();
        }

        private static void Load()
        {
            if (!File.Exists(Path))
            {
                _savesData = new SavesData
                {
                    coins = _config.StartingCoins,
                    lives = _config.MaxLives
                };
                return;
            }
            
            var formatter = new BinaryFormatter();
            var stream = new FileStream(Path, FileMode.Open);
            _savesData = formatter.Deserialize(stream) as SavesData;
            stream.Close();
        }

        internal override void Init()
        {
            _config = config;
            Load();
        }
    }
}
