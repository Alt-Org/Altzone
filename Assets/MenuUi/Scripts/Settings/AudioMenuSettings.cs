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
            if (!PlayerPrefs.HasKey("PlayerMasterVolume"))
            {
                MakeVolumePrefs();
            }
            _master.value = PlayerPrefs.GetFloat("PlayerMasterVolume");
            _menuSFX.value = PlayerPrefs.GetFloat("PlayerMenuSFXVolume");
            _gameMusic.value = PlayerPrefs.GetFloat("PlayerGameMusicVolume");
            _gameSFX.value = PlayerPrefs.GetFloat("PlayerGameSFXVolume");
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
            PlayerPrefs.SetFloat("PlayerMasterVolume", sliderValue);
            // Example if we used Unity's Audio system
            /*
            AudioMixer.SetFloat("PlayerMasterVolume", Mathf.Log10(sliderValue) * 20)
            */
            _audioManager.MasterVolume = sliderValue;
        }
        public void SetMenuSFXLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat("PlayerMenuSFXVolume", sliderValue);
            _audioManager.MenuEffectsVolume = sliderValue;
        }
        public void SetMusicLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat("PlayerGameMusicVolume", sliderValue);
            _audioManager.GameMusicVolume = sliderValue;
        }
        public void SetGameSFXLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat("PlayerGameSFXVolume", sliderValue);
            _audioManager.GameEffectsVolume = sliderValue;
        }

        private void MakeVolumePrefs()
        {
            float defaultVolume = 1.0f;
            PlayerPrefs.SetFloat("PlayerMasterVolume", defaultVolume);
            PlayerPrefs.SetFloat("PlayerMenuSFXVolume", defaultVolume);
            PlayerPrefs.SetFloat("PlayerGameMusicVolume", defaultVolume);
            PlayerPrefs.SetFloat("PlayerGameSFXVolume", defaultVolume);
        }

    }

}
