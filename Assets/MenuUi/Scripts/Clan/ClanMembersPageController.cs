using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;

public class ClanMembersPageController : MonoBehaviour
{
    [SerializeField] private Transform _membersContent;
    [SerializeField] private ClanMemberPlaque _memberPlaquePrefab;

    private void OnEnable()
    {
        Rebuild();
    }

    private void Rebuild()
    {
       if(!isActiveAndEnabled) return;
       StartCoroutine(RebuildCoroutine());
    }

    private IEnumerator RebuildCoroutine()
    {
       for (int i = _membersContent.childCount - 1; i >= 0; i--)
       {
           Destroy(_membersContent.GetChild(i).gameObject);
       }

        List<ClanMember> members = null;
        yield return StartCoroutine(ServerManager.Instance.GetClanPlayers(m => members = m));

        if (members == null) yield break;

        ServerClan clan = ServerManager.Instance.Clan;

        HashSet<string> adminIds = (clan != null && clan.admin_ids != null)
            ? new HashSet<string>(clan.admin_ids)
            : null;

        for (int i = 0; i < members.Count; i++)
        {
            ClanMember member = members[i];

            var plaque = Instantiate(_memberPlaquePrefab, _membersContent);
            plaque.gameObject.SetActive(true);

            plaque.SetPosition(i + 1);
            plaque.SetName(member?.Name);

            string roleLabel = "Member";
            if(adminIds != null && member != null && adminIds.Contains(member.Id))
            {
                roleLabel = "Admin";
            }

            plaque.SetRole(roleLabel);

            plaque.SetActivityRosette(false);

            var plaqueGO = plaque.gameObject;
            var faceLoader = plaqueGO.GetComponentInChildren<MenuUi.Scripts.AvatarEditor.AvatarFaceLoader>(true);

            if (faceLoader != null)
            {
                faceLoader.SetUseOwnAvatarVisuals(false);

                var avatarData = member.AvatarData;
                if(avatarData != null && AvatarDesignLoader.Instance != null)
                {
                    var visualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(avatarData);
                    if (visualData != null)
                        faceLoader.UpdateVisuals(visualData);
                }
            }
        }
    }
}
