using System;
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
    [field: SerializeField] public Button OpenProfileButton { get; private set; }
    [SerializeField] private Button _returnToMainClanViewButton;

    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private Image _lockImage;
    [SerializeField] private Transform _heartContainer;
    [SerializeField] private Transform _labelsField;
    [SerializeField] private GameObject _labelImagePrefab;
    [SerializeField] private ClanHeartColorSetter _clanHeart;

    private ServerClan _clan;
    public ServerClan Clan { get => _clan; set { _clan = value; SetClanInfo(); } }

    private void SetClanInfo()
    {
        if (_clan != null)
        {
            ClanData clanData = new ClanData(_clan);

            _clanName.text = _clan.name;
            _clanMembers.text = "Jäsenet: " + _clan.playerCount + "/25";
            _lockImage.enabled = !_clan.isOpen;
            ToggleJoinButton(_clan.isOpen);

            _clanHeart.SetOwnClanHeart = false;
            _clanHeart.SetOtherClanColors(clanData);

            foreach (Transform child in _labelsField) Destroy(child.gameObject);

            int i = 0;
            foreach (ClanValues value in clanData.Values)
            {
                if (i < 3)
                {
                    GameObject label = Instantiate(_labelImagePrefab, _labelsField);
                    ValueImageHandle imageHandler = label.GetComponent<ValueImageHandle>();
                    imageHandler.SetLabelInfo(value);

                    i++;
                }

            }
        }
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
