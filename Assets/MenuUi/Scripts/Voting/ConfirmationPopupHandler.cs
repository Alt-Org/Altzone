using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Altzone.Scripts.Voting;
using MenuUi.Scripts.SwipeNavigation;

public class ConfirmationPopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject Background;

    private GameFurniture furniture;

    private void OnEnable()
    {
        VotingActions.PassShopItem += SetPopupActive;
    }

    private void OnDisable()
    {
        VotingActions.PassShopItem += SetPopupActive;
    }

    private void SetPopupActive(GameFurniture newFurniture)
    {
        if (Background != null) Background.SetActive(true);

        furniture = newFurniture;
    }

    public void CreatePollPopup()
    {
        if (furniture != null) PollManager.CreateFurniturePoll(FurniturePollType.Buying, furniture);
        VotingActions.ReloadPollList?.Invoke();

        FindObjectOfType<SwipeUI>(true).CurrentPage = 3;

        if (Background != null) Background.SetActive(false);
    }
}
