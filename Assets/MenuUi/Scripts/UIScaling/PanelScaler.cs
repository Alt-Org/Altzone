using UnityEngine;

namespace MenuUi.Scripts.UIScaling
{
    public class PanelScaler : MonoBehaviour
    {
        [SerializeField] protected RectTransform _bottomPanelRectTransfrom;
        [SerializeField] protected RectTransform _topPanelRectTransfrom;
        [SerializeField] protected RectTransform _unsafeAreaRectTransfrom;

        // IPad aspect ratio and a tall and slim phone aspect ratio, used in math calculations.
        const double LowestAspectRatio = 4.0 / 3.0;
        const double HighestAspectRatio = 22.0 / 9.0;

        // Percentage values for the bottom panel, lowest is for slim and tall phones and highest is for IPad aspect ratio.
        const double LowestBottomPanelHeight = 0.18;
        const double HighestBottomPanelHeight = 0.25;

        // Percentage values for the top panel, lowest is for slim and tall phones and highest is for IPad aspect ratio.
        const double LowestTopPanelHeight = 0.07;
        const double HighestTopPanelHeight = 0.09;

        int _lastScreenWidth = 0;
        int _lastScreenHeight = 0;

        private void Awake()
        {
            SetPanelAnchors();
        }

#if (UNITY_EDITOR)
        void Update()
        {
            if (_lastScreenWidth != Screen.currentResolution.width || _lastScreenHeight != Screen.currentResolution.height)
            {
                _lastScreenWidth = Screen.currentResolution.width;
                _lastScreenHeight = Screen.currentResolution.height;
                SetPanelAnchors();
            }
        }
#endif

        protected virtual void SetPanelAnchors()
        {
            _bottomPanelRectTransfrom.anchorMax = new Vector2(1, CalculateBottomPanelHeight());

            _topPanelRectTransfrom.anchorMin = new Vector2(0, 1 - (CalculateTopPanelHeight() + CalculateUnsafeAreaHeight()));
            _topPanelRectTransfrom.anchorMax = new Vector2(1, 1 - CalculateUnsafeAreaHeight());

            _unsafeAreaRectTransfrom.anchorMin = new Vector2(0, 1 - CalculateUnsafeAreaHeight());
        }

        private static double CalculateAspectRatioPercentage()
        {
            double aspectRatio = (double)Screen.currentResolution.height / Screen.currentResolution.width;

            if (aspectRatio < LowestAspectRatio)
            {
                aspectRatio = LowestAspectRatio;
            }
            else if (aspectRatio > HighestAspectRatio)
            {
                aspectRatio = HighestAspectRatio;
            }

            return (aspectRatio - LowestAspectRatio) / (HighestAspectRatio - LowestAspectRatio);
        }

        /// <summary>
        /// Calculate UI bottom panel height depending on the phone's aspect ratio.
        /// </summary>
        /// <returns>Bottom panel height percentage in decimal format.</returns>
        public static float CalculateBottomPanelHeight()
        {
            double bottomPanelHeightPercentage = (HighestBottomPanelHeight + (LowestBottomPanelHeight - HighestBottomPanelHeight) * CalculateAspectRatioPercentage());
            return (float)bottomPanelHeightPercentage;
        }

        /// <summary>
        /// Calculate UI top panel height depending on the phone's aspect ratio.
        /// </summary>
        /// <returns>Top panel height percentage in decimal format.</returns>
        public static float CalculateTopPanelHeight()
        {
            double topPanelHeightPercentage = (HighestTopPanelHeight + (LowestTopPanelHeight - HighestTopPanelHeight) * CalculateAspectRatioPercentage());
            return (float)topPanelHeightPercentage;
        }

        /// <summary>
        /// Calculate unsafe area height for the phone resolution.
        /// </summary>
        /// <returns>Unsafe area height percentage in decimal format.</returns>
        public static float CalculateUnsafeAreaHeight()
        {
            if (!AppPlatform.IsMobile && !AppPlatform.IsSimulator) // if running on non-mobile device which is not unity simulator
            {
                return 0;
            }

            // for iphones there is unsafe area at bottom as well so the calculation in else branch gives too large of a gap at the top
            // and for some phones yMin was 0 so the first calculation wouldn't work, that's why there is an if statement
            if (Screen.safeArea.yMin != 0) 
            {
                return (float)(Screen.safeArea.yMin / Screen.currentResolution.height);
            }
            else
            {
                return (float)((Screen.currentResolution.height - Screen.safeArea.height) / Screen.currentResolution.height);
            }
        }
    }
}
