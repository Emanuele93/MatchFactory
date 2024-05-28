using UnityEngine;
using System;

namespace Configs
{
    public class GameItem : MonoBehaviour
    {
        [SerializeField] string id;
        [SerializeField] private Sprite image;
        [SerializeField] private Collider itemCollider;
        [SerializeField] private Renderer itemRenderer;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Vector3 pickedPosition;
        [SerializeField] private Vector3 pickedRotation;
        [SerializeField] private Vector3 pickedScale;

        public Action<GameItem> ItemClicked;
        private Vector3 _mainScale;

        public string ID => id;
        public Sprite Image => image;
        public Rigidbody RigidBody { get => rigidBody; set => rigidBody = value; }
        public Collider Collider => itemCollider;
        public Renderer Renderer => itemRenderer;
        public Vector3 PickedPosition => pickedPosition;
        public Vector3 PickedRotation => pickedRotation;
        public Vector3 PickedScale => pickedScale;
        public Vector3 MainScale => _mainScale;

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mainScale = transform.localScale;
                ItemClicked?.Invoke(this);
            }
        }
    }
}
