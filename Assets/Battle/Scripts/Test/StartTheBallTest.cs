using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Battle.Scripts.Test
{
    /// <summary>
    /// Starts the ball into gameplay
    /// </summary>
    /// <remarks>
    /// WIKI page: https://github.com/Alt-Org/Altzone/wiki/Pallo-ja-sen-liikkuminen
    /// </remarks>
    internal class StartTheBallTest : MonoBehaviour
    {
        private const string Tooltip = "Simulate Battle game restart after 'game over' \"Restart Limit\"";

        [Header("Debug Settings"), SerializeField] private float _countDownDelayOverride;
        [SerializeField] private float _forceSpeedOverride;

        [Header("Restart Game"), Tooltip(Tooltip), SerializeField] private bool _isRestartGameSimulation;
        [SerializeField] private bool _isRestartGameOverLevel;
        [Min(1), SerializeField] private int _gameOverRestartLimit = 1;
        [SerializeField] private int[] _teamRestartCount = new int[2];

        [Header("Optional Timer"), SerializeField] private SimpleTimerHelper _simpleTimer;

        private IGameplayManager _gameplayManager;
        private IBallManager _ballManager;
        private OnGuiWindowHelper _onGuiWindow;
        private float _countDownDelay;

        private void Awake()
        {
            Debug.Log($"{name}");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _countDownDelay = variables._roomStartDelay;
            if (_countDownDelayOverride > 0)
            {
                _countDownDelay = _countDownDelayOverride;
            }
        }

        private void LoadDependencies()
        {
            // If master client is switched during gameplay it might be that StartBallFirstTime and RestartBallInGame
            // are not called on same instance.
            if (_gameplayManager == null)
            {
                _gameplayManager = Context.GameplayManager;
                Assert.IsNotNull(_gameplayManager, "_gameplayManager != null");
            }
            if (_ballManager == null)
            {
                _ballManager = Context.BallManager;
                Assert.IsNotNull(_ballManager, "_ballManager != null");
            }
        }

        public void StartBallFirstTime()
        {
            Debug.Log($"{name} delayToStart {_countDownDelay} IsMasterClient {PhotonNetwork.IsMasterClient}");
            Array.Clear(_teamRestartCount, 0, _teamRestartCount.Length);
            print("~~");
            StartCoroutine(StartBallRoutinePreload(null));
            if (_simpleTimer != null)
            {
                _simpleTimer.ResetTimer();
                _simpleTimer.StartTimer();
            }
        }

        public void RestartBallInGame(IPlayerDriver playerToStart)
        {
            Debug.Log($"{name} delayToStart {_countDownDelay} player pos  {playerToStart.Position}");
            StartCoroutine(StartBallRoutinePreload(playerToStart));
        }

        private IEnumerator StartBallRoutinePreload(IPlayerDriver playerToStart)
        {
            // I must admit that this and below methods might not be easiest to write or read afterwards.
            LoadDependencies();
            Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
            Assert.IsTrue(PhotonNetwork.IsMasterClient, "PhotonNetwork.IsMasterClient");

            _ballManager.SetBallState(BallState.Hidden);
            yield return null;

            if (playerToStart != null && (_isRestartGameSimulation || _isRestartGameOverLevel))
            {
                _teamRestartCount[playerToStart.TeamNumber - 1] += 1;
                if (_teamRestartCount[playerToStart.TeamNumber - 1] >= _gameOverRestartLimit)
                {
                    StartCoroutine(StartGameOver(playerToStart));
                    yield break;
                }
            }
            _gameplayManager.ForEach(player => player.SetPlayMode(BattlePlayMode.Normal));
            StartCoroutine(StartBallWorkRoutine(playerToStart));
        }

        private IEnumerator StartGameOver(IPlayerDriver playerToStart)
        {
            if (_isRestartGameOverLevel)
            {
                var gameScoreManager = Context.GetGameScoreManager;
                if (gameScoreManager != null)
                {
                    _gameplayManager.ForEach(player => player.SetPlayMode(BattlePlayMode.Frozen));
                    yield return null;
                    var room = PhotonNetwork.CurrentRoom;
                    var blueScore = _teamRestartCount[PhotonBattle.TeamBlueValue-1];
                    var redScore = _teamRestartCount[PhotonBattle.TeamRedValue-1];
                    var winningTeam = blueScore > redScore ? PhotonBattle.TeamBlueValue
                        : redScore > blueScore ? PhotonBattle.TeamRedValue
                        : PhotonBattle.NoTeamValue;
                    var winType = blueScore == 0 && redScore == 0 ? PhotonBattle.WinTypeNone
                        : blueScore == redScore ? PhotonBattle.WinTypeDraw
                        : PhotonBattle.WinTypeScore;
                    gameScoreManager.ShowGameOverWindow(room, winType, winningTeam, blueScore, redScore);
                    yield break;
                }
            }
            // This code block roughly simulates what "Game Over" scene does when it reloads Battle level to restart a game. 
            if (_isRestartGameSimulation)
            {
                Array.Clear(_teamRestartCount, 0, _teamRestartCount.Length);
                _gameplayManager.ForEach(player =>
                {
                    var startPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(player.PlayerPos);
                    player.SetPlayMode(BattlePlayMode.Normal);
                    player.SetCharacterPose(0);
                    player.SetShieldResistance(player.CharacterModel.Resistance);
                    player.MoveTo(startPosition);
                });
                yield return null;
                if (_onGuiWindow == null)
                {
                    _onGuiWindow = gameObject.AddComponent<OnGuiWindowHelper>();
                    _onGuiWindow._windowTitle = "Restart the GAME when ALL players are ready";
                    _onGuiWindow._buttonCaption = $" \r\nRestart the GAME ({_onGuiWindow._controlKey})\r\n ";
                    _onGuiWindow.OnKeyPressed = () => { Debug.Log("game restarted"); };
                }
                _onGuiWindow.Show();
                yield return null;
                yield return new WaitUntil(() => !_onGuiWindow.IsVisible);
            }
            _gameplayManager.ForEach(player => player.SetPlayMode(BattlePlayMode.Normal));
            StartCoroutine(StartBallWorkRoutine(playerToStart));
        }

        private IEnumerator StartBallWorkRoutine(IPlayerDriver playerToStart)
        {
            BattleTeam startTeam;
            BattleTeam otherTeam;
            float speed;
            // During count down period players can move freely
            var countDownDelay = new WaitForSeconds(_countDownDelay);
            if (playerToStart != null)
            {
                var startingTeam = playerToStart.TeamNumber;
                var tracker = _gameplayManager.GetTeamSnapshotTracker(playerToStart.TeamNumber);
                yield return countDownDelay;

                tracker.StopTracking();
                yield return null;

                startTeam = _gameplayManager.GetBattleTeam(startingTeam);
                otherTeam = null;
                speed = 0;
                if (startTeam != null)
                {
                    var startAttack = startTeam.Attack;
                    var startDistance = Mathf.Sqrt(tracker.GetSqrDistance);
                    // Official formula for ball speed (continue gameplay)
                    speed = startAttack * startDistance;
                    Debug.Log(
                        $"{name} RESTART attack {startAttack} dist {startDistance:0.00} TEAM {startingTeam} players {startTeam.PlayerCount} speed {speed:0.00}");
                }
                if (speed == 0)
                {
                    Debug.LogWarning("SOMETHING WENT WRONG - DID A PLAYER LEAVE THE GAME?");
                    speed = 1f;
                }
            }
            else
            {
                Debug.Log($"{name} WAIT {_countDownDelay}");
                var blueTracker = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamBlueValue);
                var redTracker = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamRedValue);
                yield return countDownDelay;

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
                    // Use only winning teams data
                    // if both teams have exactly the same attack value and distance to each other - only possible if nobody moves
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
            // We can lose start team during the time it takes to (re)start the ball!
            startTeam?.SetPlayMode(BattlePlayMode.SuperGhosted);
            otherTeam?.SetPlayMode(BattlePlayMode.Ghosted);
            yield return null;

            _ballManager.SetBallPosition(ballDropPosition);
            _ballManager.SetBallState(BallState.Moving);
            _ballManager.SetBallSpeed(speed, direction);
            Debug.Log($"{name} DONE");
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
            if (startTeam == null)
            {
                do
                {
                    direction = Random.insideUnitCircle;
                } while (direction.x == 0 || direction.y == 0);

                ballDropPosition = Vector2.zero;
                return;
            }
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