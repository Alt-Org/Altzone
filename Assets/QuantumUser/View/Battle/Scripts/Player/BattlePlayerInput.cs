/// @file BattlePlayerInput.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerInput} class 
/// which handles subscribing to QuantumCallBack and polling player inputs for Quantum.<br/>
/// Input is processed and compiled into an input struct, which is passed over to the Quantum simulation when polled by Quantum.
/// </summary>
///
/// See [{PlayerInput}](#page-concepts-player-input) for more info.

//#define DEBUG_INPUT_TYPE_OVERRIDE

// Unity usings
using UnityEngine;
using UnityEngine.InputSystem;

// Quantum usings
using Quantum;
using Input = Quantum.Input;
using Photon.Deterministic;

// Prg usings
using Prg.Scripts.Common;

// Battle QSimulation usings
using Battle.QSimulation.Game;

// Battle View usings
using Battle.View.Game;

using MovementInputType = SettingsCarrier.BattleMovementInputType;
using RotationInputType = SettingsCarrier.BattleRotationInputType;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Player input <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles subscribing to QuantumCallBack and polling player inputs for %Quantum.<br/>
    /// Input is processed and compiled into an input struct, which is passed over to the %Quantum simulation when polled by %Quantum.
    /// </summary>
    public class BattlePlayerInput : MonoBehaviour
    {
        /// @name Input methods
        /// Input methods are called by BattleGameViewController when the player gives a %UI input. These methods shouldn't be called any other way.
        /// @{

        /// <summary>
        /// Called when the player interacts with the movement joystick.
        /// </summary>
        ///
        /// <param name="input">The input value of the movement joystick</param>
        public void OnJoystickMovement(Vector2 input)
        {
            _joystickMovementVector = input;
        }

        /// <summary>
        /// Called when the player interacts with the rotation joystick.
        /// </summary>
        ///
        /// <param name="input">The input value of the rotation joystick</param>
        public void OnJoystickRotation(float input)
        {
            _joystickRotationValue = input;
        }

        /// <summary>
        /// Called when the player presses one of the character selection buttons.
        /// </summary>
        ///
        /// <param name="characterNumber">The number of the character that was selected.</param>
        public void OnCharacterSelected(int characterNumber)
        {
            _characterNumber = characterNumber;
        }

        /// <summary>
        /// Called when the player presses the give up button.
        /// </summary>
        public void OnGiveUp()
        {
            _onGiveUp = true;
        }

        /// @}

        /// <summary>
        /// Struct containing data related to player movement input.
        /// </summary>
        private struct MovementInputInfo
        {
            public BattleMovementInputType MovementInput;
            public bool                    MovementDirectionIsNormalized;
            public BattleGridPosition      MovementPositionTarget;
            public FPVector2               MovementPositionMove;
            public FPVector2               MovementDirection;

            public MovementInputInfo(BattleMovementInputType movementInput, bool movementDirectionIsNormalized, BattleGridPosition movementPositionTarget, FPVector2 movementPositionMove, FPVector2 movementDirection)
            {
                MovementInput                 = movementInput;
                MovementDirectionIsNormalized = movementDirectionIsNormalized;
                MovementPositionTarget        = movementPositionTarget;
                MovementPositionMove          = movementPositionMove;
                MovementDirection             = movementDirection;
            }
        }

        /// <summary>
        /// Struct containing data related to player rotation input.
        /// </summary>
        private struct RotationInputInfo
        {
            public bool RotationInput;
            public FP   RotationValue;

            public RotationInputInfo(bool rotationInput, FP rotationValue)
            {
                RotationInput = rotationInput;
                RotationValue = rotationValue;
            }
        }

        /// @name State variables
        /// Variables related to current input states.
        /// @{

        /// <value>Saved time from previous frame.</value>
        private float _previousTime;

        /// <value>Bool for if a press input was received in the previous frame.</value>
        private bool _mouseDownPrevious;

        /// <value>Bool for if swipe movement has started and not stopped.</value>
        private bool _swipeMovementStarted = false;

        /// <value>Initial saved vector when rotation input is first detected.</value>
        private Vector2 _rotationStartVector;

        /// <value>Initial saved vector when movement input is first detected.</value>
        private Vector3 _movementStartVector;

        /// <value>The vector received from the movement joystick.</value>
        private Vector2 _joystickMovementVector;

        /// <value>The float value received from the rotation joystick.</value>
        private float _joystickRotationValue;

        /// <value>Saved character number from character swapping input.</value>
        private int _characterNumber = -1;

        /// <value>Give up button state</value>
        private bool _onGiveUp = false;

        /// <value>Bool to block screen input</value>
        private bool _blockScreenInput = false;

        /// @}

        /// @name Setting variables
        /// Data from SettingsCarrier is saved to these variables.
        /// @{

        /// <value>Saved info of the selected movement input type.</value>
        private MovementInputType _movementInputType;

        /// <value>Saved info of the selected rotation input type.</value>
        private RotationInputType _rotationInputType;

        /// <value>The minimum distance for activating swipe rotation.</value>
        private float _swipeMinDistance = 0.1f;

        /// <value>The swipe distance at which rotation reaches its maximum value.</value>
        private float _swipeMaxDistance = 1.0f;

        /// <value>The sensitivity for swipe movement.</value>
        private float _swipeSensitivity = 1.0f;

        /// <value>The minimum tilt angle for activating gyroscope rotation.</value>
        private float _gyroMinAngle = 10f;

        /// @}

        /// @name References
        /// Saved references.
        /// @{

        /// <value>Reference to the play device's attitude sensor aka gyroscope.</value>
        private AttitudeSensor _attitudeSensor;

        /// @}

        /// <summary>
        /// Saves data from SettingsCarrier to private variables. <br/>
        /// Saves a reference to the play device's gyroscope if there is one. <br/>
        /// Subscribes to QuantumCallBack.
        /// </summary>
        private void OnEnable()
        {
            _movementInputType = SettingsCarrier.Instance.BattleMovementInput;
            _rotationInputType = SettingsCarrier.Instance.BattleRotationInput;
            _swipeMinDistance  = SettingsCarrier.Instance.BattleSwipeMinDistance;
            _swipeMaxDistance  = SettingsCarrier.Instance.BattleSwipeMaxDistance;
            _swipeSensitivity  = SettingsCarrier.Instance.BattleSwipeSensitivity;
            _gyroMinAngle      = SettingsCarrier.Instance.BattleGyroMinAngle;

#if DEBUG_INPUT_TYPE_OVERRIDE
            _movementInputType = MovementInputType.FollowPointer;
            _rotationInputType = RotationInputType.TwoFinger;
#endif

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
        ///
        /// <param name="callback">Quantum Callback</param>
        private void PollInput(CallbackPollInput callback)
        {
            FP deltaTime = FP.FromFloat_UNSAFE(Time.time - _previousTime);

            // set common input variables
            bool mouseDown = ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move;
            bool twoFingers = ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll;
            bool mouseClick = !twoFingers && mouseDown && !_mouseDownPrevious;
            _mouseDownPrevious = mouseDown;

            // set default input info
            MovementInputInfo movementInputInfo = new(BattleMovementInputType.None, false, new BattleGridPosition() { Row = -1, Col = -1 }, FPVector2.Zero, FPVector2.Zero);
            RotationInputInfo rotationInputInfo = new(false, FP._0);

            // check button input
            if (_characterNumber > -1 || _onGiveUp) _blockScreenInput = true;

            // handles screen input
            if (!_blockScreenInput)
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
                _blockScreenInput = false;
            }

            //{ create and set input

            Input i = new()
            {
                MovementInput                 = movementInputInfo.MovementInput,
                MovementDirectionIsNormalized = movementInputInfo.MovementDirectionIsNormalized,
                MovementPositionTarget        = movementInputInfo.MovementPositionTarget,
                MovementPositionMove          = movementInputInfo.MovementPositionMove,
                MovementDirection             = movementInputInfo.MovementDirection,
                RotationInput                 = rotationInputInfo.RotationInput,
                RotationValue                 = rotationInputInfo.RotationValue,
                PlayerCharacterNumber         = _characterNumber,
                GiveUpInput = _onGiveUp
            };

            callback.SetInput(i, DeterministicInputFlags.Repeatable);

            //} create and set input

            _previousTime = Time.time;

            // reset
            _onGiveUp = false;
            _characterNumber = -1;
        }

        /// @name Input reading methods
        /// Helper methods for reading player input data.
        /// @{

        /// <summary>
        /// Handles player movement input based on selected input type.
        /// </summary>
        ///
        /// <param name="mouseDown">Whether input is currently held</param>
        /// <param name="mouseClick">Whether input started this frame</param>
        /// <param name="unityPosition">The unityPosition of the input</param>
        /// <param name="deltaTime">Time since previous frame</param>
        private MovementInputInfo GetMovementInput(bool mouseDown, bool mouseClick, Vector3 unityPosition, FP deltaTime)
        {
            MovementInputInfo movementInputInfo = new(BattleMovementInputType.None, false, new BattleGridPosition() { Row = -1, Col = -1 }, FPVector2.Zero, FPVector2.Zero);

            switch (_movementInputType)
            {
                case MovementInputType.PointAndClick:
                    if (mouseClick)
                    {
                        movementInputInfo.MovementInput = BattleMovementInputType.PositionTarget;
                        movementInputInfo.MovementPositionTarget = new()
                        {
                            Row = BattleGridManager.WorldYPositionToGridRow(FP.FromFloat_UNSAFE(unityPosition.z)),
                            Col = BattleGridManager.WorldXPositionToGridCol(FP.FromFloat_UNSAFE(unityPosition.x))
                        };
                    }
                    break;

                case MovementInputType.FollowPointer:
                    if (mouseDown)
                    {
                        movementInputInfo.MovementInput = BattleMovementInputType.PositionMove;
                        movementInputInfo.MovementPositionMove = new FPVector2
                        (
                            FP.FromFloat_UNSAFE(unityPosition.x),
                            FP.FromFloat_UNSAFE(unityPosition.z)
                        );
                    }
                    else
                    {
                        movementInputInfo.MovementInput = BattleMovementInputType.None;
                        movementInputInfo.MovementPositionMove = FPVector2.Zero;
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

                        if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta) movementInputInfo.MovementDirection *= -1;
                    }
                    break;
            }

            return movementInputInfo;
        }

        /// <summary>
        /// Handles player rotation input based on selected input type.
        /// </summary>
        ///
        /// <param name="mouseDown">Whether input is currently held</param>
        /// <param name="twoFingers">Whether two finger input is currently held</param>
        /// <param name="unityPosition">The unityPosition of the input</param>
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

        /// @}
    }
}
