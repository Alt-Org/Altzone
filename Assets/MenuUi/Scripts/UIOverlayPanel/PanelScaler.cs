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
        const double LowestBottomPanelHeight = 0.15;
        const double HighestBottomPanelHeight = 0.23;

        // Percentage values for the top panel, lowest is for slim and tall phones and highest is for IPad aspect ratio.
        const double LowestTopPanelHeight = 0.05;
        const double HighestTopPanelHeight = 0.07;

        private void Awake()
        {
            _bottomPanelRectTransfrom.sizeDelta = new Vector2(0, CalculateBottomPanelHeight());
            _topPanelRectTransfrom.sizeDelta = new Vector2(0, CalculateTopPanelHeight());
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
            return (float)(Screen.height * bottomPanelHeightPercentage);
        }

        private float CalculateTopPanelHeight()
        {
            double topPanelHeightPercentage = (HighestTopPanelHeight + (LowestTopPanelHeight - HighestTopPanelHeight) * CalculateAspectRatioPercentage());
            return (float)(Screen.height * topPanelHeightPercentage);
        }
    }
}
