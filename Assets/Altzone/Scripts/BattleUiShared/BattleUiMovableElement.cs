using UnityEngine;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Handles setting and getting the Battle Ui element position.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMovableElement : MonoBehaviour
    {
        protected RectTransform _rectTransform;

        protected void Awake()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Set BattleUiMovableElementData to this Ui element.
        /// </summary>
        /// <param name="data">The data which to set to this Ui element.</param>
        public virtual void SetData(BattleUiMovableElementData data)
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            _rectTransform.anchorMin = data.AnchorMin;
            _rectTransform.anchorMax = data.AnchorMax;

            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Get the data from this Ui element.
        /// </summary>
        /// <returns>Returns BattleUiMovableElementData serializable object. Null if couldn't get valid data.</returns>
        public virtual BattleUiMovableElementData GetData()
        {
            if (_rectTransform != null)
            {
                return new BattleUiMovableElementData(_rectTransform.anchorMin, _rectTransform.anchorMax);
            }
            else
            {
                return null;
            }
        }
    }
}

