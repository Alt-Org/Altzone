using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Controls visual functionality of CharacterStatsPanel.
    /// </summary>
    public class StatsPanel : MonoBehaviour
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _lockImage;
        [SerializeField] private TMP_Text _attackText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _defenceText;
        [SerializeField] private TMP_Text _charSizeText;
        [SerializeField] private TMP_Text _speedText;

        [SerializeField] private GameObject _statPage;
        [SerializeField] private GameObject _infoPage;
        private StatsWindowController _controller;

        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();

            SetCharacterImage();
            SetStatButtonTexts();

            _controller.OnStatUpdated += SetStatButtonTexts;

            if(_controller.IsCurrentCharacterLocked())
            {
                _lockImage.gameObject.SetActive(true);
            }
            else
            {
                _lockImage.gameObject.SetActive(false);
            }
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
                _charSizeText.text = _controller.GetStat(StatType.CharacterSize).ToString();
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
                    case StatType.CharacterSize:
                        _charSizeText.text = _controller.GetStat(StatType.CharacterSize).ToString();
                        break;
                    case StatType.Speed:
                        _speedText.text = _controller.GetStat(StatType.Speed).ToString();
                        break;
                }
            }
        }
        public void SwitchPage()
        {
            if (_statPage != null) _statPage.SetActive(false);
            if (_infoPage != null) _infoPage.SetActive(true);
        }
        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}
