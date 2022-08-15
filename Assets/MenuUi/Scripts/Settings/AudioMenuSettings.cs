using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Altzone.Scripts.Service.Audio;

namespace MenuUi.Scripts.Settings
{
    public class AudioMenuSettings : MonoBehaviour
    {
        // Slider Min Value must be 0.0001 and Max Value must be 1 for the Log10(x) * 20 method to work properly
        [Header("VolumeSliders"), SerializeField] private Slider _master;
        [SerializeField] private Slider _menuSFX;
        [SerializeField] private Slider _gameMusic;
        [SerializeField] private Slider _gameSFX;

        private IAudioManager _audioManager;
        
        void Awake()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefKeys.MasterVolume))
            {
                MakeVolumePrefs();
            }
            _master.value = PlayerPrefs.GetFloat(PlayerPrefKeys.MasterVolume);
            _menuSFX.value = PlayerPrefs.GetFloat(PlayerPrefKeys.MenuSfxVolume);
            _gameMusic.value = PlayerPrefs.GetFloat(PlayerPrefKeys.GameMusicVolume);
            _gameSFX.value = PlayerPrefs.GetFloat(PlayerPrefKeys.GameSfxVolume);
        }

        private void Start()
        {
            _audioManager = AudioManager.Get();
            _audioManager.MasterVolume = _master.value;
            _audioManager.MenuEffectsVolume = _menuSFX.value;
            _audioManager.GameMusicVolume = _gameMusic.value;
            _audioManager.GameEffectsVolume = _gameSFX.value;
        }

        public void SetMasterLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat(PlayerPrefKeys.MasterVolume, sliderValue);
            // Example if we used Unity's Audio system
            /*
            AudioMixer.SetFloat("PlayerMasterVolume", Mathf.Log10(sliderValue) * 20)
            */
            _audioManager.MasterVolume = sliderValue;
        }
        public void SetMenuSFXLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat(PlayerPrefKeys.MenuSfxVolume, sliderValue);
            _audioManager.MenuEffectsVolume = sliderValue;
        }
        public void SetMusicLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat(PlayerPrefKeys.GameMusicVolume, sliderValue);
            _audioManager.GameMusicVolume = sliderValue;
        }
        public void SetGameSFXLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat(PlayerPrefKeys.GameSfxVolume, sliderValue);
            _audioManager.GameEffectsVolume = sliderValue;
        }

        private void MakeVolumePrefs()
        {
            float defaultVolume = 1.0f;
            PlayerPrefs.SetFloat(PlayerPrefKeys.MasterVolume, defaultVolume);
            PlayerPrefs.SetFloat(PlayerPrefKeys.MenuSfxVolume, defaultVolume);
            PlayerPrefs.SetFloat(PlayerPrefKeys.GameMusicVolume, defaultVolume);
            PlayerPrefs.SetFloat(PlayerPrefKeys.GameSfxVolume, defaultVolume);
        }

    }

}
