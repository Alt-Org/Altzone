using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI _clanGoal;
    [SerializeField] private TextMeshProUGUI _clanAge;

    [Header("Other settings fields")]
    [SerializeField] GameObject _clanOpenObject;
    [SerializeField] GameObject _clanLockedObject;
    [SerializeField] Image _flagImage;
    [SerializeField] Transform _valueRowFirst;
    [SerializeField] Transform _valueRowSecond;

    [Header("Prefabs and scriptable objects")]
    [SerializeField] GameObject _valuePrefab;
    [SerializeField] private LanguageFlagMap _languageFlagMap;

    private void OnEnable()
    {
        ToggleClanPanel(false);

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) => SetClanProfile(clanData));
    }

    private void SetClanProfile(ClanData clan)
    {
        ToggleClanPanel(true);

        _clanName.text = clan.Name;
        _clanMembers.text = "Jäsenmäärä: " + clan.Members.Count;
        _clanCoins.text = clan.GameCoins.ToString();
        _clanPhrase.text = clan.Phrase;
        _flagImage.sprite = _languageFlagMap.GetFlag(clan.Language);
        _clanGoal.text = ClanDataTypeConverter.GetGoalText(clan.Goals);
        _clanAge.text = ClanDataTypeConverter.GetAgeText(clan.ClanAge);

        ToggleClanLockGraphic(clan.IsOpen);

        // Temp values for testing
        _clanTrophies.text = "-1";
        _clanGlobalRanking.text = "-1";
        _clanPassword.text = "";

        _leaderboard?.LoadClanLeaderboard(ServerManager.Instance.Clan);
    }

    private void Reset()
    {
        ToggleClanPanel(false);
        _clanName.text = "Clan Name";
        _clanPhrase.text = "Clan Phrase";
        _clanMembers.text = _clanCoins.text = _clanTrophies.text = _clanGlobalRanking.text = "-1";
        _clanPassword.text = _clanGoal.text = _clanAge.text = "";
        _flagImage.sprite = _languageFlagMap.GetFlag(Language.None);
    }

    private void ToggleClanPanel(bool isInClan)
    {
        _inClanPanel.SetActive(isInClan);
        _noClanPanel.SetActive(!isInClan);
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
            if (success) Reset();
        }));
    }
}
