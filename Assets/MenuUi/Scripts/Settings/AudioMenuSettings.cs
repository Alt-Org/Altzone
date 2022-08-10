using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Altzone.Scripts.Service.Audio;

namespace MenuUi.Scripts.Settings
{
    public class AudioMenuSettings : MonoBehaviour
    {
        [Header("VolumeSliders"), SerializeField] private Slider _master;
        [SerializeField] private Slider _menuSFX;
        [SerializeField] private Slider _gameMusic;
        [SerializeField] private Slider _gameSFX;

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
        public void SetMasterLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat("PlayerMasterVolume", sliderValue);
        }
        public void SetMenuSFXLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat("PlayerMenuSFXVolume", sliderValue);
        }
        public void SetMusicLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat("PlayerGameMusicVolume", sliderValue);
        }
        public void SetGameSFXLevel(float sliderValue)
        {
            PlayerPrefs.SetFloat("PlayerGameSFXVolume", sliderValue);
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
