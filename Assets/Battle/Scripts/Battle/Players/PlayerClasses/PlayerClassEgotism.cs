using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassEgotism : MonoBehaviour, IPlayerClass
    {
        [SerializeField] private GameObject _positionSprite;
        [SerializeField] private LayerMask _collisionLayer;
        [SerializeField] private float _maxDistance = 10f;
        [SerializeField] private int _maxReflections = 5;
        [SerializeField] private int _time = 5;

        [Obsolete("SpecialAbilityOverridesBallBounce is deprecated, please use return value of OnBallShieldCollision instead.")]
        public bool SpecialAbilityOverridesBallBounce => false;

        public bool OnBallShieldCollision()
        { return true; }

        public void OnBallShieldBounce()
        {
            _timer = _time;
        }

        [Obsolete("ActivateSpecialAbility is deprecated, please use OnBallShieldCollision and/or OnBallShieldBounce instead.")]
        public void ActivateSpecialAbility()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Special ability activated", _syncedFixedUpdateClock.UpdateCount));
        }

        private GridManager _gridManager;
        private Rigidbody2D _rb;
        private int _timer;
        private LineRenderer _lineRenderer;
        private List<GameObject> _positionSprites;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS EGOTISM] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time
 
        private void Start()
        {
            _rb = Context.GetBallHandler.GetComponent<Rigidbody2D>();
            Debug.Log(_rb);
            _lineRenderer = GetComponent<LineRenderer>();

            _gridManager = Context.GetGridManager;

            _positionSprites = new();

            // debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private void FixedUpdate()
        {
            if (_timer <= 0)
            {
                for (int i = 0; i < _positionSprites.Count; i++)
                {
                    _positionSprites[i].SetActive(false);
                }
                return;
            }

            _timer--;

            GridPos gridPosition = null;
            Vector2 currentVelocity = GetCurrentVelocity();
            Vector2 currentPosition = GetCurrentPosition();
            Vector2 direction = currentVelocity.normalized;
            gridPosition = _gridManager.WorldPointToGridPosition(_rb.position);
            Vector3 worldPosition = _gridManager.GridPositionToWorldPoint(gridPosition);
            float distance = _maxDistance;
            int reflections = 0;
            List<Vector3> positions = new();
            Vector3 pointPosition;
            Vector3 pointVelocity;

            while (distance > 0 && reflections < _maxReflections)
            {
                RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, distance, _collisionLayer);

                Debug.Log(DEBUG_LOG_NAME + hit);

                if (hit.collider != null)
                {
                    // Calculate the reflection
                    Vector2 hitPosition = hit.point;
                    Vector2 reflectionDirection = Vector2.Reflect(currentVelocity.normalized, hit.normal);
                    gridPosition = _gridManager.WorldPointToGridPosition(hitPosition);
                    worldPosition = _gridManager.GridPositionToWorldPoint(gridPosition);

                    pointPosition = currentPosition;
                    pointVelocity = currentVelocity;

                    // Update currentPosition for next raycast
                    currentPosition = hitPosition + reflectionDirection.normalized * 0.01f;
                    currentVelocity = reflectionDirection * currentVelocity.magnitude;

                    // Reduce distance by the distance traveled
                    distance -= hit.distance;
                    reflections++;
                }
                else
                {
                    worldPosition = currentPosition + currentVelocity.normalized * distance;
       
                    break;
                }

                int pointStep = 10;

                float pointDistance = (worldPosition - pointPosition).magnitude;

                int positionCount = (int)Mathf.Floor(pointDistance / pointVelocity.magnitude / pointStep);

                for (int i = 0; i < positionCount; i++)
                {
                    // Calculate the next point position based on velocity and step and add to the list of positions
                    pointPosition = pointVelocity * pointStep;
                    positions.Add(pointPosition);
                }
                positions.Add(worldPosition);
            }

            Debug.Log(DEBUG_LOG_NAME + positions.Count);

            // Check if there are fewer sprite objects than position points
            if (_positionSprites.Count < positions.Count)
            {
                int difference = positions.Count - _positionSprites.Count;

                // Create new sprite objects to match the number of position
                for (int i = 0; i < difference; i++)
                {
                    _positionSprites.Add(Instantiate(_positionSprite, Vector3.zero, Quaternion.identity));
                }
                Debug.Log(DEBUG_LOG_NAME + _positionSprites.Count);
                Debug.Log(DEBUG_LOG_NAME +  positions.Count);
            }

            // Update the position and activation state of each sprite
            for (int i = 0;i < positions.Count; i++)
            {
                _positionSprites[i].transform.position = positions[i];
                _positionSprites[i].SetActive(true);
            }

            if (_positionSprites.Count > positions.Count)
            {
                int difference = positions.Count - _positionSprites.Count;

                // Deactivate excess sprite objects
                for (int i = 0; i < difference; i++)
                {
                    _positionSprites[positions.Count + i].SetActive(false);
                }
            }
        }

        private void DrawLine(Vector3 startPoint, Vector3 endPoint)
        {
            startPoint.z -= 0.1f;
            endPoint.z -= 0.1f;

            // Ensure the LineRenderer has 2 points
            _lineRenderer.positionCount = 2;

            _lineRenderer.SetPosition(0, startPoint);
            _lineRenderer.SetPosition(1, endPoint);
        }

        private Vector2 GetCurrentVelocity()
        {
            return _rb.velocity;
        }

        private Vector2 GetCurrentPosition()
        {
            return _rb.position;
        }
    }
}
