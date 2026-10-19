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

        foreach (PlayerRights right in Enum.GetValues(typeof(PlayerRights)))
        {
            bool isAllowed = currentRole.HasRights(right);

            Debug.Log($"{right} {isAllowed}");

            string _roleText = $"Rooli: {currentRole.name}";
            roleText.text = _roleText;
            GameObject newTextObj = Instantiate(textPrefab, contentParent);

            TextMeshProUGUI _roleRights = newTextObj.GetComponent<TextMeshProUGUI>();
            Button _roleButton = newTextObj.GetComponentInChildren<Button>();

            if (_roleRights != null)
            {
                string statusColor = isAllowed ? "green" : "red";
                _roleRights.text = $"{right}: <color={statusColor}>{isAllowed}</color>";
                _roleButton.GetComponent<Image>().color = isAllowed ? Color.green : Color.red;
                _roleButton.onClick.AddListener(() =>
                {
                    OnButtonClicked(right, isAllowed, currentRole);
                });
            }
        }
    }

    private void OnButtonClicked(PlayerRights rightName, bool allowed, ClanRoles currentRole)
    {
        // Toggles rights locally for now
        Debug.Log($"Clicked button for right: {rightName}, Status: {allowed}");

        currentRole.SetRights(rightName, !allowed);
        PopulateRolesAndRights(currentRole);
        // Put your logic here (e.g., toggle permission, open a menu, etc.)}
    }
}
