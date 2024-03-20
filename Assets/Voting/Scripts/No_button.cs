using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class No_button : MonoBehaviour
{
    public Text textField;
    public int noAmount;

    // Start is called before the first frame update

    //etsii missä "Ei" teksti boxi sijaitsee mihin voi lisätä pisteen.
    void Start()
    {
        textField = GameObject.Find("Result_score_No").GetComponent<Text>();
    }

    //Lisää "Ei" pisteen jos painetaan Ei nappia teksti boxiin.
    public void changeText()
    {
        noAmount += 1;

        textField.text = "" + noAmount;
    }
}
