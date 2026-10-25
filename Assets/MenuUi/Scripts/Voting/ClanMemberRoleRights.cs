using System;
using System.Collections;
using Altzone.Scripts.Config;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClanMemberRoleRights : MonoBehaviour
{
    private ClanMember _currentMember;
    public ClanMember CurrentMember
    {
        get
        {
            return _currentMember;
        }
        set
        {
            _currentMember = value;

            if (_currentMember != null)
            {
                PopulateRolesAndRights(_currentMember.Role);
            }
        }
    }

    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject popup;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private TMP_Dropdown dropdown;

    private ClanData _clanData = null;

    public void OpenPopup() => popup.SetActive(true);
    public void ClosePopup() => popup.SetActive(false);

    void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        if (CurrentMember.Role.name != null)
        {
            DataStore store = Storefront.Get();
            PlayerData player = null;
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);

            store.GetClanData(player.ClanId, data => _clanData = data);

            foreach (var role in _clanData.ClanRoles) {
                Debug.Log($"Clan Role: {role.name}");
            }

            Debug.Log("Starting");
            PopulateRoles(_clanData);
            PopulateRolesAndRights(CurrentMember.Role);
        }
    }

    private void PopulateRoles(ClanData clan)
    {
        List<string> options = new List<string>();
        foreach (var role in clan.ClanRoles) {
            options.Add(role.name);
        }
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        int defaultIndex = clan.ClanRoles.FindIndex(r => r.name == CurrentMember.Role.name);
        if (defaultIndex != -1)
        {
            dropdown.value = defaultIndex;
            dropdown.RefreshShownValue();

            PopulateRolesAndRights(CurrentMember.Role);
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        ClanRoles selectedRole = _clanData.ClanRoles[index];

        PopulateRolesAndRights(selectedRole);
    }

    private void PopulateRolesAndRights(ClanRoles currentRole)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (MemberRights right in Enum.GetValues(typeof(MemberRights)))
        {
            bool isAllowed = currentRole.HasRights(right);

            Debug.Log($"{right} {isAllowed}");
            GameObject newTextObj = Instantiate(textPrefab, contentParent);

            TextMeshProUGUI roleRights = newTextObj.GetComponent<TextMeshProUGUI>();
            Button roleButton = newTextObj.GetComponentInChildren<Button>();

            if (roleRights != null)
            {
                string statusColor = isAllowed ? "green" : "red";
                roleRights.text = $"{right}: <color={statusColor}>{isAllowed}</color>";
                roleButton.GetComponent<Image>().color = isAllowed ? Color.green : Color.red;
                roleButton.onClick.AddListener(() =>
                {
                    OnButtonClicked(right, isAllowed, currentRole);
                });
            }
        }
    }

    private void OnButtonClicked(MemberRights rightName, bool allowed, ClanRoles currentRole)
    {
        // Toggles rights locally for now
        Debug.Log($"Clicked button for right: {rightName}, Status: {allowed}");

        currentRole.SetRights(rightName, !allowed);
        PopulateRolesAndRights(currentRole);
    }

    private void NewRole() {
        return;
    }
}
