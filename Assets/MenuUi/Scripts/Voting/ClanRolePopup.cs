using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;

public class RoleAssignmentPopup : MonoBehaviour
{
    [Header("List Containers")] // Containers for lists
    [SerializeField] private Transform memberListContainer; 
    [SerializeField] private Transform roleListContainer;   

    [Header("Item Prefab")] // Item prefab that gets instantiated based on the amount of members and roles
    [SerializeField] private GameObject listItemPrefab;     

    [Header("Info Texts")] // Info texts regarding the selected member and the selected members current role in the popup
    [SerializeField] private TMP_Text selectedMemberText;   
    [SerializeField] private TMP_Text selectedRoleText;    

    private List<Button> memberButtons = new List<Button>();
    private List<Button> roleButtons = new List<Button>();

    private ClanData clan;
    private ClanMember selectedMember;
    private ClanMemberRole? selectedRole;

    public void Initialize(ClanData clanData)
    {
        clan = clanData;

        PopulateMemberList();
        PopulateRoleList();

        // clear info texts initially
        selectedMemberText.text = "No member selected";
        selectedRoleText.text = "No role selected";
    }

    private void PopulateMemberList()
    {
        // Clear previous buttons
        foreach (Transform child in memberListContainer)
        Destroy(child.gameObject);
        memberButtons.Clear();

        // Populates the list regarding the clan members (based on the members in the clan)
        for (int i = 0; i < clan.Members.Count; i++)
        {
            var member = clan.Members[i];
            var buttonObj = Instantiate(listItemPrefab, memberListContainer);
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<TMP_Text>();
            text.text = member.Name;

            int index = i;
            button.onClick.AddListener(() => OnMemberSelected(index));
            memberButtons.Add(button);
        }
    }

    private void PopulateRoleList()
    {
        // Clear previous buttons
        foreach (Transform child in roleListContainer)
        Destroy(child.gameObject);
        roleButtons.Clear();

        var roles = System.Enum.GetValues(typeof(ClanMemberRole)).Cast<ClanMemberRole>().ToList();

        // Populates the list regarding the roles (Currently none, member, officer, admin)
        for (int i = 0; i < roles.Count; i++)
        {
            var role = roles[i];
            var buttonObj = Instantiate(listItemPrefab, roleListContainer);
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<TMP_Text>();
            text.text = role.ToString();

            int index = i; 
            button.onClick.AddListener(() => OnRoleSelected(index));
            roleButtons.Add(button);
        }
    }

    private void OnMemberSelected(int index)
    {
        selectedMember = clan.Members[index];

        // Update text info
        selectedMemberText.text = $"{selectedMember.Name} Current role: {selectedMember.Role}";

        // Darken selected button
        for (int i = 0; i < memberButtons.Count; i++)
        {
            var colors = memberButtons[i].colors;
            colors.normalColor = (i == index) ? Color.gray : Color.white;
            memberButtons[i].colors = colors;
        }
    }

    private void OnRoleSelected(int index)
    {
        selectedRole = (ClanMemberRole)index;

        // Update text info
        selectedRoleText.text = $"Selected role: {selectedRole}";

        // Darken selected button
        for (int i = 0; i < roleButtons.Count; i++)
        {
            var colors = roleButtons[i].colors;
            colors.normalColor = (i == index) ? Color.gray : Color.white;
            roleButtons[i].colors = colors;
        }
    }

    public void OnConfirmButton()
    {
        if (selectedMember == null)
        {
            Debug.LogWarning("No clan member selected!");
            return;
        }

        if (!selectedRole.HasValue)
        {
            Debug.LogWarning("No role selected!");
            return;
        }

        PollManager.CreateRolePromotionPoll(selectedMember.Id, selectedRole.Value); // Calls the CreateRolePromotionPoll in PollManager.cs
        Debug.Log($"Poll created to promote {selectedMember.Name} to {selectedRole.Value}");
        gameObject.SetActive(false);
    }
}
