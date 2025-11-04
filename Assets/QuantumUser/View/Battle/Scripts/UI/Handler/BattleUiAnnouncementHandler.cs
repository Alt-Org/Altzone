/// @file BattleUiAnnouncementHandler.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiAnnouncementHandler} class which handles showing the announcement/countdown text.
/// </summary>
///
/// This script:<br/>
/// Handles showing the announcement/countdown text.

// Unity usings
using UnityEngine;
using TMPro;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">Announcement @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles showing the announcement/countdown text.
    /// </summary>
    public class BattleUiAnnouncementHandler : MonoBehaviour
    {
        /// @anchor BattleUiAnnouncementHandler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to the GameObject which can be used to hide or show the announcement.</summary>
        /// @ref BattleUiAnnouncementHandler-SerializeFields
        [SerializeField] private GameObject _view;

        /// <summary>[SerializeField] Reference to the <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TextMeshProUGUI.html">TextMeshProUGUI@u-exlink</a> component which the announcement text is set to.</summary>
        /// @ref BattleUiAnnouncementHandler-SerializeFields
        [SerializeField] private TextMeshProUGUI _announcerText;

        /// @}

        /// <summary>Different types of announcement text which can be displayed.</summary>
        public enum TextType
        {
            Loading,
            WaitingForPlayers,
            EndOfCountdown,
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
        /// Sets the announcement text to #_announcerText based on given TextType.
        /// </summary>
        ///
        /// <param name="textType">The TextType which to set to the announcement.</param>
        public void SetText(TextType textType)
        {
            if (_debugmode) return;
            _announcerText.text = textType switch
            {
                TextType.Loading            => "Loading...",
                TextType.WaitingForPlayers  => "Waiting for\nplayers to connect...",
                TextType.EndOfCountdown     => "GO!",

                _ => string.Format("Unimplemented text type {0}.", textType),
            };
        }

        /// <summary>
        /// Sets a countdown number to the #_announcerText.
        /// </summary>
        ///
        /// <param name="countDown">The number which to set to the countdown.</param>
        public void SetCountDownNumber(int countDown)
        {
            if (_debugmode) return;
            _announcerText.text = $"{countDown}";
        }

        /// <summary>
        /// Clears #_announcerText text.
        /// </summary>
        public void ClearAnnouncerTextField()
        {
            if (_debugmode) return;
            _announcerText.text = "";
        }

        /// <summary>
        /// Sets a custom debugging text to #_announcerText.
        /// </summary>
        ///
        /// <param name="text">The text string which to display.</param>
        public void SetDebugtext(string text)
        {
            _announcerText.text = text;
            _announcerText.color = Color.red;
            _announcerText.textWrappingMode = TextWrappingModes.Normal;
            _announcerText.fontSize *= 0.5f;
            _debugmode = true;
        }

        /// <value>Indicates whether the announcement text is in debug mode or not.</value>
        private bool _debugmode = false;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method which clears the text in #_announcerText.
        /// </summary>
        private void Awake()
        {
            ClearAnnouncerTextField();
        }
    }
}
