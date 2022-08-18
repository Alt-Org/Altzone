using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Service.Audio
{
    public class PlayMenuEffect : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private bool _register;
        [SerializeField] private AudioClip _menuEffect;

        private string _audioClipName;
        
        private void Awake()
        {
            var audioManager = AudioManager.Get();
            if (_register)
            {
                _audioClipName = $"menu.{_menuEffect.name}";
                audioManager.RegisterAudioClip(_audioClipName, _menuEffect);
            }
            _button.onClick.AddListener(() =>
            {
                if (_audioClipName != null)
                {
                    audioManager.PlayMenuEffect(_audioClipName);
                }
                else
                {
                    audioManager.PlayMenuEffect(_menuEffect);
                }
            });
        }
    }
}