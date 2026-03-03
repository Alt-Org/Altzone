using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using UnityEngine;
using UnityEngine.UI;

public class SliderSetting : MonoBehaviour
{
    [SerializeField] private SettingsCarrier.SoundType _type;

    private Slider _slider;
    private SettingsCarrier _carrier;

    void Start()
    {
        _slider = GetComponent<Slider>();
        _carrier = SettingsCarrier.Instance;

        if (!CheckValidity()) return;

        _slider.onValueChanged.AddListener((value) => ValueChanged(value));
        GetSavedValue();
    }

    private void OnEnable() { GetSavedValue(); }

    public void GetSavedValue()
    {
        if (!CheckValidity()) return;

        _slider.value = _carrier.SentVolume(_type);
    }

    public void ValueChanged(float value)
    {
        if (!CheckValidity()) return;

        _carrier.SetVolume(_type, value);
        AudioManager.Instance.UpdateMaxVolume();
    }

    private bool CheckValidity()
    {
        if (_slider == null)
        {
            Debug.LogWarning("ERROR: No Slider found or name set");
            return false;
        }

        return true;
    }
}
