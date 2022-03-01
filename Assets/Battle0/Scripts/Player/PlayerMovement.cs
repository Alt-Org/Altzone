using Battle0.Scripts.interfaces;
using Photon.Pun;
using UnityEngine;

namespace Battle0.Scripts.Player
{
    /// <summary>
    /// Simple player movement using external controller for movement and synchronized across network using <c>RPC</c>.
    /// </summary>
    /// <remarks>
    /// Player movement is restricted to given area.
    /// </remarks>
    [RequireComponent(typeof(PhotonView))]
    public class PlayerMovement : MonoBehaviour, IMovablePlayer, IRestrictedPlayer
    {
        [Header("Live Data"), SerializeField] protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;

        [Header("RPC Input"), SerializeField] private float speed;
        [SerializeField] private bool isMoving;
        [SerializeField] private Vector3 validTarget;

        [Header("Debug"), SerializeField] private bool _canMove;
        [SerializeField] private Vector3 inputTarget;
        [SerializeField] private Rect playArea;

        private IPlayerActor playerActor;

        bool IRestrictedPlayer.canMove
        {
            get => _canMove;
            set => _canMove = value;
        }

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _transform = GetComponent<Transform>();
            playerActor = GetComponent<PlayerActor>();
            _canMove = true;
        }

        private void Update()
        {
            if (!isMoving)
            {
                return;
            }
            if (!_canMove)
            {
                return;
            }
            var nextPosition = Vector3.MoveTowards(_transform.position, validTarget, speed * Time.deltaTime);
            isMoving = nextPosition != validTarget;
            _transform.position = nextPosition;
        }

        void IMovablePlayer.moveTo(Vector3 position)
        {
            if (!_canMove)
            {
                return;
            }
            if (position.Equals(inputTarget))
            {
                return;
            }
            inputTarget = position;
            position.x = Mathf.Clamp(inputTarget.x, playArea.xMin, playArea.xMax);
            position.y = Mathf.Clamp(inputTarget.y, playArea.yMin, playArea.yMax);
            // Send position to all players
            _photonView.RPC(nameof(MoveTowardsRpc), RpcTarget.All, position, playerActor.CurrentSpeed);
        }

        void IRestrictedPlayer.setPlayArea(Rect area)
        {
            playArea = area;
        }

        [PunRPC]
        private void MoveTowardsRpc(Vector3 targetPosition, float targetSpeed)
        {
            isMoving = true;
            validTarget = targetPosition;
            speed = targetSpeed;
        }
    }
}