using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.scripts.AvatarEditor
{
    public class GridCellHandler : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _featureImage;
        [SerializeField] private Button _button;

        public void SetValues(Sprite cellImage = null,
            Color? backgroundColor = null,
            UnityEngine.Events.UnityAction onClick = null,
            bool buttonIsInteractable = true,
            Color? cellImageColor = null)
        {
            _featureImage.preserveAspect = true;

            if (cellImage != null)
            {
                _featureImage.sprite = cellImage;
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
            
            _button.interactable = buttonIsInteractable;
        }

        public void SetFeatureImageAnchors(Vector2 anchorMin, Vector2 anchorMax)
        {
            _featureImage.rectTransform.anchorMin = anchorMin;
            _featureImage.rectTransform.anchorMax = anchorMax;
        }
    }
}
