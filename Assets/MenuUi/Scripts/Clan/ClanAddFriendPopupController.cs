using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanAddFriendPopupController : MonoBehaviour
{
    [Header("Popup root")]
    [SerializeField] private GameObject _popupRoot;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _playerNameText;

    [Header("Avatar")]
    [SerializeField] private AvatarFaceLoader _avatarFaceLoader;

    [Header("Buttons")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _addButton;

    private ClanMember _currentMember;

    private void Awake()
    {
        Hide();

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(Hide);
            _closeButton.onClick.AddListener(Hide);
        }

        if (_cancelButton != null)
        {
            _cancelButton.onClick.RemoveListener(Hide);
            _cancelButton.onClick.AddListener(Hide);
        }

        if (_addButton != null)
        {
            _addButton.onClick.RemoveListener(OnClickAddFriend);
            _addButton.onClick.AddListener(OnClickAddFriend);
        }
    }

    public void Show(ClanMember member)
    {
        _currentMember = member;

        if (_popupRoot != null)
            _popupRoot.SetActive(true);
        else
            gameObject.SetActive(true);

        if (_playerNameText != null)
            _playerNameText.text = member?.Name ?? string.Empty;

        UpdateAvatar(member);
    }

    public void Hide()
    {
        if (_popupRoot != null)
            _popupRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private void UpdateAvatar(ClanMember member)
    {
        if (_avatarFaceLoader == null) return;

        _avatarFaceLoader.SetUseOwnAvatarVisuals(false);

        if (member?.AvatarData == null || AvatarDesignLoader.Instance == null)
            return;

        var visualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(member.AvatarData);

        if (visualData != null)
            _avatarFaceLoader.UpdateVisuals(visualData);
    }

    private void OnClickAddFriend()
    {
        if (_currentMember == null) return;

        Debug.Log($"Add friend clicked: {_currentMember.Name}");

        // TODO: tähän myöhemmin serverikutsu friend requestille

        Hide();
    }
}
