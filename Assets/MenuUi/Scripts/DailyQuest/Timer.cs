using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
   
    public float time = 86400;
   
    
    public Text timerText;

    void Update()
    {
        Time.timeScale = 1f;
       
        time -= Time.deltaTime;

      
        if (time <= 0)
        {
            time = 86400;
        }

      
        int hours = (int)(time / 3600);
        int minutes = (int)((time % 3600) / 60);
        int seconds = (int)(time % 60);

        
        timerText.text = "Time remaining: " + hours + " hours " + minutes + " minutes " + seconds + " seconds";
    }
}