/// @file BattleUiTimerHandler.cs
/// <summary>
/// Has a class BattleUiTimerHandler which handles setting the timer text.
/// </summary>
///
/// This script:<br/>
/// Handles setting the timer text.

using UnityEngine;

using Quantum;
using Photon.Deterministic;

using TMPro;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">Timer @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles setting the timer text.
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
        /// 
        /// <param name="show">If the %UI element should be visible or not.</param>
        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }

        /// <summary>
        /// Format and set the timer text from the given game time as seconds.
        /// </summary>
        /// 
        /// <param name="gameTimeSec">The game time as seconds which will be formatted and set to the timer text.</param>
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

        /// <value>Constant conversion ratio used in time calculations.</value>
        private const int TimeConversionRatio = 60;

        /// <value>The seconds elapsed the last time #FormatAndSetTimerText method was called.</value>
        private int _secondsElapsedPrevious;
    }
}
