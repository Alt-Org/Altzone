using Altzone.Scripts;
using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Service.Audio;

namespace MenuUi.Scripts.Settings
{
    public class AudioMenuSettings : MonoBehaviour
    {
        private const float DefaultVolume = 1.0f;

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
            _audioManager = AudioManager.Get();
            SetSlider(_master, PlayerPrefKeys.MasterVolume);
            SetSlider(_menuSFX, PlayerPrefKeys.MenuSfxVolume);
            SetSlider(_gameMusic, PlayerPrefKeys.GameMusicVolume);
            SetSlider(_gameSFX, PlayerPrefKeys.GameSfxVolume);

            void SetSlider(Slider slider, string playerPrefKeyName)
            {
                slider.minValue = 0;
                slider.maxValue = 1f;
                slider.value = PlayerPrefs.GetFloat(playerPrefKeyName, DefaultVolume);
            }
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
            PlayerPrefs.SetFloat(PlayerPrefKeys.MasterVolume, DefaultVolume);
            PlayerPrefs.SetFloat(PlayerPrefKeys.MenuSfxVolume, DefaultVolume);
            PlayerPrefs.SetFloat(PlayerPrefKeys.GameMusicVolume, DefaultVolume);
            PlayerPrefs.SetFloat(PlayerPrefKeys.GameSfxVolume, DefaultVolume);
        }
    }
}