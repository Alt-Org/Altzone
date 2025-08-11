using UnityEngine;
using UnityEngine.InputSystem;

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

        public void OnCharacterSelected(int characterNumber)
        {
            _characterNumber = characterNumber;
            _characterSelectionInput = true;
        }

        private struct MovementInputInfo
        {
            public BattleMovementInputType MovementInput;
            public bool MovementDirectionIsNormalized;
            public BattleGridPosition MovementPosition;
            public FPVector2 MovementDirection;

            public MovementInputInfo(BattleMovementInputType movementInput, bool movementDirectionIsNormalized, BattleGridPosition movementPosition, FPVector2 movementDirection)
            {
                MovementInput = movementInput;
                MovementDirectionIsNormalized = movementDirectionIsNormalized;
                MovementPosition = movementPosition;
                MovementDirection = movementDirection;
            }
        }

        private struct RotationInputInfo
        {
            public bool RotationInput;
            public FP RotationValue;

            public RotationInputInfo(bool rotationInput, FP rotationValue)
            {
                RotationInput = rotationInput;
                RotationValue = rotationValue;
            }
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

        private bool _characterSelectionInput = false;

        private float _swipeMinDistance = 0.1f;
        private float _swipeMaxDistance = 1.0f;
        private float _swipeSensitivity = 1.0f;
        private float _gyroMinAngle = 10f;

        private AttitudeSensor _attitudeSensor;

        private bool _swipeMovementStarted = false;

        private void OnEnable()
        {
            _movementInputType = SettingsCarrier.Instance.BattleMovementInput;
            _rotationInputType = SettingsCarrier.Instance.BattleRotationInput;
            _swipeMinDistance  = SettingsCarrier.Instance.BattleSwipeMinDistance;
            _swipeMaxDistance  = SettingsCarrier.Instance.BattleSwipeMaxDistance;
            _swipeSensitivity  = SettingsCarrier.Instance.BattleSwipeSensitivity;
            _gyroMinAngle      = SettingsCarrier.Instance.BattleGyroMinAngle;

            if (AttitudeSensor.current != null)
            {
                InputSystem.EnableDevice(AttitudeSensor.current);
                _attitudeSensor = AttitudeSensor.current;
            }

            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        private void PollInput(CallbackPollInput callback)
        {
            FP deltaTime = FP.FromFloat_UNSAFE(Time.time - _previousTime);

            bool mouseDown = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll;
            bool mouseClick = !twoFingers && mouseDown && !_mouseDownPrevious;
            _mouseDownPrevious = mouseDown;

            MovementInputInfo movementInputInfo = new(BattleMovementInputType.None, false, new BattleGridPosition() { Row = -1, Col = -1 }, FPVector2.Zero);
            RotationInputInfo rotationInputInfo = new(false, FP._0);

            if (!_characterSelectionInput)
            {
                Vector2 clickPosition = Vector2.zero;
                Vector3 unityPosition = Vector3.zero;
                if (mouseDown)
                {
                    clickPosition = ClickStateHandler.GetClickPosition();
                    unityPosition = BattleCamera.Camera.ScreenToWorldPoint(clickPosition);
                }

                movementInputInfo = GetMovementInput(mouseDown, mouseClick, unityPosition, deltaTime);
                rotationInputInfo = GetRotationInput(mouseDown, twoFingers, unityPosition);
            }
            else if (!mouseDown)
            {
                _characterSelectionInput = false;
            }

            Input i = new()
            {
                MovementInput = movementInputInfo.MovementInput,
                MovementDirectionIsNormalized = movementInputInfo.MovementDirectionIsNormalized,
                MovementPosition = movementInputInfo.MovementPosition,
                MovementDirection = movementInputInfo.MovementDirection,
                RotationInput = rotationInputInfo.RotationInput,
                RotationValue = rotationInputInfo.RotationValue,
                PlayerCharacterNumber = _characterNumber
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
            _previousTime = Time.time;

            _characterNumber = -1;
        }

        private MovementInputInfo GetMovementInput(bool mouseDown, bool mouseClick, Vector3 unityPosition, FP deltaTime)
        {
            MovementInputInfo movementInputInfo = new(BattleMovementInputType.None, false, new BattleGridPosition() { Row = -1, Col = -1 }, FPVector2.Zero);

            switch (_movementInputType)
            {
                case MovementInputType.PointAndClick:
                    if (mouseClick)
                    {
                        movementInputInfo.MovementInput = BattleMovementInputType.Position;
                        movementInputInfo.MovementPosition = new()
                        {
                            Row = BattleGridManager.WorldYPositionToGridRow(FP.FromFloat_UNSAFE(unityPosition.z)),
                            Col = BattleGridManager.WorldXPositionToGridCol(FP.FromFloat_UNSAFE(unityPosition.x))
                        };
                    }
                    break;

                case MovementInputType.Swipe:
                    bool _swipePerformed = false;

                    if (mouseDown && _movementStartVector == Vector3.zero)
                    {
                        _movementStartVector = unityPosition;
                    }
                    else if (mouseDown && ((unityPosition - _movementStartVector).sqrMagnitude > _swipeMinDistance * _swipeMinDistance))
                    {
                        _swipeMovementStarted = true;
                        _swipePerformed = true;
                    }
                    else if (!mouseDown)
                    {
                        _movementStartVector = Vector3.zero;
                        _swipeMovementStarted = false;
                    }

                    if (_swipeMovementStarted)
                    {
                        movementInputInfo.MovementInput = BattleMovementInputType.Direction;
                        if (_swipePerformed)
                        {
                            Vector3 direction = unityPosition - _movementStartVector;
                            movementInputInfo.MovementDirection = new FPVector2(FP.FromFloat_UNSAFE(direction.x), FP.FromFloat_UNSAFE(direction.z)) / deltaTime;
                            movementInputInfo.MovementDirection *= FP.FromFloat_UNSAFE(_swipeSensitivity);
                            
                        }
                        _movementStartVector = unityPosition;
                    }
                    break;

                case MovementInputType.Joystick:
                    if (_joystickMovementVector != Vector2.zero)
                    {
                        movementInputInfo.MovementInput = BattleMovementInputType.Direction;
                        movementInputInfo.MovementDirectionIsNormalized = true;
                        movementInputInfo.MovementDirection = new FPVector2(FP.FromFloat_UNSAFE(_joystickMovementVector.x), FP.FromFloat_UNSAFE(_joystickMovementVector.y));
                    }
                    break;
            }

            return movementInputInfo;
        }

        private RotationInputInfo GetRotationInput(bool mouseDown, bool twoFingers, Vector3 unityPosition)
        {
            RotationInputInfo rotationInputInfo = new(false, FP._0);

            switch (_rotationInputType)
            {
                case RotationInputType.Swipe:

                    if (mouseDown && _rotationStartVector == Vector2.zero)
                    {
                        _rotationStartVector = unityPosition;
                    }
                    else if (mouseDown && (unityPosition.x - _rotationStartVector.x > _swipeMinDistance || unityPosition.x - _rotationStartVector.x < -_swipeMinDistance))
                    {
                        rotationInputInfo.RotationInput = true;
                        float distance = unityPosition.x - _rotationStartVector.x;
                        float signe = distance < 0 ? -1f : 1f;
                        distance = Mathf.Abs(distance) - _swipeMinDistance;
                        float maxAdjustedDistance = Mathf.Clamp(distance / (_swipeMaxDistance - _swipeMinDistance), 0, 1) * signe;
                        rotationInputInfo.RotationValue = -FP.FromFloat_UNSAFE(maxAdjustedDistance);

                        if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta) rotationInputInfo.RotationValue *= -1;
                    }
                    else if (!mouseDown)
                    {
                        _rotationStartVector = Vector2.zero;
                    }
                    break;

                case RotationInputType.TwoFinger:
                    if (twoFingers)
                    {
                        rotationInputInfo.RotationInput = true;
                        rotationInputInfo.RotationValue = FP.FromFloat_UNSAFE(ClickStateHandler.GetRotationValue());
                        rotationInputInfo.RotationValue /= (FP)45;
                    }
                    break;

                case RotationInputType.Joystick:
                    if (_joystickRotationValue != 0)
                    {
                        rotationInputInfo.RotationInput = true;
                        rotationInputInfo.RotationValue = FP.FromFloat_UNSAFE(_joystickRotationValue);
                        rotationInputInfo.RotationValue *= -1;
                    }
                    break;

                case RotationInputType.Gyroscope:
                    float gyroValue = GetGyroValue();
                    if (Mathf.Abs(gyroValue) >= _gyroMinAngle)
                    {
                        rotationInputInfo.RotationInput = true;
                        rotationInputInfo.RotationValue = FP.FromFloat_UNSAFE(-(Mathf.Clamp(gyroValue / 75f, -1, 1)));
                    }
                    break;
            }

            return rotationInputInfo;
        }

        private float GetGyroValue()
        {
            Quaternion deviceRotation = new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * _attitudeSensor.attitude.ReadValue() * new Quaternion(0, 0, 1, 0);
            Vector3 rot = (Quaternion.Inverse(Quaternion.FromToRotation(Quaternion.identity * Vector3.forward, deviceRotation * Vector3.forward)) * deviceRotation).eulerAngles;
            if (rot.z > 180f)
            {
                rot = new Vector3(0, 0, rot.z - 360f);
            }
            return rot.z;
        }
    }
}
