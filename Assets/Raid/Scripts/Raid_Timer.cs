using Altzone.Scripts.Language;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class Raid_Timer : MonoBehaviour
{
    private static readonly Vector2 LossHaloPadding = new Vector2(200f, 200f);
    private static readonly Vector2 LossHaloOffset = Vector2.zero;
    private static readonly Vector2 TimerTextHaloMinimumSize = new Vector2(260f, 220f);

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
    private Raid_TextHalo timerTextHalo;

    public bool HasStarted => started;

    private void Start()
    {
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

    private void OnDestroy()
    {
        if (exitRaid != null)
        {
            exitRaid.ExitedRaid -= RaidExited;
        }

        timerTextHalo?.Dispose();
    }

    private void Update()
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
            if (CurrentTime <= 5f && TimerText != null)
            {
                float t = Mathf.PingPong(Time.time / duration, 1f);
                Color lerped = Color.Lerp(startColor, endColor, t);

                TimerText.color = lerped;
            }
            if (HasLimit && ((CountUp && CurrentTime >= TimerLimit) || (!CountUp && CurrentTime <= TimerLimit)))
            {
                OnTimeEnd();
                Raid_LiveInventory.Instance?.HideLiveInventory();
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
                if (TimerText != null)
                {
                    TimerText.color = Color.red;
                }
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
                    if (StartTimerText != null)
                    {
                        StartTimerText.text = Mathf.CeilToInt(TimeUntilStart).ToString();
                    }
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
        SetTimerTextHaloVisible(false);
        SetRaidControlsVisible(false);
        UpdateTimerFill();
    }

    private void ShowStartText()
    {
        if (startTextShown)
        {
            return;
        }

        if (StartTimerText == null)
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
            timerBackgroundImage.enabled = showStartText;
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
        SetTimerTextHaloVisible(visible);
    }

    private void SetTimerText()
    {
        if (TimerText == null)
        {
            return;
        }

        TimerText.text = CurrentTime.ToString(GetTimeFormat());
        timerTextHalo?.Sync();
    }

    private string GetTimeFormat()
    {
        if (!HasFormat)
        {
            return "F0";
        }

        return Format switch
        {
            TimerFormat.TenthDecimal => "F1",
            TimerFormat.HundrethsDecimal => "F2",
            _ => "F0"
        };
    }

    private void SetTimerTextHaloVisible(bool visible)
    {
        if (TimerText == null)
        {
            return;
        }

        if (timerTextHalo == null)
        {
            if (!visible)
            {
                return;
            }

            Raid_TextHalo.Settings settings = Raid_TextHalo.CreateDefaultSettings(
                TimerTextHaloMinimumSize,
                "TimerText_RedCloudHalo");
            timerTextHalo = new Raid_TextHalo(TimerText, settings);
        }

        timerTextHalo.SetTarget(TimerText);
        timerTextHalo.SetVisible(visible);
    }

    private void EnsureTimerFillCircle()
    {
        if (timerFillCircle == null && TimerText != null)
        {
            timerFillCircle = TimerText.GetComponentInParent<Raid_TimerFillCircle>(true);
            if (timerFillCircle == null && TimerText.transform.parent != null)
            {
                timerFillCircle = TimerText.transform.parent.GetComponentInChildren<Raid_TimerFillCircle>(true);
            }
        }

        if (timerFillCircle == null)
        {
            return;
        }

        timerFillCircle.raycastTarget = false;
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

    private void OnTimeEnd()
    {
        TimeEnded?.Invoke();
    }

    private void RaidExited()
    {
        CurrentTime = 0;
        UpdateTimerFill();
    }

    public enum TimerFormat
    {
        Whole, TenthDecimal, HundrethsDecimal
    }
}
