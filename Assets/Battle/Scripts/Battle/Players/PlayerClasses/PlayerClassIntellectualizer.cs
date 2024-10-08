using System;
using System.Collections.Generic;
using UnityEngine;
using Battle.Scripts.Battle.Game;
using Prg.Scripts.Common.PubSub;
using System.Linq;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassIntellectualizer : MonoBehaviour, IPlayerClass
    {
        [SerializeField] private GameObject _positionSprite;
        [SerializeField] private LayerMask _collisionLayer;
        [SerializeField] private float _maxDistance;
        [SerializeField] private int _maxReflections;
        [SerializeField] private int _time;
        [SerializeField] private List<Sprite> _spriteList;
        [SerializeField] int _pointStep;

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        public bool BounceOnBallShieldCollision => true;

        #region Public Methods

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
        }

        public void OnBallShieldCollision() { }

        public void OnBallShieldBounce()
        {
            if (_isOnLocalTeam)
            {
                _timer = _time;
            }
        }

        #endregion Public Methods

        private IReadOnlyBattlePlayer _battlePlayer;

        // Important Objects
        private GridManager _gridManager;

        // Components
        private Rigidbody2D _rb;

        // Game state variables
        private int _timer;

        // Visual representation lists
        private List<GameObject> _positionSprites;
        private List<TrailSprite> _trailSprites;

        // Gameplay related flags
        private bool _isOnLocalTeam = false;

        // Projectile simulation variables
        private Vector2 _currentPosition;
        private Vector2 _currentVelocity;
        private GridPos _gridPosition;
        private Vector3 _worldPosition;

        private Quaternion _spriteRotation;

        private class TrailSprite
        {
            public readonly GameObject GameObject;
            public int Timer;

            public TrailSprite(GameObject gameObject, Vector3 position, Sprite sprite, int timer, Quaternion rotation)
            {
                GameObject = Instantiate(gameObject, position, rotation);
                GameObject.SetActive(true);
                GameObject.GetComponent<SpriteRenderer>().sprite = sprite;
                Timer = timer;
            }
        }

        private BallHandler _ballHandler;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS INTELLECTUALIZER] ";
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        private void Start()
        {
            _ballHandler = Context.GetBallHandler;
            _rb = _ballHandler.GetComponent<Rigidbody2D>();
            _gridManager = Context.GetGridManager;
            _positionSprites = new List<GameObject>();
            _trailSprites = new List<TrailSprite>();

            // Subscribe to messages
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);

            // Debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            _isOnLocalTeam = BattlePlayer.BattleTeam.TeamNumber == data.LocalPlayer.BattleTeam.TeamNumber;
        }

        private void ProjectilePredictionUpdate()
        {
            if (_gridManager == null)
            {
                Debug.LogError("GridManager is not initialized.");
                return;
            }

            _currentVelocity = GetCurrentVelocity();
            _currentPosition = GetCurrentPosition();
            _gridPosition = _gridManager.WorldPointToGridPosition(_rb.position);
            _worldPosition = _gridManager.GridPositionToWorldPoint(_gridPosition);

            float distance = _maxDistance;
            int reflections = 0;
            List<Vector3> positions = new();

            while (distance > 0 && reflections < _maxReflections)
            {
                RaycastHit2D hit = Physics2D.Raycast(_currentPosition, _currentVelocity.normalized, distance, _collisionLayer);
                if (hit.collider != null)
                {
                    UpdatePositionAndVelocity(hit);
                    distance -= hit.distance;
                    reflections++;
                }
                else
                {
                    _worldPosition = _currentPosition + _currentVelocity.normalized * distance;
                    distance = 0;
                }

                positions.Add(_worldPosition);
                CreateInterpolatedPositions(_currentPosition, _worldPosition, _currentVelocity, positions);
            }

            UpdatePredictionSprites(positions);
        }

        private void CreateInterpolatedPositions(Vector2 startPosition, Vector3 endPosition, Vector2 velocity, List<Vector3> positions)
        {
            float pointDistance = (endPosition - (Vector3)startPosition).magnitude;
            int positionCount = Mathf.FloorToInt(pointDistance / (velocity.magnitude / SyncedFixedUpdateClock.UpdatesPerSecond / _pointStep));

            Vector3 stepPosition = startPosition;
            Vector3 stepVelocity = velocity / SyncedFixedUpdateClock.UpdatesPerSecond;
            for (int i = 0; i < positionCount; i++)
            {
                stepPosition += stepVelocity * _pointStep;
                positions.Add(stepPosition);
            }
        }

        private void UpdatePositionAndVelocity(RaycastHit2D hit)
        {
            Vector2 newDirection = Vector2.Reflect(_currentVelocity.normalized, hit.normal);
            _currentPosition = hit.point + newDirection * 0.1f;
            _currentVelocity = newDirection * _currentVelocity.magnitude;
        }

        private void UpdatePredictionSprites(List<Vector3> positions)
        {
            int requiredCount = positions.Count;
            int currentCount = _positionSprites.Count;

            // Adjust sprite count to match required positions
            for (int i = 0; i < requiredCount; i++)
            {
                GameObject sprite;
                if (i < currentCount)
                {
                    sprite = _positionSprites[i];
                }
                else
                {
                    sprite = Instantiate(_positionSprite, positions[i], _spriteRotation);
                    _positionSprites.Add(sprite);
                }
                sprite.transform.position = positions[i];
                sprite.SetActive(true);

                SpriteRenderer spriteRenderer = sprite.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = _spriteList[UnityEngine.Random.Range(0, _spriteList.Count)];
            }

            // Deactivate any extra sprites
            for (int i = requiredCount; i < currentCount; i++)
            {
                _positionSprites[i].SetActive(false);
            }
        }
        private int MAX_TRAIL_SPRITES = 40;
        private void UpdateTrailSprites()
        {
            if (_timer % _pointStep == 0)
            {
                Sprite sprite = GetRandomOrLastSprite();
                TrailSprite newTrailSprite = new TrailSprite(_positionSprite, GetCurrentPosition(), sprite, 50, _spriteRotation);

                if (_trailSprites.Count < MAX_TRAIL_SPRITES)
                {
                    _trailSprites.Add(newTrailSprite);
                }
                else
                {
                    GameObject.Destroy(_trailSprites[0].GameObject);
                    _trailSprites.RemoveAt(0);
                    _trailSprites.Add(newTrailSprite);
                }
            }
        }

        private Sprite GetRandomOrLastSprite()
        {
            if (_trailSprites.Count > 0 && UnityEngine.Random.Range(0, 5) == 0)
            {
                Sprite lastSprite = _trailSprites.Last().GameObject.GetComponent<SpriteRenderer>().sprite;
                if (_trailSprites.Count > 1 && _trailSprites[_trailSprites.Count - 2].GameObject.GetComponent<SpriteRenderer>().sprite == lastSprite)
                {
                    return _spriteList[UnityEngine.Random.Range(0, _spriteList.Count)];
                }
                return lastSprite;
            }
            return _spriteList[UnityEngine.Random.Range(0, _spriteList.Count)];
        }

        private void UpdateTrailLifespan()
        {
            for (int i = _trailSprites.Count - 1; i >= 0; i--)
            {
                _trailSprites[i].Timer--;
                if (_trailSprites[i].Timer <= 0)
                {
                    GameObject.Destroy(_trailSprites[i].GameObject);
                    _trailSprites.RemoveAt(i);
                }
            }
        }

        private void ClearPrediction()
        {
            foreach (var sprite in _positionSprites)
            {
                sprite.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            if (_timer > 0)
            {
                if (GetCurrentVelocity() != Vector2.zero)
                {
                    _timer--;
                    ProjectilePredictionUpdate();
                }
                else
                {
                    _timer = 0;
                    ClearPrediction();
                }
            }
            else
            {
                ClearPrediction();
            }

            UpdateTrailLifespan();
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
