using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Raid_EndMenu : MonoBehaviour
{
    public static Raid_EndMenu Instance { get; private set; }

    private static readonly Vector2 SpaceRemainingCloudMinimumSize = new Vector2(460f, 300f);

    [SerializeField] private RectTransform content;
    [SerializeField] private Raid_InventoryItem collectedLootItemPrefab;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private WindowNavigation lobbyNavigation;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite normalBackgroundSprite;
    [SerializeField] private Sprite overweightBackgroundSprite;
    [SerializeField] private GameObject normalEndResultText;
    [SerializeField] private GameObject overweightEndResultText;
    [SerializeField] private TMP_Text spaceRemainingText;
    [SerializeField] private Vector2 collectedLootGridCellSize = new Vector2(325f, 430f);
    [SerializeField] private Vector2 collectedLootGridSpacing = new Vector2(180f, 310f);
    [SerializeField] private int collectedLootGridPaddingTop = 80;
    [SerializeField] private int collectedLootGridPaddingBottom = 40;
    [SerializeField] private int collectedLootColumnCount = 3;

    private bool initialized;
    private bool collectedLootLossHaloVisible;
    private Raid_TextHalo spaceRemainingHalo;

    public GameObject MenuRoot
    {
        get
        {
            EnsureInitialized();
            return menuRoot;
        }
    }

    private void Awake()
    {
        EnsureInitialized();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        spaceRemainingHalo?.Dispose();
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void SetCollectedLoot(IReadOnlyList<GameFurniture> lootList)
    {
        ConfigureCollectedLootLayout();
        ClearCollectedLoot();

        if (content == null)
        {
            Debug.LogError("Cannot show collected raid loot because content is missing.");
            return;
        }

        if (lootList == null)
        {
            return;
        }

        foreach (GameFurniture loot in lootList)
        {
            CreateCollectedLootIcon(loot);
        }
    }

    public void SetLossHaloVisible(bool visible)
    {
        collectedLootLossHaloVisible = visible;
        SetCollectedLootHaloVisible(visible);
        SetSpaceRemainingHaloVisible(visible);
    }

    public void SetOverWeightLimitBackground(bool wentOverWeightLimit)
    {
        if (backgroundImage != null)
        {
            Sprite selectedBackground = wentOverWeightLimit ? overweightBackgroundSprite : normalBackgroundSprite;
            if (selectedBackground != null)
            {
                backgroundImage.sprite = selectedBackground;
            }
        }

    }

    public void SetEndReasonText(ExitRaid.RaidEndReason reason)
    {
        bool wentOverWeightLimit = reason == ExitRaid.RaidEndReason.OutOfSpace;

        if (normalEndResultText != null)
        {
            normalEndResultText.SetActive(!wentOverWeightLimit);
        }

        if (overweightEndResultText != null)
        {
            overweightEndResultText.SetActive(wentOverWeightLimit);
        }

    }

    public void SetSpaceRemainingText(float currentLootWeight, float maxLootWeight)
    {
        if (spaceRemainingText == null)
        {
            return;
        }

        spaceRemainingText.text = $"{currentLootWeight:F0} kg\n/{maxLootWeight:F0} kg";

        spaceRemainingHalo?.Sync();
    }

    public void ReturnToLobby()
    {
        if (lobbyNavigation != null)
        {
            StartCoroutine(lobbyNavigation.Navigate());
            return;
        }

        SceneManager.LoadScene("10-MenuUI");
    }

    public void Restart() 
    {
        RaidMatchmakingController.RestartNextSceneInDebugInventoryMode();
        SceneManager.LoadScene("40-Raid");
    }

    private void ClearCollectedLoot()
    {
        if (content == null)
        {
            return;
        }

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetSpaceRemainingHaloVisible(bool visible)
    {
        if (spaceRemainingText == null)
        {
            return;
        }

        if (spaceRemainingHalo == null)
        {
            if (!visible)
            {
                return;
            }

            Raid_TextHalo.Settings settings = Raid_TextHalo.CreateDefaultSettings(
                SpaceRemainingCloudMinimumSize,
                "SpaceRemainingText_RedCloudHalo");
            spaceRemainingHalo = new Raid_TextHalo(spaceRemainingText, settings);
        }

        spaceRemainingHalo.SetTarget(spaceRemainingText);
        spaceRemainingHalo.SetVisible(visible);
    }

    private void CreateCollectedLootIcon(GameFurniture furniture)
    {
        Sprite sprite = furniture?.FurnitureInfo?.Image;
        if (sprite == null)
        {
            return;
        }

        if (collectedLootItemPrefab != null)
        {
            Raid_InventoryItem lootItem = Instantiate(collectedLootItemPrefab, content);
            lootItem.name = "CollectedLootItem";
            lootItem.SetData(furniture);
            lootItem.SetLossHaloVisible(collectedLootLossHaloVisible);
            lootItem.SetShowItemWeightText(true);
            return;
        }

        Debug.LogError("Cannot show collected raid loot icon because collectedLootItemPrefab is missing from the EndMenu prefab.", this);
    }

    private void SetCollectedLootHaloVisible(bool visible)
    {
        if (content == null)
        {
            return;
        }

        foreach (Transform child in content)
        {
            Raid_InventoryItem lootItem = child.GetComponent<Raid_InventoryItem>();
            if (lootItem != null)
            {
                lootItem.SetLossHaloVisible(visible);
                continue;
            }
        }
    }

    private void SetVisible(bool visible)
    {
        EnsureInitialized();

        if (menuRoot == null)
        {
            Debug.LogError("Cannot set raid end menu visibility because menuRoot is missing from the EndMenu prefab.", this);
            return;
        }

        menuRoot.SetActive(visible);
    }

    private void ConfigureCollectedLootLayout()
    {
        if (content == null)
        {
            return;
        }

        GridLayoutGroup gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup == null)
        {
            return;
        }

        gridLayoutGroup.cellSize = collectedLootGridCellSize;
        gridLayoutGroup.spacing = collectedLootGridSpacing;
        gridLayoutGroup.padding = new RectOffset(0, 0, collectedLootGridPaddingTop, collectedLootGridPaddingBottom);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = collectedLootColumnCount;
        gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple Raid_EndMenu instances found. Using the first initialized instance.");
        }
        else
        {
            Instance = this;
        }

        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        initialized = true;
    }
}

