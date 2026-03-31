using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class FeatureCellHandler : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _featureImage;
        [SerializeField] private Button _button;
        [SerializeField] private Sprite _baseBackgroundSprite;
        [SerializeField] private Sprite _highlightBackgroundSprite;
        private bool _isColorable = false;
        public bool IsColorable {  get { return _isColorable; } }
        public void SetValues(Sprite cellImage,
            bool isColorable)
        {
            _featureImage.sprite = cellImage;
            _isColorable = isColorable;

            _featureImage.type = Image.Type.Simple;
            _featureImage.preserveAspect = true;
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
