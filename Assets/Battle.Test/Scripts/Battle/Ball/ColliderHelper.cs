using System;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Ball
{
    public class ColliderHelper : MonoBehaviour
    {
        private void OnEnable()
        {
            // Just to be able to disable us to disable debug logging.
        }

        #region Collisions

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            Debug.Log($"UNHANDLED trigger_enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            Debug.Log($"UNHANDLED trigger_exit {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = collision.gameObject;
            var layer = otherGameObject.layer;
            var material = collision.collider.sharedMaterial != null ? collision.collider.sharedMaterial.name : "NULL";
            Debug.Log($"UNHANDLED collision_enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)} mat {material}");
        }

        #endregion
    }
}