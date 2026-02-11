using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ServerChatMessage;

public class MessageObjectHandler : MonoBehaviour
{
    [SerializeField] private AvatarFaceLoader _avatar;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _date;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _addReactionsControls;
    [SerializeField] private GameObject _reactionsPanel;


    [Header("Base Message")]
    public Vector2 _baseMessageBankerSize;
    public RectTransform _baseMessageSize;
    [SerializeField] private ChatMessageScript backgroundSize;
    [SerializeField] private GameObject reactionField;
    public GameObject _reactionSize;
    public GameObject _expandedReactionSize;
    [SerializeField] private Vector2 _vectorReactionSize;
    [SerializeField] private Vector2 _vectorExpandedReactionSize;   



    private string _id;
    private Image _image;
    private Action<MessageObjectHandler> _selectMessageAction;

    public GameObject ReactionsPanel { get => _reactionsPanel;}
    [SerializeField] private GameObject ReactionObject;

    public string Id { get => _id;}

    // Start is called before the first frame update
    void Start()
    {
        _button.onClick.AddListener(SetMessageActive);
        _image = _button.GetComponent<Image>();
        Chat.OnSelectedMessageChanged += SetMessageInactive;
        ChatChannel.OnReactionReceived += UpdateReactions;

        _vectorReactionSize = new Vector2(_baseMessageSize.sizeDelta.x, _baseMessageSize.sizeDelta.y + _reactionSize.GetComponent<RectTransform>().sizeDelta.y);
        _vectorExpandedReactionSize = new Vector2(_baseMessageSize.sizeDelta.x, _baseMessageSize.sizeDelta.y + _expandedReactionSize.GetComponent<RectTransform>().sizeDelta.y);
    }

    ///Changes the Basemessages size
    public void SizeCall()
    {
        //adds extra size if there reactions have been put or not
        float extraPadding;
        if (reactionField.transform.childCount > 0)
            extraPadding = 100;
        else
            extraPadding = 0f;

        //Checks if reaction panel is active and checks which reaction pannel is on
        if (_reactionSize.activeSelf)
            if(_expandedReactionSize.activeSelf)
                _baseMessageSize.sizeDelta = new Vector2(_vectorExpandedReactionSize.x, Mathf.Max(150, _baseMessageBankerSize.y + _vectorExpandedReactionSize.y + extraPadding));
            else
                _baseMessageSize.sizeDelta = new Vector2(_vectorReactionSize.x, Mathf.Max(150, _baseMessageBankerSize.y + _vectorReactionSize.y + extraPadding));

        //reverts back to orignal
        else
        _baseMessageSize.sizeDelta = new Vector2(_baseMessageBankerSize.x, Mathf.Max(150, _baseMessageBankerSize.y + extraPadding));

    }

    private void OnDestroy()
    {
        Chat.OnSelectedMessageChanged -= SetMessageInactive;
    }

    public void SetMessageInfo(string messageText, AvatarVisualData avatarData, Action<MessageObjectHandler> selectMessageAction)
    {
        if (avatarData != null) _avatar.UpdateVisuals(avatarData);
        _text.text = messageText;
        _selectMessageAction = selectMessageAction;
    }
    public void SetMessageInfo(ChatMessage message, Action<MessageObjectHandler> selectMessageAction)
    {
        if (message.Avatar != null) _avatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(message.Avatar));
        _id = message.Id;
        _text.text = message.Message;
        _selectMessageAction = selectMessageAction;
        _name.text = message.Username;
        _time.text = $"{message.Timestamp.Hour}:{message.Timestamp.Minute:D2}";
        _date.text = $"{message.Timestamp.Day}/{message.Timestamp.Month}/{message.Timestamp.Year}";


        ///Activates and Deactivates the "AddChatMessageReactions" as it has "Message Reactions Handler" that is needed to be on
        ReactionObject.gameObject.SetActive(true);
        foreach (var reactionData in message.Reactions)
        {
            ReactionChatCall(reactionData);
        }
        ReactionObject.gameObject.SetActive(false);
    }

    public void SetPreviewMessageInfo(ChatMessage message)
    {
        if (message.Avatar != null) _avatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(message.Avatar));
        _id = message.Id;
        _text.text = message.Message;
        _name.text = message.Username;
        _button.enabled = false;
    }

    private void SetMessageActive()
    {
        if (_image != null)
        {
            _image.color = Color.gray;
        }
        _addReactionsControls.SetActive(true);

        _selectMessageAction.Invoke(this);
        SizeCall();
    }

    private void SetMessageInactive(MessageObjectHandler handler)
    {
        if (handler == this) return;

        if (_image != null)
        {
            _image.color = Color.white;
        }
        _addReactionsControls.SetActive(false);
        SizeCall();
    }

    public void SetMessageInactive()
    {
        _selectMessageAction.Invoke(null);
    }

    //Refreshes the reactions if there is any
    public void ReactionChatCall(ServerReactions EmojiId)
    {

        //Gets the set data we need to get to import saved reactions
        MessageReactionsHandler ChildsScript = ReactionObject.GetComponent<MessageReactionsHandler>();

        ChildsScript.AddReaction(EmojiId, (Mood)Enum.Parse(typeof(Mood), EmojiId.emoji), _id, true);
        
    }

    private void UpdateReactions(ChatChannelType chatChannelType, ChatMessage message)
    {
        if (chatChannelType != ChatListener.Instance.ActiveChatChannel) return;
        if (message.Id == null || _id != message.Id) return;

        //Gets the set data we need to get to import saved reactions
        MessageReactionsHandler ChildsScript = ReactionObject.GetComponent<MessageReactionsHandler>();

        foreach (ServerReactions reactions in message.Reactions)
        {
            ChildsScript.AddReaction(reactions, (Mood)Enum.Parse(typeof(Mood), reactions.emoji), _id, true);
        }
    }
}
