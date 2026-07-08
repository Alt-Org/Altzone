using UnityEngine;
using TMPro;
using System;

public class Raid_Timer : MonoBehaviour
{
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
    public event Action<float> TimerFillProgressChanged;
    public ExitRaid exitRaid;

    [SerializeField]
    private float duration;

    [Header("Timer fill")]
    [SerializeField] private Raid_TimerView timerView;
    [SerializeField] private Raid_StartTimerView startTimerView;
    [SerializeField] private Raid_Controls raidControls;

    private float timerStartTime;
    private RaidMatchmakingController matchmakingController;
    private bool waitingForGameplayRelease;

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
        ResolveTimerViews();
        timerStartTime = CurrentTime;
        raidControls.SetVisible(false);
        UpdateTimerFill();
        SubscribeGameplayReleaseChanged();
    }

    private void OnDestroy()
    {
        UnsubscribeGameplayReleaseChanged();
        UnsubscribeTimerView();

        if (exitRaid != null)
        {
            exitRaid.ExitedRaid -= RaidExited;
        }

        timerView?.Dispose();
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
            if (CurrentTime <= 5f)
            {
                timerView.SetWarningPulse(duration);
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
                timerView.SetEndColor();
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
                    startTimerView.SetCountdown(TimeUntilStart);
                    LogStartCountdownSecond(Mathf.CeilToInt(TimeUntilStart));
                }
            }
        }
        
        SetTimerText();
        UpdateTimerFill();
    }
    public void StartTimer()
    {
        startTimerView.Hide();

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

        startTimerView.ResetView(TimeUntilStart);
        timerView.ResetView(CurrentTime, HasFormat, Format, CalculateTimerFillProgress());
        raidControls.SetVisible(false);
    }

    private void ShowStartText()
    {
        if (startTextShown)
        {
            return;
        }

        startTimerView.ShowStartText();
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

    private void ResolveTimerViews()
    {
        if (timerView == null)
        {
            TryGetComponent(out timerView);
        }

        if (timerView == null)
        {
            timerView = gameObject.AddComponent<Raid_TimerView>();
        }

        timerView.Initialize(TimerText);
        TimerFillProgressChanged -= timerView.SetFillProgress;
        TimerFillProgressChanged += timerView.SetFillProgress;

        if (startTimerView == null)
        {
            TryGetComponent(out startTimerView);
        }

        if (startTimerView == null)
        {
            startTimerView = gameObject.AddComponent<Raid_StartTimerView>();
        }

        startTimerView.Initialize(StartTimerText);
        startTimerView.ResetView(TimeUntilStart);
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
        TimerFillProgressChanged?.Invoke(0f);
    }

    public void SetLossHaloVisible(bool visible)
    {
        GameObject haloTarget = raidControls != null
            ? raidControls.GetTimerPanelTarget(TimerText)
            : TimerText != null ? TimerText.transform.parent?.gameObject : null;
        timerView.SetLossHaloVisible(visible, haloTarget);
    }

    private void SetTimerText()
    {
        timerView.SetText(CurrentTime, HasFormat, Format);
    }

    private void UpdateTimerFill()
    {
        TimerFillProgressChanged?.Invoke(CalculateTimerFillProgress());
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

    private void UnsubscribeTimerView()
    {
        if (timerView != null)
        {
            TimerFillProgressChanged -= timerView.SetFillProgress;
        }
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
