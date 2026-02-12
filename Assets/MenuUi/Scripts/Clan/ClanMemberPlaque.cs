using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClanMemberPlaque : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankingPositionText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _roleText;
    [SerializeField] private GameObject _rosetteObject;
    [SerializeField] private Button _voteButton;

    public RectTransform VoteButtonRect => _voteButton != null ? _voteButton.GetComponent<RectTransform>() : null;

    [Header("Role UI")]
    [SerializeField] private Image _roleIconImage;
    [SerializeField] private ClanRoleCatalog _roleCatalog;
    [SerializeField] private Sprite _fallbackRoleIcon;

    public void SetPosition(int position)
    {
        if (_rankingPositionText != null) _rankingPositionText.text = position.ToString();
    }

    public void SetName(string name)
    {
        if (_nameText != null) _nameText.text = name ?? "";
    }

    public void SetRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            if (_roleText != null) _roleText.text = "Member";
            SetRoleIconVisible(false);
            return;
        }

        if (_roleText != null)
        {
            string displayName = _roleCatalog != null ? _roleCatalog.GetDisplayName(role) : role;
            _roleText.text = string.IsNullOrEmpty(displayName) ? role : displayName;
        }

        if (_roleIconImage != null)
        {
            Sprite icon = _roleCatalog != null ? _roleCatalog.GetIcon(role) : null;
            if (icon == null) icon = _fallbackRoleIcon;

            if (icon != null)
            {
                _roleIconImage.sprite = icon;
                SetRoleIconVisible(true);
            }
            else
            {
                SetRoleIconVisible(false);
            }
        }
    }

    public void SetActivityRosette(bool isActive)
    {
        if (_rosetteObject != null) _rosetteObject.SetActive(isActive);
    }

    private void SetRoleIconVisible(bool visible)
    {
        if (_roleIconImage != null)
            _roleIconImage.gameObject.SetActive(visible);
    }

    public void SetVoteInteractable(bool value)
    {
        if (_voteButton != null) _voteButton.interactable = value;
    }

    public void BindVote(System.Action onVote)
    {
        if (_voteButton == null) return;
        _voteButton.onClick.RemoveAllListeners();
        if (onVote != null) _voteButton.onClick.AddListener(() => onVote());
    }
}

