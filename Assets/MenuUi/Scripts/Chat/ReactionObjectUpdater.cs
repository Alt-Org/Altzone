using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using UnityEngine;
using UnityEngine.UI;

public class ReactionObjectUpdater : MonoBehaviour
{
    [SerializeField] private MessageReactionsHandler _messageList;
    [SerializeField] private RectTransform _commonReaction;
    [SerializeField] private RectTransform _AllReactions;

    private void Awake()
    {
        //Fetches the script what we need not certain if this is the best way to do it 100% but it works
        _messageList = GetComponentInParent<MessageReactionsHandler>();
    }

    private void Update()
    {
        //First checks if the gameobject is active so it wont update for no reason
        if(_commonReaction.gameObject.activeSelf)

            //Fetch the objects childs and gets their needed data
        foreach (Transform child in _commonReaction)
        {
                Mood mood = child.GetComponent<ReactionObjectHandler>().Mood;
                //goes over the list
            foreach (var r in _messageList._reactionList)
            {
                    //Then checks if they match
                if (mood == r.Mood)
                        //Switches depending if the set mood is selected or not
                    child.gameObject.SetActive(!r.Selected);
            }
        }

        ///     /\             
        ///    /  \  Same as the above but for all the reactions instead
        ///     ||
        if (_AllReactions.gameObject.activeSelf)

            foreach (Transform child in _AllReactions)
            {
                Mood mood = child.GetComponent<ReactionObjectHandler>().Mood;
                    foreach (var r in _messageList._reactionList)
                {
                    if (mood == r.Mood)
                        child.gameObject.SetActive(!r.Selected);
                }
            }




    }
}
