/// @file TextScaler.cs
/// <summary>
/// Contains @cref{Altzone.Scripts.BattleUiShared,TextScaler} class which is a text scaler for <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/TMPro.TextMeshProUGUI.html">TextMeshProUGUI@u-exlink</a>.
/// </summary>
///
/// This script:<br/>
/// Text scaler for <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/TMPro.TextMeshProUGUI.html">TextMeshProUGUI@u-exlink</a> based on text box size and font size target percentage.<br/>
/// Note: Disables font auto sizing and text wrapping for <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/TMPro.TextMeshProUGUI.html">TextMeshProUGUI@u-exlink</a>.

using UnityEngine;
using TMPro;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// <span class="brief-h">Text scaler <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Scales <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/TMPro.TextMeshProUGUI.html">TextMeshProUGUI@u-exlink</a> text based on text box size and font size target percentage. Uses arrays to apply same settings to multiple texts.
    /// </summary>
    ///
    /// Note: Disables font auto sizing and text wrapping for <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/TMPro.TextMeshProUGUI.html">TextMeshProUGUI@u-exlink</a>.
    public class TextScaler : MonoBehaviour
    {
        /// @anchor TextScaler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] The target percentage (of text box height) for font size.</summary>
        /// @ref TextScaler-SerializeFields
        [Header("Text scaling options")]
        [SerializeField, Range(0, 1)] private float _relativePercentageTarget = 0.8f;

        /// <summary>[SerializeField] The maximum percentage (of text box height) for font size.</summary>
        /// @ref TextScaler-SerializeFields
        [SerializeField, Range(0, 1)] private float _relativePercentageMax = 0.9f;

        /// <summary>[SerializeField] Minimum font size.</summary>
        /// @ref TextScaler-SerializeFields
        [SerializeField, Min(0)] private float _fontSizeMin;

        /// <summary>[SerializeField] Maximum font size.</summary>
        /// @ref TextScaler-SerializeFields
        [SerializeField, Min(0)] private float _fontSizeMax;

        /// <summary>[SerializeField] Text box's aspect ratio. This should be default (0) if text box shouldn't scale</summary>
        /// @ref TextScaler-SerializeFields
        [Header("Text box scaling options")]
        [SerializeField] private float _textBoxAspectRatio = 0;

        /// <summary>[SerializeField] The #_holderRectTransform aspect ratio the text box will start scaling at. This is for widescreen support. If #_holderRectTransform aspect ratio is wider than this value, the text box won't scale.</summary>
        /// @ref TextScaler-SerializeFields
        [SerializeField] private float _startScalingHolderAspectRatio = 9.0f / 16.0f;

        /// <summary>[SerializeField] Reference to the (full screen) parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> for all #_textArray text boxes.</summary>
        /// @ref TextScaler-SerializeFields
        [SerializeField] private RectTransform _holderRectTransform;

        /// <summary>[SerializeField] Array of <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/TMPro.TextMeshProUGUI.html">TextMeshProUGUI@u-exlink</a> references which texts should be scaled.</summary>
        /// @ref TextScaler-SerializeFields
        [Header("Text component references")]
        [SerializeField] private TextMeshProUGUI[] _textArray;

        /// @}

        /// <summary>Public getter/setter for #_relativePercentageTarget. Sets #_fontSettingsChanged true if value changed.</summary>
        /// <value>The target percentage (of text box height) for font scale.</value>
        public float RelativePercentageTarget
        {
            get => _relativePercentageTarget;
            set
            {
                value = Mathf.Clamp01(value);
                if (_relativePercentageTarget == value) return;

                _relativePercentageTarget = value;
                _fontSettingsChanged = true;
            }
        }

        /// <summary>Public getter/setter for #_relativePercentageMax. Sets #_fontSettingsChanged true if value changed.</summary>
        /// <value>The maximum percentage (of text box height) for font scale.</value>
        public float RelativePercentageMax
        {
            get => _relativePercentageMax;
            set
            {
                value = Mathf.Clamp01(value);
                if (_relativePercentageMax == value) return;

                _relativePercentageMax = value;
                _fontSettingsChanged = true;
            }
        }

        /// <summary>Public getter/setter for #_fontSizeMin. Sets #_fontSettingsChanged true if value changed.</summary>
        /// <value>Minimum font size.</value>
        public float FontSizeMin
        {
            get => _fontSizeMin;
            set
            {
                value = Mathf.Clamp(value, 0, float.PositiveInfinity);
                if (_fontSizeMin == value) return;

                _fontSizeMin = value;
                _fontSettingsChanged = true;
            }
        }

        /// <summary>Public getter/setter for #_fontSizeMax. Sets #_fontSettingsChanged true if value changed.</summary>
        /// <value>Maximum font size.</value>
        public float FontSizeMax
        {
            get => _fontSizeMax;
            set
            {
                value = Mathf.Clamp(value, 0, float.PositiveInfinity);
                if (_fontSizeMax == value) return;

                _fontSizeMax = value;
                _fontSettingsChanged = true;
            }
        }

        /// <value> Array of <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> components from #_textArray's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObjects@u-exlink</a>.</value>
        private RectTransform[] _rectTransformArray;

        /// <value>Array for previous <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform-rect.html">Rect@u-exlink</a> sizes from #_rectTransformArray. Used in #Update for checking if text box size changed.</value>
        private Vector2[] _oldRectSizeArray;

        /// <value>Array for previous text strings from #_textArray. Used in #Update for checking if text changed.</value>
        private string[] _oldTextArray;

        /// <value>Array of default text box <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform-anchorMin.html">anchorMin@u-exlink</a> values.</value>
        private Vector2[] _textBoxDefaultAnchorMinArray;

        /// <value>Array of default text box <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform-anchorMax.html">anchorMax@u-exlink</a> values.</value>
        private Vector2[] _textBoxDefaultAnchorMaxArray;

        /// <value>If public font setting variables have changed.</value>
        private bool _fontSettingsChanged;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method which initializes TextScaler variables.
        /// </summary>
        private void Awake()
        {
            _rectTransformArray = new RectTransform[_textArray.Length];
            _oldRectSizeArray = new Vector2[_textArray.Length];
            _oldTextArray = new string[_textArray.Length];
            _textBoxDefaultAnchorMinArray = new Vector2[_textArray.Length];
            _textBoxDefaultAnchorMaxArray = new Vector2[_textArray.Length];
            _fontSettingsChanged = false;

            for (int i = 0; i < _textArray.Length; i++)
            {
                // Getting rect transform component for size comparisons
                if (_textArray[i] == null) return;
                _rectTransformArray[i] = _textArray[i].gameObject.GetComponent<RectTransform>();

                // Setting text box settings
                if (_textArray[i].enableAutoSizing == true) _textArray[i].enableAutoSizing = false;
                if (_textArray[i].textWrappingMode != TextWrappingModes.NoWrap) _textArray[i].textWrappingMode = TextWrappingModes.NoWrap;

                // Setting empty values to arrays
                _oldRectSizeArray[i] = Vector2.zero;
                _oldTextArray[i] = string.Empty;

                // Saving text box scaling variables
                if (_textBoxAspectRatio == 0 || _holderRectTransform == null) continue;
                _textBoxDefaultAnchorMinArray[i] = _rectTransformArray[i].anchorMin;
                _textBoxDefaultAnchorMaxArray[i] = _rectTransformArray[i].anchorMax;
            }
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method which checks if font size has to be recalculated and/or text clipping checked.
        /// </summary>
        private void Update()
        {
            bool textBoxSizeChanged;
            bool changeFontSize;

            for (int i = 0; i < _textArray.Length; i++)
            {
                if (_rectTransformArray[i] == null) return;

                textBoxSizeChanged = _rectTransformArray[i].rect.size != _oldRectSizeArray[i];
                changeFontSize = _fontSettingsChanged || textBoxSizeChanged;

                // If text box size changed and the text box scaling variables are set, scale text box
                if (textBoxSizeChanged && _textBoxAspectRatio != 0 && _holderRectTransform != null) ScaleTextBox(i);

                // Recalculating font size if font settings changed or text box size changed
                if (changeFontSize) RecalculateFontSize(i);

                // If the font settings, text or text box size changed checking for text clipping
                if (changeFontSize || _oldTextArray[i] != _textArray[i].text) CheckTextClipping(i);
            }

            _fontSettingsChanged = false;
        }

        /// <summary>
        /// Recalculates font size for the text.
        /// </summary>
        ///
        /// <param name="index">The text index which font size to recalculate.</param>
        private void RecalculateFontSize(int index)
        {
            // Calculating the font target and max relative size
            float fontSize = _rectTransformArray[index].rect.height * _relativePercentageTarget;
            float fontRelativeSizeMax = _rectTransformArray[index].rect.height * _relativePercentageMax;

            // Clamping font size to min and max font sizes
            fontSize = Mathf.Min(Mathf.Clamp(fontSize, _fontSizeMin, _fontSizeMax == 0 ? float.PositiveInfinity : _fontSizeMax), fontRelativeSizeMax);

            // Setting font size and _oldRectSize
            _textArray[index].fontSize = fontSize;
            _oldRectSizeArray[index] = _rectTransformArray[index].rect.size;
        }

        /// <summary>
        /// Handles text box scaling while keeping aspect ratio with compatibility for widescreen.
        /// </summary>
        ///
        /// <param name="index">The text index which text box to scale.</param>
        ///
        /// The way this works is that the text box will scale y/height anchors while keeping the #_textBoxAspectRatio, and x/width anchors say the same.<br/>
        /// If #_holderRectTransform's current aspect ratio is wider than #_startScalingHolderAspectRatio then the text box won't scale in height so that the text won't become too large.
        private void ScaleTextBox(int index)
        {
            Vector2 anchorMin = _textBoxDefaultAnchorMinArray[index];
            Vector2 anchorMax = _textBoxDefaultAnchorMaxArray[index];

            // Setting default anchors
            _rectTransformArray[index].anchorMin = anchorMin;
            _rectTransformArray[index].anchorMax = anchorMax;

            // If aspect ratio is wider than start scaling aspect ratio we don't need to calculate new text box height
            if (_holderRectTransform.rect.width / _holderRectTransform.rect.height > _startScalingHolderAspectRatio) return;

            // Calculating new height for text box
            float newHeight = _rectTransformArray[index].rect.width / _textBoxAspectRatio;

            // Calculating the Y anchors
            anchorMin.y = Mathf.Clamp01((_rectTransformArray[index].position.y - newHeight * 0.5f) / _holderRectTransform.rect.height);
            anchorMax.y = Mathf.Clamp01((_rectTransformArray[index].position.y + newHeight * 0.5f) / _holderRectTransform.rect.height);

            // Setting updated anchors
            _rectTransformArray[index].anchorMin = anchorMin;
            _rectTransformArray[index].anchorMax = anchorMax;
        }

        /// <summary>
        /// Checks if text is clipping outside the textbox bounds and if so prints a log warning.
        /// </summary>
        ///
        /// <param name="index">The index of the text which clipping to check.</param>
        private void CheckTextClipping(int index)
        {
            // Printing warning text if the text is too long to fit in the box
            if (_textArray[index].preferredWidth > _rectTransformArray[index].rect.width) Debug.LogWarning($"TextScaler: Text is too long to fit in the box: {_textArray[index].text}");
            _oldTextArray[index] = _textArray[index].text;
        }
    }
}