public sealed class Raid_TextHalo
{
    private const int CloudTextureSize = 128;
    private static readonly Color DefaultCloudColor = new Color32(0xE6, 0x0A, 0x0A, 0xF2);
    private static readonly Color DefaultTextGlowColor = new Color32(0xE6, 0x0A, 0x0A, 0xB8);
    private static Sprite cloudSprite;

    private readonly Settings settings;
    private TMP_Text targetText;
    private bool visible;
    private Material originalMaterial;
    private Material haloMaterial;
    private bool ownsHaloMaterial;
    private RectTransform cloudRect;
    private Image cloudImage;

    public Raid_TextHalo(TMP_Text targetText, Settings settings, Material haloMaterial = null)
    {
        this.targetText = targetText;
        if (haloMaterial != null)
        {
            settings.HaloMaterial = haloMaterial;
        }

        this.settings = settings;
    }

    public static Settings CreateDefaultSettings(Vector2 minimumCloudSize, string cloudObjectName)
    {
        return new Settings
        {
            CloudColor = DefaultCloudColor,
            TextGlowColor = DefaultTextGlowColor,
            GlowOffset = 0.08f,
            GlowInner = 0.12f,
            GlowOuter = 1.7f,
            GlowPower = 0.14f,
            UnderlayAlpha = 0.32f,
            UnderlayDilate = 0.35f,
            UnderlaySoftness = 4.8f,
            CloudSizeMultiplier = new Vector2(2.2f, 2.15f),
            CloudMinimumSize = minimumCloudSize,
            CloudOffset = Vector2.zero,
            CloudObjectName = cloudObjectName
        };
    }

