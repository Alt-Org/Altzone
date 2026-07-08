using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class Raid_Controls : MonoBehaviour
{
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private GameObject timerPanelBackground;
    [SerializeField] private ExitRaid exitRaid;

    private bool? visible;

    public void Initialize(TextMeshProUGUI timerText, ExitRaid exitRaid)
    {
        ResolveTimerPanel(timerText);

        if (this.exitRaid == null)
        {
            this.exitRaid = exitRaid != null ? exitRaid : ExitRaid.Instance;
        }

        ApplyCurrentVisibility();
    }

    public void SetVisible(bool visible)
    {
        if (this.visible == visible)
        {
            return;
        }

        this.visible = visible;
        ApplyVisibility(visible);
    }

    public GameObject GetTimerPanelTarget(TextMeshProUGUI timerText)
    {
        ResolveTimerPanel(timerText);
        return timerPanel != null
            ? timerPanel
            : timerText != null ? timerText.transform.parent?.gameObject : null;
    }

    private void ResolveTimerPanel(TextMeshProUGUI timerText)
    {
        if (timerPanel == null && timerText != null && timerText.transform.parent != null)
        {
            timerPanel = timerText.transform.parent.gameObject;
        }

        if (timerPanelBackground != null || timerPanel == null)
        {
            return;
        }

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

    private void ApplyCurrentVisibility()
    {
        if (visible.HasValue)
        {
            ApplyVisibility(visible.Value);
        }
    }

    private void ApplyVisibility(bool visible)
    {
        if (timerPanel != null && timerPanel.activeSelf != visible)
        {
            timerPanel.SetActive(visible);
        }

        if (timerPanelBackground != null && timerPanelBackground.activeSelf != visible)
        {
            timerPanelBackground.SetActive(visible);
        }

        if (exitRaid == null)
        {
            exitRaid = ExitRaid.Instance;
        }

        exitRaid?.SetButtonVisible(visible);
    }
}
