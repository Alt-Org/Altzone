using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Winner_calculator : MonoBehaviour
{
    //Määritetään käytettävät ja tarvittavat asiat. Otin voteButton pois, koska teenki tulokset toimimaan kellon mukaisesti enkä napin alempana.
    [SerializeField] Reset_voting_stats resetScript;
    [SerializeField] Vote_amount voteAmountScript;

    public Text resultYES;
    public Text resultNO;
    public Button voteButton;
    public Text countdownTimer;
    public Text winnerResult;
    

    //Tarviin näitä erikseen jotta teen laskutoimitukset. scoreAmount on että se laskee molemmat pisteet yes and no yhteen. winningScore tekee lasku toimituksen mikä on piste määrä prosentteina että se voitto tapahtuu, joka on 70%.
    public float scoreAmount;
    public float winningScore;


    // Start is called before the first frame update
    void Start()

    //Tämä koodi etsii gameobjectin josta puhutaan ja se tietää mitä tarkoitan.
    {
        
        resultYES = GameObject.Find("Result_score_Yes").GetComponent<Text>();
        resultNO = GameObject.Find("Result_score_No").GetComponent<Text>();
        countdownTimer = GameObject.Find("CountdownText").GetComponent<Text>();
        winnerResult = GameObject.Find("WinnerText").GetComponent<Text>();
       

        voteButton.interactable = false;
     //   resetScript.reset.enabled = false;


    }

    // Update is called once per frame
    //Tämä laskee pisteet ja sen tilanteen. Taisin saada sen jopa toimimaan.
    void Update()
    {
        //Tässä määritän float muuttujat, että niillä on nyt tarkoitus ja arvot. Sanoin että scoreAmount on KYLLÄn ja EIn yhdistelmä ja se siis on pisteiden määrä. winningScore määritin, että se tekee laskutoimituksen. Hyödyntäen scoreAmount, se laskee prosentin 70% perusteella että mikä on kokonaispisteistä 70%.  
        scoreAmount = int.Parse(resultYES.text) + int.Parse(resultNO.text);
        winningScore = scoreAmount / 100 * 70;

 
      // string numberVotes = inputNumber.text;

    //Tässä on winnerResult tekstiboxin tekstimäärittelyt. Samalla osaa katsoa kuka voitti. Taisin saada myös toimimaan tuon 70% mutta en oo varma siitä pitää kysyä.  

        if (countdownTimer.text == "0:00:00" && int.Parse(resultNO.text) > int.Parse(resultYES.text))
            {
                winnerResult.text = "EI voitti " + resultNO.text;
            }
        else if (countdownTimer.text == "0:00:00" && int.Parse(resultNO.text) == int.Parse(resultYES.text) && scoreAmount >= 2)
            {
                winnerResult.text = "Tasapeli, tuotetta ei osteta.";
            }
        else if (countdownTimer.text == "0:00:00" && int.Parse(resultNO.text) < int.Parse(resultYES.text) && Mathf.Round(winningScore) <= int.Parse(resultYES.text) && scoreAmount > 3)
            {
                //winnerResult.text = "KYLLÄ voitti " + resultYES.text;
                winnerResult.text = "TESTI KYLLÄ " + winningScore + " " + int.Parse(resultYES.text);
            }
        else if (countdownTimer.text == "0:00:00" && int.Parse(resultNO.text) < int.Parse(resultYES.text) && scoreAmount > 3)
            {
                //winnerResult.text = "EI voitti. KYLLÄ ääniä liian vähän";
                winnerResult.text = "TESTI EI " + winningScore + " " + int.Parse(resultYES.text) + " " + scoreAmount;
            }
            //NÄMÄ KAKSI ALEMPAA ELSE IF ON YRITYS SAADA 3 LIMIT ALASPÄIN ETTÄ ÄÄNIÄ ON VAIN KYLLÄÄ ETTÄ HYVÄKSYY! Update: se taitaa toimia nyt!
        else if (countdownTimer.text == "0:00:00" && voteAmountScript.newLimit == "3" && int.Parse(resultYES.text) == 3 || voteAmountScript.newLimit == "2" && int.Parse(resultYES.text) == 2)
        {
            winnerResult.text = "KYLLÄ voitti! kaikki sanoi KYLLÄ";
        }
        else if (countdownTimer.text == "0:00:00" && voteAmountScript.newLimit == "3" && int.Parse(resultYES.text) < 3 && scoreAmount == 3)
        {
            winnerResult.text = "EI voitti! Kaikki ei sanonut KYLLÄ";
        }
        else
            {
                winnerResult.text = "Äänestä!";
            }

    }
}
