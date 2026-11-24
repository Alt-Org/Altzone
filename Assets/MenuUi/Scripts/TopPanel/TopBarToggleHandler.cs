using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Settings;
using System.Threading;


public class TopBarToggleHandler : MonoBehaviour
{
    public TopBarDefs.TopBarItem item;
    [SerializeField] private Toggle _toggle;
    public TopBarToggleDrag TogglesDrag;

    private const bool DebugOn = true;

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
        Debug.Log($"[TopBarDebug] CLICK item={item}, isOn={isOn}, go={gameObject.name}");
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarToggleHandler : OnChanged()");

        SettingsCarrier carrier = SettingsCarrier.Instance;
        if (carrier == null)
        {
            Debug.LogError("[TopBarDebug] SettingsCarrier.Instance is NULL");
            return;
        }

        string key = TopBarDefs.Key(item) + "_" + carrier.TopBarStyleSetting;

        PlayerPrefs.SetInt(key, isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[TopBarDebug] Saved {key} = {(isOn ? 1 : 0)}");

        TopBarOrderBridge.Active?.ApplyCurrentTarget();

        var target = TopBarSelector.Instance.GetTopBarTargets(carrier.TopBarStyleSetting);

        if (target != null && target.style == carrier.TopBarStyleSetting && target.IsReady())
        {
            Debug.Log($"[TB] APPLYING READY TARGET: {target.name}, parent={target.transform.parent.name}");

            target.ApplyFromSettings();
        }

        TopBarOrderBridge.Active?.RefreshClanSubItemIndent();
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
