using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static MessageReactionsHandler;
using MenuUi.Scripts.AvatarEditor;
using System;
using Altzone.Scripts.Chat;

public class ChatShowUsersPopUpData : MonoBehaviour
{

    [SerializeField] private GameObject _containers;
    [SerializeField] private TextMeshProUGUI ReactionAmounText;
    [SerializeField] private GameObject _reactionField; //Turn this off if "ShowUsersPopUp" isnt active otherwise the reaction system dies

    [SerializeField] private Button[] _closeButtons; 
    [SerializeField] private GameObject ShowUsersPopUp;
    [SerializeField] private MessageObjectHandler _messageObjectHandler;


    [SerializeField] public List<UserReactionData> _UserReaction;



    [SerializeField] private GameObject ReactionObject;

    //[SerializeField] private TextLanguageSelectorCaller _reactionAmount;
    private void Start()
    {
        reactiontext();
        foreach (Button button in _closeButtons)
        {
            button.onClick.AddListener(ClosePopup);
        }
    }

    public void ClosePopup()
    {
        gameObject.transform.SetParent(ShowUsersPopUp.transform);
        gameObject.SetActive(false);

    }

    void OnEnable()
    {
        _reactionField.SetActive(true);
        reactiontext();
    }


    //Incane user leaves the chat
    private void OnDisable()
    {
        ClosePopup();
    }


    public void reactiontext()
    {
        int activeChildren = 0;
        foreach (Transform container in _containers.transform)
        {
            if (container.gameObject.activeInHierarchy)
            {
                activeChildren++;
            }

        }

        ReactionAmounText.text = $"{activeChildren} reaktiota";

        //_reactionAmount.SetText(SettingsCarrier.Instance.Language, new string[1] { activeChildren.ToString() });
    }
}

[Serializable]
public class UserReactionData
{
    public AvatarFaceLoader _avatar;
    public TextMeshProUGUI _name;
    public string _id;

    //public Sprite Sprite;
    //public Mood Mood;

}
