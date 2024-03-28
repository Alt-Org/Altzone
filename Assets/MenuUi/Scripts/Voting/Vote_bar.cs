using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vote_bar : MonoBehaviour
{
//Todella yksinkertainen äänestys bar. Eli laitettiin vaan funktiot jolloin äänestys bar nousee ja laskee. Sitten Inspector kautta laitettiin tämä käytäntöön (kyllä nappiin laitettiin että vihreä nousee funktio päälle jne.)
    public Image yes_Votes;

    public Button yesButton;
    public Button noButton;
    public float barProgressFill = 0.1f;
    public float maxProgress = 1f;
    

    public void fillBar()
    {
            yes_Votes.fillAmount += barProgressFill;
    }
    public void decreaseBar()
    {
            yes_Votes.fillAmount -= barProgressFill;
    }
}
