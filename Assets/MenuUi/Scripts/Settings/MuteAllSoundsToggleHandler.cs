using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteAllSoundsToggleHandler : MonoBehaviour
{
    [SerializeField] private Image _mutedImage;
    [SerializeField] private Toggle _muteToggle;

    private void OnEnable()
    {
        SetToggle(SettingsCarrier.Instance.MuteAllSounds);
        ToggleChanged(SettingsCarrier.Instance.MuteAllSounds);
        SettingsCarrier.OnMuteAllSoundsChange += SetToggle;
        _muteToggle.onValueChanged.AddListener(ToggleChanged);
    }

    private void OnDisable()
    {
        SettingsCarrier.OnMuteAllSoundsChange -= SetToggle;
        _muteToggle.onValueChanged.RemoveListener(ToggleChanged);
    }

    private void SetToggle(bool value)
    {
        _muteToggle.isOn = value;
    }

    private void ToggleChanged(bool value)
    {
        if (value)_mutedImage.gameObject.SetActive(true);
        else _mutedImage.gameObject.SetActive(false);
        SettingsCarrier.Instance.MuteAllSounds = value;
    }
}
