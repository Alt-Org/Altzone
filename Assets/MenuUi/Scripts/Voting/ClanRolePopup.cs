using UnityEngine;
using TMPro;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;

public class RoleAssignmentPopup : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown memberDropdown;
    [SerializeField] private TMP_Dropdown roleDropdown;

    private ClanData clan;
    private ClanMember selectedMember;
    private ClanMemberRole selectedRole;

    public void Initialize(ClanData clanData)
    {
        clan = clanData;
        PopulateMemberList();
        PopulateRoleList();
    }

    private void PopulateMemberList()
    {
        memberDropdown.ClearOptions();
        var memberNames = clan.Members.Select(m => m.Name).ToList();
        memberDropdown.AddOptions(memberNames);

        if (clan.Members.Count > 0)
            selectedMember = clan.Members[0];

        memberDropdown.onValueChanged.AddListener(OnMemberSelected);
    }

    private void PopulateRoleList()
    {
        roleDropdown.ClearOptions();
        var roles = System.Enum.GetNames(typeof(ClanMemberRole)).ToList();
        roleDropdown.AddOptions(roles);

        selectedRole = (ClanMemberRole)0;

        roleDropdown.onValueChanged.AddListener(OnRoleSelected);
    }

    private void OnMemberSelected(int index)
    {
        selectedMember = clan.Members[index];
    }

    private void OnRoleSelected(int index)
    {
        selectedRole = (ClanMemberRole)index;
    }

    public void OnConfirmButton()
    {
        if (selectedMember != null)
        {
            PollManager.CreateRolePromotionPoll(selectedMember.Id, selectedRole);
            Debug.Log($"Poll created to promote {selectedMember.Name} to {selectedRole}");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No clan member selected!");
        }
    }
}
