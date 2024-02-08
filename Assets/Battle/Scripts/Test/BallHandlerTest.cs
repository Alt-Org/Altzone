using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Test;
using Battle.Scripts.Battle.Game;
using UnityConstants;
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BallHandlerTest : MonoBehaviour
{
    // Serialized Fields
    [SerializeField] private int _damage;
    [SerializeField] private GameObject _explotion;
    [SerializeField] private float _bounceAngle;
    [SerializeField] private float _attackMultiplier;
    [SerializeField] private Color _hitShieldColor; 
    [SerializeField] private Color _bounceColor; 
    [SerializeField] private Color _speedChangeColor;

    private float _arenaScaleFactor;

    // Important Objects
    private PlayerPlayArea _battlePlayArea;
    private GridManager _gridManager;

    // Game Config Variables
    private float _angleLimit;

    // Components
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;

    // Debug
    private const string DEBUG_LOG_NAME = "[BATTLE] [BALL HANDLER] ";
    private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
    private SyncedFixedUpdateClockTest _syncedFixedUpdateClock; // only needed for logging time

    private void Start()
    {
        // Get components
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();

        // Get important objects
        _battlePlayArea = Context.GetBattlePlayArea;
        _gridManager = Context.GetGridManager;

        // Get game config variables
        var variables = GameConfig.Get().Variables;
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
        var otherGameObject = collision.gameObject; 

        if (!otherGameObject.CompareTag(Tags.Player))
        {
            Bounce(); 
            Vector2 normal = collision.contacts[0].normal;
            Vector2 currentVelocity = _rb.velocity;
            Vector2 direction = Vector2.Reflect(currentVelocity, normal);
            gridPos = _gridManager.WorldPointToGridPosition(_rb.position);
            _rb.position = _gridManager.GridPositionToWorldPoint(gridPos);
            _rb.velocity = NewRotation(direction) * Vector2.up * currentVelocity.magnitude;
        }
        else
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Collision (type: player)", _syncedFixedUpdateClock.UpdateCount));
        }

        if (otherGameObject.CompareTag("Shield")) 
{
    _sprite.color = _hitShieldColor; 

    var shield = otherGameObject.GetComponent<ShieldBoxColliderTest>(); 
    if (shield != null)
    {
        var shieldTransform = shield.ShieldTransform; 
        var rb = otherGameObject.GetComponent<Rigidbody2D>(); 
        var incomingDirection = (rb.position - (Vector2)shieldTransform.position).normalized;
        var shieldDirection = shieldTransform.up; 
        var isCorrectDirection = Vector2.Dot(incomingDirection, shieldDirection) < 0; 

        if (isCorrectDirection) 
        {
            Bounce(); 
            gridPos = _gridManager.WorldPointToGridPosition(rb.position);
            rb.position = _gridManager.GridPositionToWorldPoint(gridPos);
            var angle = shieldTransform.rotation.eulerAngles.z + _bounceAngle; // Käytä kilven kulmaa
            var rotation = Quaternion.Euler(0, 0, angle);
            var newVelocity = rotation * Vector2.up * _attackMultiplier; 
            rb.velocity = newVelocity.magnitude > rb.velocity.magnitude ? newVelocity : rb.velocity; 
        }
    }
}

        else if (otherGameObject.CompareTag("Wall"))
        {
            int BrickHealth = otherGameObject.GetComponent<BrickRemove>().Health;
            otherGameObject.GetComponent<BrickRemove>().BrickHitInit(_damage);

            if (BrickHealth - _damage <= 0)
            {
                Stop();
                Instantiate(_explotion, transform.position, transform.rotation * Quaternion.Euler(0f, 0f, transform.position.y > 0 ? 0f : 180f));
            }
        }
    }

    public void Launch(Vector3 position, Vector3 direction, float speed)
    {
        _rb.position = position;
        _rb.velocity = NewRotation(direction) * Vector2.up * speed;
        _sprite.enabled = true;

        Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Ball launched (position: {1}, velocity: {2})", _syncedFixedUpdateClock.UpdateCount, _rb.position, _rb.velocity));
    }

    public void Stop()
    {
        _rb.position = Vector2.zero;
        _rb.velocity = Vector2.zero;
        _sprite.enabled = false;

        Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Ball stopped", _syncedFixedUpdateClock.UpdateCount));
    }

    private void Bounce()
    {
        _sprite.color = _bounceColor; 
        
    }

    private Quaternion NewRotation(Vector2 direction)
    {
        var angle = Vector2.SignedAngle(direction, Vector2.up);
        var multiplier = Mathf.Round(angle / _angleLimit);
        var newAngle = -multiplier * _angleLimit;
        return Quaternion.Euler(0, 0, newAngle);
    }
}



