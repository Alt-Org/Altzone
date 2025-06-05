/// @file BattleUiTimerHandler.cs
/// <summary>
/// Has a class BattleUiTimerHandler which handles setting the timer text.
/// </summary>
///
/// This script:<br/>
/// Handles setting the timer text since match started.

using UnityEngine;

using Quantum;
using Photon.Deterministic;

using TMPro;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">Timer @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles setting the timer text since match started.
    /// </summary>
    public class BattleUiTimerHandler : MonoBehaviour
    {
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Reference to the BattleUiMovableElement script which is attached to a BattleUiTimer prefab.</value>
        [SerializeField] private BattleUiMovableElement _movableUiElement;

        /// <value>[SerializeField] Reference to the <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html">TMP_Text@u-exlink</a> component which the timer text is set to.</value>
        [SerializeField] private TMP_Text _timerText;

        /// @}

        /// <value>Is the %UI element visible or not.</value>
        public bool IsVisible => MovableUiElement.gameObject.activeSelf;

        /// <value>Public getter for #_movableUiElement.</value>
        public BattleUiMovableElement MovableUiElement => _movableUiElement;

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        /// <param name="show">If the %UI element should be visible or not.</param>
        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }

        /// <summary>
        /// Start the timer from the given <a href="https://doc.photonengine.com/quantum/current/manual/frames">Frame@u-exlink</a>.
        /// </summary>
        /// <param name="f">The <a href="https://doc.photonengine.com/quantum/current/manual/frames">Frame@u-exlink</a> which the timer will be started from.</param>
        public void StartTimer(Frame f)
        {
            if (_recordTime) return;

            _hours = 0;
            _secondsElapsedPrevious = -1;

            _timer = FrameTimer.FromSeconds(f, 3600);
            _recordTime = true;
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            _recordTime = false;
        }

        /// <value>Bool which toggles recording time in the #Update method.</value>
        private bool _recordTime = false;
        /// <value><a href="https://doc.photonengine.com/quantum/current/concepts-and-patterns/frame-timer">FrameTimer@u-exlink</a> which is used for getting the game time in seconds since the game started.</value>
        private FrameTimer _timer;
        /// <value>Keeps track of hours passed.</value>
        private int _hours;

        /// <value>Holder variable for keeping track of passed seconds, so that the #_timerText is set in #Update only when the seconds increase.</value>
        private int _secondsElapsedPrevious;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method. Handles formatting and setting the #_timerText.
        /// </summary>
        private void Update()
        {
            if (!_recordTime) return;
            if (!Utils.TryGetQuantumFrame(out Frame f)) return;

            if (_timer.IsExpired(f))
            {
                _secondsElapsedPrevious = -1;
                _timer.Restart(f);
                _hours++;
            }

            FP? secondsElapsedFloat = _timer.TimeInSecondsSinceStart(f);

            if (secondsElapsedFloat != null)
            {
                int secondsElapsed = FPMath.FloorToInt(secondsElapsedFloat.Value);
                int minutes = FPMath.FloorToInt(secondsElapsedFloat.Value / 60);
                int seconds = secondsElapsed - (minutes * 60);

                if (secondsElapsed > _secondsElapsedPrevious)
                {
                    if (IsVisible) _timerText.text = _hours == 0 ? $"<mspace=1em>{minutes:D2}:{seconds:00}</mspace>" : $"{_hours}:{minutes:00}:{seconds:00}";
                    _secondsElapsedPrevious = secondsElapsed;
                }
            }
        }
    }
}
