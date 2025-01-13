using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.GA;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle1.Scripts.Battle.Players
{
    // this interface probably should be somewhere else
    // also maybe more properties/methods that are in all drivers should be added
    interface IPlayerDriver
    {
        public string NickName { get; }
        public bool IsLocal { get; }
        public int ActorNumber { get; }

        public bool MovementEnabled { get; set; }

        public IReadOnlyBattlePlayer BattlePlayer { get; }
    }

    #region Message Classes
    internal class TeamsAreReadyForGameplay
    {
        public IReadOnlyList<IReadOnlyBattlePlayer> AllPlayers {  get; }
        public IReadOnlyList<IReadOnlyBattleTeam> Teams { get; }
        public IReadOnlyBattleTeam TeamAlpha => Teams[0];
        public IReadOnlyBattleTeam TeamBeta => Teams[1];
        public IReadOnlyBattlePlayer LocalPlayer { get; }

        public TeamsAreReadyForGameplay(IReadOnlyList<IReadOnlyBattlePlayer> allPlayers, IReadOnlyList<IReadOnlyBattleTeam> teams, IReadOnlyBattlePlayer localPlayer)
        {
            AllPlayers = allPlayers;
            Teams = teams;
            LocalPlayer = localPlayer;
        }

        public IReadOnlyBattleTeam GetTeam(BattleTeamNumber teamNumber)
        {
            return teamNumber switch
            {
                BattleTeamNumber.TeamAlpha => Teams[0],
                BattleTeamNumber.TeamBeta  => Teams[1],
                _ => null
            };
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

        public void RegisterPlayer(BattlePlayer battlePlayer, BattleTeamNumber teamNumber)
        {
            _allPlayers.Add(battlePlayer);
            BattleTeam battleTeam = GetTeam(teamNumber);
            battlePlayer.SetBattleTeam(battleTeam);
            battleTeam.Players.Add(battlePlayer);
        }

        public void UpdatePeerCount()
        {
            /*   int roomPlayerCount = PhotonBattle.GetPlayerCountForRoom();
               int realPlayerCount = PhotonBattle.CountRealPlayers();*/
            int roomPlayerCount = 2;
            int realPlayerCount = 2;
            _battleDebugLogger.LogInfo("Info (room player count: {0}, real player count: {1})", roomPlayerCount, realPlayerCount);
            if (realPlayerCount < roomPlayerCount) return;
            int readyPeers = 0;
            foreach (BattlePlayer player in _allPlayers)
            {
             /*   if (!player.IsBot && ((PlayerDriverPhoton)player.PlayerDriver).PeerCount == realPlayerCount)
                {
                    readyPeers += 1;
                }*/
            }
            _battleDebugLogger.LogInfo("Info (ready peers: {0})", readyPeers);

            if (readyPeers == realPlayerCount)
            {
                SetLocalPlayer();
                SetTeammates();

                this.ExecuteOnNextFrame(() =>
                {
                    this.Publish(new TeamsAreReadyForGameplay(_allPlayers, _battleTeams, _localPlayer));
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
            foreach (IReadOnlyBattlePlayer player in _allPlayers)
            {
                player.PlayerDriver.MovementEnabled = value;
            }
            _battleDebugLogger.LogInfo("Player movement set to {0}", value);
        }

        public void OnBattleEnd(BattleTeamNumber winningTeam)
        {
            bool win = _localPlayer.BattleTeam.TeamNumber == winningTeam;

            if (win)
            {
                DataStore dataStore = Storefront.Get();
                PlayerData playerData = null;
                ClanData clanData = null;
                dataStore.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => playerData = data);
                dataStore.GetClanData(playerData.ClanId, data => clanData = data);

                foreach (ClanMember member in clanData.Members) if (member.Id == playerData.Id) { member.AddWin(); break; }
            }

            AnalyticsReportPlayerCharacterWinOrLoss(win);
        }

        public void AnalyticsReportPlayerCharacterSelection()
        {
            string name = CustomCharacter.GetCharacterClassAndName(_localPlayer.BattleCharacter.CharacterID);
            GameAnalyticsManager.Instance.CharacterSelection(name);
        }

        #endregion Public - Methods

        #endregion Public

        #region Private

        #region Private - Constants
        private const int TeamAlphaIndex = 0;
        private const int TeamBetaIndex = 1;
        #endregion Private - Constants

        #region Private - Fields

        // Players
        private readonly List<BattlePlayer> _allPlayers = new();

        // Teams
        private readonly IReadOnlyList<BattleTeam> _battleTeams = new List<BattleTeam>
        {
            new(BattleTeamNumber.TeamAlpha),
            new (BattleTeamNumber.TeamBeta)
        };

        private IReadOnlyBattlePlayer _localPlayer;

        private int _lastPlayerTeleportUpdateNumber;

        #endregion Private - Fields

        #region DEBUG
        private BattleDebugLogger _battleDebugLogger;
        #endregion DEBUG

        #region Private - Methods

        private void Start()
        {
            BattleTeam teamAlpha = _battleTeams[TeamAlphaIndex];
            BattleTeam teamBeta = _battleTeams[TeamBetaIndex];
            teamAlpha.SetOtherTeam(teamBeta);
            teamBeta.SetOtherTeam(teamAlpha);

            // debug
            _battleDebugLogger = new(this);
        }

        private void SetLocalPlayer()
        {
            foreach (BattlePlayer player in _allPlayers)
            {
                if (!player.IsBot && player.PlayerDriver.IsLocal)
                {
                    _localPlayer = player;
                    break;
                }
            }
        }

        private void SetTeammates()
        {
            foreach (BattleTeam battleTeam in _battleTeams)
            {
                if (battleTeam.Players.Count >= 2)
                {
                    BattlePlayer player0 = battleTeam.Players[0];
                    BattlePlayer player1 = battleTeam.Players[1];
                    player0.SetTeammate(player1);
                    player1.SetTeammate(player0);
                }
            }
        }

        private BattleTeam GetTeam(BattleTeamNumber teamNumber)
        {
            return teamNumber switch
            {
                BattleTeamNumber.TeamAlpha => _battleTeams[TeamAlphaIndex],
                BattleTeamNumber.TeamBeta => _battleTeams[TeamBetaIndex],
                _ => null,
            };
        }

        private void AnalyticsReportPlayerCharacterWinOrLoss(bool win)
        {
            string name = CustomCharacter.GetCharacterClassAndName(_localPlayer.BattleCharacter.CharacterID);
            if (win) GameAnalyticsManager.Instance.CharacterWin(name);
            else GameAnalyticsManager.Instance.CharacterLoss(name);
        }

        #endregion Private - Methods

        #endregion Private
    }
}
