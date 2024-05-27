using System.Collections.Generic;
using Configs;
using System;
using System.Linq;

namespace Game
{
    internal class MatchLogic
    {
        private const int pickableItems = 7;
        private const int mergeableItems = 3;

        private List<GameItem> _items;
        private Dictionary<string, int> _goals;
        private List<GameItem> _picked = new();

        private DateTime _timerStart;
        private DateTime _timerEnd;
        internal TimeSpan TotalTime => _timerEnd - _timerStart;
        internal TimeSpan RemainingTime => _timerEnd - DateTime.UtcNow;

        internal MatchLogic(List<GameItem> items, Dictionary<string, int> goals, int TimerSeconds)
        {
            _items = items;
            _goals = goals;

            _timerStart = DateTime.UtcNow;
            _timerEnd = DateTime.UtcNow.AddSeconds(TimerSeconds);
        }

        internal (GameItem[], int, bool, bool) PickItem(GameItem item)
        {
            if (_picked.Count >= pickableItems)
                return (null, -1, false, false);

            var lastSameItem = _picked.LastOrDefault(x => x.ID == item.ID);
            var index = _picked.Count;
            if (lastSameItem != null)
                index = _picked.IndexOf(lastSameItem) + 1;

            _picked.Add(item);
            for (var i = _picked.Count - 1; i > index; i--)
                (_picked[i], _picked[i - 1]) = (_picked[i - 1], _picked[i]);

            var itemGoal = -1;
            if (_goals.ContainsKey(item.ID))
            {
                _goals[item.ID]--;
                itemGoal = _goals[item.ID];
            }

            var win = _goals.Values.Sum() <= 0;
            var lose = !win && _picked.Count >= pickableItems && _picked.Count(i => i.ID == item.ID) < mergeableItems;

            return (_picked.ToArray(), itemGoal, lose, win);
        }

        internal (GameItem[], GameItem[]) MergeItems(GameItem item)
        {
            var index = _picked.IndexOf(item) - mergeableItems + 1;
            if (index < 0)
                return (null, null);

            var mergedItems = _picked.GetRange(index, mergeableItems).ToArray();
            if (mergedItems.Any(i => i.ID != item.ID))
                return (null, null);

            _picked.RemoveRange(index, mergeableItems);
            return (mergedItems, _picked.ToArray());
        }

        internal void OnMatchContinue(TimeSpan timeSpan)
        {
            _timerEnd = _timerEnd.Add(timeSpan);
        }
    }
}
