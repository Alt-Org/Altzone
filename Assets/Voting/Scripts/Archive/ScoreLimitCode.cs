using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreLimitCode : MonoBehaviour
{

    public Text result;

    public Button newButton;
    

    // Start is called before the first frame update
    void Start()
    {      
        newButton.enabled = true;
        result = GameObject.Find("Result_score").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
       
        newButton.enabled = (int.Parse(result.text) < 10);
        
    }
}
