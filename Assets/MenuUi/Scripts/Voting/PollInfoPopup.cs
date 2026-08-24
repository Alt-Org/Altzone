using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Voting;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUI.Scripts;
using System;
using System.Collections;
using Altzone.Scripts.Model.Poco.Player;

public class PollInfoPopup : MonoBehaviour
{
    public static PollInfoPopup Instance { get; private set; }

    [Header("Core UI")]
    [SerializeField] private GameObject infoBox;
    [SerializeField] private GameObject furniturePollInfoObject;
    [SerializeField] private TMP_Text timer;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Clan Role Poll UI Elements")]
    [SerializeField] private GameObject clanRolePollInfoObject;
    [SerializeField] private TMP_Text clanPlayerNameText;
    [SerializeField] private TMP_Text clanCurrentRoleText;
    [SerializeField] private TMP_Text clanTargetRoleText;

    [Header("Common Furniture")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text tradeTag;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text authorName;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text artistNameText;
    [SerializeField] private Image iconImage;

    [Header("Set information")]
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private Image setFontName;
    [SerializeField] private Image setPosterBackground;

    [Header("Rarity display")]
    [SerializeField] private RarityColourReference rarityColourReference;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image frontRarityImage;

    [Header("Voting system")]
    [SerializeField] private GameObject voteButtons;
    [SerializeField] private Button noButton;
    [SerializeField] private Button yesButton;

    [Header("Poll Results")]
    [SerializeField] private AddPlayerHeads playerHeads;
    [SerializeField] private GameObject voteBar;
    [SerializeField] private GameObject resultObject;
    [SerializeField] private TMP_Text resultBarNoVotes;
    [SerializeField] private TMP_Text resultBarYesVotes;
    [SerializeField] private TMP_Text resultNo;
    [SerializeField] private TMP_Text resultYes;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image greenFill;

    [Header("Navigation")]
    [SerializeField] private Button closeClanInfoButton;
    [SerializeField] private Button closeFurnitureInfoButton;


    private PollData _currentPollData;
    private Coroutine _timerCoroutine;

    private TMP_Text _yesVotes;
    private TMP_Text _noVotes;

    private static SettingsCarrier.LanguageType Language => SettingsCarrier.Instance.Language;

    private WaitForSeconds _oneSecondWait = new WaitForSeconds(1f);
    private readonly Color _green = HexToColor("#2FA36B");
    private readonly Color _red = HexToColor("#C83A2D");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple PollInfoPopup instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        _timerCoroutine = StartCoroutine(TimerUpdateLoop());
    }

    private void OnDisable()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (closeFurnitureInfoButton != null)
        {
            closeFurnitureInfoButton.onClick.AddListener(Close);
        }

