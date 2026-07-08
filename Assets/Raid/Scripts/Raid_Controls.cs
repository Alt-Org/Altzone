using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class Raid_Controls : MonoBehaviour
{
    [SerializeField] private GameObject timerPanel;
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
        if (timerPanel != null || timerText == null)
        {
            return;
        }

        Transform timerPanelTransform = FindAncestor(timerText.transform, "TimerPanel");
        timerPanel = timerPanelTransform != null
            ? timerPanelTransform.gameObject
            : timerText.transform.parent?.gameObject;
    }

    private static Transform FindAncestor(Transform child, string ancestorName)
    {
        Transform current = child;
        while (current != null)
        {
            if (current.name == ancestorName)
            {
                return current;
            }

            current = current.parent;
        }

        return null;
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

        if (exitRaid == null)
        {
            exitRaid = ExitRaid.Instance;
        }

        exitRaid?.SetButtonVisible(visible);
    }
}
