using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Window;

public class ClanMemberPopupController : MonoBehaviour
{
    [SerializeField] private WindowDef _playerProfileWindowDef;

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
    [SerializeField] private TMP_Text _winsText;
    [SerializeField] private TMP_Text _lossesText;

    [Header("Role")]
    [SerializeField] private Image _roleIconImage;
    [SerializeField] private ClanRoleCatalog _roleCatalog;
    [SerializeField] private Sprite _fallbackRoleIcon;

    [Header("Bottom")]
    [SerializeField] private Button _votesButton;

    [Header("Avatar")]
    [SerializeField] private AvatarLoader _avatarLoader;

    [Header("Vote menus")]
    [SerializeField] private ClanVoteActionMenu _voteActionMenu;
    [SerializeField] private RectTransform _votesButtonRect;
    [SerializeField] private ClanRoleSelectPopupController _roleSelectPopup;

    [Header("Poll started popup")]
    [SerializeField] private GameObject _pollStartedPopup;
    [SerializeField] private CanvasGroup _pollStartedCanvasGroup;

    [SerializeField] private float _pollStartedVisibleSeconds = 1.2f;
    [SerializeField] private float _pollStartedFadeSeconds = 0.35f;

    private Coroutine _pollStartedRoutine;

    private ClanMember _currentMember;

    private void Awake()
    {
        if (_root == null) _root = gameObject;

        _pollStartedPopup.SetActive(false);

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

        if (_voteActionMenu != null && _votesButtonRect == null)
        {
            _votesButtonRect = _votesButton.GetComponent<RectTransform>();
        }

        Hide();
    }

    public void Show(ClanMember member, string roleLabel, bool allowVotes = true)
    {
        if (member == null) return;
        _currentMember = member;

        _root.SetActive(true);

        if (_votesButton != null)
        {
            _votesButton.interactable = allowVotes;
        }

        _nameText.text = member.Name ?? "";
        SetRole(roleLabel);
        _mostPlayedText.text = "Eniten pelattu hahmo:";
        _winsText.text = "Voitot:";
        _lossesText.text = "Häviöt:";

        if (_avatarLoader != null && member.AvatarData != null && AvatarDesignLoader.Instance != null)
        {
            var visualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(member.AvatarData);
            if (visualData != null)
                _avatarLoader.UpdateVisuals(visualData);
        }
    }

    private void OnDisable()
    {
        Hide();
    }

    public void Hide()
    {
        _currentMember = null;

        _root.SetActive(false);

        _voteActionMenu?.Close();
        _roleSelectPopup?.Hide();

        HidePollStartedPopupImmediate();
    }

    private void OnVotesButtonPressed()
    {
        if (_voteActionMenu == null || _votesButtonRect == null)
            return;

        _voteActionMenu.Open(
            _votesButtonRect,
            onRoleVote: OnRoleVotePressed,
            onKickVote: OnKickVotePressed
            );
    }

    private void SetRole(string roleName)
    {
        // default
        if (string.IsNullOrWhiteSpace(roleName))
        {
            _rolesText.text = "Member";

            if (_roleIconImage != null)
                _roleIconImage.gameObject.SetActive(false);

            return;
        }

        // Text: käytä catalogin display-nimeä jos löytyy
        string displayName = _roleCatalog != null ? _roleCatalog.GetDisplayName(roleName) : roleName;
        _rolesText.text = string.IsNullOrEmpty(displayName) ? roleName : displayName;

        // Icon: käytä catalogin ikonia + fallback
        if (_roleIconImage != null)
        {
            var icon = _roleCatalog != null ? _roleCatalog.GetIcon(roleName) : null;
            if (icon == null) icon = _fallbackRoleIcon;

            if (icon != null)
            {
                _roleIconImage.sprite = icon;
                _roleIconImage.gameObject.SetActive(true);
            }
            else
            {
                _roleIconImage.gameObject.SetActive(false);
            }
        }
    }


    private List<ClanRoles> GetCurrentClanRoles()
    {
        var roles = ServerManager.Instance?.Clan?.roles;

        if (roles != null)
        {
            foreach (var r in roles)
                Debug.Log($"SERVER ROLE NAME: '{r.name}'");
        }
        else
        {
            Debug.Log("SERVER ROLE NAME: roles is null (no clan loaded yet?)");
        }

        return roles;
    }

    private void OnRoleVotePressed()
    {
        var roles = GetCurrentClanRoles();
        if (roles == null || _roleSelectPopup == null || _currentMember == null) return;

        _roleSelectPopup.Show(_currentMember, roles);
    }

    private void OnKickVotePressed()
    {
        if (_currentMember == null) return;

        PollManager.CreateKickPoll(_currentMember.Id);

        ShowPollStartedPopup();

        Debug.Log($"Kick vote pressed for member: {_currentMember.Name} ({_currentMember.Id})");
    }

    private void OnOpenProfileButtonPressed()
    {
        if (_currentMember == null || string.IsNullOrEmpty(_currentMember.Id) || _playerProfileWindowDef == null)
            return;

        StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(_currentMember.Id, otherPlayer =>
        {
            if (otherPlayer == null) return;

            // Convert ServerPlayer -> PlayerData (limited view is perfect for "other player" profile)
            var otherPlayerData = new PlayerData(otherPlayer, limited: true);

            DataCarrier.GetData<PlayerData>(DataCarrier.PlayerProfile, clear: true, suppressWarning: true);
            // This is what ProfileMenu already checks
            DataCarrier.AddData(DataCarrier.PlayerProfile, otherPlayerData);

            // Open the profile window
            WindowManager.Get().ShowWindow(_playerProfileWindowDef);
        }));
    }

    private void ShowPollStartedPopup()
    {
        if (_pollStartedPopup == null || _pollStartedCanvasGroup == null) return;

        _pollStartedPopup.SetActive(true);

        // reset
        _pollStartedCanvasGroup.alpha = 1f;
        _pollStartedCanvasGroup.interactable = false;
        _pollStartedCanvasGroup.blocksRaycasts = false;

        if (_pollStartedRoutine != null)
            StopCoroutine(_pollStartedRoutine);

        _pollStartedRoutine = StartCoroutine(PollStartedRoutine());
    }

    private System.Collections.IEnumerator PollStartedRoutine()
    {        
        yield return new WaitForSecondsRealtime(_pollStartedVisibleSeconds);

        // fade out
        float t = 0f;
        float start = _pollStartedCanvasGroup.alpha;

        while (t < _pollStartedFadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / _pollStartedFadeSeconds);
            _pollStartedCanvasGroup.alpha = Mathf.Lerp(start, 0f, k);
            yield return null;
        }

        _pollStartedCanvasGroup.alpha = 0f;
        _pollStartedPopup.SetActive(false);
        _pollStartedRoutine = null;
    }

    private void HidePollStartedPopupImmediate()
    {
        if (_pollStartedRoutine != null)
        {
            StopCoroutine(_pollStartedRoutine);
            _pollStartedRoutine = null;
        }

        if (_pollStartedCanvasGroup != null)
            _pollStartedCanvasGroup.alpha = 0f;

        if (_pollStartedPopup != null)
            _pollStartedPopup.SetActive(false);
    }
}