        if (closeClanInfoButton != null)
        {
            closeClanInfoButton.onClick.AddListener(Close);
        }
    }

    public void InitializeIfNeeded()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[PollInfoPopup] Initialized manually.");
        }
    }

    private IEnumerator TimerUpdateLoop()
    {
        while (!_currentPollData.IsExpired)
        {
            UpdateTimerDisplay();
            yield return _oneSecondWait;
        }
        UpdateTimerDisplay();
    }

    public void UpdateTimerDisplay(long secondsLeft = -1)
    {
        if (timer == null)
            return;

        if (_currentPollData.IsExpired)
        {
            DateTimeOffset endDateTime = DateTimeOffset.FromUnixTimeSeconds(_currentPollData.EndTime).ToLocalTime();
            timer.text = endDateTime.ToString("d.M. HH:mm");
            return;
        }

        if (secondsLeft == -1)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            secondsLeft = _currentPollData.EndTime - currentTime;
        }

        long seconds = secondsLeft % 60;
        long minutes = (secondsLeft / 60) % 60;
        long hours = secondsLeft / 3600;

        timer.text = secondsLeft switch
        {
            < 60 => $"{seconds}s",
            < 3600 => $"{minutes}m {seconds}s",
            _ => $"{hours}h {minutes}m"
        };
    }

    // Opens the popup and fills it with the data from the furniture in question
    public void OpenPopup(PollData pollData)
    {
        _yesVotes = yesButton.GetComponentInChildren<TMP_Text>();
        _noVotes = noButton.GetComponentInChildren<TMP_Text>();

        Debug.Log($"PollData in OpenPopup {pollData}");
        if (pollData == null)
        {
            return;
        }

        SetValues(pollData);
    }

    private void SetFurnitureData(FurniturePollData furnitureData)
    {
        if (furnitureData == null || furnitureData.Furniture == null) return;

        bool isBuying = furnitureData.FurniturePollType == FurniturePollType.Buying;

        tradeTag.text = Language == SettingsCarrier.LanguageType.English
            ? isBuying
                ? "Buy".ToUpper()
                : "Sell".ToUpper()
            : isBuying
                ? "Osto".ToUpper()
                : "Myynti".ToUpper();

        FurnitureInfo info = furnitureData.Furniture.FurnitureInfo;

        if (info == null)
            return;

        nameText.text = Language == SettingsCarrier.LanguageType.English
            ? $"{info.SetNameEnglish} {info.EnglishName}"
            : $"{info.SetName} {info.VisibleName}";

        iconImage.sprite = info.Image;

        if (info.SetPosterBackground != null && setPosterBackground != null)
        {
            setPosterBackground.gameObject.SetActive(true);
            setPosterBackground.sprite = info.SetPosterBackground;
        }
        if (info.SetFontName != null && setFontName != null)
        {
            setFontName.gameObject.SetActive(true);
            setFontName.sprite = info.SetFontName;
        }


        descriptionText.text = Language == SettingsCarrier.LanguageType.English
            ? $"{info.EnglishArtisticDescription}"
            : $"{info.ArtisticDescription}";

        valueText.text = $"{furnitureData.Furniture.Value}";
    }

    private void SetClanRoleData(ClanRolePollData _)
    {
        Debug.LogWarning("ClanRolePoll not implemented yet");
    }

    private void SetExpiredPollInfo()
    {
        voteButtons.SetActive(false);
        voteBar.SetActive(false);

        int yesCount = _currentPollData.YesVotes?.Count ?? 0;
        int noCount = _currentPollData.NoVotes?.Count ?? 0;
        int totalCount = yesCount + noCount;

        string yesPercent, noPercent;

        if (totalCount <= 0)
        {
            return;
        }

        float yesVoteRatio = (float)yesCount / totalCount;
        yesPercent = yesVoteRatio.ToString("P0");
        noPercent = (1.0f - yesVoteRatio).ToString("P0");

        resultYes.text = yesPercent;
        resultNo.text = noPercent;

        DataStore store = Storefront.Get();
        PlayerData player = null;
        ClanData clan = null;

        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player != null && player.ClanId != null)
        {
            store.GetClanData(player.ClanId, data => clan = data);
        }

        bool isAccepted = yesVoteRatio > _currentPollData.minPercentage;

        resultObject.GetComponent<Image>().color = isAccepted ? _green : _red;

        resultText.text = Language == SettingsCarrier.LanguageType.English
            ? isAccepted
                ? "Accepted".ToUpper()
                : "Denied".ToUpper()
            : isAccepted
                ? "Hyv\u00E4ksytty".ToUpper()
                : "Hyl\u00E4tty".ToUpper();

        resultObject.SetActive(true);

        ShowPollPanel();
    }

    private void SetValues(PollData pollData)
    {
        _currentPollData = pollData;

        authorName.text = Language == SettingsCarrier.LanguageType.English
            ? $"Created: {pollData?.Organizer}"
            : $"Luonut: {pollData?.Organizer}";

        setPosterBackground.gameObject.SetActive(false);
        setFontName.gameObject.SetActive(false);

        if (pollData is FurniturePollData furniturePollData)
        {
            SetFurnitureData(furniturePollData);
        }
        else if (pollData is ClanRolePollData clanRolePollData)
        {
            SetClanRoleData(clanRolePollData);
        }
        else
        {
            Debug.LogError("Called PollInfo with unknown data");
        }

        UpdateTimerDisplay();

        int yesCount = _currentPollData.YesVotes.Count;
        int noCount = _currentPollData.NoVotes.Count;
        SetGreenFill(yesCount, noCount);

        if (_currentPollData.IsExpired)
        {
            SetExpiredPollInfo();
            return;
        }

        // Enable and disable vote buttons and list based on whether the player has voted on the poll
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
        {
            if (this == null || data == null) return;

            bool hasVoted = !_currentPollData.NotVoted.Contains(data.Id);

            resultObject.SetActive(false);
            voteButtons.SetActive(!hasVoted);
            voteBar.SetActive(hasVoted);

            if (!hasVoted)
            {
                yesButton.onClick.RemoveAllListeners();
                noButton.onClick.RemoveAllListeners();

                yesButton.onClick.AddListener(() => OnVoteButtonClicked(true));
                noButton.onClick.AddListener(() => OnVoteButtonClicked(false));
            }
            else
            {
                playerHeads.InstantiateHeads(_currentPollData.Id);
            }
        });

        ShowPollPanel();
    }

    private void ShowPollPanel()
    {
        gameObject.SetActive(true);

        furniturePollInfoObject?.SetActive(true);

        clanRolePollInfoObject?.SetActive(false);
    }

    private void SetGreenFill(int yesCount, int noCount)
    {
        int totalCount = yesCount + noCount;

        float fillValue;
        string yesPercent, noPercent;

        if (totalCount > 0)
        {
            fillValue = (float)yesCount / totalCount;
            yesPercent = fillValue.ToString("P0");
            noPercent = (1.0f - fillValue).ToString("P0");
        }
        else
        {
            fillValue = 0.5f;
            yesPercent = "0%";
            noPercent = "0%";
        }

        greenFill.fillAmount = fillValue;

        resultBarYesVotes.text = _yesVotes.text = yesPercent;
        resultBarNoVotes.text = _noVotes.text = noPercent;
    }

    public void OnVoteButtonClicked(bool answer)
    {
        int yesCount = _currentPollData.YesVotes.Count;
        int noCount = _currentPollData.NoVotes.Count;
        if (answer) yesCount += 1;
        else noCount += 1;
        SetGreenFill(yesCount, noCount);

        _currentPollData.AddVote(answer, result =>
        {
            if (!result)
            {
                string error = Language == SettingsCarrier.LanguageType.English
                    ? "Voting failed"
                    : "Äänen antaminen epäonnistui";

                SignalBus.OnChangePopupInfoSignal(error);
                return;
            }
            voteButtons.SetActive(false);
            voteBar.SetActive(true);

            //VotingActions.ReloadPollList?.Invoke();
            playerHeads.InstantiateHeads(_currentPollData.Id);
        });
    }

    // Opens the popup for clan role polls
    public void OpenClanRolePopup(string playerName, ClanMemberRole currentRole, ClanMemberRole targetRole)
    {
        clanPlayerNameText.text = playerName;
        clanCurrentRoleText.text = currentRole.ToString();
        clanTargetRoleText.text = targetRole.ToString();

        clanRolePollInfoObject.SetActive(true);
        infoBox.SetActive(false);
        gameObject.SetActive(true);
    }


    public void Close()
    {
        if (furniturePollInfoObject != null)
            furniturePollInfoObject.SetActive(false);

        if (clanRolePollInfoObject != null)
            clanRolePollInfoObject.SetActive(false);

        // infoBox.SetActive(false);
        gameObject.SetActive(false);
    }

    // Toggles the info page in the furniture info popup
    public void ToggleInfo(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(!target.activeSelf);
        }
    }

    private static Color HexToColor(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
    }
}
