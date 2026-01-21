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
        [SerializeField] private Image _tabLineRibbon;
        [SerializeField] private Image _tabLineImage;
        [SerializeField] private Color _tabColor;

        private bool _lockActiveFromSwipe = false;

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
            foreach (TabLineButton button in _tabLineButtons)
            {
                if (_tabColor != Color.white) button.SetColour(_tabColor);
                else button.SetColour(_tabLineRibbon != null ? _tabLineRibbon.color : Color.white);
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
            if (_lockActiveFromSwipe) return;
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
            if (_tabLineImage != null)
            {
                if (image != null)
                {
                    _tabLineImage.sprite = image;
                    _tabLineImage.enabled = true;
                }
                else if (_tabLineImage.sprite == null) _tabLineImage.enabled = false;
            }

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
            [SerializeField] private TabObjectHandler _tabObjectHandler;
            [SerializeField] private Sprite _tablineImage;

            public Sprite SetActiveVisuals() => _tabObjectHandler.SetActiveVisuals(_tablineImage);
            public void SetInactiveVisuals() => _tabObjectHandler.SetInactiveVisuals();
            public void SetColour(Color colour) => _tabObjectHandler.SetColour(colour);
        }


        public void SetLockActiveFromSwipe(bool locked)
        {
            _lockActiveFromSwipe = locked;
        }
    }
}
