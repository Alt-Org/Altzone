using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.UIScaling
{
    [ExecuteAlways]
    public class WindowContentScaler : MonoBehaviour
    {
        [SerializeField] private RectTransform _headerPanelRectTransfrom;
        [SerializeField] private float _headerHeight = 300f;
        [SerializeField] private RectTransform _tablinePanelRectTransfrom;
        [SerializeField] private float _tablineHeight = 150f;
        [SerializeField] private RectTransform _contentPanelRectTransfrom;
        [SerializeField, Min(0)] private float _sideMarginPercent = 4.4f;

        private bool _isHeaderActive = false;
        private bool _isTablineActive = false;

        protected DrivenRectTransformTracker m_Tracker;

        void OnEnable()
        {
            m_Tracker.Add(this, _contentPanelRectTransfrom,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);
            m_Tracker.Add(this, _tablinePanelRectTransfrom,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);
            m_Tracker.Add(this, _headerPanelRectTransfrom,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);
        }

        void OnDisable()
        {
            m_Tracker.Clear();
        }

        // Update is called once per frame
        void Update()
        {
            CheckExistance();
        }

        private void CheckExistance()
        {
            if (_headerPanelRectTransfrom != null ? _headerPanelRectTransfrom.gameObject.activeSelf : false && !_isHeaderActive)
            {
                _isHeaderActive = true;
                _headerPanelRectTransfrom.anchorMax = Vector2.one;
                _headerPanelRectTransfrom.anchorMin = Vector2.up;
                _headerPanelRectTransfrom.offsetMin = new(0, -1 * _headerHeight);
            }
            else if(_headerPanelRectTransfrom != null ? true : !_headerPanelRectTransfrom.gameObject.activeSelf && _isHeaderActive) _isHeaderActive = false;
            if (_headerPanelRectTransfrom != null ? _tablinePanelRectTransfrom.gameObject.activeSelf : false && !_isTablineActive)
            {
                _isTablineActive = true;
                _tablinePanelRectTransfrom.anchorMax = Vector2.one;
                _tablinePanelRectTransfrom.anchorMin = Vector2.up;
                _tablinePanelRectTransfrom.offsetMax = new(0, -1 * (_isHeaderActive ? _headerHeight : 0));
                _tablinePanelRectTransfrom.offsetMin = new(0, -1 * ((_isHeaderActive ? _headerHeight : 0) + _tablineHeight));
            }
            else if (_tablinePanelRectTransfrom != null ? true : !_tablinePanelRectTransfrom.gameObject.activeSelf && _isTablineActive) _isTablineActive = false;
            float topMargin = 0f;
            if (_isHeaderActive) topMargin -= _headerHeight;
            if (_isTablineActive) topMargin -= _tablineHeight;
            _contentPanelRectTransfrom.anchorMax = new Vector2(1 - _sideMarginPercent / 100, 1);
            _contentPanelRectTransfrom.anchorMin = new Vector2(_sideMarginPercent / 100, 0);
            _contentPanelRectTransfrom.offsetMax = new(0,topMargin);
        }

    }
}
