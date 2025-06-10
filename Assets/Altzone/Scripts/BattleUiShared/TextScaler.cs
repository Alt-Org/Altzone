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
        [SerializeField] private TextMeshProUGUI _text;

        private RectTransform _rectTransform;

        private Vector2 _oldRectSize = Vector2.zero;
        private string _oldText = string.Empty;

        private Vector2 _textBoxDefaultAnchorMin;
        private Vector2 _textBoxDefaultAnchorMax;

        private void Awake()
        {
            // Getting rect transform component for size comparisons
            if (_text == null) return;
            _rectTransform = _text.gameObject.GetComponent<RectTransform>();

            // Setting text box settings
            if (_text.enableAutoSizing == true) _text.enableAutoSizing = false;
            if (_text.textWrappingMode != TextWrappingModes.NoWrap) _text.textWrappingMode = TextWrappingModes.NoWrap;

            // Saving text box scaling variables
            if (_textBoxAspectRatio == 0 || _holderRectTransform == null) return;
            _textBoxDefaultAnchorMin = _rectTransform.anchorMin;
            _textBoxDefaultAnchorMax = _rectTransform.anchorMax;
        }

        private void Update()
        {
            if (_rectTransform == null) return;
            bool textBoxSizeChanged = _rectTransform.rect.size != _oldRectSize;

            // If text box size changed and the text box scaling variables are set, scale text box
            if (textBoxSizeChanged && _textBoxAspectRatio != 0 && _holderRectTransform != null) ScaleTextBox();

            // Recalculating font size if text box size changed
            if (textBoxSizeChanged) RecalculateFontSize();

            // If the text changed or text box size changed checking for text clipping
            if (_oldText != _text.text || textBoxSizeChanged) CheckTextClipping();
        }

        private void RecalculateFontSize()
        {
            // Calculating the font target and max relative size
            float fontSize = _rectTransform.rect.height * _relativePercentageTarget;
            float fontRelativeSizeMax = _rectTransform.rect.height * _relativePercentageMax;

            // Clamping font size to min and max font sizes
            fontSize = Mathf.Clamp(fontSize, Mathf.Min(_fontSizeMin, fontRelativeSizeMax), _fontSizeMax == 0 ? float.PositiveInfinity : _fontSizeMax);

            // Setting font size and _oldRectSize
            _text.fontSize = fontSize;
            _oldRectSize = _rectTransform.rect.size;
        }

        private void ScaleTextBox()
        {
            Vector2 anchorMin = _textBoxDefaultAnchorMin;
            Vector2 anchorMax = _textBoxDefaultAnchorMax;

            // Setting default anchors
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            // If aspect ratio is wider than start scaling aspect ratio we don't need to calculate new text box height
            if (_holderRectTransform.rect.width / _holderRectTransform.rect.height > _startScalingHolderAspectRatio) return;

            // Calculating new height for text box
            float newHeight = _rectTransform.rect.width / _textBoxAspectRatio;

            // Calculating the Y anchors
            anchorMin.y = Mathf.Clamp01((_rectTransform.position.y - newHeight * 0.5f) / _holderRectTransform.rect.height);
            anchorMax.y = Mathf.Clamp01((_rectTransform.position.y + newHeight * 0.5f) / _holderRectTransform.rect.height);

            // Setting updated anchors
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }

        private void CheckTextClipping()
        {
            // Printing warning text if the text is too long to fit in the box
            if (_text.preferredWidth > _rectTransform.rect.width) Debug.LogWarning($"TextScaler: Text is too long to fit in the box: {_text.text}");
            _oldText = _text.text;
        }
    }
}
