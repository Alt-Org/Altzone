using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.Input
{
    public class ClickListener : MonoBehaviour
    {
        [Header("Settings"),SerializeField]  private Camera _camera;
        [SerializeField] private string clickableTagName;
        [SerializeField] private LayerMask clickableLayers;

        [Header("Debug"),SerializeField]  private int layerMask;

        private void OnEnable()
        {
            layerMask = clickableLayers.value;
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            this.Subscribe<InputManager.ClickDownEvent>(onClickDownEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe<InputManager.ClickDownEvent>(onClickDownEvent);
        }

        private void onClickDownEvent(InputManager.ClickDownEvent data)
        {
            if (data.ClickCount > 1)
            {
                return;
            }
            var ray = _camera.ScreenPointToRay(data.ScreenPosition);
            var hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider == null)
            {
                return;
            }
            var hitObject = hit.collider.gameObject;
            var _layer = hitObject.layer;
            var hasTag = !string.IsNullOrEmpty(clickableTagName) && hitObject.CompareTag(clickableTagName);
            var hasLayer = layerMask == (layerMask | (1 << _layer)); // unity3d check if layer mask contains layer

            //Debug.Log($"CLICK {hitObject.GetFullPath()} tag {hitObject.tag} ({hasTag}) layer {_layer} {LayerMask.LayerToName(_layer)} ({hasLayer})");
            if (hasTag || hasLayer)
            {
                this.Publish(new ClickObjectEvent(data.ScreenPosition, hitObject));
            }
        }

        public class ClickObjectEvent
        {
            public readonly Vector3 ScreenPosition;
            public readonly GameObject GameObject;

            public ClickObjectEvent(Vector3 screenPosition, GameObject gameObject)
            {
                ScreenPosition = screenPosition;
                GameObject = gameObject;
            }
        }
    }
}