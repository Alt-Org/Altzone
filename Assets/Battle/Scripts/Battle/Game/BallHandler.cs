using UnityEngine;

using Photon.Pun;

using Altzone.Scripts.Config;
using Altzone.Scripts.Config.ScriptableObjects;

using Battle.Scripts.Battle.Players;

namespace Battle.Scripts.Battle.Game
{
    [RequireComponent(requiredComponent: typeof(Rigidbody2D))]
    internal class BallHandler : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private int _damage;
        [SerializeField] private GameObject _explosion;
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private float _maxSpeed;
        [SerializeField] private float _hotFixMax;
        [SerializeField] private float _sparkleUpdateInterval;
        [SerializeField] private float _timeSinceLastUpdate;
        [SerializeField] private GameObject _sparkleSprite;
        [SerializeField] private SpawnDiamonds _diamondSpawner;
        #endregion Serialized Fields

        #region Public

        #region Public - Methods

        public int SpriteIndex()
        {
            return _spriteIndex;
        }

        public void Launch(Vector3 position, Vector3 direction, float speed)
        {
            _rb.position = position;
            SetVelocity(LimitDirectionAngle(direction, _angleLimit) * speed);
            _sprite.enabled = true;
            _sparkleSprite.SetActive(true);

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Ball launched (position: {1}, velocity: {2})", _syncedFixedUpdateClock.UpdateCount, _rb.position, _rb.velocity));
        }

        public void Stop()
        {
            _rb.position = Vector2.zero;
            _rb.velocity = Vector2.zero;
            _sprite.enabled = false;
            _sparkleSprite.SetActive(false);

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Ball stopped", _syncedFixedUpdateClock.UpdateCount));
        }

        public (Vector2 position, GridPos gridPos, Vector2 direction) CalculateCollision(Vector2 position, Vector2 direction, Vector2 normal)
        {
            GridPos gridPos = _gridManager.WorldPointToGridPosition(position);
            position = _gridManager.GridPositionToWorldPoint(gridPos);

            direction = LimitDirectionAngle(Vector2.Reflect(direction, normal), _angleLimit);

            return (position, gridPos, direction);
        }

        public (bool isCorrectDirection, bool bounce, Vector2 position, GridPos gridPos, Vector2 direction, float speed) CalculateCollision(Vector2 position, float speed, ShieldBoxCollider shieldBoxCollider)
        {
            Transform shieldTransform = shieldBoxCollider.ShieldTransform;
            Vector2 incomingDirection = (position - (Vector2)shieldTransform.position).normalized;
            Vector3 shieldDirection = shieldTransform.up;
            bool isCorrectDirection = Vector2.Dot(incomingDirection, shieldDirection) < 0;

            if (!isCorrectDirection) return (false, false, Vector2.zero, new GridPos(0, 0), Vector2.zero, 0);
            if (!shieldBoxCollider.BounceOnBallShieldCollision) return (true, false, Vector2.zero, new GridPos(0, 0), Vector2.zero, 0);

            float bounceAngle = shieldBoxCollider.BounceAngle;
            float impactForce = shieldBoxCollider.ImpactForce;

            GridPos gridPos = _gridManager.WorldPointToGridPosition(position);
            position = _gridManager.GridPositionToWorldPoint(gridPos);

            float angle = shieldTransform.rotation.eulerAngles.z + bounceAngle;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;

            speed += impactForce * _attackMultiplier;

            return (true, true, position, gridPos, direction, speed);
        }

        #endregion Public - Methods

        #endregion Public

        #region Private

        #region Private - Fields

        private float _arenaScaleFactor;

        // Important Objects
        private PlayerPlayArea _battlePlayArea;
        private GridManager _gridManager;

        // Game Config Variables
        private float _attackMultiplier;
        private float _angleLimit;

        private int _spriteIndex;

        // Components
        private Rigidbody2D _rb;
        private SpriteRenderer _sprite;

        #endregion Private - Fields

        #region DEBUG
        private const string DEBUG_LOG_NAME = "[BATTLE] [BALL HANDLER] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time
        #endregion DEBUG


        #region Private - Methods

