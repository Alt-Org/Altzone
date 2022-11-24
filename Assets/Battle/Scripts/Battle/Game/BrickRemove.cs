using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal class BrickRemove : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))
            {
                Destroy(gameObject);
            }
        }
    }
}
