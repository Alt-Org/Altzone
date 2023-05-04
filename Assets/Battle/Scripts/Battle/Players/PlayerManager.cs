using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.Scripts.Battle.Players;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _rangeIndicator;

    [Header("Drivers")]
    [SerializeField] private List<PlayerDriverPhoton> _allDrivers;

    private const float PhotonWaitTime = 2.1f;

    private void Start()
    {
        StartCoroutine(SearchPlayers());
    }

    private IEnumerator SearchPlayers()
    {
        yield return new WaitForSeconds(PhotonWaitTime);

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("PlayerDriverPhoton");
        foreach(GameObject playerObject in playerObjects) { _allDrivers.Add(playerObject.GetComponent<PlayerDriverPhoton>()); }

        AttachRangeIndicator();
    }

    private void AttachRangeIndicator()
    {
        int selfTeamNumber = 404;
        GameObject self = null;

        foreach(PlayerDriverPhoton driver in _allDrivers)
        {
            // Figures who is the local player
            if (driver._photonView.Controller.IsLocal)
            {
                self = driver._playerActor.gameObject;
                selfTeamNumber = driver.TeamNumber;
                break;
            }
        }
        foreach(PlayerDriverPhoton driver in _allDrivers)
        {
            // Figures who is the ally of local player and applies the range indicator
            if (driver.TeamNumber == selfTeamNumber && driver._playerActor.gameObject == self)
            {
                Instantiate(_rangeIndicator, driver._playerActor.gameObject.transform);
                break;
            }
        }
    }
}
