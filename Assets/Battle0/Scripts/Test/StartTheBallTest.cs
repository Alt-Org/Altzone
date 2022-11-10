using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle0.Scripts.Battle;
using Battle0.Scripts.Battle.Game;
using Battle0.Scripts.Ui;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle0.Scripts.Test
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

        [Header("Debug Settings"), SerializeField] private float _forceSpeedOverride;

        [Header("Restart Game"), Tooltip(Tooltip), SerializeField] private bool _isRestartGameSimulation;
        [SerializeField] private bool _isRestartGameOverLevel;
        [Min(1), SerializeField] private int _gameOverRestartLimit = 1;
        [SerializeField] private int[] _teamRestartCount = new int[2];

        [Header("Optional Timer"), SerializeField] private SimpleTimerHelper _simpleTimer;

        private IPlayerManager _playerManager;
        private IBallManager _ballManager;
        private float _ballSlingshotPower;
        private float _playerAttackMultiplier;
        private OnGuiWindowHelper _onGuiWindow;
        private int _roomStartDelay;
        private int _slingshotDelay;

        private void Awake()
        {
            Debug.Log($"{name}");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            var runtimeGameConfig = Battle0GameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _ballSlingshotPower = variables._ballSlingshotPower;
            _playerAttackMultiplier = variables._playerAttackMultiplier;
            _roomStartDelay = variables._battleRoomStartDelay;
            _slingshotDelay = variables._battleSlingshotDelay;
        }

        private void LoadDependencies()
        {
            // If master client is switched during gameplay it might be that StartBallFirstTime and RestartBallInGame
            // are not called on same instance.
            if (_playerManager == null)
            {
                _playerManager = Context.PlayerManager;
                Assert.IsNotNull(_playerManager, "_gameplayManager != null");
            }
            if (_ballManager == null)
            {
                _ballManager = Context.BallManager;
                Assert.IsNotNull(_ballManager, "_ballManager != null");
            }
        }

        public void StartBallFirstTime()
        {
            Debug.Log($"{name} delayToStart {_roomStartDelay} IsMasterClient {PhotonNetwork.IsMasterClient}");
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
            Debug.Log($"{name} delayToStart {_slingshotDelay} player pos  {playerToStart.Position}");
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
            _playerManager.ForEach(player => player.SetPlayMode(BattlePlayMode.Normal));
            StartCoroutine(StartBallWorkRoutine(playerToStart));
        }

        private IEnumerator StartGameOver(IPlayerDriver playerToStart)
        {
            if (_isRestartGameOverLevel)
            {
                var gameScoreManager = Context.GetGameScoreManager;
                if (gameScoreManager != null)
                {
                    _playerManager.ForEach(player => player.SetPlayMode(BattlePlayMode.Frozen));
                    yield return null;
                    var room = PhotonNetwork.CurrentRoom;
                    var blueScore = _teamRestartCount[PhotonBattle.TeamBlueValue - 1];
                    var redScore = _teamRestartCount[PhotonBattle.TeamRedValue - 1];
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
                _playerManager.ForEach(player =>
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
            _playerManager.ForEach(player => player.SetPlayMode(BattlePlayMode.Normal));
            StartCoroutine(StartBallWorkRoutine(playerToStart));
        }

        private IEnumerator StartBallWorkRoutine(IPlayerDriver playerToStart)
        {
            IBattleTeam startTeam;
            IBattleTeam otherTeam;
            float speed;
            // During count down period players can move freely
            var countdownSeconds = playerToStart != null
                ? _slingshotDelay
                : _roomStartDelay;
            var countDownDelay = new WaitForSeconds(countdownSeconds);
            if (playerToStart != null)
            {
                var startingTeam = playerToStart.TeamNumber;
                var tracker = _playerManager.GetTeamSnapshotTracker(playerToStart.TeamNumber);
                this.Publish(new UiEvents.SlingshotStart(tracker, null));
                Debug.Log($"{name} WAIT {countdownSeconds} mode {playerToStart.BattlePlayMode}");
                yield return countDownDelay;

                tracker.StopTracking();
                this.Publish(new UiEvents.SlingshotEnd(tracker, null));
                yield return null;

                startTeam = _playerManager.GetBattleTeam(startingTeam);
                otherTeam = null;
                speed = 0;
                if (startTeam != null)
                {
                    var slingshotPower = _ballSlingshotPower * startTeam.Attack * _playerAttackMultiplier;
                    var startDistance = Mathf.Sqrt(tracker.SqrDistance);
                    // Official formula for ball speed (continue gameplay)
                    speed = slingshotPower * startDistance;
                    Debug.Log($"{name} RESTART slingshotPower {slingshotPower} dist {startDistance:0.00} " +
                              $"TEAM {startingTeam} players {startTeam.PlayerCount} speed {speed:0.00}");
                }
                if (speed == 0)
                {
                    Debug.LogWarning("SOMETHING WENT WRONG - DID A PLAYER LEAVE THE GAME?");
                    speed = 1f;
                }
            }
            else
            {
                var blueTracker = _playerManager.GetTeamSnapshotTracker(PhotonBattle.TeamBlueValue);
                var redTracker = _playerManager.GetTeamSnapshotTracker(PhotonBattle.TeamRedValue);
                this.Publish(new UiEvents.SlingshotStart(blueTracker, redTracker));
                Debug.Log($"{name} WAIT {countdownSeconds}");
                yield return countDownDelay;

                blueTracker.StopTracking();
                redTracker.StopTracking();
                var startingTeam = GetStatingTeamByDistance(blueTracker, redTracker, out var startDistance, out var otherDistance);
                this.Publish(startingTeam == blueTracker.TeamNumber
                    ? new UiEvents.SlingshotEnd(blueTracker, redTracker)
                    : new UiEvents.SlingshotEnd(redTracker, blueTracker));
                yield return null;

                startTeam = _playerManager.GetBattleTeam(startingTeam);
                otherTeam = _playerManager.GetOppositeTeam(startingTeam);
                var startSlingshotPower = _ballSlingshotPower * startTeam.Attack * _playerAttackMultiplier;
                var otherSlingshotPower = _ballSlingshotPower * (otherTeam?.Attack ?? 0) * _playerAttackMultiplier;
                // Official formula for ball speed (start gameplay)
                speed = startSlingshotPower * startDistance - otherSlingshotPower * otherDistance;
                if (speed == 0)
                {
                    // Use only winning teams data if both teams have exactly the same attack value and distance to each other.
                    // - only possible if nobody moves!
                    speed = startSlingshotPower * startDistance;
                }
                Debug.Log($"{name} START slingshotPower {startSlingshotPower} dist {startDistance:0.00} - " +
                          $"OTHER slingshotPower {otherSlingshotPower} dist {otherDistance:0.00} " +
                          $"TEAM {startingTeam} players {startTeam.PlayerCount} speed {speed:0.00}");
            }

            startTeam.GetBallDropPositionAndDirection(out var ballDropPosition, out var direction);

            Debug.Log($"{name} speed {speed:0.00} direction {direction.normalized} position {ballDropPosition}");
            if (_forceSpeedOverride > 0)
            {
                speed = _forceSpeedOverride;
            }
            // We can lose start team during the time it takes to (re)start the ball!
            startTeam?.SetPlayMode(BattlePlayMode.SuperGhosted);
            otherTeam?.SetPlayMode(BattlePlayMode.Normal);
            yield return null;

            _ballManager.SetBallPosition(ballDropPosition);
            _ballManager.SetBallState(BallState.Moving);
            _ballManager.SetBallSpeed(speed, direction);
            Debug.Log($"{name} DONE");
        }

        private static int GetStatingTeamByDistance(ITeamSlingshotTracker blueTracker, ITeamSlingshotTracker redTracker, out float startDistance,
            out float otherDistance)
        {
            var blueDistance = Mathf.Sqrt(blueTracker.SqrDistance);
            var redDistance = Mathf.Sqrt(redTracker.SqrDistance);
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
    }
}