using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] Set_vote_amount voteAmountScript;
    [SerializeField] GameObject reset;

    //Määritetään aloitus aika ja mistä aika alkaa
    public float currentTime;
    public float startingTime;

    //määritetään countdown teksti ja uusi nappi
    public Text countdownText;
    public Button newButton;
    public Text timeText;
    public Text voteTimeInfo;

    public Text resultYES;
    public Text resultNO;

    // Start is called before the first frame update
    void Start()
    {
        resultYES = GameObject.Find("Result_score_Yes").GetComponent<Text>();
        resultNO = GameObject.Find("Result_score_No").GetComponent<Text>();


        //Pysäyttää ajan paikoilleen. tarkoitus on että kello ei liiku enneku äänestys on asetettu kulkemaan. 
         Time.timeScale = 0f;

        //nappi päälle ja aloitus ajan määritys.
        newButton.interactable = true;
        currentTime = startingTime;
        
    }

    // Update is called once per frame
    void Update()
    {
       
        //laskee numerot sekunnin mukaan. 1 numero alaspäin ja ilman millisekuntteja.
        currentTime -=1 * Time.deltaTime;
        countdownText.text = currentTime.ToString("0");

    //Värien kanssa pelleilyä. Jätän tähän jos tarvii myöhemmin.

      //  if (currentTime <= 20)

       // {
       //     countdownText.color = Color.yellow;
      //  }

     //   if (currentTime <= 10)

     //   {
      //      countdownText.color = Color.red;
      //  }

        if (currentTime <= 0)
        {
            DisplayTime(currentTime = 0);
            newButton.interactable = false;
            reset.SetActive(false);
        }

        else if (currentTime > 1)
            {
                DisplayTime(currentTime);
            }
        
    //Tähän on määritetty laskuri miten se näyttää kellon. 
    }
    public void DisplayTime(float timeToDisplay)
    {
        float hours = Mathf.FloorToInt(timeToDisplay / 3600) % 24; 
        float minutes = Mathf.FloorToInt(timeToDisplay / 60) % 60; 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:0}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    //Tässä on määritetty nappeja eri ajoille. Näitä asetetaan napin kautta nappiin. Päivittää myös tekstin mitä tuli painettua.
    public void MinTimer(float timeToDisplay)
    {
        currentTime = 60f;
        voteTimeInfo.text = "Äänestys aika asetettu: 1min";
        voteTimeInfo.color = Color.white;
    }
     public void fifteenMinTimer(float timeToDisplay)
    {
        currentTime = 900f;
        voteTimeInfo.text = "Äänestys aika asetettu: 15min";
        voteTimeInfo.color = Color.white;
    }
    //HUOM! Tunti toimii, yli tunnin kohdalla ei toimi!
     public void HourTimer(float timeToDisplay)
    {
        currentTime = 3600f;
        voteTimeInfo.text = "Äänestys aika asetettu: 1h";
        voteTimeInfo.color = Color.white;
    }
      public void OneDayTimer(float timeToDisplay)
    {
       currentTime = 86400f;
       voteTimeInfo.text = "Äänestys aika asetettu: 1 päivä";
       voteTimeInfo.color = Color.white;
    }
}
