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

public class FriendlistItem : MonoBehaviour

{
    [SerializeField] private GameObject _friendPanel;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoader;
    [SerializeField] private Image _onlineStatusIndicator;
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private Button _removefriendButton;
    [SerializeField] private Button _acceptFriendButton;
    [SerializeField] private Button _declineFriendButton;

    private bool _isOnline = true;
    private Action _onRemoveClick;
    
   
    public void Initialize(string name, AvatarVisualData avatarVisualData = null, ClanLogo clanLogo = null, bool isOnline = true, Action onRemoveClick = null, Action onAcceptClick = null, Action onDeclineClick = null)
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
        
        if (_removefriendButton != null) // Show button only for accepted friends
        {
            _removefriendButton.gameObject.SetActive(onRemoveClick != null);

            _removefriendButton.onClick.RemoveAllListeners();
            _removefriendButton.onClick.AddListener(() =>
            {
                onRemoveClick?.Invoke();
            });
        }
        if (_acceptFriendButton != null) // Show button only for pending requests
        {
            _acceptFriendButton.gameObject.SetActive(onAcceptClick != null);

            _acceptFriendButton.onClick.RemoveAllListeners();
            _acceptFriendButton.onClick.AddListener(() =>
            {
                onAcceptClick?.Invoke();
            });
        }
        if (_declineFriendButton != null) // Show button only for pending requests
        {
            _declineFriendButton.gameObject.SetActive(onDeclineClick != null);

            _declineFriendButton.onClick.RemoveAllListeners();
            _declineFriendButton.onClick.AddListener(() =>
            {
                onDeclineClick?.Invoke();
            });
        }

    }
    private void UpdateOnlineStatusIndicator()
    {
        _onlineStatusIndicator.color = _isOnline ? Color.green : Color.red;
    }

    public void SetOnlineStatus (bool isOnline)
    {
        _isOnline = isOnline;
        UpdateOnlineStatusIndicator();
    }
}

