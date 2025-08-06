using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Handles setting and getting the Battle Ui element position.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMovableElement : MonoBehaviour
    {
        public RectTransform RectTransformComponent => _rectTransform;
        public BattleUiElementType UiElementType { get; private set; }

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

            UiElementType = data.UiElementType;

            if (data.Transparency != _currentTransparency)
            {
                _currentTransparency = data.Transparency;
                SetTransparency();
            }
        }

        /// <summary>
        /// Get the data from this Ui element.
        /// </summary>
        /// <returns>Returns BattleUiMovableElementData serializable object. Null if couldn't get valid data.</returns>
        public virtual BattleUiMovableElementData GetData()
        {
            if (_rectTransform != null)
            {
                return new BattleUiMovableElementData(UiElementType, _rectTransform.anchorMin, _rectTransform.anchorMax, _currentTransparency);
            }
            else
            {
                return null;
            }
        }

        protected RectTransform _rectTransform;

        protected List<Image> _images;
        protected List<TMP_Text> _texts;

        protected int _currentTransparency = 0;

        protected void Awake()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            _images = GetComponentsInChildren<Image>().ToList();
            _texts = GetComponentsInChildren<TMP_Text>().ToList();
        }

        protected void SetTransparency()
        {
            if (_images == null) _images = GetComponentsInChildren<Image>().ToList();
            if (_texts == null) _texts = GetComponentsInChildren<TMP_Text>().ToList();

            // Converting transparency to opacity
            float newOpacity = 1f - _currentTransparency / 100f;

            // Setting new opacity to images
            foreach (Image image in _images)
            {
                if (image.sprite == null) continue;
                Color newImageColor = image.color;
                newImageColor.a = newOpacity;
                image.color = newImageColor;
            }

            // Setting new opacity to texts
            foreach (TMP_Text text in _texts)
            {
                Color newTextColor = text.color;
                newTextColor.a = newOpacity;
                text.color = newTextColor;
            }
        }
    }
}
