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
        // Serialized Fields
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

        public void OnBallShieldCollision()
        {}

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
        private int _trailSpritesAmount;
        private Vector2 _currentPosition;
        private Vector2 _currentVelocity;
        private GridPos _gridPosition;
        private Vector3 _worldPosition;
        private Vector3 _pointPosition;
        private Vector3 _pointVelocity;

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
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        private void Start()
        {
            // Get important objects
            _ballHandler = Context.GetBallHandler;
            _rb = _ballHandler.GetComponent<Rigidbody2D>();
            _gridManager = Context.GetGridManager;
            _positionSprites = new();
            _trailSprites = new();

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
            _gridPosition = null;
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
                Debug.Log(DEBUG_LOG_NAME + "reflections " + reflections);
                Debug.Log(DEBUG_LOG_NAME + "distance " + distance);

                if (hit.collider != null)
                {
                    Debug.Log("Collision Happened");
                    Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green);

                    UpdatePositionAndVelocity(hit);

                    distance -= hit.distance;
                    reflections++;
                }
                else
                {
                    Debug.Log("No Collision");
                    _worldPosition = _currentPosition + _currentVelocity.normalized * distance;
                    _pointPosition = _currentPosition;
                    _pointVelocity = _currentVelocity;
                    distance = 0;
                }

                Debug.DrawLine(_worldPosition, _pointPosition, Color.red);

                float pointDistance = (_worldPosition - _pointPosition).magnitude;

                _pointVelocity /= SyncedFixedUpdateClock.UpdatesPerSecond;

                int positionCount = (int)Mathf.Floor(pointDistance / _pointVelocity.magnitude / _pointStep);

                Debug.Log(DEBUG_LOG_NAME + "positionCount " + positionCount);

                for (int i = 0; i < positionCount; i++)
                {
                    // Calculate the next point position based on velocity and step and add to the list of positions
                    _pointPosition += _pointVelocity * _pointStep;
                    positions.Add(_pointPosition);
                }

                positions.Add(_worldPosition);
            }

            Debug.Log(DEBUG_LOG_NAME + positions.Count);

            UpdatePredictionSprites(positions);

            UpdateTrailSprites();
        }

        private void UpdatePositionAndVelocity(RaycastHit2D hit)
        {
            /*
            _pointPosition = _currentVelocity;
            _pointVelocity = _currentPosition;

            Vector2 newdirection;
            (_currentPosition, _, newdirection) = _ballHandler.CalculateCollision(_currentPosition, _currentVelocity, hit.normal);

            Debug.DrawLine(hit.point, hit.point + newdirection, Color.blue);


            // Update currentPosition for next raycast
            _currentPosition += newdirection * 0.1f;
            _currentVelocity = newdirection * _currentVelocity.magnitude;

            */


            // Calculate new direction based on collision normal
            Vector2 newDirection = Vector2.Reflect(_currentVelocity.normalized, hit.normal);

            // Update current position slightly past the collision point to avoid re-collision
            _currentPosition = hit.point + newDirection * 0.1f;

            // Update current velocity with the new direction while maintaining its magnitude
            _currentVelocity = newDirection * _currentVelocity.magnitude;


        }



        private void UpdatePredictionSprites(List<Vector3> positions)
        {
            // Check if there are fewer sprite objects than position points
            if (_positionSprites.Count < positions.Count)
            {
                int difference = positions.Count - _positionSprites.Count;

                // Create new sprite objects to match the number of position
                for (int i = 0; i < difference; i++)
                {
                    _positionSprites.Add(Instantiate(_positionSprite, Vector3.zero, _spriteRotation));
                }

                Debug.Log(DEBUG_LOG_NAME + "_positionSprite " + _positionSprites.Count);
                Debug.Log(DEBUG_LOG_NAME + "positions.Count " + positions.Count);
            }

            // Update the position and activation state of each sprite
            for (int i = 0; i < positions.Count; i++)
            {
                _positionSprites[i].transform.position = positions[i];
                _positionSprites[i].SetActive(true);
                SpriteRenderer spriteRenderer = _positionSprites[i].GetComponent<SpriteRenderer>();

                // Assign a random sprite from the spritelist
                spriteRenderer.sprite = _spriteList[UnityEngine.Random.Range(0, _spriteList.Count)];
            }

            // Check if there are more sprites than positions
            if (_positionSprites.Count > positions.Count)
            {
                int difference = _positionSprites.Count - positions.Count;

                // Deactivate excess sprite objects
                for (int i = 0; i < difference; i++)
                {
                    _positionSprites[positions.Count + i].SetActive(false);
                }
            }
        }

        private void UpdateTrailSprites()
        {
            // Check if the current timer value is a multiple of pointStep
            if (_timer % _pointStep == 0)
            {
                Sprite sprite;

                // Set the chance of allowing a duplicate
                bool allowDuplicate = UnityEngine.Random.Range(0, 5) == 0;

                if (_trailSpritesAmount > 0 && allowDuplicate)
                {
                    Sprite lastSprite = _trailSprites[_trailSpritesAmount - 1].GameObject.GetComponent<SpriteRenderer>().sprite;

                    if (_trailSpritesAmount > 1 && _trailSprites[_trailSpritesAmount - 2].GameObject.GetComponent<SpriteRenderer>().sprite == lastSprite)
                    {
                        // If the last two sprites are the same, use a new sprite
                        sprite = _spriteList[UnityEngine.Random.Range(0, _spriteList.Count)];
                    }
                    else
                    {
                        // Use the last sprite again
                        sprite = lastSprite;
                    }
                }
                else
                {
                    // Randomly choose a new sprite
                    sprite = _spriteList[UnityEngine.Random.Range(0, _spriteList.Count)];
                }

                TrailSprite newTrailSprite = new TrailSprite(_positionSprite, GetCurrentPosition(), sprite, 50, _spriteRotation);

                // Check if the trail sprite list is already full
                if (_trailSprites.Count <= _trailSpritesAmount)
                {
                    _trailSprites.Add(newTrailSprite);
                }
                else
                {
                    // If the list is full, replace the oldest sprite
                    _trailSprites[_trailSpritesAmount] = newTrailSprite;
                }

                _trailSpritesAmount++;

                foreach (var trailSprite in _trailSprites)
                {
                        trailSprite.Timer--;
                }
            }
        }

        // Update the lifespan of the trail sprites
        private void UpdateTrailLifespan()
        {
            Debug.Log("Updating Trail Lifespan ");
            int i2 = 0;
            bool delete;

            for (int i = 0; i < _trailSpritesAmount; i++)
            {
                // Check if the sprite's timer has expired
                delete = _trailSprites[i].Timer <= 0;

                if (delete)
                {
                    // If expired, destroy the sprite's game object
                    Destroy(_trailSprites[i].GameObject);
                    _trailSprites[i] = null;
                }
                else
                {
                    // If not expired, decrement the timer and shift the sprites position in the list if some were deleted earlier
                    _trailSprites[i].Timer--;
                    _trailSprites[i2] = _trailSprites[i];
                }

                // Increment offset if a sprite was deleted
                if (!delete)
                {
                    i2++;
                }
            }

            // Adjust the count of trail sprites by the number of deleted sprites
            _trailSpritesAmount = i2;

#if true
            string str = "TRAIL SPRITES LIST\n";

            for (int debugI = 0; debugI < _trailSprites.Count; debugI++)
            {
                str += string.Format("{0:0000} ", debugI);
                if (_trailSprites[debugI] != null)
                {
                    str += string.Format("sprite {0:00} {1:00000000}", _trailSprites[debugI].Timer, _trailSprites[debugI].GetHashCode());
                }
                else
                {
                    str += "null  00 ";
                }

                if (debugI == _trailSpritesAmount)
                {
                    str += " <- _trailSpritesAmount";
                }
                str += "\n";
            }

            Debug.Log(str);
#endif
        }

        private void ClearPrediction()
        {
            for (int i = 0; i < _positionSprites.Count; i++)
            {
                _positionSprites[i].SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            if(_timer > 0)
            {
                if (GetCurrentVelocity() != Vector2.zero)
                {
                    _timer--;
                    ProjectilePredictionUpdate();
                    return;
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
