using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Settings;
using System.Threading;


public class TopBarToggleHandler : MonoBehaviour
{
    public  TopBarDefs.TopBarItem item;
    private Toggle _toggle;

    private void OnEnable()
    {
        if (_toggle != null)
            GetToggleValue();

        // ? uusi: päivitys kun TopBar-tyyli vaihtuu
        SettingsCarrier.OnTopBarChanged += HandleTopBarChanged;
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
        SettingsCarrier.OnTopBarChanged -= HandleTopBarChanged; // ? uusi
    }

    private void OnChanged(bool isOn)
    {
        SettingsCarrier carrier = SettingsCarrier.Instance;
        if (carrier == null) return;

        string key = TopBarDefs.Key(item) + "_" + carrier.TopBarStyleSetting; // tyylikohtainen
        carrier.SetTopBarItemVisibleByKey(key, isOn);
    }

    private void GetToggleValue()
    {
        SettingsCarrier carrier = SettingsCarrier.Instance;
        SettingsCarrier.TopBarStyle style =
        carrier != null ? carrier.TopBarStyleSetting : SettingsCarrier.TopBarStyle.NewHelena;

        string key = TopBarDefs.Key(item) + "_" + style; // tyylikohtainen
        _toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(key, 1) != 0);
    }

    private void HandleTopBarChanged(int styleIndex)
    {
        GetToggleValue();
    }

}
