using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Chat;
using UnityEngine;
using UnityEngine.UI;
using static ServerChatMessage;

public class MessageReactionsHandler : AltMonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private GameObject _commonReactionsPanel;
    [SerializeField] private GameObject _allReactionsPanel;

    [SerializeField] private reactionPanelGroup _reactionPaneldata;
    [Header("Prefabs")]
    [SerializeField] private GameObject _addedReactionPrefab;

    [Header("Message Script Reference")]
    [SerializeField] private MessageObjectHandler _selectedMessage;

    [Header("Reactions")]
    [SerializeField] private List<ReactionObject> _reactionList;
    [SerializeField] private GameObject _reactionObject;

    /*[Header("Added Features")]
    [SerializeField] private GameObject CanvasChat;
    */
    [Header("Buttons")]
    [SerializeField] private Button _openMoreButton;

    private List<ReactionObjectHandler> _reactions = new();
    private List<ChatReactionHandler> _reactionHandlers = new();
    private List<int> _commonReactions = new();
    private bool _longClick = false;
    private ChatShowUsersPopUpData _reactionPopup = Chat.instance?.ChatShowUsersPopUpData;

    [SerializeField] public List<ServerReactions> _reactionData = new List<ServerReactions>(); //Used for addings data to ChatShowUserPopUpData


    void Start()
    {
        //For "AddReactions"
        _openMoreButton.onClick.AddListener((() =>
        {
            _allReactionsPanel.SetActive(true);
            _selectedMessage.SizeCall();
            _commonReactionsPanel.SetActive(false);
            GetComponent<MessageReactionResize>().UpdateSize();
        }));


            //ReactionObjectHandler.OnReactionPressed += AddReaction;


            GenarateReactionObjects(_reactionPaneldata._reactionsContent);

        PickCommonReactions();
    }


    private void OnDestroy()
    {

        //ReactionObjectHandler.OnReactionPressed -= AddReaction;
    }

    public void GenarateReactionObjects(Transform reactionPanel)
    {
        _reactions.Clear();
        foreach (Transform reaction in reactionPanel)
        {
            Destroy(reaction.gameObject);
        }
        foreach (ReactionObject reaction in _reactionList)
        {
            if (reaction.Sprite != null && reaction.Mood != Mood.None)
            {
                //_reactions.FirstOrDefault(x => x.);
                GameObject reactionObject = Instantiate(_reactionObject, reactionPanel);
                if (!reactionObject.TryGetComponent(out ReactionObjectHandler handler))
                {
                    handler = reactionObject.AddComponent<ReactionObjectHandler>();
                }
                handler.SetInfo(reaction, _selectedMessage.Id);
                _reactions.Add(handler);
            }
        }
        UpdateReactionStatus(reactionPanel);
    }

    private void UpdateReactionStatus(Transform reactionPanel)
    {
        foreach (Transform child in reactionPanel)
        {
            if (child.GetComponent<ReactionObjectHandler>() == null) continue;
            Mood mood = child.GetComponent<ReactionObjectHandler>().Mood;

            foreach (var r in _reactionList)
            {
                if (mood == r.Mood)
                    child.gameObject.SetActive(!r.Selected);
            }
        }
    }

    /// <summary>
    /// Picks common reactions for the common reaction panel that opens when first selecting a message.
    /// (Since no data about reactions is currently available, common reactions are picked at random.)
    /// </summary>
    private void PickCommonReactions()
    {
        foreach (Transform reaction in _commonReactionsPanel.transform)
        {
            if (reaction.GetComponent<ReactionObjectHandler>()) Destroy(reaction.gameObject);
        }

        List<ReactionObjectHandler> availableReactions = new();

        foreach (ReactionObjectHandler handler in _reactions) { if (!handler.Selected) availableReactions.Add(handler); }

        _commonReactions.Clear();
        int randomReaction;

        for (int i = 0; i < 3; i++)
        {
            /*do
            {
                randomReaction = UnityEngine.Random.Range(0, availableReactions.Count);
            }
            while (_commonReactions.Contains(randomReaction));*/

            if (availableReactions.Count <= i) break;
            randomReaction = i;

            _commonReactions.Add(randomReaction);
        }
        int index = 0;
        foreach (int reactionIndex in _commonReactions)
        {
            ReactionObjectHandler commonReaction = Instantiate(availableReactions[reactionIndex].gameObject, _commonReactionsPanel.transform).GetComponent<ReactionObjectHandler>();
            commonReaction.transform.SetSiblingIndex(index);

            Mood mood = availableReactions[reactionIndex].GetComponent<ReactionObjectHandler>().Mood;

            ReactionObject reactionData = _reactionList.FirstOrDefault(x => x.Mood == mood);
            if (reactionData != null)
                commonReaction.SetInfo(reactionData, _selectedMessage.Id);

            /*if (!commonReaction.TryGetComponent(out Button button))
            {
                button = commonReaction.AddComponent<Button>();
            }

            button.onClick.AddListener(() => AddReaction(commonReaction));*/

            index++;
        }
    }
    /// <summary>
    /// Updates the all of the reactions of the selected message.
    /// </summary>
    public void UpdateReactions(List<ServerReactions> reactions, string messageid, ChatMessage message)
    {
        foreach (ChatReactionHandler addedReaction in _reactionHandlers)
        {
            addedReaction.ResetReactions();
        }
        foreach (ServerReactions reaction in reactions)
        {

            
            AddReaction(reaction, (Mood)Enum.Parse(typeof(Mood), reaction.emoji), messageid, _reactionPaneldata._reactionField, message);

        }
        List<ChatReactionHandler> removableReactions = new();
        foreach (ChatReactionHandler addedReaction in _reactionHandlers)
        {
            if (addedReaction.Count <= 0)
            {
                removableReactions.Add(addedReaction);
            }
        }
        for (int i = removableReactions.Count - 1; i >= 0; i--)
        {
            RemoveReaction(removableReactions[i], message);
        }

            UpdateReactionData(reactions, message);


            //Used for "ChatShowUserPopUpData"
            if (_reactionPopup.gameObject.activeSelf)
        {
            _reactionPopup.ReactionFieldCopyUpdate(_reactionPaneldata._reactionField, this, message);
        }

    }
    /// <summary>
    /// Adds the chosen reaction to the selected message.
    /// </summary>
    public void AddReaction(ServerReactions _reaction, Mood mood, string message_id, GameObject ReactionPanel = null, ChatMessage message = null)
    {
        if (_selectedMessage != null)
        {
            string messageID = _selectedMessage.Id;
            if (messageID != message_id)
                return;

            HorizontalLayoutGroup reactionsFields = ReactionPanel.GetComponent<HorizontalLayoutGroup>();

            Sprite reactionSprite = _reactionList.FirstOrDefault(x => x.Mood == mood)?.Sprite;
            int i = _reactionList.FindIndex(m => m.Sprite == reactionSprite);

            // Checks if chosen reaction is already added to the selected message. If so, deletes it.
            foreach (ChatReactionHandler addedReaction in _reactionHandlers)
            {
                if (addedReaction.Mood == mood)
                {
                    addedReaction.AddReaction(_reaction);
                    StartCoroutine(GetPlayerData(player =>
                    {
                        if (player != null)

                            if (player.Id == _reaction.sender_id)
                            {
                                
                                //Refreshes the reaction selection
                                ClearList(_reaction);

                                _reactionList[i].Selected = true;

                                addedReaction.Select();

                            }
                    }));
                    GenarateReactionObjects(_reactionPaneldata._reactionsContent);
                    _selectedMessage.SetMessageInactive();
                    return;

                }
            }
            // Creates a reaction with the needed info and adds it to the selected message.
            GameObject newReaction = Instantiate(_addedReactionPrefab, reactionsFields.transform);

            ChatReactionHandler chatReactionHandler = newReaction.GetComponentInChildren<ChatReactionHandler>();
            chatReactionHandler.SetReactionInfo(reactionSprite, messageID, mood);
            chatReactionHandler.AddReaction(_reaction);
            _reactionHandlers.Add(chatReactionHandler);
            chatReactionHandler.Button.onClick.AddListener(() => ToggleReaction(chatReactionHandler, _reaction, message));
            chatReactionHandler.LongClickButton.onLongClick.AddListener(() => ShowUsers(chatReactionHandler, message));

            StartCoroutine(GetPlayerData(player =>
            {
                if (player != null)
                    if (player.Id == _reaction.sender_id)
                    {

                        chatReactionHandler.Select();

                        //Refreshes the reaction selection
                        ClearList(_reaction);
                        _reactionList[i].Selected = true;

                    } 
            }));

            GenarateReactionObjects(_reactionPaneldata._reactionsContent);

            PickCommonReactions();

            LayoutRebuilder.ForceRebuildLayoutImmediate(ReactionPanel.GetComponent<RectTransform>());

            _selectedMessage.SetMessageInactive();


            gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");

        }
    }

    /// <summary>
    /// Toggles the added reactions as selected and unselected.
    /// </summary>
    /// <param name="reactionHandler"></param>
    public void ToggleReaction(ChatReactionHandler reactionHandler, ServerReactions _reaction, ChatMessage message)
    {


        ChatListener.Instance.SendReaction(!reactionHandler.Selected ? reactionHandler.Mood.ToString() : string.Empty, reactionHandler.MessageID, ChatListener.Instance.ActiveChatChannel);
        if (!_longClick)
        {
            ClearList(_reaction);
            

            if (reactionHandler.Selected)
            {
                reactionHandler.Deselect();
            }
            else
            {
                reactionHandler.Select();
            }
            GenarateReactionObjects(_reactionPaneldata._reactionsContent);
        }
    }

    private void ShowUsers(ChatReactionHandler reactionHandler, ChatMessage message)
    {

        if (_reactionPopup.gameObject.activeSelf)
            return;

        //Gets needed data for ChatShowUserPopUpData1
        _reactionPopup.gameObject.SetActive(true);

        //Changes the reactions object sizes
        _reactionPopup.ReactionFieldCopyUpdate(_reactionPaneldata._reactionField, this, message);

        foreach (var reactionData in _reactionData)
        {
            _reactionPopup.AddUsersReaction(message, reactionData);
        }

        _longClick = true;

        Invoke("ResetLongClick", 2);

    }

    //Used for ChatShowUsersPopUpData to update the data
    private void UpdateReactionData(List<ServerReactions> reactions, ChatMessage message)
    {
        //Empties the data
        if (_reactionPopup.gameObject.activeSelf)
        {
            foreach (var i in _reactionData)
            {
                _reactionPopup.RemoveUserReaction(i.sender_id, message);
            }
        }

        _reactionData.Clear();


        //Adds the data
        foreach (ServerReactions reaction in reactions)
        {
            _reactionData.Add(reaction);

        }

        if (_reactionPopup.gameObject.activeSelf)
        {

            foreach (var reactionData in _reactionData)
            {
                _reactionPopup.AddUsersReaction(message, reactionData);

            }
        }
    }

    private void ResetLongClick()
    {
        _longClick = false;
    }

    private void RemoveReaction(ChatReactionHandler reaction, ChatMessage message)
    {
        //Checks if the set gameObject is on or not
        HorizontalLayoutGroup reactionsField = _reactionPaneldata._reactionField.GetComponent<HorizontalLayoutGroup>();
        reaction.transform.SetParent(null);
        _reactionHandlers.Remove(reaction);


        foreach (var i in _reactionList)
        {
            if (i.Mood == reaction.Mood)
            {
                //Resets the reactions selections list back to zero
                i.Selected = false;

            }
        }
        Destroy(reaction.gameObject);

        GenarateReactionObjects(_reactionPaneldata._reactionsContent);


        PickCommonReactions();
        _selectedMessage.SizeCall();

        LayoutRebuilder.ForceRebuildLayoutImmediate(reactionsField.GetComponent<RectTransform>());
    }


    //Clearns the list empty (with those what has players ID) so that new data can be added
    private void ClearList(ServerReactions _reaction)
    {

        for (int i = 0; i < _reactionData.Count; i++)
        {
            if (_reactionData[i].sender_id == _reaction.sender_id)
            {
                foreach (var j in _reactionList)
                {
                    //Adds the set sprite back in to reaction selection
                    j.Selected = false;
                }
            }
        }
    }
}

    //Reverts reactions size back to orignal size when leaving ShowUserPopUp

    [Serializable]
    public class ReactionObject
    {
        public delegate void SelectedStatusChanged(Mood mood, bool selected);
        public event SelectedStatusChanged OnSelectedStatusChanged;

        public Sprite Sprite;
        public Mood Mood;
        private bool _selected;
        public bool Selected { get => _selected;
            set
            {
                _selected = value;
                OnSelectedStatusChanged?.Invoke(Mood, value);
            }
        }
    }

    [Serializable]
    public class reactionPanelGroup
    {
        public Transform _reactionsContent; //Imports the reaction
        public GameObject _reactionField; //holds  the received reactions
    }

