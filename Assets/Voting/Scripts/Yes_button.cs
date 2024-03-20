using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Yes_button : MonoBehaviour
{
    public Text textField;
    public int yesAmount;

    // Start is called before the first frame update

    //Etsii missä on "kyllä" teksti boxi joihin pystyy lisäämään aina pisteen.
    void Start()
    {
        textField = GameObject.Find("Result_score_Yes").GetComponent<Text>();
    }
    //Laskee että jos painetaan "kyllä" se lisää kyllä tulokseen aina yhden lisää pisteitä.
    public void changeText()
    {
        yesAmount += 1;

        textField.text = "" + yesAmount;
    }


}
