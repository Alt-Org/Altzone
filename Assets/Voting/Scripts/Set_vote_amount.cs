using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Set_vote_amount : MonoBehaviour
{
    [SerializeField] InputField inputNumber;   
    [SerializeField] CountdownTimer countdownScript;
    [SerializeField] Timer_Controller timeScript;
    [SerializeField] Reset_voting_stats resetScript;
    
    
    public Text resultText;
    public Button cancelButton;
    public Text numberVotesBox;
    public Text countdownText;
    public Button inputButton;
    public Button voteButton;
    public Button startVoting;

    //Kunnes on parempi tapa, laitan jokasen 6 nappia nyt manuaalisesti tähän script. Disabloin ne päältä pois jos starting vote if lause on toteutunut.
    public Button oneMinButton;
    public Button fifteenMinButton;
    public Button oneHourButton;
    public Button oneDayButton;
    public Button oneWeekButton;
    public Button oneMonthButton;

    

    float currentTime = 1f;

    //Ylemmät on asetettu componenttina nappiin! Ei Canvaseen.

    private void Awake()
    {
        inputNumber = GameObject.Find("Input_votes").GetComponent<InputField>();       
        numberVotesBox = GameObject.Find("NumberLimitBox").GetComponent<Text>();
        cancelButton = GameObject.Find("Close_Vote_amount_Button").GetComponent<Button>();
        countdownText = GameObject.Find("CountdownText").GetComponent<Text>();
        inputButton = GameObject.Find("Input_vote_amount_button").GetComponent<Button>();
        startVoting = GameObject.Find("Start_Voting_Button").GetComponent<Button>();

        startVoting.enabled = false;
    }

        //Tehdään funktio napin toiminnalle. Asetettiin numero vaatimus 2 - 30. Jos ei ole oikea, kysy uudestaan luku. Tekstikenttään määritetty erikseen, että se voi olla vain luku ja 2 numeron kokoinen luku.
    public void SetVoteNumber(InputField inputNumber)
    {  
        string numberVotes = inputNumber.text;
        string numberBox = numberVotesBox.text;
        //Tässä loin tilanteen jossa tyhjä input ei voi olla vaihtoehto. Ennen tuli Unityn oma Error siitä, mutta nyt ei tule edes sitä. Nyt sen sijaan tulee vaan "Ei käy valinnaksi". Myös edelleen se osaa kattoa vain 2-30 luvut!
        if(!string.IsNullOrWhiteSpace(numberVotes))
        {
           if(int.Parse(numberVotes) >= 2 && int.Parse(numberVotes) <= 30)
           {
                resultText.color = Color.green;
                resultText.text = "Äänestys määrä asetettu!"; 
                numberVotesBox.text = numberVotes; 
                Debug.Log("Äänestäjät: " + numberVotes);
               startVoting.enabled = true;
           }
        else
            {
                resultText.text = "Luku ei ole sopiva!";
                resultText.color = Color.red;
                startVoting.enabled = false;
            }
        
        }
        else
            {
                resultText.text = "Tyhjä ei käy valinnaksi!";
                resultText.color = Color.red;
                startVoting.enabled = false;
            }    
    }
    public void StartVotingButton()
    //Tämä on koodi start napille. Se laittaa jotkut napit pois päältä ja kytkee jotkut päälle. Resume() on että se aloittaa ajastimen kun se oli pausella.
    {
        if (countdownScript.currentTime > 59f)
            {
                cancelButton.enabled = true;
                inputButton.enabled = false;
                voteButton.enabled = true;
                timeScript.Resume();
                startVoting.enabled = false;
                oneMinButton.enabled = false;
                fifteenMinButton.enabled = false;
                oneHourButton.enabled = false;
                oneDayButton.enabled = false;
                oneWeekButton.enabled = false;
                oneMonthButton.enabled = false;
                resultText.text = "Äänestys on alkanut!";
                resultText.color = Color.white;

            }
        else
        //Jos aikaa ei ole laitettu, se pyytää sinua laittamaan ajan ennenkuin voit alottaa äänestyksen.
            {
                startVoting.enabled = true;
                Debug.Log("Ei ollut aikaa määritetty! Aseta aika");
                countdownScript.voteTimeInfo.text = "Ei ollut aikaa määritetty! Aseta aika";
                countdownScript.voteTimeInfo.color = Color.red;
            }
    }

}


