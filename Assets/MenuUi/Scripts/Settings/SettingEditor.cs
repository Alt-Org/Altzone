using System;
using System.Linq;
using MenuUi.Scripts.MainMenu;
using MenuUi.Scripts.Settings.BattleUiEditor;
using Altzone.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Audio;
using Altzone.Scripts.Language;

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
    [SerializeField] private GameObject[] _settingsPopups;
    [SerializeField] private Button _topBarStyleButton;
    [SerializeField] private TextLanguageSelectorCaller _topBarStyleText;

    [SerializeField] private Image _languageImage;
    [SerializeField] private Sprite _finnishSprite;
    [SerializeField] private Sprite _englishSprite;
    [SerializeField] private TextLanguageSelectorCaller _languageCaller;

    private void OnEnable()
    {
        if(mainMenuController == null)
            mainMenuController = FindObjectOfType<MainMenuController>(true);

        foreach (Slider slider in volumeSliders)
        {
            SetToSlider(slider);
        }

        //SetFPSButtons();
        SetIntroSkipToggle();
        
        
        SetShowButtonLabelsToggle();

        // Opening Battle Ui Editor if DataCarrier has a bool BattleUiEditorRequested and it's true
        if (DataCarrier.GetData<bool>(DataCarrier.BattleUiEditorRequested, suppressWarning: true))
        {
            DataCarrier.AddData(DataCarrier.RequestedWindow, 1);
            _battleEditor.OpenEditor();
        }

        foreach(GameObject popup in _settingsPopups)
        {
            popup.SetActive(false);
        }

        SettingsCarrier.OnLanguageChanged += ChangeLanguage;
        ChangeLanguage(SettingsCarrier.Instance.Language);
    }

    private void Start()
    {
        _battleSettingsButton.onClick.AddListener(() => _battleEditor.OpenEditor());

        PlayerPrefs.SetFloat("MasterVolume", 1f);

        _topBarStyleButton.onClick.AddListener(() => ChangeTopbarStyle());
        _topBarStyleText.SetText(SettingsCarrier.Instance.Language, new string[1] { carrier.TopBarStyleSetting.ToString() });

        _introSkipToggle.onValueChanged.AddListener(_ => SetIntroSkip());
    }

    private void OnDisable()
    {
        SettingsCarrier.OnLanguageChanged -= ChangeLanguage;
    }

    public void SetFromSlider(Slider usedSlider)
    {
        // Somewhat hardcoded, but best i could do way of setting volume to SettingsCarrier from the sliders
        switch (usedSlider.name)
        {
            case "MasterVolume": carrier.masterVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("MasterVolume", carrier.masterVolume); break;
            case "MenuSFXVolume": carrier.menuVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("MenuVolume", carrier.menuVolume); break;
            case "MusicVolume": carrier.musicVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("MusicVolume", carrier.musicVolume); break;
            case "GameSFXVolume":
                carrier.soundVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("SoundVolume", carrier.soundVolume);
                carrier.menuVolume = RoundToTwoDecimals(usedSlider.value); PlayerPrefs.SetFloat("MenuVolume", carrier.menuVolume);
                break;
        }

        AudioManager.Instance.UpdateMaxVolume();
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
        if (Application.targetFrameRate == (int)Screen.currentResolution.refreshRateRatio.value)
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
        _introSkipToggle.isOn = (PlayerPrefs.GetInt("SkipIntroVideo", 0) != 0);
    }

    public void SetIntroSkip()
    {
        if(_introSkipToggle.isOn) PlayerPrefs.SetInt("SkipIntroVideo", 1);
        else PlayerPrefs.SetInt("SkipIntroVideo", 0);
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

    public void ChangeTopbarStyle()
    {
        if (carrier.TopBarStyleSetting == ((SettingsCarrier.TopBarStyle[])Enum.GetValues(typeof(SettingsCarrier.TopBarStyle))).ToList().Last())
        {
            carrier.TopBarStyleSetting = 0;
        }
        else carrier.TopBarStyleSetting++;

        _topBarStyleText.SetText(SettingsCarrier.Instance.Language, new string[1] { carrier.TopBarStyleSetting.ToString() });

    }

    public void PlayMainMenuMusic() { AudioManager.Instance.PlayMusic("MainMenu"); }

    private void ChangeLanguage(SettingsCarrier.LanguageType language)
    {
        switch (language)
        {
            case SettingsCarrier.LanguageType.Finnish:
                _languageImage.sprite = _finnishSprite;
                break;
            case SettingsCarrier.LanguageType.English:
                _languageImage.sprite = _englishSprite;
                break;
        }
        _languageCaller.SetText(language, new string[0]);
        _topBarStyleText.SetText(SettingsCarrier.Instance.Language, new string[1] { carrier.TopBarStyleSetting.ToString() });
    }
}
