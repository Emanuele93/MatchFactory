using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Game
{
    public class GameGoalImage: MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI text;

        public Image Image => image;
        public TextMeshProUGUI Text => text;
    }
}