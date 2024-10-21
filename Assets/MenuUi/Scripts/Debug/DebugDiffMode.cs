using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugDiffMode : MonoBehaviour
    {
        [SerializeField] private LogBoxController _logBoxController;
        [SerializeField] private TextMeshProUGUI _OnOffText;
        [SerializeField] private Toggle _diffToggle;

        // Start is called before the first frame update
        void Start()
        {
            if (_logBoxController.DiffMode)
            {
                _diffToggle.isOn = true;
                ChangeColor(_diffToggle, Color.white);
                _OnOffText.text = "Päällä";
            }
            else
            {
                _diffToggle.isOn = false;
                ChangeColor(_diffToggle, Color.grey);
                _OnOffText.text = "Pois Päältä";
            }
            _diffToggle.onValueChanged.AddListener(DiffToggle);
        }

        private void DiffToggle(bool toggle)
        {
            if (toggle)
            {
                ChangeColor(_diffToggle, Color.white);
                _OnOffText.text = "Päällä";
            }
            else
            {
                ChangeColor(_diffToggle, Color.grey);
                _OnOffText.text = "Pois Päältä";
            }
            _logBoxController.SetDiffMode(toggle);
        }

        private void ChangeColor(Toggle toggle, Color colour)
        {
            ColorBlock block = toggle.colors;
            block.normalColor = colour;
            block.selectedColor = colour;
            toggle.colors = block;
        }
    }
}
