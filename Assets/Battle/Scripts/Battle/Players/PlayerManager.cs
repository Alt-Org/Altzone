using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle.Players;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _rangeIndicator;

    [Header("Drivers")]
    [SerializeField] private List<PlayerDriverPhoton> _allDrivers;
    private PlayerDriverPhoton _localPlayer;

    private const float PhotonWaitTime = 2.1f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(PhotonWaitTime);

        SearchPlayers(); // Finds all drivers and puts them in _allDrivers
        GetLocalDriver(); // Finds the local driver from _allDrivers and sets it in _localPlayer
        AttachRangeIndicator(); // Attaches a range indicator to the ally of _localPlayer
    }

    private void SearchPlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("PlayerDriverPhoton");
        foreach(GameObject playerObject in playerObjects) { _allDrivers.Add(playerObject.GetComponent<PlayerDriverPhoton>()); }
    }
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
}
