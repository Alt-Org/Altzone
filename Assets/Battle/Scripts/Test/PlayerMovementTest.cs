using System.Collections;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementTest : MonoBehaviourPun, IPunObservable
{
    public const int PlayerPosition1 = 1;
    private Transform _transform;
    private Camera _camera;

    private Vector2 _targetPosition;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _playerMoveSpeedMultiplier;
    [SerializeField] public EdgeCollider2D _shieldCollider;
    [SerializeField] private ShieldTriggerOnTest _shieldTriggerOn;
    [SerializeField] private float _shieldBackOnDelay = 1f;

    private IBattlePlayArea _battlePlayArea;
    private Rect _playArea;

    private GridManager _gridManager;

    private Vector2 _tempPosition;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) => throw new System.NotImplementedException();

    private void Awake()
    {
        _targetPosition = transform.position;
    }

    private void OnEnable()
    {
        _gridManager = Context.GetGridManager;
        _transform = transform;
        _camera = Camera.main;
        _battlePlayArea = Context.GetBattlePlayArea;
        _playArea = _battlePlayArea.GetPlayerPlayArea(PlayerPosition1);
    }

    void Update()
    {
        if (base.photonView.IsMine)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log("Mouse button pressed!");
                _targetPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
            var maxWorld = _camera.ViewportToWorldPoint(Vector2.one);
            var minWorld = _camera.ViewportToWorldPoint(Vector2.zero);
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _playArea.xMin, _playArea.xMax);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, _playArea.yMin, _playArea.yMax);
            var gridPos = _gridManager.WorldPointToGridPosition(_targetPosition);
            _targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            var maxDistanceDelta = _movementSpeed * _playerMoveSpeedMultiplier * Time.deltaTime;
            _tempPosition = Vector2.MoveTowards(_transform.position, _targetPosition, maxDistanceDelta);
            _transform.position = _tempPosition;
        }
    }

    public void TurnShieldOn()
    {
        Debug.Log("turning shield ON");
        _shieldCollider.enabled = true;
    }

    public void TurnShieldOff()
    {
        Debug.Log("turning shield OFF");
        _shieldCollider.enabled = false;
        StartCoroutine(TurnShieldBackOn());
    }

    private IEnumerator TurnShieldBackOn()
    {
        yield return new WaitForSeconds(_shieldBackOnDelay);
        if (!_shieldTriggerOn.isTouching)
        {
            TurnShieldOn();
        }
    }
}
