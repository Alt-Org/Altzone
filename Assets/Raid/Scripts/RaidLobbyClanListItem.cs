using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Language;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidLobbyClanListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clanNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Image progressFill;
    [SerializeField] private Image[] playerIcons;
    [SerializeField] private AvatarFaceLoader[] playerAvatarFaceLoaders;

    private RectTransform _rectTransform;

    private RectTransform RectTransform => _rectTransform ??= (RectTransform)transform;

    private const float BottomPadding = 0.02f;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        ResolveReferences();
    }

    public void SetTemplateStackPosition(int index, int rowCount, float spacing)
    {
        Vector2 anchorMin = RectTransform.anchorMin;
        Vector2 anchorMax = RectTransform.anchorMax;

        float clampedSpacing = Mathf.Max(0f, spacing);
        float templateTop = anchorMax.y;
        float templateBottom = anchorMin.y;
        float templateHeight = templateTop - templateBottom;
        int clampedRowCount = Mathf.Max(1, rowCount);
        float availableHeight = templateTop - BottomPadding;

        float rowHeight = Mathf.Min(
            templateHeight,
            (availableHeight - clampedSpacing * (clampedRowCount - 1)) / clampedRowCount);
        rowHeight = Mathf.Max(0.01f, rowHeight);

        float rowTop = templateTop - Mathf.Max(0, index) * (rowHeight + clampedSpacing);

        RectTransform.anchorMin = new Vector2(anchorMin.x, rowTop - rowHeight);
        RectTransform.anchorMax = new Vector2(anchorMax.x, rowTop);
        RectTransform.anchoredPosition = Vector2.zero;
        RectTransform.sizeDelta = Vector2.zero;
    }

    public void Configure(string clanName, int playerCount, int maxPlayersPerClan, IReadOnlyList<RaidPlayerIconData> players = null)
    {
        ResolveReferences();

        int clampedCount = Mathf.Clamp(playerCount, 0, Mathf.Max(0, maxPlayersPerClan));
        if (clanNameText != null)
        {
            clanNameText.text = string.IsNullOrWhiteSpace(clanName)
                ? GetDefaultClanName()
                : clanName;
        }

        if (playerCountText != null)
        {
            playerCountText.text = GetPlayerCountText(clampedCount, maxPlayersPerClan);
        }

        if (progressFill != null)
        {
            float fill = maxPlayersPerClan <= 0 ? 0f : Mathf.Clamp01((float)clampedCount / maxPlayersPerClan);
            progressFill.fillAmount = fill;
        }

        if (playerIcons == null)
        {
            return;
        }

        for (int i = 0; i < playerIcons.Length; i++)
        {
            if (playerIcons[i] != null)
            {
                bool hasPlayer = i < clampedCount;
                playerIcons[i].gameObject.SetActive(hasPlayer);
                if (hasPlayer)
                {
                    RaidPlayerIconData player = players != null && i < players.Count
                        ? players[i]
                        : default;
                    ConfigurePlayerIcon(i, player);
                }
            }
        }
    }

    private void ConfigurePlayerIcon(int iconIndex, RaidPlayerIconData player)
    {
        Image iconImage = playerIcons[iconIndex];
        AvatarFaceLoader avatarFaceLoader = GetPlayerAvatarFaceLoader(iconIndex);

        if (iconImage == null)
        {
            return;
        }

        if (TryConfigureAvatarIcon(iconImage, avatarFaceLoader, player))
        {
            return;
        }

        SetAvatarHeadVisible(avatarFaceLoader, false);
        iconImage.enabled = true;
        iconImage.color = ResolveFallbackIconColor(player.PlayerName);
        iconImage.preserveAspect = true;
    }

    private bool TryConfigureAvatarIcon(Image iconImage, AvatarFaceLoader avatarFaceLoader, RaidPlayerIconData player)
    {
        if (player.AvatarData == null || avatarFaceLoader == null)
        {
            return false;
        }

        AvatarVisualData visualData = AvatarDesignLoader.CreateAvatarVisualDataFallback(player.AvatarData, player.CharacterId);
        if (visualData == null)
        {
            return false;
        }

        avatarFaceLoader.SetUseOwnAvatarVisuals(false);

        iconImage.enabled = false;
        iconImage.raycastTarget = false;

        avatarFaceLoader.gameObject.SetActive(true);
        avatarFaceLoader.UpdateVisuals(visualData);
        return true;
    }

    private static void SetAvatarHeadVisible(AvatarFaceLoader avatarFaceLoader, bool isVisible)
    {
        if (avatarFaceLoader != null)
        {
            avatarFaceLoader.gameObject.SetActive(isVisible);
        }
    }

    private static Color ResolveFallbackIconColor(string playerName)
    {
        int hash = string.IsNullOrWhiteSpace(playerName) ? 0 : playerName.GetHashCode();
        float hue = Mathf.Abs(hash % 360) / 360f;
        return Color.HSVToRGB(hue, 0.55f, 0.95f);
    }

    private static string GetDefaultClanName()
    {
        return GetCurrentLanguage() == SettingsCarrier.LanguageType.English ? "Clan" : "Klaani";
    }

    private static string GetPlayerCountText(int playerCount, int maxPlayersPerClan)
    {
        return GetCurrentLanguage() == SettingsCarrier.LanguageType.English
            ? $"Players {playerCount}/{maxPlayersPerClan}"
            : $"Pelaajat {playerCount}/{maxPlayersPerClan}";
    }

    private static SettingsCarrier.LanguageType GetCurrentLanguage()
    {
        SettingsCarrier.LanguageType language = SettingsCarrier.Instance ? SettingsCarrier.Instance.Language : SettingsCarrier.LanguageType.Finnish;

        return language == SettingsCarrier.LanguageType.English
            ? SettingsCarrier.LanguageType.English
            : SettingsCarrier.LanguageType.Finnish;
    }

    private AvatarFaceLoader GetPlayerAvatarFaceLoader(int index)
    {
        return playerAvatarFaceLoaders != null && index >= 0 && index < playerAvatarFaceLoaders.Length
            ? playerAvatarFaceLoaders[index]
            : null;
    }

    private void ResolveReferences()
    {
        if (playerIcons == null)
        {
            return;
        }

        if (playerAvatarFaceLoaders == null || playerAvatarFaceLoaders.Length != playerIcons.Length)
        {
            playerAvatarFaceLoaders = new AvatarFaceLoader[playerIcons.Length];
        }
    }
}
