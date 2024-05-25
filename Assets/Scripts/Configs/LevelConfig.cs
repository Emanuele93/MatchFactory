using UnityEngine;
using System;

namespace Configs
{
    [Serializable] public class LevelItem
    {
        public GameItem item;
        public int qty;
        public int goalQty;
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ScriptableObjects/LevelConfig", order = 3)]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level")] 
        [SerializeField] private int timerSeconds;
        public int TimerSeconds => timerSeconds;
        
        [Header("Items will be multiplied x 3")]
        [SerializeField] private LevelItem[] levelItems;
        public LevelItem[] LevelItems => levelItems;
        
    }
}
