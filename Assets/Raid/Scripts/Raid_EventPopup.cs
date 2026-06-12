using System;
using System.Collections;
using Altzone.Scripts.Language;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Raid_EventPopup : MonoBehaviour
{
    public enum Scenario
    {
        EndTrap,
        Freeze,
        DoubleWeight,
        OutOfTime,
        OutOfSpace,
        PlayerExit,
        StartCountdown
    }

    [Serializable]
    private class ScenarioVisual
    {
        public Scenario Scenario;
        [FormerlySerializedAs("Message")]
        [TextArea(1, 3)] public string FinnishMessage;
        [TextArea(1, 3)] public string EnglishMessage;
        public Sprite BackgroundSprite = null;
        public Color BackgroundColor = new Color(0f, 0f, 0f, 0.55f);
        public Sprite Effect = null;
        public Color EffectColor = Color.white;
        public Color TextColor = Color.white;
        public Sprite Image = null;
        public Sprite SecondaryImage = null;
        [FormerlySerializedAs("MultText")]
        [TextArea(1, 3)] public string FinnishMultText = "";
        [TextArea(1, 3)] public string EnglishMultText = "";

        public string Message { get; private set; }
        public string MultText { get; private set; }

        public void SetLocalizedTexts(string message, string multText)
        {
            Message = message;
            MultText = multText;
        }
    }

    private const string PopupResourcePath = "Prefabs/RaidEventPopup";
    private const float DefaultShowTime = 1.75f;
    private const float DefaultStartCountdownStepTime = 1f;

    private static Raid_EventPopup instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image effectImage;
    [SerializeField] private Image image;
    [SerializeField] private Image secondaryImage;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI MultText;
    [SerializeField] private float showTime = DefaultShowTime;
    [SerializeField] private ScenarioVisual[] scenarioVisuals;

    public static void Show(MonoBehaviour owner, Scenario scenario)
    {
        if (owner == null)
        {
            return;
        }

        owner.StartCoroutine(ShowAndWait(owner, scenario));
    }

    public static void Show(MonoBehaviour owner, Scenario scenario, float duration)
    {
        if (owner == null)
        {
            return;
        }

        owner.StartCoroutine(ShowAndWait(owner, scenario, duration));
    }

    public static IEnumerator ShowAndWait(MonoBehaviour owner, Scenario scenario, Action onComplete = null)
    {
        yield return ShowAndWait(owner, scenario, -1f, onComplete);
    }

    public static IEnumerator ShowAndWait(MonoBehaviour owner, Scenario scenario, float duration, Action onComplete = null)
    {
        Raid_EventPopup popup = GetOrCreate();
        if (popup == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        yield return popup.ShowRoutine(scenario, duration);
        onComplete?.Invoke();
    }

    public static IEnumerator ShowStartCountdownAndWait(MonoBehaviour owner, float stepTime = DefaultStartCountdownStepTime, Action onComplete = null)
    {
        if (owner == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        Raid_EventPopup popup = GetOrCreate();
        if (popup == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        yield return popup.ShowStartCountdownRoutine(Mathf.Max(0.01f, stepTime));
        onComplete?.Invoke();
    }

    public static void HideActive()
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(false);
        }
    }

    private static Raid_EventPopup GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        Raid_EventPopup prefab = Resources.Load<Raid_EventPopup>(PopupResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"Raid event popup prefab was not found at Resources/{PopupResourcePath}.");
            return null;
        }

        instance = Instantiate(prefab);
        instance.name = "Raid Event Popup";
        DontDestroyOnLoad(instance.gameObject);
        instance.gameObject.SetActive(false);
        return instance;
    }

    private IEnumerator ShowRoutine(Scenario scenario, float duration)
    {
        ScenarioVisual visual = GetScenarioVisual(scenario);
        ApplyScenarioVisual(visual);

        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        float displayTime = duration >= 0f ? duration : showTime;
        if (scenario == Scenario.Freeze && displayTime > 0f)
        {
            yield return ShowFreezeCountdownRoutine(visual, displayTime);
        }
        else
        {
            yield return new WaitForSeconds(displayTime);
        }

        gameObject.SetActive(false);
    }

    private IEnumerator ShowFreezeCountdownRoutine(ScenarioVisual visual, float duration)
    {
        float remainingTime = duration;
        int lastDisplayedSeconds = -1;

        while (remainingTime > 0f)
        {
            int secondsRemaining = Mathf.Max(1, Mathf.CeilToInt(remainingTime));
            if (secondsRemaining != lastDisplayedSeconds)
            {
                SetLocalizedText(messageText, FormatCountdownMessage(visual.Message, secondsRemaining), visual.TextColor);
                lastDisplayedSeconds = secondsRemaining;
            }

            yield return null;
            remainingTime -= Time.deltaTime;
        }
    }

    private IEnumerator ShowStartCountdownRoutine(float stepTime)
    {
        ScenarioVisual visual = GetScenarioVisual(Scenario.StartCountdown);
        ApplyScenarioVisual(visual);

        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        string startText = string.IsNullOrWhiteSpace(visual.Message)
            ? GetLocalizedText("Aloita", "Start")
            : visual.Message;
        string[] countdownSteps = { "3", "2", "1", startText };

        foreach (string step in countdownSteps)
        {
            SetLocalizedText(messageText, step, visual.TextColor);
            SetLocalizedText(MultText, string.Empty, visual.TextColor);
            if (MultText != null)
            {
                MultText.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(stepTime);
        }

        gameObject.SetActive(false);
    }

    private string FormatCountdownMessage(string messageTemplate, int secondsRemaining)
    {
        if (string.IsNullOrWhiteSpace(messageTemplate))
        {
            return secondsRemaining.ToString();
        }

        string secondsText = secondsRemaining.ToString();
        if (messageTemplate.Contains("{0}"))
        {
            return messageTemplate.Replace("{0}", secondsText);
        }

        return messageTemplate.Replace("10", secondsText);
    }

    private void ApplyScenarioVisual(ScenarioVisual visual)
    {
        SetLocalizedText(messageText, visual.Message, visual.TextColor);
        SetLocalizedText(MultText, visual.MultText, visual.TextColor);

        ApplyImage(backgroundImage, visual.BackgroundSprite, visual.BackgroundColor);
        ApplyImage(effectImage, visual.Effect, visual.EffectColor);

        if (effectImage != null)
        {
            effectImage.gameObject.SetActive(visual.Effect != null);
        }

        if (image != null)
        {
            image.gameObject.SetActive(visual.Image != null);
            image.sprite = visual.Image;
        }

        if (secondaryImage != null)
        {
            secondaryImage.gameObject.SetActive(visual.SecondaryImage != null);
            secondaryImage.sprite = visual.SecondaryImage;
        }

        if (MultText != null)
        {
            MultText.gameObject.SetActive(!string.IsNullOrWhiteSpace(visual.MultText));
        }
    }

    private ScenarioVisual GetScenarioVisual(Scenario scenario)
    {
        ScenarioVisual scenarioVisual = null;

        if (scenarioVisuals != null)
        {
            foreach (ScenarioVisual visual in scenarioVisuals)
            {
                if (visual != null && visual.Scenario == scenario)
                {
                    scenarioVisual = visual;
                    break;
                }
            }
        }

        scenarioVisual ??= CreateFallbackScenarioVisual(scenario);

        scenarioVisual.SetLocalizedTexts(
            GetLocalizedText(scenarioVisual.FinnishMessage, scenarioVisual.EnglishMessage),
            GetLocalizedText(scenarioVisual.FinnishMultText, scenarioVisual.EnglishMultText));

        return scenarioVisual;
    }

    private ScenarioVisual CreateFallbackScenarioVisual(Scenario scenario)
    {
        if (scenario == Scenario.StartCountdown)
        {
            ScenarioVisual baseVisual = GetFirstConfiguredVisual();
            return new ScenarioVisual
            {
                Scenario = scenario,
                FinnishMessage = "Aloita",
                EnglishMessage = "Start",
                BackgroundSprite = baseVisual?.BackgroundSprite,
                BackgroundColor = baseVisual != null ? baseVisual.BackgroundColor : new Color(0f, 0f, 0f, 0.55f),
                Effect = baseVisual?.Effect,
                EffectColor = baseVisual != null ? baseVisual.EffectColor : Color.white,
                TextColor = baseVisual != null ? baseVisual.TextColor : Color.white
            };
        }

        return new ScenarioVisual
        {
            Scenario = scenario,
            FinnishMessage = "Ry\u00f6st\u00f6 p\u00e4\u00e4ttyy.",
            EnglishMessage = "The raid is ending."
        };
    }

    private ScenarioVisual GetFirstConfiguredVisual()
    {
        if (scenarioVisuals == null)
        {
            return null;
        }

        foreach (ScenarioVisual visual in scenarioVisuals)
        {
            if (visual != null)
            {
                return visual;
            }
        }

        return null;
    }

    private string GetLocalizedText(string finnishText, string englishText)
    {
        if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
        {
            return string.IsNullOrWhiteSpace(englishText) ? finnishText : englishText;
        }

        return string.IsNullOrWhiteSpace(finnishText) ? englishText : finnishText;
    }

    private void SetLocalizedText(
        TextMeshProUGUI textField,
        string text,
        Color textColor)
    {
        if (textField == null)
        {
            return;
        }

        TextLanguageSelectorCaller textLanguageSelector = textField.GetComponent<TextLanguageSelectorCaller>();
        if (textLanguageSelector != null)
        {
            textLanguageSelector.SetText(text);
        }
        else
        {
            textField.text = text;
        }

        textField.color = textColor;
    }

    private void ApplyImage(Image image, Sprite sprite, Color color)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.color = color;
        image.type = sprite == null ? Image.Type.Simple : image.type;
    }
}
