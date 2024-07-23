using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassRetroflection : MonoBehaviour, IPlayerClass
    {
        // Serialized fields
        [SerializeField] private int _hitsBeforeShapeChange;
        [SerializeField] private GameObject _shield;
        [SerializeField] private Mesh[] _shieldShapes; // Array of different shield shapes

        public bool BounceOnBallShieldCollision => true;

        public void OnBallShieldCollision()
        {}

        public void OnBallShieldBounce()
        {
            TrackShieldCollisions();
        }

        private int collisionCount;
        private int currentShapeIndex;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS RETROFLECTION] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock;

        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            currentShapeIndex = 0;
            UpdateShieldShape();
        }

        private void TrackShieldCollisions()
        {
            collisionCount++;

            if (collisionCount >= _hitsBeforeShapeChange)
            {
                collisionCount = 0;
                ChangeShieldShape();
            }
        }

        private void ChangeShieldShape()
        {
            currentShapeIndex = (currentShapeIndex + 1) % _shieldShapes.Length;
            UpdateShieldShape();
        }

        private void UpdateShieldShape()
        {
            MeshFilter meshFilter = _shield.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.mesh = _shieldShapes[currentShapeIndex];
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME, _syncedFixedUpdateClock.UpdateCount) + $" Shield shape changed to: {currentShapeIndex}");
            }
        }
    }
}

