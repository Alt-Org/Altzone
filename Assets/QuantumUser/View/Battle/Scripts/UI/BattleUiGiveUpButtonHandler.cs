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
        /// @anchor BattleUiGiveUpButtonHandler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to BattleUiController.</summary>
        /// @ref BattleUiGiveUpButtonHandler-SerializeFields
        [SerializeField] private BattleUiController _uiController;

        /// <summary>[SerializeField] Reference to the BattleUiMovableElement script which is attached to a BattleUiGiveUpButton prefab.</summary>
        /// @ref BattleUiGiveUpButtonHandler-SerializeFields
        [SerializeField] private BattleUiMovableElement _movableUiElement;

        /// <summary>[SerializeField] Reference to the Button component of the give up button.</summary>
        /// @ref BattleUiGiveUpButtonHandler-SerializeFields
        [SerializeField] private Button _giveUpButton;

        /// @}

        /// <value>Is the %UI element visible or not.</value>
        public bool IsVisible => MovableUiElement.gameObject.activeSelf;

        /// <value>Public getter for #_movableUiElement.</value>
        public BattleUiMovableElement MovableUiElement => _movableUiElement;

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        ///
        /// <param name="show">True/False : visible / not visible.</param>
        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method
        /// which adds listener to the #_giveUpButton's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Button.html#UnityEngine_UI_Button_onClick">onClick@u-exlink</a> event.
        /// </summary>
        private void Awake()
        {
            _giveUpButton.onClick.AddListener(_uiController.GameViewController.UiInputOnLocalPlayerGiveUp);
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.OnDestroy.html">OnDestroy@u-exlink</a> method
        /// which removes all listeners from #_giveUpButton's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Button.html#UnityEngine_UI_Button_onClick">onClick@u-exlink</a> event.
        /// </summary>
        private void OnDestroy()
        {
            _giveUpButton.onClick.RemoveAllListeners();
        }
    }
}
