using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatAddReactions : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private Transform _commonReactionsPanel;
    [SerializeField] private Transform _allReactionsPanel;
    [SerializeField] private GameObject _usersWhoAdded;

    [Header("Prefabs")]
    [SerializeField] private GameObject _addedReactionPrefab;

    [Header("Chat Reference")]
    [SerializeField] private Chat _chatScript;

    [Header("Reactions")]
    [SerializeField] private GameObject[] _reactions;

    private List<ChatReactionHandler> _reactionHandlers = new();
    private List<int> _commonReactions = new();

    private bool _longClick = false;

    void Start()
    {
        CreateReactionInteractions();
        PickCommonReactions();
    }

    /// <summary>
    /// Adds interaction to all the reactions
    /// </summary>
    private void CreateReactionInteractions()
    {
        foreach (GameObject reaction in _reactions)
        {
            // Adds a button to the reaction if it doesn't already have one
            if (!reaction.TryGetComponent(out Button button))
            {
                button = reaction.AddComponent<Button>();
            }

            button.onClick.AddListener(() => AddReaction(reaction));
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
                randomReaction = Random.Range(0, _reactions.Length);
            }
            while (_commonReactions.Contains(randomReaction));

            _commonReactions.Add(randomReaction);
        }

        foreach (int reactionIndex in _commonReactions)
        {
            GameObject commonReaction = Instantiate(_reactions[reactionIndex], _commonReactionsPanel.transform);
            commonReaction.transform.SetAsFirstSibling();

            if (!commonReaction.TryGetComponent(out Button button))
            {
                button = commonReaction.AddComponent<Button>();
            }

            button.onClick.AddListener(() => AddReaction(commonReaction));
        }
    }

    /// <summary>
    /// Adds the chosen reaction to the selected message.
    /// </summary>
    private void AddReaction(GameObject reaction)
    {
        if (_chatScript != null)
        {
            GameObject selectedMessage = _chatScript.selectedMessage;
            HorizontalLayoutGroup reactionsField = selectedMessage.GetComponentInChildren<HorizontalLayoutGroup>();

            Image reactionImage = reaction.GetComponentInChildren<Image>();
            Sprite reactionSprite = reactionImage.sprite;
            int messageID = selectedMessage.GetInstanceID();

            // Checks if chosen reaction is already added to the selected message. If so, deletes it.
            foreach (ChatReactionHandler addedReaction in _reactionHandlers)
            {
                if (addedReaction._messageID == messageID && addedReaction._reactionImage.sprite == reactionSprite)
                {
                    RemoveReaction(addedReaction);
                    _chatScript.DeselectMessage(selectedMessage);

                    return;
                }
            }

            // Creates a reaction with the needed info and adds it to the selected message.
            GameObject newReaction = Instantiate(_addedReactionPrefab, reactionsField.transform);
            ChatReactionHandler chatReactionHandler = newReaction.GetComponentInChildren<ChatReactionHandler>();
            chatReactionHandler.SetReactionInfo(reactionSprite, messageID);
            _reactionHandlers.Add(chatReactionHandler);

            chatReactionHandler._button.onClick.AddListener(() => ToggleReaction(chatReactionHandler));
            chatReactionHandler._longClickButton.onLongClick.AddListener(() => ShowUsers(chatReactionHandler));
            chatReactionHandler._button.onClick.AddListener(() => _chatScript.MinimizeOptions());

            LayoutRebuilder.ForceRebuildLayoutImmediate(reactionsField.GetComponent<RectTransform>());

            _chatScript.DeselectMessage(selectedMessage);
            _chatScript.UpdateContentLayout(reactionsField);
        }
    }

    /// <summary>
    /// Toggles the added reactions as selected and unselected.
    /// </summary>
    /// <param name="reactionHandler"></param>
    private void ToggleReaction(ChatReactionHandler reactionHandler)
    {
        if (!_longClick)
        {
            _chatScript.DeselectMessage(_chatScript.selectedMessage);

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
        _chatScript.OpenUsersWhoAddedReactionPanel();

        Invoke("ResetLongClick", 2);
    }

    private void ResetLongClick()
    {
        _longClick = false;
    }

    private void RemoveReaction(ChatReactionHandler reaction)
    {
        HorizontalLayoutGroup reactionsField = reaction.GetComponentInParent<HorizontalLayoutGroup>();

        _reactionHandlers.Remove(reaction);
        Destroy(reaction.gameObject);

        LayoutRebuilder.ForceRebuildLayoutImmediate(reactionsField.GetComponent<RectTransform>());
        _chatScript.UpdateContentLayout(reactionsField);
    }
}
