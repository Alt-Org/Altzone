using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.Input
{
    /// <summary>
    /// Sends <c>ClickDownObjectEvent</c> when user clicks on eligible UI element.
    /// </summary>
    /// <remarks>
    /// Eligible means that UI element is marked given with tag or layer for these events.
    /// </remarks>
    public class ClickDownListener : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private Camera _camera;
        [SerializeField] private string _clickableTagName;
        [SerializeField] private LayerMask _clickableLayers;

        [Header("Debug"), SerializeField] private int _layerMask;

        private void OnEnable()
        {
            _layerMask = _clickableLayers.value;
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            this.Subscribe<InputManager.ClickDownEvent>(OnClickDownEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnClickDownEvent(InputManager.ClickDownEvent data)
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
            var layer = hitObject.layer;
            var hasLayer = _layerMask == (_layerMask | (1 << layer)); // unity3d check if layer mask contains layer
            if (hasLayer)
            {
                //Debug.Log($"CLICK {hitObject.GetFullPath()} tag {hitObject.tag} layer {layer} {LayerMask.LayerToName(layer)}");
                this.Publish(new ClickDownObjectEvent(data.ScreenPosition, hitObject));
                return;
            }
            var hasTag = !string.IsNullOrEmpty(_clickableTagName) && hitObject.CompareTag(_clickableTagName);
            if (hasTag)
            {
                //Debug.Log($"CLICK {hitObject.GetFullPath()} tag {hitObject.tag} layer {layer} {LayerMask.LayerToName(layer)}");
                this.Publish(new ClickDownObjectEvent(data.ScreenPosition, hitObject));
            }
        }

        public class ClickDownObjectEvent
        {
            public readonly Vector2 ScreenPosition;
            public readonly GameObject GameObject;

            public ClickDownObjectEvent(Vector2 screenPosition, GameObject gameObject)
            {
                ScreenPosition = screenPosition;
                GameObject = gameObject;
            }

            public override string ToString()
            {
                return $"ClickDown {nameof(ScreenPosition)}: {ScreenPosition}, {nameof(GameObject)}: {GameObject.GetFullPath()}";
            }
        }
    }
}