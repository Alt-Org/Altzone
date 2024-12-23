using UnityEngine;

namespace MenuUi.Scripts.UIScaling
{
    public class ContentScaler : MonoBehaviour
    {
        [SerializeField] private RectTransform _contentRectTransfrom;

        int _lastScreenWidth = 0;
        int _lastScreenHeight = 0;

        private void Awake()
        {
            SetContentAnchors();
        }

#if (UNITY_EDITOR)
        void Update()
        {
            if (_lastScreenWidth != Screen.currentResolution.width || _lastScreenHeight != Screen.currentResolution.height)
            {
                _lastScreenWidth = Screen.currentResolution.width;
                _lastScreenHeight = Screen.currentResolution.height;
                SetContentAnchors();
            }
        }
#endif

        private void SetContentAnchors()
        {
            _contentRectTransfrom.anchorMin = new Vector2(0, PanelScaler.CalculateBottomPanelHeight());
            _contentRectTransfrom.anchorMax = new Vector2(1, 1 - PanelScaler.CalculateTopPanelHeight());
        }
    }
}
