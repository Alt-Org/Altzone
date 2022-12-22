using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Removes a brick from the wall when hit conditions are met.
    /// </summary>
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
