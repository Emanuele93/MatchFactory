using UnityEngine.UI;
using UnityEngine;
using System;

namespace Common
{
    public class ActiveButton : MonoBehaviour
    {
        [SerializeField] private GameObject frame;
        [SerializeField] private Button button;
        [SerializeField] private bool startingState;

        public Button Button => button;

        private bool _currentState;
        public Action<bool> OnStateChange;

        private void OnEnable()
        {
            _currentState = startingState;
            button.onClick.AddListener(ChangeSatate);
            frame.SetActive(_currentState);
        }

        private void ChangeSatate()
        {
            _currentState = !_currentState;
            frame.SetActive(_currentState);
            OnStateChange?.Invoke(_currentState);
        }
    }
}