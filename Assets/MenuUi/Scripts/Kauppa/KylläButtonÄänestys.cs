using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KylläButtonÄänestyst : MonoBehaviour
{
    public GameObject item;
    public GameObject panelToBeSetInActive;


    // Tämä metodi kutsutaan, kun pelaaja painaa "Kyllä" -näppäintä
    public void YesButtonPressed()
    {
        Debug.Log("trying to closse the panel");
        // tavaran Id ulos, kun on painettu nappia, jotta menee oikein aanestykseen
        // Tässä voit vaihtaa scenen tai siirtyä valikkoon
        //SceneManager.LoadScene("ÄänestysView"); // Vaihda "UusiSceneNimi" haluamasi scenen nimeen
        Invoke("SetInactiveAfterTime", 2f);

        SettingsCarrier.Instance.MakeVotingObject();
    }

    public void NoButtonPressed()
    {
        item = null;
        SettingsCarrier.Instance.ItemVotingCanceled();
    }


    public void SetInactiveAfterTime()
    {
        panelToBeSetInActive.SetActive(false);
    }

}

    

