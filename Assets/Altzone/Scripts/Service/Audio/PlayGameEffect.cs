using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Service.Audio
{
    [RequireComponent(typeof(Button))]
    public class PlayGameEffect : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private bool _register;
        [SerializeField] private AudioClip _audioClip;

        private string _audioClipName;

        private void Awake()
        {
            if (_audioClip == null)
            {
                return;
            }
            var audioManager = AudioManager.Get();
            if (_register)
            {
                _audioClipName = $"game.{_audioClip.name}";
                audioManager.RegisterAudioClip(_audioClipName, _audioClip);
            }
            _button.onClick.AddListener(() =>
            {
                if (_audioClipName != null)
                {
                    audioManager.PlayGameEffect(_audioClipName);
                }
                else
                {
                    audioManager.PlayGameEffect(_audioClip);
                }
            });
        }
    }
}