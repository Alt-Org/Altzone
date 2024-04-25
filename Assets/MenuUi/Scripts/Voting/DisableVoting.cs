using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableVoting : MonoBehaviour
{
    //Tämä pitäisi tehdä että se voisi äänestyksessä vain yhden kerran äänestää. En ole vielä sitä saanut toimimaan.

    public Button voteButton;
    public Button yesButton;
    public Button noButton;
    public GameObject votePopup;

    // Start is called before the first frame update
    void Awake()
    {
        // adding a delegate with no parameters
        voteButton.onClick.AddListener(VotingIsClicked);
        yesButton.onClick.AddListener(YesIsClicked);
        noButton.onClick.AddListener(NoIsClicked);
           
    }
    
    private void VotingIsClicked()
    {
       /* if (YesIsClicked(true) || NoIsClicked(true))
        {
            Debug.Log("Olet äänestänyt jo!");
        }
        else
        {
            Debug.Log("Äänestys painettu!");
        }
        */
        Debug.Log("Äänestys painettu!");
    }

    private void YesIsClicked()
    {
        Debug.Log("Kyllä painettu!");
    }

    private void NoIsClicked()
    {
        Debug.Log("Ei painettu!");
    }
}
