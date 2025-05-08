using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.TabLine;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class ProfileMenuTabs : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            int j = i;
            _buttons[i].Button.onClick.AddListener(() => SetVisible(j));
        }
        SetVisible(_defaultTab);
    }

    void SetVisible(int activeIndex)
    {
        // If the window uses a swipe scroll then send the message to it to change the tab, otherwise switch panels the old way.
        if (_tablineScript.Swipe == null)
        for (int i = 0; i < _buttons.Count; i++)
            _buttons[i].Window.SetActive(i == activeIndex);
        else _tablineScript.Swipe.CurrentPage = activeIndex;
        _tablineScript.ActivateTabButton(activeIndex);
    }
}
