using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Players;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using UnityConstants;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Container for team members, first player has always lower player position. Second player can be null.
    /// </summary>
    internal class BattleTeam : IBattleTeam
    {
        public int TeamNumber { get; }
        public IPlayerDriver FirstPlayer { get; }
        public IPlayerDriver SecondPlayer { get; }
        public int PlayerCount { get; }

        public BattleTeam(int teamNumber, IPlayerDriver firstPlayer, IPlayerDriver secondPlayer)
        {
            Assert.IsNotNull(firstPlayer, "firstPlayer != null");
            TeamNumber = teamNumber;
            if (secondPlayer != null && secondPlayer.PlayerPos < firstPlayer.PlayerPos)
            {
                FirstPlayer = secondPlayer;
                SecondPlayer = firstPlayer;
            }
            else
            {
                FirstPlayer = firstPlayer;
                SecondPlayer = secondPlayer;
            }
            PlayerCount = SecondPlayer == null ? 1 : 2;
        }

        public IPlayerDriver GetMyTeamMember(int actorNumber)
        {
            Assert.IsTrue(PlayerCount == 2, "PlayerCount == 2");
            if (FirstPlayer.ActorNumber == actorNumber)
            {
                return SecondPlayer;
            }
            if (SecondPlayer.ActorNumber == actorNumber)
            {
                return FirstPlayer;
            }
            return null;
        }

        public int Attack => FirstPlayer.CharacterModel.Attack + (SecondPlayer?.CharacterModel.Attack ?? 0);

        public void SetPlayMode(BattlePlayMode playMode)
        {
            FirstPlayer.SetPlayMode(playMode);
            SecondPlayer?.SetPlayMode(playMode);
        }

        public Transform SafePlayerTransform(IPlayerDriver player)
        {
            // Note that SafePlayerTransform is not published via our public interface because currently only TeamSlingshotTracker needs this. 
            if (player != null)
            {
                Assert.IsTrue(player.ActorNumber == FirstPlayer.ActorNumber || player.ActorNumber == SecondPlayer.ActorNumber,
                    "player == FirstPlayer || player == SecondPlayer");
                return player.PlayerTransform;
            }
            var playArea = Context.GetBattlePlayArea;
            switch (TeamNumber)
            {
                case PhotonBattle.TeamBlueValue:
                    return playArea.BlueTeamTransform;
                case PhotonBattle.TeamRedValue:
                    return playArea.RedTeamTransform;
                default:
                    throw new UnityException($"invalid team number {TeamNumber}");
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

    #region Gameplay events

    internal class PlayerJoined
    {
        public readonly IPlayerDriver Player;

        public PlayerJoined(IPlayerDriver player)
        {
            Player = player;
        }

        public override string ToString()
        {
            return $"{Player}";
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

        public TeamCreated(BattleTeam battleTeam)
        {
            BattleTeam = battleTeam;
        }

        public override string ToString()
        {
            return $"{BattleTeam}";
        }
    }

    internal class TeamUpdated : TeamCreated
    {
        public TeamUpdated(BattleTeam battleTeam) : base(battleTeam)
        {
        }
    }

    internal class TeamBroken : TeamCreated
    {
        public readonly IPlayerDriver PlayerWhoLeft;

        public TeamBroken(BattleTeam battleTeam, IPlayerDriver playerWhoLeft) : base(battleTeam)
        {
            PlayerWhoLeft = playerWhoLeft;
        }

        public override string ToString()
        {
            return $"{base.ToString()} left {PlayerWhoLeft}";
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

    #endregion

    internal class PlayerManager : MonoBehaviour, IPlayerManager
    {
        [Serializable]
        internal class DebugSettings
        {
            public List<PlayerDriver> _playerList;
            public List<GameObject> _abandonedPlayerList;
        }

        [SerializeField] private DebugSettings _debug;

        private readonly HashSet<IPlayerDriver> _players = new();
        private readonly Dictionary<int, GameObject> _abandonedPlayersByPlayerPos = new();
        private readonly List<IPlayerDriver> _teamBlue = new();
        private readonly List<IPlayerDriver> _teamRed = new();

        private TeamColliderPlayAreaTracker _teamBluePlayAreaTracker;
        private TeamColliderPlayAreaTracker _teamRedPlayAreaTracker;

        private bool _isApplicationQuitting;
        private bool _isDisableTeamForfeit;

        private void Awake()
        {
            Debug.Log($"{name}");
            Application.quitting += () => _isApplicationQuitting = true;
            var features = RuntimeGameConfig.Get().Features;
            if (features._isDisablePlayModeChanges)
            {
                return;
            }
            _isDisableTeamForfeit = features._isDisableTeamForfeit;
            SetPlayAreaTracking();
        }

        private void SetPlayAreaTracking()
        {
            var playArea = Context.GetBattlePlayArea;
            var blueCollider = playArea.BlueTeamCollider;
            var redCollider = playArea.RedTeamCollider;
            Assert.IsTrue(blueCollider.isTrigger, "blueCollider.isTrigger");
            Assert.IsTrue(redCollider.isTrigger, "redCollider.isTrigger");

            _teamBluePlayAreaTracker = blueCollider.gameObject.AddComponent<TeamColliderPlayAreaTracker>();
            _teamBluePlayAreaTracker.TeamMembers = _teamBlue;
            _teamRedPlayAreaTracker = redCollider.gameObject.AddComponent<TeamColliderPlayAreaTracker>();
            _teamRedPlayAreaTracker.TeamMembers = _teamRed;
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

        private ITeamSlingshotTracker GetTeamSnapshotTracker(int teamNumber)
        {
            var team = CreateBattleTeam(teamNumber);
            return team != null ? new TeamSlingshotTracker(team) : new NullTeamSlingshotTracker(teamNumber);
        }

        #region IGameplayManager

        int IPlayerManager.PlayerCount => _players.Count;

        IPlayerDriver IPlayerManager.LocalPlayer => _players.FirstOrDefault(x => x.IsLocal);

        void IPlayerManager.ForEach(Action<IPlayerDriver> action)
        {
            foreach (var playerDriver in _players)
            {
                action(playerDriver);
            }
        }

        IBattleTeam IPlayerManager.GetBattleTeam(int teamNumber)
        {
            return CreateBattleTeam(teamNumber);
        }

        IBattleTeam IPlayerManager.GetOppositeTeam(int teamNumber)
        {
            return CreateBattleTeam(PhotonBattle.GetOppositeTeamNumber(teamNumber));
        }

        ITeamSlingshotTracker IPlayerManager.GetTeamSnapshotTracker(int teamNumber)
        {
            return GetTeamSnapshotTracker(teamNumber);
        }

        IPlayerDriver IPlayerManager.GetPlayerByActorNumber(int actorNumber)
        {
            return _players.FirstOrDefault(x => x.ActorNumber == actorNumber);
        }

        IPlayerDriver IPlayerManager.GetPlayerByLastBallHitTime(int teamNumber)
        {
            var team = CreateBattleTeam(teamNumber);
            if (team == null)
            {
                return null;
            }
            if (team.PlayerCount == 1)
            {
                return team.FirstPlayer;
            }
            return team.FirstPlayer.LastBallHitTime > team.SecondPlayer.LastBallHitTime ? team.FirstPlayer : team.SecondPlayer;
        }

        void IPlayerManager.RegisterPlayer(IPlayerDriver playerDriver)
        {
            Debug.Log($"add {playerDriver.NickName} pos {playerDriver.PlayerPos} actor {playerDriver.ActorNumber} " +
                      $"peers {playerDriver.PeerCount} local {playerDriver.IsLocal} players #{_players.Count}");
            Assert.IsFalse(_players.Count > 0 && _players.Any(x => x.ActorNumber == playerDriver.ActorNumber),
                "_players.Count > 0 && _players.Any(x => x.ActorNumber == playerDriver.ActorNumber)");

            // Update state
            _players.Add(playerDriver);
            if (_abandonedPlayersByPlayerPos.TryGetValue(playerDriver.PlayerPos, out var deletePreviousInstance))
            {
                _abandonedPlayersByPlayerPos.Remove(playerDriver.PlayerPos);
                deletePreviousInstance.SetActive(false);
                Destroy(deletePreviousInstance);
            }
            if (playerDriver.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                Assert.IsTrue(_teamBlue.Count < 2, "_teamBlue.Count < 2");
                _teamBlue.Add(playerDriver);
            }
            else if (playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                Assert.IsTrue(_teamRed.Count < 2, "_teamRed.Count < 2");
                _teamRed.Add(playerDriver);
            }
            else
            {
                throw new UnityException($"Invalid team number {playerDriver.TeamNumber}");
            }

            // Publish (after we have stable state): PlayerJoined first, then TeamCreated or TeamUpdated
            this.Publish(new PlayerJoined(playerDriver));
            if (playerDriver.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                this.Publish(_teamBlue.Count == 1
                    ? new TeamCreated(CreateBattleTeam(playerDriver.TeamNumber))
                    : new TeamUpdated(CreateBattleTeam(playerDriver.TeamNumber)));
            }
            else if (playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                this.Publish(_teamBlue.Count == 1
                    ? new TeamCreated(CreateBattleTeam(playerDriver.TeamNumber))
                    : new TeamUpdated(CreateBattleTeam(playerDriver.TeamNumber)));
            }
            else
            {
                throw new UnityException($"Invalid team number {playerDriver.TeamNumber}");
            }

            // This is related to how camera and game objects are rotated.
            // - there is not really good place for this stuff, so it is now here.
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
                Context.BallManager.FixCameraRotation(gameCamera.Camera);
            }
            UpdateDebugPlayerList();
        }

        void IPlayerManager.UpdatePeerCount(IPlayerDriver playerDriver)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            var realPlayers = PhotonBattle.CountRealPlayers();
            Assert.IsTrue(realPlayers > 0, "realPlayers > 0");
            var roomPlayers = PhotonBattle.GetPlayerCountForRoom();
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

        void IPlayerManager.UnregisterPlayer(IPlayerDriver playerDriver, GameObject playerInstanceRoot)
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            Debug.Log(
                $"remove {playerDriver.NickName} pos {playerDriver.PlayerPos} actor {playerDriver.ActorNumber} players #{_players.Count}");
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
                    this.Publish(new TeamBroken(CreateBattleTeam(playerDriver.TeamNumber), playerDriver));
                }
                else if (_teamBlue.Count == 0 && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    TeamForfeitAndGameOver(PhotonNetwork.CurrentRoom);
                    return;
                }
            }
            else if (playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _teamRed.Remove(playerDriver);
                if (_teamRed.Count == 1)
                {
                    this.Publish(new TeamBroken(CreateBattleTeam(playerDriver.TeamNumber), playerDriver));
                }
                else if (_teamRed.Count == 0 && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    TeamForfeitAndGameOver(PhotonNetwork.CurrentRoom);
                    return;
                }
            }
            this.Publish(new PlayerLeft(playerDriver));
            UpdateDebugPlayerList();
        }

        private void TeamForfeitAndGameOver(Room room)
        {
            if (_isDisableTeamForfeit)
            {
                return;
            }
            Assert.IsTrue(_teamBlue.Count == 0 || _teamRed.Count == 0, "_teamBlue.Count == 0 || _teamRed.Count == 0");
            Assert.IsTrue(_teamBlue.Count > 0 || _teamRed.Count > 0, "_teamBlue.Count > 0 || _teamRed.Count > 0");
            var gameScoreManager = Context.GetGameScoreManager;
            if (gameScoreManager == null)
            {
                return;
            }
            if (_teamBlue.Count == 0)
            {
                var score = gameScoreManager.RedScore;
                var scoreTotal = score.Item1 + score.Item2;
                gameScoreManager.ShowGameOverWindow(room, PhotonBattle.WinTypeResign, PhotonBattle.TeamRedValue, 0, scoreTotal);
            }
            else
            {
                var score = gameScoreManager.BlueScore;
                var scoreTotal = score.Item1 + score.Item2;
                gameScoreManager.ShowGameOverWindow(room, PhotonBattle.WinTypeResign, PhotonBattle.TeamBlueValue, scoreTotal, 0);
            }
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

    internal class NullTeamSlingshotTracker : ITeamSlingshotTracker
    {
        public int TeamNumber { get; }
        public int PlayerCount { get; }

        public NullTeamSlingshotTracker(int teamNumber)
        {
            TeamNumber = teamNumber;
            PlayerCount = 0;
        }

        public float SqrDistance => 0;

        public Transform Player1Transform => null;
        public Transform Player2Transform => null;

        public float StopTracking()
        {
            Debug.Log($"team {TeamNumber} sqr distance {SqrDistance:0.00}");
            return SqrDistance;
        }

        public override string ToString()
        {
            return $"Team:{TeamNumber}[#{PlayerCount}]";
        }
    }

    /// <summary>
    /// Helper to safely calculate distance between two players while gameplay is (re)starting.
    /// </summary>
    internal class TeamSlingshotTracker : ITeamSlingshotTracker
    {
        public int TeamNumber { get; }
        public int PlayerCount { get; }
        public float SqrDistance => CalculateDistance();

        public Transform Player1Transform { get; }
        public Transform Player2Transform { get; }

        private bool _isStopped;
        private float _sqrDistance;

        public TeamSlingshotTracker(BattleTeam battleTeam)
        {
            TeamNumber = battleTeam.TeamNumber;
            PlayerCount = battleTeam.PlayerCount;
            Player1Transform = battleTeam.SafePlayerTransform(battleTeam.FirstPlayer);
            Player2Transform = battleTeam.SafePlayerTransform(battleTeam.SecondPlayer);
            Debug.Log($"team {TeamNumber} p1 {Player1Transform.position} p2 {Player2Transform.position}");
        }

        public float StopTracking()
        {
            CalculateDistance();
            _isStopped = true;
            Debug.Log($"team {TeamNumber} sqr distance {SqrDistance:0.00}");
            return SqrDistance;
        }

        private float CalculateDistance()
        {
            if (!_isStopped)
            {
                _sqrDistance = Mathf.Abs((Player1Transform.position - Player2Transform.position).sqrMagnitude);
            }
            return _sqrDistance;
        }

        public override string ToString()
        {
            return $"Team:{TeamNumber}[#{PlayerCount}]";
        }
    }

    /// <summary>
    /// Helper to track ball movement inside team gameplay area.
    /// </summary>
    internal class TeamColliderPlayAreaTracker : MonoBehaviour
    {
        /// <summary>
        /// Hard reference to the list of team members we are tracking.
        /// </summary>
        public List<IPlayerDriver> TeamMembers { get; set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            if (!otherGameObject.CompareTag(Tags.Ball))
            {
                return;
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
            var otherGameObject = other.gameObject;
            if (!otherGameObject.CompareTag(Tags.Ball))
            {
                return;
            }
            foreach (var playerDriver in TeamMembers)
            {
                playerDriver.SetPlayMode(BattlePlayMode.Normal);
            }
        }
    }
}