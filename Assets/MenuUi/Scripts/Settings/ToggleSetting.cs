using UnityEngine;
using UnityEngine.UI;

public class ToggleSetting : MonoBehaviour
{
    [SerializeField] private string _name;

    private Toggle _toggle;
    private SettingsCarrier _carrier;
    [SerializeField] private SettingsCarrier.SettingsType _type;
    private bool _ignoreChange = false;

    // Start is called before the first frame update
    private void Start()
    {
        _toggle = GetComponent<Toggle>();
        _carrier = SettingsCarrier.Instance;

        GetSavedValue();
    }

    private void OnEnable() { if (_toggle && _carrier) GetSavedValue(); }

    private void GetSavedValue()
    {
        if (!CheckValidity()) return;

        bool? value = _carrier.GetBoolValue(_type);

        bool newValue = value ?? (PlayerPrefs.GetInt(_name, 0) != 0);

        if (_toggle.isOn != newValue) _ignoreChange = true;

        _toggle.isOn = newValue;
    }


    public void ChangeValue()
    {
        if(!CheckValidity() || _ignoreChange)
        {
            _ignoreChange = false;
            return;
        }

        bool valueFound = _carrier.SetBoolValue(_type);

        if (!valueFound) return;

        PlayerPrefs.SetInt(_name, _toggle.isOn ? 1 : 0);
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
