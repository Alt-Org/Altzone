using Altzone.Scripts.Language;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class Raid_StartTimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startTimerText;
    [SerializeField] private GameObject startTimerRoot;

    public void Initialize(TextMeshProUGUI startTimerText)
    {
        if (this.startTimerText == null)
        {
            this.startTimerText = startTimerText;
        }

        ResolveStartTimerRoot();
    }

    public void ResetView(float timeUntilStart)
    {
        if (startTimerText == null)
        {
            return;
        }

        SetRootVisible(true);
        SetCountdown(timeUntilStart);
    }

    public void Hide()
    {
        SetRootVisible(false);
    }

    public void SetCountdown(float timeUntilStart)
    {
        if (startTimerText != null)
        {
            startTimerText.text = Mathf.CeilToInt(timeUntilStart).ToString();
        }
    }

    public void ShowStartText()
    {
        if (startTimerText == null)
        {
            return;
        }

        TextLanguageSelectorCaller textLanguageSelector = startTimerText.GetComponent<TextLanguageSelectorCaller>();
        if (textLanguageSelector != null)
        {
            textLanguageSelector.SetText(string.Empty);
        }
        else
        {
            startTimerText.text = "Aloita!";
        }

    }

    private void SetRootVisible(bool visible)
    {
        ResolveStartTimerRoot();
        if (startTimerRoot != null && startTimerRoot.activeSelf != visible)
        {
            startTimerRoot.SetActive(visible);
        }
    }

    private void ResolveStartTimerRoot()
    {
        if (startTimerRoot != null || startTimerText == null)
        {
            return;
        }

        if (startTimerText.transform.parent != null)
        {
            startTimerRoot = startTimerText.transform.parent.gameObject;
        }
    }
}
