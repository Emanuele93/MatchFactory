using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System;

namespace Services
{
    public partial class SavesManager : Service
    {
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

        public static Dictionary<PowerUps, int> PowerUps => _savesData.powerUps?.ToDictionary(p => p.powerUps, p => p.qty) ?? new();

        public static bool UsePowerUps(PowerUps powerUps)
        {
            var data = _savesData.powerUps.FirstOrDefault(p => p.powerUps == powerUps);
            if (data == null || data.qty <= 0)
                return false;
            data.qty--;
            Save();
            return true;
        }

    }
}