        private void Start()
        {
            // Get components
            _rb = GetComponent<Rigidbody2D>();
            _sprite = GetComponentInChildren<SpriteRenderer>();

            // Get important objects
            _battlePlayArea = Context.GetBattlePlayArea;
            _gridManager = Context.GetGridManager;
            _diamondSpawner = FindObjectOfType<SpawnDiamonds>();

            // Get game config variables
            GameVariables variables = GameConfig.Get().Variables;
            _attackMultiplier = variables._playerAttackMultiplier;
            _angleLimit = variables._angleLimit;

            if (PhotonBattle.GetTeamNumber(PhotonNetwork.LocalPlayer) == PhotonBattle.TeamBetaValue) transform.eulerAngles = new Vector3(0, 0, 180f);

            _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
            transform.localScale = Vector3.one * _arenaScaleFactor;
            _sprite.enabled = false;

            // Debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GridPos gridPos = null;
            GameObject otherGameObject = collision.gameObject;

            if (!otherGameObject.CompareTag("ShieldBoxCollider"))
            {
                Vector2 position, direction;
                (position, gridPos, direction) = CalculateCollision(_rb.position, _rb.velocity, collision.contacts[0].normal);
                _rb.position = position;
                SetVelocity(direction * _rb.velocity.magnitude);
            }
            else
            {
                if (otherGameObject.TryGetComponent(out ShieldBoxCollider shield))
                {
                    bool isCorrectDirection, bounce;
                    Vector2 position, direction;
                    float speed;
                    (isCorrectDirection, bounce, position, gridPos, direction, speed) = CalculateCollision(_rb.position, _rb.velocity.magnitude, shield);

                    if (isCorrectDirection)
                    {
                        shield.OnBallShieldCollision();

                        if (bounce)
                        {
                            _rb.position = position;
                            SetVelocity(direction * speed);

                            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Collision (type: player (bounce), position: {1}, grid position: ({2}), velocity: {3})", _syncedFixedUpdateClock.UpdateCount, _rb.position, gridPos, _rb.velocity));

                            shield.OnBallShieldBounce();
                        }
                        else
                        {
                            gridPos = _gridManager.WorldPointToGridPosition(_rb.position);
                            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Collision (type: player (no bounce), position: {1}, grid position: ({2}))", _syncedFixedUpdateClock.UpdateCount, _rb.position, gridPos));
                        }
                    }
                }
            }

            if (otherGameObject.CompareTag("Wall"))
            {
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Collision (type: soul wall, position: {1}, grid position: ({2}), velocity: {3})", _syncedFixedUpdateClock.UpdateCount, _rb.position, gridPos, _rb.velocity));

                int soulWallSegmentHealth = otherGameObject.GetComponent<SoulWallSegmentRemove>().Health;
                otherGameObject.GetComponent<SoulWallSegmentRemove>().BrickHitInit(_damage);

                if (soulWallSegmentHealth - _damage <= 0)
                {
                    Vector3 spawnPoint = transform.position;
                    Stop();
                    Instantiate(_explosion, transform.position, transform.rotation * Quaternion.Euler(0f, 0f, transform.position.y > 0 ? 0f : 180f));
                    _diamondSpawner.DiamondSpawner(spawnPoint);
                }
            }
            else
            {
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Collision (type: arena border, position: {1}, grid position: ({2}), velocity: {3})", _syncedFixedUpdateClock.UpdateCount, _rb.position, gridPos, _rb.velocity));
            }
        }

        private void SetVelocity(Vector3 velocity)
        {
            if (velocity.magnitude > _hotFixMax)
            {
                velocity = Vector3.ClampMagnitude(velocity, _hotFixMax);
            }

            _rb.velocity = velocity;

            // Calculate the index of the sprite to use based on the magnitude of the rigidbodys velocity
            _spriteIndex = Mathf.Clamp(
                (int)Mathf.Floor(_rb.velocity.magnitude / _maxSpeed * (_sprites.Length - 1)),
                0,
                _sprites.Length - 1
            );

            _sprite.sprite = _sprites[_spriteIndex];
        }

        private void ChangeSparkleScale()
        {
            if (_sparkleSprite != null)
            {
                SpriteRenderer spriteRenderer = _sparkleSprite.GetComponent<SpriteRenderer>();
                spriteRenderer.transform.position = _sprite.transform.position;
                float randomScale = UnityEngine.Random.Range(8, 12);

                // Set the scale of the sprite renderer with the random scale value
                spriteRenderer.transform.localScale = new Vector3(randomScale, randomScale, 1);
            }
        }

        private Vector2 LimitDirectionAngle(Vector2 direction, float angleLimit)
        {
            float angle = Vector2.SignedAngle(direction, Vector2.up);
            float multiplier = Mathf.Round(angle / angleLimit);
            float newAngle = -multiplier * _angleLimit;
            return Quaternion.Euler(0, 0, newAngle) * Vector2.up;
        }

        private void FixedUpdate()
        {
            if (_rb.velocity != Vector2.zero)
            {
                _timeSinceLastUpdate += Time.fixedDeltaTime;

                // Check if enough time has passed since the last sparkle update
                if (_timeSinceLastUpdate >= _sparkleUpdateInterval)
                {
                    ChangeSparkleScale();
                    _timeSinceLastUpdate = 0f;
                }
            }
        }

        #endregion Private - Methods

        #endregion Private
    }
}
