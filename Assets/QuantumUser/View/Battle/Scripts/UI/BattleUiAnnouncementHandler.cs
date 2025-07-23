/// @file BattleUiAnnouncementHandler.cs
/// <summary>
/// Has a class BattleUiAnnouncementHandler which handles showing the announcement/countdown text.
/// </summary>
///
/// This script:<br/>
/// Handles showing the announcement/countdown text.

using UnityEngine;

using TMPro;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">Announcement @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles showing the announcement/countdown text.
    /// </summary>
    public class BattleUiAnnouncementHandler : MonoBehaviour
    {
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Reference to the GameObject which can be used to hide or show the announcement.</value>
        [SerializeField] private GameObject _view;

        /// <value>[SerializeField] Reference to the <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TextMeshProUGUI.html">TextMeshProUGUI@u-exlink</a> component which the announcement text is set to.</value>
        [SerializeField] private TextMeshProUGUI _announcerText;
        [SerializeField] private TextScaler _announcementTextScaler;

        /// @}


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
        /// <param name="show">If the %UI element should be visible or not.</param>
        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

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

        //countdown from x to 0 based on GameSessionState

        /// <summary>
        /// Sets a countdown number to the #_announcerText.
        /// </summary>
        /// <param name="countDown">The number which to set to the countdown.</param>
        public void SetCountDownNumber(int countDown)
        {
            if (_debugmode) return;
            _announcerText.text = $"{countDown}";
        }

        public void ClearAnnouncerTextField()
        {
            if (_debugmode) return;
            _announcerText.text = "";
        }

        public void SetDebugtext(string text)
        {
            _announcerText.text = text;
            _announcerText.color = Color.red;
            _announcerText.textWrappingMode = TextWrappingModes.Normal;
            _announcementTextScaler.RelativePercentageMax *= 0.5f;
            _debugmode = true;
        }

        private bool _debugmode = false;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method which clears the text in #_announcerText.
        /// </summary>
        private void Awake()
        {
            ClearAnnouncerTextField();
        }
    }
}
