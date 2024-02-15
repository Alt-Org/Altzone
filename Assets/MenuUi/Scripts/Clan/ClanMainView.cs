using UnityEngine;
using TMPro;

public class ClanMainView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _inClanPanel;
    [SerializeField] private GameObject _noClanPanel;

    [SerializeField] private ClanLeaderboard _leaderboard;
    [SerializeField] private TextMeshProUGUI _clanCoins;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanNameTextField;

    private void OnEnable()
    {
        ToggleClanPanel(false);

        if(ServerManager.Instance.Clan != null)
        {
            ToggleClanPanel(true);
            SetPanelValues(ServerManager.Instance.Clan);
            _leaderboard?.LoadClanLeaderboard(ServerManager.Instance.Clan);
        }
    }

    private void Reset()
    {
        ToggleClanPanel(false);
        _clanNameTextField.text = "Clan Name";
        _clanMembers.text = "-1";
        _clanCoins.text = "-1";
    }

    private void ToggleClanPanel(bool isInClan)
    {
        _inClanPanel.SetActive(isInClan);
        _noClanPanel.SetActive(!isInClan);
    }

    private void SetPanelValues(ServerClan clan)
    {
        _clanNameTextField.text = clan.name;
        _clanMembers.text = clan.playerCount.ToString();
        _clanCoins.text = clan.gameCoins.ToString();
    }

    public void LeaveClan()
    {
        StartCoroutine(ServerManager.Instance.LeaveClan(success =>
        {
            if(success)
                Reset();
        }));
    }
}
