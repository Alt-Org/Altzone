/// @file BattlePlayerInput.cs
/// <summary>
/// Handles subscribing to QuantumCallBack and polling player inputs for Quantum.
/// </summary>
///
/// This script:<br/>
/// Subscribes to QuantumCallBack.<br/>
/// Polls player inputs for Quantum.<br/>
/// Handles movement and rotation input based on chosen input methods. <br/>
/// Handles obtaining data from device gyroscope.

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
using static SettingsCarrier;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Player input <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles subscribing to QuantumCallBack and polling player inputs for Quantum.
    /// </summary>
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

        private MovementInputType _movementInputType;
        private RotationInputType _rotationInputType;

        private float _previousTime;
        /// <value>Previous click by a player.</value>
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

        /// <summary>
        /// Subscribes to QuantumCallBack when this script is enabled.
        /// </summary>
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

        /// <summary>
        /// Handles polling player input for Quantum.
        /// </summary>
        /// <param name="callback">Quantum Callback</param>
        private void PollInput(CallbackPollInput callback)
        {
            FP deltaTime = FP.FromFloat_UNSAFE(Time.time - _previousTime);

            bool mouseDown = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll;
            bool mouseClick = !twoFingers && mouseDown && !_mouseDownPrevious;
            _mouseDownPrevious = mouseDown;

            Quantum.BattleMovementInputType movementInput = Quantum.BattleMovementInputType.None;
            bool movementDirectionIsNormalized = false;
            BattleGridPosition movementPosition = new BattleGridPosition() { Row = -1, Col = -1 };
            FPVector2 movementDirection = FPVector2.Zero;
            bool rotationInput = false;
            FP rotationValue = FP._0;

            if (!_characterSelectionInput)
            {
                Vector2 clickPosition = Vector2.zero;
                Vector3 unityPosition = Vector3.zero;
                if (mouseDown)
                {
                    clickPosition = ClickStateHandler.GetClickPosition();
                    unityPosition = BattleCamera.Camera.ScreenToWorldPoint(clickPosition);
                }

                (movementInput, movementDirectionIsNormalized, movementPosition, movementDirection) = GetMovementInput(mouseDown, mouseClick, unityPosition, deltaTime);
                (rotationInput, rotationValue) = GetRotationInput(mouseDown, twoFingers, unityPosition);
            }
            else if (!mouseDown)
            {
                _characterSelectionInput = false;
            }

            Input i = new()
            {
                MovementInput = movementInput,
                MovementDirectionIsNormalized = movementDirectionIsNormalized,
                MovementPosition = movementPosition,
                MovementDirection = movementDirection,
                RotationInput = rotationInput,
                RotationValue = rotationValue,
                PlayerCharacterNumber = _characterNumber
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);
            _previousTime = Time.time;

            _characterNumber = -1;
        }

        /// <summary>
        /// Handles player movement input based on selected input type.
        /// </summary>
        /// <param name="mouseDown">Whether input is currently held</param>
        /// <param name="mouseClick">Whether input started this frame</param>
        /// <param name="unityPosition">The unityPosition of the input</param>
        /// <param name="deltaTime">Time since previous frame</param>
        private (Quantum.BattleMovementInputType movementInput, bool movementDirectionIsNormalized, BattleGridPosition movementPosition, FPVector2 movementDirection) GetMovementInput(bool mouseDown, bool mouseClick, Vector3 unityPosition, FP deltaTime)
        {
            Quantum.BattleMovementInputType movementInput = Quantum.BattleMovementInputType.None;
            bool movementDirectionIsNormalized = false;
            BattleGridPosition movementPosition = new BattleGridPosition() { Row = -1, Col = -1 };
            FPVector2 movementDirection = FPVector2.Zero;

            switch (_movementInputType)
            {
                case MovementInputType.PointAndClick:
                    if (mouseClick)
                    {
                        movementInput = Quantum.BattleMovementInputType.Position;
                        movementPosition = new BattleGridPosition()
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
                        movementInput = Quantum.BattleMovementInputType.Direction;
                        if (_swipePerformed)
                        {
                            Vector3 direction = unityPosition - _movementStartVector;
                            movementDirection = new FPVector2(FP.FromFloat_UNSAFE(direction.x), FP.FromFloat_UNSAFE(direction.z)) / deltaTime;
                            movementDirection *= FP.FromFloat_UNSAFE(_swipeSensitivity);
                            
                        }
                        _movementStartVector = unityPosition;
                    }
                    break;

                case MovementInputType.Joystick:
                    if (_joystickMovementVector != Vector2.zero)
                    {
                        movementInput = Quantum.BattleMovementInputType.Direction;
                        movementDirectionIsNormalized = true;
                        movementDirection = new FPVector2(FP.FromFloat_UNSAFE(_joystickMovementVector.x), FP.FromFloat_UNSAFE(_joystickMovementVector.y));
                    }
                    break;
            }

            return (movementInput, movementDirectionIsNormalized, movementPosition, movementDirection);
        }

        /// <summary>
        /// Handles player rotation input based on selected input type.
        /// </summary>
        /// <param name="mouseDown">Whether input is currently held</param>
        /// <param name="twoFingers">Whether two finger input is currently held</param>
        /// <param name="unityPosition">The unityPosition of the input</param>
        private (bool rotationInput, FP rotationValue) GetRotationInput(bool mouseDown, bool twoFingers, Vector3 unityPosition)
        {
            bool rotationInput = false;
            FP rotationValue = FP._0;

            switch (_rotationInputType)
            {
                case RotationInputType.Swipe:

                    if (mouseDown && _rotationStartVector == Vector2.zero)
                    {
                        _rotationStartVector = unityPosition;
                    }
                    else if (mouseDown && (unityPosition.x - _rotationStartVector.x > _swipeMinDistance || unityPosition.x - _rotationStartVector.x < -_swipeMinDistance))
                    {
                        rotationInput = true;
                        float distance = unityPosition.x - _rotationStartVector.x;
                        float signe = distance < 0 ? -1f : 1f;
                        distance = Mathf.Abs(distance) - _swipeMinDistance;
                        float maxAdjustedDistance = Mathf.Clamp(distance / (_swipeMaxDistance - _swipeMinDistance), 0, 1) * signe;
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
                    float gyroValue = GetGyroValue();
                    if (Mathf.Abs(gyroValue) >= _gyroMinAngle)
                    {
                        rotationInput = true;
                        rotationValue = FP.FromFloat_UNSAFE(-(Mathf.Clamp(gyroValue / 75f, -1, 1)));
                    }
                    break;
            }

            return (rotationInput, rotationValue);
        }

        /// <summary>
        /// Gets the tilt value from the play device's gyroscope.
        /// </summary>
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
