/// @file BattleUiMultiOrientationElement.cs
/// <summary>
/// Contains @cref{Altzone.Scripts.BattleUiShared,BattleUiMultiOrientationElement} class which sets the %Battle Ui multi orientation element's data.
/// </summary>
///
/// This script:<br/>
/// Handles setting the %Battle Ui multi orientation element position and orientation and flipping it.

using UnityEngine;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// <span class="brief-h">Multi orientation BattleUiMovableElement.</span><br/>
    /// Handles setting the %Battle Ui multi orientation element position and orientation and flipping it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMultiOrientationElement : BattleUiMovableElement
    {
        /// @anchor BattleUiMultiOrientationElement-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to the horizontal orientation's parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</summary>
        /// @ref BattleUiMultiOrientationElement-SerializeFields
        [SerializeField] private GameObject _horizontalConfiguration;

        /// <summary>[SerializeField] Multi orientation element's aspect ratio for horizontal orientation.</summary>
        /// @ref BattleUiMultiOrientationElement-SerializeFields
        [SerializeField] private float _horizontalAspectRatio;

        /// <summary>[SerializeField] Reference to the vertical orientation's parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</summary>
        /// @ref BattleUiMultiOrientationElement-SerializeFields
        [SerializeField] private GameObject _verticalConfiguration;

        /// <summary>[SerializeField] Multi orientation element's aspect ratio for vertical orientation.</summary>
        /// @ref BattleUiMultiOrientationElement-SerializeFields
        [SerializeField] private float _verticalAspectRatio;

        /// @}

        /// <summary>
        /// Multi orientation element's possible orientations.
        /// </summary>
        public enum OrientationType
        {
            None = -1,
            Horizontal = 0,
            Vertical = 1,
        }

        /// <summary>Public getter for #_orientation.</summary>
        /// <value>Multi orientation element's current orientation.</value>
        public OrientationType Orientation => _orientation;

        /// <summary>Public getter for #_horizontalConfiguration.</summary>
        /// <value>Reference to the horizontal orientation's parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</value>
        public GameObject HorizontalConfiguration => _horizontalConfiguration;

        /// <summary>Public getter for #_verticalConfiguration.</summary>
        /// <value>Reference to the vertical orientation's parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</value>
        public GameObject VerticalConfiguration => _verticalConfiguration;

        /// <summary>Public getter for #_isFlippedHorizontally.</summary>
        /// <value>Is multi orientation element flipped horizontally.</value>
        public bool IsFlippedHorizontally => _isFlippedHorizontally;

        /// <summary>Public getter for #_isFlippedVertically.</summary>
        /// <value>Is multi orientation element flipped vertically.</value>
        public bool IsFlippedVertically => _isFlippedVertically;

        /// <summary>Public getter for #_horizontalAspectRatio.</summary>
        /// <value>Multi orientation element's aspect ratio for horizontal orientation.</value>
        public float HorizontalAspectRatio => _horizontalAspectRatio;

        /// <summary>Public getter for #_verticalAspectRatio.</summary>
        /// <value>Multi orientation element's aspect ratio for vertical orientation.</value>
        public float VerticalAspectRatio => _verticalAspectRatio;

        /// <value>Is the multi orientation element currently horizontal.</value>
        public bool IsHorizontal
        {
            get
            {
                return _orientation == OrientationType.Horizontal;
            }
        }

        /// <summary>
        /// Get the currently active orientation <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.
        /// </summary>
        ///
        /// <returns>Horizontal or vertical orientation <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</returns>
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
        ///
        /// <param name="data">The data which to set to this Ui element.</param>
        public override void SetData(BattleUiMovableElementData data)
        {
            base.SetData(data);
            SetOrientation(data);
        }

        /// <summary>
        /// Get the data from this Ui multi orientation element.
        /// </summary>
        ///
        /// <returns>Returns BattleUiMovableElementData serializable object. Null if couldn't get valid data.</returns>
        public override BattleUiMovableElementData GetData()
        {
            if (_rectTransform != null)
            {
                return new BattleUiMovableElementData(
                    uiElementType: UiElementType,
                    transparency: _currentTransparency,
                    anchorMin: _rectTransform.anchorMin,
                    anchorMax: _rectTransform.anchorMax,
                    orientation: _orientation,
                    isFlippedHorizontally: IsFlippedHorizontally,
                    isFlippedVertically: IsFlippedVertically
                );
            }
            else
            {
                return null;
            }
        }

        /// <value>Multi orientation element's current orientation.</value>
        private OrientationType _orientation = OrientationType.Horizontal;

        /// <value>Is multi orientation element flipped horizontally.</value>
        private bool _isFlippedHorizontally = false;

        /// <value>Is multi orientation element flipped vertically.</value>
        private bool _isFlippedVertically = false;

        /// <summary>
        /// Set orientation from BattleUiMovableData.
        /// </summary>
        ///
        /// <param name="data">The BattleUiMovableData which contains the new orientation.</param>
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

        /// <summary>
        /// Flip children's anchors horizontally from side to the other side, in the x axis.
        /// </summary>
        ///
        /// <param name="parent">The parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Transform.html">Transform@u-exlink</a>.</param>
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

        /// <summary>
        /// Flip children's anchors vertically from side to the other side, in the y axis.
        /// </summary>
        ///
        /// <param name="parent">The parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Transform.html">Transform@u-exlink</a>.</param>
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

        /// <summary>
        /// Calculates a flipped anchor value for the given anchor value.
        /// </summary>
        ///
        /// <param name="anchorValue">The anchor value to be flipped.</param>
        ///
        /// <returns>Flipped anchor as float.</returns>
        private float GetFlippedAnchor(float anchorValue)
        {
            return -anchorValue + 1;
        }
    }
}
