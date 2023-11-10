using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
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

    #region Public Methods

    public void Launch(Vector3 position, Vector3 direction, float speed)
    {
        _rb.position = position;
        _rb.velocity = NewRotation(direction) * Vector2.up * speed;
        _sprite.enabled = true;
    }

    public void Stop()
    {
        _rb.position = Vector2.zero;
        _rb.velocity = Vector2.zero;
        _sprite.enabled = false;
    }

    #endregion

    private float _arenaScaleFactor;

    // Important Objects
    private PlayerPlayArea _battlePlayArea;
    private GridManager _gridManager;

    // Game Config Variables
    private float _angleLimit;

    // Components
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;

    private void Start()
    {
        // get components
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();

        // get important objects
        _battlePlayArea = Context.GetBattlePlayArea;
        _gridManager = Context.GetGridManager;

        // get game config variables
        var variables = GameConfig.Get().Variables;
        _angleLimit = variables._angleLimit;

        _sprite.enabled = false;
        _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
        transform.localScale = Vector3.one * _arenaScaleFactor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(Tags.Player))
        {
            var normal = collision.contacts[0].normal;
            Debug.DrawRay(collision.GetContact(0).point, normal * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 5f);
            var currentVelocity = _rb.velocity;
            var direction = Vector2.Reflect(currentVelocity, normal);
            Debug.DrawRay(collision.GetContact(0).point, direction * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 5f);
            var gridPos = _gridManager.WorldPointToGridPosition(_rb.position);
            _rb.position = _gridManager.GridPositionToWorldPoint(gridPos);
            _rb.velocity = NewRotation(direction) * Vector2.up * currentVelocity.magnitude;
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            int BrickHealt = collision.gameObject.GetComponent<BrickRemove>().Health;
            collision.gameObject.GetComponent<BrickRemove>().BrickHitInit(_damage);

            if (BrickHealt - _damage <= 0)
            {
                Stop();
                Instantiate(_explotion, transform.position, transform.rotation * Quaternion.Euler(0f, 0f, transform.position.y > 0 ? 0f : 180f));
            }
        }
    }

    private Quaternion NewRotation(Vector2 direction)
    {
        var angle = Vector2.SignedAngle(direction, Vector2.up);
        var multiplier = Mathf.Round(angle / _angleLimit);
        var newAngle = -multiplier * _angleLimit;
        return Quaternion.Euler(0, 0, newAngle);
    }

}
