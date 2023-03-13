using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raid_Timer : MonoBehaviour
{
    [Header("Component")]
    public TextMeshProUGUI TimerText;

    [Header("Timer settings")]
    public float CurrentTime;
    public bool CountUp;

    [Header("Limit settings")]
    public bool HasLimit;
    public float TimerLimit;

    [Header("Gormat settings")]
    public bool HasFormat;
    public TimerFormat Format;
    private Dictionary<TimerFormat, string> TimeFormat = new Dictionary<TimerFormat, string>();

    void Start()
    {
        TimeFormat.Add(TimerFormat.Whole, "0");
        TimeFormat.Add(TimerFormat.TenthDecimal, "0.0");
        TimeFormat.Add(TimerFormat.HundrethsDecimal, "0.00");
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime = CountUp ? CurrentTime += Time.deltaTime : CurrentTime -= Time.deltaTime;

        if (HasLimit && ((CountUp && CurrentTime >= TimerLimit) || (!CountUp && CurrentTime <= TimerLimit)))
        {
            CurrentTime = TimerLimit;
            SetTimerText();
            TimerText.color = Color.red;
            enabled = false;
        }
        SetTimerText();
    }

    private void SetTimerText()
    {
        TimerText.text = HasFormat ? CurrentTime.ToString(TimeFormat[Format]) : CurrentTime.ToString();
    }

    public enum TimerFormat
    {
        Whole, TenthDecimal, HundrethsDecimal
    }
}
