using UnityEngine;
using Photon.Pun;

using Prg.Scripts.Common.PubSub;
using Battle.Scripts.Battle.Game;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassProjection : MonoBehaviour, IPlayerClass
    {
        // Serialized Fields
        [SerializeField] PlayerActor _playerActor;
        [SerializeField] PlayerActor _teammatePlayerActor;
        [SerializeField] Transform _fakeBallTransform;

        public bool BounceOnBallShieldCollision => false;

        public void OnBallShieldCollision()
        {
            float speed = _ballHandler.GetComponent<Rigidbody2D>().velocity.magnitude;

            const float teleportDuration = 0.5f;

            _ballHandler.Stop();

            if (PhotonNetwork.IsMasterClient)
            {
                int ballTeleportUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(teleportDuration);

                _photonView.RPC(nameof(TeleportBallRpc), RpcTarget.All, ballTeleportUpdateNumber, speed);
            }

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldCollision called", _syncedFixedUpdateClock.UpdateCount));
        }

        public void OnBallShieldBounce()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldBounce called", _syncedFixedUpdateClock.UpdateCount));
        }

        private SyncedFixedUpdateClock _syncedFixedUpdateClock;
        private BallHandler _ballHandler;
        private PhotonView _photonView;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS PROJECTION] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;

        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            _ballHandler = Context.GetBallHandler;
            _photonView = GetComponent<PhotonView>();
            _fakeBallTransform.gameObject.SetActive(false);

            // Subscribe to TeamsAreReadyForGameplay event
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            // Find and store the teammate
            FindTeammate(data);
        }

        private void FindTeammate(TeamsAreReadyForGameplay data)
        {
            int teamNumber = PhotonBattle.NoTeamValue;

            // Iterate through all drivers to find the teammate
            foreach (IDriver driver in data.AllDrivers)
            {
                PlayerActor playerActor = driver.PlayerActor;
                if (playerActor != null && playerActor == transform.parent.GetComponentInParent<PlayerActor>())
                {
                    teamNumber = driver.TeamNumber;
                    _playerActor = playerActor;
                    break;
                }
            }

            foreach (IDriver driver in data.AllDrivers)
            {
                PlayerActor playerActor = driver.PlayerActor;
                if (playerActor != null && playerActor != transform.parent.GetComponentInParent<PlayerActor>() && driver.TeamNumber == teamNumber)
                {
                    _teammatePlayerActor = playerActor;
                    return; // Once the teammate is found, exit the loop
                }
            }
        }

        [PunRPC]
        private void TeleportBallRpc(int ballTeleportUpdateNumber, float ballSpeed)
        {
            _syncedFixedUpdateClock.ExecuteOnUpdate(ballTeleportUpdateNumber, -5, () => {
                Vector2 direction = new Vector2(0, _teammatePlayerActor.ShieldTransform.position.y > 0 ? -1f : 1f);

                Vector2 position = new Vector2(
                    _teammatePlayerActor.ShieldTransform.position.x,
                    _teammatePlayerActor.ShieldTransform.position.y + direction.y
                );

                Debug.DrawLine(position, position + direction, Color.red, 3);

                _ballHandler.Launch(position, direction, ballSpeed);
            });
        }
    }
}
