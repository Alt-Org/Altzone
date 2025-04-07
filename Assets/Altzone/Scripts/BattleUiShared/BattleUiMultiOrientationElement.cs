using UnityEngine;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Handles setting and getting the Battle Ui multi orientation element position and orientation.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMultiOrientationElement : BattleUiMovableElement
    {
        public enum OrientationType
        {
            None = -1,
            Horizontal = 0,
            HorizontalFlipped = 1,
            Vertical = 2,
            VerticalFlipped = 3,
        }

        [SerializeField] private GameObject _horizontalConfiguration;
        [SerializeField] private float _horizontalAspectRatio;
        [SerializeField] private GameObject _verticalConfiguration;
        [SerializeField] private float _verticalAspectRatio;

        private OrientationType _orientation;
        public OrientationType Orientation => _orientation;

        public bool IsHorizontal
        {
            get
            {
                return _orientation == OrientationType.Horizontal || _orientation == OrientationType.HorizontalFlipped;
            }
        }

        private void SetOrientation(OrientationType newOrientation)
        {
            // If orientation is same we don't have to do anything
            if (_orientation == newOrientation) return;

            // If we had flipped orientation resetting the flip, Horizontal and Vertical are the default configurations
            switch (_orientation)
            {
                case OrientationType.HorizontalFlipped:
                case OrientationType.VerticalFlipped:
                    FlipOrientation();
                    break;
            }

            // Setting new orientation value
            _orientation = newOrientation;

            // Showing either horizontal or vertical configuration
            if (_horizontalConfiguration != null) _horizontalConfiguration.SetActive(IsHorizontal);
            if (_verticalConfiguration != null) _verticalConfiguration.SetActive(!IsHorizontal);

            // Flipping orientation if new orientation is flipped
            switch (newOrientation)
            {
                case OrientationType.HorizontalFlipped:
                case OrientationType.VerticalFlipped:
                    FlipOrientation();
                    break;
            }
        }

        private void FlipOrientation()
        {
            if (_orientation == OrientationType.Horizontal || _orientation == OrientationType.HorizontalFlipped)
            {
                if (_horizontalConfiguration == null) return;

                // Repositioning every horizontal configuration child anchor
                for (int i = 0; i < _horizontalConfiguration.transform.childCount; i++)
                {
                    Transform child = _horizontalConfiguration.transform.GetChild(i);
                    RectTransform childRectTransform = child.GetComponent<RectTransform>();
                    if (childRectTransform != null)
                    {
                        FlipHorizontally(childRectTransform);
                    }
                }
            }
            else if (_orientation == OrientationType.Vertical || _orientation == OrientationType.VerticalFlipped)
            {
                if (_verticalConfiguration == null) return;

                // Repositioning every vertical configuration child anchor
                for (int i = 0; i < _verticalConfiguration.transform.childCount; i++)
                {
                    Transform child = _verticalConfiguration.transform.GetChild(i);
                    RectTransform childRectTransform = child.GetComponent<RectTransform>();
                    if (childRectTransform != null)
                    {
                        FlipHorizontally(childRectTransform);
                    }
                }
            }
        }

        private void FlipHorizontally(RectTransform childRectTransform)
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

        private float GetFlippedAnchor(float anchorValue)
        {
            return -anchorValue + 1;
        }

        /// <summary>
        /// Get the currently active gameobject.
        /// </summary>
        /// <returns>GameObject which is either the horizontal or vertical configuration.</returns>
        public GameObject GetActiveGameObject()
        {
            if (IsHorizontal)
            {
                return _horizontalConfiguration;
            }
            else
            {
                return _verticalConfiguration;
            }
        }

        /// <summary>
        /// Set BattleUiMovableElementData to this Ui multi orientation element.
        /// </summary>
        /// <param name="data">The data which to set to this Ui element.</param>
        public override void SetData(BattleUiMovableElementData data)
        {
            base.SetData(data);
            SetOrientation(data.Orientation);
        }

        /// <summary>
        /// Get the data from this Ui multi orientation element.
        /// </summary>
        /// <returns>Returns BattleUiMovableElementData serializable object. Null if couldn't get valid data.</returns>
        public override BattleUiMovableElementData GetData()
        {
            if (_rectTransform != null)
            {
                return new BattleUiMovableElementData(_rectTransform.anchorMin, _rectTransform.anchorMax, _orientation);
            }
            else
            {
                return null;
            }
        }
    }
}

