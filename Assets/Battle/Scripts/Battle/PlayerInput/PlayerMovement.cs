using Battle.Scripts.Battle.interfaces;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.PlayerInput
{
    /// <summary>
    /// Simple player movement using external controller for movement and synchronized across network using <c>RPC</c>.
    /// </summary>
    /// <remarks>
    /// Player movement can be restricted to given area.
    /// </remarks>
    [RequireComponent(typeof(PhotonView))]
    internal class PlayerMovement : MonoBehaviour, IMovablePlayer, IRestrictedPlayer
    {
        [Header("Live Data"), SerializeField] private PhotonView _photonView;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;

        [Header("Debug"), SerializeField] private bool _canMove;
        [SerializeField] private float _speed;
        [SerializeField] private bool _isMoving;
        [SerializeField] private Vector3 _currentTarget;
        [SerializeField] private Vector2 _inputTarget;
        [SerializeField] private Rect _playArea;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _transform = GetComponent<Transform>();
            _camera = Camera.main;
            _currentTarget = _transform.position;
            _playArea = Rect.MinMaxRect(-100, -100, 100, 100);
            _canMove = true;
        }

        private void Update()
        {
            if (!_isMoving)
            {
                return;
            }
            if (!_canMove)
            {
                return;
            }
            var nextPosition = Vector3.MoveTowards(_transform.position, _currentTarget, _speed * Time.deltaTime);
            _isMoving = nextPosition != _currentTarget;
            _transform.position = nextPosition;
        }

        Camera IMovablePlayer.Camera => _camera;

        Transform IMovablePlayer.Transform => _transform;

        float IMovablePlayer.Speed
        {
            get => _speed;
            set => _speed = value;
        }

        void IMovablePlayer.MoveTo(Vector2 position)
        {
            if (!_canMove)
            {
                return;
            }
            if (position.Equals(_inputTarget))
            {
                return;
            }
            _inputTarget = position;
            position.x = Mathf.Clamp(_inputTarget.x, _playArea.xMin, _playArea.xMax);
            position.y = Mathf.Clamp(_inputTarget.y, _playArea.yMin, _playArea.yMax);
            // Send position to all players
            _photonView.RPC(nameof(MovePlayerRpc), RpcTarget.All, position, _speed);
        }

        bool IRestrictedPlayer.CanMove
        {
            get => _canMove;
            set => _canMove = value;
        }

        void IRestrictedPlayer.SetPlayArea(Rect area)
        {
            _playArea = area;
        }

        [PunRPC]
        private void MovePlayerRpc(Vector2 targetPosition, float targetSpeed)
        {
            _isMoving = true;
            _currentTarget.x = targetPosition.x;
            _currentTarget.y = targetPosition.y;
            _speed = targetSpeed;
        }
    }
}