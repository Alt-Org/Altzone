using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static MessageReactionsHandler;
using System;
using Altzone.Scripts.Chat;
using Assets.Altzone.Scripts.Model.Poco.Player;
using static ServerChatMessage;
using System.Linq;
using static ChatShowUsersPopUpData;

public class ChatShowUsersPopUpData : MonoBehaviour
{

    
    [SerializeField] private TextMeshProUGUI ReactionAmounText;
    [SerializeField] private Button[] _closeButtons;
    [SerializeField] private Button _closeAllReactionButton;
    [SerializeField] private Button _listOrderButton;
    [SerializeField] private GameObject ShowUsersPopUp;

    [Header("Scripts")]
    [SerializeField] private MessageObjectHandler _messageObjectHandler;
    [SerializeField] private MessageReactionsHandler _messageReactionsHandler;
    [Header("User Info")]

    [SerializeField] private VerticalLayoutGroup _userContent;
    [SerializeField] private GameObject _userData;
    private List<UsersReactionData> _usersReactionData = new();
    [SerializeField] private List<UserReactionInfo> _userInfo;

    [Header("Reactions")]
    public GameObject _reactionFieldNewLocation;
    [SerializeField] private GameObject _reactionFieldOldLocation;
    [SerializeField] private List<ReactionObject> _reactionList;
    [SerializeField] private GameObject _allReactions;
    [SerializeField] private GameObject _selectedReaction;


    [SerializeField] private GameObject ReactionObject;
    private int _listorder = 0;
    private int currentOrder = 1;


    //[SerializeField] private TextLanguageSelectorCaller _reactionAmount;
    private void Start()
    {
        _closeAllReactionButton.onClick.AddListener(LayoutButton);
        _listOrderButton.onClick.AddListener(() => currentOrder++);
        _listOrderButton.onClick.AddListener(() => ListOrder(currentOrder));
        reactiontext();
        foreach (Button button in _closeButtons)
        {
            button.onClick.AddListener(ClosePopup);
        }
    }

    //Puts back the reactions and popup back to messageobject
    public void ClosePopup()
    {
        _messageReactionsHandler.ReactionResize();

        RectTransform rt = _messageObjectHandler.ReactionsPanel.GetComponent<RectTransform>();
        gameObject.transform.SetParent(ShowUsersPopUp.transform);

        _messageObjectHandler.ReactionsPanel.transform.SetParent(_reactionFieldOldLocation.transform);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        gameObject.SetActive(false);

    }

    void OnEnable()
    {
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

        if(_userInfo.Count + 1 >= _listorder)
        {
        _listorder++;
        }
        GameObject newUserData = Instantiate(_userData, _userContent.transform);
        _userInfo.Add(new UserReactionInfo { _avatar = message.Avatar, _id = Emoji.sender_id, _name = Emoji.playerName, Emoji = reactionSprite, UserDataObj = newUserData, _mood = mood, _order = _listorder});
        UsersReactionData userData = newUserData.GetComponent<UsersReactionData>();
       userData.SetReactionInfo(_userInfo[_userInfo.Count - 1]._avatar, _userInfo[_userInfo.Count - 1]._name, _userInfo[_userInfo.Count - 1]._id, _userInfo[_userInfo.Count - 1].Emoji);
        _usersReactionData.Add(userData);
        reactiontext();
        ListOrder(currentOrder);

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


    //Puts down AllReaction selection if not pressing the reaction
    void LayoutButton()
    {
        if(_allReactions.activeSelf)
        {
        _allReactions.SetActive(false);
        _selectedReaction.SetActive(true);
        }
    }
    //Sets the List in certain order




    void ListOrder(int order)
    {
        if (order >= 6)
        {
            currentOrder = 1;
            order = currentOrder;
        }
        switch (order)
        {
            //List Order

            //Oldest => Newest
            case 1:
                Debug.LogWarning("FIND ME Order 1");
                _userInfo = _userInfo.OrderBy(m => m._order).ToList();
                break;

            //Newest => Oldest
            case 2:
                Debug.LogWarning("FIND ME Order 2");
                _userInfo = _userInfo.OrderByDescending(m => m._order).ToList();
                break;

            // A => Z By Username
            case 3:
                Debug.LogWarning("FIND ME Order 3");
                _userInfo = _userInfo.OrderBy(m => m._name.ToLower().Trim()).ToList();
                break;

            // Z => A By Username
            case 4:
                Debug.LogWarning("FIND ME Order 4");
                _userInfo = _userInfo.OrderByDescending(m => m._name.ToLower().Trim()).ToList();
                break;

            //Sadness => Love by Reactions
            case 5:
                Debug.LogWarning("FIND ME Order 5");
                _userInfo = _userInfo.OrderBy(m => m._mood).ToList();
                break;
        }

       //Sets the new arangement
        for (int i = 0; i < _userInfo.Count; i++) {

            _userInfo[i].UserDataObj.transform.SetSiblingIndex(i);
        }
    }

}



[Serializable]
public class UserReactionInfo
{
    public int _order;
    public GameObject UserDataObj;
    public AvatarData _avatar;
    public string _name;
    public string _id;
    public Mood _mood;
    public Sprite Emoji;

    //public Sprite Sprite;
    //public Mood Mood;

}


