using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ClanSearchResponsiveLayout : MonoBehaviour
{
    [System.Serializable]
    private class LayoutPreset
    {
        [Header("Popup scales")]
        public float clanPopupScale = 1f;
        public float filtersPopupScale = 1f;
        public float languageEditPopupScale = 1f;
        public float ageEditPopupScale = 1f;
    }

    [Header("Layout root")]
    [SerializeField] private RectTransform layoutRoot;

    [Header("Breakpoint")]
    [Tooltip("Width divided by height. Screens at or above this value use the tablet preset.")]
    [SerializeField] private float tabletAspectThreshold = 0.65f;

    [Header("Phone preset")]
    [SerializeField]
    private LayoutPreset phonePreset = new LayoutPreset
    {
        clanPopupScale = 1f,
        filtersPopupScale = 1f,
        languageEditPopupScale = 1f,
        ageEditPopupScale = 1f
    };

    [Header("Tablet preset")]
    [SerializeField]
    private LayoutPreset tabletPreset = new LayoutPreset
    {
        clanPopupScale = 0.8f,
        filtersPopupScale = 0.8f,
        languageEditPopupScale = 0.8f,
        ageEditPopupScale = 0.8f
    };

    [Header("Popups")]
    [SerializeField] private RectTransform clanPopup;
    [SerializeField] private RectTransform filtersPopup;
    [SerializeField] private RectTransform languageEditPopup;
    [SerializeField] private RectTransform ageEditPopup;

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
        if (Screen.width == lastScreenWidth &&
            Screen.height == lastScreenHeight)
        {
            return;
        }

        ApplyLayout();
    }

    private void ApplyLayout()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        LayoutPreset preset = IsTabletLikeScreen()
            ? tabletPreset
            : phonePreset;

        SetPopupScale(clanPopup, preset.clanPopupScale);
        SetPopupScale(filtersPopup, preset.filtersPopupScale);
        SetPopupScale(languageEditPopup, preset.languageEditPopupScale);
        SetPopupScale(ageEditPopup, preset.ageEditPopupScale);

        ForceRebuild();
    }

    private bool IsTabletLikeScreen()
    {
        if (Screen.height <= 0)
            return false;

        float aspectRatio = (float)Screen.width / Screen.height;
        return aspectRatio >= tabletAspectThreshold;
    }

    private void SetPopupScale(RectTransform popup, float scale)
    {
        if (popup == null)
            return;

        popup.localScale = new Vector3(scale, scale, 1f);
    }

    private void ForceRebuild()
    {
        if (layoutRoot == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
    }
}
