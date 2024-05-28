using System.Collections.Generic;
using System.Linq;
using Services;
using Configs;
using System;

namespace Game
{
    internal class MatchLogic
    {
        private const int pickableItems = 7;
        private const int mergeableItems = 3;

        private const int missileItems = 9;
        private const int vacuumItems = 3;
        private const int hourglassSeconds = 30;
        private const int laserGunSeconds = 10;
        private const int fanItems = 30;

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

            for (var i = index; i < index + mergeableItems; i++)
                _items.Remove(_picked[i]);
            _picked.RemoveRange(index, mergeableItems);
            return (mergedItems, _picked.ToArray());
        }

        internal void OnMatchContinue(TimeSpan timeSpan)
        {
            _timerEnd = _timerEnd.Add(timeSpan);
        }

        internal bool CanUsePowerUps(PowerUps powerUps)
        {
            switch (powerUps)
            {
                case PowerUps.Missile:
                    return _items.Count > 0;
                case PowerUps.Hourglass:
                    return DateTime.UtcNow < _timerEnd;
                case PowerUps.Fan:
                    return true;
                case PowerUps.Vacuum:
                    return _items.Count > 0;
                case PowerUps.Piston:
                    return _picked.Count > 0;
                case PowerUps.LaserGun:
                    return DateTime.UtcNow < _timerEnd;
            }
            return false;
        }

        private GameItem[] RemoveRandomItems(int qty)
        {
            var removedItems = new List<GameItem>();
            var count = 0;
            var rnd = new System.Random();
            while (count < qty && count < _items.Count)
            {
                var notRemovedItems = _items.Where(i => !removedItems.Contains(i)).ToArray();
                var itemID = notRemovedItems[rnd.Next(0, notRemovedItems.Length - 1)].ID;
                removedItems.AddRange(_items.Where(i => i.ID == itemID).Take(mergeableItems));
                count += mergeableItems;
            }
            foreach (var removedItem in removedItems)
                _items.Remove(removedItem);
            return removedItems.ToArray();
        }

        internal GameItem[] UseMissilePowerUps() => RemoveRandomItems(missileItems);

        internal void UseHourglassPowerUps() => _timerEnd = _timerEnd.AddSeconds(hourglassSeconds);

        internal GameItem[] UseVacuumPowerUps() => RemoveRandomItems(vacuumItems);
        internal GameItem[] UseFanPowerUps()
        {
            var rnd = new System.Random();
            var items = _items.Where(i => !_picked.Contains(i)).ToArray();
            for (var i = 0; i < items.Length; i++)
            {
                var index = rnd.Next(0, items.Length - 1);
                (items[index], items[i]) = (items[i], items[index]);
            }
            return items.Take(Math.Min(items.Length, fanItems)).ToArray();
        }

        internal GameItem[] UsePistonPowerUps()
        {
            var lastPicked = _picked.Last();
            _picked.Remove(lastPicked);
            return new[] { lastPicked };
        }

        internal void UseLaserGunPowerUps() => _timerEnd = _timerEnd.AddSeconds(laserGunSeconds);
    }
}
