using UnityEngine;

using Quantum;
using Input = Quantum.Input;
using Photon.Deterministic;

using Prg.Scripts.Common;

using Battle.QSimulation.Game;
using Battle.View.Game;

using MovementInputType = SettingsCarrier.BattleMovementInputType;
using RotationInputType = SettingsCarrier.BattleRotationInputType;

namespace Battle.View.Player
{
    public class BattlePlayerInput : MonoBehaviour
    {
        private bool _mouseDownPrevious;
        private static Vector2 s_rotationStartVector;
        
        private const RotationInputType _rotationInputType = RotationInputType.TwoFinger;

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

            bool movementInput = false;
            BattleGridPosition movementPosition = new BattleGridPosition() {Row = -1, Col = -1};
            bool rotationInput = false;
            FP rotationValue = FP._0;

            if (mouseClick)
            {
                movementInput = true;
                Vector3 unityPosition = BattleCamera.Camera.ScreenToWorldPoint(ClickStateHandler.GetClickPosition());
                movementPosition = new BattleGridPosition()
                {
                    Row = BattleGridManager.WorldYPositionToGridRow(FP.FromFloat_UNSAFE(unityPosition.z)),
                    Col = BattleGridManager.WorldXPositionToGridCol(FP.FromFloat_UNSAFE(unityPosition.x))
                };
            }

            switch (_rotationInputType)
            {
                case RotationInputType.Swipe:
                    
                    float swipeMinDistance = 30f;

                    if (mouseDown && s_rotationStartVector == Vector2.zero)
                    {
                        s_rotationStartVector = ClickStateHandler.GetClickPosition();
                    }
                    else if (mouseDown && (ClickStateHandler.GetClickPosition().x - s_rotationStartVector.x > swipeMinDistance || ClickStateHandler.GetClickPosition().x - s_rotationStartVector.x < -swipeMinDistance))
                    {
                        rotationInput = true;
                        Vector2 currentPosition = ClickStateHandler.GetClickPosition();
                        float distance = currentPosition.x - s_rotationStartVector.x;
                        rotationValue = -FP.FromFloat_UNSAFE(distance);

                    }
                    else if (!mouseDown)
                    {
                        s_rotationStartVector = Vector2.zero;
                    }
                    break;

                case RotationInputType.TwoFinger:
                    if (twoFingers)
                    {
                        rotationInput = true;
                        rotationValue = FP.FromFloat_UNSAFE(ClickStateHandler.GetRotationValue());
                    }
                    break;
            }

            Input i = new()
            {
                MovementInput = movementInput,
                MovementPosition = movementPosition,
                RotationInput = rotationInput,
                RotationValue = rotationValue,
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}
