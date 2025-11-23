using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] public Vector2 _BaseMessageBankerSize;
    [SerializeField] public RectTransform _BaseMessageSize;
    [SerializeField] private ChatMessageScript BackgroundSize;
    [SerializeField] private GameObject ReactionField;
    public GameObject ReactionSize;
    public GameObject ExpandedReactionSize;
    public Vector2 rect;
    public Vector2 rect2;

    private string _id;
    private Image _image;
    private Action<MessageObjectHandler> _selectMessageAction;

    public GameObject ReactionsPanel { get => _reactionsPanel;}
    public string Id { get => _id;}

    // Start is called before the first frame update
    void Start()
    {
        _button.onClick.AddListener(SetMessageActive);
        _image = _button.GetComponent<Image>();
        Chat.OnSelectedMessageChanged += SetMessageInactive;


        rect = new Vector2(_BaseMessageSize.sizeDelta.x, _BaseMessageSize.sizeDelta.y + ReactionSize.GetComponent<RectTransform>().sizeDelta.y);
        rect2 = new Vector2(_BaseMessageSize.sizeDelta.x, _BaseMessageSize.sizeDelta.y + ExpandedReactionSize.GetComponent<RectTransform>().sizeDelta.y);
    }

    ///Changes the Basemessages size
    public void sizeCall()
    {
        //adds extra size if there reactions have been put or not
        float extraPadding;
        if (ReactionField.transform.childCount > 0)
            extraPadding = 100;
        else
            extraPadding = 0f;

        //Checks if reaction panel is active and checks which reaction pannel is on
        if (ReactionSize.activeSelf)
            if(ExpandedReactionSize.activeSelf)
                _BaseMessageSize.sizeDelta = new Vector2(rect2.x, Mathf.Max(150, _BaseMessageBankerSize.y + rect2.y + extraPadding));
            else
                _BaseMessageSize.sizeDelta = new Vector2(rect.x, Mathf.Max(150, _BaseMessageBankerSize.y + rect.y + extraPadding));

        //reverts back to orignal
        else
        _BaseMessageSize.sizeDelta = new Vector2(_BaseMessageBankerSize.x, Mathf.Max(150, _BaseMessageBankerSize.y + extraPadding));

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
        sizeCall();
    }

    private void SetMessageInactive(MessageObjectHandler handler)
    {
        if (handler == this) return;

        if (_image != null)
        {
            _image.color = Color.white;
        }
        _addReactionsControls.SetActive(false);
        sizeCall();
    }

    public void SetMessageInactive()
    {
        _selectMessageAction.Invoke(null);
    }

}
