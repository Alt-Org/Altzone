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
    [SerializeField] private Image SetRibbonBackground;
    [SerializeField] private Image GreenFill;
    [SerializeField] private Image Background;
    [SerializeField] private Image InfoBackground;

    [Header("TradeTag")]
    [SerializeField] private Image TradeBackground;
    [SerializeField] private TextMeshProUGUI TradeText;
    [SerializeField] private TextMeshProUGUI Price;

    [Header("Poll Ended")]
    [SerializeField] private GameObject ResultObject;
    [SerializeField] private TextMeshProUGUI ResultText;

    [Header("Results")]
    [SerializeField] private GameObject YesVoters;
    [SerializeField] private GameObject NoVoters;
    [SerializeField] private GameObject ShowVoteButton;
    [SerializeField] private GameObject VoteBar;

    [Header("PlayerHeads")]
    [SerializeField] private AddPlayerHeads playerHeads;

    [Header("Avatar for Clan Polls")]
    [SerializeField] private GameObject avatarHandleGameObject;
    [SerializeField] private AvatarFaceLoader avatarFaceLoader;

    private PollData pollData;
    private bool showEndTimeManually = false;
    private Coroutine updateCoroutine;
    private bool pollEnded = false;

    private readonly Color _green = HexToColor("#2FA36B");
    private readonly Color _red = HexToColor("#C83A2D");

    private void Start()
    {
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
                YesVoters.gameObject.SetActive(true);
                NoVoters.gameObject.SetActive(true);

                // Convert poll end time to local time
                DateTimeOffset endDateTime = DateTimeOffset.FromUnixTimeSeconds(pollData.EndTime).ToLocalTime();

                // Format and show local time. Example: "20.6. 13:50"
                TimeLeftText.text = endDateTime.ToString("d.M. HH:mm");

                //PollManager.EndPoll(pollId);

                yield break;
            }

            Clock.fillAmount = 1f - (secondsLeft / totalDuration);
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
            if (secondsLeft < 60) TimeLeftText.text = secondsLeft + "s";
            else if (secondsLeft < 3600) TimeLeftText.text = (secondsLeft / 60) + "m";
            else TimeLeftText.text = (secondsLeft / 3600) + "h";
        }
    }

    private bool PollPassed()
    {
        return pollData.YesVotes.Count > pollData.NoVotes.Count;
    }

    public void AddVote(bool answer)
    {
        pollData.AddVote(answer, result =>
        {
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
            if (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
                                && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationSocialType == Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ClanVote)
            {
                DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ClanVote, "1");
            }
        });

        gameObject.SetActive(false);
    }


    private void SetValues() // Handles the info and values showcased in the PollObject
    {
        // Reset UI elements
        Image.gameObject.SetActive(false);
        SetRibbonBackground.gameObject.SetActive(false);
        // avatarHandleGameObject.SetActive(false);

        if (InfoBackground != null)
        {
            // *** kommentoidaan värityksen testauksen ajaksi pois ***

            //if (pollData is ClanRolePollData)
            //{
            //    // Clan Role Poll Color
            //    InfoBackground.color = new Color(1f, 1f, 1f); // White
            //    Background.color = new Color(0f, 1f, 1f); // Cyan
            //}
            //else if (pollData is FurniturePollData)
            //{
            //    // Furniture Poll Background Color
            //    InfoBackground.color = new Color(0.2f, 1f, 0.2f); // Green
            //    Background.color = new Color(1f, 0.75f, 0f); // Yellow
            //}
        }

        if (pollData is ClanRolePollData)
        {
            if (PollTypeText != null)
                PollTypeText.text = "Clan Poll";
        }
        else
        {
            if (PollTypeText != null)
                PollTypeText.text = "General Poll";
        }

        PlayerData player = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
        {
            player = data;

            if (player != null)
            {
                bool hasVoted = !pollData.NotVoted.Contains(player.Id);
                ShowVoteButton.gameObject.SetActive(!hasVoted);
                VoteBar.gameObject.SetActive(hasVoted);
            }
            else
            {
                Debug.LogError("Failed to fetch player data!");
            }
        });

        if (pollData is FurniturePollData furniturePollData)
        {
            SetFurnitureData(furniturePollData);
        }
        else if (pollData is ClanRolePollData clanRolePoll)
        {
            SetClanRoleData(clanRolePoll);
        }

        int yesCount = pollData.YesVotes.Count;
        int noCount = pollData.NoVotes.Count;
        int totalCount = yesCount + noCount;

        float fillValue = (totalCount > 0) ? (float)yesCount / totalCount : 0.5f;

        if (pollData.IsExpired)
        {
            DataStore store = Storefront.Get();
            ClanData clan = null;

            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

            if (player != null && player.ClanId != null)
            {
                store.GetClanData(player.ClanId, data => clan = data);
            }

            int requiredYesVotes = Mathf.CeilToInt(clan.Members.Count * 0.33f);
            bool isAccepted = yesCount >= requiredYesVotes && yesCount > noCount;

            ResultObject.GetComponent<Image>().color = isAccepted ? _green : _red;

            ResultText.text = isAccepted
                ? "Hyv\u00E4ksytty".ToUpper()
                : "Hyl\u00E4tty".ToUpper();

            ResultObject.SetActive(true);
            VoteBar.gameObject.SetActive(false);
            ShowVoteButton.gameObject.SetActive(false);

            return;
        }
        else
        {
            ResultObject.SetActive(false);
        }

        if (YesVotesText != null) YesVotesText.text = fillValue.ToString("P0");
        if (NoVotesText != null) NoVotesText.text = (1f - fillValue).ToString("P0");

        GreenFill.fillAmount = fillValue;
    }

    private void SetClanRoleData(ClanRolePollData clanRolePoll)
    {
        // avatarHandleGameObject.SetActive(true);

        string memberName = "Unknown";
        string roleName = "None";
        ClanData clan = null;
        PlayerData player = null;

        if (player == null || string.IsNullOrEmpty(player.ClanId))
            return;

        Storefront.Get().GetClanData(player.ClanId, data =>
        {
            if (clan == null)
                return;

            ClanMember targetMember = clan.Members.Find(m => m.Id == clanRolePoll.TargetPlayerId);
            if (targetMember == null)
                return;

            memberName = targetMember.Name;

            if (targetMember.Role != null)
            {
                roleName = targetMember.Role.name.ToString();
            }
            else
            {
                roleName = "None";
            }

            StartCoroutine(LoadAndApplyAvatar(targetMember));
        });

        if (PollDescriptionText != null)
        {
            string currentRoleText = string.IsNullOrEmpty(roleName) ? "None" : roleName;
            string targetRoleText = clanRolePoll.TargetRole.ToString();
            PollDescriptionText.text = $"Promoting {memberName} from {currentRoleText} to {targetRoleText}";
        }
    }

    private void SetFurnitureData(FurniturePollData furniturePollData)
    {
        if (furniturePollData == null || furniturePollData.Furniture == null)
        {
            Debug.LogError("SetFurnitureData received null poll or furniture data!");
            return;
        }

        Image.gameObject.SetActive(true);
        SetRibbonBackground.gameObject.SetActive(true);
        FurnitureInfo info = furniturePollData.Furniture.FurnitureInfo;
        PollTypeText.text = $"{info.SetName} {info.VisibleName}";

        bool isBuying = furniturePollData.FurniturePollType == FurniturePollType.Buying;
        TradeBackground.color = isBuying ? _green : _red;
        TradeText.text = isBuying ? "OSTO" : "MYYNTI";

        Price.text = furniturePollData.Furniture.Value.ToString();


        // Fetch the furniture info from StorageFurnitureReference
        FurnitureInfo furnitureInfo = StorageFurnitureReference.Instance.GetFurnitureInfo(furniturePollData.Furniture.Name);

        Image.sprite = (furnitureInfo != null) ? furnitureInfo.Image : furniturePollData.Sprite;

        if (furnitureInfo != null)
        {
            SetRibbonBackground.sprite = furnitureInfo.SetRibbonBackground;
        }

        /*
        // Poll description for Furniture Polls
        if (PollDescriptionText != null && furniturePollData.Furniture != null)
        {
            string priceText = furniturePollData.Furniture.Value.ToString();
            if (furniturePollData.FurniturePollType is FurniturePollType.Buying)
            {
                PollDescriptionText.text = $"Buying {furnitureName} for {priceText}";
            }
            else if (furniturePollData.FurniturePollType is FurniturePollType.Selling)
            {
                PollDescriptionText.text = $"Suggesting {furnitureName} to be sold for {priceText}";
            }
        }
        */
    }



    //// Erjan lisäys, testi ***

    public void SetTheme(Color themeColor)
    {
        if (Background == null)
        {
            Debug.LogError("Background is NULL in PollObject!");
            return;
        }

        Debug.Log("Applying theme color: " + themeColor);
        Background.color = themeColor;
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

        // if (visualData != null && avatarFaceLoader != null)
        // {
        //     Debug.LogWarning($"Loaded AvatarData successfully for {targetMember.Name}");
        //     // avatarFaceLoader.UpdateVisuals(visualData);
        // }
        // else
        // {
        //     Debug.LogWarning($"Failed to load AvatarData for {targetMember.Name}");
        // }
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
                updateCoroutine = StartCoroutine(UpdateValues());
            }
        }
        else
        {
            pollEnded = true;
            showEndTimeManually = true;
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
        if (PollInfoPopup.Instance == null)
        {
            Debug.LogWarning("PollInfoPopup instance is not found in the scene.");
            return;
        }
        if (pollData == null)
        {
            return;
        }

        if (pollData is FurniturePollData)
        {
            PollInfoPopup.Instance.OpenPopup(pollData);
        }
        if (pollData is ClanRolePollData clanRolePoll)
        {
            // Start coroutine to fetch clan data
            StartCoroutine(FetchClanMemberAndOpenPopup(clanRolePoll));
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

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }
        return Color.white;
    }
}
