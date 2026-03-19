using System;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using Assets.Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ScrollBarCategoryLoader : MonoBehaviour
    {
        [SerializeField] private Button _noseButton;
        [SerializeField] private Button _mouthButton;
        [SerializeField] private Button _handsButton;
        [SerializeField] private Button _bodyButton;
        [SerializeField] private Button _hairButton;
        [SerializeField] private Button _eyesButton;
        [SerializeField] private Button _clothesButton;
        [SerializeField] private Button _shoesButton;
        [SerializeField] private TextMeshProUGUI _categoryText;

        private Dictionary<Button, string> _buttonToCategoryId;

        // This is needed before awake even runs so need to do this
        private Dictionary<Button, string> ButtonToCategoryId
        {
            get
            {
                if (_buttonToCategoryId == null || _buttonToCategoryId.Count == 0)
                {
                    _buttonToCategoryId = new Dictionary<Button, string>
                    {
                        { _hairButton, "10" },
                        { _eyesButton, "21" },
                        { _noseButton, "22" },
                        { _mouthButton, "23" },
                        { _bodyButton, "" }, // not sure what this should do
                        { _clothesButton, "31" },
                        { _handsButton, "32" },
                        { _shoesButton, "33" }
                    };
                }
                return _buttonToCategoryId;
            }
        }
        private string _currentlySelectedCategory = "10";
        public string CurrentlySelectedCategory => _currentlySelectedCategory;
        private TextMeshProUGUI _currentCategoryTMP;

        public void ClickHairButton()
        {
            _hairButton.onClick.Invoke();
        }

        public void SetCategoryButtons(Action<string> buttonFunction)
        {
            foreach (KeyValuePair<Button, string> entry in ButtonToCategoryId)
            {
                Button button = entry.Key;
                string id = entry.Value;

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

                button.onClick.RemoveAllListeners();

                button.onClick.AddListener(() =>
                {
                    _currentlySelectedCategory = id;
                    _currentCategoryTMP = buttonText;

                    _categoryText.text = buttonText.text;
                    buttonFunction.Invoke(id);
                });
            }
        }
    }
}
