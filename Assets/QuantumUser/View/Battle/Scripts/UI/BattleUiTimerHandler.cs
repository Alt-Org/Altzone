using UnityEngine;

using Quantum;
using Photon.Deterministic;

using TMPro;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles setting timer text since match started.
    /// </summary>
    public class BattleUiTimerHandler : MonoBehaviour
    {
        [SerializeField] private BattleUiMovableElement _movableUiElement;
        [SerializeField] private TMP_Text _timerText;

        public bool IsVisible => MovableUiElement.gameObject.activeSelf;
        public BattleUiMovableElement MovableUiElement => _movableUiElement;

        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }

        public void StartTimer(Frame f)
        {
            if (_recordTime) return;

            _hours = 0;
            _oldSeconds = -1;

            _timer = FrameTimer.FromSeconds(f, 3600);
            _recordTime = true;
        }

        public void StopTimer()
        {
            _recordTime = false;
        }

        private bool _recordTime = false;
        private FrameTimer _timer;
        private int _hours;
        private int _oldSeconds;

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

                if (Mathf.Abs(seconds - _oldSeconds) >= 1)
                {
                    if (IsVisible) _timerText.text = _hours == 0 ? $"{minutes}:{seconds:00}" : $"{_hours}:{minutes:00}:{seconds:00}";
                    _oldSeconds = seconds;
                }
            }
        }
    }
}
