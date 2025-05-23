/// @file BattleUiGameOverHandler.cs
/// <summary>
/// Has a class BattleUiGameOverHandler which handles showing the game over popup.
/// </summary>
///
/// This script:<br/>
/// Handles showing the game over popup. Part of @ref UIHandlers.

using UnityEngine;
using UnityEngine.UI;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles showing the game over popup. Part of @ref UIHandlers.
    /// </summary>
    public class BattleUiGameOverHandler : MonoBehaviour
    {
        /// <value>[SerializeField] Reference to BattleUiController.</value>
        [SerializeField] private BattleUiController _controller;

        /// <value>[SerializeField] Reference to the GameObject which can be used to hide or show the game over popup.</value>
        [SerializeField] private GameObject _view;

        /// <value>[SerializeField] Reference to the button which is used to exit game.</value>
        [SerializeField] private Button _button;

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
        /// Private Awake method which adds listener to the #_button's onClick event.
        /// </summary>
        ///
        /// The listener added to the #_button's onClick event calls @ref Battle::View::Game::BattleGameViewController::UiInputOnExitGamePressed "UiInputOnExitGamePressed" method through BattleUiController::GameViewController reference from #_controller.
        private void Awake()
        {
            _button.onClick.AddListener(_controller.GameViewController.UiInputOnExitGamePressed);
        }

        /// <summary>
        /// Private OnDestroy method which removes all listeners from #_button's onClick event.
        /// </summary>
        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}
