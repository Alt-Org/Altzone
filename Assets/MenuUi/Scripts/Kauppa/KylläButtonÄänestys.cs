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

    public void setItem(GameFurniture newFurniture)
    {
        furniture = newFurniture;
        //UnityEngine.Debug.Log(newFurniture.ToString());
    }

    public void YesButtonPressed()
    {
        if (furniture != null) PollManager.CreateBuyFurniturePoll(FurniturePollType.Buying, furniture, "0");
        VotingActions.ReloadPollList?.Invoke();

        FindObjectOfType<SwipeUI>(true).CurrentPage = 3;

        panelToBeSetInActive.SetActive(false);
    }
}
