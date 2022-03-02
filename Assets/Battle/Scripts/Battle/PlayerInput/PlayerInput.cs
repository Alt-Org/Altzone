using Battle.Scripts.Battle.interfaces;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.PlayerInput
{
    /// <summary>
    /// Listens <c>InputManager</c> click down and up events and forwards them to <c>IMovablePlayer</c> for processing.
    /// </summary>
    internal class PlayerInput : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private Camera _camera;
        [SerializeField] private Vector2 _mousePosition;

        private IMovablePlayer _playerMovement;

        public IMovablePlayer PlayerMovement
        {
            get => _playerMovement;
            set
            {
                _playerMovement = value;
                _camera = _playerMovement.Camera;
                this.Subscribe<InputManager.ClickDownEvent>(OnClickDownEvent);
                this.Subscribe<InputManager.ClickUpEvent>(OnClickUpEvent);
                enabled = true;
            }
        }

        private void Awake()
        {
            Assert.IsNotNull(FindObjectOfType<InputManager>(), "InputManager was not found on the scene");
            // We need PlayerMovement before we can do work.
            enabled = false;
        }

        private void OnDestroy()
        {
            this.Unsubscribe();
        }

        private void OnClickDownEvent(InputManager.ClickDownEvent data)
        {
            MovePlayerTo(data.ScreenPosition);
        }

        private void OnClickUpEvent(InputManager.ClickUpEvent data)
        {
            MovePlayerTo(data.ScreenPosition);
        }

        private void MovePlayerTo(Vector3 screenPosition)
        {
            _mousePosition = _camera.ScreenToWorldPoint(screenPosition);
            _playerMovement.MoveTo(_mousePosition);
        }
    }
}