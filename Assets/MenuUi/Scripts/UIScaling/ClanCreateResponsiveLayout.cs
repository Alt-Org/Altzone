using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class ClanCreateResponsiveLayout : MonoBehaviour
{
    [System.Serializable]
    private class LayoutPreset
    {
        [Header("Main create panel")]
        public float createPanelWidth = 1080f;
        public float createPanelHeight = 655f;

        [Header("Name / heart section")]
        public float nameHeartHeight = 210f;
        public Vector2 clanLogoSize = new Vector2(170f, 170f);
        public float clanNameFieldHeight = 65f;

        [Header("Age / lock / language section")]
        public float ageLockLanguageHeight = 190f;
        public Vector2 settingItemSize = new Vector2(333f, 240f);
        public Vector2 settingLabelSize = new Vector2(160f, 55f);
        public Vector2 settingImageSize = new Vector2(150f, 150f);

        [Header("Values section")]
        public float valuesHeight = 250f;
        public Vector2 valuesCellSize = new Vector2(440f, 45f);
        public float valuesLabelHeight = 35f;

        [Header("Popups")]
        public float languagePopupScale = 1f;
        public float heartEditPopupScale = 1f;
        public float agePopupScale = 1f;
        public float clanAgreementPopupScale = 1f;
    }

    [Header("Layout root")]
    [SerializeField] private RectTransform layoutRoot;

    [Header("Breakpoint")]
    [SerializeField] private float tabletAspectThreshold = 0.65f;

    [Header("Phone preset")]
    [SerializeField]
    private LayoutPreset phonePreset = new LayoutPreset
    {
        createPanelWidth = 1080f,
        createPanelHeight = 655f,

        nameHeartHeight = 300f,
        clanLogoSize = new Vector2(300f, 300f),
        clanNameFieldHeight = 110f,

        ageLockLanguageHeight = 300f,
        settingItemSize = new Vector2(333f, 240f),
        settingLabelSize = new Vector2(160f, 55f),
        settingImageSize = new Vector2(150f, 150f),

        valuesHeight = 250f,
        valuesCellSize = new Vector2(440f, 45f),
        valuesLabelHeight = 35f,

        languagePopupScale = 1f,
        heartEditPopupScale = 1f,
        agePopupScale = 1f,
        clanAgreementPopupScale = 1f
    };

    [Header("Tablet preset")]
    [SerializeField]
    private LayoutPreset tabletPreset = new LayoutPreset
    {
        createPanelWidth = 850f,
        createPanelHeight = 500f,

        nameHeartHeight = 150f,
        clanLogoSize = new Vector2(120f, 120f),
        clanNameFieldHeight = 50f,

        ageLockLanguageHeight = 190f,
        settingItemSize = new Vector2(230f, 150f),
        settingLabelSize = new Vector2(120f, 40f),
        settingImageSize = new Vector2(95f, 95f),

        valuesHeight = 180f,
        valuesCellSize = new Vector2(360f, 36f),
        valuesLabelHeight = 28f,

        languagePopupScale = 0.8f,
        heartEditPopupScale = 0.8f,
        agePopupScale = 0.8f,
        clanAgreementPopupScale = 0.8f
    };

    [Header("Main sections")]
    [SerializeField] private LayoutElement mainCreatePanel;
    [SerializeField] private LayoutElement clanNameHeart;
    [SerializeField] private LayoutElement ageLockLanguage;
    [SerializeField] private LayoutElement clanValues;

    [Header("Name / heart children")]
    [SerializeField] private LayoutElement clanLogo;
    [SerializeField] private LayoutElement clanNameField;

    [Header("Age / lock / language children")]
    [SerializeField] private LayoutElement languageSetting;
    [SerializeField] private LayoutElement clanAgeSetting;
    [SerializeField] private LayoutElement toggleLock;

    [SerializeField] private LayoutElement languageLabel;
    [SerializeField] private LayoutElement ageLabel;
    [SerializeField] private LayoutElement lockLabel;

    [SerializeField] private LayoutElement languageIcon;
    [SerializeField] private LayoutElement ageIcon;
    [SerializeField] private LayoutElement lockIcon;

    [Header("Values children")]
    [SerializeField] private LayoutElement labelBackground;
    [SerializeField] private GridLayoutGroup allLabelsGrid;
    [SerializeField] private TextMeshProUGUI valuesInstructionText;

    [Header("Popups")]
    [SerializeField] private RectTransform languagePopup;
    [SerializeField] private RectTransform clanHeartEditPopup;
    [SerializeField] private RectTransform agePopup;
    [SerializeField] private RectTransform clanAgreementPopup;

    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        if (layoutRoot == null)
            layoutRoot = transform as RectTransform;
    }

    private void OnEnable()
    {
        ApplyLayout();
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;

        ApplyLayout();
    }

    private void Update()
    {
        if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
            return;

        ApplyLayout();
    }

    private void ApplyLayout()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        LayoutPreset preset = IsTabletLikeScreen() ? tabletPreset : phonePreset;

        ApplyMainPanel(preset);
        ApplyNameHeartSection(preset);
        ApplyAgeLockLanguageSection(preset);
        ApplyValuesSection(preset);
        ApplyPopups(preset);

        ForceRebuild();
    }

    private bool IsTabletLikeScreen()
    {
        if (Screen.height <= 0)
            return false;

        float aspect = (float)Screen.width / Screen.height;
        return aspect >= tabletAspectThreshold;
    }

    private void ApplyMainPanel(LayoutPreset preset)
    {
        SetPreferredSize(mainCreatePanel, new Vector2(preset.createPanelWidth, preset.createPanelHeight));

        SetPreferredHeight(clanNameHeart, preset.nameHeartHeight);
        SetPreferredHeight(ageLockLanguage, preset.ageLockLanguageHeight);
        SetPreferredHeight(clanValues, preset.valuesHeight);
    }

    private void ApplyNameHeartSection(LayoutPreset preset)
    {
        SetPreferredHeight(clanNameHeart, preset.nameHeartHeight);
        SetPreferredSize(clanLogo, preset.clanLogoSize);
        SetPreferredHeight(clanNameField, preset.clanNameFieldHeight);
    }
    private void ApplyAgeLockLanguageSection(LayoutPreset preset)
    {
        SetPreferredSize(languageSetting, preset.settingItemSize);
        SetPreferredSize(clanAgeSetting, preset.settingItemSize);
        SetPreferredSize(toggleLock, preset.settingItemSize);

        SetPreferredSize(languageLabel, preset.settingLabelSize);
        SetPreferredSize(ageLabel, preset.settingLabelSize);
        SetPreferredSize(lockLabel, preset.settingLabelSize);

        SetPreferredSize(languageIcon, preset.settingImageSize);
        SetPreferredSize(ageIcon, preset.settingImageSize);
        SetPreferredSize(lockIcon, preset.settingImageSize);
    }

    private void ApplyValuesSection(LayoutPreset preset)
    {
        SetPreferredHeight(labelBackground, preset.valuesLabelHeight);

        if (valuesInstructionText != null)
        {
            valuesInstructionText.enableAutoSizing = true;
            valuesInstructionText.fontSizeMax = IsTabletLikeScreen() ? 18f : 24f;
        }

        if (allLabelsGrid != null)
        {
            allLabelsGrid.cellSize = preset.valuesCellSize;
            allLabelsGrid.spacing = IsTabletLikeScreen()
                ? new Vector2(0f, 6f)
                : new Vector2(0f, 10f);

            allLabelsGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            allLabelsGrid.startAxis = GridLayoutGroup.Axis.Vertical;
            allLabelsGrid.childAlignment = TextAnchor.UpperLeft;
            allLabelsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            allLabelsGrid.constraintCount = 2;
        }
    }

    private void ApplyPopups(LayoutPreset preset)
    {
        SetPopupScale(languagePopup, preset.languagePopupScale);
        SetPopupScale(clanHeartEditPopup, preset.heartEditPopupScale);
        SetPopupScale(agePopup, preset.agePopupScale);
        SetPopupScale(clanAgreementPopup, preset.clanAgreementPopupScale);
    }

    private void SetPopupScale(RectTransform popup, float scale)
    {
        if (popup == null)
            return;

        popup.localScale = new Vector3(scale, scale, 1f);
    }

    private void SetPreferredHeight(LayoutElement element, float height)
    {
        if (element == null)
            return;

        element.minHeight = height;
        element.preferredHeight = height;
        element.flexibleHeight = 0f;
        element.layoutPriority = 10;
    }

    private void SetPreferredSize(LayoutElement element, Vector2 size)
    {
        if (element == null)
            return;

        element.preferredWidth = size.x;
        element.preferredHeight = size.y;
        element.flexibleWidth = 0f;
        element.flexibleHeight = 0f;
    }

    private void ForceRebuild()
    {
        if (layoutRoot == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
    }
}
