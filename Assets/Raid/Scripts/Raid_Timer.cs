using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raid_Timer : MonoBehaviour
{
    [SerializeField, Header("Raid Inventory ref")]
    private Raid_References raid_References;

    [Header("Timer text")]
    public TextMeshProUGUI TimerText;

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
            raid_References.RedScreen.SetActive(true);
            raid_References.EndMenu.SetActive(true);
            if(raid_References.OutOfSpace.enabled == true)
            {
                raid_References.OutOfTime.enabled = false;
            }
            else if(raid_References.OutOfSpace.enabled == false)
            {
                raid_References.OutOfTime.enabled = true;
            }
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
