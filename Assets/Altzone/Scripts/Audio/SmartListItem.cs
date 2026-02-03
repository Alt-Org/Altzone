using UnityEngine;
using UnityEngine.EventSystems;

namespace Altzone.Scripts.Audio
{
    public class SmartListItem : MonoBehaviour
    {
        //Obsolete?
        private float _yStartPosition;
        protected RectTransform _selfRectTransform;

        //Obsolete?
        public float YStartPosition  { get { return _yStartPosition; } }
        public RectTransform SelfRectTransform  { get { return _selfRectTransform; } }

        public bool Visible  { get { return gameObject.activeSelf; } }

        public SmartListItem() { }

        public SmartListItem(float yStartPosition, RectTransform selfRectTransform)
        {
            _yStartPosition = yStartPosition;
            _selfRectTransform = selfRectTransform;
        }

        protected static bool CheckClassType<T1, T2>(T1 data1)
        {
            if (data1 is T2) return true;

            Debug.LogError("Error: data is of type: " + data1.GetType());
            return false;
        }

        public void UpdateStartPosition() { if (_selfRectTransform != null) _yStartPosition = _selfRectTransform.position.y; }

        public void UpdateStartPosition(float yStartPosition) { _yStartPosition = yStartPosition; }

        public virtual void SetVisibility(bool visible) { gameObject.SetActive(visible); /*Debug.LogError(visible ? "Visible" : "Invisible");*/ }

        public virtual void SetData<T1>(T1 data1) { }

        public virtual void ClearData() { }
    }
}
