using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using System;
using MenuUi.Scripts.AvatarEditor;

public class OnlinePlayersPanelItem : MonoBehaviour

{
    [SerializeField] private GameObject _onlinePlayersPanel;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoader;
    [SerializeField] private Image _onlineStatusIndicator;
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private Button _addfriendButton;
    [SerializeField] private TextMeshProUGUI _addFriendButtonText; 

    private bool _isOnline = true;
    private Action _onRemoveClick;


    public void Initialize(string name, AvatarVisualData avatarVisualData = null, ClanLogo clanLogo = null, bool isOnline = true, Action onRemoveClick = null, bool hideClanLogo = false, bool isFriend = false, bool alreadyRequested = false, Action onAddFriendClick = null)
    {
        _nameText.text = name;
        _isOnline = isOnline;
        _onRemoveClick = onRemoveClick;


        if (avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }

        if (clanLogo != null && !hideClanLogo)
        {
            _clanHeart.SetHeartColors(clanLogo);
            _clanHeart.gameObject.SetActive(true);
        }
        else
        {
            _clanHeart.gameObject.SetActive(false);
        }

        UpdateOnlineStatusIndicator();

        if (_addfriendButton != null)
        {
            _addfriendButton.gameObject.SetActive(true);
            _addfriendButton.onClick.RemoveAllListeners();

            if (isFriend)
            {
                _addfriendButton.interactable = false;
                _addFriendButtonText.text = "Kaveri";
            }
            else if (alreadyRequested)
            {
                _addfriendButton.interactable = false;
                _addFriendButtonText.text = "Kaveriyyntö lähetetty";
            }
            else
            {
                _addfriendButton.interactable = true;
                _addFriendButtonText.text = "Lisää kaveriksi";

                _addfriendButton.onClick.AddListener(() =>
                {
                    onAddFriendClick?.Invoke();
                });
            }
        }
    }
    private void UpdateOnlineStatusIndicator()
    {
        _onlineStatusIndicator.color = _isOnline ? Color.green : Color.red;
    }

    public void SetOnlineStatus(bool isOnline)
    {
        _isOnline = isOnline;
        UpdateOnlineStatusIndicator();
    }
}

