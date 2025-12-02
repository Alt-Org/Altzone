using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Chat;
using UnityEngine;
using UnityEngine.UI;

public class MessageReactionsHandler : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private GameObject _commonReactionsPanel;
    [SerializeField] private GameObject _allReactionsPanel;
    [SerializeField] private GameObject _usersWhoAdded;
    [SerializeField] private Transform _reactionsContent;

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

    private List<GameObject> _reactions = new();
    private List<ChatReactionHandler> _reactionHandlers = new();
    private List<int> _commonReactions = new();
    private bool _longClick = false;

    public static MessageReactionsHandler Instance;

    private Mood _mood;
    void Start()
    {
        _openMoreButton.onClick.AddListener((() => { _allReactionsPanel.SetActive(true); _selectedMessage.SizeCall(); _commonReactionsPanel.SetActive(false);}));



        ChatChannel.OnReactionReceived += RefreshReaction;


        ReactionObjectHandler.OnReactionPressed += AddReaction;

        GenarateReactionObjects();
        CreateReactionInteractions();
        PickCommonReactions();
    }



    /// <summary>
    /// Old code does not work
    /// </summary>
   /*void ReactionRefresh(ChatChannelType chatChannelType, ChatMessage reaction) => StartCoroutine(ReactionRefreshCoroutine(chatChannelType, reaction));

    IEnumerator ReactionRefreshCoroutine(ChatChannelType chatChannelType, ChatMessage reaction)
    {
        if (chatChannelType is ChatChannelType.Global)
            yield return new WaitUntil(() => ChatListener.Instance.GlobalChatFetched);
        if (chatChannelType is ChatChannelType.Clan)
            yield return new WaitUntil(() => ChatListener.Instance.ClanChatFetched);

        if(CanvasChat.gameObject.activeSelf)
        {
            List<ChatMessage> messagelist = ChatListener.Instance.GetChatChannel(chatChannelType).ChatMessages;
            if(messagelist != null)
            {
                foreach (ChatMessage message in messagelist)
                {
                    Sprite reactionSprite = _reactionList.FirstOrDefault(x => x.Mood == message.mood)?.Sprite;
                    bool ownRct = message?.SenderId == ServerManager.Instance.Player._id;
                    AddReaction(reactionSprite, message, ChatReactionHandler.Instance.SetReactionInfo(ownRct, message.Mood));
                }
            }
        }

    }*/

    private void OnDestroy()
    {

        ChatChannel.OnReactionReceived -= RefreshReaction;

        ReactionObjectHandler.OnReactionPressed -= AddReaction;
    }

    private void GenarateReactionObjects()
    {
        _reactions.Clear();
        foreach (Transform reaction in _reactionsContent)
        {
            Destroy(reaction.gameObject);
        }
        foreach (ReactionObject reaction in _reactionList)
        {
            if (reaction.Sprite != null && reaction.Mood != Mood.None)
            {
                //_reactions.FirstOrDefault(x => x.);
                GameObject reactionObject = Instantiate(_reactionObject, _reactionsContent);
                if (!reactionObject.TryGetComponent(out ReactionObjectHandler handler))
                {
                    handler = reactionObject.AddComponent<ReactionObjectHandler>();
                }
                handler.SetInfo(reaction.Mood, reaction.Sprite, _selectedMessage.Id);
                _reactions.Add(reactionObject);
            }
        }
    }

    /// <summary>
    /// Adds interaction to all the reactions
    /// </summary>
    private void CreateReactionInteractions()
    {
        foreach (GameObject reaction in _reactions)
        {
            // Adds a button to the reaction if it doesn't already have one
            if (!reaction.TryGetComponent(out ReactionObjectHandler handler))
            {
                handler = reaction.AddComponent<ReactionObjectHandler>();
            }

            //handler.SetInfo();
        }
    }

    /// <summary>
    /// Picks common reactions for the common reaction panel that opens when first selecting a message.
    /// (Since no data about reactions is currently available, common reactions are picked at random.)
    /// </summary>
    private void PickCommonReactions()
    {
        int randomReaction;

        for (int i = 0; i < 3; i++)
        {
            do
            {
                randomReaction = UnityEngine.Random.Range(0, _reactions.Count);
            }
            while (_commonReactions.Contains(randomReaction));

            _commonReactions.Add(randomReaction);
        }

        foreach (int reactionIndex in _commonReactions)
        {
            GameObject commonReaction = Instantiate(_reactions[reactionIndex], _commonReactionsPanel.transform);
            commonReaction.transform.SetAsFirstSibling();

            Mood mood = _reactions[reactionIndex].GetComponent<ReactionObjectHandler>().Mood;

            if (!commonReaction.TryGetComponent(out ReactionObjectHandler handler))
            {
                handler = commonReaction.AddComponent<ReactionObjectHandler>();
            }

            ReactionObject reactionData = _reactionList.FirstOrDefault(x => x.Mood == mood);
            if(reactionData != null)
            handler.SetInfo(reactionData.Mood, reactionData.Sprite, _selectedMessage.Id);

            /*if (!commonReaction.TryGetComponent(out Button button))
            {
                button = commonReaction.AddComponent<Button>();
            }

            button.onClick.AddListener(() => AddReaction(commonReaction));*/
        }
    }

    private void SaveReactions(ChatMessage message)
    {
        foreach (var reaction in message.Reactions)
        {
            Debug.LogWarning("Saved reaction");
        }
    }

    //converts the string into mood
    private Mood ConvertVoid(string emojiString)
    {
        if (Enum.TryParse(emojiString, out Mood mood)) {
            return mood;
        }

        return Mood.None;
    }

    //Saves and refreshes the reactions
    //Some reason doesnt do that now as it will just fetch its messages reaction type :/
    private void RefreshReaction(ChatChannelType chatChannelType, ChatMessage message)
    {
        Debug.Log("Reaction receive form " + message.Id);

        SaveReactions(message);


        foreach (var reaction in message.Reactions)
        {
            //Mood moodEnum = ConvertVoid(reaction.emoji);

            AddReaction(message.Id, message.Mood);
        }
    }


    /// <summary>
    /// Adds the chosen reaction to the selected message.
    /// </summary>
    private void AddReaction(string _id, Mood mood)
    {
        if (_selectedMessage != null)
        {
            string messageID = _selectedMessage.Id;
            if (messageID != _id) return;

            HorizontalLayoutGroup reactionsField = _selectedMessage.ReactionsPanel.GetComponentInChildren<HorizontalLayoutGroup>();

            Sprite reactionSprite =_reactionList.FirstOrDefault(x => x.Mood == mood)?.Sprite;

            // Checks if chosen reaction is already added to the selected message. If so, deletes it.
            foreach (ChatReactionHandler addedReaction in _reactionHandlers)
            {
                if (addedReaction._messageID == messageID && addedReaction.ReactionImage.sprite == reactionSprite)
                {
                    RemoveReaction(addedReaction);
                    _selectedMessage.SetMessageInactive();

                    return;
                }
            }

            // Creates a reaction with the needed info and adds it to the selected message.
            GameObject newReaction = Instantiate(_addedReactionPrefab, reactionsField.transform);
            ChatReactionHandler chatReactionHandler = newReaction.GetComponentInChildren<ChatReactionHandler>();
            chatReactionHandler.SetReactionInfo(reactionSprite, messageID, mood);
            _reactionHandlers.Add(chatReactionHandler);

            chatReactionHandler.Button.onClick.AddListener(() => ToggleReaction(chatReactionHandler));
            chatReactionHandler.LongClickButton.onLongClick.AddListener(() => ShowUsers(chatReactionHandler));



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
        ChatListener.Instance.SendReaction(reactionHandler.Mood.ToString(), reactionHandler._messageID, ChatListener.Instance.ActiveChatChannel);

        if (!_longClick)
        {
            _selectedMessage.SetMessageInactive();

            if (reactionHandler._selected)
            {
                reactionHandler.Deselect();

                if (reactionHandler._count <= 0)
                {
                    RemoveReaction(reactionHandler);
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
        _longClick = true;
        //_chatScript.OpenUsersWhoAddedReactionPanel();

        Invoke("ResetLongClick", 2);
    }

    private void ResetLongClick()
    {
        _longClick = false;
    }

    private void RemoveReaction(ChatReactionHandler reaction)
    {
        HorizontalLayoutGroup reactionsField = reaction.GetComponentInParent<HorizontalLayoutGroup>();

        reaction.transform.SetParent(null);
        _reactionHandlers.Remove(reaction);
        Destroy(reaction.gameObject);
        _selectedMessage.SizeCall();

        LayoutRebuilder.ForceRebuildLayoutImmediate(reactionsField.GetComponent<RectTransform>());
        //_chatScript.UpdateContentLayout(reactionsField);
    }


    [Serializable]
    private class ReactionObject
    {
        public Sprite Sprite;
        public Mood Mood;
    }
}
