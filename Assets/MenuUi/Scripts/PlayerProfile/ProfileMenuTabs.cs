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
    }
    [Header("TablineScript reference")]
    public TabLine _tablineScript;

    [SerializeField] List<ButtonWindowBind> _buttons = new List<ButtonWindowBind>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            int j = i;
            _buttons[i].Button.onClick.AddListener(() => SetVisible(j));
        }
    }

    void SetVisible(int activeIndex)
    {
        for (int i = 0; i < _buttons.Count; i++)
            _buttons[i].Window.SetActive(i == activeIndex);
        _tablineScript.ActivateTabButton(activeIndex);
    }
}
