using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingEditor : MonoBehaviour
{
    SettingsCarrier carrier = SettingsCarrier.Instance;
    public void SetFromSlider(Slider usedSlider)
    {
        switch (usedSlider.name)
        {
            case "MasterVolume": carrier.masterVolume = usedSlider.value; break;
            case "MenuSFXVolume": carrier.menuVolume = usedSlider.value; break;
            case "MusicVolume": carrier.musicVolume = usedSlider.value; break;
            case "GameSFXVolume": carrier.soundVolume = usedSlider.value; break;
        }
    }
}
