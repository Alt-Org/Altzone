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

        public bool IsVisible => _view.activeSelf;

        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        public void SetStats(BattlePlayerStats stats)
        {
            _impactForce.Value.text = stats.StatAttack.ToString();
            _hp.Value.text          = stats.StatHp.ToString();
            _speed.Value.text       = stats.StatSpeed.ToString();
            _charSize.Value.text    = stats.StatCharacterSize.ToString();
            _defence.Value.text     = stats.StatDefence.ToString();
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

