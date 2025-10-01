using UnityEngine;
using UnityEngine.UI;


public class TopBarToggleHandler : MonoBehaviour
{
    public TopBarDefs.TopBarItem item;
    private Toggle _toggle;

    private void OnEnable()
    {
        if (_toggle != null)
            GetToggleValue();
    }

    private void Start()
    {
        _toggle = GetComponent<Toggle>();

        GetToggleValue();  

        _toggle.onValueChanged.AddListener(OnChanged);
    }

    private void OnDestroy()
    {
        if (_toggle) _toggle.onValueChanged.RemoveListener(OnChanged);
    }

    private void OnChanged(bool isOn)
    {
        SettingsCarrier.Instance
            .SetTopBarItemVisibleByKey(TopBarDefs.Key(item), isOn);
    }

    private void GetToggleValue()
    {
        // Build the key safely
        string key = TopBarDefs.Key(item);

        // Static read from PlayerPrefs so it works even if SettingsCarrier.Instance isn't ready yet
        _toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(key, 1) != 0);
    }

    
}
