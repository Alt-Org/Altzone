using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.UIOverlayPanel
{
    public class PanelScaler : MonoBehaviour
    {
        [SerializeField] private RectTransform _bottomPanelRectTransfrom;
        [SerializeField] private RectTransform _topPanelRectTransfrom;

        // IPad aspect ratio and a tall and slim phone aspect ratio, used in math calculations.
        const double LowestAspectRatio = 4.0 / 3.0;
        const double HighestAspectRatio = 22.0 / 9.0;

        // Percentage values for the bottom panel, lowest is for slim and tall phones and highest is for IPad aspect ratio.
        const double LowestBottomPanelHeight = 0.18;
        const double HighestBottomPanelHeight = 0.25;

        // Percentage values for the top panel, lowest is for slim and tall phones and highest is for IPad aspect ratio.
        const double LowestTopPanelHeight = 0.05;
        const double HighestTopPanelHeight = 0.07;

        int _lastScreenWidth = 0;
        int _lastScreenHeight = 0;

        private void Awake()
        {
            SetPanelResolutions();
        }

#if (UNITY_EDITOR)
        void Update()
        {
            if (_lastScreenWidth != Screen.currentResolution.width || _lastScreenHeight != Screen.currentResolution.height)
            {
                _lastScreenWidth = Screen.currentResolution.width;
                _lastScreenHeight = Screen.currentResolution.height;
                SetPanelResolutions();
            }
        }
#endif

        private void SetPanelResolutions()
        {
            _bottomPanelRectTransfrom.anchorMax = new Vector2(1, CalculateBottomPanelHeight());
            _topPanelRectTransfrom.anchorMin = new Vector2(0, 1 - CalculateTopPanelHeight());
        }

        private double CalculateAspectRatioPercentage()
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

        private float CalculateBottomPanelHeight()
        {
            double bottomPanelHeightPercentage = (HighestBottomPanelHeight + (LowestBottomPanelHeight - HighestBottomPanelHeight) * CalculateAspectRatioPercentage());
            return (float)bottomPanelHeightPercentage;
        }

        private float CalculateTopPanelHeight()
        {
            double topPanelHeightPercentage = (HighestTopPanelHeight + (LowestTopPanelHeight - HighestTopPanelHeight) * CalculateAspectRatioPercentage());
            return (float)topPanelHeightPercentage;
        }
    }
}
