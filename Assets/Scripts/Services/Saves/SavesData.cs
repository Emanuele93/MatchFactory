using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;
using Configs;
using System;

namespace Services
{
    [Serializable]
    public enum PowerUps
    {
        Missile,
        Hourglass,
        Vacuum,
        Piston,
        Fan,
        LaserGun
    }

    public partial class SavesManager : Service
    {
        [Serializable]
        private class SavesDataPuwerUp
        {
            public PowerUps powerUps;
            public int qty;
        }

        [Serializable]
        private class SavesData
        {
            public bool isMusicActive = true;
            public bool isSoundEffectsActive = true;

            public int currentLevel;
            public int coins;
            public int lives;
            public string recoverLiveStart;

            public SavesDataPuwerUp[] powerUps = { };
        }

        [SerializeField] private GameConfig config;

        private static GameConfig _config;
        private static SavesData _savesData;
        private static string Path => Application.persistentDataPath + "MatchFactory.saves";
        public static Action OnDataReset;

        private static SavesData DefaultData => new()
        {
            lives = _config.MaxLives,
            coins = _config.StartingCoins,
            powerUps = new[]
            {
                new SavesDataPuwerUp {powerUps =Services.PowerUps.Missile, qty=10},
                new SavesDataPuwerUp {powerUps =Services.PowerUps.Hourglass, qty=10},
                new SavesDataPuwerUp {powerUps =Services.PowerUps.Fan, qty=5},
                new SavesDataPuwerUp {powerUps =Services.PowerUps.LaserGun, qty=5},
                new SavesDataPuwerUp {powerUps =Services.PowerUps.Piston, qty=5},
                new SavesDataPuwerUp {powerUps =Services.PowerUps.Vacuum, qty=5}
            }
        };

        public static void Reset()
        {
            var data = DefaultData;
            data.isMusicActive = _savesData.isMusicActive;
            data.isSoundEffectsActive = _savesData.isSoundEffectsActive;
            _savesData = data;
            Save();
            OnDataReset?.Invoke();
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
                _savesData = DefaultData;
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