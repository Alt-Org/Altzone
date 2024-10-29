using System.Diagnostics;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KylläButtonÄänestyst : MonoBehaviour
{
    public GameObject panelToBeSetInActive;
    public EsineDisplay esine;

    private void OnEnable()
    {
        VotingActions.PassKauppaItem += setItem;
    }

    private void OnDisable()
    {
        VotingActions.PassKauppaItem -= setItem;
    }

    private void setItem(EsineDisplay item)
    {
        esine = item;
    }

    public void YesButtonPressed()
    {
        Invoke("SetInactiveAfterTime", 2f);

        PollManager.CreatePollObject(PollType.Kauppa, esine.name, 2, esine.items.esine, EsinePollType.Buying, esine.items.value);
    }

    public void SetInactiveAfterTime()
    {
        panelToBeSetInActive.SetActive(false);
    }
}
