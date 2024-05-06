using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveValuesScript : MonoBehaviour
{
    public Text resultYES;
    public Text resultNO;
    public Text timerTEXT;

    public string yes;
    public string no;
    public string timer;

    void Start()
    {
    //Etsii missä on nämä tiedot jossa näkyy kyllä ja ei.

        resultYES = GameObject.Find("Result_score_Yes").GetComponent<Text>();
        resultNO = GameObject.Find("Result_score_No").GetComponent<Text>();
        timerTEXT = GameObject.Find("CountdownText").GetComponent<Text>();
                
    }

    //Ottaa tekstibox tiedoista arvot talteen muuttujiin joita voi näyttää myöhemmin äänestyslistassa (kokeilu)
    public void SaveValues()
    {
       yes = resultYES.text;
       no = resultNO.text;
       timer = timerTEXT.text;

       Debug.Log("arvot tallennettu! Kyllä: " + yes + ", Ei: " + no + ", aika: " + timer);
    }

    //Tämä laittaa ne näkyviin ne uudet arvot siihen listaan.
    public void SetValues()
    {
        resultYES.text = yes;
        resultNO.text = no;
        timerTEXT.text = timer;

        Debug.Log("Uudet arvot asetettu! Kyllä: " + yes + ", Ei: " + no + ", aika: " + timer);
    
    }
}
