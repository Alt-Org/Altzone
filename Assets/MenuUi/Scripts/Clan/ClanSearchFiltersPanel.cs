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

    private enum Ranking
    {
        LessThan10,
    }

    private ClanAge _clanAge = ClanAge.All;
    private Language _clanLanguage = Language.None;
    private bool _isOpen = true;

    private void OnEnable()
    {
        InitSelectors();
        //_clanNameField.onEndEdit.RemoveAllListeners();
        //_clanNameField.onEndEdit.AddListener((value) => UpdateFilters());

        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(() =>
        {
            UpdateFilters();
            _filtersPopup.SetActive(false);
        });

        //UpdateFilters();
    }

    private void OnDisable()
    {
        _filtersPopup.SetActive(false);
    }

    private void InitSelectors()
    {
        InitializeAge();
        InitializeLanguage();

        //_goalDropdown.onValueChanged.RemoveAllListeners();
        //_goalDropdown.value = 0;
        //_goalDropdown.options.Clear();
        //foreach (Goals goal in Enum.GetValues(typeof(Goals)))
        //{
        //    string text = ClanDataTypeConverter.GetGoalText(goal);
        //    _goalDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        //}
        //_goalDropdown.onValueChanged.AddListener((value) => UpdateFilters());

        //_activityDropdown.onValueChanged.RemoveAllListeners();
        //_activityDropdown.value = 0;
        //_activityDropdown.options.Clear();
        //foreach (ClanActivity activity in Enum.GetValues(typeof(ClanActivity)))
        //{
        //    string text = ClanDataTypeConverter.GetActivityText(activity);
        //    _activityDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        //}
        //_activityDropdown.onValueChanged.AddListener((value) => UpdateFilters());

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

    private void InitializeLock()
    {
        _openButtonNext.onClick.RemoveAllListeners();
        _openButtonPrevious.onClick.RemoveAllListeners();

        _openButtonNext.onClick.AddListener(() =>
        {
            ToggleLock();
        });

        _openButtonPrevious.onClick.AddListener(() =>
        {
            ToggleLock();
        });
    }

    private void ToggleLock()
    {
        switch (_isOpen)
        {
            case true:
                _isOpen = false;
                _openText.text = "Lukittu";
                _lockImage.sprite = _closedLockSprite;
                break;
            case false:
                _isOpen = true;
                _openText.text = "Avoin";
                _lockImage.sprite = _openLockSprite;
                break;
        }
    }

    private void UpdateFilters()
    {
        OnFiltersChanged?.Invoke(new ClanSearchFilters()
        {
            clanName = "",
            activity = ClanActivity.None,
            age = _clanAge,
            language = _clanLanguage,
            goal = Goals.None,
            isOpen = _isOpen
        });
    }
}
