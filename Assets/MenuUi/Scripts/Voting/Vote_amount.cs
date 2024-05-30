using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vote_amount : MonoBehaviour

//Lisää tarvittavat komponentit HUOM! Public termi on siksi, että voin käyttää sitä toisessa koodissa kun se script on yhdistetty componenttiin.
{
  public Text resultYES;
   public Text resultNO;
   public Text Votes;
   int Votes_total;
   public string newLimit;

   public Text numberVotesBox;

  [SerializeField] InputField inputNumber;

//variables voi käyttää muualla kuin siinä olevassa scriptissä.
  [SerializeField] Set_vote_amount script;


  
    // Start is called before the first frame update

    //Etsii ja määrittää mitä tarvitaan. Tarvitaan verrata piste tilannetta.
    void Start()
    {      
        
        resultYES = GameObject.Find("Result_score_Yes").GetComponent<Text>();
        resultNO = GameObject.Find("Result_score_No").GetComponent<Text>();

       
    }

    // Update is called once per frame

    //Laskee ja lisää äänestysmäärään kun on annettu 1 ääni joka kerta kun EI ja KYLLÄ tuloksiin lisääntyy numero. Se katsoo sen siitä tekstibox ruudun perusteella ei napin "äänestä" painahduksien perusteella.
    void Update()
    {
         newLimit = numberVotesBox.text;
         int temp = Votes_total;

   //   if (int.Parse(resultYES.text) > Votes_total || int.Parse(resultNO.text) > Votes_total || int.Parse(resultYES.text) < Votes_total || int.Parse(resultNO.text) < Votes_total || Votes_total == int.Parse(resultNO.text))
      if (int.Parse(resultYES.text) != Votes_total || int.Parse(resultNO.text) != Votes_total || Votes_total == int.Parse(resultNO.text))
        {
      //  Votes_total = Mathf.Max(int.Parse(resultYES.text) + int.Parse(resultNO.text));
          Votes_total = int.Parse(newLimit);
          temp = Votes_total -(int.Parse(resultYES.text) + (int.Parse(resultNO.text)));              
          Votes.text = temp.ToString() + "/" + newLimit.ToString();
            
            
          //Debug.Log("Testi: " + script.numberVotesBox);
        }
      //Tämä pitää siistiä tämä koodipätkä selkeämmäksi. Nyt se siis vertaa että "Jos result.NO on < kuin Votes_total TAI resultYES < kuin Votes_total TAI resultYES on < Votes_total TAI resultNO < Votes_total TAI Votes_total on sama kuin resultNO... Vähän turhan pitkä IF lause mutta se toimii
        
    }
}
