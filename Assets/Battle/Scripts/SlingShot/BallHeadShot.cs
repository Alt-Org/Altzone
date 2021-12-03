using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.interfaces;
using Battle.Scripts.Player;
using Battle.Scripts.Scene;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.SlingShot
{
    /// <summary>
    ///  Puts the ball on the game using "slingshot" method between two team mates in positions A and B.
    /// </summary>
    /// <remarks>
    /// Team mate in position A is the player whose head was hit - the "take a catch" player.
    /// </remarks>
    public class BallHeadShot : MonoBehaviourPunCallbacks, ICatchABall
    {
        private const int notPhotonOwner = -2; // -1 is reserved magic number in Photon!

        [Header("Settings"), SerializeField] private float gapToBall;
        [SerializeField] private LineRenderer line;

        [Header("Live Data"), SerializeField] private Transform followA;
        [SerializeField] private Transform followB;
        [SerializeField] private Transform ballTransform;
        [SerializeField] private Vector3 a;
        [SerializeField] private Vector3 b;
        [SerializeField] private int _teamNumber;
        [SerializeField] private float ballStartTime;

        [Header("Debug"), SerializeField] private Vector3 deltaBall;
        [SerializeField] private float distanceBall;
        [SerializeField] private int followAOwnerId;
        [SerializeField] private int followBOwnerId;

        private IBallControl ballControl;

        private void Awake()
        {
            resetMe();
            enabled = false;
        }

        private void resetMe()
        {
            followAOwnerId = notPhotonOwner;
            followBOwnerId = notPhotonOwner;
            line.gameObject.SetActive(false);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            if (!PhotonBattle.IsRealPlayer(otherPlayer))
            {
                return; // Ignore non players
            }
            var otherOwnerId = otherPlayer.ActorNumber;
            if (followAOwnerId == otherOwnerId || followBOwnerId == otherOwnerId)
            {
                resetMe();
                enabled = false;
            }
        }

        private void Update()
        {
            a = followA.position;
            b = followB.position;
            // Track line
            line.SetPosition(0, b);
            line.SetPosition(1, a);
            // Track ball
            ballTransform.position = a + (a - b).normalized * distanceBall;

            if (Time.time > ballStartTime)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    startBall();
                }
                resetMe();
                enabled = false;
            }
        }

        private void startBall()
        {
            var variables = RuntimeGameConfig.Get().Variables;
            var delta = a - b;
            var direction = delta.normalized;
            var clampedSpeed = Mathf.Clamp(Mathf.Abs(delta.magnitude), variables.minSlingShotDistance, variables.maxSlingShotDistance);
            var multiplier = RuntimeGameConfig.Get().Variables.ballMoveSpeedMultiplier;
            var speed = clampedSpeed * multiplier;
            startTheBall(ballControl, ballTransform.position, _teamNumber, direction, speed);
        }

        public void catchABall(IBallControl ball, int playerPos)
        {
            Debug.Log($"restartBall playerPos={playerPos}");
            ballControl = ball;
            ballTransform = ((Component)ballControl).transform;

            var playerA = PlayerActivator.AllPlayerActors.Find(x => x.PlayerPos == playerPos);
            playerA.setGhostedMode();
            _teamNumber = playerA.TeamNumber;
            followA = ((Component)playerA).transform;
            followAOwnerId = PhotonView.Get(followA).Owner.ActorNumber;
            var teamMate = playerA.TeamMate;
            if (teamMate != null)
            {
                teamMate.setGhostedMode();
                followB = ((Component)teamMate).transform;
                followBOwnerId = PhotonView.Get(followB).Owner.ActorNumber;
            }
            else
            {
                var teamIndex = PhotonBattle.GetTeamIndex(_teamNumber);
                followB = SceneConfig.Get().ballAnchors[teamIndex];
                followBOwnerId = notPhotonOwner;
            }
            ballControl.ghostBall();
            ballControl.moveBall(Vector2.zero, 0f);

            deltaBall = (ballTransform.position - followA.position) * (1f + gapToBall);
            distanceBall = Mathf.Abs(deltaBall.magnitude);

            ballStartTime = Time.time + RuntimeGameConfig.Get().Variables.ballRestartDelay;

            line.gameObject.SetActive(true);
            enabled = true;
        }

        private static void startTheBall(IBallControl ballControl, Vector2 position, int teamIndex, Vector2 direction, float speed)
        {
            ballControl.teleportBall(position, teamIndex);
            ballControl.showBall();
            ballControl.moveBall(direction, speed);
        }
    }
}