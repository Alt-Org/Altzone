using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class StatsPanel : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private Image _characterImage;
        [SerializeField] private TMP_Text _attackText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _defenceText;
        [SerializeField] private TMP_Text _charSizeText;
        [SerializeField] private TMP_Text _speedText;

        private void OnEnable()
        {
            SetCharacterImage();
            SetStatButtonTexts();

            _controller.OnStatUpdated += SetStatButtonTexts;
        }


        private void OnDisable()
        {
            _controller.OnStatUpdated -= SetStatButtonTexts;
        }


        private void SetCharacterImage()
        {
            Sprite sprite = _controller.GetCurrentCharacterSprite();

            if (sprite != null)
            {
                _characterImage.sprite = sprite;
            }
        }


        private void SetStatButtonTexts(StatType statType = StatType.None)
        {
            if(statType == StatType.None)
            {
                _attackText.text = _controller.GetStat(StatType.Attack).ToString();
                _hpText.text = _controller.GetStat(StatType.Hp).ToString();
                _defenceText.text = _controller.GetStat(StatType.Defence).ToString();
                _charSizeText.text = _controller.GetStat(StatType.Resistance).ToString();
                _speedText.text = _controller.GetStat(StatType.Speed).ToString();
            }
            else
            {
                switch (statType)
                {
                    case StatType.Attack:
                        _attackText.text = _controller.GetStat(StatType.Attack).ToString();
                        break;
                    case StatType.Hp:
                        _hpText.text = _controller.GetStat(StatType.Hp).ToString();
                        break;
                    case StatType.Defence:
                        _defenceText.text = _controller.GetStat(StatType.Defence).ToString();
                        break;
                    case StatType.Resistance:
                        _charSizeText.text = _controller.GetStat(StatType.Resistance).ToString();
                        break;
                    case StatType.Speed:
                        _speedText.text = _controller.GetStat(StatType.Speed).ToString();
                        break;
                }
            }
        }
    }
}
