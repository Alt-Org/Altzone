using Battle0.Scripts.interfaces;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;

namespace Battle0.Scripts.Player
{
    /// <summary>
    /// Listens <c>InputManager</c> click down and up events and forwards them to <c>IMovablePlayer</c> for processing.
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private Camera _camera;
        [SerializeField] protected Transform _transform;
        [SerializeField] private float playerPositionZ;
        [SerializeField] private Vector3 mousePosition;

        private IMovablePlayer playerMovement;

        public Camera Camera
        {
            get => _camera;
            set => _camera = value;
        }

        public IMovablePlayer PlayerMovement
        {
            get => playerMovement;
            set
            {
                playerMovement = value;
                _transform = transform;
                playerPositionZ = _transform.position.z;
                this.Subscribe<InputManager.ClickDownEvent>(OnClickDownEvent);
                this.Subscribe<InputManager.ClickUpEvent>(OnClickUpEvent);
            }
        }

        private void Awake()
        {
            if (FindObjectOfType<InputManager>() == null)
            {
                throw new UnityException("InputManager was not found on the scene");
            }
        }

        private void OnDestroy()
        {
            this.Unsubscribe();
        }

        private void OnClickDownEvent(InputManager.ClickDownEvent data)
        {
            movePlayerTo(data.ScreenPosition);
        }

        private void OnClickUpEvent(InputManager.ClickUpEvent data)
        {
            movePlayerTo(data.ScreenPosition);
        }

        private void movePlayerTo(Vector3 screenPosition)
        {
            mousePosition = _camera.ScreenToWorldPoint(screenPosition);
            mousePosition.z = playerPositionZ;
            playerMovement.moveTo(mousePosition);
        }
    }
}