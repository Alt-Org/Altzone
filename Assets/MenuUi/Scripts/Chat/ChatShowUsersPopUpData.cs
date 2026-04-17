using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static MessageReactionsHandler;

public class ChatShowUsersPopUpData : MonoBehaviour
{

    [SerializeField] private GameObject _containers;
    [SerializeField] private TextMeshProUGUI ReactionAmounText;
    [SerializeField] private GameObject _reactionField; //Turn this off if "ShowUsersPopUp" isnt active otherwise the reaction system dies

    [SerializeField] private Button[] _closeButtons; 
    [SerializeField] private GameObject ShowUsersPopUp;
    [SerializeField] private MessageObjectHandler _messageObjectHandler;
    public string _id;

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
        gameObject.SetActive(false);

    }

    void OnEnable()
    {


        _reactionField.SetActive(true);

            

        reactiontext();

       
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