    public void SetTarget(TMP_Text targetText)
    {
        if (this.targetText == targetText)
        {
            return;
        }

        SetVisible(false);
        this.targetText = targetText;
    }

    public void SetVisible(bool isVisible)
    {
        if (targetText == null)
        {
            return;
        }

        visible = isVisible;

        if (visible)
        {
            EnsureHaloMaterial();
            if (haloMaterial != null)
            {
                targetText.fontMaterial = haloMaterial;
            }

            EnsureCloudHalo();
            Sync();
        }
        else if (originalMaterial != null)
        {
            targetText.fontMaterial = originalMaterial;
        }

        if (cloudImage != null)
        {
            cloudImage.gameObject.SetActive(visible);
        }

        targetText.UpdateMeshPadding();
        targetText.SetMaterialDirty();
    }

    public void Sync()
    {
        if (!visible || cloudRect == null || targetText == null)
        {
            return;
        }

        RectTransform textRect = targetText.rectTransform;
        if (textRect == null)
        {
            return;
        }

        targetText.ForceMeshUpdate(true, true);
        Bounds textBounds = targetText.textBounds;
        Vector2 textSize = new Vector2(textBounds.size.x, textBounds.size.y);

        if (textSize.x <= 0f || textSize.y <= 0f)
        {
            textSize = textRect.rect.size;
        }

        Vector2 cloudSize = new Vector2(
            Mathf.Max(textSize.x * settings.CloudSizeMultiplier.x, settings.CloudMinimumSize.x),
            Mathf.Max(textSize.y * settings.CloudSizeMultiplier.y, settings.CloudMinimumSize.y));

        Vector3 textCenter = textRect.localRotation * textBounds.center;
        Vector3 cloudOffset = new Vector3(settings.CloudOffset.x, settings.CloudOffset.y, 0f);

        cloudRect.anchorMin = Vector2.zero;
        cloudRect.anchorMax = Vector2.zero;
        cloudRect.pivot = new Vector2(0.5f, 0.5f);
        cloudRect.localPosition = textRect.localPosition + textCenter + cloudOffset;
        cloudRect.sizeDelta = cloudSize;
        cloudRect.localRotation = textRect.localRotation;
        cloudRect.localScale = textRect.localScale;

        if (cloudRect.GetSiblingIndex() > textRect.GetSiblingIndex())
        {
            cloudRect.SetSiblingIndex(textRect.GetSiblingIndex());
        }

        cloudRect.gameObject.SetActive(true);
    }

    public void Dispose()
    {
        if (haloMaterial != null && ownsHaloMaterial)
        {
            UnityEngine.Object.Destroy(haloMaterial);
        }

        haloMaterial = null;

        if (cloudRect != null)
        {
            UnityEngine.Object.Destroy(cloudRect.gameObject);
            cloudRect = null;
            cloudImage = null;
        }
    }

