using System.Collections;

using UnityEngine;
using Photon.Pun;

namespace Battle.Scripts.Battle.Game
{
    internal class Diamond : MonoBehaviour
    {
        // Public methods
        public void InitInstance(Rigidbody2D rb, float bottomBoundary, float topBoundary, bool isTopSide, int diamondDisappearUpdateNumber)
        {
            _rb = rb;
            _bottomBoundary = bottomBoundary;
            _topBoundary = topBoundary;
            _isTopSide = isTopSide;
            _diamondDisappearUpdateNumber = diamondDisappearUpdateNumber;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            _syncedFixedUpdateClock.ExecuteOnUpdate(_diamondDisappearUpdateNumber, 0, () =>
            {
                Destroy(gameObject);
            });
        }

        // Private methods
        private void Update()
        {
            if (_rb != null)
            {
                if (_isTopSide)
                {
                    if (transform.position.y <= _bottomBoundary)
                    {
                        _rb.velocity = Vector2.zero;
                    }
                }
                else
                {
                    if (transform.position.y >= _topBoundary)
                    {
                        _rb.velocity = Vector2.zero;
                    }
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //Debug.Log("Collision detected with: " + collision.gameObject.name);

            if (_rb != null)
            {
                Vector2 reflectVelocity = Vector2.Reflect(_rb.velocity, collision.contacts[0].normal);
                _rb.velocity = reflectVelocity;
            }
        }

        // Private fields
        private Rigidbody2D _rb;
        private float _bottomBoundary;
        private float _topBoundary;
        private bool _isTopSide;
        private int _diamondDisappearUpdateNumber;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock;
    }
}