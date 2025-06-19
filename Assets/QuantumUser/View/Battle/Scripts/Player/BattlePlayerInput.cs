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
        public void OnJoystickMovement(Vector2 input)
        {
            _joystickMovementVector = input;
        }

        public void OnJoystickRotation(float input)
        {
            _joystickRotationValue = input;
        }

        private MovementInputType _movementInputType;
        private RotationInputType _rotationInputType;

        private bool _mouseDownPrevious;
        private Vector2 _rotationStartVector;
        private Vector2 _movementStartVector;
        private Vector2 _joystickMovementVector;
        private float _joystickRotationValue;

        private float _swipeMinDistance = 30f;

        private void OnEnable()
        {
            _movementInputType = SettingsCarrier.Instance.BattleMovementInput;
            _rotationInputType = SettingsCarrier.Instance.BattleRotationInput;

            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        private void PollInput(CallbackPollInput callback)
        {
            Vector2 clickPosition = ClickStateHandler.GetClickPosition();

            bool mouseDown = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll;
            bool mouseClick = !twoFingers && mouseDown && !_mouseDownPrevious;
            _mouseDownPrevious = mouseDown;

            bool movementInput = false;
            BattleGridPosition movementPosition = new BattleGridPosition() {Row = -1, Col = -1};
            FPVector2 movementDirection = FPVector2.Zero;
            bool rotationInput = false;
            FP rotationValue = FP._0;

            switch (_movementInputType)
            {
                case MovementInputType.PointAndClick:
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
                    break;

                case MovementInputType.Swipe:
                    if (mouseDown && _movementStartVector == Vector2.zero)
                    {
                        _movementStartVector = clickPosition;
                    }
                    else if (mouseDown && (clickPosition.x - _movementStartVector.x > _swipeMinDistance || clickPosition.x - _movementStartVector.x < -_swipeMinDistance))
                    {
                        movementInput = true;
                        Vector2 direction = clickPosition - _movementStartVector;
                        movementDirection = new FPVector2(FP.FromFloat_UNSAFE(direction.x), FP.FromFloat_UNSAFE(direction.y));
                    }
                    else if (!mouseDown)
                    {
                        _movementStartVector = Vector2.zero;
                    }
                    break;

                case MovementInputType.Joystick:
                    if (_joystickMovementVector != Vector2.zero)
                    {
                        movementInput = true;
                        movementDirection = new FPVector2(FP.FromFloat_UNSAFE(_joystickMovementVector.x), FP.FromFloat_UNSAFE(_joystickMovementVector.y));
                    }
                    break;
            }

            switch (_rotationInputType)
            {
                case RotationInputType.Swipe:
                    
                    if (mouseDown && _rotationStartVector == Vector2.zero)
                    {
                        _rotationStartVector = clickPosition;
                    }
                    else if (mouseDown && (clickPosition.x - _rotationStartVector.x > _swipeMinDistance || clickPosition.x - _rotationStartVector.x < -_swipeMinDistance))
                    {
                        rotationInput = true;
                        float distance = clickPosition.x - _rotationStartVector.x;
                        rotationValue = -FP.FromFloat_UNSAFE(distance);

                    }
                    else if (!mouseDown)
                    {
                        _rotationStartVector = Vector2.zero;
                    }
                    break;

                case RotationInputType.TwoFinger:
                    if (twoFingers)
                    {
                        rotationInput = true;
                        rotationValue = FP.FromFloat_UNSAFE(ClickStateHandler.GetRotationValue());
                        rotationValue /= (FP)45;
                    }
                    break;

                case RotationInputType.Joystick:
                    if (_joystickRotationValue != 0)
                    {
                        rotationInput = true;
                        rotationValue = FP.FromFloat_UNSAFE(_joystickRotationValue);
                        rotationValue *= -1;
                    }
                    break;
            }

            Input i = new()
            {
                MovementInput = movementInput,
                MovementPosition = movementPosition,
                MovementDirection = movementDirection,
                RotationInput = rotationInput,
                RotationValue = rotationValue,
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}
