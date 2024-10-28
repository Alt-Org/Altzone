using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Battle1.Scripts.Battle.Game
{
    internal class Diamond : MonoBehaviour
    {
        // Public methods
        public void InitInstance(Rigidbody2D rb, float bottomBoundary, float topBoundary, bool isTopSide, int diamondDisappearUpdateNumber, DiamondType diamondType)
        {
            _rb = rb;
            _bottomBoundary = bottomBoundary;
            _topBoundary = topBoundary;
            _isTopSide = isTopSide;
            _diamondDisappearUpdateNumber = diamondDisappearUpdateNumber;
            _diamondType = diamondType;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            _syncedFixedUpdateClock.ExecuteOnUpdate(_diamondDisappearUpdateNumber, 0, () =>
            {
                Destroy(gameObject);
            });
        }

        public DiamondType GetDiamondType()
        {
            return _diamondType;
        }

        // Private fields
        private Rigidbody2D _rb;
        private float _bottomBoundary;
        private float _topBoundary;
        private bool _isTopSide;
        private int _diamondDisappearUpdateNumber;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock;
        private DiamondType _diamondType;

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
            if (_rb != null)
            {
                Vector2 reflectVelocity = Vector2.Reflect(_rb.velocity, collision.contacts[0].normal);
                float reflectionAngle = Vector2.Angle(_rb.velocity.normalized, collision.contacts[0].normal.normalized);
                Debug.Log("[Diamond] Reflection angle: " + reflectionAngle);

                _rb.velocity = reflectVelocity;
                if (reflectionAngle >= 165f)
                {
                    _rb.velocity = transform.position.y > 0 ? new Vector2(reflectVelocity.x, reflectVelocity.y - 1) : new Vector2(reflectVelocity.x, reflectVelocity.y + 1);
                    Debug.Log("[Diamond] Added more vertical velocity due to reflection angle");
                }
            }
        }
    }
}