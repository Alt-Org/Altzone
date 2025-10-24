using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using System;

public class FriendlistItem : MonoBehaviour

{
    [SerializeField] private GameObject _friendPanel;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private Image _onlineStatusIndicator;
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;
    [SerializeField] private Image _clanLogo;
    [SerializeField] private Button _removefriendButton;

    private Action _onRemoveClick;

    void Start()
    {

    }
    //Katso vinkint avatariin ja logoon leaderboardista

    /* public void Initialize(PlayerData playerData, AvatarVisualData avatarVisualData = null, ClanData clanData = null, bool isOnline = false, Action onRemoveClick)
   {
        _nameText.text = playerData.name;
        _onRemoveClick = onRemoveClick;
      
        if (avatarVisualData != null)
        {
            _avatarImage.GetComponent<AvatarFaceLoader>()?.UpdateVisuals(avatarVisualData);
        }

        if (clanLogo != null)
        {
         _clanLogo.sprite = clanLogo;
        }

        _onlineStatusIndicator.color = isOnline ? Color.green : Color.red;
    
        _removefriendButton.onClick.RemoveAllListeners();
        _removefriendButton.onClick.AddListener(() =>
        {
            _onRemoveClick?.Invoke();
        });

    }*/
}

