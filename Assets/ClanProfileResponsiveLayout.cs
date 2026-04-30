using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ClanProfileResponsiveLayout : MonoBehaviour
{
    private void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;

        ApplyLayout();
    }

    [System.Serializable]
    private class LayoutPreset
    {
        [Header("Main section heights")]
        public float headerHeight = 200f;
        public float heroRowHeight = 550f;
        public float clanMoodHeight = 230f;

        [Header("Popups")]
        public float rulesPopupScale = 1f;
        public float carbonEmissionPopupScale = 1f;

        [Header("Header right icons")]
        public Vector2 headerInfoIconSize = new Vector2(140f, 120f);

        [Header("Hero left side")]
        public float logoColumnWidth = 190f;
        public Vector2 clanLogoSize = new Vector2(170f, 170f);
        public Vector2 rankingWinsSize = new Vector2(150f, 100f);

        [Header("Hero right side")]
        public float ageLanguageHeight = 250f;
        public Vector2 ageSettingSize = new Vector2(120f, 120f);
        public Vector2 languageSettingSize = new Vector2(120f, 120f);
        public float valuesHeight = 100f;
        public float clanRulesHeight = 100f;

        [Header("Values panel")]
        public Vector2 valuesGridCellSize = new Vector2(440f, 45f);

        [Header("Clan mood")]
        public float moodLabelHeight = 45f;
        public float moodLabelMaxFontSize = 30f;
        public float moodBoardHeight = 170f;
    }

    [Header("Layout root")]
    [SerializeField] private RectTransform layoutRoot;

    [Header("Breakpoint")]
    [SerializeField] private float tabletAspectThreshold = 0.65f;

    [Header("Phone preset")]
    [SerializeField]
    private LayoutPreset phonePreset = new LayoutPreset
    {
        headerHeight = 200f,
        heroRowHeight = 550f,
        clanMoodHeight = 230f,

        rulesPopupScale = 1f,
        carbonEmissionPopupScale = 1f,

        headerInfoIconSize = new Vector2(140f, 120f),

        logoColumnWidth = 190f,
        clanLogoSize = new Vector2(170f, 170f),
        rankingWinsSize = new Vector2(150f, 100f),

        ageLanguageHeight = 250f,
        ageSettingSize = new Vector2(120f, 120f),
        languageSettingSize = new Vector2(120f, 120f),
        valuesHeight = 100f,
        clanRulesHeight = 100f,

        valuesGridCellSize = new Vector2(440f, 45f),

        moodLabelHeight = 45f,
        moodLabelMaxFontSize = 30f,
        moodBoardHeight = 170f
    };

    [Header("Tablet preset")]
    [SerializeField]
    private LayoutPreset tabletPreset = new LayoutPreset
    {
        headerHeight = 150f,
        heroRowHeight = 350f,
        clanMoodHeight = 170f,

        rulesPopupScale = 0.8f,
        carbonEmissionPopupScale = 0.75f,

        headerInfoIconSize = new Vector2(110f, 90f),

        logoColumnWidth = 150f,
        clanLogoSize = new Vector2(130f, 130f),
        rankingWinsSize = new Vector2(120f, 80f),

        ageLanguageHeight = 180f,
        ageSettingSize = new Vector2(90f, 90f),
        languageSettingSize = new Vector2(90f, 90f),
        valuesHeight = 80f,
        clanRulesHeight = 80f,

        valuesGridCellSize = new Vector2(440f, 38f),

        moodLabelHeight = 35f,
        moodLabelMaxFontSize = 24f,
        moodBoardHeight = 120f
    };

    [Header("Main sections")]
    [SerializeField] private LayoutElement clanProfileHeader;
    [SerializeField] private LayoutElement clanProfileHeroRow;
    [SerializeField] private LayoutElement clanMood;

    [Header("Header children")]
    [SerializeField] private LayoutElement clanLockSetting;
    [SerializeField] private LayoutElement memberCount;
    [SerializeField] private LayoutElement headerRules;
    [SerializeField] private LayoutElement carbonEmission;

    [Header("Hero left children")]
    [SerializeField] private LayoutElement logoRankingStar;
    [SerializeField] private LayoutElement clanLogo;
    [SerializeField] private LayoutElement rankingWins;

    [Header("Hero right children")]
    [SerializeField] private LayoutElement ageLanguage;
    [SerializeField] private LayoutElement clanAgeSetting;
    [SerializeField] private LayoutElement clanLanguageSetting;
    [SerializeField] private LayoutElement values;
    [SerializeField] private LayoutElement clanRules;

    [Header("Values panel")]
    [SerializeField] private GridLayoutGroup valuesPanelGrid;
    [SerializeField] private bool autoFitValuesGridWidth = true;

    [Header("Clan mood children")]
    [SerializeField] private LayoutElement moodLabel;
    [SerializeField] private TextMeshProUGUI moodLabelText;
    [SerializeField] private LayoutElement clanMoodBoard;

    [Header("Popups")]
    [SerializeField] private RectTransform rulesPopup;
    [SerializeField] private RectTransform carbonEmissionPopup;


    private int lastScreenWidth;
    private int lastScreenHeight;
    private bool lastWasTablet;

    private void Awake()
    {
        if (layoutRoot == null)
        {
            layoutRoot = transform as RectTransform;
        }
    }

    private void OnEnable()
    {
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

        bool isTablet = IsTabletLikeScreen();
        lastWasTablet = isTablet;

        LayoutPreset preset = isTablet ? tabletPreset : phonePreset;

        ApplyMainSections(preset);
        ApplyHeaderChildren(preset);
        ApplyHeroLeft(preset);
        ApplyHeroRight(preset);
        ApplyValuesPanel(preset);
        ApplyClanMood(preset);
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

    private void ApplyMainSections(LayoutPreset preset)
    {
        SetPreferredHeight(clanProfileHeader, preset.headerHeight);
        SetPreferredHeight(clanProfileHeroRow, preset.heroRowHeight);
        SetPreferredHeight(clanMood, preset.clanMoodHeight);
    }

    private void ApplyHeaderChildren(LayoutPreset preset)
    {
        if (clanLockSetting != null)
        {
            float lockSize = lastWasTablet ? 55f : 70f;
            SetPreferredSize(clanLockSetting, new Vector2(lockSize, lockSize));
        }

        SetPreferredSize(memberCount, preset.headerInfoIconSize);
        SetPreferredSize(headerRules, preset.headerInfoIconSize);
        SetPreferredSize(carbonEmission, preset.headerInfoIconSize);
    }

    private void ApplyHeroLeft(LayoutPreset preset)
    {
        SetPreferredWidth(logoRankingStar, preset.logoColumnWidth);
        SetPreferredSize(clanLogo, preset.clanLogoSize);
        SetPreferredSize(rankingWins, preset.rankingWinsSize);
    }

    private void ApplyHeroRight(LayoutPreset preset)
    {
        SetPreferredHeight(ageLanguage, preset.ageLanguageHeight);
        SetPreferredSize(clanAgeSetting, preset.ageSettingSize);
        SetPreferredSize(clanLanguageSetting, preset.languageSettingSize);

        SetPreferredHeight(values, preset.valuesHeight);
        SetPreferredHeight(clanRules, preset.clanRulesHeight);
    }

    private void ApplyValuesPanel(LayoutPreset preset)
    {
        if (valuesPanelGrid == null)
            return;

        valuesPanelGrid.cellSize = preset.valuesGridCellSize;
        valuesPanelGrid.spacing = new Vector2(0f, 10f);

        valuesPanelGrid.startCorner = GridLayoutGroup.Corner.UpperRight;
        valuesPanelGrid.startAxis = GridLayoutGroup.Axis.Vertical;
        valuesPanelGrid.childAlignment = TextAnchor.UpperRight;

        valuesPanelGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        valuesPanelGrid.constraintCount = 1;
    }

    private void ApplyClanMood(LayoutPreset preset)
    {
        SetPreferredHeight(moodLabel, preset.moodLabelHeight);
        SetPreferredHeight(clanMoodBoard, preset.moodBoardHeight);

        if (moodLabelText != null)
        {
            moodLabelText.enableAutoSizing = true;
            moodLabelText.fontSizeMax = preset.moodLabelMaxFontSize;
        }
    }

    private void ApplyPopups(LayoutPreset preset)
    {
        SetPopupScale(rulesPopup, preset.rulesPopupScale);
        SetPopupScale(carbonEmissionPopup, preset.carbonEmissionPopupScale);
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

        element.preferredHeight = height;
        element.flexibleHeight = 0f;
    }

    private void SetPreferredWidth(LayoutElement element, float width)
    {
        if (element == null)
            return;

        element.preferredWidth = width;
        element.flexibleWidth = 0f;
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
