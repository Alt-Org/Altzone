using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Raid_EventPopup : MonoBehaviour
{
    public enum Scenario
    {
        EndTrap,
        Freeze,
        DoubleWeight,
        OutOfTime,
        OutOfSpace
    }

    [Serializable]
    private class ScenarioVisual
    {
        public Scenario Scenario;
        public string Title;
        public string Message;
        public Sprite BackgroundSprite = null;
        public Color BackgroundColor = new Color(0f, 0f, 0f, 0.55f);
        public Color TextColor = Color.white;
        public Sprite Image = null;
    }

    private const string PopupResourcePath = "Prefabs/RaidEventPopup";
    private const float DefaultShowTime = 1.75f;

    private static Raid_EventPopup instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
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

    public static IEnumerator ShowAndWait(MonoBehaviour owner, Scenario scenario, Action onComplete = null)
    {
        Raid_EventPopup popup = GetOrCreate();
        if (popup == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        yield return popup.ShowRoutine(scenario);
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

    private IEnumerator ShowRoutine(Scenario scenario)
    {
        ApplyScenarioVisual(scenario);

        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(showTime);
        gameObject.SetActive(false);
    }

    private void ApplyScenarioVisual(Scenario scenario)
    {
        ScenarioVisual visual = GetScenarioVisual(scenario);

        titleText.text = visual.Title;
        messageText.text = visual.Message;
        titleText.color = visual.TextColor;
        messageText.color = visual.TextColor;

        ApplyImage(backgroundImage, visual.BackgroundSprite, visual.BackgroundColor);

        if (image != null)
        {
            image.sprite = visual.Image;
        }
    }

    private ScenarioVisual GetScenarioVisual(Scenario scenario)
    {
        if (scenarioVisuals != null)
        {
            foreach (ScenarioVisual visual in scenarioVisuals)
            {
                if (visual != null && visual.Scenario == scenario)
                {
                    return visual;
                }
            }
        }

        return new ScenarioVisual
        {
            Scenario = scenario,
            Title = "Trap triggered!",
            Message = "The raid is ending."
        };
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
