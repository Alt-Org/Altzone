using UnityEngine;

namespace Battle0.Scripts.Test
{
    internal class ColliderHelper : MonoBehaviour
    {
        [SerializeField] private bool _isShowTriggers;
        [SerializeField] private bool _isShowColliders;

        private void OnEnable()
        {
            // Just to be able to enable/disable us from Editor conveniently.
        }

        #region Collisions

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled || !_isShowTriggers)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            Debug.Log($"enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled || !_isShowTriggers)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            Debug.Log($"exit {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!enabled || !_isShowColliders)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = collision.gameObject;
            var layer = otherGameObject.layer;
            var material = collision.collider.sharedMaterial != null ? collision.collider.sharedMaterial.name : "NULL";
            Debug.Log($"enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)} mat {material}");
        }

        #endregion
    }
}
