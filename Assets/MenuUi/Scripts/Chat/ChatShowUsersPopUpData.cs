using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Chat;
using Altzone.Scripts.Common;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ServerChatMessage;

public class ChatShowUsersPopUpData : AltMonoBehaviour
{
    [SerializeField] private GameObject _panel;

    [Header("Buttons")]
    [SerializeField] private Button[] _closeButtons;
    [SerializeField] private Button _closeAllReactionButton;
    [SerializeField] private Button _listOrderButton;

    [Header("For Copied Reactions")]
    [SerializeField] private Transform _reactionFieldLocation;
    [SerializeField] private Transform _popUpAllReactions;
    [SerializeField] private ScrollRect _scrollRect;

    [Header("User Info")]
    [SerializeField] private VerticalLayoutGroup _userContent;
    [SerializeField] private GameObject _userData;
    private List<UsersReactionData> _usersReactionData = new();
    [SerializeField] private List<UserReactionInfo> _userInfo;

    [Header("Reactions")]
    [SerializeField] private TextMeshProUGUI _reactionAmounText;
    [SerializeField] private List<ReactionObject> _reactionList;
    private List<ReactionObjectHandler> _reactions = new();
    [SerializeField] private GameObject _reactionObject;
    [SerializeField] private GameObject _allReactions;
    [SerializeField] private GameObject _selectedReaction;
    [SerializeField] private GameObject _noReactions;


    [Header("List Order")]
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _orderButtonText;


    [SerializeField] private TextLanguageSelectorCaller _textLanguageSelectorCaller;

    private List<ChatReactionHandler> _reactionHandlers = new();
    private string _currentMessage;
    private int _lineOrder = 0;    //Whats the newest and the oldest reaction set
    private int _currentOrder = 1; //What type of order we are on the list

    private void Start()
    {
        _closeAllReactionButton.onClick.AddListener(LayoutButton);
        _listOrderButton.onClick.AddListener(() => {_currentOrder++; ListOrderSystem(_currentOrder);});
        Reactiontext();
        foreach (Button button in _closeButtons)
        {
            button.onClick.AddListener(ClosePopup);
        }
    }

    //Puts back the reactions and popup back to messageobject
    private void ClosePopup()
    {
        for (int i = _userInfo.Count - 1; i >= 0; i--)
        {
            Destroy(_userInfo[i].UserDataObj);
        }

        _userInfo.Clear();

        //Destroy(_copiedReactionField);
        //_copiedReactionField = null;
        _scrollRect.content = null;
        _currentMessage = null;
        gameObject.SetActive(false);
        _lineOrder = 0; 
        _currentOrder = 1; 
    }

    void OnEnable()
    {
        Reactiontext();
        ListOrderSystem(_currentOrder);
        MessageReactionsHandler.OnOpenReactionsPopup += ReactionFieldCopyUpdate;
    }


    //Incane user leaves the chat
    private void OnDisable()
    {
        ClosePopup();
        MessageReactionsHandler.OnOpenReactionsPopup -= ReactionFieldCopyUpdate;
    }


    private void Reactiontext()
    {
        int activeChildren = 0;
        foreach (Transform container in _userContent.transform)
        {
            if (container.gameObject.activeInHierarchy)
            {
                activeChildren++;
            }

        }

        //Language System
        if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
        {
            _textLanguageSelectorCaller.SetText("{} reaktiota");
        }
        else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
        {
            _textLanguageSelectorCaller.SetText("{} reactions");
        }

        _reactionAmounText.text = _reactionAmounText.text.Replace("{}", $"{activeChildren}");

    }



    public void AddUsersReaction(ChatMessage message, ServerReactions Emoji)
    {
        if (!_panel.activeInHierarchy) return;

        //Checks if it's on a correct Message section
        if (_currentMessage != null)
        {
            if (_currentMessage != message.Id)
                return;
        }
        else
        {
            _currentMessage = message.Id;
        }

        Emotion mood = Emotion.Blank;
        if (Emoji.emoji != null)
            if (!Enum.TryParse(Emoji.emoji, out mood))
            {
                if (Enum.TryParse(Emoji.emoji, out Mood moodType))
                {
                    mood = (Emotion)moodType;
                }
            }

        _lineOrder++;
            //Gets sprite
        Sprite reactionSprite = _reactionList.FirstOrDefault(x => x.Mood == mood)?.Sprite;

        //Sets up the Data
        GameObject newUserData = Instantiate(_userData, _userContent.transform);
        _userInfo.Add(new UserReactionInfo { _avatar = message.Avatar, _id = Emoji.sender_id, _name = Emoji.playerName, Emoji = reactionSprite, UserDataObj = newUserData, _mood = mood, _order = _lineOrder});
        UsersReactionData userData = newUserData.GetComponent<UsersReactionData>();
       userData.SetReactionInfo(_userInfo[_userInfo.Count - 1]._avatar, _userInfo[_userInfo.Count - 1]._name, _userInfo[_userInfo.Count - 1]._id, _userInfo[_userInfo.Count - 1].Emoji);
        _usersReactionData.Add(userData);
        Reactiontext();
        ListOrderSystem(_currentOrder);

        if (_reactionFieldLocation.childCount > 0)
        {
            _noReactions.SetActive(false);
        }

    }

