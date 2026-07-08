using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class Raid_TimerView : MonoBehaviour
{
    private static readonly Vector2 LossHaloPadding = new Vector2(200f, 200f);
    private static readonly Vector2 LossHaloOffset = Vector2.zero;
    private static readonly Vector2 TimerTextHaloMinimumSize = new Vector2(260f, 220f);

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Raid_TimerFillCircle timerFillCircle;

    private readonly Color startColor = Color.white;
    private readonly Color endColor = Color.red;
    private Raid_TextHalo timerTextHalo;
    private int lastDisplayedTimerValue = int.MinValue;
    private Raid_Timer.TimerFormat lastTimerFormat;
    private bool lastHasFormat;

    public TextMeshProUGUI TimerText => timerText;

    public void Initialize(TextMeshProUGUI timerText)
    {
        if (this.timerText == null)
        {
            this.timerText = timerText;
        }

        EnsureTimerFillCircle();
    }

    public void ResetView(float currentTime, bool hasFormat, Raid_Timer.TimerFormat format, float fillProgress)
    {
        SetColor(startColor);
        SetText(currentTime, hasFormat, format);
        SetTimerTextHaloVisible(false);
        SetFillProgress(fillProgress);
    }

    public void SetText(float currentTime, bool hasFormat, Raid_Timer.TimerFormat format)
    {
        if (timerText == null)
        {
            return;
        }

        int displayValue = GetDisplayTimerValue(currentTime, hasFormat, format);
        if (displayValue == lastDisplayedTimerValue && hasFormat == lastHasFormat && format == lastTimerFormat)
        {
            return;
        }

        lastDisplayedTimerValue = displayValue;
        lastHasFormat = hasFormat;
        lastTimerFormat = format;

        timerText.text = currentTime.ToString(GetTimeFormat(hasFormat, format));
        timerTextHalo?.Sync();
    }

    public void SetWarningPulse(float duration)
    {
        if (timerText == null)
        {
            return;
        }

        float t = Mathf.PingPong(Time.time / duration, 1f);
        timerText.color = Color.Lerp(startColor, endColor, t);
    }

    public void SetEndColor()
    {
        SetColor(Color.red);
    }

    public void SetFillProgress(float progress)
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

    public void SetLossHaloVisible(bool visible, GameObject haloTarget)
    {
        Raid_RedHalo.SetVisible(haloTarget, visible, LossHaloPadding, LossHaloOffset);
        SetTimerTextHaloVisible(visible);
    }

    public void Dispose()
    {
        timerTextHalo?.Dispose();
        timerTextHalo = null;
    }

    private void SetColor(Color color)
    {
        if (timerText != null)
        {
            timerText.color = color;
        }
    }

    private void EnsureTimerFillCircle()
    {
        if (timerFillCircle == null && timerText != null)
        {
            timerFillCircle = timerText.GetComponentInParent<Raid_TimerFillCircle>(true);
            if (timerFillCircle == null && timerText.transform.parent != null)
            {
                timerFillCircle = timerText.transform.parent.GetComponentInChildren<Raid_TimerFillCircle>(true);
            }
        }

        if (timerFillCircle != null)
        {
            timerFillCircle.raycastTarget = false;
        }
    }

    private void SetTimerTextHaloVisible(bool visible)
    {
        if (timerText == null)
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
            timerTextHalo = new Raid_TextHalo(timerText, settings);
        }

        timerTextHalo.SetTarget(timerText);
        timerTextHalo.SetVisible(visible);
    }

    private static int GetDisplayTimerValue(float currentTime, bool hasFormat, Raid_Timer.TimerFormat format)
    {
        if (!hasFormat)
        {
            return Mathf.RoundToInt(currentTime);
        }

        return format switch
        {
            Raid_Timer.TimerFormat.TenthDecimal => Mathf.RoundToInt(currentTime * 10f),
            Raid_Timer.TimerFormat.HundrethsDecimal => Mathf.RoundToInt(currentTime * 100f),
            _ => Mathf.RoundToInt(currentTime)
        };
    }

    private static string GetTimeFormat(bool hasFormat, Raid_Timer.TimerFormat format)
    {
        if (!hasFormat)
        {
            return "F0";
        }

        return format switch
        {
            Raid_Timer.TimerFormat.TenthDecimal => "F1",
            Raid_Timer.TimerFormat.HundrethsDecimal => "F2",
            _ => "F0"
        };
    }
}
