using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Battle.Scripts.Battle.Players
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
        [SerializeField] private InputActionReference _clickInputAction;
        [SerializeField] private InputActionReference _moveInputAction;

        [Header("Live Data"), SerializeField] private Camera _camera;
        [SerializeField] private bool _isKeyboardReversed;
        [SerializeField] private Rect _playerArea = DefaultPlayerArea;
        [SerializeField] private Transform _playerTransform;

        private IPlayerDriver _playerDriver;
        private IGridManager _gridManager;
        private Vector2 _inputClick;
        private bool _isGridMovementDisabled;
        private int _gridWidth;
        private int _gridHeight;

        // We might want to simulate mobile device screen by ignoring click outside out window.
        private bool _isLimitMouseXYOnDesktop;

        private void Awake()
        {
            Assert.IsNull(_camera);
            _gridManager = Context.GetGridManager;
            _camera = Context.GetBattleCamera.Camera;
            var isDesktop = !Application.isMobilePlatform;
            _isLimitMouseXYOnDesktop = isDesktop;
            // PlayerInput is mandatory to have, for some reason!
            Assert.IsNotNull(FindObjectOfType<PlayerInput>(), "FindObjectOfType<PlayerInput>() != null");

            var runtimeGameConfig = RuntimeGameConfig.Get();
            var features = runtimeGameConfig.Features;
            var variables = runtimeGameConfig.Variables;
            _isGridMovementDisabled = features._isDisableBattleGridMovement;
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
            var isDesktop = !Application.isMobilePlatform;
            _isKeyboardReversed = isDesktop && Context.GetBattleCamera.IsRotated;
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
            if (_isGridMovementDisabled)
            {
                _playerDriver.MoveTo(targetPosition);
                return;
            }
            var rowAndColumn = _gridManager.CalcRowAndColumn(targetPosition, Context.GetBattleCamera.IsRotated);
            _playerDriver.SendMoveRequest(rowAndColumn[0], rowAndColumn[1]);
        }

        #endregion IPlayerInputHandler

        #region UNITY Input System

        private void SetupInput()
        {
            // https://gamedevbeginner.com/input-in-unity-made-easy-complete-guide-to-the-new-system/

            // WASD or GamePad -> performed is called once per key press
            var moveAction = _moveInputAction.action;
            moveAction.performed += DoMove;
            moveAction.canceled += StopMove;

            // Pointer movement when pressed down -> move to given point even pointer is released.
            var clickAction = _clickInputAction.action;
            clickAction.performed += DoClick;
        }

        private void ReleaseInput()
        {
            var moveAction = _moveInputAction.action;
            moveAction.performed -= DoMove;
            moveAction.canceled -= StopMove;

            var clickAction = _clickInputAction.action;
            clickAction.performed -= DoClick;
        }

        private void DoMove(InputAction.CallbackContext ctx)
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

        private void StopMove(InputAction.CallbackContext ctx)
        {
            // Simulate mouse click by "moving" to our current position.
            _inputClick = _playerTransform.position;
            SendMoveTo(_inputClick);
        }

        private void DoClick(InputAction.CallbackContext ctx)
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
