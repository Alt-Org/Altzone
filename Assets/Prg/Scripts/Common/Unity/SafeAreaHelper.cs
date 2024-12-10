using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// Resizes this <c>RectTransform</c> to be 'inside' UNITY Screen.safeArea.<br />
    /// Code from UnitySafeAreaHelper: https://github.com/howtungtung/UnitySafeAreaHelper<br />
    /// Safe Area Helper: https://assetstore.unity.com/packages/tools/gui/safe-area-helper-130488
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHelper : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Rect _lastSafeArea;

        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnDisable()
        {
            var fullScreen = new Rect(0, 0, Screen.width, Screen.height);
            _lastSafeArea = fullScreen;
            Refresh();
        }

        private void LateUpdate()
        {
            if (_lastSafeArea != Screen.safeArea)
            {
                _lastSafeArea = Screen.safeArea;
                Refresh();
            }
        }

        private void Refresh()
        {
            var anchorMin = _lastSafeArea.position;
            var anchorMax = _lastSafeArea.position + _lastSafeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    }
}
