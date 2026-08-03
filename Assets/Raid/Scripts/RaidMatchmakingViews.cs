using System;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Language;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidMatchmakingViews : MonoBehaviour
{
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI matchmakingTitleText;
    [SerializeField] private TextMeshProUGUI matchmakingStatusText;
    [SerializeField] private TextMeshProUGUI matchmakingDetailText;
    [SerializeField] private GameObject[] matchmakingDots;
    [SerializeField] private float matchmakingDotSpacing = 48f;
    [SerializeField] private TextMeshProUGUI lobbyCountdownText;
    [SerializeField] private Transform participantListRoot;
    [SerializeField] private RaidLobbyClanListItem clanListItemTemplate;
    [SerializeField] private Button exitRaidButton;
    [SerializeField] private Button debugStartButton;

    private Action _exitRaidAction;
    private Action _debugStartAction;

    public bool IsMatchmakingPanelVisible => matchmakingPanel != null && matchmakingPanel.activeInHierarchy;

    private void OnDestroy()
    {
        if (exitRaidButton != null)
        {
            exitRaidButton.onClick.RemoveListener(OnExitRaidPressed);
        }

        if (debugStartButton != null)
        {
            debugStartButton.onClick.RemoveListener(OnDebugStartPressed);
        }
    }

    public void Initialize(Action exitRaidAction, Action debugStartAction)
    {
        _exitRaidAction = exitRaidAction;
        _debugStartAction = debugStartAction;

        if (exitRaidButton != null)
        {
            exitRaidButton.onClick.RemoveListener(OnExitRaidPressed);
            exitRaidButton.onClick.AddListener(OnExitRaidPressed);
        }

        if (debugStartButton != null)
        {
            debugStartButton.onClick.RemoveListener(OnDebugStartPressed);
            debugStartButton.onClick.AddListener(OnDebugStartPressed);
        }

        ResolveMatchmakingDotsFallback();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowMatchmaking()
    {
        gameObject.SetActive(true);
        SetActive(matchmakingPanel, true);
        SetActive(lobbyPanel, false);

        SetMatchmakingDotsActive(0);
    }

    public void UpdateMatchmakingTexts(string title, string status, string detail)
    {
        SetText(matchmakingTitleText, title);
        SetText(matchmakingStatusText, status);
        SetText(matchmakingDetailText, detail);
        SetTextActive(matchmakingStatusText, !string.IsNullOrWhiteSpace(status));
        SetTextActive(matchmakingDetailText, !string.IsNullOrWhiteSpace(detail));
    }

    public void ShowMatchmakingSearchState(int currentPlayers, bool showFiveDots)
    {
        gameObject.SetActive(true);
        SetActive(matchmakingPanel, true);
        SetActive(lobbyPanel, false);

        bool playersFound = currentPlayers > 1;
        SetText(
            matchmakingTitleText,
            GetCurrentLanguage() == SettingsCarrier.LanguageType.English
                ? (playersFound ? "Players found" : "Searching for players")
                : (playersFound ? "Pelaajia l\u00F6ydetty" : "Etsit\u00E4\u00E4n pelaajia"));

        SetTextActive(matchmakingStatusText, playersFound);
        if (playersFound)
        {
            SetText(matchmakingStatusText, $"{currentPlayers} / {RaidPhotonRoom.RequiredPlayers}");
        }

        SetTextActive(matchmakingDetailText, false);
        UpdateMatchmakingDots(showFiveDots);
    }

    public void UpdateMatchmakingDots(bool showFiveDots)
    {
        SetMatchmakingDotsActive(showFiveDots ? 5 : 4);
    }

    public void ShowLobby()
    {
        gameObject.SetActive(true);
        SetActive(matchmakingPanel, false);
        SetActive(lobbyPanel, true);
    }

    public void SetLobbyCountdown(string timeText)
    {
        SetLocalizedText(lobbyCountdownText, "Kokoaminen alkaa\n{0}", "Gathering starts\n{0}", timeText);
    }

    public void RefreshParticipantList(IReadOnlyList<RaidLobbyClanRowData> clanRows, int maxPlayersPerClan)
    {
        if (participantListRoot == null)
        {
            return;
        }

        if (clanListItemTemplate == null)
        {
            Debug.LogError("Raid lobby clan list item template is not assigned in RaidMatchmakingViews.");
            return;
        }

        for (int i = participantListRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(participantListRoot.GetChild(i).gameObject);
        }

        int rowCount = clanRows?.Count ?? 0;
        for (int i = 0; i < rowCount; i++)
        {
            RaidLobbyClanListItem row = Instantiate(clanListItemTemplate, participantListRoot);
            row.name = $"Clan {i + 1}";
            row.gameObject.SetActive(true);
            row.SetTemplateStackPosition(i, rowCount, 0.04f);
            row.Configure(
                clanRows[i].ClanName,
                clanRows[i].PlayerCount,
                maxPlayersPerClan,
                clanRows[i].Players);
        }
    }

    private void OnExitRaidPressed()
    {
        _exitRaidAction?.Invoke();
    }

    private void OnDebugStartPressed()
    {
        _debugStartAction?.Invoke();
    }

    private static void SetActive(GameObject target, bool isActive)
    {
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }

    private static void SetText(TextMeshProUGUI textField, string text)
    {
        if (textField != null)
        {
            textField.text = text;
        }
    }

    private static void SetTextActive(TextMeshProUGUI textField, bool isActive)
    {
        if (textField != null)
        {
            textField.gameObject.SetActive(isActive);
        }
    }

    private void SetMatchmakingDotsActive(int visibleCount)
    {
        if (matchmakingDots == null)
        {
            return;
        }

        for (int i = 0; i < matchmakingDots.Length; i++)
        {
            GameObject dot = matchmakingDots[i];
            if (dot == null)
            {
                continue;
            }

            bool isVisible = i < visibleCount;
            dot.SetActive(isVisible);

            if (isVisible && dot.transform is RectTransform dotTransform)
            {
                float startX = -matchmakingDotSpacing * (visibleCount - 1) * 0.5f;
                dotTransform.anchoredPosition = new Vector2(startX + matchmakingDotSpacing * i, 0f);
            }
        }
    }

    private void ResolveMatchmakingDotsFallback()
    {
        if (matchmakingDots != null && matchmakingDots.Length > 0)
        {
            return;
        }

        Transform dotRoot = matchmakingPanel != null
            ? matchmakingPanel.transform.Find("MatchmakingDots")
            : null;
        if (dotRoot == null)
        {
            return;
        }

        matchmakingDots = new GameObject[dotRoot.childCount];
        for (int i = 0; i < dotRoot.childCount; i++)
        {
            matchmakingDots[i] = dotRoot.GetChild(i).gameObject;
        }
    }

    private static void SetLocalizedText(TextMeshProUGUI textField, string finnishText, string englishText, params string[] additions)
    {
        if (textField == null)
        {
            return;
        }

        TextLanguageSelectorCaller selector = textField.GetComponent<TextLanguageSelectorCaller>();
        if (selector != null)
        {
            selector.SetText(GetCurrentLanguage(), additions ?? Array.Empty<string>());
            return;
        }

        string format = GetCurrentLanguage() == SettingsCarrier.LanguageType.English ? englishText : finnishText;
        textField.text = string.Format(format, additions ?? Array.Empty<string>());
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

public readonly struct RaidLobbyClanRowData
{
    public RaidLobbyClanRowData(string clanName, int playerCount, IReadOnlyList<RaidPlayerIconData> players)
    {
        ClanName = clanName;
        PlayerCount = playerCount;
        Players = players;
    }

    public string ClanName { get; }
    public int PlayerCount { get; }
    public IReadOnlyList<RaidPlayerIconData> Players { get; }
}
