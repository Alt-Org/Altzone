using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Ball;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Manager for gameplay activities where one or more players are involved.<br />
    /// Requires player's registration in order to participate.
    /// </summary>
    internal interface IGameplayManager
    {
        int PlayerCount { get; }

        IPlayerDriver LocalPlayer { get; }

        void ForEach(Action<IPlayerDriver> action);

        BattleTeam GetBattleTeam(int teamNumber);

        BattleTeam GetOppositeTeam(int teamNumber);

        ITeamSnapshotTracker GetTeamSnapshotTracker(int teamNumber);

        IPlayerDriver GetPlayerByActorNumber(int actorNumber);

        void RegisterPlayer(IPlayerDriver playerDriver);

        void UpdatePeerCount(IPlayerDriver playerDriver);

        void UnregisterPlayer(IPlayerDriver playerDriver, GameObject playerInstanceRoot);
    }

    internal class BattleTeam
    {
        public readonly int TeamNumber;
        public readonly IPlayerDriver FirstPlayer;
        public readonly IPlayerDriver SecondPlayer;

        public readonly int PlayerCount;

        public BattleTeam(int teamNumber, IPlayerDriver firstPlayer, IPlayerDriver secondPlayer)
        {
            Assert.IsNotNull(firstPlayer, "firstPlayer != null");
            TeamNumber = teamNumber;
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
            PlayerCount = SecondPlayer == null ? 1 : 2;
        }

        public int Attack => FirstPlayer.CharacterModel.Attack + (SecondPlayer?.CharacterModel.Attack ?? 0);

        public void SetPlayMode(BattlePlayMode playMode)
        {
            FirstPlayer.SetPlayMode(playMode);
            SecondPlayer?.SetPlayMode(playMode);
        }

        public Transform SafeTransform(IPlayerDriver player)
        {
            Assert.IsTrue(player == FirstPlayer || player == SecondPlayer, "player == FirstPlayer || player == SecondPlayer");
            if (player != null)
            {
                return player.PlayerTransform;
            }
            var playArea = Context.GetBattlePlayArea;
            switch (TeamNumber)
            {
                case PhotonBattle.TeamBlueValue:
                    return playArea.BlueTeamMiddlePosition;
                case PhotonBattle.TeamRedValue:
                    return playArea.RedTeamMiddlePosition;
                default:
                    return null;
            }
        }

        public override string ToString()
        {
            if (SecondPlayer == null)
            {
                return $"Team:{TeamNumber}[{FirstPlayer.PlayerPos}]";
            }
            return $"Team:{TeamNumber}[{FirstPlayer.PlayerPos}+{SecondPlayer.PlayerPos}]";
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

    internal class TeamsAreReadyForGameplay
    {
        public readonly BattleTeam TeamBlue;
        public readonly BattleTeam TeamRed;

        public TeamsAreReadyForGameplay(BattleTeam teamBlue, BattleTeam teamRed)
        {
            TeamBlue = teamBlue;
            TeamRed = teamRed;
        }
    }

    internal class GameplayManager : MonoBehaviour, IGameplayManager
    {
        [Serializable]
        internal class DebugSettings
        {
            public List<PlayerDriver> _playerList;
            public List<GameObject> _abandonedPlayerList;
        }

        public static IGameplayManager Get() => FindObjectOfType<GameplayManager>();

        [SerializeField] private DebugSettings _debug;

        private readonly HashSet<IPlayerDriver> _players = new();
        private readonly Dictionary<int, GameObject> _abandonedPlayersByPlayerPos = new();
        private readonly List<IPlayerDriver> _teamBlue = new();
        private readonly List<IPlayerDriver> _teamRed = new();

        private TeamColliderPlayModeTrigger _teamBluePlayModeTrigger;
        private TeamColliderPlayModeTrigger _teamRedPlayModeTrigger;
        private bool _isApplicationQuitting;

        private void Awake()
        {
            Debug.Log($"{name}");
            var features = RuntimeGameConfig.Get().Features;
            if (features._isDisablePlayModeChanges)
            {
                return;
            }
            Application.quitting += () => _isApplicationQuitting = true;
            var playArea = Context.GetBattlePlayArea;
            _teamBluePlayModeTrigger = playArea.BlueTeamCollider.gameObject.AddComponent<TeamColliderPlayModeTrigger>();
            _teamBluePlayModeTrigger.TeamMembers = _teamBlue;
            _teamRedPlayModeTrigger = playArea.RedTeamCollider.gameObject.AddComponent<TeamColliderPlayModeTrigger>();
            _teamRedPlayModeTrigger.TeamMembers = _teamRed;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private BattleTeam CreateBattleTeam(int teamNumber)
        {
            if (teamNumber == PhotonBattle.TeamBlueValue)
            {
                switch (_teamBlue.Count)
                {
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
            var team = CreateBattleTeam(teamNumber);
            if (team == null)
            {
                return new NullTeamSnapshotTracker(teamNumber);
            }
            var tracker = new TeamSnapshotTracker(team);
            StartCoroutine(tracker.TrackTheTeam());
            return tracker;
        }

        #region IGameplayManager

        int IGameplayManager.PlayerCount => _players.Count;

        IPlayerDriver IGameplayManager.LocalPlayer => _players.FirstOrDefault(x => x.IsLocal);

        void IGameplayManager.ForEach(Action<IPlayerDriver> action)
        {
            foreach (var playerDriver in _players)
            {
                action(playerDriver);
            }
        }

        BattleTeam IGameplayManager.GetBattleTeam(int teamNumber)
        {
            return CreateBattleTeam(teamNumber);
        }

        BattleTeam IGameplayManager.GetOppositeTeam(int teamNumber)
        {
            return CreateBattleTeam(PhotonBattle.GetOppositeTeamNumber(teamNumber));
        }

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
            Debug.Log($"add {playerDriver.NickName} pos {playerDriver.PlayerPos} actor {playerDriver.ActorNumber} " +
                      $"peers {playerDriver.PeerCount} local {playerDriver.IsLocal}");
            Assert.IsFalse(_players.Count > 0 && _players.Any(x => x.ActorNumber == playerDriver.ActorNumber),
                "_players.Count > 0 && _players.Any(x => x.ActorNumber == playerDriver.ActorNumber)");
            _players.Add(playerDriver);
            if (_abandonedPlayersByPlayerPos.TryGetValue(playerDriver.PlayerPos, out var deletePreviousInstance))
            {
                _abandonedPlayersByPlayerPos.Remove(playerDriver.PlayerPos);
                deletePreviousInstance.SetActive(false);
                Destroy(deletePreviousInstance);
            }

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
            var gameCamera = Context.GetBattleCamera;
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

        void IGameplayManager.UpdatePeerCount(IPlayerDriver playerDriver)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            var realPlayers = PhotonBattle.CountRealPlayers();
            Assert.IsTrue(realPlayers > 0, "realPlayers > 0");
            var roomPlayers = PhotonWrapper.GetRoomProperty(PhotonBattle.PlayerCountKey, int.MaxValue);
            if (realPlayers < roomPlayers)
            {
                Debug.Log($"WAIT more realPlayers {realPlayers} : roomPlayers {roomPlayers}");
                return;
            }
            var readyPeers = 0;
            foreach (var player in _players)
            {
                if (player.PeerCount == realPlayers)
                {
                    readyPeers += 1;
                }
            }
            var gameCanStart = realPlayers == readyPeers;
            Debug.Log($"readyPeers {readyPeers} realPlayers {realPlayers} roomPlayers {roomPlayers} gameCanStart {gameCanStart}");
            if (gameCanStart)
            {
                print("++ >>");
                var teamBlue = CreateBattleTeam(PhotonBattle.TeamBlueValue);
                var teamRed = CreateBattleTeam(PhotonBattle.TeamRedValue);
                var message = new TeamsAreReadyForGameplay(teamBlue, teamRed);
                Debug.Log($"TeamsAreReadyForGameplay {message.TeamBlue} vs {message.TeamRed?.ToString() ?? "null"}");
                this.ExecuteOnNextFrame(() => this.Publish(message));
            }
        }

        void IGameplayManager.UnregisterPlayer(IPlayerDriver playerDriver, GameObject playerInstanceRoot)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            Debug.Log($"remove {playerDriver.NickName} pos {playerDriver.PlayerPos} actor {playerDriver.ActorNumber}");
            _players.Remove(playerDriver);
            if (_abandonedPlayersByPlayerPos.TryGetValue(playerDriver.PlayerPos, out var deletePreviousInstance))
            {
                deletePreviousInstance.SetActive(false);
                Destroy(deletePreviousInstance);
            }
            _abandonedPlayersByPlayerPos[playerDriver.PlayerPos] = playerInstanceRoot;

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
            var gameBackground = Context.GetBattleBackground;
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
            _debug._playerList = playerList.Cast<PlayerDriver>().ToList();
            _debug._abandonedPlayerList = _abandonedPlayersByPlayerPos.Values.ToList();
        }

        #endregion
    }

    internal interface ITeamSnapshotTracker
    {
        float GetSqrDistance { get; }

        public void StopTracking();
    }

    internal class NullTeamSnapshotTracker : ITeamSnapshotTracker
    {
        private readonly int _teamNumber;

        public NullTeamSnapshotTracker(int teamNumber)
        {
            _teamNumber = teamNumber;
        }

        public float GetSqrDistance => 0;

        public void StopTracking()
        {
            Debug.Log($"team {_teamNumber} sqr distance {GetSqrDistance:0.00}");
        }
    }

    internal class TeamSnapshotTracker : ITeamSnapshotTracker
    {
        public float GetSqrDistance => Mathf.Abs(_sqrSqrDistance);

        private readonly int _teamNumber;
        private readonly Transform _player1;
        private readonly Transform _player2;

        private bool _isStopped;
        private float _sqrSqrDistance;
        private float _prevSqrSqrDistance;

        public TeamSnapshotTracker(BattleTeam battleTeam)
        {
            _teamNumber = battleTeam.TeamNumber;
            _player1 = battleTeam.SafeTransform(battleTeam.FirstPlayer);
            _player2 = battleTeam.SafeTransform(battleTeam.SecondPlayer);
            Debug.Log($"team {_teamNumber} p1 {_player1.position} p2 {_player2.position}");
        }

        public void StopTracking()
        {
            _isStopped = true;
            CalculateDistance();
            Debug.Log($"team {_teamNumber} sqr distance {_sqrSqrDistance:0.00}");
        }

        private void CalculateDistance()
        {
            _sqrSqrDistance = (_player1.position - _player2.position).sqrMagnitude;
        }

        public IEnumerator TrackTheTeam()
        {
            var delay = new WaitForSeconds(0.1f);
            const float debugInterval = 0.5f;
            var debugLogTime = Time.time + debugInterval;
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
                if (Time.time > debugLogTime && _prevSqrSqrDistance != _sqrSqrDistance)
                {
                    debugLogTime = Time.time + debugInterval;
                    _prevSqrSqrDistance = _sqrSqrDistance;
                    Debug.Log($"team {_teamNumber} sqr distance {_sqrSqrDistance:0.00}");
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