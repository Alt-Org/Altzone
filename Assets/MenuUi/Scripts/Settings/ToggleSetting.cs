using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSetting : MonoBehaviour
{
    [SerializeField] private string _name;

    private Toggle _toggle;
    private SettingsCarrier _carrier;
    [SerializeField] private SettingsCarrier.SettingsType _type;

    // Start is called before the first frame update
    void Start()
    {
        _toggle = GetComponent<Toggle>();
        _carrier = SettingsCarrier.Instance;

        CheckValidity();

        GetSavedValue();
    }

    public void GetSavedValue()
    {
        if (!CheckValidity())
        {
            return;
        }

        bool? value = _carrier.GetBoolValue(_type);

        _toggle.isOn = value ?? (PlayerPrefs.GetInt(_name, 0) != 0);
    }


    public void ChangeValue()
    {
        if(!CheckValidity())
        {
            return;
        }

        bool valueFound= _carrier.SetBoolValue(_type);

        if (!valueFound) return;

        if (_toggle.isOn) PlayerPrefs.SetInt(_name, 1);
        else PlayerPrefs.SetInt(_name, 0);
    }

    private bool CheckValidity()
    {
        if(_toggle == null || _name.Length == 0 && _type is SettingsCarrier.SettingsType.None)
        {
            Debug.LogWarning("ERROR: No Toggle found or name set");
            return false;
        }

        return true;
    }
}
