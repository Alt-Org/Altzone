using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using UnityEngine;

namespace MenuUi.Scripts.UIScaling
{
    [ExecuteAlways]
    public class WindowPanelScaler : PanelScaler
    {
        [SerializeField] private RectTransform _contentPanelRectTransfrom;
        [SerializeField] private bool _ignoreOverlayStatus = false;

        protected override IEnumerator SetPanelAnchors()
        {
            yield return new WaitForEndOfFrame();
            float bottomLine = CalculateBottomPanelHeight();
            if (!_ignoreOverlayStatus && Application.isPlaying && (OverlayPanelCheck.Instance == null || !OverlayPanelCheck.Instance.BottomBarActive)) bottomLine = 0;
            else if (!_ignoreOverlayStatus && OverlayPanelCheck.Instance && !OverlayPanelCheck.Instance.ChatActive) bottomLine /= 2f;
            float unsafeAreaLine = 1 - CalculateUnsafeAreaHeight();
            float topLine;
            if (!_ignoreOverlayStatus && Application.isPlaying && (OverlayPanelCheck.Instance == null || !OverlayPanelCheck.Instance.TopBarActive)) topLine = unsafeAreaLine;
            else topLine = unsafeAreaLine - CalculateTopPanelHeight();
            _bottomPanelRectTransfrom.anchorMax = new(1,bottomLine);
            _unsafeAreaRectTransfrom.anchorMin = new(0, unsafeAreaLine);
            _topPanelRectTransfrom.anchorMin = new (0, topLine);
            _topPanelRectTransfrom.anchorMax = new (1, unsafeAreaLine);
            _contentPanelRectTransfrom.anchorMin = new (0, bottomLine);
            _contentPanelRectTransfrom.anchorMax = new(1, topLine);
            if(_fullPanelPopupsTransform != null) _fullPanelPopupsTransform.anchorMax = new Vector2(1, unsafeAreaLine);
        }

        protected override void UpdateBottomLineChat(bool value)
        {
            if (_ignoreOverlayStatus) return;
            float bottomLine = CalculateBottomPanelHeight();
            if (!value) bottomLine /= 2f;
            _bottomPanelRectTransfrom.anchorMax = new(1, bottomLine);
            _contentPanelRectTransfrom.anchorMin = new(0, bottomLine);
        }
        protected override void UpdateBottomLine(bool value)
        {
            if (_ignoreOverlayStatus) return;
            float bottomLine = CalculateBottomPanelHeight();
            if (!value) bottomLine = 0;
            _bottomPanelRectTransfrom.anchorMax = new(1, bottomLine);
            _contentPanelRectTransfrom.anchorMin = new(0, bottomLine);
        }

        protected override void UpdateTopLine(bool value)
        {
            if (_ignoreOverlayStatus) return;
            float topLine = 1 - (CalculateTopPanelHeight() + CalculateUnsafeAreaHeight());
            if (!value) topLine = 1 - CalculateUnsafeAreaHeight();
            _topPanelRectTransfrom.anchorMin = new(0, topLine);
            _contentPanelRectTransfrom.anchorMax = new(1, topLine);
        }
    }
}
