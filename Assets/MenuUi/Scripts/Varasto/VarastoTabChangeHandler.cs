using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class VarastoTabChangeHandler : MonoBehaviour
{
    [System.Serializable]
    public class ButtonWindowBind
    {
        public Button Button;
        public GameObject Window;
        public Image Image;
    }

    [Header("TablineScript reference")]
    public TabLine _tablineScript;

    [SerializeField] List<ButtonWindowBind> _buttons = new List<ButtonWindowBind>();

    [SerializeField] private int _defaultTab = 1;

    private bool _canClick = true;

    private int _currentTabIndex = -1; // Integer to track tabs

    void Start()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            int j = i;
            _buttons[i].Button.onClick.AddListener(() => OnTabButtonClicked(j));
        }
    }

    private void OnEnable()
    {
        int? value;
        if (_tablineScript.Swipe == null)
            value = DataCarrier.GetData<int?>(DataCarrier.RequestedWindow, true, suppressWarning: true);
        else
            value = DataCarrier.GetData<int?>(DataCarrier.RequestedWindow, false, suppressWarning: true);

        if (value != null) SetVisible((int)value);
        else SetVisible(_defaultTab);
    }

    private void OnTabButtonClicked(int index)
    {
        if (!_canClick)
            return;

        if (index == _currentTabIndex)
            return; // Ignore the click if the index matches the index of the current tab

        StartCoroutine(HandleTabChangeWithDelay(index));
    }

    // Coroutine to delay tab change to avoid Varasto breaking due to spamming tab change
    private IEnumerator HandleTabChangeWithDelay(int index)
    {
        _canClick = false;
        SetVisible(index);

        yield return new WaitForSeconds(0.5f); // Delay to prevent tab spamming

        _canClick = true;
    }

    void SetVisible(int activeIndex)
    {
        // If the window uses a swipe scroll then send the message to it to change the tab, otherwise switch panels the old way.
        if (_tablineScript.Swipe == null)
            for (int i = 0; i < _buttons.Count; i++)
                _buttons[i].Window.SetActive(i == activeIndex);
        else _tablineScript.Swipe.CurrentPage = activeIndex;
        _tablineScript.ActivateTabButton(activeIndex);

        _currentTabIndex = activeIndex; // Update the currently active tab
    }
}
