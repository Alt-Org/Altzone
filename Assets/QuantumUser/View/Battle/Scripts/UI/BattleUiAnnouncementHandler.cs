/// @file BattleUiAnnouncementHandler.cs
/// <summary>
/// Has a class BattleUiAnnouncementHandler which handles showing the announcement/countdown text.
/// </summary>
///
/// This script:<br/>
/// Handles showing the announcement/countdown text. Part of @uihandlerslink.

using UnityEngine;

using TMPro;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles showing the announcement/countdown text. Part of @uihandlerslink.
    /// </summary>
    public class BattleUiAnnouncementHandler : MonoBehaviour
    {
        /// <value>[SerializeField] Reference to the GameObject which can be used to hide or show the announcement.</value>
        [SerializeField] private GameObject _view;

        /// <value>[SerializeField] Reference to the TextMeshProUGUI component which the announcement text is set to.</value>
        [SerializeField] private TextMeshProUGUI _announcerText;

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

        /// <summary>
        /// Sets a countdown number to the #_announcerText.
        /// </summary>
        /// <param name="countDown">The number which to set to the countdown.</param>
        public void SetCountDownNumber(int countDown)
        {
            _announcerText.text = $"{countDown}";
        }

        /// <summary>
        /// Sets the end of the countdown text to the #_announcerText.
        /// </summary>
        public void ShowEndOfCountDownText()
        {
            _announcerText.text = "GO!";
        }

        /// <summary>
        /// Sets the game over text to the #_announcerText.
        /// </summary>
        public void ShowGameOverText()
        {
            _announcerText.text = "Game Over!";
        }

        /// <summary>
        /// Clears the text in #_announcerText.
        /// </summary>
        public void ClearAnnouncerTextField()
        {
            _announcerText.text = "";
        }

        /// <summary>
        /// Private Awake method which clears the text in #_announcerText.
        /// </summary>
        private void Awake()
        {
            _announcerText.text = "";
        }
    }
}
