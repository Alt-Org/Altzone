using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.UI;

public class Raid_EventLog : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxEntries = 80;
    [SerializeField, Min(1)] private int visibleEntryCount = 2;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private GameObject entryTemplate;
    [SerializeField] private Sprite systemMessageSprite;
    [SerializeField] private Color normalEntryColor = new(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color trapEntryColor = new(1f, 1f, 1f, 0.95f);

    private readonly Queue<GameObject> entries = new();
    private Coroutine scrollRoutine;
    private bool warnedMissingReferences;

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

    public void LogSystemMessage(string message)
    {
        AddEntry("System", message, normalEntryColor, CharacterID.None, null, true);
    }

    private void AddEntry(string actorName, string message, Color color, CharacterID characterId, AvatarData avatarData, bool useSystemIcon = false)
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        GameObject entry = GetReusableEntry();
        ConfigureEntry(entry, actorName, message, color, characterId, avatarData, useSystemIcon);
        entry.SetActive(true);
        entries.Enqueue(entry);

        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
        }

        scrollRoutine = StartCoroutine(ScrollToBottomNextFrame());
    }

    private GameObject GetReusableEntry()
    {
        while (entries.Count >= maxEntries)
        {
            GameObject reusableEntry = entries.Dequeue();
            if (reusableEntry == null)
            {
                continue;
            }

            reusableEntry.transform.SetParent(contentRoot, false);
            reusableEntry.transform.SetAsLastSibling();
            return reusableEntry;
        }

        GameObject entry = Instantiate(entryTemplate, contentRoot);
        entry.name = "EventLogEntry";
        return entry;
    }

    private void ConfigureEntry(GameObject entry, string actorName, string message, Color color, CharacterID characterId, AvatarData avatarData, bool useSystemIcon)
    {
        if (entry == null)
        {
            return;
        }

        if (!entry.TryGetComponent(out RaidEventLogEntryHandler entryHandler))
        {
            entryHandler = entry.AddComponent<RaidEventLogEntryHandler>();
        }

        entryHandler.ApplyLayout(GetRuntimeEntryHeight());
        entryHandler.Configure(actorName, message, color, characterId, avatarData, systemMessageSprite, useSystemIcon);
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

        string displayName = UseEnglish()
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

}
