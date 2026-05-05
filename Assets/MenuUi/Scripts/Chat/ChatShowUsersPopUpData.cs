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
using System.Linq;
using NUnit.Framework.Internal.Commands;

public class ChatShowUsersPopUpData : MonoBehaviour
{

    
    [SerializeField] private TextMeshProUGUI ReactionAmounText;
    [SerializeField] private Button[] _closeButtons; 
    [SerializeField] private GameObject ShowUsersPopUp;
    [SerializeField] private MessageObjectHandler _messageObjectHandler;

    [Header("User Info")]

    [SerializeField] private VerticalLayoutGroup _userContent;
    [SerializeField] private GameObject _userData;
    private List<UsersReactionData> _usersReactionData = new();
    [SerializeField] private List<UserReactionInfo> _userInfo;

    [Header("Reactions")]
    [SerializeField] private GameObject _reactionField; //Turn this off if "ShowUsersPopUp" isnt active otherwise the reaction system dies
    [SerializeField] private List<ReactionObject> _reactionList;



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
        foreach (Transform container in _userContent.transform)
        {
            if (container.gameObject.activeInHierarchy)
            {
                activeChildren++;
            }

        }

        ReactionAmounText.text = $"{activeChildren} reaktiota";

        //_reactionAmount.SetText(SettingsCarrier.Instance.Language, new string[1] { activeChildren.ToString() });
    }

    public void AddUsersReaction(ChatMessage message, ServerReactions Emoji)
    {

            Mood mood = (Mood)Enum.Parse(typeof(Mood), Emoji.emoji);
           
        //halts the system if there's already a same user on the list
        for (int i = _userInfo.Count - 1; i >= 0; i--)
        {
            if (_userInfo[i]._id != Emoji.sender_id)
                continue;


            if (_userInfo[i]._id == Emoji.sender_id &&  _userInfo[i]._mood != mood)
            {
                RemoveUserReaction(Emoji.sender_id);
                continue;
            }
            else if (_userInfo[i]._id == Emoji.sender_id && _userInfo[i]._mood == mood)
            {
                
                return;
            }
        }
            //Gets sprite
        Sprite reactionSprite = _reactionList.FirstOrDefault(x => x.Mood == mood)?.Sprite;

        //Sets up the Data
        GameObject newUserData = Instantiate(_userData, _userContent.transform);
        _userInfo.Add(new UserReactionInfo { _avatar = message.Avatar, _id = Emoji.sender_id, _name = Emoji.playerName, Emoji = reactionSprite, UserDataObj = newUserData, _mood = mood});
        UsersReactionData userData = newUserData.GetComponent<UsersReactionData>();
       userData.SetReactionInfo(_userInfo[_userInfo.Count - 1]._avatar, _userInfo[_userInfo.Count - 1]._name, _userInfo[_userInfo.Count - 1]._id, _userInfo[_userInfo.Count - 1].Emoji);
        _usersReactionData.Add(userData);
        reactiontext();


    }

    //removes the userData
    public void RemoveUserReaction(string Userid)
    {


        for (int i = _userInfo.Count - 1; i >= 0; i--)
            {

                if (_userInfo[i]._id == Userid)
                {
                _userInfo[i].UserDataObj.transform.SetParent(null);
                Destroy(_userInfo[i].UserDataObj);
                _userInfo.RemoveAt(i);
                }


            }
                reactiontext();
            
    }
}

[Serializable]
public class UserReactionInfo
{
    public GameObject UserDataObj;
    public AvatarData _avatar;
    public string _name;
    public string _id;
    public Mood _mood;
    public Sprite Emoji;

    //public Sprite Sprite;
    //public Mood Mood;

}
