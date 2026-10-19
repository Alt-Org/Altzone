using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
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

    private ClanData _clanData = null;

    public void OpenPopup() => popup.SetActive(true);
    public void ClosePopup() => popup.SetActive(false);

    void Start()
    {
        if (CurrentMember.Role.name != null)
        {
            Debug.Log("Starting");
            PopulateRolesAndRights(CurrentMember.Role);
        }
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

            roleText.text = $"Rooli: {currentRole.name}";
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
        // Put your logic here (e.g., toggle permission, open a menu, etc.)}
    }
}
