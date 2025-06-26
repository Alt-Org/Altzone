using UnityEngine;
using TMPro;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Text scaler for TextMeshProUGUI based on text box size and font size target percentage.
    /// Note: Disables font auto sizing and text wrapping for TextMeshProUGUI.
    /// </summary>
    public class TextScaler : MonoBehaviour
    {
        [Header("Text scaling options")]
        [SerializeField, Range(0, 1)] private float _relativePercentageTarget = 0.8f;
        [SerializeField, Range(0, 1)] private float _relativePercentageMax = 0.9f;
        [SerializeField, Min(0)] private float _fontSizeMin;
        [SerializeField, Min(0)] private float _fontSizeMax;

        [Header("Text box scaling options")]
        [SerializeField] private float _textBoxAspectRatio = 0;
        [SerializeField] private float _startScalingHolderAspectRatio = 9.0f / 16.0f;
        [SerializeField] private RectTransform _holderRectTransform;

        [Header("Text component references")]
        [SerializeField] private TextMeshProUGUI[] _textArray;

        [HideInInspector]
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

        [HideInInspector]
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

        [HideInInspector]
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

        [HideInInspector]
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

        private RectTransform[] _rectTransformArray;

        private Vector2[] _oldRectSizeArray;
        private string[] _oldTextArray;

        private Vector2[] _textBoxDefaultAnchorMinArray;
        private Vector2[] _textBoxDefaultAnchorMaxArray;

        private bool _fontSettingsChanged;

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

        private void Update()
        {
            for (int i = 0; i < _textArray.Length; i++)
            {
                if (_rectTransformArray[i] == null) return;
                bool textBoxSizeChanged = _rectTransformArray[i].rect.size != _oldRectSizeArray[i];

                // If text box size changed and the text box scaling variables are set, scale text box
                if (textBoxSizeChanged && _textBoxAspectRatio != 0 && _holderRectTransform != null) ScaleTextBox(i);

                // Recalculating font size if text box size changed or font settings changed
                if (textBoxSizeChanged || _fontSettingsChanged) RecalculateFontSize(i);

                // If the text, text box size or font settings changed checking for text clipping
                if (_oldTextArray[i] != _textArray[i].text || textBoxSizeChanged || _fontSettingsChanged) CheckTextClipping(i);
            }

            _fontSettingsChanged = false;
        }

        private void RecalculateFontSize(int index)
        {
            // Calculating the font target and max relative size
            float fontSize = _rectTransformArray[index].rect.height * _relativePercentageTarget;
            float fontRelativeSizeMax = _rectTransformArray[index].rect.height * _relativePercentageMax;

            // Clamping font size to min and max font sizes
            fontSize = Mathf.Clamp(fontSize, Mathf.Min(_fontSizeMin, fontRelativeSizeMax), _fontSizeMax == 0 ? float.PositiveInfinity : _fontSizeMax);

            // Setting font size and _oldRectSize
            _textArray[index].fontSize = fontSize;
            _oldRectSizeArray[index] = _rectTransformArray[index].rect.size;
        }

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

        private void CheckTextClipping(int index)
        {
            // Printing warning text if the text is too long to fit in the box
            if (_textArray[index].preferredWidth > _rectTransformArray[index].rect.width) Debug.LogWarning($"TextScaler: Text is too long to fit in the box: {_textArray[index].text}");
            _oldTextArray[index] = _textArray[index].text;
        }
    }
}
