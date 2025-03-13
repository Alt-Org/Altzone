using System;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.TabLine
{
    /// <summary>
    /// Is used to set active and inactive tab button visuals. Also has a method which can be called to change swipe current page.
    /// </summary>
    public class TabLine : MonoBehaviour
    {
        [SerializeField] private bool _getActiveButtonFromSwipe = false;
        [SerializeField] private TabLineButton[] _tabLineButtons;

        private SwipeUI _swipe;


        private void Awake()
        {
            foreach (TabLineButton button in _tabLineButtons)
            {
                button.SetImageRectTransform();
            }

            if (_getActiveButtonFromSwipe)
            {
                _swipe = FindObjectOfType<SwipeUI>();
                _swipe.OnCurrentPageChanged += OnSwipeCurrentPageChanged;
                ActivateTabButton(_swipe.CurrentPage);
            }
            else
            {
                ActivateTabButton(0);
            }
        }


        private void OnDestroy()
        {
            if (_getActiveButtonFromSwipe)
            {
                _swipe.OnCurrentPageChanged -= OnSwipeCurrentPageChanged;
            }
        }


        private void OnSwipeCurrentPageChanged()
        {
            ActivateTabButton(_swipe.CurrentPage);
        }


        /// <summary>
        /// Sets the tab button at the index active and others inactive.
        /// </summary>
        /// <param name="index">The index in tab line buttons array.</param>
        public void ActivateTabButton(int index)
        {
            // Check if enough tab button entries in array.
            if (_tabLineButtons.Length - 1 < index)
            {
                return;
            }

            _tabLineButtons[index].SetActiveVisuals();

            for (int i = 0; i < _tabLineButtons.Length; i++)
            {
                if (i == index) continue;

                _tabLineButtons[i].SetInactiveVisuals();
            }
        }


        /// <summary>
        /// Changes the current page for swipe. Works if getting active button from swipe is toggled on.
        /// </summary>
        /// <param name="index">The index which to change the swipe current page to.</param>
        public void SetSwipeCurrentPage(int index)
        {
            if (_swipe != null && _getActiveButtonFromSwipe)
            {
                _swipe.CurrentPage = index;
            }
        }


        [Serializable]
        private class TabLineButton
        {
            [Header("References to components")]
            [SerializeField] private Image _tabImageComponent;
            [SerializeField] private Image _detailImageComponent;

            private RectTransform _imageRectTransform;
            private RectTransform _detailImageRectTransform;

            private const float OffsetAmount = -20.0f;

            private Vector2 _offset = new Vector2(0, OffsetAmount);

            public void SetImageRectTransform()
            {
                _imageRectTransform = _tabImageComponent.gameObject.GetComponent<RectTransform>();
                if (_detailImageComponent != null ) _detailImageRectTransform = _tabImageComponent.gameObject.GetComponent<RectTransform>();
            }


            public void SetActiveVisuals()
            {
                _imageRectTransform.offsetMin = Vector2.zero;
                _imageRectTransform.offsetMax = Vector2.zero;

                if (_detailImageComponent != null )
                {
                    _detailImageRectTransform.offsetMin = Vector2.zero;
                    _detailImageRectTransform.offsetMax = Vector2.zero;
                }
            }


            public void SetInactiveVisuals()
            {
                
                _imageRectTransform.offsetMin = _offset;
                _imageRectTransform.offsetMax = _offset;

                if (_detailImageComponent != null)
                {
                    _detailImageRectTransform.offsetMin = _offset;
                    _detailImageRectTransform.offsetMax = _offset;
                }
            }
        }
    }
}
