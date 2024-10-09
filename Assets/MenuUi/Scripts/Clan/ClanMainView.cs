using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Clan;

public class ClanMainView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _inClanPanel;
    [SerializeField] private GameObject _noClanPanel;

    [SerializeField] private ClanLeaderboard _leaderboard;

    [Header("Text fields")]
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanPhrase;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanCoins;
    [SerializeField] private TextMeshProUGUI _clanTrophies;
    [SerializeField] private TextMeshProUGUI _clanGlobalRanking;
    [SerializeField] private TextMeshProUGUI _clanPassword;
    [SerializeField] private TextMeshProUGUI _clanLanguage;
    [SerializeField] private TextMeshProUGUI _clanGoal;
    [SerializeField] private TextMeshProUGUI _clanAge;

    [Header("Other settings fields")]
    [SerializeField] GameObject _clanOpenObject;
    [SerializeField] GameObject _clanLockedObject;
    [SerializeField] Transform _valueRowFirst;
    [SerializeField] Transform _valueRowSecond;

    [Header("Prefabs")]
    [SerializeField] GameObject _valuePrefab;

    private void OnEnable()
    {
        ToggleClanPanel(false);

        if (ServerManager.Instance.Clan != null)
        {
            ToggleClanPanel(true);
            SetPanelValues(ServerManager.Instance.Clan);
            _leaderboard?.LoadClanLeaderboard(ServerManager.Instance.Clan);
        }
    }

    private void Reset()
    {
        ToggleClanPanel(false);
        _clanName.text = "Clan Name";
        _clanPhrase.text = "Klaanin motto";
        _clanMembers.text = "-1";
        _clanCoins.text = "-1";
        _clanTrophies.text = "-1";
        _clanGlobalRanking.text = "-1";
        _clanPassword.text = "SalainenSana123";
        _clanLanguage.text = "suomi";
        _clanGoal.text = "Tavoite";
        _clanAge.text = "Kaikki";
    }

    private void ToggleClanPanel(bool isInClan)
    {
        _inClanPanel.SetActive(isInClan);
        _noClanPanel.SetActive(!isInClan);
    }

    private void SetPanelValues(ServerClan clan)
    {
        _clanName.text = clan.name;
        _clanMembers.text = "Jäsenmäärä: " + clan.playerCount.ToString();
        _clanCoins.text = clan.gameCoins.ToString();
        _clanPhrase.text = clan.phrase;
        _clanLanguage.text = ClanDataTypeConverter.GetLanguageText(clan.language);
        _clanGoal.text = ClanDataTypeConverter.GetGoalText(clan.goals);
        _clanAge.text = ClanDataTypeConverter.GetAgeText(clan.clanAge);

        Debug.Log($"language: {clan.language}, goal: {clan.goals}, age: {clan.clanAge}");

        ToggleClanLockGraphic(clan.isOpen);

        // Temp values for testing
        _clanTrophies.text = "-1";
        _clanGlobalRanking.text = "-1";
        _clanPassword.text = "";
    }

    private void ToggleClanLockGraphic(bool isClanOpen)
    {
        _clanOpenObject.SetActive(isClanOpen);
        _clanLockedObject.SetActive(!isClanOpen);
    }

    public void LeaveClan()
    {
        StartCoroutine(ServerManager.Instance.LeaveClan(success =>
        {
            if (success)
                Reset();
        }));
    }
}
