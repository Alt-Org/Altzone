using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.UIScaling
{
    public class BackgroundPanelScaler : MonoBehaviour
    {
        [SerializeField] protected RectTransform _unsafeAreaRectTransfrom;
        [SerializeField] protected RectTransform _fullPanelPopupsTransform;

        // Start is called before the first frame update
        private void Awake()
        {
            SetPanelAnchors();
        }

        protected virtual void SetPanelAnchors()
        {
            _unsafeAreaRectTransfrom.anchorMin = new Vector2(0, 1 - PanelScaler.CalculateUnsafeAreaHeight());

            _fullPanelPopupsTransform.anchorMax = new Vector2(1, 1 - PanelScaler.CalculateUnsafeAreaHeight());
        }
    }
}
