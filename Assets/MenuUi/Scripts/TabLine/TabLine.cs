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
        [SerializeField] private Image _tabLineImage;

        private SwipeUI _swipe;

        public SwipeUI Swipe { get => _swipe;}

        private void OnEnable()
        {
            if (_swipe != null && _getActiveButtonFromSwipe)
            {
                ActivateTabButton(_swipe.CurrentPage);
            }
        }


        private void Awake()
        {
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
            if (index >= _tabLineButtons.Length || index < 0)
            {
                return;
            }

            Sprite image = _tabLineButtons[index].SetActiveVisuals();
            if (image != null)
            {
                _tabLineImage.sprite = image;
                _tabLineImage.enabled = true;
            }
            else if(_tabLineImage.sprite == null) _tabLineImage.enabled = false;

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
            [SerializeField] private Sprite _tablineImage;

            const float InactiveAlpha = 0.5f;

            public Sprite SetActiveVisuals()
            {
                if (_tabImageComponent != null)
                {
                    _tabImageComponent.color = new Color(_tabImageComponent.color.r, _tabImageComponent.color.g, _tabImageComponent.color.b, 1);
                }

                if (_detailImageComponent != null)
                {
                    _detailImageComponent.color = new Color(_tabImageComponent.color.r, _tabImageComponent.color.g, _tabImageComponent.color.b, 1);
                }
                return _tablineImage;
            }


            public void SetInactiveVisuals()
            {
                if (_tabImageComponent != null)
                {
                    _tabImageComponent.color = new Color(_tabImageComponent.color.r, _tabImageComponent.color.g, _tabImageComponent.color.b, InactiveAlpha);
                }

                if (_detailImageComponent != null)
                {
                    _detailImageComponent.color = new Color(_tabImageComponent.color.r, _tabImageComponent.color.g, _tabImageComponent.color.b, InactiveAlpha);
                }
            }
        }
    }
}
