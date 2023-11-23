using UnityEngine;
using UnityEngine.UI;

public class SettingEditor : MonoBehaviour
{
    private SettingsCarrier carrier = SettingsCarrier.Instance;

    [SerializeField] private Button[] fpsButtons;            // 0 - Native, 1 - 60FPS, 2 - 30FPS
    [SerializeField] private Slider[] volumeSliders;

    private void OnEnable()
    {
        foreach(Slider slider in volumeSliders)
        {
            SetToSlider(slider);
        }

        SetFPSButtons();
    }

    public void SetFromSlider(Slider usedSlider)
    {
        // Somewhat hardcoded, but best i could do way of setting volume to SettingsCarrier from the sliders
        switch (usedSlider.name)
        {
            case "MasterVolume":carrier.masterVolume = RoundToTwoDecimals(usedSlider.value); break;
            case "MenuSFXVolume": carrier.menuVolume = RoundToTwoDecimals(usedSlider.value); break;
            case "MusicVolume": carrier.musicVolume = RoundToTwoDecimals(usedSlider.value); break;
            case "GameSFXVolume": carrier.soundVolume = RoundToTwoDecimals(usedSlider.value); break;
        }
    }

    public void SetToSlider(Slider usedSlider)
    {
        // Somewhat hardcoded, but best i could do way of setting volume to SettingsCarrier from the sliders
        switch (usedSlider.name)
        {
            case "MasterVolume": usedSlider.value = carrier.masterVolume; break;
            case "MenuSFXVolume": usedSlider.value = carrier.menuVolume; break;
            case "MusicVolume": usedSlider.value = carrier.musicVolume; break;
            case "GameSFXVolume": usedSlider.value = carrier.soundVolume; break;
        }
    }

    public void SetFPSButtons()
    {
        if (Application.targetFrameRate == Screen.currentResolution.refreshRate)
            fpsButtons[0].onClick.Invoke();
        else if(Application.targetFrameRate == 60)
            fpsButtons[1].onClick.Invoke();
        else if (Application.targetFrameRate == 30)
            fpsButtons[2].onClick.Invoke();
    }

    private float RoundToTwoDecimals(float toRound)
    { // Rounding the volume to two decimals so that we dont get extremely specific volumes. aka: 57.2124865223% volume
        float multipliedVal = Mathf.Round(toRound * 100);
        return multipliedVal / 100;
    }
}
