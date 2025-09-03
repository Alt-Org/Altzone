using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [SerializeField] private Button[] _closeButtons;   // esim. se n�kym�t�n taustabuttoni
    [SerializeField] private SwipeBlockerPopupHandler _blockerHandler;

    private void Start()
    {
        foreach (Button button in _closeButtons)
        {
            button.onClick.AddListener(ClosePopup);
        }
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);

        _blockerHandler.OpenPopup(gameObject);
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);

        _blockerHandler.ClosePopup(gameObject);
    }
}
