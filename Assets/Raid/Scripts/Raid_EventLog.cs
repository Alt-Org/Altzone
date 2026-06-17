using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Raid_EventLog : MonoBehaviour
{
    private static readonly Color[] IconColors =
    {
        new(1f, 0.62f, 0.12f, 1f),
        new(0.71f, 0.36f, 1f, 1f),
        new(0.21f, 0.78f, 1f, 1f),
        new(0.46f, 0.82f, 0.37f, 1f)
    };
    private static readonly AvatarPiece[] AvatarPieces =
    {
        AvatarPiece.Hair,
        AvatarPiece.Eyes,
        AvatarPiece.Nose,
        AvatarPiece.Mouth,
        AvatarPiece.Clothes,
        AvatarPiece.Feet,
        AvatarPiece.Hands
    };

    [SerializeField, Min(1)] private int maxEntries = 80;
    [SerializeField, Min(1)] private int visibleEntryCount = 2;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private GameObject entryTemplate;
    [SerializeField] private AvatarFaceLoader avatarHeadTemplate;
    [SerializeField] private Color normalEntryColor = new(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color trapEntryColor = new(1f, 1f, 1f, 0.95f);

    private readonly Queue<GameObject> entries = new();
    private Coroutine scrollRoutine;
    private bool warnedMissingReferences;

    public static Raid_EventLog FindForInventory(Transform inventoryRoot)
    {
        return inventoryRoot != null
            ? inventoryRoot.GetComponentInChildren<Raid_EventLog>(true)
            : FindObjectOfType<Raid_EventLog>(true);
    }

    private void Awake()
    {
        if (entryTemplate != null)
        {
            entryTemplate.SetActive(false);
        }
    }

    public void Clear()
    {
        foreach (GameObject entry in entries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }

        entries.Clear();
        if (entryTemplate != null)
        {
            entryTemplate.SetActive(false);
        }
    }

    public void LogLootTaken(string playerName, GameFurniture furniture, float lootWeightMultiplier = 1f, CharacterID characterId = CharacterID.None, AvatarData avatarData = null)
    {
        if (furniture == null)
        {
            return;
        }

        double addedWeight = furniture.Weight * Math.Max(0f, lootWeightMultiplier);
        string actorName = FormatActorName(playerName);
        string message = UseEnglish()
            ? $"{actorName} took {GetFurnitureName(furniture)} {FormatNumber(addedWeight)} kg"
            : $"{actorName} nappasi \u00E4sken {GetFurnitureName(furniture)} {FormatNumber(addedWeight)} kg";
        AddEntry(actorName, message, normalEntryColor, characterId, avatarData);
    }

    public void LogTrapTriggered(string playerName, int trapType, CharacterID characterId = CharacterID.None, AvatarData avatarData = null)
    {
        string actorName = FormatActorName(playerName);
        string message = UseEnglish()
            ? $"{actorName} triggered {GetTrapName(trapType)} trap"
            : $"{actorName} laukaisi \u00E4sken ansan";
        AddEntry(actorName, message, trapEntryColor, characterId, avatarData);
    }

    private void AddEntry(string actorName, string message, Color color, CharacterID characterId, AvatarData avatarData)
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        entryTemplate.SetActive(false);
        GameObject entry = Instantiate(entryTemplate, contentRoot);
        entry.name = "EventLogEntry";
        ConfigureEntry(entry, actorName, message, color, characterId, avatarData);
        entry.SetActive(true);
        entries.Enqueue(entry);

        while (entries.Count > maxEntries)
        {
            GameObject removedEntry = entries.Dequeue();
            if (removedEntry != null)
            {
                Destroy(removedEntry);
            }
        }

        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
        }

        scrollRoutine = StartCoroutine(ScrollToBottomNextFrame());
    }

    private void ConfigureEntry(GameObject entry, string actorName, string message, Color color, CharacterID characterId, AvatarData avatarData)
    {
        TMP_Text messageText = FindMessageText(entry.transform);

        ApplyEntryLayout(entry, messageText);

        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }

        ConfigureIcon(FindAvatarSlot(entry.transform), actorName, characterId, avatarData);
    }

    private static TMP_Text FindMessageText(Transform entryTransform)
    {
        if (entryTransform == null)
        {
            return null;
        }

        TMP_Text messageText = entryTransform.Find("Message")?.GetComponent<TMP_Text>();
        if (messageText != null)
        {
            return messageText;
        }

        messageText = entryTransform.GetComponent<TMP_Text>();
        return messageText != null
            ? messageText
            : entryTransform.GetComponentInChildren<TMP_Text>(true);
    }

    private static Transform FindAvatarSlot(Transform entryTransform)
    {
        return entryTransform == null
            ? null
            : entryTransform.Find("Profile Image") ?? entryTransform.Find("Icon");
    }

    private void ConfigureIcon(Transform iconTransform, string actorName, CharacterID characterId, AvatarData avatarData)
    {
        if (iconTransform == null)
        {
            return;
        }

        Image iconImage = iconTransform.GetComponent<Image>();
        if (TryConfigureAvatarIcon(iconTransform, iconImage, characterId, avatarData))
        {
            return;
        }

        SetAvatarHeadVisible(iconTransform, false);

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.color = ResolveIconColor(actorName);
            iconImage.preserveAspect = false;
        }
    }

    private bool TryConfigureAvatarIcon(Transform iconTransform, Image iconImage, CharacterID characterId, AvatarData avatarData)
    {
        if (avatarData == null)
        {
            return false;
        }

        AvatarVisualData visualData = CreateAvatarVisualData(avatarData, characterId);
        if (visualData == null)
        {
            return false;
        }

        AvatarFaceLoader avatarFaceLoader = iconTransform.GetComponentInChildren<AvatarFaceLoader>(true);
        if (avatarFaceLoader == null)
        {
            if (avatarHeadTemplate == null)
            {
                return false;
            }

            avatarFaceLoader = Instantiate(avatarHeadTemplate, iconTransform);
            avatarFaceLoader.name = "CharacterHeadImage";
        }

        avatarFaceLoader.SetUseOwnAvatarVisuals(false);
        DisableAvatarRaycasts(avatarFaceLoader);

        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.raycastTarget = false;
        }

        avatarFaceLoader.gameObject.SetActive(true);
        avatarFaceLoader.UpdateVisuals(visualData);
        return true;
    }

    private static void SetAvatarHeadVisible(Transform iconTransform, bool isVisible)
    {
        if (iconTransform == null)
        {
            return;
        }

        AvatarFaceLoader avatarFaceLoader = iconTransform.GetComponentInChildren<AvatarFaceLoader>(true);
        if (avatarFaceLoader != null)
        {
            avatarFaceLoader.gameObject.SetActive(isVisible);
        }
    }

    private AvatarVisualData CreateAvatarVisualData(AvatarData avatarData, CharacterID characterId)
    {
        AvatarPartsReference avatarPartsReference = AvatarPartsReference.Instance;
        if (avatarData == null || avatarPartsReference == null)
        {
            return null;
        }

        AvatarVisualData visualData = new();
        foreach (AvatarPiece piece in AvatarPieces)
        {
            int pieceId = avatarData.GetPieceID(piece);
            if (pieceId <= 0)
            {
                continue;
            }

            visualData.SetAvatarPiece(piece, avatarPartsReference.GetAvatarPartById(pieceId.ToString(CultureInfo.InvariantCulture)));
            visualData.SetColor(piece, ParseAvatarColor(avatarData.GetPieceColor(piece)));
        }

        visualData.SkinColor = ParseAvatarColor(avatarData.Color);
        visualData.ClassColor = ResolveClassColor(characterId);
        return visualData;
    }

    private static void DisableAvatarRaycasts(AvatarFaceLoader avatarFaceLoader)
    {
        if (avatarFaceLoader == null)
        {
            return;
        }

        foreach (Graphic graphic in avatarFaceLoader.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }
    }

    private static Color ParseAvatarColor(string colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue))
        {
            return Color.white;
        }

        string normalizedColor = colorValue.StartsWith("#", StringComparison.Ordinal)
            ? colorValue
            : "#" + colorValue;
        return ColorUtility.TryParseHtmlString(normalizedColor, out Color color)
            ? color
            : Color.white;
    }

    private static Color ResolveClassColor(CharacterID characterId)
    {
        if (characterId == CharacterID.None || ClassReference.Instance == null)
        {
            return Color.white;
        }

        return ClassReference.Instance.GetColor(BaseCharacter.GetClass(characterId));
    }

    private void ApplyEntryLayout(GameObject entry, TMP_Text messageText)
    {
        if (entry == null)
        {
            return;
        }

        float rowHeight = GetRuntimeEntryHeight();
        if (entry.TryGetComponent(out LayoutElement layoutElement))
        {
            layoutElement.ignoreLayout = false;
            layoutElement.minHeight = rowHeight;
            layoutElement.preferredHeight = rowHeight;
            layoutElement.flexibleHeight = 0f;
        }

        if (entry.transform is RectTransform entryRect)
        {
            entryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
        }

    }

    private float GetTemplateEntryHeight()
    {
        if (entryTemplate != null)
        {
            if (entryTemplate.TryGetComponent(out LayoutElement layoutElement))
            {
                if (layoutElement.preferredHeight > 0f)
                {
                    return layoutElement.preferredHeight;
                }

                if (layoutElement.minHeight > 0f)
                {
                    return layoutElement.minHeight;
                }
            }

            if (entryTemplate.transform is RectTransform entryRect)
            {
                if (entryRect.sizeDelta.y > 0f)
                {
                    return entryRect.sizeDelta.y;
                }

                if (entryRect.rect.height > 0f)
                {
                    return entryRect.rect.height;
                }
            }
        }

        return 31f;
    }

    private float GetTemplateFontSize()
    {
        TMP_Text templateText = null;
        if (entryTemplate != null)
        {
            templateText = FindMessageText(entryTemplate.transform);
        }

        return templateText != null && templateText.fontSize > 0f
            ? templateText.fontSize
            : 13f;
    }

    private float GetRuntimeEntryHeight()
    {
        float templateHeight = GetTemplateEntryHeight();
        if (scrollRect == null || scrollRect.viewport == null)
        {
            return templateHeight;
        }

        float viewportHeight = scrollRect.viewport.rect.height;
        if (viewportHeight <= 0f)
        {
            Canvas.ForceUpdateCanvases();
            viewportHeight = scrollRect.viewport.rect.height;
        }

        if (viewportHeight <= 0f)
        {
            return templateHeight;
        }

        int rowCount = Mathf.Max(1, visibleEntryCount);
        float spacing = 0f;
        float contentPadding = 0f;

        if (contentRoot != null && contentRoot.TryGetComponent(out VerticalLayoutGroup layoutGroup))
        {
            spacing = Mathf.Max(0f, layoutGroup.spacing);
            contentPadding = layoutGroup.padding.top + layoutGroup.padding.bottom;
        }

        float availableHeight = viewportHeight - contentPadding - (Mathf.Max(0, rowCount - 1) * spacing);
        return Mathf.Max(1f, availableHeight / rowCount);
    }

    private bool HasRequiredReferences()
    {
        if (contentRoot != null && entryTemplate != null)
        {
            return true;
        }

        if (!warnedMissingReferences)
        {
            Debug.LogWarning("Raid event log is missing prefab entry template references.", this);
            warnedMissingReferences = true;
        }

        return false;
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        scrollRoutine = null;
    }

    private static string GetFurnitureName(GameFurniture furniture)
    {
        if (furniture == null)
        {
            return "item";
        }

        bool useEnglish = SettingsCarrier.Instance != null
            && SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English;

        string displayName = useEnglish
            ? furniture.FurnitureInfo?.EnglishName
            : furniture.FurnitureInfo?.VisibleName;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = furniture.FurnitureInfo?.VisibleName;
        }

        return string.IsNullOrWhiteSpace(displayName) ? furniture.Name : displayName;
    }

    private static string GetTrapName(int trapType)
    {
        return trapType switch
        {
            0 => "End Raid",
            1 => "Freeze",
            2 => "Double Weight",
            _ => "Unknown"
        };
    }

    private static string FormatActorName(string playerName)
    {
        return string.IsNullOrWhiteSpace(playerName)
            ? UseEnglish() ? "Clan" : "Klaani"
            : playerName.Trim();
    }

    private static string FormatNumber(double value)
    {
        double roundedValue = Math.Round(value);
        return Math.Abs(value - roundedValue) < 0.05d
            ? roundedValue.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.#", CultureInfo.InvariantCulture);
    }

    private static bool UseEnglish()
    {
        return SettingsCarrier.Instance != null
            && SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English;
    }

    private static Color ResolveIconColor(string actorName)
    {
        int hash = 0;
        if (!string.IsNullOrWhiteSpace(actorName))
        {
            foreach (char character in actorName)
            {
                hash = hash * 31 + character;
            }
        }

        int colorIndex = Math.Abs(hash % IconColors.Length);
        return IconColors[colorIndex];
    }

}
