using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [SerializeField] private Button[] _closeButtons;        // background button is huge invisible button behind the popup to close it if clicked outside


    private void Start()
    {
        foreach (Button _button in _closeButtons)
        {
            _button.onClick.AddListener(() => ClosePopup());
        }
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
