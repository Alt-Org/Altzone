using UnityEngine;
using TMPro;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Text scaler based on text box size. 
    /// </summary>
    public class TextScaler : MonoBehaviour
    {
        [Header("Text scaling options")]
        [SerializeField] private float _relativePercentageTarget;
        [SerializeField] private float _relativePercentageMax;
        [SerializeField] private float _fontSizeMin;
        [SerializeField] private float _fontSizeMax;

        [Header("Text component references")]
        [SerializeField] private TextMeshProUGUI _text;

        private RectTransform _rectTransform;
        private Vector2 _oldSize = Vector2.zero;

        private void Awake()
        {
            if (_text == null) return;
            _rectTransform = _text.gameObject.GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (_rectTransform == null) return;
            if (_rectTransform.rect.size == _oldSize) return;
        }
    }
}
