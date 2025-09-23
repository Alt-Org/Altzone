using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class TabChangeHandler : MonoBehaviour
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

    [SerializeField] protected List<ButtonWindowBind> _buttons = new List<ButtonWindowBind>();
    [SerializeField] protected int _defaultTab = 1;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            int j = i;
            _buttons[i].Button.onClick.AddListener(() => SetVisible(j));
        }
    }

    protected virtual void OnEnable()
    {
        int? value;
        if (_tablineScript.Swipe == null)
            value = DataCarrier.GetData<int?>(DataCarrier.RequestedWindow, true, suppressWarning: true);
        else
            value = DataCarrier.GetData<int?>(DataCarrier.RequestedWindow, false, suppressWarning: true);
        if (value != null) SetVisible((int)value);
        else SetVisible(_defaultTab);
    }

    protected virtual void SetVisible(int activeIndex)
    {
        // If the window uses a swipe scroll then send the message to it to change the tab, otherwise switch panels the old way.
        if (_tablineScript.Swipe == null)
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i].Window != null) _buttons[i].Window.SetActive(i == activeIndex);
            }
        else _tablineScript.Swipe.CurrentPage = activeIndex;
        _tablineScript.ActivateTabButton(activeIndex);
    }
}
