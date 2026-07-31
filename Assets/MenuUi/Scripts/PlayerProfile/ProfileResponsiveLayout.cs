using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ProfileResponsiveLayout : MonoBehaviour
{
    [System.Serializable]
    public class LayoutProfile
    {
        [Header("Left column")]
        public int leftColumnTopPadding = 0;
        public int leftColumnBottomPadding = 0;
        public float leftColumnSpacing = 0f;

        [Header("Top group")]
        public int topPadding = 0;
        public int topBottomPadding = 0;
        public float topSpacing = 10f;

        [Header("Bottom group")]
        public int bottomPadding = 0;
        public int bottomBottomPadding = 0;
        public float bottomSpacing = 10f;

        [Header("Section heights")]
        public float namePlateHeight = 140f;
        public float statsHeight = 120f;
        public float favoriteDefenceHeight = 130f;
        public float clanPlateHeight = 110f;
        public float rolesHeight = 80f;

        [Header("Optional element sizes")]
        public float clanLogoSize = 80f;
        public float roleIconSize = 70f;

        [Header("Optional character tuning")]
        public Vector3 characterScale = Vector3.one;
        public Vector2 characterAnchoredPosition = Vector2.zero;
    }

    [Header("Detection")]
    [Tooltip("If the shortest screen side is at least this many pixels, tablet profile is used.")]
    [SerializeField] private int tabletShortestSideThreshold = 900;

    [SerializeField] private bool autoApplyInEditor = true;

    [Header("Layout references")]
    [SerializeField] private VerticalLayoutGroup leftColumnLayout;
    [SerializeField] private VerticalLayoutGroup topLayout;
    [SerializeField] private VerticalLayoutGroup bottomLayout;

    [Header("Section LayoutElements")]
    [SerializeField] private LayoutElement namePlateLayout;
    [SerializeField] private LayoutElement statsLayout;
    [SerializeField] private LayoutElement favoriteDefenceLayout;
    [SerializeField] private LayoutElement clanPlateLayout;
    [SerializeField] private LayoutElement rolesLayout;

    [Header("Optional size LayoutElements")]
    [SerializeField] private LayoutElement clanLogoLayout;
    [SerializeField] private LayoutElement roleIconLayout;

    [Header("Optional character")]
    [SerializeField] private RectTransform characterRect;

    [Header("Profiles")]
    [SerializeField]
    private LayoutProfile phoneProfile = new LayoutProfile
    {
        leftColumnTopPadding = 10,
        leftColumnBottomPadding = 0,
        leftColumnSpacing = 8f,

        topPadding = 8,
        topBottomPadding = 0,
        topSpacing = 10f,

        bottomPadding = 8,
        bottomBottomPadding = 0,
        bottomSpacing = 10f,

        namePlateHeight = 140f,
        statsHeight = 120f,
        favoriteDefenceHeight = 130f,
        clanPlateHeight = 110f,
        rolesHeight = 80f,

        clanLogoSize = 80f,
        roleIconSize = 70f,

        characterScale = new Vector3(0.9f, 0.9f, 1f),
        characterAnchoredPosition = new Vector2(0f, 0f)
    };

    [SerializeField]
    private LayoutProfile tabletProfile = new LayoutProfile
    {
        leftColumnTopPadding = 0,
        leftColumnBottomPadding = 0,
        leftColumnSpacing = 12f,

        topPadding = 0,
        topBottomPadding = 0,
        topSpacing = 12f,

        bottomPadding = 0,
        bottomBottomPadding = 0,
        bottomSpacing = 12f,

        namePlateHeight = 165f,
        statsHeight = 145f,
        favoriteDefenceHeight = 160f,
        clanPlateHeight = 125f,
        rolesHeight = 95f,

        clanLogoSize = 96f,
        roleIconSize = 85f,

        characterScale = Vector3.one,
        characterAnchoredPosition = new Vector2(0f, 0f)
    };

    private int lastWidth = -1;
    private int lastHeight = -1;

    private void OnEnable()
    {
        ApplyCurrentProfile();
    }

    private void Update()
    {
        if (!Application.isPlaying && !autoApplyInEditor)
            return;

        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            ApplyCurrentProfile();
        }
    }

    private void OnValidate()
    {
        if (!autoApplyInEditor)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += DelayedApply;
#endif
    }

#if UNITY_EDITOR
    private void DelayedApply()
    {
        if (this == null)
            return;

        ApplyCurrentProfile();
    }
#endif

    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying && !autoApplyInEditor)
            return;

        ApplyCurrentProfile();
    }

    [ContextMenu("Apply Current Profile")]
    public void ApplyCurrentProfile()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        LayoutProfile profile = IsTablet() ? tabletProfile : phoneProfile;
        ApplyProfile(profile);

        Debug.Log(IsTablet() ? "Applied tablet profile" : "Applied phone profile");
    }

    private bool IsTablet()
    {
        int shortestSide = Mathf.Min(Screen.width, Screen.height);
        return shortestSide >= tabletShortestSideThreshold;
    }

    private void ApplyProfile(LayoutProfile profile)
    {
        ApplyLayoutGroup(leftColumnLayout, profile.leftColumnTopPadding, profile.leftColumnBottomPadding, profile.leftColumnSpacing);
        ApplyLayoutGroup(topLayout, profile.topPadding, profile.topBottomPadding, profile.topSpacing);
        ApplyLayoutGroup(bottomLayout, profile.bottomPadding, profile.bottomBottomPadding, profile.bottomSpacing);

        SetPreferredHeight(namePlateLayout, profile.namePlateHeight);
        SetPreferredHeight(statsLayout, profile.statsHeight);
        SetPreferredHeight(favoriteDefenceLayout, profile.favoriteDefenceHeight);
        SetPreferredHeight(clanPlateLayout, profile.clanPlateHeight);
        SetPreferredHeight(rolesLayout, profile.rolesHeight);

        SetSquareSize(clanLogoLayout, profile.clanLogoSize);
        SetSquareSize(roleIconLayout, profile.roleIconSize);

        if (characterRect != null)
        {
            characterRect.localScale = profile.characterScale;
            characterRect.anchoredPosition = profile.characterAnchoredPosition;
        }
    }

    private void ApplyLayoutGroup(VerticalLayoutGroup layoutGroup, int topPadding, int bottomPadding, float spacing)
    {
        if (layoutGroup == null)
            return;

        RectOffset padding = layoutGroup.padding;
        padding.top = topPadding;
        padding.bottom = bottomPadding;
        layoutGroup.padding = padding;
        layoutGroup.spacing = spacing;

        LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.GetComponent<RectTransform>());
    }

    private void SetPreferredHeight(LayoutElement element, float height)
    {
        if (element == null)
            return;

        element.preferredHeight = height;
    }

    private void SetSquareSize(LayoutElement element, float size)
    {
        if (element == null)
            return;

        element.preferredWidth = size;
        element.preferredHeight = size;
    }
}
