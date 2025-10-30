/// @file BattleUiDebugStatsOverlayHandler.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiDebugStatsOverlayHandler} class which displays character's stats.
/// </summary>
///
/// This script:<br/>
/// Handles displaying the player's character's stats for debug purposes.

// System usings
using System;

// Unity usings
using UnityEngine;
using TMPro;

// Quantum usings
using Quantum;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">DebugStatsOverlay @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles displaying the player's character's stats for debug purposes.
    /// </summary>
    public class BattleUiDebugStatsOverlayHandler : MonoBehaviour
    {
        /// @anchor BattleUiDebugStatsOverlayHandler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to the GameObject which can be used to hide or show the overlay.</summary>
        /// @ref BattleUiDebugStatsOverlayHandler-SerializeFields
        [SerializeField] private GameObject _view;

        /// <summary>[SerializeField] StatText struct for the ImpactForce stat.</summary>
        /// @ref BattleUiDebugStatsOverlayHandler-SerializeFields
        [SerializeField] private StatText _impactForce;

        /// <summary>[SerializeField] StatText struct for the Hp stat.</summary>
        /// @ref BattleUiDebugStatsOverlayHandler-SerializeFields
        [SerializeField] private StatText _hp;

        /// <summary>[SerializeField] StatText struct for the Speed stat.</summary>
        /// @ref BattleUiDebugStatsOverlayHandler-SerializeFields
        [SerializeField] private StatText _speed;

        /// <summary>[SerializeField] StatText struct for the CharacterSize stat.</summary>
        /// @ref BattleUiDebugStatsOverlayHandler-SerializeFields
        [SerializeField] private StatText _charSize;

        /// <summary>[SerializeField] StatText struct for the Defence stat.</summary>
        /// @ref BattleUiDebugStatsOverlayHandler-SerializeFields
        [SerializeField] private StatText _defence;

        /// <summary>[SerializeField] Reference to the <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html">TMP_Text@u-exlink</a> component of the debug stat text which's text is longest to get its font size.</summary>
        /// @ref BattleUiDebugStatsOverlayHandler-SerializeFields
        [SerializeField] private TMP_Text _referenceText;

        /// @}

        /// <summary>
        /// Serializable StatText struct. Part of BattleUiDebugStatsOverlayHandler.<br/>
        /// Holds references to #Name and #Value <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html">TMP_Text@u-exlink</a> components which display the stat's name and value.
        /// </summary>
        [Serializable]
        public struct StatText
        {
            /// <value>Reference to the <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html">TMP_Text@u-exlink</a> component which displays the stat's name.</value>
            public TMP_Text Name;

            /// <value>Reference to the <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html">TMP_Text@u-exlink</a> component which displays the stat's value.</value>
            public TMP_Text Value;

            /// <summary>
            /// Set's the font size to both #Name and #Value <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html">TMP_Text@u-exlink</a> components.
            /// </summary>
            ///
            /// <param name="size"></param>
            public void SetSize(float size)
            {
                Name.fontSize = size;
                Value.fontSize = size;
            }
        }

        /// <value>Is the %UI element visible or not.</value>
        public bool IsVisible => _view.activeSelf;

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        ///
        /// <param name="show">If the %UI element should be visible or not.</param>
        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        /// <summary>
        /// Sets the stat values to the debug stat texts.
        /// </summary>
        ///
        /// <param name="stats">The BattlePlayerStats which to get the stats from.</param>
        public void SetStats(BattlePlayerStats stats)
        {
            _impactForce.Value.text = stats.Attack.ToString();
            _hp.Value.text          = stats.Hp.ToString();
            _speed.Value.text       = stats.Speed.ToString();
            _charSize.Value.text    = stats.CharacterSize.ToString();
            _defence.Value.text     = stats.Defence.ToString();
        }

        /// <value>Keeps track of the current font size set to the debug stat texts.</value>
        private float _currentFontSize;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method. Handles resizing the font sizes for the debug stat texts.
        /// </summary>
        private void Update()
        {
            if (_view.activeSelf && _referenceText.fontSize != _currentFontSize) ResizeFontSizes();
        }

        /// <summary>
        /// Private method to resize the debug stat texts font sizes.
        /// </summary>
        private void ResizeFontSizes()
        {
            _currentFontSize = _referenceText.fontSize;

            _impactForce .SetSize(_currentFontSize);
            _hp          .SetSize(_currentFontSize);
            _speed       .SetSize(_currentFontSize);
            _charSize    .SetSize(_currentFontSize);
            _defence     .SetSize(_currentFontSize);
        }
    }
}
