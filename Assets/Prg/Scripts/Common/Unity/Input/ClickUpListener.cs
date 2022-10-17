using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.Input
{
    /// <summary>
    /// Sends <c>ClickUpObjectEvent</c> when user clicks on eligible UI element.
    /// </summary>
    /// <remarks>
    /// Eligible means that UI element is marked given with tag or layer for these events.
    /// </remarks>
    public class ClickUpListener : MonoBehaviour
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
            this.Subscribe<InputManager.ClickUpEvent>(OnClickUpEvent);
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
            var isValid = _layerMask == (_layerMask | (1 << layer)); // unity3d check if layer mask contains layer
            if (!isValid)
            {
                isValid = !string.IsNullOrEmpty(_clickableTagName) && hitObject.CompareTag(_clickableTagName);
            }
            if (!isValid)
            {
                return;
            }
            //Debug.Log($"CLICK {hitObject.GetFullPath()} tag {hitObject.tag} layer {layer} {LayerMask.LayerToName(layer)} DOWN");
            this.Publish(new ClickUpObjectEvent(ClickUpPhase.ClickUpStart, data.ScreenPosition, hitObject));
        }

        private void OnClickUpEvent(InputManager.ClickUpEvent data)
        {
            var ray = _camera.ScreenPointToRay(data.ScreenPosition);
            var hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider == null)
            {
                return;
            }
            var hitObject = hit.collider.gameObject;
            var layer = hitObject.layer;
            var isValid = _layerMask == (_layerMask | (1 << layer)); // unity3d check if layer mask contains layer
            if (!isValid)
            {
                isValid = !string.IsNullOrEmpty(_clickableTagName) && hitObject.CompareTag(_clickableTagName);
            }
            if (!isValid)
            {
                return;
            }
            //Debug.Log($"CLICK {hitObject.GetFullPath()} tag {hitObject.tag} layer {layer} {LayerMask.LayerToName(layer)} UP");
            this.Publish(new ClickUpObjectEvent(ClickUpPhase.ClickUpEnd, data.ScreenPosition, hitObject));
        }

        public enum ClickUpPhase
        {
            ClickUpStart,
            ClickUpEnd
        }

        public class ClickUpObjectEvent
        {
            public readonly float Time;
            public readonly ClickUpPhase Phase;
            public readonly Vector2 ScreenPosition;
            public readonly GameObject GameObject;

            public ClickUpObjectEvent(ClickUpPhase phase, Vector2 screenPosition, GameObject gameObject)
            {
                Time = UnityEngine.Time.time;
                Phase = phase;
                ScreenPosition = screenPosition;
                GameObject = gameObject;
            }

            public override string ToString()
            {
                return $"ClickUp {nameof(ScreenPosition)}: {ScreenPosition} {GameObject.GetFullPath()} time {Time} {Phase}";
            }
        }
    }
}