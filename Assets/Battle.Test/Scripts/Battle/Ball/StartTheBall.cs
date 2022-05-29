using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.interfaces;
using Battle.Test.Scripts.Battle.Players;
using Photon.Pun;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Ball
{
    internal class StartTheBall : MonoBehaviour
    {
        private static StartTheBall _instance;

        [Header("Debug Settings"), SerializeField] private float _testDelay;
        [SerializeField] private float _forceSpeedOverride;

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

        public void StartBallFirstTime()
        {
            Debug.Log($"{name} delayToStart {_delayToStart}");
            _gameplayManager = GameplayManager.Get();
            Assert.IsNotNull(_gameplayManager, "_gameplayManager != null");
            _ballManager = BallManager.Get();
            Assert.IsNotNull(_ballManager, "_ballManager != null");
            StartCoroutine(StartBallRoutine(null));
        }

        public static void RestartBallInGame(IPlayerDriver playerToStart)
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StartTheBall>();
            }
            Debug.Log($"{_instance.name} delayToStart {_instance._delayToStart} player pos  {playerToStart.Position}");
            _instance.StartCoroutine(_instance.StartBallRoutine(playerToStart));
        }

        private IEnumerator StartBallRoutine(IPlayerDriver playerToStart)
        {
            print("~~");
            Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
            Assert.IsTrue(PhotonNetwork.IsMasterClient, "PhotonNetwork.IsMasterClient");

            _ballManager.SetBallState(BallState.Hidden);
            _ballManager.SetBallPosition(Vector2.zero);
            yield return null;

            _gameplayManager.ForEach(player => player.SetPlayMode(BattlePlayMode.Normal));
            yield return null;

            int startingTeam;
            BattleTeam startTeam;
            BattleTeam otherTeam;
            float speed;
            if (playerToStart != null)
            {
                startingTeam = playerToStart.TeamNumber;

                var delay = new WaitForSeconds(_delayToStart);
                var tracker = _gameplayManager.GetTeamSnapshotTracker(startingTeam);
                yield return delay;

                startTeam = _gameplayManager.GetBattleTeam(startingTeam);
                otherTeam = _gameplayManager.GetOppositeTeam(startingTeam);
                
                var startAttack = startTeam.Attack;
                var startDistance = Mathf.Sqrt(tracker.GetSqrDistance);
                speed = startAttack * startDistance;
                Debug.Log(
                    $"{name} RESTART attack {startAttack} dist {startDistance:0.00} TEAM {startingTeam} players {startTeam.PlayerCount} speed {speed:0.00}");
            }
            else
            {
                var delay = new WaitForSeconds(_delayToStart);
                var tracker1 = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamBlueValue);
                var tracker2 = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamRedValue);
                yield return delay;

                tracker1.StopTracking();
                tracker2.StopTracking();
                yield return null;

                float startDistance;
                float otherDistance;
                {
                    var distance1 = Mathf.Sqrt(tracker1.GetSqrDistance);
                    var distance2 = Mathf.Sqrt(tracker2.GetSqrDistance);
                    if (distance1 > distance2)
                    {
                        startingTeam = PhotonBattle.TeamBlueValue;
                        startDistance = distance1;
                        otherDistance = distance2;
                    }
                    else
                    {
                        startingTeam = PhotonBattle.TeamRedValue;
                        startDistance = distance2;
                        otherDistance = distance1;
                    }
                }
                startTeam = _gameplayManager.GetBattleTeam(startingTeam);
                otherTeam = _gameplayManager.GetOppositeTeam(startingTeam);
                var startAttack = startTeam.Attack;
                var otherAttack = otherTeam?.Attack ?? 0;
                speed = startAttack * startDistance - otherAttack * otherDistance;
                Debug.Log(
                    $"{name} START attack {startAttack} dist {startDistance:0.00} - OTHER attack {otherAttack} dist {otherDistance:0.00} TEAM {startingTeam} players {startTeam.PlayerCount} speed {speed:0.00}");
            }

            Vector2 direction;
            Vector2 ballDropPosition;
            var center = Vector3.zero;
            var transform1 = startTeam.FirstPlayer.PlayerTransform;
            var pos1 = transform1.position;
            if (startTeam.PlayerCount == 1)
            {
                direction = center - pos1;
                ballDropPosition = pos1;
            }
            else
            {
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
            Debug.Log($"{name} speed {speed:0.00} direction {direction.normalized} position {ballDropPosition}");
            if (_forceSpeedOverride > 0)
            {
                speed = _forceSpeedOverride;
            }
            if (speed == 0)
            {
                // This can happen if
                // - both teams has the same attack value and
                // - no player has moved (thus distances can be floating point exactly the same for both teams)
                speed = 1;
                if (otherTeam == null)
                {
                    direction = Vector2.one * (startingTeam == PhotonBattle.TeamBlueValue ? 1 : -1);
                }
                else
                {
                    direction = Vector2.one * (Time.frameCount % 2 == 0 ? 1 : -1);
                }
            }
            startTeam.SetPlayMode(BattlePlayMode.SuperGhosted);
            otherTeam?.SetPlayMode(BattlePlayMode.Ghosted);
            yield return null;

            _ballManager.SetBallPosition(ballDropPosition);
            _ballManager.SetBallState(BallState.NoTeam);
            _ballManager.SetBallSpeed(speed, direction);
            print("~~");
        }
    }
}