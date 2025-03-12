using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatAddReactions : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private Transform _commonReactionsPanel;
    [SerializeField] private Transform _allReactionsPanel;

    [Header("Prefabs")]
    [SerializeField] private GameObject _addedReactionPrefab;

    [Header("Chat Reference")]
    [SerializeField] private Chat _chatScript;

    [Header("Reactions")]
    [SerializeField] private GameObject[] _reactions;

    private List<ChatReactionHandler> _reactionHandlers = new();

    void Start()
    {
        CreateReactionInteractions();
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
    /// Pick common reactions for the common reaction panels that opens when first selecting a message.
    /// (Since no data about reactions is currently available, common reactions are picked at random.)
    /// </summary>
    private void PickCommonReactions()
    {

    }

    /// <summary>
    /// Adds the chosen reaction to the selected message.
    /// </summary>
    public void AddReaction(GameObject reaction)
    {
        if (_chatScript != null)
        {
            GameObject selectedMessage = _chatScript.selectedMessage;
            HorizontalLayoutGroup reactionsField = selectedMessage.GetComponentInChildren<HorizontalLayoutGroup>();

            Image reactionImage = reaction.GetComponentInChildren<Image>();
            Sprite reactionSprite = reactionImage.sprite;
            int counter = 1;

            GameObject reactionPanel = Instantiate(_addedReactionPrefab, reactionsField.transform);
            ChatReactionHandler chatReactionHandler = reactionPanel.GetComponentInChildren<ChatReactionHandler>();
            chatReactionHandler.SetReactionInfo(reactionSprite, counter);
            _reactionHandlers.Add(chatReactionHandler);

            _chatScript.DeselectMessage(selectedMessage);
        }
    }
}
