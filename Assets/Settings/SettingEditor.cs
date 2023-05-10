using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingEditor : MonoBehaviour
{
    private SettingsCarrier carrier = SettingsCarrier.Instance;
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

    private float RoundToTwoDecimals(float toRound)
    {
        float multipliedVal = Mathf.Round(toRound * 100);
        return multipliedVal / 100;
    }
}
