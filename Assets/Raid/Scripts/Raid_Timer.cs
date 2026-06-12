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
    private float initialCurrentTime;
    private float initialTimeUntilStart;

    [Header("Limit settings")]
    public bool HasLimit;
    public float TimerLimit;

    [Header("Format settings")]
    public bool HasFormat;
    public TimerFormat Format;
    private Dictionary<TimerFormat, string> TimeFormat = new Dictionary<TimerFormat, string>();

    public event Action TimeEnded;
    public event Action TimerStarted;
    public ExitRaid exitRaid;

    private Color startColor = Color.white;
    private Color endColor = Color.red;
    [SerializeField]
    private float duration;

    [Header("Timer fill")]
    [SerializeField] private Raid_TimerFillCircle timerFillCircle;

    private float timerStartTime;

    public bool HasStarted => started;

    void Start()
    {
        TimeFormat.Add(TimerFormat.Whole, "0");
        TimeFormat.Add(TimerFormat.TenthDecimal, "0.0");
        TimeFormat.Add(TimerFormat.HundrethsDecimal, "0.00");
        initialCurrentTime = CurrentTime;
        initialTimeUntilStart = TimeUntilStart;

        if (exitRaid == null)
        {
            exitRaid = ExitRaid.Instance;
        }

        if (exitRaid != null)
        {
            exitRaid.ExitedRaid += RaidExited;
        }

        SetStartTimerVisualState(false);
        timerStartTime = CurrentTime;
        EnsureTimerFillCircle();
        UpdateTimerFill();
    }

    void Update()
    {
        if (RaidMatchmakingController.Instance != null
            && RaidMatchmakingController.Instance.ControlsInventorySetup
            && !RaidMatchmakingController.Instance.HasReleasedGameplay)
        {
            SetTimerText();
            SetTimerFillProgress(0f);
            return;
        }

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
                if (exitRaid == null)
                {
                    exitRaid = ExitRaid.Instance;
                }

                if (exitRaid != null)
                {
                    exitRaid.EndRaid(ExitRaid.RaidEndReason.OutOfTime);
                }
                else
                {
                    Debug.LogError("Raid timer ended, but ExitRaid.Instance was not available.");
                }
                CurrentTime = TimerLimit;
                SetTimerText();
                UpdateTimerFill();
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
        UpdateTimerFill();
    }
    public void StartTimer()
    {
        if (StartTimerText != null && StartTimerText.transform.parent != null)
        {
            StartTimerText.gameObject.transform.parent.gameObject.SetActive(false);
        }

        if (!timerActive && !started)
        {
            timerStartTime = CurrentTime;
            timerActive = true;
            started = true;
            UpdateTimerFill();
            TimerStarted?.Invoke();
        }
            
    }

    public void ResetStartCountdown()
    {
        timerActive = false;
        started = false;
        startTextShown = false;
        CurrentTime = initialCurrentTime;
        TimeUntilStart = initialTimeUntilStart;
        timerStartTime = CurrentTime;

        if (StartTimerText != null && StartTimerText.transform.parent != null)
        {
            StartTimerText.transform.parent.gameObject.SetActive(true);
            SetStartTimerVisualState(false);
            StartTimerText.text = Mathf.CeilToInt(TimeUntilStart).ToString();
        }

        if (TimerText != null)
        {
            TimerText.color = startColor;
        }

        SetTimerText();
        UpdateTimerFill();
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
        UpdateTimerFill();
    }

    private void SetTimerText()
    {
        //TimerText.text = HasFormat ? CurrentTime.ToString(TimeFormat[Format]) : CurrentTime.ToString();
        TimerText.text = CurrentTime.ToString("F0");
    }

    private void EnsureTimerFillCircle()
    {
        bool createdFillCircle = false;

        if (timerFillCircle == null && TimerText != null && TimerText.transform.parent != null)
        {
            Transform timerPanel = TimerText.transform.parent;
            Transform existingFill = timerPanel.Find("TimerFillCircle");

            if (existingFill != null)
            {
                timerFillCircle = existingFill.GetComponent<Raid_TimerFillCircle>();
            }

            if (timerFillCircle == null)
            {
                GameObject fillObject = new GameObject("TimerFillCircle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Raid_TimerFillCircle));
                RectTransform fillRect = fillObject.GetComponent<RectTransform>();
                fillRect.SetParent(timerPanel, false);
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.anchoredPosition = Vector2.zero;
                fillRect.sizeDelta = Vector2.zero;
                fillRect.pivot = new Vector2(0.5f, 0.5f);

                timerFillCircle = fillObject.GetComponent<Raid_TimerFillCircle>();
                createdFillCircle = true;
            }
        }

        if (timerFillCircle == null)
        {
            return;
        }

        if (createdFillCircle)
        {
            timerFillCircle.raycastTarget = false;
            timerFillCircle.transform.SetAsFirstSibling();

            if (TimerText != null)
            {
                TimerText.transform.SetAsLastSibling();
            }
        }
    }

    private void UpdateTimerFill()
    {
        SetTimerFillProgress(CalculateTimerFillProgress());
    }

    private void SetTimerFillProgress(float progress)
    {
        if (timerFillCircle == null)
        {
            EnsureTimerFillCircle();
        }

        if (timerFillCircle != null)
        {
            timerFillCircle.Progress = progress;
        }
    }

    private float CalculateTimerFillProgress()
    {
        if (!started)
        {
            return 0f;
        }

        if (Mathf.Approximately(timerStartTime, TimerLimit))
        {
            return Mathf.Approximately(CurrentTime, TimerLimit) ? 1f : 0f;
        }

        return Mathf.InverseLerp(timerStartTime, TimerLimit, CurrentTime);
    }

    void OnTimeEnd()
    {
        TimeEnded?.Invoke();
    }

    void RaidExited()
    {
        CurrentTime = 0;
        UpdateTimerFill();
    }

    public enum TimerFormat
    {
        Whole, TenthDecimal, HundrethsDecimal
    }
}
