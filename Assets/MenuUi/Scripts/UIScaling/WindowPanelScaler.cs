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
            float unsafeAreaLine = 1 - CalculateUnsafeAreaHeight();
            float topLine = unsafeAreaLine - CalculateTopPanelHeight();
            _bottomPanelRectTransfrom.anchorMax = new(1,bottomLine);
            _unsafeAreaRectTransfrom.anchorMin = new(0, unsafeAreaLine);
            _topPanelRectTransfrom.anchorMin = new (0, topLine);
            _topPanelRectTransfrom.anchorMax = new (1, unsafeAreaLine);
            _contentPanelRectTransfrom.anchorMin = new (0, bottomLine);
            _contentPanelRectTransfrom.anchorMax = new(1, topLine);
            if(_fullPanelPopupsTransform != null) _fullPanelPopupsTransform.anchorMax = new Vector2(1, unsafeAreaLine);
        }
    }
}
