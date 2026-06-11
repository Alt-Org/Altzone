using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Altzone.Scripts.Chat;
using Altzone.Scripts.Model.Poco.Player;
using static ServerChatMessage;
using System.Linq;

public class ChatShowUsersPopUpData : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button[] _closeButtons;
    [SerializeField] private Button _closeAllReactionButton;
    [SerializeField] private Button _listOrderButton;

    [Header("For Copied Reactions")]
    [SerializeField] private Transform _reactionFieldLocation;
    private GameObject CopiedReactionField;
    [SerializeField] private Transform PopUpAllReactions;
    [SerializeField] private ScrollRect _scrollRect;

    [Header("User Info")]
    [SerializeField] private VerticalLayoutGroup _userContent;
    [SerializeField] private GameObject _userData;
    private List<UsersReactionData> _usersReactionData = new();
    [SerializeField] private List<UserReactionInfo> _userInfo;

    [Header("Reactions")]
    [SerializeField] private TextMeshProUGUI ReactionAmounText;
    [SerializeField] private List<ReactionObject> _reactionList;
    [SerializeField] private GameObject _allReactions;
    [SerializeField] private GameObject _selectedReaction;
    [SerializeField] private GameObject _noReactions;

    [Header("List Order")]
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _OrderButtonText;

    private string CurrentMessage;
    private int LineOrder = 0;
    private int currentOrder = 1;

    private void Start()
    {
        _closeAllReactionButton.onClick.AddListener(LayoutButton);
        _listOrderButton.onClick.AddListener(() => {currentOrder++; ListOrder(currentOrder);});
        reactiontext();
        foreach (Button button in _closeButtons)
        {
            button.onClick.AddListener(ClosePopup);
        }
    }

    //Puts back the reactions and popup back to messageobject
    public void ClosePopup()
    {
        for (int i = _userInfo.Count - 1; i >= 0; i--)
        {
            Destroy(_userInfo[i].UserDataObj);
        }

        _userInfo.Clear();

        Destroy(CopiedReactionField);
        CopiedReactionField = null;
        _scrollRect.content = null;
        CurrentMessage = null;
        gameObject.SetActive(false);
        LineOrder = 0;

    }

    void OnEnable()
    {
        reactiontext();
    }


    //Incane user leaves the chat
    private void OnDisable()
    {
        ListOrder(1);
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
        //Checks if it's on a correct Message section
        if (CurrentMessage != null)
        {
            if (CurrentMessage != message.Id)
                return;
        }
        else
        {
            CurrentMessage = message.Id;
        }

                Mood mood = (Mood)Enum.Parse(typeof(Mood), Emoji.emoji);

        //halts the system if there's already a same user on the list
        for (int i = _userInfo.Count - 1; i >= 0; i--)
        {
            if (_userInfo[i]._id != Emoji.sender_id)
                continue;


            if (_userInfo[i]._id == Emoji.sender_id &&  _userInfo[i]._mood != mood)
            {
                RemoveUserReaction(Emoji.sender_id, message);
                continue;
            }
            else if (_userInfo[i]._id == Emoji.sender_id && _userInfo[i]._mood == mood)
            {
                
                return;
            }
        }
        LineOrder++;
            //Gets sprite
        Sprite reactionSprite = _reactionList.FirstOrDefault(x => x.Mood == mood)?.Sprite;

        //Sets up the Data
        GameObject newUserData = Instantiate(_userData, _userContent.transform);
        _userInfo.Add(new UserReactionInfo { _avatar = message.Avatar, _id = Emoji.sender_id, _name = Emoji.playerName, Emoji = reactionSprite, UserDataObj = newUserData, _mood = mood, _order = LineOrder});
        UsersReactionData userData = newUserData.GetComponent<UsersReactionData>();
       userData.SetReactionInfo(_userInfo[_userInfo.Count - 1]._avatar, _userInfo[_userInfo.Count - 1]._name, _userInfo[_userInfo.Count - 1]._id, _userInfo[_userInfo.Count - 1].Emoji);
        _usersReactionData.Add(userData);
        reactiontext();
        ListOrder(currentOrder);

        if (CopiedReactionField.transform.childCount > 0)
        {
            _noReactions.SetActive(false);
        }

    }

    //removes the userData
    public void RemoveUserReaction(string Userid, ChatMessage message)
    {
        if (CurrentMessage != null)
        {
            if (CurrentMessage != message.Id)
                return;
        }

        for (int i = _userInfo.Count - 1; i >= 0; i--)
            {

                if (_userInfo[i]._id == Userid)
                {
                _userInfo[i].UserDataObj.transform.SetParent(null);
                Destroy(_userInfo[i].UserDataObj);
                _userInfo.RemoveAt(i);
                LineOrder--;
                }


            }
                reactiontext();
        if(CopiedReactionField.transform.childCount == 0)
        {
            _noReactions.SetActive(true);
        }
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

    //Changes the reactions object sizes
    public void ReactionFieldCopyUpdate(GameObject ReactionField, MessageReactionsHandler ReactionHandler, ChatMessage message)
    {

        //Checks if it's on a correct Message section
        if (CurrentMessage != null)
        {
            if (CurrentMessage != message.Id)
                return;
        }


        Destroy(CopiedReactionField);

        CopiedReactionField = Instantiate(ReactionField, _reactionFieldLocation);

        _scrollRect.content = CopiedReactionField.GetComponent<RectTransform>();

        foreach (RectTransform child in CopiedReactionField.transform)
        {
            ContentSizeFitter childFitter = child.GetComponent<ContentSizeFitter>();
            childFitter.enabled = false;

            child.sizeDelta = new Vector2(150, 110);
        }

        ReactionHandler.GenarateReactionObjects(PopUpAllReactions);

    }


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
                
                _userInfo = _userInfo.OrderBy(m => m._order).ToList();
                _OrderButtonText.text = "Old";
                _image.sprite = null;
                break;

            //Newest => Oldest
            case 2:
                _userInfo = _userInfo.OrderByDescending(m => m._order).ToList();
                _OrderButtonText.text = "New";
                _image.sprite = null;
                break;

            // A => Z By Username
            case 3:
                _userInfo = _userInfo.OrderBy(m => m._name.ToLower().Trim()).ToList();
                _OrderButtonText.text = "A-Z";
                _image.sprite = null;
                break;

            // Z => A By Username
            case 4:
                _userInfo = _userInfo.OrderByDescending(m => m._name.ToLower().Trim()).ToList();
                _OrderButtonText.text = "Z-A";
                _image.sprite = null;
                break;

            //Sadness => Love by Reactions
            case 5:
                _userInfo = _userInfo.OrderBy(m => m._mood).ToList();
                _OrderButtonText.text = "";
                if (_sprites[4] != null)
                _image.sprite = _sprites[4];
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

}


