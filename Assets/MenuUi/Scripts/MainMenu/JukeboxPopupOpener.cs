using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxPopupOpener : MonoBehaviour
{
    [SerializeField] private Button _openJukeboxPopupButton;
    [SerializeField] private JukeBoxSoulhomeHandler _jukeboxSoulhomeHandler;

    private void Start()
    {
        _openJukeboxPopupButton.onClick.AddListener(() => { _jukeboxSoulhomeHandler.ToggleJukeboxScreen(true); });
    }
}
