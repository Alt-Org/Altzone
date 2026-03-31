using System;
using System.Collections.Generic;
using Altzone.Scripts.Language;
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
    [SerializeField] private Button _returnToMainMenuButton;
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextLanguageSelectorCaller _clanMembers;
    [SerializeField] private Image _lockImage;
    [SerializeField] private Sprite _lockOpen;
    [SerializeField] private Transform _heartContainer;
    [SerializeField] private Transform _labelsField;
    [SerializeField] private GameObject _labelImagePrefab;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private TextMeshProUGUI _winsRankText;
    [SerializeField] private TextMeshProUGUI _activityRankText;
    public TextMeshProUGUI WinsRank => _winsRankText;
    public TextMeshProUGUI ActivityRank => _activityRankText;
    [SerializeField] private LanguageFlagImage _languageFlagImage;

    private ServerClan _clan;
    public ServerClan Clan { get => _clan; set { _clan = value; SetClanInfo(); } }

    private void SetClanInfo()
    {
        if (_clan != null)
        {
            ClanData clanData = new ClanData(_clan);

            _clanName.text = _clan.name;
            _clanMembers.SetText(SettingsCarrier.Instance.Language,new string[1] { _clan.playerCount + "/30" });
            // By default the lock image is locked
            if (_clan.isOpen)
            {
                _lockImage.sprite = _lockOpen;
            }
            ToggleJoinButton(_clan.isOpen);

            _clanHeart.SetOwnClanHeart = false;
            _clanHeart.SetOtherClanColors(clanData);

            _languageFlagImage.SetFlag(clanData.Language);

            // Get rankings
            StartCoroutine(ServerManager.Instance.GetClanLeaderboardFromServer((clanLeaderboard) =>
            {
                // Wins are not available yet
                _winsRankText.text = "0";

                // Activity
                clanLeaderboard.Sort((a, b) => a.Points.CompareTo(b.Points));

                int rankActivity = 1;

                foreach (ClanLeaderboard ranking in clanLeaderboard)
                {
                    if (ranking.Clan.Id.Equals(clanData.Id))
                    {
                        break;
                    }

                    rankActivity++;
                }

                _activityRankText.text = rankActivity.ToString();
            }));

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
            if (ServerManager.Instance.FirstJoin)
                _returnToMainMenuButton.onClick.Invoke();
            else
                _returnToMainClanViewButton.onClick.Invoke();
            ServerManager.Instance.RaiseClanChangedEvent();
        }));
    }
}
