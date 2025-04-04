using System.Collections;
using Altzone.Scripts.BattleUi;
using Photon.Deterministic;
using Quantum;
using TMPro;
using UnityEngine;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles setting timer text since match started.
    /// </summary>
    public class GameUiTimerHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;
        private bool _recordTime = false;
        private FrameTimer _timer;
        private int _hours;
        private int _oldSeconds;

        public BattleUiElement MovableUiElement;

        private void OnDisable()
        {
            StopTimer();
        }

        private void Update()
        {
            if (!_recordTime) return;
            if (!Utils.TryGetQuantumFrame(out Frame f)) return;

            if (_timer.IsExpired(f))
            {
                _oldSeconds = -1;
                _timer.Restart(f);
                _hours++;
            }

            FP? secondsSinceStart = _timer.TimeInSecondsSinceStart(f);

            if (secondsSinceStart != null)
            {
                int minutes = FPMath.FloorToInt(secondsSinceStart.Value / 60);
                int seconds = FPMath.FloorToInt(secondsSinceStart.Value - minutes * 60);

                if (seconds > _oldSeconds)
                {
                    _timerText.text = _hours == 0 ? $"{minutes}:{seconds:00}" : $"{_hours}:{minutes:00}:{seconds:00}";
                    _oldSeconds = seconds;
                }
            }
        }

        public void StartTimer(Frame f)
        {
            if(_recordTime) return;

            _hours = 0;
            _oldSeconds = -1;

            _timer = FrameTimer.FromSeconds(f, 3600);
            _recordTime = true;
            _timerText.gameObject.SetActive(true);
        }

        public void StopTimer()
        {
            _recordTime = false;
            _timerText.gameObject.SetActive(false);
        }
    }
}

