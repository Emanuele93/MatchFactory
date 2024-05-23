using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/GameConfig", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private int startingCoins;
        [SerializeField] private int maxLives;
        [SerializeField] private int recoverLiveSecondsDuration;

        public int StartingCoins => startingCoins;
        public int MaxLives => maxLives;
        public int RecoverLiveSecondsDuration => recoverLiveSecondsDuration;
    }
}