using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Jukebox
{
    public class JukeboxMainDiskHandler : JukeboxDiskBase
    {
        [Space]
        [SerializeField] private GameObject _offlineIndicatorContent;
        [SerializeField] private Button _multiUseButton;
        [SerializeField] private TMPro.TextMeshProUGUI _indicatorText;
        [SerializeField] private GameObject _customIndicatorImageHolder;

        public delegate void MultiUseButtonPressed();
        public event MultiUseButtonPressed OnMultiUseButtonPressed;

        private List<string> _indicatorTexts = new()
        {
            "",
            "Esikuuntelu\r\nPäällä",
            "Jukeboxi\r\nMykistetty",
            "Pysäytetty",
            "Soittolista\r\nTyhjä",
            "Mykistetty\r\nAsetuksista"
        };

        public enum JukeboxDiskTextType
        {
            None = 0,
            Preview = 1,
            Muted = 2,
            Stopped = 3,
            Empty = 4,
            VolumeZero = 5
        }

        private void Awake()
        {
            if (_multiUseButton != null) _multiUseButton.onClick.AddListener(() => OnMultiUseButtonPressed?.Invoke());

            _mainDiskRectTransform.anchorMin = _mainAnchorMinEnd;
            _mainDiskRectTransform.anchorMax = _mainAnchorMaxEnd;

            _secondaryDiskRectTransform.anchorMin = _secondaryAnchorMinEnd;
            _secondaryDiskRectTransform.anchorMax = _secondaryAnchorMaxEnd;
        }

        #region Indicators

        public void ToggleIndicatorHolder(bool value)
        {
            _offlineIndicatorContent.SetActive(value);
        }

        public void SetIndicatorText(JukeboxDiskTextType textType)
        {
            _indicatorText.text = _indicatorTexts[(int)textType];
            ToggleIndicatorHolder(true);
        }

        public void ToggleCustomIndicatorImage(bool value)
        {
            _customIndicatorImageHolder.SetActive(value);
        }

        #endregion
    }
}
