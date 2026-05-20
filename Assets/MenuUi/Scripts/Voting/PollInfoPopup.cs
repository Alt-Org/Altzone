using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Voting;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;

public class PollInfoPopup : MonoBehaviour
{
    public static PollInfoPopup Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text artistNameText;
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image frontRarityImage;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image greenFill;
    [SerializeField] private TMP_Text timer;

    [Header("Votes")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private GameObject voteButtons;
    [SerializeField] private GameObject voteBar;
    [SerializeField] private TMP_Text yesVotes;
    [SerializeField] private TMP_Text noVotes;
    [SerializeField] private TMP_Text yesVotesButton;
    [SerializeField] private TMP_Text noVotesButton;

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

    public void UpdateTimerDisplay(long secondsLeft)
    {
        if (timer != null)
        {
            if (secondsLeft < 60) timer.text = secondsLeft + "s";
            else if (secondsLeft < 3600) timer.text = (secondsLeft / 60) + "m";
            else timer.text = (secondsLeft / 3600) + "h";
        }
    }

    // Opens the popup and fills it with the data from the furniture in question
    public void OpenFurniturePopup(PollData pollData)
    {
        if (pollData == null)
        {
            Debug.LogWarning("PollInfoPopup Open called with null furniture!");
            return;
        }

        _currentPollData = pollData;

        SetValues();
    }

    // We assume all data is furniture data for now
    private void SetValues()
    {
        var furnitureData = _currentPollData as FurniturePollData;
        if (furnitureData == null || furnitureData.Furniture == null) return;

        nameText.text = furnitureData.Furniture.Name ?? "";
        iconImage.sprite = furnitureData.Furniture.FurnitureInfo?.Image;
        descriptionText.text = furnitureData.Furniture.FurnitureInfo?.ArtisticDescription ?? "";
        valueText.text = $"{furnitureData.Furniture.Value}";

        /*
        setNameText.text = furnitureData.Furniture.FurnitureInfo?.SetName ?? "";

        string artistName = furniture.FurnitureInfo?.ArtistName;
        artistNameText.text = string.IsNullOrEmpty(artistName) ? "" : $"Artist: {artistName}";

        weightText.text = $"Weight: {furniture.Weight}";
        rarityText.text = $"Rarity: {furniture.Rarity}";

        // Apply colour to the two background images of the card based on rarityColourReference
        if (rarityColourReference != null)
        {
            Color rarityColor = rarityColourReference.GetColor(furniture.Rarity);
            rarityImage.color = rarityColor;

            if (frontRarityImage != null)
            {
                frontRarityImage.color = rarityColor;
            }
        }
        */

        string currentPollId = _currentPollData.Id;
        // Enable and disable vote buttons and list based on whether the player has voted on the poll
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
        {
            if (this == null || data == null) return;

            if (data != null)
            {
                bool hasVoted = !_currentPollData.NotVoted.Contains(data.Id);

                voteButtons.SetActive(!hasVoted);
                voteBar.SetActive(hasVoted);

                if (!hasVoted)
                {
                    yesButton.onClick.RemoveAllListeners();
                    noButton.onClick.RemoveAllListeners();

                    yesButton.onClick.AddListener(() => OnVoteButtonClicked(true));
                    noButton.onClick.AddListener(() => OnVoteButtonClicked(false));
                }
            }
        });


        int yesCount = _currentPollData.YesVotes.Count;
        int noCount = _currentPollData.NoVotes.Count;
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

        gameObject.SetActive(true);
        furniturePollInfoObject.SetActive(true);
        if (clanRolePollInfoObject != null) clanRolePollInfoObject.SetActive(false);
    }

    public void OnVoteButtonClicked(bool answer)
    {
        _currentPollData.AddVote(answer, result =>
        {
            SetValues();
        });

        voteButtons.SetActive(false);
        voteBar.SetActive(true);
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
}
