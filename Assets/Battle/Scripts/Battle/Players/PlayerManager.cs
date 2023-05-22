using System.Collections.Generic;
using System.Linq;
using Battle.Scripts.Battle.Players;
using Battle.Scripts.Battle;
using UnityEngine;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;

internal class TeamsAreReadyForGameplay
{ }

internal class PlayerManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _rangeIndicator;

    [Header("Drivers")]
    [SerializeField] private List<PlayerDriverPhoton> _allDrivers;
    private PlayerDriverPhoton _localPlayer;

    /*
    private const float PhotonWaitTime = 2.1f;
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => !FindObjectOfType<PhotonPlayerInstantiate>().enabled);

        SearchPlayers(); // Finds all drivers and puts them in _allDrivers
    }
    */

    public void RegisterPlayer(PlayerDriverPhoton playerDriver)
    {
        _allDrivers.Add(playerDriver);
    }

    public void UpdatePeerCount()
    {
        var roomPlayers = PhotonNetwork.CurrentRoom.Players.Values;
        int roomPlayerCount = roomPlayers.Count();
        int realPlayerCount = roomPlayers.Sum(x => PhotonBattle.IsRealPlayer(x) ? 1 : 0);
        if (realPlayerCount < roomPlayerCount) return;
        int readyPeers = 0;
        foreach (PlayerDriverPhoton player in _allDrivers)
        {
            if (player.PeerCount == realPlayerCount)
            {
                readyPeers += 1;
            }
        }

        if (readyPeers == realPlayerCount)
        {
            GetLocalDriver(); // Finds the local driver from _allDrivers and sets it in _localPlayer
            AttachRangeIndicator(); // Attaches a range indicator to the ally of _localPlayer
            this.ExecuteOnNextFrame(() => this.Publish(new TeamsAreReadyForGameplay()));
        }
    }

    /*
    private void SearchPlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("PlayerDriverPhoton");
        foreach(GameObject playerObject in playerObjects) { _allDrivers.Add(playerObject.GetComponent<PlayerDriverPhoton>()); }
    }
     */

    private void GetLocalDriver()
    {
        foreach (PlayerDriverPhoton driver in _allDrivers)
        {
            if (driver._photonView.Controller.IsLocal)
            {
                _localPlayer = driver;
                break;
            }
        }
    }

    private void AttachRangeIndicator()
    {
        try { Instantiate(_rangeIndicator, GetAlly(_localPlayer).transform); }
        catch { Debug.Log("Local player is missing an ally"); }
    }

    private GameObject GetAlly(PlayerDriverPhoton selfDriver)
    {
        // Returns the ally GameObject of the drivers owner
        foreach (PlayerDriverPhoton driver in _allDrivers)
        {
            if (driver.TeamNumber == selfDriver.TeamNumber && driver._playerActor.gameObject != selfDriver._playerActor.gameObject)
            {
                return driver._playerActor.gameObject;
            }
        }
        return null;
    }

    /*
    public bool AllExist()
    {
        if (_allDrivers.Count == 4) { return true; }
        return false;
    }
    */
}
