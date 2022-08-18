using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Service.Audio
{
    public class PlayGameMusic : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private AudioClip _gameMusic;

        private void Awake()
        {
            var audioManager = AudioManager.Get();
            _button.onClick.AddListener(() => { audioManager.PlayGameMusic(_gameMusic); });
        }
    }
}