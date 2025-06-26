using UnityEngine;

using Quantum;
using Input = Quantum.Input;
using Photon.Deterministic;

using Prg.Scripts.Common;

using Battle.QSimulation.Game;
using Battle.View.Game;

using MovementInputType = SettingsCarrier.BattleMovementInputType;
using RotationInputType = SettingsCarrier.BattleRotationInputType;
using TMPro;

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

        public void OnCharacterSelected(int characterNumber)
        {
            Debug.LogWarning("character select detected");
            _characterNumber = characterNumber;
        }

        private MovementInputType _movementInputType;
        private RotationInputType _rotationInputType;

        private float _previousTime;
        private bool _mouseDownPrevious;
        private Vector2 _rotationStartVector;
        private Vector3 _movementStartVector;
        private Vector2 _joystickMovementVector;
        private float _joystickRotationValue;
        private int _characterNumber = -1;

        private float _swipeMinDistance = 0.1f;
        private float _swipeMaxDistance = 1.0f;
        private float _swipeSensitivity = 1.0f;
        private float _gyroMinAngle = 10f;

        private void OnEnable()
        {
            _movementInputType = SettingsCarrier.Instance.BattleMovementInput;
            _rotationInputType = SettingsCarrier.Instance.BattleRotationInput;
            _swipeMinDistance  = SettingsCarrier.Instance.BattleSwipeMinDistance;
            _swipeMaxDistance  = SettingsCarrier.Instance.BattleSwipeMaxDistance;
            _swipeSensitivity  = SettingsCarrier.Instance.BattleSwipeSensitivity;
            _gyroMinAngle      = SettingsCarrier.Instance.BattleGyroMinAngle;

            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        private void PollInput(CallbackPollInput callback)
        {
            FP deltaTime = FP.FromFloat_UNSAFE(Time.time - _previousTime);

            bool mouseDown = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll;
            bool mouseClick = !twoFingers && mouseDown && !_mouseDownPrevious;
            _mouseDownPrevious = mouseDown;

            if (_characterNumber > -1)
            {
                if (!mouseDown)
                {
                    _characterNumber = -1;
                }
                return;
            }

            bool movementInput = false;
            bool movementDirectionIsNormalized = false;
            BattleGridPosition movementPosition = new BattleGridPosition() {Row = -1, Col = -1};
            FPVector2 movementDirection = FPVector2.Zero;
            bool rotationInput = false;
            FP rotationValue = FP._0;

            Vector2 clickPosition = Vector2.zero;
            Vector3 unityPosition = Vector3.zero;
            if (mouseDown)
            { 
                clickPosition = ClickStateHandler.GetClickPosition();
                unityPosition = BattleCamera.Camera.ScreenToWorldPoint(clickPosition);
            }

            switch (_movementInputType)
            {
                case MovementInputType.PointAndClick:
                    if (mouseClick)
                    {
                        movementInput = true;
                        movementPosition = new BattleGridPosition()
                        {
                            Row = BattleGridManager.WorldYPositionToGridRow(FP.FromFloat_UNSAFE(unityPosition.z)),
                            Col = BattleGridManager.WorldXPositionToGridCol(FP.FromFloat_UNSAFE(unityPosition.x))
                        };
                    }
                    break;

                case MovementInputType.Swipe:
                    if (mouseDown && _movementStartVector == Vector3.zero)
                    {
                        _movementStartVector = unityPosition;
                    }
                    else if (mouseDown && ((unityPosition - _movementStartVector).sqrMagnitude > _swipeMinDistance * _swipeMinDistance))
                    {
                        movementInput = true;
                        Vector3 direction = unityPosition - _movementStartVector;
                        movementDirection = new FPVector2(FP.FromFloat_UNSAFE(direction.x), FP.FromFloat_UNSAFE(direction.z)) / deltaTime;
                        movementDirection *= FP.FromFloat_UNSAFE(_swipeSensitivity);
                        _movementStartVector = unityPosition;
                    }
                    else if (!mouseDown)
                    {
                        _movementStartVector = Vector3.zero;
                    }
                    break;

                case MovementInputType.Joystick:
                    if (_joystickMovementVector != Vector2.zero)
                    {
                        movementInput = true;
                        movementDirectionIsNormalized = true;
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
                        float maxAdjustedDistance = Mathf.Clamp(distance / _swipeMaxDistance, -1, 1);
                        rotationValue = -FP.FromFloat_UNSAFE(maxAdjustedDistance);

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

                case RotationInputType.Gyroscope:
                    float gyroValue = ClickStateHandler.GetGyroValue();
                    if (Mathf.Abs(gyroValue) >= _gyroMinAngle)
                    {
                        rotationInput = true;
                        rotationValue = FP.FromFloat_UNSAFE(-(Mathf.Clamp(gyroValue/75, -1, 1)));
                    }
                    break;
            }

            Input i = new()
            {
                MovementInput = movementInput,
                MovementDirectionIsNormalized = movementDirectionIsNormalized,
                MovementPosition = movementPosition,
                MovementDirection = movementDirection,
                RotationInput = rotationInput,
                RotationValue = rotationValue,
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
            _previousTime = Time.time;
        }
    }
}
