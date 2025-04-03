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

        public BattleUiElement MovableUiElement;

        private void OnDisable()
        {
            StopTimer();
        }

        private IEnumerator UpdateText()
        {
            bool timerStartFrameFound;
            Frame startFrame;
            do
            {
                timerStartFrameFound = Utils.TryGetQuantumFrame(out startFrame);
            } while (!timerStartFrameFound);

            int hours = 0;
            int oldSeconds = 0;
            FrameTimer timer = FrameTimer.FromSeconds(startFrame, 3600);
            while (_recordTime)
            {
                if (Utils.TryGetQuantumFrame(out Frame currentFrame))
                {
                    FP? secondsSinceStart = timer.TimeInSecondsSinceStart(currentFrame);

                    if (secondsSinceStart != null)
                    {
                        int minutes = FPMath.FloorToInt(secondsSinceStart.Value / 60);
                        int seconds = FPMath.FloorToInt(secondsSinceStart.Value - minutes * 60);

                        if (seconds > oldSeconds)
                        {
                            _timerText.text = hours == 0 ? $"{minutes}:{seconds:00}" : $"{hours}:{minutes:00}:{seconds:00}";
                            oldSeconds = seconds;
                        }
                    }
                    else
                    {
                        timer.Restart(currentFrame);
                        hours++;
                    }
                }
                yield return null;
            }
        }

        public void StartTimer()
        {
            _recordTime = true;
            StartCoroutine(UpdateText());
            _timerText.gameObject.SetActive(true);
        }

        public void StopTimer()
        {
            _recordTime = false;
        }
    }
}

