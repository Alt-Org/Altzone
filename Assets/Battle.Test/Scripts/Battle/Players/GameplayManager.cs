using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Battle.Test.Scripts.Battle.Ball;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Manager for gameplay activities where one or more players are involved.<br />
    /// Requires player's registration in order to participate.
    /// </summary>
    internal interface IGameplayManager
    {
        int PlayerCount { get; }

        IPlayerDriver LocalPlayer { get; }

        ITeamSnapshotTracker GetTeamSnapshotTracker(int teamNumber);

        IPlayerDriver GetPlayerByActorNumber(int actorNumber);

        void RegisterPlayer(IPlayerDriver playerDriver);

        void UnregisterPlayer(IPlayerDriver playerDriver);
    }

    internal class BattleTeam
    {
        public readonly int TeamNumber;
        public readonly IPlayerDriver FirstPlayer;
        public readonly IPlayerDriver SecondPlayer;

        public BattleTeam(int teamNumber, IPlayerDriver firstPlayer, IPlayerDriver secondPlayer)
        {
            TeamNumber = teamNumber;
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
        }
    }

    internal class PlayerJoined
    {
        public readonly IPlayerDriver Player;

        public PlayerJoined(IPlayerDriver player)
        {
            Player = player;
        }
    }

    internal class PlayerLeft : PlayerJoined
    {
        public PlayerLeft(IPlayerDriver player) : base(player)
        {
        }
    }

    internal class TeamCreated
    {
        public readonly BattleTeam BattleTeam;

        public TeamCreated(int teamNumber, IPlayerDriver firstPlayer, IPlayerDriver secondPlayer)
        {
            BattleTeam = new BattleTeam(teamNumber, firstPlayer, secondPlayer);
        }
    }

    internal class TeamBroken
    {
        public readonly IPlayerDriver PlayerWhoLeft;

        public TeamBroken(IPlayerDriver playerWhoLeft)
        {
            PlayerWhoLeft = playerWhoLeft;
        }
    }

    internal class GameplayManager : MonoBehaviour, IGameplayManager
    {
        [Serializable]
        internal class DebugSettings
        {
            public List<MonoBehaviour> _playerList;
        }

        public static IGameplayManager Get() => FindObjectOfType<GameplayManager>();

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private readonly HashSet<IPlayerDriver> _players = new();
        private readonly List<IPlayerDriver> _teamBlue = new();
        private readonly List<IPlayerDriver> _teamRed = new();

        private TeamColliderPlayModeTrigger _teamBluePlayModeTrigger;
        private TeamColliderPlayModeTrigger _teamRedPlayModeTrigger;

        private void Awake()
        {
            Debug.Log($"{name}");
            var features = RuntimeGameConfig.Get().Features;
            if (features._isDisablePlayModeChanges)
            {
                return;
            }
            var playArea = Context.GetPlayerPlayArea;
            _teamBluePlayModeTrigger = playArea.BlueTeamCollider.gameObject.AddComponent<TeamColliderPlayModeTrigger>();
            _teamBluePlayModeTrigger.TeamMembers = _teamBlue;
            _teamRedPlayModeTrigger = playArea.RedTeamCollider.gameObject.AddComponent<TeamColliderPlayModeTrigger>();
            _teamRedPlayModeTrigger.TeamMembers = _teamRed;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private BattleTeam GetBattleTeam(int teamNumber)
        {
            if (teamNumber == PhotonBattle.TeamBlueValue)
            {
                switch (_teamBlue.Count)
                {
                    case 0:
                        return new BattleTeam(teamNumber, null, null);
                    case 1:
                        return new BattleTeam(teamNumber, _teamBlue[0], null);
                    case 2:
                        return new BattleTeam(teamNumber, _teamBlue[0], _teamBlue[1]);
                }
            }
            if (teamNumber == PhotonBattle.TeamRedValue)
            {
                switch (_teamRed.Count)
                {
                    case 0:
                        return new BattleTeam(teamNumber, null, null);
                    case 1:
                        return new BattleTeam(teamNumber, _teamRed[0], null);
                    case 2:
                        return new BattleTeam(teamNumber, _teamRed[0], _teamRed[1]);
                }
            }
            return null;
        }

        private ITeamSnapshotTracker GetTeamSnapshotTracker(int teamNumber)
        {
            var tracker = new TeamSnapshotTracker(GetBattleTeam(teamNumber));
            StartCoroutine(tracker.TrackTheTeam());
            return tracker;
        }

        #region IGameplayManager

        int IGameplayManager.PlayerCount => _players.Count;

        IPlayerDriver IGameplayManager.LocalPlayer => _players.FirstOrDefault(x => x.IsLocal);

        ITeamSnapshotTracker IGameplayManager.GetTeamSnapshotTracker(int teamNumber)
        {
            return GetTeamSnapshotTracker(teamNumber);
        }

        IPlayerDriver IGameplayManager.GetPlayerByActorNumber(int actorNumber)
        {
            return _players.FirstOrDefault(x => x.ActorNumber == actorNumber);
        }

        void IGameplayManager.RegisterPlayer(IPlayerDriver playerDriver)
        {
            Debug.Log($"add {playerDriver.NickName} pos {playerDriver.PlayerPos} actor {playerDriver.ActorNumber} local {playerDriver.IsLocal}");
            Assert.IsFalse(_players.Count > 0 && _players.Any(x => x.ActorNumber == playerDriver.ActorNumber),
                "_players.Count > 0 && _players.Any(x => x.ActorNumber == playerDriver.ActorNumber)");
            _players.Add(playerDriver);
            // Publish PlayerJoined first, then TeamCreated
            this.Publish(new PlayerJoined(playerDriver));
            if (playerDriver.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                _teamBlue.Add(playerDriver);
                Assert.IsTrue(_teamBlue.Count <= 2, "_teamBlue.Count <= 2");
                if (_teamBlue.Count == 2)
                {
                    _teamBlue.Sort((a, b) => a.PlayerPos.CompareTo(b.PlayerPos));
                    this.Publish(new TeamCreated(PhotonBattle.TeamBlueValue, _teamBlue[0], _teamBlue[1]));
                }
            }
            else if (playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _teamRed.Add(playerDriver);
                Assert.IsTrue(_teamRed.Count <= 2, "_teamRed.Count <= 2");
                if (_teamRed.Count == 2)
                {
                    _teamRed.Sort((a, b) => a.PlayerPos.CompareTo(b.PlayerPos));
                    this.Publish(new TeamCreated(PhotonBattle.TeamRedValue, _teamRed[0], _teamRed[1]));
                }
            }
            else
            {
                throw new UnityException($"Invalid team number {playerDriver.TeamNumber}");
            }
            var gameCamera = Context.GetGameCamera;
            if (playerDriver.IsLocal)
            {
                CheckLocalPlayerSettings(gameCamera.Camera, playerDriver);
            }
            if (gameCamera.IsRotated)
            {
                // When game camera is rotated, we must allow all existing and new players to setup their visual orientation accordingly.
                foreach (var player in _players)
                {
                    player.FixCameraRotation(gameCamera.Camera);
                }
                BallManager.Get().FixCameraRotation(gameCamera.Camera);
            }
            UpdateDebugPlayerList();
        }

        void IGameplayManager.UnregisterPlayer(IPlayerDriver playerDriver)
        {
            Debug.Log($"remove {playerDriver.NickName} pos {playerDriver.PlayerPos} actor {playerDriver.ActorNumber}");
            _players.Remove(playerDriver);
            // Publish events in reverse order: TeamBroken first, then PlayerLeft
            if (playerDriver.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                _teamBlue.Remove(playerDriver);
                if (_teamBlue.Count == 1)
                {
                    this.Publish(new TeamBroken(_teamBlue[0]));
                }
            }
            else if (playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _teamRed.Remove(playerDriver);
                if (_teamRed.Count == 1)
                {
                    this.Publish(new TeamBroken(_teamRed[0]));
                }
            }
            this.Publish(new PlayerLeft(playerDriver));
            UpdateDebugPlayerList();
        }

        private static void CheckLocalPlayerSettings(Camera gameCamera, IPlayerDriver playerDriver)
        {
            var gameBackground = Context.GetGameBackground;
            gameBackground.SetBackgroundImageByIndex(0);

            var teamNumber = PhotonBattle.GetTeamNumber(playerDriver.PlayerPos);
            if (teamNumber == PhotonBattle.TeamBlueValue)
            {
                return;
            }
            RotateLocalPlayerGameplayExperience(gameCamera, gameBackground.Background);
        }

        private static void RotateLocalPlayerGameplayExperience(Camera gameCamera, GameObject gameBackground)
        {
            var features = RuntimeGameConfig.Get().Features;
            var isRotateGameCamera = features._isRotateGameCamera && gameCamera != null;
            if (isRotateGameCamera)
            {
                // Rotate game camera.
                Debug.Log($"RotateGameCamera upsideDown");
                gameCamera.GetComponent<Transform>().Rotate(true);
            }
            var isRotateGameBackground = features._isRotateGameBackground && gameBackground != null;
            if (isRotateGameBackground)
            {
                // Rotate background.
                Debug.Log($"RotateGameBackground upsideDown");
                gameBackground.GetComponent<Transform>().Rotate(true);
                // Separate sprites for each team gameplay area - these might not be visible in final game
                // - see Battle.Scripts.Room.RoomSetup.SetupLocalPlayer() how this is done in Altzone project.
            }
        }

        #endregion

        #region Debugging

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void UpdateDebugPlayerList()
        {
            var playerList = _players.ToList();
            playerList.Sort((a, b) => a.PlayerPos.CompareTo(b.PlayerPos));
            _debug._playerList = playerList.Cast<MonoBehaviour>().ToList();
        }

        #endregion
    }

    internal interface ITeamSnapshotTracker
    {
        float GetSqrDistance { get; }

        public void StopTracking();
    }

    internal class TeamSnapshotTracker : ITeamSnapshotTracker
    {
        public float GetSqrDistance => _sqrSqrDistance;

        private readonly int _teamNumber;
        private readonly Transform _player1;
        private readonly Transform _player2;

        private bool _isStopped;
        private float _sqrSqrDistance;
        private float _prevSqrSqrDistance;

        public TeamSnapshotTracker(BattleTeam battleTeam)
        {
            _teamNumber = battleTeam.TeamNumber;
            var firstPlayer = battleTeam.FirstPlayer;
            _player1 = ((MonoBehaviour)firstPlayer).GetComponent<Transform>();
            var secondPlayer = battleTeam.SecondPlayer;
            if (secondPlayer != null)
            {
                _player2 = ((MonoBehaviour)secondPlayer).GetComponent<Transform>();
            }
            else
            {
                var playArea = Context.GetPlayerPlayArea;
                switch (battleTeam.TeamNumber)
                {
                    case PhotonBattle.TeamBlueValue:
                        _player2 = playArea.BlueTeamMiddlePosition;
                        break;
                    case PhotonBattle.TeamRedValue:
                        _player2 = playArea.RedTeamMiddlePosition;
                        break;
                    default:
                        _player2 = _player1;
                        break;
                }
            }
            Debug.Log($"team {_teamNumber} p1 {_player1.position} p2 {_player2.position}");
        }

        public void StopTracking()
        {
            _isStopped = true;
            CalculateDistance();
            Debug.Log($"team {_teamNumber} distance {_sqrSqrDistance:0.00}");
        }

        private void CalculateDistance()
        {
            _sqrSqrDistance = Mathf.Abs((_player1.position - _player2.position).sqrMagnitude);
        }

        public IEnumerator TrackTheTeam()
        {
            var delay = new WaitForSeconds(0.1f);
            const float debugInterval = 0.5f;
            var debugLogTIme = Time.time + debugInterval;
            _sqrSqrDistance = 0;
            _prevSqrSqrDistance = _sqrSqrDistance;
            while (!_isStopped)
            {
                yield return delay;
                if (_isStopped)
                {
                    yield break;
                }
                CalculateDistance();
                if (Time.time > debugLogTIme && _prevSqrSqrDistance != _sqrSqrDistance)
                {
                    debugLogTIme = Time.time + debugInterval;
                    _prevSqrSqrDistance = _sqrSqrDistance;
                    Debug.Log($"team {_teamNumber} distance {_sqrSqrDistance:0.00}");
                }
            }
        }
    }

    internal class TeamColliderPlayModeTrigger : MonoBehaviour
    {
        public List<IPlayerDriver> TeamMembers { get; set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            foreach (var playerDriver in TeamMembers)
            {
                playerDriver.SetPlayMode(BattlePlayMode.Frozen);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            foreach (var playerDriver in TeamMembers)
            {
                playerDriver.SetPlayMode(BattlePlayMode.Normal);
            }
        }
    }
}