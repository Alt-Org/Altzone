/// @file BattleUiMovableElement.cs
/// <summary>
/// Contains @cref{Altzone.Scripts.BattleUiShared,BattleUiMovableElement} class which sets the %Battle Ui element's position and transparency.
/// </summary>
///
/// This script:<br/>
/// Handles setting the %Battle Ui element's position and transparency.
/// Base class for other %Battle Ui element scripts.

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// <span class="brief-h">Movable element <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Base class for %Battle Ui element scripts. Handles setting the %Battle Ui element's position and transparency.
    /// </summary>
    ///
    /// Should be attached to the top level <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a> of the %Battle Ui element prefab.
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMovableElement : MonoBehaviour
    {
        /// <summary>Public getter for #_rectTransform.</summary>
        /// <value>Reference to the %Battle Ui element's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</value>
        public RectTransform RectTransformComponent => _rectTransform;

        /// <value>%Battle Ui element's BattleUiElementType.</value>
        public BattleUiElementType UiElementType { get; private set; }

        /// <summary>
        /// Set BattleUiMovableElementData to this Ui element.
        /// </summary>
        ///
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
        ///
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

        /// <value>Reference to the %Battle Ui element's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</value>
        protected RectTransform _rectTransform;

        /// <value>List of %Battle Ui element's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Image.html">Image@u-exlink</a> component references.</value>
        protected List<Image> _images;

        /// <value>List of %Battle Ui element's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/TMPro.TMP_Text.html">TMP_Text@u-exlink</a> component references.</value>
        protected List<TMP_Text> _texts;

        /// <value>%Battle Ui element's current transparency.</value>
        protected int _currentTransparency = 0;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method which initializes #_rectTransform, #_images and #_texts.
        /// </summary>
        protected void Awake()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            _images = GetComponentsInChildren<Image>().ToList();
            _texts = GetComponentsInChildren<TMP_Text>().ToList();
        }

        /// <summary>
        /// Sets %Battle Ui element's transparency.
        /// </summary>
        ///
        /// Calculates opacity value from #_currentTransparency which is then applied to all elements of #_images and #_texts.
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
