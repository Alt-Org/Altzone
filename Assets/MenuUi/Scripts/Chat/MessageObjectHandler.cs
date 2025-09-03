using System;
using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageObjectHandler : MonoBehaviour
{
    [SerializeField] private AvatarFaceLoader _avatar;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _messageControls;
    private Image _image;
    private Action<MessageObjectHandler> _selectMessageAction;

    // Start is called before the first frame update
    void Start()
    {
        _button.onClick.AddListener(SetMessageActive);
        _image = _button.GetComponent<Image>();
        Chat.OnSelectedMessageChanged += SetMessageInactive;
    }

    public void SetMessageInfo(string messageText, AvatarVisualData avatarData, Action<MessageObjectHandler> selectMessageAction)
    {
        if (avatarData != null) _avatar.UpdateVisuals(avatarData);
        _text.text = messageText;
        _selectMessageAction = selectMessageAction;
    }

    private void SetMessageActive()
    {
        if (_image != null)
        {
            _image.color = Color.gray;
        }
        _messageControls.SetActive(true);

        _selectMessageAction.Invoke(this);
    }

    private void SetMessageInactive(MessageObjectHandler handler)
    {
        if (handler == this) return;

        if (_image != null)
        {
            _image.color = Color.white;
        }
        _messageControls.SetActive(false);
    }

}
