using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ClanListing contains references to instantiated ClanListPrefab.
/// </summary>
public class ClanListing : MonoBehaviour
{
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _returnToMainClanViewButton;

    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private Image _lockImage;
    [SerializeField] private Transform _heartContainer;

    private ServerClan _clan;
    public ServerClan Clan { get => _clan; set { _clan = value; SetClanValues(); } }

    private void SetClanValues()
    {
        _clanName.text = _clan.name;
        _clanMembers.text = "Jäsenet: " + _clan.playerCount;
        _lockImage.enabled = !_clan.isOpen;
        ToggleJoinButton(_clan.isOpen);

        // Temp clan heart values until those can be get from server
        List<HeartPieceData> heartPieces = new();
        for (int j = 0; j < 50; j++) heartPieces.Add(new HeartPieceData(j, Color.red));
        SetHeartColors(heartPieces);
    }

    internal void ToggleJoinButton(bool value)
    {
        _joinButton.interactable = value;
    }

    public void SetHeartColors(List<HeartPieceData> heartPieces)
    {
        HeartPieceColorHandler[] _heartPieceHandlers = _heartContainer.GetComponentsInChildren<HeartPieceColorHandler>();

        int i = 0;
        foreach (HeartPieceColorHandler colorhandler in _heartPieceHandlers)
        {
            colorhandler.Initialize(heartPieces[i].pieceNumber, heartPieces[i].pieceColor);
            i++;
        }
    }

    public void JoinButtonPressed()
    {
        StartCoroutine(ServerManager.Instance.JoinClan(Clan, clan =>
        {
            if (clan == null)
            {
                return;
            }

            ServerManager.Instance.RaiseClanChangedEvent();
            _returnToMainClanViewButton.onClick.Invoke();
        }));
    }
}
