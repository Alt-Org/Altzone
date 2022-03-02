using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Helper class to manage countdown counter using <c>Text Mesh Pro</c>.
    /// </summary>
    internal class CountdownListener : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private TMP_Text _countdownTextCanvas;

        [SerializeField] private TextMeshPro _countdownText;

        private bool _isCanvas;
        private GameObject _countdown;

        private void Awake()
        {
            _isCanvas = _countdownTextCanvas != null;
            _countdown = _isCanvas
                ? _countdownTextCanvas.GetComponentInParent<Canvas>().gameObject
                : _countdownText.gameObject;
            HideCountdown();
            this.Subscribe<PlayerManager.CountdownEvent>(OnCountdownEvent);
        }

        private void OnDestroy()
        {
            this.Unsubscribe();
        }

        private void OnCountdownEvent(PlayerManager.CountdownEvent data)
        {
            if (data.CurValue == data.MaxValue)
            {
                StartCountdown(data.CurValue);
                return;
            }
            if (data.CurValue >= 0)
            {
                SetCountdownValue(data.CurValue);
                return;
            }
            HideCountdown();
            this.Unsubscribe();
        }

        private void StartCountdown(int value)
        {
            _countdown.SetActive(true);
            SetCountdownValue(value);
        }

        private void SetCountdownValue(int value)
        {
            if (_isCanvas)
            {
                _countdownTextCanvas.text = value.ToString("N0");
            }
            else
            {
                _countdownText.text = value.ToString("N0");
            }
        }

        private void HideCountdown()
        {
            _countdown.SetActive(false);
        }
    }
}