using System;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanSearchFiltersPanel : MonoBehaviour
{
    [Header("Member Selection")]
    [SerializeField] private TextMeshProUGUI _membersText;
    [SerializeField] private Button _membersButtonPrevious;
    [SerializeField] private Button _membersButtonNext;

    [Header("Ranking Selection")]
    [SerializeField] private TextMeshProUGUI _rankingText;
    [SerializeField] private Button _rankingButtonPrevious;
    [SerializeField] private Button _rankingButtonNext;

    [Header("Age Selection")]
    [SerializeField] private TextMeshProUGUI _ageText;
    [SerializeField] private Button _ageButtonPrevious;
    [SerializeField] private Button _ageButtonNext;

    [Header("Language Selection")]
    [SerializeField] private TextMeshProUGUI _languageText;
    [SerializeField] private LanguageFlagImage _languageFlag;
    [SerializeField] private Button _languageButtonPrevious;
    [SerializeField] private Button _languageButtonNext;

    [Header("Activity Selection")]
    [SerializeField] private TextMeshProUGUI _activityText;
    [SerializeField] private Button _activityButtonPrevious;
    [SerializeField] private Button _activityButtonNext;

    [Header("Open Selection")]
    [SerializeField] private TextMeshProUGUI _openText;
    [SerializeField] private Button _openButtonPrevious;
    [SerializeField] private Button _openButtonNext;
    [SerializeField] private Image _lockImage;
    [SerializeField] private Sprite _openLockSprite;
    [SerializeField] private Sprite _closedLockSprite;

    [Header("Values selection")]
    [Space]
    [SerializeField] private Button _confirmButton;
    [SerializeField] private GameObject _filtersPopup;

    public Action<ClanSearchFilters> OnFiltersChanged;

    private ClanAge _clanAge = ClanAge.All;
    private Language _clanLanguage = Language.None;
    private ClanActivity _clanActivity = ClanActivity.None;
    private ClanRanking _clanRanking = ClanRanking.All;
    private ClanMembers _clanMembers = ClanMembers.All;
    private Goals _clanGoal = Goals.None;  // Lisätty puuttuva goal
    private bool _isOpen = true;

    private void OnEnable()
    {
        InitSelectors();
        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(() =>
        {
            UpdateFilters();
            _filtersPopup.SetActive(false);
        });
    }

    private void OnDisable()
    {
        _filtersPopup.SetActive(false);
    }

    private void InitSelectors()
    {
        InitializeAge();
        InitializeLanguage();
        InitializeActivity();
        InitializeRanking();
        InitializeMembers();
        InitializeLock();
    }

    private void InitializeAge()
    {
        _ageText.text = ClanDataTypeConverter.GetAgeText(_clanAge);

        _ageButtonNext.onClick.AddListener(() =>
        {
            bool isLast = _clanAge == Enum.GetValues(typeof(ClanAge)).Cast<ClanAge>().Last();
            _clanAge = isLast ? (ClanAge)1 : _clanAge + 1;
            _ageText.text = ClanDataTypeConverter.GetAgeText(_clanAge);
        });

        _ageButtonPrevious.onClick.AddListener(() =>
        {
            bool isFirst = (int)_clanAge <= 1;
            _clanAge = isFirst ? Enum.GetValues(typeof(ClanAge)).Cast<ClanAge>().Last() : _clanAge - 1;
            _ageText.text = ClanDataTypeConverter.GetAgeText(_clanAge);
        });
    }

    private void InitializeLanguage()
    {
        _languageText.text = ClanDataTypeConverter.GetLanguageText(_clanLanguage);
        _languageFlag.SetFlag(_clanLanguage);

        _languageButtonNext.onClick.AddListener(() =>
        {
            bool isLast = _clanLanguage == Enum.GetValues(typeof(Language)).Cast<Language>().Last();
            _clanLanguage = isLast ? (Language)1 : _clanLanguage + 1;
            _languageText.text = ClanDataTypeConverter.GetLanguageText(_clanLanguage);
            _languageFlag.SetFlag(_clanLanguage);
        });

        _languageButtonPrevious.onClick.AddListener(() =>
        {
            bool isFirst = (int)_clanLanguage <= 1;
            _clanLanguage = isFirst ? Enum.GetValues(typeof(Language)).Cast<Language>().Last() : _clanLanguage - 1;
            _languageText.text = ClanDataTypeConverter.GetLanguageText(_clanLanguage);
            _languageFlag.SetFlag(_clanLanguage);
        });
    }

    private void InitializeActivity()
    {
        _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);

        _activityButtonNext.onClick.AddListener(() =>
        {
            bool isLast = _clanActivity == Enum.GetValues(typeof(ClanActivity)).Cast<ClanActivity>().Last();
            _clanActivity = isLast ? (ClanActivity)1 : _clanActivity + 1;
            _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);
        });

        _activityButtonPrevious.onClick.AddListener(() =>
        {
            bool isFirst = (int)_clanActivity <= 1;
            _clanActivity = isFirst ? Enum.GetValues(typeof(ClanActivity)).Cast<ClanActivity>().Last() : _clanActivity - 1;
            _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);
        });
    }

    private void InitializeRanking()
    {
        _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);

        _rankingButtonNext.onClick.AddListener(() =>
        {
            bool isLast = _clanRanking == Enum.GetValues(typeof(ClanRanking)).Cast<ClanRanking>().Last();
            _clanRanking = isLast ? (ClanRanking)1 : _clanRanking + 1;
            _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);
        });

        _rankingButtonPrevious.onClick.AddListener(() =>
        {
            bool isFirst = (int)_clanRanking <= 1;
            _clanRanking = isFirst ? Enum.GetValues(typeof(ClanRanking)).Cast<ClanRanking>().Last() : _clanRanking - 1;
            _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);
        });
    }

    private void InitializeMembers()
    {
        _membersText.text = ClanDataTypeConverter.GetMembersText(_clanMembers);

        _membersButtonNext.onClick.AddListener(() =>
        {
            bool isLast = _clanMembers == Enum.GetValues(typeof(ClanMembers)).Cast<ClanMembers>().Last();
            _clanMembers = isLast ? (ClanMembers)1 : _clanMembers + 1;
            _membersText.text = ClanDataTypeConverter.GetMembersText(_clanMembers);
        });

        _membersButtonPrevious.onClick.AddListener(() =>
        {
            bool isFirst = (int)_clanMembers <= 1;
            _clanMembers = isFirst ? Enum.GetValues(typeof(ClanMembers)).Cast<ClanMembers>().Last() : _clanMembers - 1;
            _membersText.text = ClanDataTypeConverter.GetMembersText(_clanMembers);
        });
    }

    private void InitializeLock()
    {
        _openButtonNext.onClick.RemoveAllListeners();
        _openButtonPrevious.onClick.RemoveAllListeners();

        _openButtonNext.onClick.AddListener(() => ToggleLock());
        _openButtonPrevious.onClick.AddListener(() => ToggleLock());
    }

    private void ToggleLock()
    {
        _isOpen = !_isOpen;
        _openText.text = _isOpen ? "Avoin" : "Lukittu";
        _lockImage.sprite = _isOpen ? _openLockSprite : _closedLockSprite;
    }

    private void UpdateFilters()
    {
        OnFiltersChanged?.Invoke(new ClanSearchFilters()
        {
            clanName = "",
            activity = _clanActivity,
            age = _clanAge,
            language = _clanLanguage,
            goal = _clanGoal,      // Lisätty puuttuva goal kenttä
            ranking = _clanRanking,
            memberCount = _clanMembers,  // Korjattu: members -> memberCount
            isOpen = _isOpen
        });
    }
}
