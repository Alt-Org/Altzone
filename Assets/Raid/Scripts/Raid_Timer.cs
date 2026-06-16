using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class Raid_Timer : MonoBehaviour
{
    private static readonly Vector2 LossHaloPadding = new Vector2(64f, 64f);
    private static readonly Vector2 LossHaloOffset = Vector2.zero;

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
    [SerializeField] private CanvasGroup exitRaidButtonCanvasGroup;

    private float timerStartTime;
    private GameObject timerPanel;
    private GameObject timerPanelBackground;
    private Button exitRaidButton;

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
        SetRaidControlsVisible(false);
        UpdateTimerFill();
    }

    void Update()
    {
        if (RaidMatchmakingController.Instance != null
            && RaidMatchmakingController.Instance.ControlsInventorySetup
            && !RaidMatchmakingController.Instance.HasReleasedGameplay)
        {
            SetRaidControlsVisible(false);
            SetTimerText();
            SetTimerFillProgress(0f);
            return;
        }

        SetRaidControlsVisible(started || startTextShown);

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
            SetRaidControlsVisible(true);
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
        SetRaidControlsVisible(false);
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
        SetRaidControlsVisible(true);
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
            timerBackgroundImage.enabled = false;
        }

        Transform overlay = startTimerParent.Find("Overlay");
        if (overlay != null)
        {
            overlay.gameObject.SetActive(!showStartText);
        }
    }

    private void SetRaidControlsVisible(bool visible)
    {
        ResolveRaidControlReferences();

        if (timerPanel != null && timerPanel.activeSelf != visible)
        {
            timerPanel.SetActive(visible);
        }

        if (timerPanelBackground != null && timerPanelBackground.activeSelf != visible)
        {
            timerPanelBackground.SetActive(visible);
        }

        if (exitRaidButtonCanvasGroup != null)
        {
            exitRaidButtonCanvasGroup.alpha = visible ? 1f : 0f;
            exitRaidButtonCanvasGroup.interactable = visible;
            exitRaidButtonCanvasGroup.blocksRaycasts = visible;
        }

        if (exitRaidButton != null)
        {
            exitRaidButton.interactable = visible;
        }
    }

    private void ResolveRaidControlReferences()
    {
        if (timerPanel == null && TimerText != null && TimerText.transform.parent != null)
        {
            timerPanel = TimerText.transform.parent.gameObject;
        }

        if (timerPanelBackground == null && timerPanel != null)
        {
            Transform background = timerPanel.transform.Find("background");
            if (background == null && timerPanel.transform.parent != null)
            {
                background = timerPanel.transform.parent.Find("background");
            }

            if (background != null)
            {
                timerPanelBackground = background.gameObject;
            }
        }

        if (exitRaid == null)
        {
            exitRaid = ExitRaid.Instance;
        }

        if (exitRaid == null)
        {
            return;
        }

        if (exitRaidButtonCanvasGroup == null)
        {
            exitRaidButtonCanvasGroup = exitRaid.GetComponent<CanvasGroup>();
            if (exitRaidButtonCanvasGroup == null)
            {
                exitRaidButtonCanvasGroup = exitRaid.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (exitRaidButton == null)
        {
            exitRaidButton = exitRaid.GetComponent<Button>();
        }
    }

    public void FinishRaid()
    {
        timerActive = false;
        UpdateTimerFill();
    }

    public void SetLossHaloVisible(bool visible)
    {
        ResolveRaidControlReferences();

        GameObject haloTarget = timerPanel != null ? timerPanel : TimerText != null ? TimerText.transform.parent?.gameObject : null;
        Raid_RedHalo.SetVisible(haloTarget, visible, LossHaloPadding, LossHaloOffset);
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
            return 1f;
        }

        if (Mathf.Approximately(timerStartTime, TimerLimit))
        {
            return Mathf.Approximately(CurrentTime, TimerLimit) ? 0f : 1f;
        }

        return 1f - Mathf.InverseLerp(timerStartTime, TimerLimit, CurrentTime);
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