    private void EnsureCloudHalo()
    {
        if (cloudImage != null || targetText == null)
        {
            return;
        }

        RectTransform textRect = targetText.rectTransform;
        if (textRect == null || textRect.parent == null)
        {
            return;
        }

        string cloudName = string.IsNullOrEmpty(settings.CloudObjectName)
            ? $"{targetText.gameObject.name}_RedCloudHalo"
            : settings.CloudObjectName;

        GameObject cloudObject = new GameObject(cloudName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        cloudObject.transform.SetParent(textRect.parent, false);

        cloudRect = cloudObject.GetComponent<RectTransform>();
        cloudImage = cloudObject.GetComponent<Image>();
        cloudImage.sprite = GetCloudSprite();
        cloudImage.color = settings.CloudColor;
        cloudImage.raycastTarget = false;
        cloudImage.preserveAspect = false;

        LayoutElement layoutElement = cloudObject.GetComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        cloudObject.SetActive(false);
    }

    private void EnsureHaloMaterial()
    {
        if (haloMaterial != null || targetText == null)
        {
            return;
        }

        Material sourceMaterial = targetText.fontMaterial != null
            ? targetText.fontMaterial
            : targetText.fontSharedMaterial;

        if (sourceMaterial == null)
        {
            return;
        }

        originalMaterial = sourceMaterial;
        if (settings.HaloMaterial != null)
        {
            haloMaterial = settings.HaloMaterial;
            ownsHaloMaterial = false;
            return;
        }

        haloMaterial = new Material(sourceMaterial)
        {
            name = $"{sourceMaterial.name} Red Text Halo",
            hideFlags = HideFlags.HideAndDontSave
        };
        ownsHaloMaterial = true;

        haloMaterial.EnableKeyword("GLOW_ON");
        haloMaterial.EnableKeyword("UNDERLAY_ON");

        SetMaterialColor(haloMaterial, "_GlowColor", settings.TextGlowColor);
        SetMaterialFloat(haloMaterial, "_GlowOffset", settings.GlowOffset);
        SetMaterialFloat(haloMaterial, "_GlowInner", settings.GlowInner);
        SetMaterialFloat(haloMaterial, "_GlowOuter", settings.GlowOuter);
        SetMaterialFloat(haloMaterial, "_GlowPower", settings.GlowPower);

        Color underlayColor = settings.TextGlowColor;
        underlayColor.a *= settings.UnderlayAlpha;
        SetMaterialColor(haloMaterial, "_UnderlayColor", underlayColor);
        SetMaterialFloat(haloMaterial, "_UnderlayOffsetX", 0f);
        SetMaterialFloat(haloMaterial, "_UnderlayOffsetY", 0f);
        SetMaterialFloat(haloMaterial, "_UnderlayDilate", settings.UnderlayDilate);
        SetMaterialFloat(haloMaterial, "_UnderlaySoftness", settings.UnderlaySoftness);
    }

    private static Sprite GetCloudSprite()
    {
        if (cloudSprite != null)
        {
            return cloudSprite;
        }

        Texture2D texture = new Texture2D(CloudTextureSize, CloudTextureSize, TextureFormat.RGBA32, false)
        {
            name = "Raid Text Red Cloud Halo",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        for (int y = 0; y < CloudTextureSize; y++)
        {
            for (int x = 0; x < CloudTextureSize; x++)
            {
                float u = ((x + 0.5f) / CloudTextureSize) * 2f - 1f;
                float v = ((y + 0.5f) / CloudTextureSize) * 2f - 1f;
                float distance = Mathf.Sqrt((u * u) + (v * v));
                float fade = 1f - Mathf.SmoothStep(0.12f, 1f, distance);
                float alpha = Mathf.Pow(Mathf.Clamp01(fade), 1.35f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply(false, true);

        cloudSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, CloudTextureSize, CloudTextureSize),
            new Vector2(0.5f, 0.5f),
            100f);
        cloudSprite.name = "Raid Text Red Cloud Halo";
        cloudSprite.hideFlags = HideFlags.HideAndDontSave;

        return cloudSprite;
    }

    private static void SetMaterialColor(Material material, string propertyName, Color value)
    {
        if (material != null && material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }

    private static void SetMaterialFloat(Material material, string propertyName, float value)
    {
        if (material != null && material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    public struct Settings
    {
        public Color CloudColor;
        public Color TextGlowColor;
        public float GlowOffset;
        public float GlowInner;
        public float GlowOuter;
        public float GlowPower;
        public float UnderlayAlpha;
        public float UnderlayDilate;
        public float UnderlaySoftness;
        public Vector2 CloudSizeMultiplier;
        public Vector2 CloudMinimumSize;
        public Vector2 CloudOffset;
        public string CloudObjectName;
        public Material HaloMaterial;
    }
}
