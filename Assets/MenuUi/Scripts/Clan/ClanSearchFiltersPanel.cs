using System;
using System.Collections.Generic;
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
    [SerializeField] private Image _languageFlagImage;
    [SerializeField] private LanguageFlagMap _languageFlagMap;
    //[SerializeField] private Button _languageButtonPrevious;
    //[SerializeField] private Button _languageButtonNext;

    [Header("Language Popup")]
    [SerializeField] private Button _openLanguagePopupButton;
    [SerializeField] private GameObject _languagePopup;
    [SerializeField] private ClanLanguageList _languageList;
    [SerializeField] private Button _languageConfirmButton;
    [SerializeField] private Button _languageCancelButton;
    [SerializeField] private Button _languageCloseButton;

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
    [SerializeField] private ClanValuesUIManager _valueManager;
    [Space]
    [SerializeField] private GameObject _filtersPopup;
 

    public Action<ClanSearchFilters> OnFiltersChanged;

    private ClanAge _clanAge = ClanAge.None;
    private Language _clanLanguage = Language.None;
    private ClanActivity _clanActivity = ClanActivity.None;
    private ClanRanking _clanRanking = ClanRanking.None;
    private ClanMembers _clanMembers = ClanMembers.None;
    private Goals _clanGoal = Goals.None;
    private bool _isOpen = true;

    
    private ClanAge[] _ageValues;
    private ClanActivity[] _activityValues;
    private ClanRanking[] _rankingValues;
    private ClanMembers[] _membersValues;

    
    private int _ageIndex;
    private int _activityIndex;
    private int _rankingIndex;
    private int _membersIndex;

    private void Awake()
    {
        InitializeEnumArrays();
    }

    private void OnEnable()
    {
        InitSelectors();
        CloseLanguagePopup();

        if (_openLanguagePopupButton != null)
            _openLanguagePopupButton.onClick.AddListener(OpenLanguagePopup);

        if (_languageConfirmButton != null)
            _languageConfirmButton.onClick.AddListener(ConfirmLanguagePopup);

        if (_languageCancelButton != null)
            _languageCancelButton.onClick.AddListener(CloseLanguagePopup);

        if (_languageCloseButton != null)
            _languageCloseButton.onClick.AddListener(CloseLanguagePopup);
    }

    private void OnDisable()
    {
        if (_openLanguagePopupButton != null)
            _openLanguagePopupButton.onClick.RemoveListener(OpenLanguagePopup);

        if (_languageConfirmButton != null)
            _languageConfirmButton.onClick.RemoveListener(ConfirmLanguagePopup);

        if (_languageCancelButton != null)
            _languageCancelButton.onClick.RemoveListener(CloseLanguagePopup);

        if (_languageCloseButton != null)
            _languageCloseButton.onClick.RemoveListener(CloseLanguagePopup);
    }

    private void InitializeEnumArrays()
    {
        _ageValues = GetEnumValues<ClanAge>();
        _activityValues = GetEnumValues<ClanActivity>();
        _rankingValues = GetEnumValues<ClanRanking>();
        _membersValues = GetEnumValues<ClanMembers>();

        
        _ageIndex = FindEnumIndex(_ageValues, _clanAge);
        _activityIndex = FindEnumIndex(_activityValues, _clanActivity);
        _rankingIndex = FindEnumIndex(_rankingValues, _clanRanking);
        _membersIndex = FindEnumIndex(_membersValues, _clanMembers);
    }

    private T[] GetEnumValues<T>() where T : Enum
    {
        return (T[])Enum.GetValues(typeof(T));
    }

    private int FindEnumIndex<T>(T[] values, T targetValue) where T : Enum
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].Equals(targetValue))
                return i;
        }
        return 0;
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

        _ageButtonNext.onClick.RemoveAllListeners();
        _ageButtonPrevious.onClick.RemoveAllListeners();

        _ageButtonNext.onClick.AddListener(() =>
        {
            _ageIndex = GetNextIndex(_ageIndex, _ageValues.Length);
            _clanAge = _ageValues[_ageIndex];
            _ageText.text = ClanDataTypeConverter.GetAgeText(_clanAge);
        });

        _ageButtonPrevious.onClick.AddListener(() =>
        {
            _ageIndex = GetPreviousIndex(_ageIndex, _ageValues.Length);
            _clanAge = _ageValues[_ageIndex];
            _ageText.text = ClanDataTypeConverter.GetAgeText(_clanAge);
        });
    }

    private void InitializeLanguage()
    {
        UpdateLanguageDisplay();
    }

    private void InitializeActivity()
    {
        _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);

        _activityButtonNext.onClick.RemoveAllListeners();
        _activityButtonPrevious.onClick.RemoveAllListeners();

        _activityButtonNext.onClick.AddListener(() =>
        {
            _activityIndex = GetNextIndex(_activityIndex, _activityValues.Length);
            _clanActivity = _activityValues[_activityIndex];
            _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);
        });

        _activityButtonPrevious.onClick.AddListener(() =>
        {
            _activityIndex = GetPreviousIndex(_activityIndex, _activityValues.Length);
            _clanActivity = _activityValues[_activityIndex];
            _activityText.text = ClanDataTypeConverter.GetActivityText(_clanActivity);
        });
    }

    private void InitializeRanking()
    {
        _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);

        _rankingButtonNext.onClick.RemoveAllListeners();
        _rankingButtonPrevious.onClick.RemoveAllListeners();

        _rankingButtonNext.onClick.AddListener(() =>
        {
            _rankingIndex = GetNextIndex(_rankingIndex, _rankingValues.Length);
            _clanRanking = _rankingValues[_rankingIndex];
            _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);
        });

        _rankingButtonPrevious.onClick.AddListener(() =>
        {
            _rankingIndex = GetPreviousIndex(_rankingIndex, _rankingValues.Length);
            _clanRanking = _rankingValues[_rankingIndex];
            _rankingText.text = ClanDataTypeConverter.GetRankingText(_clanRanking);
        });
    }

    private void InitializeMembers()
    {
        _membersText.text = ClanDataTypeConverter.GetMembersText(_clanMembers);

        _membersButtonNext.onClick.RemoveAllListeners();
        _membersButtonPrevious.onClick.RemoveAllListeners();

        _membersButtonNext.onClick.AddListener(() =>
        {
            _membersIndex = GetNextIndex(_membersIndex, _membersValues.Length);
            _clanMembers = _membersValues[_membersIndex];
            _membersText.text = ClanDataTypeConverter.GetMembersText(_clanMembers);
        });

        _membersButtonPrevious.onClick.AddListener(() =>
        {
            _membersIndex = GetPreviousIndex(_membersIndex, _membersValues.Length);
            _clanMembers = _membersValues[_membersIndex];
            _membersText.text = ClanDataTypeConverter.GetMembersText(_clanMembers);
        });
    }

    private void InitializeLock()
    {
        UpdateLockDisplay();

        _openButtonNext.onClick.RemoveAllListeners();
        _openButtonPrevious.onClick.RemoveAllListeners();

        _openButtonNext.onClick.AddListener(ToggleLock);
        _openButtonPrevious.onClick.AddListener(ToggleLock);
    }

    private void ToggleLock()
    {
        _isOpen = !_isOpen;
        UpdateLockDisplay();
    }

    private void UpdateLockDisplay()
    {
        if(SettingsCarrier.Instance.Language is not SettingsCarrier.LanguageType.English)
        _openText.text = _isOpen ? "Avoin" : "Lukittu";
        else _openText.text = _isOpen ? "Open" : "Locked";
        _lockImage.sprite = _isOpen ? _openLockSprite : _closedLockSprite;
    }

    private int GetNextIndex(int currentIndex, int arrayLength)
    {
        return (currentIndex + 1) % arrayLength;
    }

    private int GetPreviousIndex(int currentIndex, int arrayLength)
    {
        return currentIndex == 0 ? arrayLength - 1 : currentIndex - 1;
    }

    public void ApplyFilters()
    {
        OnFiltersChanged?.Invoke(new ClanSearchFilters()
        {
            clanName = "",
            activity = _clanActivity,
            age = _clanAge,
            language = _clanLanguage,
            goal = _clanGoal,
            ranking = _clanRanking,
            memberCount = _clanMembers,
            isOpen = _isOpen,
            values = _valueManager != null ? _valueManager.GetSelectedValues() : new List<ClanValues>()
        });
    }

    private void OpenLanguagePopup()
    {
        if (_languagePopup != null)
            _languagePopup.SetActive(true);

        if (_languageList != null)
            _languageList.Initialize(_clanLanguage, true);
    }

    private void ConfirmLanguagePopup()
    {
        if (_languageList != null)
        {
            _clanLanguage = _languageList.SaveLanguage();

            UpdateLanguageDisplay();
        }

        CloseLanguagePopup();
    }

    private void CloseLanguagePopup()
    {
        if (_languagePopup != null)
            _languagePopup.SetActive(false);
    }

    private void UpdateLanguageDisplay()
    {
        if (_languageText != null)
        {
            _languageText.text = _clanLanguage == Language.None
                ? "Kaikki kielet"
                : ClanDataTypeConverter.GetLanguageText(_clanLanguage);
        }

        if (_languageFlagImage != null && _languageFlagMap != null)
        {
            _languageFlagImage.sprite = _languageFlagMap.GetFlag(_clanLanguage);
        }
    }
}
