using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.Input
{
    public class ClickListenerTimed : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private Camera _camera;
        [SerializeField] private string _clickableTagName;
        [SerializeField] private LayerMask _clickableLayers;
        [Min(0), SerializeField] private float _timerInterval = 0.1f;

        [Header("Debug"), SerializeField] private float _timerStartTime;
        [SerializeField] private float _timerFireEventTime;
        [SerializeField] private int _eventId;
        [SerializeField] private int _layerMask;

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
            if (isValid)
            {
                if (data.ClickCount == 1)
                {
                    _eventId += 1;
                    _timerStartTime = Time.time;
                    _timerFireEventTime = 0;
                }
                else
                {
                    _timerFireEventTime += Time.deltaTime;
                    if (_timerFireEventTime < _timerInterval)
                    {
                        return;
                    }
                    _timerFireEventTime = 0;
                }
                var duration = Time.time - _timerStartTime;
                Debug.Log($"CLICK {hitObject.GetFullPath()} tag {hitObject.tag} layer {layer} {LayerMask.LayerToName(layer)} " +
                          $"id {_eventId} dur {duration:0.00}");
                this.Publish(new ClickObjectTimedEvent(_eventId, data.ScreenPosition, hitObject, duration));
            }
        }

        public class ClickObjectTimedEvent
        {
            public readonly int EventId;
            public readonly Vector2 ScreenPosition;
            public readonly GameObject GameObject;
            public readonly float Duration;

            public ClickObjectTimedEvent(int eventId, Vector2 screenPosition, GameObject gameObject, float duration)
            {
                EventId = eventId;
                ScreenPosition = screenPosition;
                GameObject = gameObject;
                Duration = duration;
            }

            public override string ToString()
            {
                return $"{nameof(ScreenPosition)}: #{EventId} {ScreenPosition} {GameObject.GetFullPath()} dur {Duration:0.00}";
            }
        }
    }
}