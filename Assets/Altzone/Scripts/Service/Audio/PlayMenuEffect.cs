using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Service.Audio
{
    public class PlayMenuEffect : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private AudioClip _menuEffect;

        private void Awake()
        {
            var audioManager = AudioManager.Get();
            _button.onClick.AddListener(() => { audioManager.PlayMenuEffect(_menuEffect); });
        }
    }
}