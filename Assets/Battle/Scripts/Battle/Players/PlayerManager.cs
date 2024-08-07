using System.Collections.Generic;
using UnityEngine;

using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.GA;
using Prg.Scripts.Common.PubSub;

using Battle.Scripts.Test;

namespace Battle.Scripts.Battle.Players
{
    // this interface probably should be somewhere else
    // also maybe more properties/methods that are in all drivers should be added
    interface IDriver
    {
        public int PlayerPos { get; }
        public int TeamNumber { get; }

        public Transform ActorShieldTransform { get; }
        public Transform ActorCharacterTransform { get; }
        public Transform ActorSoulTransform { get; }
        public PlayerActor PlayerActor { get; }
        public bool MovementEnabled { get; set; }
    }

    #region Battle Team

    interface IReadOnlyBattleTeam
    {
        public int GetTeamNumber();
        public IReadOnlyList<IDriver> GetAllDrivers();
        public IReadOnlyList<PlayerDriverPhoton> GetPlayerDrivers();
        public IReadOnlyList<PlayerDriverStatic> GetBotDrivers();
    }

    internal class BattleTeam : IReadOnlyBattleTeam
    {
        public int TeamNumber;
        public List<IDriver> AllDrivers = new();
        public List<PlayerDriverPhoton> PlayerDrivers = new();
        public List<PlayerDriverStatic> BotDrivers = new();

        public BattleTeam(int teamNumber)
        {
            TeamNumber = teamNumber;
        }

        public int GetTeamNumber()
            { return TeamNumber; }

        public IReadOnlyList<IDriver> GetAllDrivers()
            { return AllDrivers; }


        public IReadOnlyList<PlayerDriverPhoton> GetPlayerDrivers()
            { return PlayerDrivers; }


        public IReadOnlyList<PlayerDriverStatic> GetBotDrivers()
            { return BotDrivers; }


        public void AddPlayer(PlayerDriverPhoton player)
        {
            PlayerDrivers.Add(player);
            AllDrivers.Add(player);
        }


        public void AddBot(PlayerDriverStatic bot)
        {
            BotDrivers.Add(bot);
            AllDrivers.Add(bot);
        }

    }

    #endregion Battle Team

    #region Message Classes
    internal class TeamsAreReadyForGameplay
    {
        public readonly IReadOnlyList<IDriver> AllDrivers;
        public readonly IReadOnlyBattleTeam TeamAlpha;
        public readonly IReadOnlyBattleTeam TeamBeta;
        public readonly PlayerDriverPhoton LocalPlayer;

        public TeamsAreReadyForGameplay(IReadOnlyList<IDriver> allDrivers, IReadOnlyBattleTeam teamAlphaDrivers, IReadOnlyBattleTeam teamBetaDrivers, PlayerDriverPhoton localPlayer)
        {
            AllDrivers = allDrivers;
            TeamAlpha = teamAlphaDrivers;
            TeamBeta = teamBetaDrivers;
            LocalPlayer = localPlayer;
        }
    }
    #endregion Message Classes

    internal class PlayerManager : MonoBehaviour
    {
        #region Public

        #region Public - Properties
        public int LastPlayerTeleportUpdateNumber => _lastPlayerTeleportUpdateNumber;
        #endregion Public - Properties

        #region Public - Methods

        public void RegisterPlayer(PlayerDriverPhoton playerDriver)
        {
            _allDrivers.Add(playerDriver);
            _allPlayerDrivers.Add(playerDriver);
            switch (playerDriver.TeamNumber)
            {
                case PhotonBattle.TeamAlphaValue:
                    _teamAlpha.AddPlayer(playerDriver);
                    Debug.Log(DEBUG_LOG_NAME + "Registered player to team alpha");
                    break;

                case PhotonBattle.TeamBetaValue:
                    _teamBeta.AddPlayer(playerDriver);
                    Debug.Log(DEBUG_LOG_NAME + "Registered player to team beta");
                    break;
            }
        }

        public void RegisterBot(PlayerDriverStatic botDriver)
        {
            _allDrivers.Add(botDriver);
            _allBotDrivers.Add(botDriver);
            switch (botDriver.TeamNumber)
            {
                case PhotonBattle.TeamAlphaValue:
                    _teamAlpha.AddBot(botDriver);
                    Debug.Log(DEBUG_LOG_NAME + "Registered bot to team alpha");
                    break;

                case PhotonBattle.TeamBetaValue:
                    _teamBeta.AddBot(botDriver);
                    Debug.Log(DEBUG_LOG_NAME + "Registered bot to team beta");
                    break;
            }
        }

