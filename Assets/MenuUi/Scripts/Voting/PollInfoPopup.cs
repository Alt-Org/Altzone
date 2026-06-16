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

public class PollInfoPopup : MonoBehaviour
{
    public static PollInfoPopup Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text authorName;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text artistNameText;
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image frontRarityImage;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image greenFill;
    [SerializeField] private TMP_Text timer;
    [SerializeField] private TMP_Text tradeTag;

    [Header("Expired Polls")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject resultObject;
    [SerializeField] private TMP_Text resultYes;
    [SerializeField] private TMP_Text resultNo;

    [Header("Votes")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private GameObject voteButtons;
    [SerializeField] private GameObject voteBar;
    [SerializeField] private TMP_Text yesVotes;
    [SerializeField] private TMP_Text noVotes;
    [SerializeField] private TMP_Text yesVotesButton;
    [SerializeField] private TMP_Text noVotesButton;
    [SerializeField] private AddPlayerHeads playerHeads;

    [Header("Rarity Color Reference")]
    [SerializeField] private RarityColourReference rarityColourReference;

    [Header("Buttons")]
    [SerializeField] private Button closeFurnitureInfoButton;
    [SerializeField] private Button closeClanInfoButton;

    [Header("Panels")]
    [SerializeField] private GameObject furniturePollInfoObject;
    [SerializeField] private GameObject infoBox;

    [Header("Clan Role Poll UI Elements")]
    [SerializeField] private GameObject clanRolePollInfoObject;
    [SerializeField] private TMP_Text clanPlayerNameText;
    [SerializeField] private TMP_Text clanCurrentRoleText;
    [SerializeField] private TMP_Text clanTargetRoleText;

    private PollData _currentPollData;

    private readonly Color _green = HexToColor("#2FA36B");
    private readonly Color _red = HexToColor("#C83A2D");

    public bool IsPollEnded { get; set; } = false;

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
        tradeTag.text = isBuying ? "OSTO" : "MYYNTI";

        FurnitureInfo info = furnitureData.Furniture.FurnitureInfo;
        nameText.text = $"{info?.SetName} {info?.VisibleName}";

        iconImage.sprite = info?.Image;
        descriptionText.text = $"{info?.ArtisticDescription}";
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

        bool isAccepted = yesCount > noCount;

        resultObject.GetComponent<Image>().color = isAccepted ? _green : _red;

        resultText.text = isAccepted
            ? "Hyv\u00E4ksytty".ToUpper()
            : "Hyl\u00E4tty".ToUpper();

        resultObject.SetActive(true);

        playerHeads.InstantiateHeads(_currentPollData.Id);
        ShowPollPanel();
    }

    private void SetValues(PollData pollData)
    {
        _currentPollData = pollData;

        authorName.text = $"Luonut: {pollData?.Organizer}";

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
                if (playerHeads.isActiveAndEnabled)
                    playerHeads.InstantiateHeads(_currentPollData.Id);
            }
        });

        ShowPollPanel();
    }

    private void ShowPollPanel()
    {
        gameObject.SetActive(true);
        furniturePollInfoObject.SetActive(true);
        if (clanRolePollInfoObject != null) clanRolePollInfoObject.SetActive(false);
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
        yesVotes.text = yesVotesButton.text = yesPercent;
        noVotes.text = noVotesButton.text = noPercent;
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
                SignalBus.OnChangePopupInfoSignal("Äänen antaminen epäonnistui");
                return;
            }
            voteButtons.SetActive(false);
            voteBar.SetActive(true);

            VotingActions.ReloadPollList?.Invoke();
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
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }
        return Color.white;
    }
}
