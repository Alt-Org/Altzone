using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class GridCellHandler : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _featureImage;
        [SerializeField] private Button _button;

        public void SetValues(Sprite cellImage = null,
            Color? cellImageColor = null,
            Color? backgroundColor = null,
            UnityEngine.Events.UnityAction onClick = null,
            bool? buttonIsInteractable = null)
        {

            if (cellImage == null &&
                cellImageColor == null &&
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
            else if (cellImageColor.HasValue)
            {
                _featureImage.color = cellImageColor.Value;
            }

            if (backgroundColor.HasValue)
            {
                _backgroundImage.color = backgroundColor.Value;
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

        public void SetFeatureImageAnchors(Vector2 anchorMin, Vector2 anchorMax)
        {
            _featureImage.rectTransform.anchorMin = anchorMin;
            _featureImage.rectTransform.anchorMax = anchorMax;
        }
    }
}
