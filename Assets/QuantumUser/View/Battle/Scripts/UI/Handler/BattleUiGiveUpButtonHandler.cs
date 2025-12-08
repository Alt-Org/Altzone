/// @file BattleUiGiveUpButtonHandler.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiGiveUpButtonHandler} class which handles give up button functionality.
/// </summary>
///
/// This script:<br/>
/// Handles give up button functionality.

// Unity usings
using UnityEngine;
using TMPro;

// Quantum usings
using Quantum;

// Altzone usings
using Altzone.Scripts.BattleUiShared;

// Battle QSimulation usings
using Battle.QSimulation;

// Battle View usings
using Battle.View.Game;

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
        [SerializeField] private OnPointerDownButton _giveUpButton;

        /// <summary>[SerializeField] Reference to the Text component of the give up info text.</summary>
        /// @ref BattleUiGiveUpButtonHandler-SerializeFields
        [SerializeField] private TextMeshProUGUI _giveUpButtonInfoText;

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
        /// Updates the give up info text state.
        /// </summary>
        ///
        /// <param name="playerSlot">The slot of the player</param>
        /// <param name="stateUpdate">Type of the give up state update.</param>
        public void UpdateState(BattlePlayerSlot playerSlot, BattleGiveUpStateUpdate stateUpdate)
        {
            if (_buttonInfoState == ButtonInfoState.TeamGiveUp) return;

            switch (stateUpdate)
            {
                case BattleGiveUpStateUpdate.GiveUpVote:
                    if (playerSlot == BattleGameViewController.LocalPlayerSlot)
                    {
                        _buttonInfoState = _buttonInfoState is ButtonInfoState.TeammateGiveUp or ButtonInfoState.TeammateAbandoned
                                         ? ButtonInfoState.TeamGiveUp
                                         : ButtonInfoState.LocalGiveUp;
                    }
                    else
                    {
                        _buttonInfoState = _buttonInfoState is ButtonInfoState.LocalGiveUp
                                         ? ButtonInfoState.TeamGiveUp
                                         : ButtonInfoState.TeammateGiveUp;
                    }
                    break;

                case BattleGiveUpStateUpdate.Abandoned:
                    if (playerSlot == BattleGameViewController.LocalPlayerSlot)
                    {
                        _debugLogger.Warning("Active local client marked as abandoned");
                    }
                    else
                    {
                        _buttonInfoState = ButtonInfoState.TeammateAbandoned;
                    }
                    break;

                case BattleGiveUpStateUpdate.GiveUpVoteCancel:
                    _buttonInfoState = ButtonInfoState.Normal;
                    break;

                case BattleGiveUpStateUpdate.GiveUpNow:
                    _buttonInfoState = ButtonInfoState.TeamGiveUp;
                    break;

                default:
                    break;
            }

            UpdateInfoText();
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private BattleDebugLogger _debugLogger;

        /// <summary>
        /// States for the give up info text.
        /// </summary>
        private enum ButtonInfoState
        {
            Normal,
            LocalGiveUp,
            TeammateGiveUp,
            TeammateAbandoned,
            TeamGiveUp
        }

        /// <summary>
        /// Current state of the give up info text.
        /// </summary>
        private ButtonInfoState _buttonInfoState;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method
        /// which adds listener to the #_giveUpButton's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Button.html#UnityEngine_UI_Button_onClick">onClick@u-exlink</a> event.
        /// </summary>
        private void Awake()
        {
            _debugLogger = BattleDebugLogger.Create<BattleUiGiveUpButtonHandler>();

            _buttonInfoState = ButtonInfoState.Normal;
            UpdateInfoText();
            _giveUpButton.onClick.AddListener(_uiController.GameViewController.UiInputOnLocalPlayerGiveUp);
        }

        /// <summary>
        /// Private helper method for updating give up button info text based on <see cref="ButtonInfoState"/>.
        /// </summary>
        private void UpdateInfoText()
        {
            switch (_buttonInfoState)
            {
                case ButtonInfoState.Normal:
                    _giveUpButtonInfoText.text = "";
                    break;

                case ButtonInfoState.LocalGiveUp:
                    _giveUpButtonInfoText.text = "You want to give up";
                    break;

                case ButtonInfoState.TeammateGiveUp:
                    _giveUpButtonInfoText.text = "Teammate wants to give up";
                    break;

                case ButtonInfoState.TeammateAbandoned:
                    _giveUpButtonInfoText.text = "Teammate has abandoned you";
                    break;

                case ButtonInfoState.TeamGiveUp:
                    _giveUpButtonInfoText.text = "Your team gave up";
                    break;

                default:
                    break;
            }
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
