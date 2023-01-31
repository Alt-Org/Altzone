using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Removes a brick from the wall when hit conditions are met.
    /// </summary>
    internal class BrickRemove : MonoBehaviour
    {
        public PlayerPlayArea PlayerPlayArea;
        private int Health = 0;

        private void Start()
        {
            Health = PlayerPlayArea.BrickHealth;
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))
            {
                Health = Health - 1;
                if (Health <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
