using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Controls visual functionality of StatUpdatePopUp.
    /// </summary>
    public class StatUpdatePopUp : MonoBehaviour
    {
        [SerializeField] private StatsReference _statsReference;
        [SerializeField] private GameObject _contents;
        [SerializeField] private Image _statIcon;
        [SerializeField] private TMP_Text _statName;
        [SerializeField] private TMP_Text _statNumber;
        [SerializeField] private TMP_Text _diamondCost;
        [SerializeField] private TMP_Text _eraserCost;
        [SerializeField] private Image _touchBlocker;

        private StatsWindowController _controller;
        private StatType _statType;


        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();
            ClosePopUp();
            _controller.OnStatUpdated += UpdateStatNumber;
            _controller.OnStatUpdated += UpdateDiamondCost;
            _controller.OnStatUpdated += UpdateEraserCost;
        }


        private void OnDisable()
        {
            _controller.OnStatUpdated -= UpdateStatNumber;
            _controller.OnStatUpdated -= UpdateDiamondCost;
            _controller.OnStatUpdated -= UpdateEraserCost;
        }


        /// <summary>
        /// Open StatUpdatePopUp
        /// </summary>
        /// <param name="statType">Stat type int which is compared to StatType enum. (is there a better way to do this?)</param>
        public void OpenPopUp(int statType)
        {
            StatInfo statInfo = null;

            if (_controller.IsCurrentCharacterLocked())
            {
                PopupSignalBus.OnChangePopupInfoSignal("Et voi muokata lukittua hahmoa.");
                ClosePopUp();
                return;
            }

            if (_controller.GetCurrentCharacterClass() == CharacterClassType.Obedient) // obedient characters can't be modified
            {
                PopupSignalBus.OnChangePopupInfoSignal("Tottelijoita ei voi muokata.");
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
                case StatType.CharacterSize:
                    statInfo = _statsReference.GetStatInfo(StatType.CharacterSize);
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
                UpdateDiamondCost(_statType);
                UpdateEraserCost(_statType);
                UpdateStatNumber(_statType);

                _statIcon.sprite = statInfo.Image;
                _statName.text = statInfo.Name;

                _touchBlocker.enabled = true;
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
            _touchBlocker.enabled = false;
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
            if (_controller.CanIncreaseStat(statType))
            {
                _diamondCost.text = _controller.GetUpgradeMaterialCost(statType).ToString();
            }
            else
            {
                _diamondCost.text = "-";
            }
        }


        private void UpdateEraserCost(StatType statType)
        {
            if (_controller.CanDecreaseStat(statType))
            {
                _eraserCost.text = "1";
            }
            else
            {
                _eraserCost.text = "-";
            }
        }
    }
}
