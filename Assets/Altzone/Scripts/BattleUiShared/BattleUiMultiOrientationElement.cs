using UnityEngine;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Handles setting and getting the Battle Ui multi orientation element position and orientation.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMultiOrientationElement : BattleUiMovableElement
    {
        [SerializeField] private GameObject _horizontalConfiguration;
        [SerializeField] private float _horizontalAspectRatio;
        [SerializeField] private GameObject _verticalConfiguration;
        [SerializeField] private float _verticalAspectRatio;

        public enum OrientationType
        {
            None = -1,
            Horizontal = 0,
            Vertical = 1,
        }

        public OrientationType Orientation => _orientation;
        public GameObject HorizontalConfiguration => _horizontalConfiguration;
        public GameObject VerticalConfiguration => _verticalConfiguration;
        public bool IsFlippedHorizontally => _isFlippedHorizontally;
        public bool IsFlippedVertically => _isFlippedVertically;
        public float HorizontalAspectRatio => _horizontalAspectRatio;
        public float VerticalAspectRatio => _verticalAspectRatio;

        public bool IsHorizontal
        {
            get
            {
                return _orientation == OrientationType.Horizontal;
            }
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
            SetOrientation(data);
        }

        /// <summary>
        /// Get the data from this Ui multi orientation element.
        /// </summary>
        /// <returns>Returns BattleUiMovableElementData serializable object. Null if couldn't get valid data.</returns>
        public override BattleUiMovableElementData GetData()
        {
            if (_rectTransform != null)
            {
                return new BattleUiMovableElementData(_rectTransform.anchorMin, _rectTransform.anchorMax, _orientation, IsFlippedHorizontally, IsFlippedVertically);
            }
            else
            {
                return null;
            }
        }

        private OrientationType _orientation = OrientationType.Horizontal;
        private bool _isFlippedHorizontally = false;
        private bool _isFlippedVertically = false;

        private void SetOrientation(BattleUiMovableElementData data)
        {
            // If orientation and flip is same we don't have to do anything
            if (_orientation == data.Orientation &&
                _isFlippedHorizontally == data.IsFlippedHorizontally &&
                _isFlippedVertically == data.IsFlippedVertically) return;

            // Setting new orientation value
            _orientation = data.Orientation;

            // Showing either horizontal or vertical configuration
            if (_horizontalConfiguration != null) _horizontalConfiguration.SetActive(IsHorizontal);
            if (_verticalConfiguration != null) _verticalConfiguration.SetActive(!IsHorizontal);

            // Flipping orientation if data flip status is different
            if (data.IsFlippedHorizontally != _isFlippedHorizontally)
            {
                if (_horizontalConfiguration != null) FlipChildrenHorizontally(_horizontalConfiguration.transform);
                if (_verticalConfiguration != null) FlipChildrenHorizontally(_verticalConfiguration.transform);
            }

            if (data.IsFlippedVertically != _isFlippedVertically)
            {
                if (_horizontalConfiguration != null) FlipChildrenVertically(_horizontalConfiguration.transform);
                if (_verticalConfiguration != null) FlipChildrenVertically(_verticalConfiguration.transform);
            }

            // Setting new flip value
            _isFlippedHorizontally = data.IsFlippedHorizontally;
            _isFlippedVertically = data.IsFlippedVertically;
        }

        private void FlipChildrenHorizontally(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                RectTransform childRectTransform = parent.GetChild(i).GetComponent<RectTransform>();

                if (childRectTransform == null) return;

                // Calculating flipped x anchors
                float flippedXMin = GetFlippedAnchor(childRectTransform.anchorMax.x); // we have to get the value from anchorMax so that it works
                float flippedXMax = GetFlippedAnchor(childRectTransform.anchorMin.x); // same here we have to get it from anchorMin

                // Setting new x anchors
                childRectTransform.anchorMin = new Vector2(flippedXMin, childRectTransform.anchorMin.y);
                childRectTransform.anchorMax = new Vector2(flippedXMax, childRectTransform.anchorMax.y);

                // Resetting offset values in case they changed
                childRectTransform.offsetMin = Vector2.zero;
                childRectTransform.offsetMax = Vector2.zero;

                // Flipping the child's children
                FlipChildrenHorizontally(childRectTransform);
            }
        }

        private void FlipChildrenVertically(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                RectTransform childRectTransform = parent.GetChild(i).GetComponent<RectTransform>();

                if (childRectTransform == null) return;

                // Calculating flipped y anchors
                float flippedYMin = GetFlippedAnchor(childRectTransform.anchorMax.y);
                float flippedYMax = GetFlippedAnchor(childRectTransform.anchorMin.y);

                // Setting new y anchors
                childRectTransform.anchorMin = new Vector2(childRectTransform.anchorMin.x, flippedYMin);
                childRectTransform.anchorMax = new Vector2(childRectTransform.anchorMax.x, flippedYMax);

                // Resetting offset values in case they changed
                childRectTransform.offsetMin = Vector2.zero;
                childRectTransform.offsetMax = Vector2.zero;

                // Flipping the child's children
                FlipChildrenVertically(childRectTransform);
            }
        }

        private float GetFlippedAnchor(float anchorValue)
        {
            return -anchorValue + 1;
        }
    }
}
