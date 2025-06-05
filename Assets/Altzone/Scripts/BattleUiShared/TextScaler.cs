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

        [Header("Text component references")]
        [SerializeField] private TextMeshProUGUI _text;

        private RectTransform _rectTransform;

        private Vector2 _oldRectSize = Vector2.zero;
        private string _oldText = string.Empty;

        private void Awake()
        {
            // Getting rect transform component for size comparisons
            if (_text == null) return;
            _rectTransform = _text.gameObject.GetComponent<RectTransform>();

            // Setting text box settings
            if (_text.enableAutoSizing == true) _text.enableAutoSizing = false;
            if (_text.textWrappingMode != TextWrappingModes.NoWrap) _text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private void Update()
        {
            if (_rectTransform == null) return;
            bool textBoxSizeChanged = _rectTransform.rect.size != _oldRectSize;

            // Recalculating font size if text box size changed
            if (textBoxSizeChanged) RecalculateFontSize();

            // If the text changed or text box size changed checking for text clipping
            if (_oldText != _text.text || textBoxSizeChanged) CheckTextClipping();
        }

        private void RecalculateFontSize()
        {
            // Calculating the font size
            float fontSize = _rectTransform.rect.height * _relativePercentageTarget;

            // Checking if the font size clamped is different
            if (fontSize != Mathf.Clamp(fontSize, _fontSizeMin, _fontSizeMax))
            {
                float fontRelativeSizeMax = _rectTransform.rect.height * _relativePercentageMax;

                // Saving min and max for font size since we don't want to modify original values. Also if _fontSizeMax is 0 using relative max.
                float fontSizeMin = _fontSizeMin;
                float fontSizeMax = _fontSizeMax == 0 ? fontRelativeSizeMax : _fontSizeMax;

                // If max relative font size is smaller than min font size adjusting the min font size to be the max relative font size
                if (fontRelativeSizeMax < fontSizeMin)
                {
                    fontSizeMin = fontRelativeSizeMax;

                    // Ensuring that the _fontSizeMax is not smaller than _fontSizeMin
                    fontSizeMax = fontSizeMin > fontSizeMax ? fontSizeMin : fontSizeMax;
                }

                // Clamping font size
                fontSize = Mathf.Clamp(fontSize, fontSizeMin, fontSizeMax);
            }

            // Setting font size and _oldRectSize
            _text.fontSize = fontSize;
            _oldRectSize = _rectTransform.rect.size;
        }

        private void CheckTextClipping()
        {
            // Printing warning text if the text is too long to fit in the box
            if (_text.preferredWidth > _rectTransform.rect.width) Debug.LogWarning($"TextScaler: Text is too long to fit in the box: {_text.text}");
            _oldText = _text.text;
        }
    }
}
