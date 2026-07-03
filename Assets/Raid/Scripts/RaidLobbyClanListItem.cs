using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidLobbyClanListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clanNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private RectTransform progressFill;
    [SerializeField] private Image[] playerIcons;

    private RectTransform _rectTransform;

    private RectTransform RectTransform => _rectTransform ??= (RectTransform)transform;

    public readonly struct PlayerIconData
    {
        public PlayerIconData(string playerName, CharacterID characterId, AvatarData avatarData)
        {
            PlayerName = playerName;
            CharacterId = characterId;
            AvatarData = avatarData;
        }

        public string PlayerName { get; }
        public CharacterID CharacterId { get; }
        public AvatarData AvatarData { get; }
    }

    public void SetTemplateStackPosition(int index, int rowCount, float spacing)
    {
        Vector2 anchorMin = RectTransform.anchorMin;
        Vector2 anchorMax = RectTransform.anchorMax;
        float clampedSpacing = Mathf.Max(0f, spacing);
        float templateTop = anchorMax.y;
        const float bottomPadding = 0.02f;
        float templateBottom = anchorMin.y;
        float templateHeight = templateTop - templateBottom;
        int clampedRowCount = Mathf.Max(1, rowCount);
        float availableHeight = templateTop - bottomPadding;
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

    public void Configure(string clanName, int playerCount, int maxPlayersPerClan, IReadOnlyList<PlayerIconData> players = null)
    {
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
            progressFill.anchorMax = new Vector2(fill, progressFill.anchorMax.y);
            progressFill.anchoredPosition = Vector2.zero;
            progressFill.sizeDelta = Vector2.zero;
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
                    PlayerIconData player = players != null && i < players.Count
                        ? players[i]
                        : default;
                    ConfigurePlayerIcon(playerIcons[i], player);
                }
            }
        }
    }

    private void ConfigurePlayerIcon(Image iconImage, PlayerIconData player)
    {
        if (iconImage == null)
        {
            return;
        }

        if (TryConfigureAvatarIcon(iconImage, player))
        {
            return;
        }

        SetAvatarHeadVisible(iconImage.transform, false);
        iconImage.enabled = true;
        iconImage.color = ResolveFallbackIconColor(player.PlayerName);
        iconImage.preserveAspect = true;
    }

    private bool TryConfigureAvatarIcon(Image iconImage, PlayerIconData player)
    {
        if (player.AvatarData == null)
        {
            return false;
        }

        AvatarVisualData visualData = AvatarDesignLoader.CreateAvatarVisualDataFallback(player.AvatarData, player.CharacterId);
        if (visualData == null)
        {
            return false;
        }

        AvatarFaceLoader avatarFaceLoader = iconImage.GetComponentInChildren<AvatarFaceLoader>(true);
        if (avatarFaceLoader == null)
        {
            return false;
        }

        avatarFaceLoader.SetUseOwnAvatarVisuals(false);
        DisableAvatarRaycasts(avatarFaceLoader);

        iconImage.enabled = false;
        iconImage.raycastTarget = false;

        avatarFaceLoader.gameObject.SetActive(true);
        avatarFaceLoader.UpdateVisuals(visualData);
        return true;
    }

    private static void SetAvatarHeadVisible(Transform iconTransform, bool isVisible)
    {
        AvatarFaceLoader avatarFaceLoader = iconTransform.GetComponentInChildren<AvatarFaceLoader>(true);
        if (avatarFaceLoader != null)
        {
            avatarFaceLoader.gameObject.SetActive(isVisible);
        }
    }

    private static void DisableAvatarRaycasts(AvatarFaceLoader avatarFaceLoader)
    {
        foreach (Graphic graphic in avatarFaceLoader.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
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
        SettingsCarrier.LanguageType language = SettingsCarrier.Instance != null
            ? SettingsCarrier.Instance.Language
            : SettingsCarrier.LanguageType.Finnish;

        return language == SettingsCarrier.LanguageType.English
            ? SettingsCarrier.LanguageType.English
            : SettingsCarrier.LanguageType.Finnish;
    }
}
