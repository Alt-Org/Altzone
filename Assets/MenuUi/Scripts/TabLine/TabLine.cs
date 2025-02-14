using System;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.TabLine
{
    /// <summary>
    /// Is used to set active and inactive tab button visuals.
    /// </summary>
    public class TabLine : MonoBehaviour
    {
        [SerializeField] private bool _getActiveButtonFromSwipe = false;
        [SerializeField] private TabLineButton[] _tabLineButtons;

        private SwipeUI _swipe;


        private void Awake()
        {
            if (_getActiveButtonFromSwipe)
            {
                _swipe = FindObjectOfType<SwipeUI>();
                _swipe.OnCurrentPageChanged += OnSwipeCurrentPageChanged;
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


        [Serializable]
        private class TabLineButton
        {
            [Header("Sprite assets (detail sprites are optional)")]
            [SerializeField] private Sprite _activeTabSprite;
            [SerializeField] private Sprite _activeDetailSprite;
            [SerializeField] private Sprite _inactiveTabSprite;
            [SerializeField] private Sprite _inactiveDetailSprite;

            [Header("References to components")]
            [SerializeField] public Button ButtonComponent;
            [SerializeField] private Image _tabImageComponent;
            [SerializeField] private Image _detailImageComponent;


            public void SetActiveVisuals()
            {
                if (_tabImageComponent != null && _activeTabSprite != null)
                {
                    _tabImageComponent.sprite = _activeTabSprite;
                }

                if (_detailImageComponent != null && _activeDetailSprite != null)
                {
                    _detailImageComponent.sprite = _activeDetailSprite;
                }
            }


            public void SetInactiveVisuals()
            {
                if (_tabImageComponent != null && _inactiveTabSprite != null)
                {
                    _tabImageComponent.sprite = _inactiveTabSprite;
                }

                if (_detailImageComponent != null && _inactiveDetailSprite != null)
                {
                    _detailImageComponent.sprite = _inactiveDetailSprite;
                }
            }
        }
    }
}
