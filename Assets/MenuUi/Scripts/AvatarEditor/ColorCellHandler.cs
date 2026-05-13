using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ColorCellHandler : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _colorImage;
        [SerializeField] private Button _button;
        [SerializeField] private Sprite _baseBackgroundSprite;
        [SerializeField] private Sprite _highlightBackgroundSprite;
        public void SetColor(Color color)
        {
           _colorImage.color = color;
        }

        public void Highlight(bool isHighlighted)
        {
            if (isHighlighted)
            {
                _backgroundImage.sprite = _highlightBackgroundSprite;
            }
            else
            {
                _backgroundImage.sprite = _baseBackgroundSprite;
            }
        }

        public void SetOnClick(UnityEngine.Events.UnityAction onClick)
        {
            _button.onClick.AddListener(onClick);
        }
    }
}
