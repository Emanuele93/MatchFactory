using UnityEngine;

namespace Configs
{
    public class GameItem : MonoBehaviour
    {
        [SerializeField] private Sprite image;
        public Sprite Image => image;
    }
}
