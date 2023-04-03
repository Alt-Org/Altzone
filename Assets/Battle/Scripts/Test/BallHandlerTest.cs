using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Photon.Pun;
using UnityConstants;
using UnityEngine;

public class BallHandlerTest : MonoBehaviour
{
    [SerializeField] private int _startingSpeed;
    private GridManager _gridManager;
    private PlayerPlayArea _battlePlayArea;
    private float _arenaScaleFactor;
    private float _angleLimit;

    private const float waitTime = 2f;

    private Rigidbody2D _rb;

    private void Start()
    {
        _battlePlayArea = Context.GetBattlePlayArea;
        var variables = GameConfig.Get().Variables;
        _angleLimit = variables._angleLimit;
        _gridManager = Context.GetGridManager;
        _rb = GetComponent<Rigidbody2D>();
        _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
        transform.localScale = Vector3.one * _arenaScaleFactor;
        StartCoroutine(LaunchBall());
    }

    private IEnumerator LaunchBall()
    {
        yield return new WaitForSeconds(waitTime);
        if (!PhotonNetwork.IsMasterClient)
        {
            yield break;
        }
        var randomDir = new Vector2(Random.Range(-4f, 4f), Random.Range(4f, 8f));
        var randomSide = Random.value;
        if (randomSide < 0.5)
        {
            _rb.velocity = NewRotation(randomDir) * Vector2.up * - _startingSpeed;
            yield break;
        }
        _rb.velocity = NewRotation(randomDir) * Vector2.up * _startingSpeed;
    }

    //private void Update()
    //{
    //    var velocity = rb.velocity;
    //    var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
    //    _transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
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
