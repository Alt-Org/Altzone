using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerCollider2 : MonoBehaviour
    {
        public Action<Collision2D> Callback;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (collision.contactCount == 0)
            {
                return;
            }
            var otherGameObject = collision.gameObject;
            var layer = otherGameObject.layer;
            Debug.Log($"collision_enter {otherGameObject.name} {otherGameObject.tag} layer {layer} {LayerMask.LayerToName(layer)}");
            Callback?.Invoke(collision);
        }
    }
}