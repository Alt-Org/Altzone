using Altzone.Scripts.Config;
using Battle0.Scripts.Battle.Game;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Battle0.Scripts.Battle.Players
{
    internal interface IPlayerInputHandler
    {
        void SetPlayerDriver(IPlayerDriver playerDriver, Transform playerTransform, Rect playerArea);

        void ResetPlayerDriver();
    }

    internal class PlayerInputHandler : MonoBehaviour, IPlayerInputHandler
    {
        private static readonly Rect DefaultPlayerArea = Rect.MinMaxRect(-100, -100, 100, 100);

        public static IPlayerInputHandler Get() => FindObjectOfType<PlayerInputHandler>();

        [Header("Settings"), SerializeField] private float _unReachableDistance = 100;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private InputActionReference _clickInputAction;
        [SerializeField] private InputActionReference _moveInputAction;

        [Header("Live Data"), SerializeField] private Camera _camera;
        [SerializeField] private bool _isKeyboardReversed;
        [SerializeField] private Rect _playerArea = DefaultPlayerArea;
        [SerializeField] private Transform _playerTransform;

        private IPlayerDriver _playerDriver;
        private IGridManager _gridManager;
        private Vector2 _inputClick;
        private bool _useBattleGridMovement;
        private bool _isKeyboardEnabled;
        private int _gridWidth;
        private int _gridHeight;

        // We might want to simulate mobile device screen by ignoring click outside out window.
        private bool _isLimitMouseXYOnDesktop;

        private void Awake()
        {
            Assert.IsNull(_camera);
            _gridManager = Context.GetGridManager;
            _camera = Context.GetBattleCamera.Camera;
            _isLimitMouseXYOnDesktop = AppPlatform.IsDesktop;
            // PlayerInput is required for input actions we use. They are setup in Editor.
            Assert.IsNotNull(_playerInput, "_playerInput != null");

            var runtimeGameConfig = Battle0GameConfig.Get();
            var features = runtimeGameConfig.Features;
            var variables = runtimeGameConfig.Variables;
            _useBattleGridMovement = features._useBattleGridMovement;
            // Keyboard is not supported (now) when grid based movement is in use.
            _isKeyboardEnabled = !_useBattleGridMovement;
            _gridWidth = variables._battleUiGridWidth;
            _gridHeight = variables._battleUiGridHeight;
            Assert.IsTrue(_gridWidth > 0, "_gridWidth > 0");
            Assert.IsTrue(_gridHeight > 0, "_gridHeight > 0");
        }

        private void OnDestroy()
        {
            Debug.Log($"{name}");
            ReleaseInput();
        }

        private void SetupCamera()
        {
            _isKeyboardReversed = AppPlatform.IsDesktop && Context.GetBattleCamera.IsRotated;
        }

        #region IPlayerInputHandler

        public void SetPlayerDriver(IPlayerDriver playerDriver, Transform playerTransform, Rect playerArea)
        {
            Debug.Log($"{name}");
            _playerDriver = playerDriver;
            _playerTransform = playerTransform;
            _playerArea = playerArea;
            SetupCamera();
            SetupInput();
        }

        public void ResetPlayerDriver()
        {
            Debug.Log($"{name}");
            ReleaseInput();
            _playerDriver = null;
            _playerTransform = null;
            _playerArea = DefaultPlayerArea;
        }

        private void SendMoveTo(Vector2 targetPosition)
        {
            if (_useBattleGridMovement)
            {
                var gridPos = _gridManager.WorldPointToGridPosition(targetPosition, Context.GetBattleCamera.IsRotated);
                _playerDriver.SendMoveRequest(gridPos);
                return;
            }
            _playerDriver.MoveTo(targetPosition);
        }

        #endregion IPlayerInputHandler

        #region UNITY Input System

        private void SetupInput()
        {
            // https://gamedevbeginner.com/input-in-unity-made-easy-complete-guide-to-the-new-system/

            if (_isKeyboardEnabled)
            {
                // WASD or GamePad -> performed is called once per key press
                var moveAction = _moveInputAction.action;
                moveAction.performed += DoKeyboardMove;
                moveAction.canceled += StopKeyboardMove;
            }
            // Pointer movement when pressed down -> move to given point even pointer is released.
            var clickAction = _clickInputAction.action;
            clickAction.performed += DoPointerClick;
        }

        private void ReleaseInput()
        {
            if (_isKeyboardEnabled)
            {
                var moveAction = _moveInputAction.action;
                moveAction.performed -= DoKeyboardMove;
                moveAction.canceled -= StopKeyboardMove;
            }
            var clickAction = _clickInputAction.action;
            clickAction.performed -= DoPointerClick;
        }

        private void DoKeyboardMove(InputAction.CallbackContext ctx)
        {
            // Simulate mouse click by trying to move very far.
            _inputClick = ctx.ReadValue<Vector2>() * _unReachableDistance;
            Vector2 inputPosition = _playerTransform.position;
            if (_isKeyboardReversed)
            {
                inputPosition.x -= _inputClick.x;
                inputPosition.y -= _inputClick.y;
            }
            else
            {
                inputPosition.x += _inputClick.x;
                inputPosition.y += _inputClick.y;
            }
            _inputClick.x = Mathf.Clamp(inputPosition.x, _playerArea.xMin, _playerArea.xMax);
            _inputClick.y = Mathf.Clamp(inputPosition.y, _playerArea.yMin, _playerArea.yMax);
            SendMoveTo(_inputClick);
        }

        private void StopKeyboardMove(InputAction.CallbackContext ctx)
        {
            // Simulate mouse click by "moving" to our current position.
            _inputClick = _playerTransform.position;
            SendMoveTo(_inputClick);
        }

        private void DoPointerClick(InputAction.CallbackContext ctx)
        {
            _inputClick = ctx.ReadValue<Vector2>();
#if UNITY_STANDALONE
            if (_isLimitMouseXYOnDesktop)
            {
                if (_inputClick.x < 0 || _inputClick.y < 0 ||
                    _inputClick.x > Screen.width || _inputClick.y > Screen.height)
                {
                    return;
                }
            }
#endif
            _inputClick = _camera.ScreenToWorldPoint(_inputClick);
            _inputClick.x = Mathf.Clamp(_inputClick.x, _playerArea.xMin, _playerArea.xMax);
            _inputClick.y = Mathf.Clamp(_inputClick.y, _playerArea.yMin, _playerArea.yMax);
            SendMoveTo(_inputClick);
        }

        #endregion UNITY Input System
    }
}
