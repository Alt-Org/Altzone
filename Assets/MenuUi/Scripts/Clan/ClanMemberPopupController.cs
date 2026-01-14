using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;

public class ClanMemberPopupController : MonoBehaviour
{
    [Header("Root / Close")]
    [SerializeField] private GameObject _root;              
    [SerializeField] private Button _closeButton;           
    [SerializeField] private Button _backgroundCloseButton; 

    [Header("Top")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _openProfileButton;    

    [Header("Right info")]
    [SerializeField] private TMP_Text _rolesText;           
    [SerializeField] private TMP_Text _mostPlayedText;      
    [SerializeField] private TMP_Text _winsLossesText;    

    [Header("Bottom")]
    [SerializeField] private Button _votesButton;

    [Header("Avatar")]
    [SerializeField] private AvatarLoader _avatarLoader;


    private ClanMember _currrentMember;

    private void Awake()
    {
        if (_root == null) _root = gameObject;

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Hide);
        }

        if (_backgroundCloseButton != null)
        {
            _backgroundCloseButton.onClick.AddListener(Hide);
        }

        if (_votesButton != null)
        {
            _votesButton.onClick.AddListener(OnVotesButtonPressed);
        }

        if (_openProfileButton != null)
        {
            _openProfileButton.onClick.AddListener(OnOpenProfileButtonPressed);
        }
        Hide();
    }

    public void Show(ClanMember member, string roleLabel)
    {
        if (member == null) return;
        _currrentMember = member;

        _root.SetActive(true);

        _nameText.text = member.Name ?? "";
        _rolesText.text = string.IsNullOrEmpty(roleLabel) ? "Member" : roleLabel;
        _mostPlayedText.text = "Eniten pelattu: tulossa pian";
        _winsLossesText.text = "Voitot häviöt: tulossa pian";

        if (_avatarLoader != null && member.AvatarData != null && AvatarDesignLoader.Instance != null)
        {
            var visualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(member.AvatarData);
            if (visualData != null)
                _avatarLoader.UpdateVisuals(visualData);
        }
    }


    public void Hide()
    {
        _currrentMember = null;

        _root.SetActive(false);
    }

    private void OnVotesButtonPressed()
    {
        // TODO: Implement vote/actions functionality.
        Debug.Log("ClanMemberPopupController: OnVotesButtonPressed - Not implemented yet.");
    }

    private void OnOpenProfileButtonPressed()
    {
        //TODO: Navigate to member profile page.
        if (_currrentMember == null) return;

        Debug.Log($"ClanMemberPopupController: OnOpenProfileButtonPressed - Open profile for {_currrentMember.Name} (ID: {_currrentMember.Id}) - Not implemented yet.");
    }
}
