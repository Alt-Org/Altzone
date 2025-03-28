using Photon.Deterministic;
using UnityEngine;

using Prg.Scripts.Common;

using QuantumUser.Scripts;

namespace Quantum
{
    public class PlayerInput : MonoBehaviour
    {
        private bool _mouseDownPrevious;

        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        private void PollInput(CallbackPollInput callback)
        {
            bool mouseDown = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll;
            bool mouseClick = !twoFingers && mouseDown && !_mouseDownPrevious;
            _mouseDownPrevious = mouseDown;

            GridPosition movementPosition;
            if (mouseClick)
            {
                Vector3 unityPosition = BattleCamera.Camera.ScreenToWorldPoint(ClickStateHandler.GetClickPosition());
                movementPosition = new GridPosition()
                {
                    Row = GridManager.WorldYPositionToGridRow(FP.FromFloat_UNSAFE(unityPosition.z)),
                    Col = GridManager.WorldXPositionToGridCol(FP.FromFloat_UNSAFE(unityPosition.x))
                };
            }
            else
            {
                movementPosition = new GridPosition()
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
