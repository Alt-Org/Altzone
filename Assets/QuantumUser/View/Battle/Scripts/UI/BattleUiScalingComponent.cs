using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Battle.View.UI
{
    public class BattleUiScalingComponent : MonoBehaviour
    {

        [SerializeField] RectTransform _container;
        [SerializeField] RectTransform _target;

        private Vector2 _oldContainerSize = new Vector2(1920, 1080);

        private void Update()
        {
            bool containerSizeChanged;

            containerSizeChanged = _container.rect.size != _oldContainerSize;

            if (containerSizeChanged && _container != null) Scale();
        }

        private void Scale()
        {
            _target.localScale /= 1 + (_oldContainerSize.x - _container.rect.size.x) / _container.rect.size.x;

            _oldContainerSize = _container.rect.size;
        }
    }
}
