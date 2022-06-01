using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.interfaces;
using Battle.Test.Scripts.Battle.Ball;
using Battle.Test.Scripts.Battle.Players;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Test
{
    /// <summary>
    /// Starts the ball into gameplay
    /// </summary>
    /// <remarks>
    /// WIKI page: https://github.com/Alt-Org/Altzone/wiki/Pallo-ja-sen-liikkuminen
    /// </remarks>
    internal class StartTheBallTest : MonoBehaviour
    {
        private static StartTheBallTest _instance;

        [Header("Debug Settings"), SerializeField] private float _testDelay;
        [SerializeField] private float _forceSpeedOverride;
        [SerializeField] private int[] _teamRestartCount = new int[3];

        private IGameplayManager _gameplayManager;
        private IBallManager _ballManager;
        private float _delayToStart;

        private void Awake()
        {
            Debug.Log($"{name}");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _delayToStart = variables._roomStartDelay;
            if (_testDelay > 0)
            {
                _delayToStart = _testDelay;
            }
        }

        private void LoadDependencies()
        {
            // If master client is switched during gameplay it might be that StartBallFirstTime and RestartBallInGame
            // are not called on same instance.
            if (_gameplayManager == null)
            {
                _gameplayManager = GameplayManager.Get();
                Assert.IsNotNull(_gameplayManager, "_gameplayManager != null");
            }
            if (_ballManager == null)
            {
                _ballManager = BallManager.Get();
                Assert.IsNotNull(_ballManager, "_ballManager != null");
            }
        }

        public void StartBallFirstTime()
        {
            Debug.Log($"{name} delayToStart {_delayToStart}");
            StartCoroutine(StartBallRoutine(null));
        }

        public static void RestartBallInGame(IPlayerDriver playerToStart)
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StartTheBallTest>();
            }
            Debug.Log($"{_instance.name} delayToStart {_instance._delayToStart} player pos  {playerToStart.Position}");
            _instance.StartCoroutine(_instance.StartBallRoutine(playerToStart));
        }

        private IEnumerator StartBallRoutine(IPlayerDriver playerToStart)
        {
            // I must admit that this method is not easiest to write or read afterwards.
            LoadDependencies();
            print("~~");
            Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
            Assert.IsTrue(PhotonNetwork.IsMasterClient, "PhotonNetwork.IsMasterClient");

            _ballManager.SetBallState(BallState.Hidden);
            _ballManager.SetBallPosition(Vector2.zero);
            yield return null;

            _gameplayManager.ForEach(player => player.SetPlayMode(BattlePlayMode.Normal));
            yield return null;

            BattleTeam startTeam;
            BattleTeam otherTeam;
            float speed;
            var delay = new WaitForSeconds(_delayToStart);
            if (playerToStart != null)
            {
                _teamRestartCount[playerToStart.TeamNumber] += 1;
                var tracker = _gameplayManager.GetTeamSnapshotTracker(playerToStart.TeamNumber);
                yield return delay;

                tracker.StopTracking();
                yield return null;

                var startingTeam = playerToStart.TeamNumber;
                startTeam = _gameplayManager.GetBattleTeam(startingTeam);
                otherTeam = _gameplayManager.GetOppositeTeam(startingTeam);

                var startAttack = startTeam.Attack;
                var startDistance = Mathf.Sqrt(tracker.GetSqrDistance);
                // Official formula for ball speed (continue gameplay)
                speed = startAttack * startDistance;
                Debug.Log(
                    $"{name} RESTART attack {startAttack} dist {startDistance:0.00} TEAM {startingTeam} players {startTeam.PlayerCount} speed {speed:0.00}");
            }
            else
            {
                var blueTracker = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamBlueValue);
                var redTracker = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamRedValue);
                yield return delay;

                blueTracker.StopTracking();
                redTracker.StopTracking();
                yield return null;

                var startingTeam = GetStatingTeamByDistance(blueTracker, redTracker, out var startDistance, out var otherDistance);
                startTeam = _gameplayManager.GetBattleTeam(startingTeam);
                otherTeam = _gameplayManager.GetOppositeTeam(startingTeam);

                var startAttack = startTeam.Attack;
                var otherAttack = otherTeam?.Attack ?? 0;
                // Official formula for ball speed (start gameplay)
                speed = startAttack * startDistance - otherAttack * otherDistance;
                if (speed == 0)
                {
                    speed = startAttack * startDistance;
                }
                Debug.Log(
                    $"{name} START attack {startAttack} dist {startDistance:0.00} - OTHER attack {otherAttack} dist {otherDistance:0.00} TEAM {startingTeam} players {startTeam.PlayerCount} speed {speed:0.00}");
            }

            GetDirectionAndPosition(startTeam, out var direction, out var ballDropPosition);

            Debug.Log($"{name} speed {speed:0.00} direction {direction.normalized} position {ballDropPosition}");
            if (_forceSpeedOverride > 0)
            {
                speed = _forceSpeedOverride;
            }
            startTeam.SetPlayMode(BattlePlayMode.SuperGhosted);
            otherTeam?.SetPlayMode(BattlePlayMode.Ghosted);
            yield return null;

            _ballManager.SetBallPosition(ballDropPosition);
            _ballManager.SetBallState(BallState.NoTeam);
            _ballManager.SetBallSpeed(speed, direction);
            print("~~");
        }

        private static int GetStatingTeamByDistance(ITeamSnapshotTracker blueTracker, ITeamSnapshotTracker redTracker, out float startDistance,
            out float otherDistance)
        {
            var blueDistance = Mathf.Sqrt(blueTracker.GetSqrDistance);
            var redDistance = Mathf.Sqrt(redTracker.GetSqrDistance);
            if (blueDistance < redDistance)
            {
                startDistance = redDistance;
                otherDistance = blueDistance;
                return PhotonBattle.TeamRedValue;
            }
            startDistance = blueDistance;
            otherDistance = redDistance;
            return PhotonBattle.TeamBlueValue;
        }

        private static void GetDirectionAndPosition(BattleTeam startTeam, out Vector2 direction, out Vector2 ballDropPosition)
        {
            var center = Vector3.zero;

            var transform1 = startTeam.FirstPlayer.PlayerTransform;
            var pos1 = transform1.position;
            if (startTeam.PlayerCount == 1)
            {
                direction = center - pos1;
                ballDropPosition = pos1;
                return;
            }
            var transform2 = startTeam.SecondPlayer.PlayerTransform;
            var pos2 = transform2.position;
            var dist1 = Mathf.Abs((pos1 - center).sqrMagnitude);
            var dist2 = Mathf.Abs((pos2 - center).sqrMagnitude);
            if (dist1 < dist2)
            {
                direction = pos1 - pos2;
                ballDropPosition = pos1;
            }
            else
            {
                direction = pos2 - pos1;
                ballDropPosition = pos2;
            }
        }
    }
}