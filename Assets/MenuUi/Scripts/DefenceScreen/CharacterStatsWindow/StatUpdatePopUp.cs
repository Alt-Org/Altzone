using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class StatUpdatePopUp : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private StatsReference _statsReference;
        [SerializeField] private GameObject _contents;
        [SerializeField] private Image _statIcon;
        [SerializeField] private TMP_Text _statName;
        [SerializeField] private TMP_Text _statNumber;
        [SerializeField] private TMP_Text _diamondCost;

        private StatType _statType;


        private void OnEnable()
        {
            ClosePopUp();
            _controller.OnStatUpdated += UpdateStatNumber;
            _controller.OnStatUpdated += UpdateDiamondCost;
        }


        private void OnDisable()
        {
            _controller.OnStatUpdated -= UpdateStatNumber;
            _controller.OnStatUpdated -= UpdateDiamondCost;
        }


        /// <summary>
        /// Open StatUpdatePopUp
        /// </summary>
        /// <param name="statType">Stat type int which is compared to StatType enum. (is there a better way to do this?)</param>
        public void OpenPopUp(int statType)
        {
            StatInfo statInfo = null;

            if (_controller.GetCurrentCharacterClass() == CharacterClassID.Obedient) // obedient characters can't be modified
            {
                ClosePopUp();
                return;
            }

            // Getting StatInfo from StatsReference sheet
            switch ((StatType)statType)
            {
                case StatType.None:
                    ClosePopUp();
                    break;
                case StatType.Attack:
                    statInfo = _statsReference.GetStatInfo(StatType.Attack);
                    break;
                case StatType.Defence:
                    statInfo = _statsReference.GetStatInfo(StatType.Defence);
                    break;
                case StatType.Resistance:
                    statInfo = _statsReference.GetStatInfo(StatType.Resistance);
                    break;
                case StatType.Hp:
                    statInfo = _statsReference.GetStatInfo(StatType.Hp);
                    break;
                case StatType.Speed:
                    statInfo = _statsReference.GetStatInfo(StatType.Speed);
                    break;
            }

            // Setting up stat popup
            if (statInfo != null)
            {
                _statType = (StatType)statType;
                _diamondCost.text = _controller.GetDiamondCost(_statType).ToString();
                _statNumber.text = _controller.GetStat(_statType).ToString();

                _statIcon.sprite = statInfo.Image;
                _statName.text = statInfo.Name;

                _contents.SetActive(true);
            }
            else
            {
                ClosePopUp();
            }
        }


        /// <summary>
        /// Close StatUpdatePopUp
        /// </summary>
        public void ClosePopUp()
        {
            _contents.SetActive(false);
        }


        /// <summary>
        /// Method for when plus button is clicked in the popup
        /// </summary>
        public void PlusButtonClicked()
        {
            _controller.TryIncreaseStat(_statType);
        }


        /// <summary>
        /// Method for when minus button is clicked in the popup
        /// </summary>
        public void MinusButtonClicked()
        {
            _controller.TryDecreaseStat(_statType);
        }


        private void UpdateStatNumber(StatType statType)
        {
            _statNumber.text = _controller.GetStat(statType).ToString();
        }


        private void UpdateDiamondCost(StatType statType)
        {
            _diamondCost.text = _controller.GetDiamondCost(statType).ToString();
        }
    }
}
