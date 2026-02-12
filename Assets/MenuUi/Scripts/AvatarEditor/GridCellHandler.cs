using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public abstract class GridCellHandler : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] protected Image _featureImage;
        [SerializeField] private Button _button;
        private Color _highlightColor;
        private Color _backgroundColor;

        protected void SetValues(Sprite cellImage = null,
            Color? higlightColor = null,
            Color? backgroundColor = null,
            UnityEngine.Events.UnityAction onClick = null,
            bool? buttonIsInteractable = null)
        {

            if (cellImage == null &&
                higlightColor == null &&
                backgroundColor == null &&
                onClick == null &&
                buttonIsInteractable == null)
            {
                Debug.LogWarning("SetValues() called without any arguments");
                return;
            }

            _featureImage.preserveAspect = true;

            if (cellImage != null)
            {
                _featureImage.sprite = cellImage;
                _featureImage.type = Image.Type.Simple;
            }

            if (backgroundColor.HasValue)
            {
                _backgroundImage.color = backgroundColor.Value;
                _backgroundColor = backgroundColor.Value;
            }

            if (higlightColor.HasValue)
            {
                _highlightColor = higlightColor.Value;
            }

            if (onClick != null)
            {
                _button.onClick.AddListener(onClick);
            }

            if (buttonIsInteractable.HasValue)
            {
                _button.interactable = buttonIsInteractable.Value;
            }
        }

        protected void Highlight(bool isHighlighted)
        {
            if (isHighlighted)
            {
                _backgroundImage.color = _highlightColor;
            }
            else
            {
                _backgroundImage.color = _backgroundColor;
            }
        }
    }
}
