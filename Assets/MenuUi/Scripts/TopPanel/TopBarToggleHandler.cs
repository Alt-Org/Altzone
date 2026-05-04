using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Settings;
using System.Threading;


public class TopBarToggleHandler : MonoBehaviour
{
    public TopBarDefs.TopBarItem item;
    private Toggle _toggle;

    private const bool DebugOn = false;

    private void OnEnable()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderHandler : OnEnable()");

        if (_toggle != null)
            GetToggleValue();

        SettingsCarrier.OnTopBarChanged += HandleTopBarChanged;
    }

    private void OnDisable()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleHandler : OnDisable()");

        SettingsCarrier.OnTopBarChanged -= HandleTopBarChanged;
    }

    private void Start()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleHandler : Start()");

        _toggle = GetComponent<Toggle>();

        if (_toggle == null)
        {
            Debug.LogError($"TopBarToggleHandler: Toggle not found under {gameObject.name}", this);
            return;
        }

        GetToggleValue();
        _toggle.onValueChanged.AddListener(OnChanged);
    }

    private void OnDestroy()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleHandler : OnDestroy()");

        if (_toggle) _toggle.onValueChanged.RemoveListener(OnChanged);
        SettingsCarrier.OnTopBarChanged -= HandleTopBarChanged;
    }

    private void OnChanged(bool isOn)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleHandler : OnChanged()");

        SettingsCarrier carrier = SettingsCarrier.Instance;
        if (carrier == null) return;

        string key = TopBarDefs.Key(item) + "_" + carrier.TopBarStyleSetting;
    }

    private void GetToggleValue()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleHandler : GetToggleValue()");

        if (_toggle == null) return;

        SettingsCarrier carrier = SettingsCarrier.Instance;
        SettingsCarrier.TopBarStyle style =
            carrier != null ? carrier.TopBarStyleSetting : SettingsCarrier.TopBarStyle.NewHelena;

        string key = TopBarDefs.Key(item) + "_" + style;
        _toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(key, 1) != 0);
    }

    private void HandleTopBarChanged(int styleIndex)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleHandler : HandleTopBarChanged()");

        GetToggleValue();
    }
}
