using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.UIScaling
{
    [ExecuteAlways]
    public class WindowPanelScaler : PanelScaler
    {
        [SerializeField] private RectTransform _contentPanelRectTransfrom;


        protected override void SetPanelAnchors()
        {
            float bottomLine = CalculateBottomPanelHeight();
            float topLine = 1-CalculateTopPanelHeight();
            _bottomPanelRectTransfrom.anchorMax = new(1,bottomLine);
            _topPanelRectTransfrom.anchorMin = new (0, topLine);
            _contentPanelRectTransfrom.anchorMin = new (0, bottomLine);
            _contentPanelRectTransfrom.anchorMax = new(1, topLine);
        }
    }
}
