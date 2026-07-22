using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class Raid_Controls : MonoBehaviour
{
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private GameObject timerPanelBackground;
    [SerializeField] private GameObject timerPanelClock;
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

    public void GetTimerPanelHaloTargets(TextMeshProUGUI timerText, out GameObject backgroundTarget, out GameObject clockTarget)
    {
        ResolveTimerPanel(timerText);
        ResolveTimerPanelHaloTargets(timerText);

        backgroundTarget = timerPanelBackground;
        clockTarget = timerPanelClock;
    }

    private void ResolveTimerPanel(TextMeshProUGUI timerText)
    {
        if (timerPanel != null || timerText == null)
        {
            return;
        }

        Transform timerPanelTransform = FindAncestor(timerText.transform, "TimerPanel");
        timerPanel = timerPanelTransform != null ? timerPanelTransform.gameObject : null;
    }

    private void ResolveTimerPanelHaloTargets(TextMeshProUGUI timerText)
    {
        if (timerPanelBackground == null && timerPanel != null)
        {
            Transform background = FindDescendant(timerPanel.transform, "background");
            timerPanelBackground = background != null ? background.gameObject : null;
        }

        if (timerPanelClock == null)
        {
            Transform clock = timerPanel != null ? FindDescendant(timerPanel.transform, "Clock") : null;
            timerPanelClock = clock != null ? clock.gameObject : null;
        }
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

    private static Transform FindDescendant(Transform root, string descendantName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == descendantName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindDescendant(root.GetChild(i), descendantName);
            if (result != null)
            {
                return result;
            }
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
