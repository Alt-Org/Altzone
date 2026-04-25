using System.Collections.Generic;
using Altzone.Scripts.Chat;
using Assets.Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ServerChatMessage;

public class UsersReactionData : MonoBehaviour
{
    private AvatarData UserAvatar;
    [SerializeField] private AvatarFaceLoader _avatar;
    private string _userID;
    private string _userName;
    [SerializeField] private TextMeshProUGUI _userNameText;
    // Start is called before the first frame update
    public void SetReactionInfo(AvatarData Avatar, string UserName, string userID)
    {
        if (Avatar != null) _avatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(Avatar));
        _userID = userID;
        _userName = UserName;
        _userNameText.text = UserName;
    }
}
