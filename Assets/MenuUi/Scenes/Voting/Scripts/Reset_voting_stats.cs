using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reset_voting_stats : MonoBehaviour
{
    //Kysyin tässä jokaisen scriptin jotta minä voin helposti access jokaiseen muuttujaan joka pitää nollata.
    [SerializeField] CountdownTimer timeScript;
    [SerializeField] Winner_calculator winnerScript;
    [SerializeField] Set_vote_amount voteAmountScript;
    [SerializeField] Yes_button yesButtonScript;
    [SerializeField] No_button noButtonScript;
    
    //Määritettiin erikseen nämä objectit jotka voi laittaa inspector kautta mukaan tähän koodiin.
    public Button reset;
    public Text resultYES;
    public Text resultNO;
    public Button amountSetButton;
    public Button startVoting;
    public Image voteBarGreen;


    // Start is called before the first frame update
    void Start()
    {
    //Etsii missä on nämä tiedot jossa näkyy kyllä ja ei.

        resultYES = GameObject.Find("Result_score_Yes").GetComponent<Text>();
        resultNO = GameObject.Find("Result_score_No").GetComponent<Text>();        
    }

    // Update is called once per frame
    public void ResetVoting()
    //Tämä koodi nollaa kaiken. Se laittaa asiat nollaan, pysäyttää ajan jotta se ei mee yli 0 jne. Myös nollaa mittarin joka näytti äänestyksen tilanteen. Laittaa myös asetuksista kaikki päälle mitä pitää eli uudestaan kyselyt ajasta ja uudesta äänestys rajasta.
    {
        resultYES.text = "0";
        resultNO.text = "0";
        winnerScript.scoreAmount = int.Parse(resultYES.text) + int.Parse(resultNO.text);
        timeScript.currentTime = 0f;
        timeScript.timeText.color = Color.white;
        Time.timeScale = 0f;
        amountSetButton.enabled = true;
        winnerScript.winningScore = winnerScript.scoreAmount / 100 * 70;
        voteAmountScript.numberVotesBox.text = "0";
        yesButtonScript.yesAmount = 0;
        noButtonScript.noAmount = 0;
        timeScript.voteTimeInfo.text = "Aseta äänestys aika";
        voteAmountScript.resultText.color = Color.white;
        voteAmountScript.resultText.text = "Äänestäjien määrä?"; 
        startVoting.enabled = false;
        voteAmountScript.oneMinButton.enabled = true;
        voteAmountScript.fifteenMinButton.enabled = true;
        voteAmountScript.oneHourButton.enabled = true;
        voteAmountScript.oneDayButton.enabled = true;
        voteAmountScript.oneWeekButton.enabled = true;
        voteAmountScript.oneMonthButton.enabled = true;
        voteBarGreen.fillAmount = 0.5f;       

        Debug.Log("Uudet arvot: ResultYES: " + resultYES.text + " ResultNO: " + resultNO.text + " scoreAmount: " + winnerScript.scoreAmount);
    }
}
