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
        PlayerExit
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
        ApplyScenarioVisual(scenario);

        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(duration >= 0f ? duration : showTime);
        gameObject.SetActive(false);
    }

    private void ApplyScenarioVisual(Scenario scenario)
    {
        ScenarioVisual visual = GetScenarioVisual(scenario);

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

        scenarioVisual ??= new ScenarioVisual
        {
            Scenario = scenario,
            FinnishMessage = "Ry\u00f6st\u00f6 p\u00e4\u00e4ttyy.",
            EnglishMessage = "The raid is ending."
        };

        scenarioVisual.SetLocalizedTexts(
            GetLocalizedText(scenarioVisual.FinnishMessage, scenarioVisual.EnglishMessage),
            GetLocalizedText(scenarioVisual.FinnishMultText, scenarioVisual.EnglishMultText));

        return scenarioVisual;
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
