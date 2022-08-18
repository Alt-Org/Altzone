using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Service.Audio
{
    public class PlayGameMusic : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private AudioClip _gameMusic;

        private string _audioClipName;

        private void Awake()
        {
            var audioManager = AudioManager.Get();
            _audioClipName = $"menu.{_gameMusic.name}";
            audioManager.RegisterAudioClip(_audioClipName, _gameMusic);
            _button.onClick.AddListener(() => { audioManager.PlayGameMusic(_audioClipName); });
        }
    }
}