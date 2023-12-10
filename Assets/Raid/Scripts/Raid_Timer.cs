using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Raid_Timer : MonoBehaviour
{
    [SerializeField, Header("Raid Inventory ref")]
    private Raid_References raid_References;

    [Header("Timer text")]
    public TextMeshProUGUI TimerText;

    [Header("Timer graphic")]
    public Image Lungs;

    [Header("Timer settings")]
    public float CurrentTime;
    public bool CountUp;

    [Header("Limit settings")]
    public bool HasLimit;
    public float TimerLimit;

    [Header("Format settings")]
    public bool HasFormat;
    public TimerFormat Format;
    private Dictionary<TimerFormat, string> TimeFormat = new Dictionary<TimerFormat, string>();

    public event Action TimeEnded;

    void Start()
    {
        TimeFormat.Add(TimerFormat.Whole, "0");
        TimeFormat.Add(TimerFormat.TenthDecimal, "0.0");
        TimeFormat.Add(TimerFormat.HundrethsDecimal, "0.00");
    }

    void Update()
    {
        CurrentTime = CountUp ? CurrentTime += Time.deltaTime : CurrentTime -= Time.deltaTime;

        if (HasLimit && ((CountUp && CurrentTime >= TimerLimit) || (!CountUp && CurrentTime <= TimerLimit)))
        {
            OnTimeEnd();
            raid_References.RedScreen.SetActive(true);
            raid_References.EndMenu.SetActive(true);
            if(raid_References.OutOfSpace.enabled || raid_References.RaidEndedText.enabled)
            {
                raid_References.OutOfTime.enabled = false;
            }
            else if(!raid_References.OutOfSpace.enabled && !raid_References.RaidEndedText.enabled)
            {
                raid_References.OutOfTime.enabled = true;
            }
            CurrentTime = TimerLimit;
            SetTimerText();
            TimerText.color = Color.red;
            enabled = false;
        }
        SetTimerText();
        SetTimerGraphic();
    }

    private void SetTimerText()
    {
        TimerText.text = HasFormat ? CurrentTime.ToString(TimeFormat[Format]) : CurrentTime.ToString();
    }
    private void SetTimerGraphic()
    {
        Lungs.fillAmount = 1.0f - (10.0f - CurrentTime) * 0.1f;
    }
    void OnTimeEnd()
    {
        TimeEnded?.Invoke();
    }

    public enum TimerFormat
    {
        Whole, TenthDecimal, HundrethsDecimal
    }
}
