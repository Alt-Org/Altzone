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
    [SerializeField] private Image _ageIconImage;

    [Header("Age Popup")]
    [SerializeField] private Button _openAgePopupButton;
    [SerializeField] private GameObject _agePopup;
    [SerializeField] private ClanAgeList _ageList;
    [SerializeField] private Button _ageConfirmButton;
    [SerializeField] private Button _ageCancelButton;
    [SerializeField] private Button _ageCloseButton;


    [Header("Language Selection")]
    [SerializeField] private TextMeshProUGUI _languageText;
    [SerializeField] private Image _languageFlagImage;
    [SerializeField] private LanguageFlagMap _languageFlagMap;

    [Header("Language Popup")]
    [SerializeField] private Button _openLanguagePopupButton;
    [SerializeField] private GameObject _languagePopup;
    [SerializeField] private ClanLanguageList _languageList;
    [SerializeField] private Button _languageConfirmButton;
    [SerializeField] private Button _languageCancelButton;
    [SerializeField] private Button _languageCloseButton;

    [Header("Raycast Blockers")]
    [SerializeField] private GameObject _nestedPopupRaycastBlocker;

    [Header("Activity Selection")]
    [SerializeField] private TextMeshProUGUI _activityText;
    [SerializeField] private Button _activityButtonPrevious;
    [SerializeField] private Button _activityButtonNext;

    [Header("Open Selection")]
    [SerializeField] private TextMeshProUGUI _openText;
    [SerializeField] private Button _lockButton;
    [SerializeField] private Image _lockImage;
    [SerializeField] private Image _lockBackgroundImage;
    [SerializeField] private Sprite _openLockSprite;
    [SerializeField] private Sprite _closedLockSprite;

    [Header("Lock Background Colors")]
    [SerializeField] private Color _openBackgroundColor = Color.white;
    [SerializeField] private Color _lockedBackgroundColor;

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

        CloseAllPopups();

        if (_openLanguagePopupButton != null)
            _openLanguagePopupButton.onClick.AddListener(OpenLanguagePopup);

        if (_languageConfirmButton != null)
            _languageConfirmButton.onClick.AddListener(ConfirmLanguagePopup);

        if (_languageCancelButton != null)
            _languageCancelButton.onClick.AddListener(CloseLanguagePopup);

        if (_languageCloseButton != null)
            _languageCloseButton.onClick.AddListener(CloseLanguagePopup);


        if (_openAgePopupButton != null)
            _openAgePopupButton.onClick.AddListener(OpenAgePopup);

        if (_ageConfirmButton != null)
            _ageConfirmButton.onClick.AddListener(ConfirmAgePopup);

        if (_ageCancelButton != null)
            _ageCancelButton.onClick.AddListener(CloseAgePopup);

        if (_ageCloseButton != null)
            _ageCloseButton.onClick.AddListener(CloseAgePopup);

        if (_nestedPopupRaycastBlocker != null)
            _nestedPopupRaycastBlocker.SetActive(false);
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


        if (_openAgePopupButton != null)
            _openAgePopupButton.onClick.RemoveListener(OpenAgePopup);

        if (_ageConfirmButton != null)
            _ageConfirmButton.onClick.RemoveListener(ConfirmAgePopup);

        if (_ageCancelButton != null)
            _ageCancelButton.onClick.RemoveListener(CloseAgePopup);

        if (_ageCloseButton != null)
            _ageCloseButton.onClick.RemoveListener(CloseAgePopup);
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
        UpdateAgeDisplay();
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

        if (_lockButton != null)
        {
            _lockButton.onClick.RemoveAllListeners();
            _lockButton.onClick.AddListener(ToggleLock);
        }
    }

    private void ToggleLock()
    {
        _isOpen = !_isOpen;
        UpdateLockDisplay();
    }

    private void UpdateLockDisplay()
    {
        if (_openText != null)
        {
            if (SettingsCarrier.Instance.Language is not SettingsCarrier.LanguageType.English)
            {
                _openText.text = _isOpen ? "Avoin" : "Lukittu";
            }
            else
            {
                _openText.text = _isOpen ? "Open" : "Locked";
            }
        }

        if (_lockImage != null)
        {
            _lockImage.sprite = _isOpen ? _openLockSprite : _closedLockSprite;
        }

        if (_lockBackgroundImage != null)
        {
            _lockBackgroundImage.color = _isOpen ? _openBackgroundColor : _lockedBackgroundColor;
        }
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

    private void CloseAllPopups()
    {
        if (_languagePopup != null)
            _languagePopup.SetActive(false);

        if (_agePopup != null)
            _agePopup.SetActive(false);

        if (_nestedPopupRaycastBlocker != null)
            _nestedPopupRaycastBlocker.SetActive(false);
    }

    private void OpenLanguagePopup()
    {
        if (_nestedPopupRaycastBlocker != null)
            _nestedPopupRaycastBlocker.SetActive(true);

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

        UpdateNestedPopupRaycastBlocker();
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

    private void OpenAgePopup()
    {
        if (_nestedPopupRaycastBlocker != null)
            _nestedPopupRaycastBlocker.SetActive(true);

        if (_agePopup != null)
            _agePopup.SetActive(true);

        if (_ageList != null)
            _ageList.Initialize(_clanAge, true);
    }

    private void ConfirmAgePopup()
    {
        if (_ageList != null)
        {
            _clanAge = _ageList.SaveAge();
            UpdateAgeDisplay();
        }

        CloseAgePopup();
    }

    private void CloseAgePopup()
    {
        if (_agePopup != null)
            _agePopup.SetActive(false);

        UpdateNestedPopupRaycastBlocker();
    }

    private void UpdateAgeDisplay()
    {
        if (_ageText != null)
        {
            _ageText.text = _clanAge == ClanAge.None
                ? "Kaikki iät"
                : ClanDataTypeConverter.GetAgeText(_clanAge);
        }

        if (_ageIconImage != null && _ageList != null)
        {
            _ageIconImage.sprite = _ageList.GetAgeSprite(_clanAge);
        }
    }

    private void UpdateNestedPopupRaycastBlocker()
    {
        bool languageOpen = _languagePopup != null && _languagePopup.activeSelf;
        bool ageOpen = _agePopup != null && _agePopup.activeSelf;

        if (_nestedPopupRaycastBlocker != null)
            _nestedPopupRaycastBlocker.SetActive(languageOpen || ageOpen);
    }
}
