using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class Raid_Timer : MonoBehaviour
{
    [SerializeField, Header("Raid Inventory ref")]
    private Raid_References raid_References;

    [Header("Timer texts")]
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI StartTimerText;

    [Header("Timer settings")]
    public float CurrentTime;
    public bool CountUp;
    private bool timerActive;
    private bool started;

    [Header("Start Timer settings")]
    public float TimeUntilStart;
    private bool startTextShown;

    [Header("Limit settings")]
    public bool HasLimit;
    public float TimerLimit;

    [Header("Format settings")]
    public bool HasFormat;
    public TimerFormat Format;
    private Dictionary<TimerFormat, string> TimeFormat = new Dictionary<TimerFormat, string>();

    public event Action TimeEnded;
    public ExitRaid exitRaid;

    private Color startColor = Color.white;
    private Color endColor = Color.red;
    [SerializeField]
    private float duration;

    void Start()
    {
        TimeFormat.Add(TimerFormat.Whole, "0");
        TimeFormat.Add(TimerFormat.TenthDecimal, "0.0");
        TimeFormat.Add(TimerFormat.HundrethsDecimal, "0.00");

        if (exitRaid == null || !exitRaid.gameObject.activeInHierarchy)
        {
            exitRaid = FindActiveExitRaid();
        }

        if (exitRaid != null)
        {
            exitRaid.ExitedRaid += RaidExited;
        }

        SetStartTimerVisualState(false);
    }

    void Update()
    {
        if (timerActive)
        {
            CurrentTime = CountUp ? CurrentTime += Time.deltaTime : CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 5)
            {
                float t = Mathf.PingPong(Time.time / duration, 1f);
                Color lerped = Color.Lerp(startColor, endColor, t);

                TimerText.color = lerped;
            }
            if (HasLimit && ((CountUp && CurrentTime >= TimerLimit) || (!CountUp && CurrentTime <= TimerLimit)))
            {
                OnTimeEnd();
                if (exitRaid == null || !exitRaid.gameObject.activeInHierarchy)
                {
                    exitRaid = FindActiveExitRaid();
                }

                if (exitRaid != null)
                {
                    exitRaid.EndRaid(ExitRaid.RaidEndReason.OutOfTime);
                }
                else
                {
                    Debug.LogError("Raid timer ended, but no active ExitRaid was found.");
                }
                CurrentTime = TimerLimit;
                SetTimerText();
                TimerText.color = Color.red;
                enabled = false;
            }
        }
        else
        {
            if (!started)
            {
                TimeUntilStart -= Time.deltaTime;
                if (TimeUntilStart <= -1f)
                {
                    StartTimer();
                }
                else if (TimeUntilStart <= 0f)
                {
                    ShowStartText();
                }
                else
                {
                    StartTimerText.text = Mathf.CeilToInt(TimeUntilStart).ToString();
                }
            }
        }
        
        SetTimerText();
    }
    public void StartTimer()
    {
        StartTimerText.gameObject.transform.parent.gameObject.SetActive(false);
        if (!timerActive && !started)
        {
            timerActive = true;
            started = true;
        }
            
    }

    private void ShowStartText()
    {
        if (startTextShown)
        {
            return;
        }

        TextLanguageSelectorCaller textLanguageSelector = StartTimerText.GetComponent<TextLanguageSelectorCaller>();
        if (textLanguageSelector != null)
        {
            textLanguageSelector.SetText(string.Empty);
        }
        else
        {
            StartTimerText.text = "Aloita!";
        }

        SetStartTimerVisualState(true);
        startTextShown = true;
    }

    private void SetStartTimerVisualState(bool showStartText)
    {
        if (StartTimerText == null)
        {
            return;
        }

        Transform startTimerParent = StartTimerText.transform.parent;
        if (startTimerParent == null)
        {
            return;
        }

        Image timerBackgroundImage = startTimerParent.GetComponent<Image>();
        if (timerBackgroundImage != null)
        {
            timerBackgroundImage.enabled = showStartText;
        }

        Transform overlay = startTimerParent.Find("Overlay");
        if (overlay != null)
        {
            overlay.gameObject.SetActive(!showStartText);
        }
    }

    public void FinishRaid()
    {
        timerActive = false;

    }

    private void SetTimerText()
    {
        //TimerText.text = HasFormat ? CurrentTime.ToString(TimeFormat[Format]) : CurrentTime.ToString();
        TimerText.text = CurrentTime.ToString("F0");
    }
    void OnTimeEnd()
    {
        TimeEnded?.Invoke();
    }

    private ExitRaid FindActiveExitRaid()
    {
        ExitRaid[] exitRaids = FindObjectsOfType<ExitRaid>();
        foreach (ExitRaid activeExitRaid in exitRaids)
        {
            if (activeExitRaid != null && activeExitRaid.gameObject.activeInHierarchy)
            {
                return activeExitRaid;
            }
        }

        return null;
    }

    void RaidExited()
    {
        CurrentTime = 0;
    }

    public enum TimerFormat
    {
        Whole, TenthDecimal, HundrethsDecimal
    }
}
