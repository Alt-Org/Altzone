using MenuUi.Scripts.MainMenu;
using MenuUi.Scripts.Settings.BattleUiEditor;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class SettingEditor : MonoBehaviour
{
    private SettingsCarrier carrier = SettingsCarrier.Instance;
    private MainMenuController mainMenuController = null;

    [SerializeField] private Toggle[] fpsButtons;            // 0 - Native, 1 - 60FPS, 2 - 30FPS
    [SerializeField] private Slider[] volumeSliders;
    [SerializeField] private Toggle _introSkipToggle;
    [SerializeField] private Toggle _showButtonLabelsToggle;
    [SerializeField] private Button _battleSettingsButton;
    [SerializeField] private BattleUiEditor _battleEditor;

    private void OnEnable()
    {
        if(mainMenuController == null)
            mainMenuController = FindObjectOfType<MainMenuController>(true);

        foreach (Slider slider in volumeSliders)
        {
            SetToSlider(slider);
        }

        SetFPSButtons();
        SetIntroSkipToggle();
        SetShowButtonLabelsToggle();

        if (DataCarrier.GetData<object>(DataCarrier.BattleUiEditorRequested) != null) _battleEditor.OpenEditor();
    }
    private void Start()
    {
        _battleSettingsButton.onClick.AddListener(() => _battleEditor.OpenEditor());
    }

    public void SetFromSlider(Slider usedSlider)
    {
        // Somewhat hardcoded, but best i could do way of setting volume to SettingsCarrier from the sliders
        switch (usedSlider.name)
        {
            case "MasterVolume": carrier.masterVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("MasterVolume", carrier.masterVolume); break;
            case "MenuSFXVolume": carrier.menuVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("MenuVolume", carrier.menuVolume); break;
            case "MusicVolume": carrier.musicVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("MusicVolume", carrier.musicVolume); break;
            case "GameSFXVolume": carrier.soundVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("SoundVolume", carrier.soundVolume); break;
        }

        mainMenuController.SetAudioVolumeLevels();
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
            fpsButtons[0].isOn = true;
        else if (Application.targetFrameRate == 60)
            fpsButtons[1].isOn = true;
        else if (Application.targetFrameRate == 30)
            fpsButtons[2].isOn = true;
    }

    private float RoundToTwoDecimals(float toRound)
    { // Rounding the volume to two decimals so that we dont get extremely specific volumes. aka: 57.2124865223% volume
        float multipliedVal = Mathf.Round(toRound * 100);
        return multipliedVal / 100;
    }

    public void SetIntroSkipToggle()
    {
        _introSkipToggle.isOn = (PlayerPrefs.GetInt("skipIntroVideo", 0) != 0);
    }

    public void SetIntroSkip()
    {
        if(_introSkipToggle.isOn) PlayerPrefs.SetInt("skipIntroVideo", 1);
        else PlayerPrefs.SetInt("skipIntroVideo", 0);
    }

    public void SetShowButtonLabelsToggle()
    {
        _showButtonLabelsToggle.isOn = carrier.ShowButtonLabels;
    }

    public void SetShowButtonLabels()
    {
        PlayerPrefs.SetInt("showButtonLabels", _showButtonLabelsToggle.isOn ? 1 : 0);
        carrier.ShowButtonLabels = _showButtonLabelsToggle.isOn;
    }
}
