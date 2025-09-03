using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [SerializeField] private Button[] _closeButtons;   // esim. se näkymätön taustabuttoni
    [SerializeField] private GameObject _swipeBlockOverlay; // se SwipeBlockOverlay-objekti

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

        if (_swipeBlockOverlay != null)
        {
            _swipeBlockOverlay.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);

        if (_swipeBlockOverlay != null)
        {
            // tarkista onko vielä muita popuppeja aktiivisena
            bool anyPopupOpen = false;

            // käy läpi saman parentin kaikki SettingsPopupit
            var siblings = GetComponentsInParent<Transform>(true)[0].GetComponentsInChildren<SettingsPopup>(true);
            foreach (var popup in siblings)
            {
                if (popup.gameObject.activeSelf)
                {
                    anyPopupOpen = true;
                    break;
                }
            }

            if (!anyPopupOpen)
            {
                _swipeBlockOverlay.SetActive(false);
            }
        }
    }
}
