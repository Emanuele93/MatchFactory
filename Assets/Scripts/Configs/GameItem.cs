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

        public Action<GameItem> ItemClicked;
        public string ID => id;
        public Sprite Image => image;
        public Rigidbody RigidBody => rigidBody;
        public Collider Collider => itemCollider;
        public Renderer Renderer => itemRenderer;

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0))
                ItemClicked?.Invoke(this);
        }
    }
}
