using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Window;
using UnityEngine;

public class ClanSearchWindowController : AltMonoBehaviour
{
    [SerializeField] private GameObject _overlay;
    [SerializeField] private OverlayPanelCheck _overlayCheck;

    private void OnEnable()
    {
        PlayerData data = null;
        StartCoroutine(GetPlayerData(callback => data = callback));
        if (string.IsNullOrWhiteSpace(data.ClanId))
        {
            _overlay.SetActive(false); //Change this to be false when we want to force players to join a clan.
            _overlayCheck.enabled = false;
        }
        else
        {
            _overlay.SetActive(true);
        }
    }
}
