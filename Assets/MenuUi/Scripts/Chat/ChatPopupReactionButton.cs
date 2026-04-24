using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MessageReactionsHandler;

public class ChatPopupReactionButton : MonoBehaviour
{
    [SerializeField] private GameObject AllReaction;
    [SerializeField] private GameObject SelectedReaction;

    //turns off the All reaction and shows back again the selected reactions for the "ShowUsersPopUp"
    void OnEnable()
    {
        Button[] _buttons = GetComponentsInChildren<Button>();

        foreach (var button in _buttons)
        {
            button.onClick.AddListener((() =>
            {
                AllReaction.SetActive(false);
                SelectedReaction.SetActive(true);
            }));
        }
    }
}
