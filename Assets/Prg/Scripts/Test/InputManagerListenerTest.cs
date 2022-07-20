using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;

namespace Prg.Scripts.Test
{
    public class InputManagerListenerTest : MonoBehaviour
    {
        public bool _isPointerDown;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        protected void Awake()
        {
        }

        private void OnEnable()
        {
            this.Subscribe<InputManager.ClickDownEvent>(OnClickDownEvent);
            this.Subscribe<InputManager.ClickUpEvent>(OnClickUpEvent);
            this.Subscribe<InputManager.PanEvent>(OnPanEvent);
            this.Subscribe<InputManager.ZoomEvent>(OnZoomEvent);
            this.Subscribe<ClickListener.ClickObjectEvent>(OnClickObjectEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnClickDownEvent(InputManager.ClickDownEvent data)
        {
            if (_isPointerDown)
            {
                return;
            }
            _isPointerDown = true;
            Debug.Log($"{data}");
        }

        private void OnClickUpEvent(InputManager.ClickUpEvent data)
        {
            _isPointerDown = false;
            Debug.Log($"{data}");
        }

        private void OnPanEvent(InputManager.PanEvent data)
        {
            Debug.Log($"{data}");
        }

        private void OnZoomEvent(InputManager.ZoomEvent data)
        {
            Debug.Log($"{data}");
        }

        private void OnClickObjectEvent(ClickListener.ClickObjectEvent data)
        {
            Debug.Log($"{data}");
        }
#endif
    }
}