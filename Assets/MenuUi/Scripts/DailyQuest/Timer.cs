using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Timer : MonoBehaviour
{
    private DateTime midnight;
    public Text timerText;

    void Start()
    {
     
        DateTime now = DateTime.Now;
       
        midnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0).AddDays(1);
    }

    void Update()
    {
        Time.timeScale = 1f;
      
        TimeSpan timeRemaining = midnight - DateTime.Now;
        
        if (DateTime.Now >= midnight)
        {
            
            midnight = midnight.AddDays(1);
        }

        int hours = (int)(timeRemaining.TotalHours);
        int minutes = (int)(timeRemaining.TotalMinutes % 60);
        int seconds = (int)(timeRemaining.TotalSeconds % 60);

        timerText.text = hours + " hours " + minutes + " minutes " + seconds + " seconds";
    }
}