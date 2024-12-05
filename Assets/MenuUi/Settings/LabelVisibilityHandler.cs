using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MenuUi.Scripts.Settings
{
    public class LabelVisibilityHandler : MonoBehaviour
    {
        private SettingsCarrier _carrier = SettingsCarrier.Instance;

        private void Start()
        {
            _carrier.OnButtonLabelVisibilityChange += SetVisibility;
        }

        void OnEnable()
        {
            SetVisibility();
        }

        private void SetVisibility()
        {
            gameObject.SetActive(_carrier.ShowButtonLabels);
        }
    }
}
