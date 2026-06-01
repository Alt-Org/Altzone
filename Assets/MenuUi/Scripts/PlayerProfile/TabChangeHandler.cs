using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Window;
using MenuUi.Scripts.TabLine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabChangeHandler : MonoBehaviour
{
    [System.Serializable]
    public class ButtonWindowBind
    {
        public Button Button;
        public GameObject Window;
        public string TabName;
        public GameObject TabAreaContent;
        public Image Image;
    }
    [Header("TablineScript reference")]
    public TabLine _tablineScript;

    [SerializeField] protected List<ButtonWindowBind> _buttons = new List<ButtonWindowBind>();
    [SerializeField] protected int _defaultTab = 1;
    [SerializeField] private bool _ignoreChange;
    [SerializeField] private TextMeshProUGUI _textField;

    [Header("Optional Tab Context UI")]
    [SerializeField] private bool _updateTabContextUI;
    [SerializeField] private int _primaryTabIndex = 0;
    [SerializeField] private int _secondaryTabIndex = 1;

    [SerializeField] private GameObject _primaryOnlyButton;
    [SerializeField] private GameObject _secondaryOnlyButton;
    [SerializeField] private TMP_Text _headerText;

    [SerializeField] private string _primaryHeaderText = "Kaikki klaanit";
    [SerializeField] private string _secondaryHeaderText = "Luo klaani";
    // Start is called before the first frame update
    protected virtual void Start()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            int j = i;
            _buttons[i].Button.onClick.AddListener(() => SetVisible(j));
        }
    }

    private void Awake()
    {
        TabLine.OnTabChanged += SetVisible;
    }

    protected virtual void OnEnable()
    {
        int? value;
        if (_tablineScript.Swipe == null)
            value = DataCarrier.GetData<int?>(DataCarrier.RequestedWindow, true, suppressWarning: true);
        else
            value = DataCarrier.GetData<int?>(DataCarrier.RequestedWindow, false, suppressWarning: true);
        if (value != null) SetVisible((int)value);
        else
        //Added "if(!_ignoreChange)" so that "chat channel" tabs wouldn't change
        if(!_ignoreChange) SetVisible(_defaultTab);
    }

    private void OnDestroy()
    {
        TabLine.OnTabChanged -= SetVisible;
    }

    protected virtual void SetVisible(int activeIndex)
    {
        // If the window uses a swipe scroll then send the message to it to change the tab, otherwise switch panels the old way.
        if (_tablineScript.Swipe == null)
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i].Window != null) _buttons[i].Window.SetActive(i == activeIndex);
                else continue;
                if (_buttons[i].TabAreaContent != null) _buttons[i].TabAreaContent.SetActive(i == activeIndex);
                if (i == activeIndex) _textField.text = _buttons[i].TabName;
            }
        else
        {
            _tablineScript.Swipe.CurrentPage = activeIndex;
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i].TabAreaContent != null) _buttons[i].TabAreaContent.SetActive(i == activeIndex);
                if (i == activeIndex) _textField.text = _buttons[i].TabName;
            }
        }
        _tablineScript.UpdateTabVisuals(activeIndex);
        UpdateTabContextUI(activeIndex);
    }

    private void UpdateTabContextUI(int activeIndex)
    {
        if (!_updateTabContextUI)
            return;

        bool isSecondaryTab = activeIndex == _secondaryTabIndex;

        if (_primaryOnlyButton != null)
            _primaryOnlyButton.SetActive(!isSecondaryTab);

        if (_secondaryOnlyButton != null)
            _secondaryOnlyButton.SetActive(isSecondaryTab);

        if (_headerText != null)
            _headerText.text = isSecondaryTab ? _secondaryHeaderText : _primaryHeaderText;
    }
}
