using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.interfaces;
using Battle.Scripts.Player;
using Battle.Scripts.Room;
using Battle.Scripts.SlingShot;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Ball
{
    /// <summary>
    /// Simple ball with <c>Rigidbody2D</c> that synchronizes its movement across network using <c>PhotonView</c> and <c>RPC</c>.
    /// </summary>
    public class BallActor : MonoBehaviour, IPunObservable, IBallControl
    {
        private const int visibilityModeNormal = 0;
        private const int visibilityModeHidden = 1;
        private const int visibilityModeGhosted = 2;

        public static IBallControl Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<BallActor>();
            }
            return _Instance;
        }

        private static IBallControl _Instance;

        [Header("Settings"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Collider2D _collider;

        [SerializeField] private LayerMask collisionToHeadMask;
        [SerializeField] private int collisionToHead;
        [SerializeField] private LayerMask collisionToWallMask;
        [SerializeField] private int collisionToWall;
        [SerializeField] private LayerMask collisionToBrickMask;
        [SerializeField] private int collisionToBrick;

        [Header("Live Data"), SerializeField] private int _curTeamNumber;
        [SerializeField] private float targetSpeed;
        [SerializeField] private BallCollision ballCollision;
        [SerializeField] private ICatchABall ballHeadShot;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        private Rigidbody2D _rigidbody;
        private PhotonView _photonView;
        private IBallColor ballColor;

        // Configurable settings
        private GameVariables variables;

        private void Awake()
        {
            Debug.Log("Awake");
            variables = RuntimeGameConfig.Get().Variables;
            _rigidbody = GetComponent<Rigidbody2D>();
            _photonView = PhotonView.Get(this);
            _rigidbody.isKinematic = !_photonView.IsMine;
            _collider.enabled = false;

            collisionToHead = collisionToHeadMask.value;
            collisionToWall = collisionToWallMask.value;
            collisionToBrick = collisionToBrickMask.value;

            _curTeamNumber = PhotonBattle.NoTeamValue;
            targetSpeed = 0;
            ballCollision = gameObject.AddComponent<BallCollision>();
            ballCollision.enabled = false;
            ((IBallCollisionSource)ballCollision).onCurrentTeamChanged = onCurrentTeamChanged;
            ((IBallCollisionSource)ballCollision).onCollision2D = onBallCollision;
            ballHeadShot = GetComponent<BallHeadShot>(); // Disables itself automatically
            ballColor = GetComponent<BallColor>();
            ballColor.initialize(); // Must initialize ball color explicitly to avoid script execution order problems
            if (PhotonNetwork.IsMasterClient)
            {
                gameObject.AddComponent<BallWatchdog>();
            }
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        private void onCurrentTeamChanged(int newTeamNumber)
        {
            Debug.Log($"onCurrentTeamChanged ({_curTeamNumber}) <- ({newTeamNumber})");
            _curTeamNumber = newTeamNumber;
            this.Publish(new ActiveTeamEvent(newTeamNumber));
        }

        private void onBallCollision(Collision2D other)
        {
            var otherGameObject = other.gameObject;
            var colliderMask = 1 << otherGameObject.layer;
            if (collisionToBrick == (collisionToBrick | colliderMask))
            {
                BrickManager.deleteBrick(other.gameObject);
                return;
            }
            if (collisionToWall == (collisionToWall | colliderMask))
            {
                ScoreManager.AddWallScore(other.gameObject);
                return;
            }
            if (collisionToHead == (collisionToHead | colliderMask))
            {
                // Contract: player is one level up from head collider
                var playerActor = otherGameObject.GetComponentInParent<PlayerActor>() as IPlayerActor;
                playerActor.headCollision(this);
                return;
            }
            Debug.Log($"onBallCollision UNHANDLED team={_curTeamNumber} other={other.gameObject.name}");
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            ((IBallControl)this).showBall();
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            if (PhotonNetwork.InRoom)
            {
                ((IBallControl)this).hideBall();
            }
        }

        int IBallControl.currentTeamIndex => _curTeamNumber;

        void IBallControl.teleportBall(Vector2 position, int teamNumber)
        {
            onCurrentTeamChanged(teamNumber);
            _rigidbody.position = position;
            _collider.enabled = true;
        }

        void IBallControl.moveBall(Vector2 direction, float speed)
        {
            targetSpeed = speed;
            _rigidbody.velocity = direction.normalized * speed;
            Debug.Log($"moveBall position={_rigidbody.position} velocity={_rigidbody.velocity} speed={targetSpeed}");
        }

        void IBallControl.showBall()
        {
            _photonView.RPC(nameof(setBallVisibilityRpc), RpcTarget.All, visibilityModeNormal);
        }

        void IBallControl.hideBall()
        {
            _photonView.RPC(nameof(setBallVisibilityRpc), RpcTarget.All, visibilityModeHidden);
        }

        void IBallControl.ghostBall()
        {
            _photonView.RPC(nameof(setBallVisibilityRpc), RpcTarget.All, visibilityModeGhosted);
        }

        void IBallControl.catchABallFor(IPlayerActor player)
        {
            _photonView.RPC(nameof(catchABallRpc), RpcTarget.All, player.PlayerPos);
        }

        private void _showBall()
        {
            ballColor.setNormalMode();
            ballCollision.enabled = true;
            _sprite.enabled = true;
            _collider.enabled = true;
            Debug.Log($"showBall position={_rigidbody.position}");
        }

        private void _hideBall()
        {
            ballCollision.enabled = false;
            _sprite.enabled = false;
            _collider.enabled = false;
            // Stop it
            targetSpeed = 0;
            _rigidbody.velocity = Vector2.zero;
            Debug.Log($"hideBall position={_rigidbody.position}");
        }

        private void _ghostBall()
        {
            ballColor.setGhostedMode();
            ballCollision.enabled = false;
            _sprite.enabled = true;
            _collider.enabled = false;
            Debug.Log($"ghostBall position={_rigidbody.position}");
        }

        private void _catchABall(int playerPos)
        {
            Debug.Log($"restartBall playerPos={playerPos}");
            ballHeadShot.catchABall(this, playerPos);
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_rigidbody.position);
                stream.SendNext(_rigidbody.velocity);
            }
            else
            {
                networkPosition = (Vector2)stream.ReceiveNext();
                _rigidbody.velocity = (Vector2)stream.ReceiveNext();

                networkLag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                networkPosition += _rigidbody.velocity * networkLag;
            }
        }

        private void Update()
        {
            if (!_photonView.IsMine)
            {
                var curPos = _rigidbody.position;
                var deltaX = Mathf.Abs(curPos.x - networkPosition.x);
                var deltaY = Mathf.Abs(curPos.y - networkPosition.y);
                if (deltaX > variables.ballTeleportDistance || deltaY > variables.ballTeleportDistance)
                {
                    _rigidbody.position = networkPosition;
                }
                else
                {
                    _rigidbody.position = Vector2.MoveTowards(curPos, networkPosition, Time.deltaTime);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!_photonView.IsMine)
            {
                return;
            }
            if (targetSpeed > 0)
            {
                keepConstantVelocity(Time.fixedDeltaTime);
            }
        }

        private void keepConstantVelocity(float deltaTime)
        {
            var _velocity = _rigidbody.velocity;
            var targetVelocity = _velocity.normalized * targetSpeed;
            if (targetVelocity == Vector2.zero)
            {
                randomReset(_curTeamNumber);
                return;
            }
            if (targetVelocity != _rigidbody.velocity)
            {
                _rigidbody.velocity = Vector2.Lerp(_velocity, targetVelocity, deltaTime * variables.ballLerpSmoothingFactor);
            }
        }

        private void randomReset(int forTeam)
        {
            transform.position = Vector3.zero;
            var direction = forTeam == 0 ? Vector2.up : Vector2.down;
            _rigidbody.velocity = direction * targetSpeed;
        }

        [PunRPC]
        private void setBallVisibilityRpc(int visibilityMode)
        {
            switch (visibilityMode)
            {
                case visibilityModeNormal:
                    _showBall();
                    return;
                case visibilityModeHidden:
                    _hideBall();
                    return;
                case visibilityModeGhosted:
                    _ghostBall();
                    return;
                default:
                    throw new UnityException($"unknown visibility mode: {visibilityMode}");
            }
        }

        [PunRPC]
        private void catchABallRpc(int playerPos)
        {
            _catchABall(playerPos);
        }

        internal class ActiveTeamEvent
        {
            public readonly int newTeamNumber;

            public ActiveTeamEvent(int newTeamNumber)
            {
                this.newTeamNumber = newTeamNumber;
            }

            public override string ToString()
            {
                return $"team: {newTeamNumber}";
            }
        }
    }
}