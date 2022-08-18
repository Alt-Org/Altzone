using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Service.Audio
{
    public class PlayGameEffect : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private AudioClip _gameEffect;

        private void Awake()
        {
            var audioManager = AudioManager.Get();
            _button.onClick.AddListener(() => { audioManager.PlayGameEffect(_gameEffect); });
        }
    }
}