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
    [SerializeField] private GameObject _usersWhoAdded;

    [SerializeField] public List<reactionPanelGroup> _reactionPaneldata;



    [SerializeField] private GameObject _allReactionsPanelPopUp;
    [SerializeField] private GameObject _selectedReaction;

    [Header("Prefabs")]
    [SerializeField] private GameObject _addedReactionPrefab;

    [Header("Message Script Reference")]
    [SerializeField] private MessageObjectHandler _selectedMessage;

    [Header("Reactions")]
    [SerializeField] public List<ReactionObject> _reactionList;
    [SerializeField] private GameObject _reactionObject;

    /*[Header("Added Features")]
    [SerializeField] private GameObject CanvasChat;
    */
    [Header("Buttons")]
    [SerializeField] private Button _openMoreButton;
    [SerializeField] private Button _openMoreButtonShowUser;

    private List<ReactionObjectHandler> _reactions = new();
    private List<ChatReactionHandler> _reactionHandlers = new();
    private List<int> _commonReactions = new();
    private bool _longClick = false;

    public MessageObjectHandler _messageObjectHandler;
    [SerializeField] private ChatShowUsersPopUpData _chatShowUsersPopUpData;

    void Start()
    {
         //For "AddReactions"
        _openMoreButton.onClick.AddListener((() => {
            _allReactionsPanel.SetActive(true);
            _selectedMessage.SizeCall();
            _commonReactionsPanel.SetActive(false);
            GetComponent<MessageReactionResize>().UpdateSize();
        }));
        //For "ShowUsersPopUp"
        _openMoreButtonShowUser.onClick.AddListener((() => {
            _allReactionsPanelPopUp.SetActive(true);
            _selectedReaction.SetActive(false);
            GetComponent<MessageReactionResize>().UpdateSize();
        }));



        //ReactionObjectHandler.OnReactionPressed += AddReaction;


        foreach (var reactionContent in _reactionPaneldata)
        {

            GenarateReactionObjects(reactionContent._reactionsContent);
            UpdateReactionStatus(reactionContent._reactionsContent);
        }

        PickCommonReactions();
    }


    private void OnDestroy()
    {

        //ReactionObjectHandler.OnReactionPressed -= AddReaction;
    }

    private void GenarateReactionObjects(Transform reactionPanel)
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
            if(reaction.GetComponent<ReactionObjectHandler>()) Destroy(reaction.gameObject);
        }

        List<ReactionObjectHandler> availableReactions = new();

        foreach (ReactionObjectHandler handler in _reactions) { if(!handler.Selected) availableReactions.Add(handler); }

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
        int index=0;
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
    public void UpdateReactions(List<ServerReactions> reactions, string messageid)
    {
        int objectAmount = 0;


        foreach (var reactionContent in _reactionPaneldata)
        {

            objectAmount++;

            foreach (ChatReactionHandler addedReaction in _reactionHandlers)
            {
                addedReaction.ResetReactions();
            }
            foreach (ServerReactions reaction in reactions)
            {
                        AddReaction(reaction, (Mood)Enum.Parse(typeof(Mood), reaction.emoji), messageid, true, reactionContent._reactionField, objectAmount);
            }
            List<ChatReactionHandler> removableReactions = new();
            foreach (ChatReactionHandler addedReaction in _reactionHandlers)
            {
                if (addedReaction.Count <= 0) removableReactions.Add(addedReaction);
            }
            for (int i = removableReactions.Count - 1; i >= 0; i--)
            {

                RemoveReaction(removableReactions[i]);
            }
        }
    }

    /// <summary>
    /// Adds the chosen reaction to the selected message.
    /// </summary>
    public void AddReaction(ServerReactions _reaction, Mood mood, string message_id, bool fromServer = false, GameObject ReactionPanel = null, int ActiveObjects = 0)
    {
        bool skip = true;
        int  ObjectActive = 0;


        ///"foreach" first checks how many the reactionfields are there
        ///"if" is used so that the other Reactionfield receives the same reaction too

        foreach(var i in _reactionPaneldata)
        {
                ObjectActive++;
        }
        if(ActiveObjects < ObjectActive + 1)
        {
           skip = false;
        }
        

        if (_selectedMessage != null)
        {
            
            string messageID = _selectedMessage.Id;
            if (messageID != message_id)
                return; 


                HorizontalLayoutGroup reactionsField = ReactionPanel.GetComponentInChildren<HorizontalLayoutGroup>();
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

                                //Removes the selected reaction
                                _reactionList[i].Selected = true;
                                    
                                addedReaction.Select();
                            }
                        }));
                    if (skip)
                    {
                        _selectedMessage.SetMessageInactive();
                        return;
                    }
                    }
                }

            // Creates a reaction with the needed info and adds it to the selected message.
            GameObject newReaction = Instantiate(_addedReactionPrefab, reactionsField.transform);
                ChatReactionHandler chatReactionHandler = newReaction.GetComponentInChildren<ChatReactionHandler>();
                chatReactionHandler.SetReactionInfo(reactionSprite, messageID, mood);
                chatReactionHandler.AddReaction(_reaction);
                _reactionHandlers.Add(chatReactionHandler);

                chatReactionHandler.Button.onClick.AddListener(() => ToggleReaction(chatReactionHandler));
                chatReactionHandler.LongClickButton.onLongClick.AddListener(() => ShowUsers(chatReactionHandler));
                StartCoroutine(GetPlayerData(player =>
                {
                    if (player != null)
                        if (player.Id == _reaction.sender_id)
                        {
                            chatReactionHandler.Select();
                            //Removes the selected Mood from the list  by checking what sprite is being used
                            _reactionList[i].Selected = true;
                        }

                }));
            foreach (var reactionContent in _reactionPaneldata)
            {
                GenarateReactionObjects(reactionContent._reactionsContent);
                UpdateReactionStatus(reactionContent._reactionsContent);
            }
            _chatShowUsersPopUpData.reactiontext();
                PickCommonReactions();
                LayoutRebuilder.ForceRebuildLayoutImmediate(reactionsField.GetComponent<RectTransform>());

            
                _selectedMessage.SetMessageInactive();

                gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
            
        }
    }

    /// <summary>
    /// Toggles the added reactions as selected and unselected.
    /// </summary>
    /// <param name="reactionHandler"></param>
    private void ToggleReaction(ChatReactionHandler reactionHandler)
    {
        ChatListener.Instance.SendReaction(!reactionHandler.Selected?reactionHandler.Mood.ToString(): string.Empty, reactionHandler.MessageID, ChatListener.Instance.ActiveChatChannel);

        if (!_longClick)
        {
            _selectedMessage.SetMessageInactive();


            if (reactionHandler.Selected)
            {
                reactionHandler.Deselect();

            foreach(var i in _reactionList)
            {
                if(i.Mood == reactionHandler.Mood)
                {
                        //Adds the set sprite back in to reaction selection
                    i.Selected = false;
                }
            }

                if (reactionHandler.Count <= 0)
                {
                     //Need to find away how i import data to the ServerChatMessage to make this work
                    //RemoveReaction(reactionHandler, null);
                }
            }
            else
            {
                reactionHandler.Select();
            }
        }
    }

    private void ShowUsers(ChatReactionHandler reactionHandler)
    {
        RectTransform rt = _usersWhoAdded.GetComponent<RectTransform>();

        _longClick = true;
        _usersWhoAdded.transform.SetParent(Chat.instance.PopUps.transform);

        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _usersWhoAdded.SetActive(true);
        //_chatScript.OpenUsersWhoAddedReactionPanel();

        Invoke("ResetLongClick", 2);
    }

    private void ResetLongClick()
    {
        _longClick = false;
    }

    private void RemoveReaction(ChatReactionHandler reaction)
    {
        //Checks if the set gameObject is on or not
        foreach (var reactioncontent in _reactionPaneldata)
        {
            HorizontalLayoutGroup reactionsField = reactioncontent._reactionField.GetComponent<HorizontalLayoutGroup>();

            

            foreach (var i in _reactionList)
            {
                if (i.Mood == reaction.Mood)
                {
                    i.Selected = false;
                }
            }

            reaction.transform.SetParent(null);
            _reactionHandlers.Remove(reaction);

            Destroy(reaction.gameObject);


            GenarateReactionObjects(reactioncontent._reactionsContent);
            UpdateReactionStatus(reactioncontent._reactionsContent);

            _chatShowUsersPopUpData.reactiontext();
            PickCommonReactions();
            _selectedMessage.SizeCall();

            LayoutRebuilder.ForceRebuildLayoutImmediate(reactionsField.GetComponent<RectTransform>());
        }
        //_chatScript.UpdateContentLayout(reactionsField);
    }

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
}
