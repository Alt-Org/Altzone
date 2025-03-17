using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using System.Linq;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ColorPicker : MonoBehaviour
    {
        [SerializeField] private GameObject _colorButtonPrefab;
        [SerializeField] private GameObject _defaultColorButtonPrefab;
        [SerializeField] private List<Color> _colors;
        [SerializeField] private List<Transform> _colorButtonPositions;
        [SerializeField] private Transform _characterImageParent;

        private Image _colorChangeTarget;
        private CharacterClassID _characterClassID;

        private List<string> _currentColor = new()
        {
            "#ffffff",
        };
        private FeatureSlot _currentlySelectedCategory;

        public void OnEnable()
        {
            InstantiateColorButtons();
            _colorChangeTarget = _characterImageParent.GetChild(0).GetChild(0).GetComponent<Image>();
        }

        public void OnDisable()
        {
            DestroyColorButtons();
        }

        public void SelectFeature(FeatureSlot feature)
        {
            _currentlySelectedCategory = feature;
            //_colorChangeTarget = _characterImageParent.GetChild(0).GetChild((int)feature).GetComponent<Image>();
        }

        private void InstantiateColorButtons()
        {
            for (int i = 0; i < _colors.Count; i++)
            {
                int j = i;
                if (i == 0)
                {
                    Button button = Instantiate(_defaultColorButtonPrefab, _colorButtonPositions[i]).GetComponent<Button>();
                    button.onClick.AddListener(delegate{SetColor(_colors[j]);});
                }
                else
                {
                    Button button = Instantiate(_colorButtonPrefab, _colorButtonPositions[i]).GetComponent<Button>();
                    button.GetComponent<Image>().color = _colors[i];
                    button.onClick.AddListener(delegate{SetColor(_colors[j]);});
                }
            }
        }

        private void DestroyColorButtons()
        {
            foreach(Transform pos in _colorButtonPositions)
            {
                if(pos.childCount > 0)
                {
                    foreach(Transform child in pos)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private void SetColor(Color color)
        {
            //if ((int)_currentlySelectedCategory != 0)
            //    return;

            _currentColor[0] = "#" + ColorUtility.ToHtmlStringRGB(color);
            _colorChangeTarget = _characterImageParent.GetChild(0).GetChild(0).GetComponent<Image>();

            if (_colorChangeTarget != null)
            {
                _colorChangeTarget.color = color;

                if(_characterClassID == CharacterClassID.Confluent)
                {
                    _colorChangeTarget.transform.GetChild(0).GetComponent<Image>().color = color;
                }
            }
        }

        private void SetDefaultColor()
        {

        }

        private void SetTransparentColor()
        {
            if(_colorChangeTarget != null)
            {
                _colorChangeTarget.color = new Color(255,255,255,0);
                if(_characterClassID == CharacterClassID.Confluent)
                {
                    _colorChangeTarget.transform.GetChild(0).GetComponent<Image>().color = new Color(255,255,255,0);
                }
            }
        }

        public void SetCharacterClassID(CharacterClassID id)
        {
            _characterClassID = id;
        }

        public List<string> GetCurrentColors()
        {
            string[] colors = new string[_currentColor.Count];
            _currentColor.CopyTo(colors);
            return (colors.ToList());
        }

        public void SetLoadedColors(List<string> colors, List<string> features)
        {
            if (colors.Count != 0 && ColorUtility.TryParseHtmlString(colors[0], out Color color))
                SetColor(color);
            //for (int i = 0; i < _currentColor.Count; i++)
            //{
            //    SelectFeature((FeatureSlot)i);

            //    if (features[i] == "")
            //    {
            //        Debug.LogError($"{i} sdasd");
            //        SetTransparentColor();
            //    }
            //    //else if (features[i] == FeatureID.Default)
            //    //{
            //    //    Debug.Log("feature was default when coloring!");
            //    //}
            //    else if (i < _currentColor.Count)
            //    {
            //        if (ColorUtility.TryParseHtmlString(colors[i], out Color color))
            //            SetColor(color);
            //    }
            //}
        }

        public void RestoreDefaultColor(FeatureSlot slot)
        {
            _currentColor[0] = "#ffffff";
        }

        // internal void SetCurrentCategoryAndColors(FeatureSlot category, List<FeatureColor> colors) {
        //     _currentlySelectedCategory = category;
        //     _currentColors = colors;
        // }
    }
}
