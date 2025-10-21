using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;

public class FriendlistItem : MonoBehaviour

{
    [SerializeField] private GameObject _friendPanel;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private Image _onlineStatusIndicator;
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;
    [SerializeField] private Image _clanLogo;
    [SerializeField] private Button _removefriendButton;


    void Start()
    {

    }
    //Katso vinkint avatariin ja logoon leaderboardista

    /* public void Initialize(PlayerData playerData, AvatarVisualData avatarVisualData, ClanData clanData = null, bool isOnline = false, System.Action onRemoveClick)
   {
        _nameText.text = playerData.name;
      
        if (avatarVisualData != null)
        {
            _avatarImage.GetComponent<AvatarFaceLoader>()?.UpdateVisuals(avatarVisualData);
        }

        if (clanLogo != null)
        {
         _clanLogo.sprite = clanLogo;
        }

        _onlineStatusIndicator.color = isOnline ? Color.green : Color.red;
    
        _removefriendButton.onClick.RemoveAllListeners(); //poistetaan kuuntelijat
        _removefriendButton.onClick.AddListener(() => onClickAction()); //asetetaan uusi toiminto
    }*/
}

