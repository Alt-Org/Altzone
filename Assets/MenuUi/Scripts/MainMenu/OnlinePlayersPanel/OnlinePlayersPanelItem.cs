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

    private bool _isOnline = true;
    private Action _onRemoveClick;


    public void Initialize(string name, AvatarVisualData avatarVisualData = null, ClanLogo clanLogo = null, bool isOnline = true, Action onRemoveClick = null)
    {
        _nameText.text = name;
        _isOnline = isOnline;
        _onRemoveClick = onRemoveClick;


        if (avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }

        if (clanLogo != null)
        {
            _clanHeart.SetHeartColors(clanLogo);
        }

        UpdateOnlineStatusIndicator();


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

