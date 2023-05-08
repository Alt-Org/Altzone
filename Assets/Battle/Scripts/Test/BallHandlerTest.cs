using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using UnityConstants;
using UnityEngine;

public class BallHandlerTest : MonoBehaviour
{
    private GridManager _gridManager;
    private PlayerPlayArea _battlePlayArea;
    private float _arenaScaleFactor;
    private float _angleLimit;

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;

    private void Start()
    {
        _battlePlayArea = Context.GetBattlePlayArea;
        var variables = GameConfig.Get().Variables;
        _angleLimit = variables._angleLimit;
        _gridManager = Context.GetGridManager;
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _sprite.enabled = false;
        _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
        transform.localScale = Vector3.one * _arenaScaleFactor;
    }

    public void Launch(Vector3 position, Vector3 direction, float speed)
    {
        _rb.position = position;
        _rb.velocity = NewRotation(direction) * Vector2.up * speed;
        _sprite.enabled = true;
    }

    //private void Update()
    //{
    //    var velocity = rb.velocity;
    //    var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
    //    _transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(Tags.Player))
        {
            var normal = collision.contacts[0].normal;
            UnityEngine.Debug.DrawRay(collision.GetContact(0).point, normal * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 5f);
            var currentVelocity = _rb.velocity;
            var direction = Vector2.Reflect(currentVelocity, normal);
            UnityEngine.Debug.DrawRay(collision.GetContact(0).point, direction * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 5f);
            var gridPos = _gridManager.WorldPointToGridPosition(_rb.position);
            _rb.position = _gridManager.GridPositionToWorldPoint(gridPos);
            _rb.velocity = NewRotation(direction) * Vector2.up * currentVelocity.magnitude;
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
