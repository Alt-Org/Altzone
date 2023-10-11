using System.Collections.Generic;
using System.Linq;
using Battle.Scripts.Battle.Players;
using Battle.Scripts.Battle;
using UnityEngine;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Battle.Scripts.Test;


// this interface probably should be somewhere else
// also maybe more properties/methods that are in all drivers should be added
interface IDriver
{
    public int TeamNumber { get; }

    public Transform ActorTransform { get; }
}


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

internal class PlayerManager : MonoBehaviour
{
    /*
    [Header("Prefabs")]
    [SerializeField] private GameObject _rangeIndicator;
    */

    private List<IDriver> _allDrivers = new();
    private List<PlayerDriverPhoton> _allPlayerDrivers = new();
    private List<PlayerDriverStatic> _allBotDrivers = new();
    private BattleTeam _teamAlpha = new(PhotonBattle.TeamAlphaValue);
    private BattleTeam _teamBeta = new(PhotonBattle.TeamBetaValue);
    private PlayerDriverPhoton _localPlayer;

    public void RegisterPlayer(PlayerDriverPhoton playerDriver)
    {
        _allDrivers.Add(playerDriver);
        _allPlayerDrivers.Add(playerDriver);
        switch (playerDriver.TeamNumber)
        {
            case PhotonBattle.TeamAlphaValue:
                _teamAlpha.AddPlayer(playerDriver);
                break;

            case PhotonBattle.TeamBetaValue:
                _teamBeta.AddPlayer(playerDriver);
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
                break;

            case PhotonBattle.TeamBetaValue:
                _teamBeta.AddBot(botDriver);
                break;
        }
    }
 

    public void UpdatePeerCount()
    {
        var roomPlayers = PhotonNetwork.CurrentRoom.Players.Values;
        int roomPlayerCount = roomPlayers.Count();
        int realPlayerCount = roomPlayers.Sum(x => PhotonBattle.IsRealPlayer(x) ? 1 : 0);
        if (realPlayerCount < roomPlayerCount) return;
        int readyPeers = 0;
        foreach (PlayerDriverPhoton player in _allPlayerDrivers)
        {
            if (player.PeerCount == realPlayerCount)
            {
                readyPeers += 1;
            }
        }

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

    private void GetLocalDriver()
    {
        foreach (PlayerDriverPhoton driver in _allPlayerDrivers)
        {
            if (driver._photonView.Controller.IsLocal)
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

    private GameObject GetAlly(PlayerDriverPhoton selfDriver)
    {
        // Returns the ally GameObject of the drivers owner
        foreach (PlayerDriverPhoton driver in _allPlayerDrivers)
        {
            if (driver.TeamNumber == selfDriver.TeamNumber && driver._playerActor.gameObject != selfDriver._playerActor.gameObject)
            {
                return driver._playerActor.gameObject;
            }
        }
        return null;
    }
}