        public void UpdatePeerCount()
        {
            int roomPlayerCount = PhotonBattle.GetPlayerCountForRoom();
            int realPlayerCount = PhotonBattle.CountRealPlayers();
            Debug.Log(string.Format(DEBUG_LOG_NAME + "Info (room player count: {0}, real player count: {1})", roomPlayerCount, realPlayerCount));
            if (realPlayerCount < roomPlayerCount) return;
            int readyPeers = 0;
            foreach (PlayerDriverPhoton player in _allPlayerDrivers)
            {
                if (player.PeerCount == realPlayerCount)
                {
                    readyPeers += 1;
                }
            }
            Debug.Log(string.Format(DEBUG_LOG_NAME + "Info (ready peers: {0})", readyPeers));

            if (readyPeers == realPlayerCount)
            {
                GetLocalDriver(); // Finds the local driver from _allDrivers and sets it in _localPlayer
                //AttachRangeIndicator(); // Attaches a range indicator to the ally of _localPlayer
                this.ExecuteOnNextFrame(() =>
                {
                    this.Publish(new TeamsAreReadyForGameplay(_allDrivers, _teamAlpha, _teamBeta, _localPlayer));
                });
            }
        }

        public void ReportMovement(int teleportUpdateNumber)
        {
            if (teleportUpdateNumber > _lastPlayerTeleportUpdateNumber)
            {
                _lastPlayerTeleportUpdateNumber = teleportUpdateNumber;
            }
        }

        public void SetPlayerMovementEnabled(bool value)
        {
            foreach (IDriver driver in _allDrivers)
            {
                driver.MovementEnabled = value;
            }
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Player movement set to {1}", _syncedFixedUpdateClock.UpdateCount, value));
        }

        public void AnalyticsReportPlayerCharacterSelection()
        {
            string name = CustomCharacter.GetCharacterClassAndName(_localPlayer.PlayerActor.CharacterID);
            GameAnalyticsManager.Instance.CharacterSelection(name);
        }

        public void AnalyticsReportPlayerCharacterWinOrLoss(int winningTeam)
        {
            string name = CustomCharacter.GetCharacterClassAndName(_localPlayer.PlayerActor.CharacterID);
            if (_localPlayer.TeamNumber == winningTeam) GameAnalyticsManager.Instance.CharacterWin(name);
            else GameAnalyticsManager.Instance.CharacterLoss(name);
        }

        #endregion Public - Methods

        #endregion Public

        #region Private

        #region Private - Fields

        // Driver Lists
        private readonly List<IDriver> _allDrivers = new();
        private readonly List<PlayerDriverPhoton> _allPlayerDrivers = new();
        private readonly List<PlayerDriverStatic> _allBotDrivers = new();

        // Teams
        private readonly BattleTeam _teamAlpha = new(PhotonBattle.TeamAlphaValue);
        private readonly BattleTeam _teamBeta = new(PhotonBattle.TeamBetaValue);

        private PlayerDriverPhoton _localPlayer;

        private int _lastPlayerTeleportUpdateNumber;

        #endregion Private - Fields

        #region DEBUG
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER MANAGER] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time
        #endregion DEBUG

        #region Private - Methods

        // debug
        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private void GetLocalDriver()
        {
            foreach (PlayerDriverPhoton driver in _allPlayerDrivers)
            {
                if (driver.IsLocal)
                {
                    _localPlayer = driver;
                    break;
                }
            }
        }

        /*
        private void AttachRangeIndicator()
        {
            try { Instantiate(_rangeIndicator, GetAlly(_localPlayer).transform); }
            catch { Debug.Log("Local player is missing an ally"); }
        }
        */

        /*
        private GameObject GetAlly(PlayerDriverPhoton selfDriver)
        {
            // Returns the ally GameObject of the drivers owner
            foreach (PlayerDriverPhoton driver in _allPlayerDrivers)
            {
                if (driver.TeamNumber == selfDriver.TeamNumber && driver.PlayerActor.gameObject != selfDriver.PlayerActor.gameObject)
                {
                    return driver.PlayerActor.gameObject;
                }
            }
            return null;
        }
        */

        #endregion Private - Methods

        #endregion Private
    }
}
