using System.Security.AccessControl;
using Altzone.Scripts.Voting;
using MenuUi.Scripts.SwipeNavigation;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KylläButtonÄänestyst : MonoBehaviour
{
    public GameObject panelToBeSetInActive;
    private GameFurniture furniture;

    private void Awake()
    {
        VotingActions.PassShopItem += setItem;
        panelToBeSetInActive.SetActive(false);
    }

    private void setItem(GameFurniture newFurniture)
    {
        furniture = newFurniture;
        UnityEngine.Debug.Log(newFurniture.ToString());
    }

    public void YesButtonPressed()
    {
        Invoke("SetInactiveAfterTime", 2f);

        if (furniture != null) PollManager.CreateFurniturePoll(FurniturePollType.Buying, furniture);
        VotingActions.ReloadPollList?.Invoke();

        FindObjectOfType<SwipeUI>(true).CurrentPage = 3;
    }

    public void SetInactiveAfterTime()
    {
        panelToBeSetInActive.SetActive(false);
    }
}
