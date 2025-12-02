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
}
