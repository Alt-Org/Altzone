using UnityEngine;
using UnityEngine.UI;
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

        }
        public void SetMasterLevel(float sliderValue)
        {

        }
        public void SetMenuSFXLevel(float sliderValue)
        {

        }
        public void SetMusicLevel(float sliderValue)
        {

        }
        public void SetGameSFXLevel(float sliderValue)
        {

        }
    }

}
