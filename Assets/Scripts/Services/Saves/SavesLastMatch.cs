using System.Globalization;
using System.Linq;
using System;

namespace Services
{
    public class MatchData
    {
        public PowerUps[] InitialPowerUps { get; }
        public bool Win { get; internal set; }
        public TimeSpan RemainingTimer { get; internal set; }
        public DateTime? PauseStart { get; internal set; }

        internal MatchData(PowerUps[] initialPowerUps)
        {
            InitialPowerUps = initialPowerUps;
        }
    }

    public partial class SavesManager : Service
    {
        private static MatchData _lastMatch;
        public static MatchData LastMatch => _lastMatch;
        public static Action OnMatchRestart;
        public static Action<TimeSpan> OnMatchContinue;

        public static void StartNewMatch(PowerUps[] initialPowerUps)
        {
            _lastMatch = new MatchData(initialPowerUps);
            _savesData.lives--;
            _savesData.recoverLiveStart ??= DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            Save();
        }

        public static void SetMatchResult(bool win, TimeSpan remainingTimer = new TimeSpan())
        {
            if (win)
            {
                _savesData.coins += _config.MatchWinCoinsReward;
                _savesData.currentLevel++;
                _savesData.lives = Math.Min(_savesData.lives + 1, _config.MaxLives);
                if (_savesData.lives >= _config.MaxLives)
                    _savesData.recoverLiveStart = null;
                Save();
            }

            _lastMatch.Win = win;
            _lastMatch.RemainingTimer = remainingTimer;
        }

        public static void RestartMatch(PowerUps[] initialPowerUps)
        {
            StartNewMatch(initialPowerUps);
            OnMatchRestart?.Invoke();
        }

        public static void PauseMatch()
        {
            _lastMatch.PauseStart = DateTime.UtcNow;
        }

        public static void ContineMatch()
        {
            OnMatchContinue?.Invoke(DateTime.UtcNow - (DateTime)_lastMatch.PauseStart);
            _lastMatch.PauseStart = null;
        }
    }
}