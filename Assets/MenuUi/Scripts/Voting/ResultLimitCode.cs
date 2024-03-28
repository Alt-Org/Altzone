using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//AINA KUN TEKEE UI JUTTUJA, MUISTA UNITYENGINE.UI LISÄTÄ TÄNNE YLÖS!

public class ResultLimitCode : MonoBehaviour
{
    [SerializeField] CountdownTimer script;
    

   public Text resultYES;
   public Text resultNO;
   public Text countdownText;

   public Button newButton;
   public Text numberVotesBox;
   string newLimit;
   
    

    // Start is called before the first frame update

    //Etsii ja määrittää missä nämä objectit on joita tarvitaan määrittämään onko 10 äänestystä.
    void Start()
    {      
        newButton.enabled = true;
        resultYES = GameObject.Find("Result_score_Yes").GetComponent<Text>();
        resultNO = GameObject.Find("Result_score_No").GetComponent<Text>();
        countdownText = GameObject.Find("CountdownText").GetComponent<Text>();
      

       
    }

    // Update is called once per frame

    //Jos tuloksia EI ja KYLLÄ on yhteensä valittu input määrä, sammuta ÄÄNESTYS nappi. Lisäsin elsen että näyttää sitten vain timen normaalisti koska koodi näytti oudolta ilman "else".
    void Update()
    {
        
        newLimit = numberVotesBox.text;
      
       
        if(int.Parse(resultYES.text) + int.Parse(resultNO.text) >= int.Parse(newLimit) && int.Parse(newLimit) >= 2)
        {
            newButton.enabled = false;
            script.currentTime = 0;
            
        }
        else
        {
            script.DisplayTime(script.currentTime);
        }
        
    }
}
