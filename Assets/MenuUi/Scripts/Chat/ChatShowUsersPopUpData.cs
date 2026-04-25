using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static MessageReactionsHandler;
using MenuUi.Scripts.AvatarEditor;
using System;
using Altzone.Scripts.Chat;
using Assets.Altzone.Scripts.Model.Poco.Player;
using static ServerChatMessage;

public class ChatShowUsersPopUpData : MonoBehaviour
{

    
    [SerializeField] private TextMeshProUGUI ReactionAmounText;
    [SerializeField] private GameObject _reactionField; //Turn this off if "ShowUsersPopUp" isnt active otherwise the reaction system dies
    [SerializeField] private VerticalLayoutGroup _userContent;
    [SerializeField] private Button[] _closeButtons; 
    [SerializeField] private GameObject ShowUsersPopUp;
    [SerializeField] private MessageObjectHandler _messageObjectHandler;
    [SerializeField] private GameObject _userData;
    private List<UsersReactionData> _usersReactionData = new();
    [SerializeField] private List<UserReactionInfo> _userInfo;



    [SerializeField] private GameObject ReactionObject;

    //[SerializeField] private TextLanguageSelectorCaller _reactionAmount;
    private void Start()
    {
        reactiontext();
        foreach (Button button in _closeButtons)
        {
            button.onClick.AddListener(ClosePopup);
        }
    }

    public void ClosePopup()
    {
        gameObject.transform.SetParent(ShowUsersPopUp.transform);
        gameObject.SetActive(false);

    }

    void OnEnable()
    {
        _reactionField.SetActive(true);
        reactiontext();
    }


    //Incane user leaves the chat
    private void OnDisable()
    {
        ClosePopup();
    }


    public void reactiontext()
    {
        int activeChildren = 0;
        foreach (Transform container in _reactionField.transform)
        {
            if (container.gameObject.activeInHierarchy)
            {
                activeChildren++;
            }

        }

        ReactionAmounText.text = $"{activeChildren} reaktiota";

        //_reactionAmount.SetText(SettingsCarrier.Instance.Language, new string[1] { activeChildren.ToString() });
    }

    public void UsersReaction(ChatMessage message, ServerReactions mood)
    {
        _userInfo.Add(new UserReactionInfo { _avatar = message.Avatar, _id = message.Id, _name = message.Username});

        GameObject newUserData = Instantiate(_userData, _userContent.transform);
        UsersReactionData userData = newUserData.GetComponent<UsersReactionData>();
        



        userData.SetReactionInfo(_userInfo[_userInfo.Count - 1]._avatar, _userInfo[_userInfo.Count - 1]._name, _userInfo[_userInfo.Count - 1]._id);

        _usersReactionData.Add(userData);

    }


}

[Serializable]
public class UserReactionInfo
{
    public AvatarData _avatar;
    public string _name;
    public string _id;

    //public Sprite Sprite;
    //public Mood Mood;

}
