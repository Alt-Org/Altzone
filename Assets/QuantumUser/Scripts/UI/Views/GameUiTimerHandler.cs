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

        private IEnumerator UpdateText()
        {
            bool timerStartFrameFound = false;
            Frame startFrame = null;
            do
            {
                timerStartFrameFound = Utils.TryGetQuantumFrame(out startFrame);
            } while (!timerStartFrameFound);

            FrameTimer timer = FrameTimer.FromSeconds(startFrame, 1);
            int timeSeconds = 0;
            while (_recordTime)
            {
                int minutes = Mathf.FloorToInt(timeSeconds / 60.0f);
                _timerText.text = $"{minutes}:{timeSeconds - minutes * 60}";

                if (Utils.TryGetQuantumFrame(out Frame currentFrame))
                {
                    yield return new WaitUntil(() => timer.IsExpired(currentFrame));
                    timer.Restart(currentFrame);
                    timeSeconds += 1;
                }
                else
                {
                    yield return null;
                }
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

