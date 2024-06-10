using Altzone.Scripts.Config;
using Altzone.Scripts.Config.ScriptableObjects;
using Battle.Scripts.Battle;
using Battle.Scripts.Test;
using Battle.Scripts.Battle.Game;
using UnityConstants;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System;
using Prg.Scripts.Common.Unity.Input;

public class BallHandler : MonoBehaviour
{
    // Serialized Fields
    [SerializeField] private int _damage;
    [SerializeField] private GameObject _explotion;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _hotFixMax;
    [SerializeField] private float _sparkleUpdateInterval;
    [SerializeField] private float _timeSinceLastUpdate;
    [SerializeField] private GameObject _sparkleSprite;


    #region Public Methods

    public int SpriteIndex()
    {
        return _spriteIndex;
    }
    public void Launch(Vector3 position, Vector3 direction, float speed)
    {
        _rb.position = position;
        SetVelocity(NewRotation(direction) * Vector2.up * speed);
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

    #endregion Public Methods

    private float _arenaScaleFactor;

    // Important Objects
    private PlayerPlayArea _battlePlayArea;
    private GridManager _gridManager;

    // Game Config Variables
    private float _angleLimit;

    private int _spriteIndex;

    // Components
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;

    // Debug
    private const string DEBUG_LOG_NAME = "[BATTLE] [BALL HANDLER] ";
    private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
    private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

    private void Start()
    {
        // Get components
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();

        // Get important objects
        _battlePlayArea = Context.GetBattlePlayArea;
        _gridManager = Context.GetGridManager;

        // Get game config variables
        GameVariables variables = GameConfig.Get().Variables;
        _angleLimit = variables._angleLimit;

        _sprite.enabled = false;
        _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
        transform.localScale = Vector3.one * _arenaScaleFactor;

        // Debug
        _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GridPos gridPos = null;
        GameObject otherGameObject = collision.gameObject;

        if (!otherGameObject.CompareTag("ShieldBoxCollider"))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 currentVelocity = _rb.velocity;
            Vector2 direction = Vector2.Reflect(currentVelocity, normal);
            gridPos = _gridManager.WorldPointToGridPosition(_rb.position);
            _rb.position = _gridManager.GridPositionToWorldPoint(gridPos);
            SetVelocity(NewRotation(direction) * Vector2.up * currentVelocity.magnitude);
        }
        else
        {
            ShieldBoxCollider shield = otherGameObject.GetComponent<ShieldBoxCollider>(); 
            if (shield != null)
            {
                bool doBoucne = shield.OnBallShieldCollision();

                if (doBoucne)
                {
                    Transform shieldTransform = shield.ShieldTransform;
                    float bounceAngle = shield.BounceAngle;
                    float attackMultiplier = shield.AttackMultiplier;
                    Vector2 incomingDirection = (_rb.position - (Vector2)shieldTransform.position).normalized;
                    Vector3 shieldDirection = shieldTransform.up;
                    bool isCorrectDirection = Vector2.Dot(incomingDirection, shieldDirection) < 0;

                    if (isCorrectDirection)
                    {
                        gridPos = _gridManager.WorldPointToGridPosition(_rb.position);
                        _rb.position = _gridManager.GridPositionToWorldPoint(gridPos);
                        float angle = shieldTransform.rotation.eulerAngles.z + bounceAngle; // Käytä kilven kulmaa
                        Quaternion rotation = Quaternion.Euler(0, 0, angle);
                        Vector2 newVelocity = rotation * Vector2.up * (_rb.velocity.magnitude + attackMultiplier);

                        SetVelocity(newVelocity);

                        Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Collision (type: player (bounce), position: {1}, grid position: ({2}), velocity: {3})", _syncedFixedUpdateClock.UpdateCount, _rb.position, gridPos, _rb.velocity));
                    }

                    shield.OnBallShieldBounce();
                }
                else
                {
                    gridPos = _gridManager.WorldPointToGridPosition(_rb.position);
                    Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Collision (type: player (no bounce), position: {1}, grid position: ({2}))", _syncedFixedUpdateClock.UpdateCount, _rb.position, gridPos));
                }
            }
        }

        if (otherGameObject.CompareTag("Wall"))
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Collision (type: soul wall, position: {1}, grid position: ({2}), velocity: {3})", _syncedFixedUpdateClock.UpdateCount, _rb.position, gridPos, _rb.velocity));
            int BrickHealth = otherGameObject.GetComponent<BrickRemove>().Health;
            otherGameObject.GetComponent<BrickRemove>().BrickHitInit(_damage);

            if (BrickHealth - _damage <= 0)
            {
                Stop();
                Instantiate(_explotion, transform.position, transform.rotation * Quaternion.Euler(0f, 0f, transform.position.y > 0 ? 0f : 180f));
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
           (int)Mathf.Floor(
               _rb.velocity.magnitude / _maxSpeed * (_sprites.Length - 1)
               ),
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

    private Quaternion NewRotation(Vector2 direction)
    {
        var angle = Vector2.SignedAngle(direction, Vector2.up);
        var multiplier = Mathf.Round(angle / _angleLimit);
        var newAngle = -multiplier * _angleLimit;
        return Quaternion.Euler(0, 0, newAngle);
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
}



