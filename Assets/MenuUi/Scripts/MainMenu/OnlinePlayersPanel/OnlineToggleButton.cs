using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class OnlineToggleButton : MonoBehaviour
{
    [SerializeField] private Button _onlineToggleButton;
    [SerializeField] private PopupButtonVisual _buttonVisuals;

    private void OnEnable()
    {
        _onlineToggleButton.onClick.AddListener(ToggleOnlinePlayers);

        if(_buttonVisuals) _buttonVisuals.ButtonSelected(OnlinePlayersPanel.Instance.IsOpen);

        OnlinePlayersPanel.OnToggleOnlinePlayerButton += ToggleVisuals;
    }

    private void OnDisable()
    {
        _onlineToggleButton?.onClick.RemoveAllListeners();
    }

    private void ToggleOnlinePlayers()
    {
        OverlayPanelCheck.ToggleOnlinePlayers();
    }

    private void ToggleVisuals(bool? value)
    {
        if(_buttonVisuals && value.HasValue)_buttonVisuals.ButtonSelected((bool)value);
    }
}