    //removes the userData
    public void RemoveUserReaction(string Userid, ChatMessage message)
    {
        if (!_panel.activeInHierarchy) return;

        //Checks if it's on a correct Message section
        if (_currentMessage != null)
        {
            if (_currentMessage != message.Id)
                return;
        }

        for (int i = _userInfo.Count - 1; i >= 0; i--)
            {

                if (_userInfo[i]._id == Userid)
                {
                _userInfo[i].UserDataObj.transform.SetParent(null);
                Destroy(_userInfo[i].UserDataObj);
                _userInfo.RemoveAt(i);
                _lineOrder--;
                }


            }

        Reactiontext();

        if(_reactionFieldLocation.childCount == 0)
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

    //Copies the Reaction
    public void ReactionFieldCopyUpdate(MessageReactionsHandler ReactionHandler, ChatMessage message, List<ServerReactions> reactions)
    {

        //Checks if it's on a correct Message section
        if (_currentMessage != null)
        {
            if (_currentMessage != message.Id)
                return;
        }

        _panel.SetActive(true);

        foreach (Transform t in _reactionFieldLocation)
        {
            Destroy(t.gameObject);
        }

        foreach (ServerReactions reaction in reactions)
        {
            Emotion emojiType = Emotion.Blank;
            if (reaction.emoji != null)
                if (!Enum.TryParse(reaction.emoji, out emojiType))
                {
                    if (Enum.TryParse(reaction.emoji, out Mood mood))
                    {
                        emojiType = (Emotion)mood;
                    }
                }

            Sprite reactionSprite = _reactionList.FirstOrDefault(x => x.Mood == emojiType)?.Sprite;
            int i = _reactionList.FindIndex(m => m.Sprite == reactionSprite);

            // Checks if chosen reaction is already added to the selected message. If so, deletes it.
            foreach (ChatReactionHandler addedReaction in _reactionHandlers)
            {
                if (addedReaction.Mood == emojiType)
                {
                    addedReaction.AddReaction(reaction);
                    StartCoroutine(GetPlayerData(player =>
                    {
                        if (player != null)

                            if (player.Id == reaction.sender_id)
                            {

                                _reactionList[i].Selected = true;

                                addedReaction.Select();

                            }
                    }));
                    return;

                }
            }
            // Creates a reaction with the needed info and adds it to the selected message.
            GameObject newReaction = Instantiate(_reactionObject, _reactionFieldLocation);

            ChatReactionHandler chatReactionHandler = newReaction.GetComponentInChildren<ChatReactionHandler>();
            chatReactionHandler.SetReactionInfo(reactionSprite, message.Id, emojiType);
            chatReactionHandler.AddReaction(reaction);
            _reactionHandlers.Add(chatReactionHandler);
            AddUsersReaction(message, reaction);
            //chatReactionHandler.Button.onClick.AddListener(() => ToggleReaction(chatReactionHandler));
        }

        _scrollRect.content = _reactionFieldLocation.GetComponent<RectTransform>();

        //Changes the reactions object sizes
        foreach (RectTransform child in _reactionFieldLocation)
        {
            ContentSizeFitter childFitter = child.GetComponent<ContentSizeFitter>();
            childFitter.enabled = false;

            child.sizeDelta = new Vector2(150, 110);

            ChatReactionHandler Childhandler = child.GetComponent<ChatReactionHandler>();

            //Adds the toggle system to the copied button to direct the data to its original place
            foreach (ChatReactionHandler i in ReactionHandler.ReactionHandlers)
            {
                if(Childhandler.Mood == i.Mood)
                {
                    Childhandler.Button.onClick.AddListener(() => ReactionHandler.ToggleReaction(i));
                }
            }

        }
        ReactionHandler.GenarateReactionObjects(_popUpAllReactions);

    }


    void ListOrderSystem(int order)
    {
        if (order >= 6)
        {
            _currentOrder = 1;
            order = _currentOrder;
        }
        switch (order)
        {
            //List Order

            //Oldest => Newest
            case 1:
                
                _userInfo = _userInfo.OrderBy(m => m._order).ToList();

                //Language System
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                {
                    _orderButtonText.text = "Vanha";
                }
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                {
                    _orderButtonText.text =  "Old";
                }

                _image.sprite = null;
                break;

            //Newest => Oldest
            case 2:
                _userInfo = _userInfo.OrderByDescending(m => m._order).ToList();

                //Language System
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                {
                    _orderButtonText.text = "Uusi";
                }
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                {
                    _orderButtonText.text = "New";
                }

                _image.sprite = null;
                break;

            // A => Z By Username
            case 3:
                _userInfo = _userInfo.OrderBy(m => m._name.ToLower().Trim()).ToList();
                _orderButtonText.text = "A-Z";
                _image.sprite = null;
                break;

            // Z => A By Username
            case 4:
                _userInfo = _userInfo.OrderByDescending(m => m._name.ToLower().Trim()).ToList();
                _orderButtonText.text = "Z-A";
                _image.sprite = null;
                break;

            //Sadness => Love by Reactions
            case 5:
                _userInfo = _userInfo.OrderBy(m => m._mood).ToList();
                _orderButtonText.text = "";
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
    public Emotion _mood;
    public Sprite Emoji;

}


