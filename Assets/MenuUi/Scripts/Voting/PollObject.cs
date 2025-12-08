using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Voting;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PollObject : MonoBehaviour
{
    private string pollId;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI YesVotesText;
    [SerializeField] private TextMeshProUGUI NoVotesText;
    [SerializeField] private TextMeshProUGUI TimeLeftText;
    [SerializeField] private TextMeshProUGUI PollTypeText;
    [SerializeField] private TextMeshProUGUI PollDescriptionText;

    [Header("Images")]
    [SerializeField] private Image Clock;
    [SerializeField] private Image Image;
    [SerializeField] private Image GreenFill;
    [SerializeField] private Image Background;
    [SerializeField] private Image InfoBackground;

    [Header("Buttons")]
    [SerializeField] private Button ClockButton;
    [SerializeField] private GameObject VoteYes;
    [SerializeField] private GameObject VoteNo;

    [Header("Results")]
    [SerializeField] private GameObject YesVoters;
    [SerializeField] private GameObject NoVoters;

    [Header("PlayerHeads")]
    [SerializeField] private AddPlayerHeads playerHeads;

    [Header("Avatar for Clan Polls")]
    [SerializeField] private GameObject avatarHandleGameObject;
    [SerializeField] private AvatarFaceLoader avatarFaceLoader;

    private PollData pollData;
    private bool showEndTimeManually = false;
    private Coroutine updateCoroutine;
    private bool pollEnded = false;

    private void Start()
    {
        ClockButton.onClick.AddListener(OnClockButtonClicked);

        if (pollData != null)
        {
            updateCoroutine = StartCoroutine(UpdateValues());
        }
    }

    private void OnEnable()
    {
        VotingActions.ReloadPollList += SetValues;

        if (pollData != null && updateCoroutine == null && !pollEnded)
        {
            updateCoroutine = StartCoroutine(UpdateValues());
        }
    }

    private void OnDisable()
    {
        VotingActions.ReloadPollList -= SetValues;
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
    }

    private IEnumerator UpdateValues()
    {
        float totalDuration = pollData.EndTime - pollData.StartTime;

        while (true)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long secondsLeft = pollData.EndTime - currentTime;

            if (secondsLeft <= 0)
            {
                pollEnded = true;
                showEndTimeManually = true;
                ClockButton.interactable = false;
                VoteYes.gameObject.SetActive(false);
                VoteNo.gameObject.SetActive(false);
                YesVoters.gameObject.SetActive(true);
                NoVoters.gameObject.SetActive(true);

                // Convert poll end time to local time
                DateTimeOffset endDateTime = DateTimeOffset.FromUnixTimeSeconds(pollData.EndTime).ToLocalTime();

                // Format and show local time. Example: "20.6. 13:50"
                TimeLeftText.text = endDateTime.ToString("d.M. HH:mm");

                //PollManager.EndPoll(pollId);

                yield break;
            }

            Clock.fillAmount = 1 - (float)(secondsLeft) / totalDuration;
            UpdateClockDisplay(secondsLeft);

            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateClockDisplay(long secondsLeft = -1)
    {
        if (pollData == null) return;

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (secondsLeft == -1)
        {
            secondsLeft = pollData.EndTime - currentTime;
        }

        if (pollEnded || showEndTimeManually)
        {
            DateTimeOffset endDateTime = DateTimeOffset.FromUnixTimeSeconds(pollData.EndTime).ToLocalTime();
            TimeLeftText.text = endDateTime.ToString("d.M. HH:mm");
        }
        else
        {
            // Display the remaining time, requires the "left" to read properly in-game
            if (secondsLeft < 60) TimeLeftText.text = secondsLeft + "s\nleft";
            else if (secondsLeft < 3600) TimeLeftText.text = (secondsLeft / 60) + "m\nleft";
            else TimeLeftText.text = (secondsLeft / 3600) + "h\nleft";
        }
    }

    private bool PollPassed()
    {
        return pollData.YesVotes.Count > pollData.NoVotes.Count;
    }

    public void AddVote(bool answer)
    {
        pollData.AddVote(answer);

        // --- DEBUG CHECK ---
        DataStore store = Storefront.Get();
        PlayerData player = null;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

        if (player != null)
        {
            bool inYes = pollData.YesVotes.Any(v => v.PlayerId == player.Id);
            bool inNo = pollData.NoVotes.Any(v => v.PlayerId == player.Id);
            bool inNotVoted = pollData.NotVoted.Contains(player.Id);

            Debug.Log($"[Poll Debug] Player '{player.Name}' voted: " +
                      $"Yes={inYes}, No={inNo}, NotVoted={inNotVoted}");
        }
        else
        {
            Debug.LogWarning("[Poll Debug] Player data not found!");
        }
        // -------------------

        SetValues();
        VotingActions.ReloadPollList?.Invoke();

        gameObject.SetActive(false);
    }


    private void SetValues() // Handles the info and values showcased in the PollObject
    {
        // Reset UI elements
        Image.gameObject.SetActive(false);
        avatarHandleGameObject.SetActive(false);

        if (InfoBackground != null)
        {
            if (pollData is ClanRolePollData)
            {
                // Clan Role Poll Color
                InfoBackground.color = new Color(1f, 1f, 1f); // White
                Background.color = new Color(0f, 1f, 1f); // Cyan
            }
            else if (pollData is FurniturePollData)
            {
                // Furniture Poll Background Color
                InfoBackground.color = new Color(0.2f, 1f, 0.2f); // Green
                Background.color = new Color(1f, 0.75f, 0f); // Yellow
            }
        }

        // Handles the Poll Header
        if (pollData is FurniturePollData)
        {
            if (PollTypeText != null)
                PollTypeText.text = "Furniture Poll";
        }
        else if (pollData is ClanRolePollData)
        {
            if (PollTypeText != null)
                PollTypeText.text = "Clan Poll";
        }
        else
        {
            if (PollTypeText != null)
                PollTypeText.text = "General Poll";
        }

        // Handle UI for Furniture Polls
        if (pollData is FurniturePollData furniturePollData)
        {
            Image.gameObject.SetActive(true);

           
            Sprite ribbonSprite = null;
            if (furniturePollData.Furniture != null)
            {
                // Fetch the furniture info from StorageFurnitureReference
                var furnitureInfo = StorageFurnitureReference.Instance.GetFurnitureInfo(furniturePollData.Furniture.Name);
                if (furnitureInfo != null && furnitureInfo.RibbonImage != null)
                {
                    ribbonSprite = furnitureInfo.RibbonImage;
                }
            }

            // In the case of ribbonSprite is missing, show the normal furniture sprite
            Image.sprite = ribbonSprite ?? furniturePollData.Sprite;

            // Poll description for Furniture Polls
            if (PollDescriptionText != null && furniturePollData.Furniture != null)
            {
                string furnitureName = furniturePollData.Furniture.Name ?? "Unknown Item";
                string priceText = furniturePollData.Furniture.Value.ToString();
                if(furniturePollData.FurniturePollType is FurniturePollType.Buying)
                    PollDescriptionText.text = $"Buying {furnitureName} for {priceText}";
                else if(furniturePollData.FurniturePollType is FurniturePollType.Selling)
                    PollDescriptionText.text = $"Suggesting {furnitureName} to be sold for {priceText}";
            }
        }

        // Handle UI for Clan Polls
        else if (pollData is ClanRolePollData clanRolePoll)
        {
            avatarHandleGameObject.SetActive(true);

            // Default values
            string memberName = "Unknown";
            string roleName = "None";

            PlayerData player = null;
            Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

            if (player != null && !string.IsNullOrEmpty(player.ClanId))
            {
                ClanData clan = null;
                Storefront.Get().GetClanData(player.ClanId, data => clan = data);

                if (clan != null)
                {
                    ClanMember targetMember = clan.Members.Find(m => m.Id == clanRolePoll.TargetPlayerId);
                    if (targetMember != null)
                    {
                        memberName = targetMember.Name;

                        if (targetMember.Role != null)
                        {
                            roleName = targetMember.Role.name.ToString();
                        }
                        else
                        {
                            roleName = "None"; // Fallback
                        }

                        // Load the avatar and apply visuals
                        StartCoroutine(LoadAndApplyAvatar(targetMember));
                    }
                }
            }

            // Poll Description for Clan Role Polls
            if (PollDescriptionText != null)
            {
                string currentRoleText = string.IsNullOrEmpty(roleName) ? "None" : roleName;
                string targetRoleText = clanRolePoll.TargetRole.ToString();
                PollDescriptionText.text = $"Promoting {memberName} from {currentRoleText} to {targetRoleText}";
            }
        }

        // Enable and disable vote buttons and list based on whether the player has voted on the poll
        PlayerData currentPlayer = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => currentPlayer = data);

        if (currentPlayer != null)
        {
            bool hasNotVoted = pollData.NotVoted.Contains(currentPlayer.Id);

            // Vote buttons
            VoteYes.SetActive(hasNotVoted);
            VoteNo.SetActive(hasNotVoted);

            // Voter lists
            YesVoters.SetActive(!hasNotVoted);
            NoVoters.SetActive(!hasNotVoted);
        }

        if (YesVotesText != null) YesVotesText.text = pollData.YesVotes.Count.ToString();
        if (NoVotesText != null) NoVotesText.text = pollData.NoVotes.Count.ToString();

        if (GreenFill != null)
        {
            if (pollData.YesVotes.Count == 0 && pollData.NoVotes.Count == 0)
                GreenFill.fillAmount = 0.5f;
            else
                GreenFill.fillAmount = (float)pollData.YesVotes.Count / (pollData.NoVotes.Count + pollData.YesVotes.Count);
        }

        playerHeads.InstantiateHeads(pollId);
    }


    private IEnumerator LoadAndApplyAvatar(ClanMember targetMember)
    {
        Debug.LogWarning($"Starting Avatar Loading for {targetMember.Name}");

        // Use GetPlayerData to Player Data
        PlayerData targetPlayer = targetMember.GetPlayerData();

        if (targetPlayer == null)
        {
            Debug.LogWarning($"Getting player data failed for {targetMember.Name}");
            yield break;
        }

        AvatarVisualData visualData = AvatarDesignLoader.Instance.LoadAvatarDesign(targetPlayer);

        if (visualData != null && avatarFaceLoader != null)
        {
            Debug.LogWarning($"Loaded AvatarData successfully for {targetMember.Name}");
            avatarFaceLoader.UpdateVisuals(visualData);
        }
        else
        {
            Debug.LogWarning($"Failed to load AvatarData for {targetMember.Name}");
        }
    }

    public void SetPollId(string newPollId)
    {
        pollId = newPollId;
        pollData = PollManager.GetAnyPollData(pollId);
        if (pollData == null) return;

        SetValues();

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime < pollData.EndTime)
        {
            // Start coroutine only if GameObject is active in hierarchy
            if (gameObject.activeInHierarchy)
            {
                if (updateCoroutine != null) StopCoroutine(updateCoroutine);
                pollEnded = false;
                showEndTimeManually = false;
                ClockButton.interactable = true;
                updateCoroutine = StartCoroutine(UpdateValues());
            }
        }
        else
        {
            pollEnded = true;
            showEndTimeManually = true;
            ClockButton.interactable = false;
            Clock.fillAmount = 1f;
            UpdateClockDisplay();
        }
    }

    public void PassPollId()
    {
        VotingActions.PassPollId?.Invoke(pollId);
    }

    private void OnClockButtonClicked()
    {
        // Don't allow toggling if poll has ended
        if (pollEnded) return;

        showEndTimeManually = !showEndTimeManually;
        UpdateClockDisplay();
    }

    public void ShowPollInfoPopup()
    {
        if (pollData is FurniturePollData furniturePollData && furniturePollData.Furniture != null)
        {
            if (PollInfoPopup.Instance != null)
            {
                PollInfoPopup.Instance.OpenFurniturePopup(furniturePollData.Furniture);
            }
            else
            {
                Debug.LogWarning("PollInfoPopup instance is not found in the scene.");
            }
        }
        else if (pollData is ClanRolePollData clanRolePoll)
        {
            if (PollInfoPopup.Instance == null)
            {
                Debug.LogWarning("PollInfoPopup instance is not found in the scene.");
                return;
            }

            // Start coroutine to fetch clan data
            StartCoroutine(FetchClanMemberAndOpenPopup(clanRolePoll));
        }
        else
        {
            Debug.LogWarning("FurniturePollData or Furniture is null, cannot open PollInfoPopup.");
        }
    }

    private IEnumerator FetchClanMemberAndOpenPopup(ClanRolePollData clanRolePoll)
    {
        if (clanRolePoll == null || string.IsNullOrEmpty(clanRolePoll.TargetPlayerId))
            yield break;

        // Fetch current player to get the clan ID
        PlayerData player = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);
        yield return new WaitUntil(() => player != null);

        if (string.IsNullOrEmpty(player.ClanId))
        {
            Debug.LogWarning("Player is not in a clan.");
            yield break;
        }

        // Fetch the clan data 
        ClanData clan = null;
        Storefront.Get().GetClanData(player.ClanId, data => clan = data);
        yield return new WaitUntil(() => clan != null);

        // Find the target member
        ClanMember targetMember = clan.Members.Find(m => m.Id == clanRolePoll.TargetPlayerId);

        if (targetMember == null)
        {
            Debug.LogWarning("Target member not found in clan.");
            yield break;
        }

        ClanMemberRole currentRole = ClanMemberRole.None;
        string roleString = targetMember.Role.name?.Trim();

        if (!string.IsNullOrEmpty(roleString) &&
            Enum.TryParse(roleString, true, out ClanMemberRole parsedRole))
        {
            currentRole = parsedRole;
        }
        else if (int.TryParse(roleString, out int roleInt) &&
                 Enum.IsDefined(typeof(ClanMemberRole), roleInt))
        {
            currentRole = (ClanMemberRole)roleInt;
        }
        else
        {
            Debug.LogWarning($"Invalid role string for member {targetMember.Name}: {targetMember.Role}. Defaulting to None.");
            currentRole = ClanMemberRole.None; // fallback 
        }

        // Open the clan role popup with the fetched data
        PollInfoPopup.Instance.OpenClanRolePopup(targetMember.Name, currentRole, clanRolePoll.TargetRole);
    }
}
