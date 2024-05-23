using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;
using System;

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
        }

        private static SavesData _savesData;

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
        
        private static string Path => Application.persistentDataPath + "MatchFactory.saves";
        
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
                _savesData = new SavesData();
                return;
            }
            
            var formatter = new BinaryFormatter();
            var stream = new FileStream(Path, FileMode.Open);
            _savesData = formatter.Deserialize(stream) as SavesData;
            stream.Close();
        }

        internal override void Init()
        {
            Load();
        }
    }
}
