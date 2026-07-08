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
    [SerializeField] private Raid_EventLog eventLog;

    [Header("Timer texts")]
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI StartTimerText;

    [Header("Timer settings")]
    public float CurrentTime;
    public bool CountUp;
    private bool timerActive;
    private bool started;
    private bool finished;

    [Header("Start Timer settings")]
    public float TimeUntilStart;
    private bool startTextShown;
    private float initialCurrentTime;
    private float initialTimeUntilStart;
    private int lastLoggedStartCountdownSecond = int.MaxValue;
    private bool gameStartLogged;

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
    [SerializeField] private Raid_Controls raidControls;

    private float timerStartTime;
    private Raid_TextHalo timerTextHalo;
    private RaidMatchmakingController matchmakingController;
    private bool waitingForGameplayRelease;
    private int lastDisplayedTimerValue = int.MinValue;
    private TimerFormat lastTimerFormat;
    private bool lastHasFormat;

    public bool HasStarted => started;

    private void Start()
    {
        initialCurrentTime = CurrentTime;
        initialTimeUntilStart = TimeUntilStart;
        ResolveEventLog();

        if (exitRaid == null)
        {
            exitRaid = ExitRaid.Instance;
        }

        if (exitRaid != null)
        {
            exitRaid.ExitedRaid += RaidExited;
        }

        ResolveRaidControls();
        SetStartTimerVisualState(false);
        timerStartTime = CurrentTime;
        EnsureTimerFillCircle();
        raidControls.SetVisible(false);
        UpdateTimerFill();
        SubscribeGameplayReleaseChanged();
    }

    private void OnDestroy()
    {
        UnsubscribeGameplayReleaseChanged();

        if (exitRaid != null)
        {
            exitRaid.ExitedRaid -= RaidExited;
        }

        timerTextHalo?.Dispose();
    }

    private void Update()
    {
        if (waitingForGameplayRelease || finished)
        {
            return;
        }

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
                        int countdownSecond = Mathf.CeilToInt(TimeUntilStart);
                        StartTimerText.text = countdownSecond.ToString();
                        LogStartCountdownSecond(countdownSecond);
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
            raidControls.SetVisible(true);
            UpdateTimerFill();
            LogGameStart();
            TimerStarted?.Invoke();
        }
            
    }

    public void ResetStartCountdown()
    {
        timerActive = false;
        started = false;
        finished = false;
        waitingForGameplayRelease = false;
        startTextShown = false;
        CurrentTime = initialCurrentTime;
        TimeUntilStart = initialTimeUntilStart;
        lastLoggedStartCountdownSecond = int.MaxValue;
        gameStartLogged = false;
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
        raidControls.SetVisible(false);
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
        raidControls.SetVisible(true);
    }

    private void LogStartCountdownSecond(int countdownSecond)
    {
        if (countdownSecond < 1 || countdownSecond > 3 || countdownSecond == lastLoggedStartCountdownSecond)
        {
            return;
        }

        lastLoggedStartCountdownSecond = countdownSecond;
        ResolveEventLog();
        eventLog?.LogSystemMessage(UseEnglish() ? $"Game starts {countdownSecond}" : $"Peli alkaa {countdownSecond}");
    }

    private void LogGameStart()
    {
        if (gameStartLogged)
        {
            return;
        }

        gameStartLogged = true;
        ResolveEventLog();
        eventLog?.LogSystemMessage(UseEnglish() ? "Gathering started" : "Kokoaminen aloitettu");
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

    private void ResolveRaidControls()
    {
        if (exitRaid == null)
        {
            exitRaid = ExitRaid.Instance;
        }

        if (raidControls == null)
        {
            TryGetComponent(out raidControls);
        }

        if (raidControls == null)
        {
            raidControls = gameObject.AddComponent<Raid_Controls>();
        }

        raidControls.Initialize(TimerText, exitRaid);
    }

    public void FinishRaid()
    {
        timerActive = false;
        UpdateTimerFill();
    }

    public void HideRaidControlsForEndMenu()
    {
        finished = true;
        raidControls.SetVisible(false);
    }

    private void SubscribeGameplayReleaseChanged()
    {
        matchmakingController = RaidMatchmakingController.Instance;
        if (matchmakingController == null || !matchmakingController.ControlsInventorySetup)
        {
            waitingForGameplayRelease = false;
            return;
        }

        matchmakingController.GameplayReleasedChanged -= OnGameplayReleasedChanged;
        matchmakingController.GameplayReleasedChanged += OnGameplayReleasedChanged;
        OnGameplayReleasedChanged(matchmakingController.HasReleasedGameplay);
    }

    private void UnsubscribeGameplayReleaseChanged()
    {
        if (matchmakingController != null)
        {
            matchmakingController.GameplayReleasedChanged -= OnGameplayReleasedChanged;
        }
    }

    private void OnGameplayReleasedChanged(bool released)
    {
        waitingForGameplayRelease = !released;
        if (released)
        {
            UpdateTimerFill();
            return;
        }

        ResetTimerForGameplaySetup();
    }

    private void ResetTimerForGameplaySetup()
    {
        ResetStartCountdown();
        waitingForGameplayRelease = true;
        raidControls.SetVisible(false);
        SetTimerFillProgress(0f);
    }

    public void SetLossHaloVisible(bool visible)
    {
        GameObject haloTarget = raidControls != null
            ? raidControls.GetTimerPanelTarget(TimerText)
            : TimerText != null ? TimerText.transform.parent?.gameObject : null;
        Raid_RedHalo.SetVisible(haloTarget, visible, LossHaloPadding, LossHaloOffset);
        SetTimerTextHaloVisible(visible);
    }

    private void SetTimerText()
    {
        if (TimerText == null)
        {
            return;
        }

        int displayValue = GetDisplayTimerValue();
        if (displayValue == lastDisplayedTimerValue && HasFormat == lastHasFormat && Format == lastTimerFormat)
        {
            return;
        }

        lastDisplayedTimerValue = displayValue;
        lastHasFormat = HasFormat;
        lastTimerFormat = Format;

        TimerText.text = CurrentTime.ToString(GetTimeFormat());
        timerTextHalo?.Sync();
    }

    private int GetDisplayTimerValue()
    {
        if (!HasFormat)
        {
            return Mathf.RoundToInt(CurrentTime);
        }

        return Format switch
        {
            TimerFormat.TenthDecimal => Mathf.RoundToInt(CurrentTime * 10f),
            TimerFormat.HundrethsDecimal => Mathf.RoundToInt(CurrentTime * 100f),
            _ => Mathf.RoundToInt(CurrentTime)
        };
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

    private void ResolveEventLog()
    {
        if (eventLog != null)
        {
            return;
        }

        if (raid_References == null)
        {
            raid_References = Raid_References.Instance;
        }

        if (raid_References != null)
        {
            eventLog = raid_References.EventLog;
        }
    }

    private static bool UseEnglish()
    {
        return SettingsCarrier.Instance != null
            && SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English;
    }

    public enum TimerFormat
    {
        Whole, TenthDecimal, HundrethsDecimal
    }
}
