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
        [SerializeField] private GameObject _verticalConfiguration;

        private BattleUiElementOrientation _orientation;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Set BattleUiElementData to this Ui element.
        /// </summary>
        /// <param name="data">The data which to set to this Ui element.</param>
        public void SetData(BattleUiElementData data)
        {
            _rectTransform.anchorMin = data.AnchorMin;
            _rectTransform.anchorMin = data.AnchorMax;

            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            _orientation = data.Orientation;
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

