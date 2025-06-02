/// @file BattleUiGiveUpButtonHandler.cs
/// <summary>
/// Has a class BattleUiGiveUpButtonHandler which handles give up button functionality.
/// </summary>
///
/// This script:<br/>
/// Handles give up button functionality.

using UnityEngine;
using UnityEngine.UI;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">GiveUpButton @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles give up button functionality.
    /// </summary>
    public class BattleUiGiveUpButtonHandler : MonoBehaviour
    {
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Reference to BattleUiController.</value>
        [SerializeField] private BattleUiController _uiController;

        /// <value>[SerializeField] Reference to the BattleUiMovableElement script which is attached to a BattleUiGiveUpButton prefab.</value>
        [SerializeField] private BattleUiMovableElement _movableUiElement;

        /// <value>[SerializeField] Reference to the Button component of the give up button.</value>
        [SerializeField] private Button _giveUpButton;

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
        /// Private <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method which adds listener to the #_giveUpButton's <a href="https://docs.unity3d.com/530/Documentation/ScriptReference/UI.Button-onClick.html">onClick@u-exlink</a> event.
        /// </summary>
        private void Awake()
        {
            _giveUpButton.onClick.AddListener(_uiController.GameViewController.UiInputOnLocalPlayerGiveUp);
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/MonoBehaviour.OnDestroy.html">OnDestroy@u-exlink</a> method which removes all listeners from #_giveUpButton's <a href="https://docs.unity3d.com/530/Documentation/ScriptReference/UI.Button-onClick.html">onClick@u-exlink</a> event.
        /// </summary>
        private void OnDestroy()
        {
            _giveUpButton.onClick.RemoveAllListeners();
        }
    }
}
