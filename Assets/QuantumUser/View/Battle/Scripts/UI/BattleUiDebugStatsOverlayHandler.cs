using System;

using UnityEngine;
using Quantum;

using TMPro;

namespace Battle.View.UI
{
    public class BattleUiDebugStatsOverlayHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _view;
        [SerializeField] private StatText _impactForce;
        [SerializeField] private StatText _hp;
        [SerializeField] private StatText _speed;
        [SerializeField] private StatText _charSize;
        [SerializeField] private StatText _defence;

        [SerializeField] private TMP_Text _referenceText;

        [Serializable]
        public struct StatText
        {
            public TMP_Text Name;
            public TMP_Text Value;

            public void SetSize(float size)
            {
                Name.fontSize = size;
                Value.fontSize = size;
            }
        }

        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        public void SetStats(BattleCharacterBase character)
        {
            _impactForce.Value.text = character.Attack.ToString();
            _hp.Value.text          = character.Hp.ToString();
            _speed.Value.text       = character.Speed.ToString();
            _charSize.Value.text    = character.CharacterSize.ToString();
            _defence.Value.text     = character.Defence.ToString();
        }

        private float _currentFontSize;

        private void Update()
        {
            if (_view.activeSelf && _referenceText.fontSize != _currentFontSize) ResizeFontSizes();
        }

        private void ResizeFontSizes()
        {
            _currentFontSize = _referenceText.fontSize;

            _impactForce .SetSize(_currentFontSize);
            _hp          .SetSize(_currentFontSize);
            _speed       .SetSize(_currentFontSize);
            _charSize    .SetSize(_currentFontSize);
            _defence     .SetSize(_currentFontSize);
        }
    }
}

