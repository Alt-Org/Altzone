using Altzone.Scripts.Language;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Raid_StartTimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startTimerText;
    [SerializeField] private GameObject startTimerRoot;
    [SerializeField] private Image timerBackgroundImage;
    [SerializeField] private GameObject overlay;

    public void Initialize(TextMeshProUGUI startTimerText)
    {
        if (this.startTimerText == null)
        {
            this.startTimerText = startTimerText;
        }

        ResolveStartTimerRoot();
        ResolveCountdownVisuals();
    }

    public void ResetView(float timeUntilStart)
    {
        if (startTimerText == null)
        {
            return;
        }

        SetRootVisible(true);
        SetCountdownVisualsVisible(true);
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

        SetPostCountdownVisualsVisible();
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

    private void ResolveCountdownVisuals()
    {
        ResolveStartTimerRoot();
        if (startTimerRoot == null)
        {
            return;
        }

        if (timerBackgroundImage == null)
        {
            timerBackgroundImage = startTimerRoot.GetComponent<Image>();
        }

        if (overlay == null)
        {
            Transform overlayTransform = startTimerRoot.transform.Find("Overlay");
            if (overlayTransform != null)
            {
                overlay = overlayTransform.gameObject;
            }
        }
    }

    private void SetCountdownVisualsVisible(bool visible)
    {
        ResolveCountdownVisuals();

        if (timerBackgroundImage != null && timerBackgroundImage.enabled == visible)
        {
            timerBackgroundImage.enabled = !visible;
        }

        if (overlay != null && overlay.activeSelf != visible)
        {
            overlay.SetActive(visible);
        }

        if (startTimerText != null && startTimerText.gameObject.activeSelf != visible)
        {
            startTimerText.gameObject.SetActive(visible);
        }
    }

    private void SetPostCountdownVisualsVisible()
    {
        ResolveCountdownVisuals();

        if (timerBackgroundImage != null && !timerBackgroundImage.enabled)
        {
            timerBackgroundImage.enabled = true;
        }

        if (overlay != null && overlay.activeSelf)
        {
            overlay.SetActive(false);
        }

        if (startTimerText != null && !startTimerText.gameObject.activeSelf)
        {
            startTimerText.gameObject.SetActive(true);
        }
    }
}
