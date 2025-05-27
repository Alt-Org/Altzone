/// @file BattlePlayerInput.cs
/// <summary>
/// Handles subscribing to QuantumCallBack and polling player inputs for Quantum.
/// </summary>
///
/// This script:<br/>
/// Subscribes to QuantumCallBack.<br/>
/// Polls player inputs for Quantum.<br/>
/// Checks that player isn't dragging their finger on the screen to move their character.<br/>
/// Converts Unity's worldposition the player clicked into BattleGridManager's GridPosition.

using UnityEngine;

using Quantum;
using Input = Quantum.Input;
using Photon.Deterministic;

using Prg.Scripts.Common;

using Battle.QSimulation.Game;
using Battle.View.Game;

namespace Battle.View.Player
{
    /// <summary>
    /// %Player input <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour@u-exlink</a> script.<br/>
    /// Handles subscribing to QuantumCallBack and polling player inputs for Quantum.
    /// </summary>
    public class BattlePlayerInput : MonoBehaviour
    {
        /// <value>Previous click by a player.</value>
        private bool _mouseDownPrevious;

        /// <summary>
        /// Subscribes to QuantumCallBack when this script is enabled.
        /// </summary>
        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        /// <summary>
        /// Handles polling player input for Quantum.
        /// </summary>
        /// <param name="callback">Quantum Callback</param>
        private void PollInput(CallbackPollInput callback)
        {
            bool mouseDown = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll;
            bool mouseClick = !twoFingers && mouseDown && !_mouseDownPrevious;
            _mouseDownPrevious = mouseDown;

            BattleGridPosition movementPosition;
            if (mouseClick)
            {
                Vector3 unityPosition = BattleCamera.Camera.ScreenToWorldPoint(ClickStateHandler.GetClickPosition());
                movementPosition = new BattleGridPosition()
                {
                    Row = BattleGridManager.WorldYPositionToGridRow(FP.FromFloat_UNSAFE(unityPosition.z)),
                    Col = BattleGridManager.WorldXPositionToGridCol(FP.FromFloat_UNSAFE(unityPosition.x))
                };
            }
            else
            {
                movementPosition = new BattleGridPosition()
                {
                    Row = -1,
                    Col = -1
                };
            }

            Input i = new()
            {
                MouseClick = mouseClick,
                MovementPosition = movementPosition,
                //RotateMotion = twoFingers,
                RotationDirection = twoFingers ? FP.FromFloat_UNSAFE(ClickStateHandler.GetRotationDirection()) : 0,
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}
