using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ClanListing contains references to instantiated ClanListPrefab.
/// </summary>
public class ClanListing : MonoBehaviour
{
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _returnToMainClanViewButton;
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanTag;
    [SerializeField] private TextMeshProUGUI _clanIsOpen;

    private ServerClan _clan;
    private string _joinMessage = "Default Join Message";

    private const string CLAN_OPEN = "Kyllä";
    private const string CLAN_CLOSED = "Ei";

    public ServerClan Clan { get => _clan; set { _clan = value; SetClanValues(); } }

    private void SetClanValues()
    {
        _clanName.text = _clan.name;
        _clanMembers.text += " " + _clan.playerCount;
        _clanTag.text += " " + _clan.tag;
        string isOpenString = _clan.isOpen ? CLAN_OPEN : CLAN_CLOSED;
        _clanIsOpen.text += " " + isOpenString;
        ToggleJoinButton(_clan.isOpen);
    }

    internal void ToggleJoinButton(bool value)
    {
        _joinButton.interactable = value;
    }
    public void JoinButtonPressed()
    {
        string body = @$"{{""clan_id"":""{Clan._id}"",""player_id"":""{ServerManager.Instance.Player._id}""}}";

        StartCoroutine(ServerManager.Instance.JoinClan(Clan, clan =>
        {
            if(clan == null)
            {
                return;
            }

            ServerManager.Instance.RaiseClanChangedEvent();
            _returnToMainClanViewButton.onClick.Invoke();
        }));
    }
}
