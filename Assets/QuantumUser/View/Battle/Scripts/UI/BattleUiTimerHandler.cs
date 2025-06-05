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

        public void FormatAndSetTimerText(FP gameTimeSec)
        {
            int secondsElapsed = FPMath.FloorToInt(gameTimeSec);
            int hours = Mathf.FloorToInt(secondsElapsed / (float)TimeConversionRatio / TimeConversionRatio);
            int minutes = Mathf.FloorToInt(secondsElapsed / (float)TimeConversionRatio) - hours * TimeConversionRatio;
            int seconds = secondsElapsed - (minutes * TimeConversionRatio);

            if (secondsElapsed > _secondsElapsedPrevious)
            {
                if (IsVisible) _timerText.text = hours == 0 ? $"<mspace=1em>{minutes:D2}:{seconds:00}</mspace>" : $"{hours}:{minutes:00}:{seconds:00}";
                _secondsElapsedPrevious = secondsElapsed;
            }
        }

        private const int TimeConversionRatio = 60;
        private int _secondsElapsedPrevious;
    }
}
