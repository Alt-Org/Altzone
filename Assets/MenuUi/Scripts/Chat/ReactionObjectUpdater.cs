using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using UnityEngine;
using UnityEngine.UI;

public class ReactionObjectUpdater : MonoBehaviour
{
    [SerializeField] private RectTransform _commonReaction;
    [SerializeField] private RectTransform _allReactions;

    //Goes on effect when user goes off from
    private void OnDisable()
    {
        _commonReaction.gameObject.SetActive(true);
        _allReactions.gameObject.SetActive(false);
    }
}
