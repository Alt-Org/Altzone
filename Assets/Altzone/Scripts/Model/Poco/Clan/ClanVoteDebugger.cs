using UnityEngine;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;

public class ClanVoteDebugger : MonoBehaviour
{
    [SerializeField] private RoleAssignmentPopup rolePopup;

    private ClanData clanData;

    public void StartPromotionPoll()
    {
        StartCoroutine(StartPromotionPollCoroutine());
    }

    public void PromoteToOfficer()
    {
        StartCoroutine(PromotePlayerToOfficerCoroutine());
    }

    public void OnOpenRolePopupButton()
    {
        StartCoroutine(LoadClanDataAndOpenPopup());
    }

    private IEnumerator LoadClanDataAndOpenPopup()
    {
        string displayId = GameConfig.Get().PlayerSettings.PlayerGuid;

        PlayerData playerData = null;

        // Load player data
        Storefront.Get().GetPlayerData(displayId, data => playerData = data);
        yield return new WaitUntil(() => playerData != null);

        if (playerData == null || string.IsNullOrEmpty(playerData.ClanId))
        {
            Debug.LogWarning("Player data missing or not in a clan.");
            yield break;
        }

        // Load clan data
        Storefront.Get().GetClanData(playerData.ClanId, data => clanData = data);
        yield return new WaitUntil(() => clanData != null);

        // Initialize and show popup
        rolePopup.Initialize(clanData);
        rolePopup.gameObject.SetActive(true);
    }

    private IEnumerator StartPromotionPollCoroutine()
    {
        string displayId = GameConfig.Get().PlayerSettings.PlayerGuid;

        PlayerData playerData = null;
        ClanData clanData = null;
        bool timeout = false;

        // Get player data
        Storefront.Get().GetPlayerData(displayId, data => playerData = data);
        yield return new WaitUntil(() => playerData != null || timeout);

        if (playerData == null || string.IsNullOrEmpty(playerData.ClanId))
        {
            Debug.LogWarning("Player data missing or not in a clan.");
            yield break;
        }

        // Get clan data
        Storefront.Get().GetClanData(playerData.ClanId, data => clanData = data);
        yield return new WaitUntil(() => clanData != null || timeout);

        if (clanData == null)
        {
            Debug.LogWarning("Clan data not found.");
            yield break;
        }

        ClanPollManager.CreateRolePromotionPoll(playerData.Id, ClanMemberRole.Officer);
    }

    private IEnumerator PromotePlayerToOfficerCoroutine()
    {
        string displayId = GameConfig.Get().PlayerSettings.PlayerGuid;

        PlayerData playerData = null;
        ClanData clanData = null;
        bool timeout = false;

        // Fetch player data
        Storefront.Get().GetPlayerData(displayId, data => playerData = data);
        yield return new WaitUntil(() => playerData != null || timeout);

        if (playerData == null)
        {
            Debug.LogWarning("Player data not found.");
            yield break;
        }

        if (string.IsNullOrEmpty(playerData.ClanId))
        {
            Debug.LogWarning("Player is not in a clan.");
            yield break;
        }

        // Fetch clan data
        Storefront.Get().GetClanData(playerData.ClanId, data => clanData = data);
        yield return new WaitUntil(() => clanData != null || timeout);

        if (clanData == null)
        {
            Debug.LogWarning("Clan data not found.");
            yield break;
        }

        // Try to find this player in clan member list
        ClanMember targetMember = clanData.Members.Find(m =>
            !string.IsNullOrEmpty(m.Id) &&
            m.Id.Trim().Equals(playerData.Id.Trim(), System.StringComparison.OrdinalIgnoreCase));

        if (targetMember == null)
        {
            Debug.LogWarning("Player not found in clan members.");
            yield break;
        }

        // Change role to Officer
        targetMember.Role = ClanMemberRole.Officer;

        Debug.Log($"Player '{targetMember.Name}' promoted to Officer.");
    }
}
