using UnityEngine;

namespace Altzone.Scripts.BattleUi
{
    public enum BattleUiElementOrientation
    {
        Horizontal = 0,
        HorizontalFlipped = 1,
        Vertical = 2,
        VerticalFlipped = 3,
    }

    /// <summary>
    /// Handles setting and getting the Battle Ui element position and orientation.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiElement : MonoBehaviour
    {
        [SerializeField] private GameObject _horizontalConfiguration;
        [SerializeField] private float _horizontalAspectRatio;
        [SerializeField] private GameObject _verticalConfiguration;
        [SerializeField] private float _verticalAspectRatio;

        private RectTransform _rectTransform;

        private BattleUiElementOrientation _orientation;
        public BattleUiElementOrientation Orientation
        {
            get
            {
                return _orientation;
            }
            set
            {
                // If orientation is same we don't have to do anything
                if (_orientation == value) return;

                // If we had flipped orientation resetting the flip, Horizontal and Vertical are the default configurations
                switch (_orientation)
                {
                    case BattleUiElementOrientation.HorizontalFlipped:
                    case BattleUiElementOrientation.VerticalFlipped:
                        FlipOrientation();
                        break;
                }

                // Setting new orientation value
                _orientation = value;

                // Showing either horizontal or vertical configuration
                if (_horizontalConfiguration != null) _horizontalConfiguration.SetActive(IsHorizontal);
                if (_verticalConfiguration != null) _verticalConfiguration.SetActive(!IsHorizontal);

                // Flipping orientation if new orientation is flipped
                switch (value)
                {
                    case BattleUiElementOrientation.HorizontalFlipped:
                    case BattleUiElementOrientation.VerticalFlipped:
                        FlipOrientation();
                        break;
                }
            }
        }

        public bool IsHorizontal
        {
            get
            {
                return _orientation == BattleUiElementOrientation.Horizontal || _orientation == BattleUiElementOrientation.HorizontalFlipped;
            }
        }

        private void Awake()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        }

        private void FlipOrientation()
        {
            if (_orientation == BattleUiElementOrientation.Horizontal || _orientation == BattleUiElementOrientation.HorizontalFlipped)
            {
                if (_horizontalConfiguration == null) return;

                // Repositioning every horizontal configuration child anchor
                for (int i = 0; i < _horizontalConfiguration.transform.childCount; i++)
                {
                    Transform child = _horizontalConfiguration.transform.GetChild(i);
                    RectTransform childRectTransform = child.GetComponent<RectTransform>();
                    if (childRectTransform != null)
                    {
                        // Calculating flipped x anchors
                        float flippedXMin = GetFlippedAnchor(childRectTransform.anchorMax.x); // we have to get the value from anchorMax so that it works
                        float flippedXMax = GetFlippedAnchor(childRectTransform.anchorMin.x); // same here we have to get it from anchorMin

                        // Setting new x anchors
                        childRectTransform.anchorMin = new Vector2(flippedXMin, childRectTransform.anchorMin.y);
                        childRectTransform.anchorMax = new Vector2(flippedXMax, childRectTransform.anchorMax.y);

                        // Resetting offset values in case they changed
                        childRectTransform.offsetMin = Vector2.zero;
                        childRectTransform.offsetMax = Vector2.zero;
                    }
                }
            }
            else if (_orientation == BattleUiElementOrientation.Vertical || _orientation == BattleUiElementOrientation.VerticalFlipped)
            {
                if (_verticalConfiguration == null) return;

                // Repositioning every vertical configuration child anchor
                for (int i = 0; i < _verticalConfiguration.transform.childCount; i++)
                {
                    Transform child = _verticalConfiguration.transform.GetChild(i);
                    RectTransform childRectTransform = child.GetComponent<RectTransform>();
                    if (childRectTransform != null)
                    {
                        // Calculating flipped y anchors
                        float flippedYMin = GetFlippedAnchor(childRectTransform.anchorMax.y);
                        float flippedYMax = GetFlippedAnchor(childRectTransform.anchorMin.y);

                        // Setting new y anchors
                        childRectTransform.anchorMin = new Vector2(childRectTransform.anchorMin.x, flippedYMin);
                        childRectTransform.anchorMax = new Vector2(childRectTransform.anchorMax.x, flippedYMax);

                        // Resetting offset values in case they changed
                        childRectTransform.offsetMin = Vector2.zero;
                        childRectTransform.offsetMax = Vector2.zero;
                    }
                }
            }
        }

        private float GetFlippedAnchor(float anchorValue)
        {
            return -anchorValue + 1;
        }

        /// <summary>
        /// Set BattleUiElementData to this Ui element.
        /// </summary>
        /// <param name="data">The data which to set to this Ui element.</param>
        public void SetData(BattleUiElementData data)
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            _rectTransform.anchorMin = data.AnchorMin;
            _rectTransform.anchorMax = data.AnchorMax;

            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            Orientation = data.Orientation;
        }

        /// <summary>
        /// Get the data from this Ui element.
        /// </summary>
        /// <returns>Returns BattleUiElementData serializable object. Null if couldn't get valid data.</returns>
        public BattleUiElementData GetData()
        {
            if (_rectTransform != null)
            {
                return new BattleUiElementData(_rectTransform.anchorMin, _rectTransform.anchorMax, _orientation);
            }
            else
            {
                return null;
            }
        }
    }
}

