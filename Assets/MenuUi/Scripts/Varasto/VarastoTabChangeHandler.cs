using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class VarastoTabChangeHandler : TabChangeHandler
{

    private bool _canClick = true;
    private int _currentTabIndex = -1; // Integer to track tabs

    protected override void Start()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            int j = i;
            _buttons[i].Button.onClick.AddListener(() => OnTabButtonClicked(j));
        }
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

    protected override void SetVisible(int activeIndex)
    {
        base.SetVisible(activeIndex);

        _currentTabIndex = activeIndex; // Update the currently active tab
    }
}
