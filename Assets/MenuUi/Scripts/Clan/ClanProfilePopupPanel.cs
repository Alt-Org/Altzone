using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanProfilePopupPanel : MonoBehaviour
{
    [SerializeField] private Button _joinClanButton;
    [SerializeField] private Button _returnToMainClanViewButton;

    [Header("Text fields")]
    [SerializeField] private TextMeshProUGUI _clanNameField;
    [SerializeField] private TextMeshProUGUI _phraseField;
    [SerializeField] private TextMeshProUGUI _membersField;
    [SerializeField] private TextMeshProUGUI _coinsField;
    [SerializeField] private TextMeshProUGUI _trophiesField;
    [SerializeField] private TextMeshProUGUI _rankingField;
    [SerializeField] private TextMeshProUGUI _languageField;
    [SerializeField] private TextMeshProUGUI _goalField;
    [SerializeField] private TextMeshProUGUI _ageField;

    [Header("Lock fields")]
    [SerializeField] private GameObject _openClanField;
    [SerializeField] private GameObject _closedClanField;
    [SerializeField] private TMP_InputField _passwordField;

    private ServerClan _clan;

    private void Awake()
    {
        _joinClanButton.onClick.AddListener(JoinClan);
    }

    public void ShowClanProfile(ServerClan clan)
    {
        Debug.Log(clan.name + " " + clan._id);
        _clan = clan;

        _clanNameField.text = clan.name;
        _phraseField.text = clan.phrase;
        _membersField.text = "Jäsenmäärä: " + clan.playerCount;
        _coinsField.text = clan.gameCoins.ToString();
        _trophiesField.text = "-";
        _rankingField.text = "-";
        _languageField.text = ClanDataTypeConverter.GetLanguageText(clan.language);
        _goalField.text = ClanDataTypeConverter.GetGoalText(clan.goal);
        _ageField.text = ClanDataTypeConverter.GetAgeText(clan.ageRange);

        _openClanField.SetActive(clan.isOpen);
        _closedClanField.SetActive(!clan.isOpen);

        gameObject.SetActive(true);
    }

    public void JoinClan()
    {
        StartCoroutine(ServerManager.Instance.JoinClan(_clan, clan =>
        {
            if (clan == null) return;

            ServerManager.Instance.RaiseClanChangedEvent();
            _returnToMainClanViewButton.onClick.Invoke();
        }));
    }
}
