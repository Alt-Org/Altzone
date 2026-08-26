using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.CharacterGallery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterFilterPanel : MonoBehaviour
{
    [Header("Popup open buttons")]
    [SerializeField] private GameObject _filterPopUp;
    [SerializeField] private Button _openFilterPopUpButton1;
    [SerializeField] private Button _openFilterPopUpButton2;
    [SerializeField] private Button _openFilterPopUpButton3;

    [Header("Filter toggles")]
    [SerializeField] private Toggle _filterConscious;
    [SerializeField] private Toggle _filterUnconscious;

    [SerializeField] private GameObject _filterConsciousText;
    [SerializeField] private GameObject _filterConsciousTextBold;
    [SerializeField] private GameObject _filterUnconsciousText;
    [SerializeField] private GameObject _filterUnconsciousTextBold;

    [SerializeField] private Toggle _toggleAll;
    [SerializeField] private List<Toggle> _classFilterToggles = new List<Toggle>();

    [Header("Dropdown")]
    [SerializeField] private TMP_Dropdown _classSortDropdown;
    [SerializeField] private GameObject _dropDownArrowUp;
    [SerializeField] private GameObject _dropDownArrowDown;

    [Header("Popup close buttons")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _confirmButton;

    [Header("GalleryView")]
    [SerializeField] private GalleryView _galleryView;


    private List<CharacterClassType> _filteredClasses = new List<CharacterClassType>();

    private enum SortOrder
    {
        Classes = 0,
        Alphabetical = 1,
        AlphabeticalReverse = 2,
        LowestLevel = 3,
        HighestLevel = 4,
        RarestFirst = 5,
        RarestLast = 6,
        FavouritesFirst = 7
    }

    private SortOrder _sortOrder = SortOrder.Classes;


    private void Start()
    {
        _openFilterPopUpButton1.onClick.AddListener(Open);
        _openFilterPopUpButton2.onClick.AddListener(Open);
        _openFilterPopUpButton3.onClick.AddListener(Open);

        _closeButton.onClick.AddListener(Close);
        _cancelButton.onClick.AddListener(Close);

        _confirmButton.onClick.AddListener(CloseAndApply);

        _filterConscious.onValueChanged.AddListener(ConsciousToggleBold);
        _filterUnconscious.onValueChanged.AddListener(UnconsciousToggleBold);

        foreach (Toggle toggle in _classFilterToggles)
        {
            toggle.onValueChanged.AddListener(CheckClassToggles);
        }

        _classSortDropdown.onValueChanged.AddListener(ChangeDropDownValue);
    }


    #region Dropdown controls
    public void Update()
    {
        //control the direction of the drop down arrow
        if (_classSortDropdown.IsExpanded) 
        {
            _dropDownArrowUp.SetActive(true);
            _dropDownArrowDown.SetActive(false);
        }
        else
        {
            _dropDownArrowUp.SetActive(false);
            _dropDownArrowDown.SetActive(true);
        }
    }

    private void ChangeDropDownValue(int value)
    {
        switch (value)
        {
            case 0:
                _sortOrder = SortOrder.Classes;
                break;
            case 1:
                _sortOrder = SortOrder.Alphabetical;
                break;
            case 2:
                _sortOrder = SortOrder.AlphabeticalReverse;
                break;
            case 3:
                _sortOrder = SortOrder.LowestLevel;
                break;
            case 4:
                _sortOrder = SortOrder.HighestLevel;
                break;
            case 5:
                _sortOrder = SortOrder.RarestFirst;
                break;
            case 6:
                _sortOrder = SortOrder.RarestLast;
                break;
            case 7:
                _sortOrder = SortOrder.FavouritesFirst;
                break;

        }
    }
    #endregion

    #region Controls for _toggleAll
    private void CheckClassToggles(bool toggleIsOn) //if all class toggles are on, set _toggleAll on (and in reverse)
    {
        if (!toggleIsOn)
        {
            _toggleAll.isOn = false;
            return;
        }

        int i = 0;
        foreach (Toggle toggle in _classFilterToggles)
        {
            if (toggle.isOn)
                i++;
        }

        if (i < 7)
        {
            _toggleAll.isOn = false;
        }
        else if(i == 7)
        {
            _toggleAll.isOn = true;
        }
    }

    public void ToggleAllCheck() //toggle all class filters on or off
    {
        if (_toggleAll.isOn)
        {
            foreach (Toggle toggle in _classFilterToggles)
            {
                toggle.isOn = true;
            }
        }
        else if (!_toggleAll.isOn)
        {
            foreach (Toggle toggle in _classFilterToggles)
            {
                toggle.isOn = false;
            }
        }
    }
    #endregion

    #region Turn the bold text on/off for the concious toggles
    private void ConsciousToggleBold(bool toggleIsOn)
    {
        if (toggleIsOn)
        {
            _filterConsciousText.SetActive(false);
            _filterConsciousTextBold.SetActive(true);
        }
        else if (!toggleIsOn)
        {
            _filterConsciousText.SetActive(true);
            _filterConsciousTextBold.SetActive(false);
        }

        /*
        TextMeshProUGUI toggleText = null;
     
        if (toggleIsOn)
        {
            //toggleText.text = _filterAcknowledged.GetComponentInChildren<TextMeshProUGUI>().text;
            //_filterAcknowledged.GetComponentInChildren<TextMeshProUGUI>().text = $"<b> {toggleText} </b>";
            toggleText = _filterAcknowledgedText;

            _filterAcknowledgedText.text = $"<b> {toggleText} </b>";
        }
        else if (!toggleIsOn)
        {
            //toggleText.text = _filterAcknowledged.GetComponentInChildren<TextMeshProUGUI>().text;
            //_filterAcknowledged.GetComponentInChildren<TextMeshProUGUI>().text = toggleText.ToString();
            toggleText = _filterAcknowledgedText;

            _filterAcknowledgedText.text = toggleText.text;

        }*/

    }

    private void UnconsciousToggleBold(bool toggleIsOn)
    {
        if (toggleIsOn)
        {
            _filterUnconsciousText.SetActive(true);
            _filterUnconsciousTextBold.SetActive(true);
        }
        else if (!toggleIsOn)
        {
            _filterUnconsciousText.SetActive(true);
            _filterUnconsciousTextBold.SetActive(false);
        }
    }
    #endregion


    #region Check filters
    private void CheckConsciousFilters()
    {
        if (_filterConscious.isOn && _filterUnconscious.isOn)
        {
            _galleryView.FilterUnlocked(true, true);
        }
        else if (_filterConscious.isOn && !_filterUnconscious.isOn)
        {
            _galleryView.FilterUnlocked(true, false);
        }
        else if (_filterUnconscious.isOn && !_filterConscious.isOn)
        {
            _galleryView.FilterUnlocked(false, true);
        }
        else if (!_filterUnconscious.isOn && !_filterConscious.isOn)
        {
            _galleryView.FilterUnlocked(false, false);
        }
    }

    private void CheckClassFilters()
    {
        _filteredClasses.Clear();

        int i = 0;

        foreach(Toggle toggle in _classFilterToggles)
        {
            if (toggle.isOn)
            {
                _filteredClasses.Add(CharacterClassType.Desensitizer + i);
            }
            i += 100;
        }

        _galleryView.FilterClasses(_filteredClasses);
    }
    #endregion



    #region Open, close and apply
    public void Open()
    {
        _filterPopUp.SetActive(true);
    }

    private void Close()
    {
        _filterPopUp.SetActive(false);
    }

    private void CloseAndApply()
    {
        // set filters
        _galleryView.ResetFilter();
        CheckClassFilters();
        CheckConsciousFilters();

        //organize characters (all ways of organizing are not possible currently)
        switch (_sortOrder) {
            case SortOrder.Classes:
                _galleryView.OrganizeGallery(false, true);
                break;
            case SortOrder.Alphabetical:
                _galleryView.OrganizeGallery(false, false);
                break;
            case SortOrder.AlphabeticalReverse:
                _galleryView.OrganizeGallery(true, false);
                break;
            case SortOrder.LowestLevel:
                break;
            case SortOrder.HighestLevel:
                break;
            case SortOrder.RarestFirst:
                break;
            case SortOrder.RarestLast:
                break;
            case SortOrder.FavouritesFirst:
                break;
        }

        Debug.Log("Close and apply");

        _filterPopUp.SetActive(false);
    }
    #endregion
}
