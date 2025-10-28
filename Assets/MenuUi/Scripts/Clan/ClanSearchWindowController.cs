using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ClanSearchWindowController : AltMonoBehaviour
{
    [SerializeField] private GameObject _overlay;

    private void OnEnable()
    {
        PlayerData data = null;
        StartCoroutine(GetPlayerData(callback => data = callback));
        if (string.IsNullOrEmpty(data.ClanId))
        {
            _overlay.SetActive(true); //Change this to be false when we want to force players to join a clan.
        }
        else
        {
            _overlay.SetActive(true);
        }
    }
}
