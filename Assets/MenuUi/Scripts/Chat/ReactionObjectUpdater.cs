using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using UnityEngine;
using UnityEngine.UI;

public class ReactionObjectUpdater : MonoBehaviour
{
    [SerializeField] private RectTransform _selectedReaction;
    [SerializeField] private RectTransform _allReactions;

    //Goes on effect when user goes off from
    public void ReactionUpdate()
    {
        _selectedReaction.gameObject.SetActive(true);
        _allReactions.gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        ReactionUpdate();
    }
}
