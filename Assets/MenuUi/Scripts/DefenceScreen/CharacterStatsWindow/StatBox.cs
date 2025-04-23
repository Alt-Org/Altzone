using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Controls visual functionality of StatBox
    /// </summary>
    public class StatBox : MonoBehaviour
    {
        [SerializeField] private StatType _statType;
        [SerializeField] private Image _statIcon;
        [SerializeField] private Image _statBackground;
        [SerializeField] private TMP_Text _statName;
        [SerializeField] private TMP_Text _statLevel;
        [SerializeField] private TMP_Text _statValue;
        [SerializeField] private TMP_Text _diamondCost;
        [SerializeField] private TMP_Text _eraserCost;
        [SerializeField] private Button _eraserButton;
        [SerializeField] private Button _diamondButton;
        [SerializeField] private TMP_Text _statDescription;
        [SerializeField] private GameObject _contents;
        [SerializeField] private GameObject _defenceIconColor;
        [SerializeField] private TMP_Text _developmentName;

        private StatsWindowController _controller;
        StatInfo _statInfo;

        private void OnEnable()
        {
            if (_contents != null) _contents.SetActive(false);
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();
            if (_statLevel != null) _controller.OnStatUpdated += UpdateStatLevel;
            if (_diamondCost != null) _controller.OnStatUpdated += UpdateDiamondCost;
            if (_eraserCost != null) _controller.OnStatUpdated += UpdateEraserCost;
            if (_statValue != null) _controller.OnStatUpdated += UpdateStatValue;
            if (_statValue != null) UpdateStatValue(_statType);
            if (_statLevel != null) UpdateStatLevel(_statType);

        }

        private void Awake()
        {
            if (_diamondButton != null) _diamondButton.onClick.AddListener(IncreaseStat);
            if (_eraserButton != null) _eraserButton.onClick.AddListener(DecreaseStat);
        }

        private void OnDisable()
        {
            if (_statLevel != null) _controller.OnStatUpdated -= UpdateStatLevel;
            if (_diamondCost != null) _controller.OnStatUpdated -= UpdateDiamondCost;
            if (_eraserCost != null) _controller.OnStatUpdated -= UpdateEraserCost;
            if (_statValue != null) _controller.OnStatUpdated -= UpdateStatValue;

        }

        private void OnDestroy()
        {
            if (_diamondButton != null) _diamondButton.onClick.RemoveAllListeners();
            if (_eraserButton != null) _eraserButton.onClick.RemoveAllListeners();
        }

        public void ChangeStatBox(int statType)
        {
            _contents.SetActive(true);
            _statType = (StatType)statType;
            _statInfo = _controller.GetStatInfo(_statType);
            _statIcon.sprite = _statInfo.Image;
            _statBackground.color = _statInfo.StatBoxColor;
            _statDescription.text = _statInfo.Description;
            string developmentName = string.Empty;
            ValueStrength statStrenght = _controller.GetStatStrength(_statType);
            switch(statStrenght)
            {
                case ValueStrength.VeryStrong:
                    developmentName = "Mestari";
                    break;
                case ValueStrength.Strong:
                    developmentName = "Asiantuntija";
                    break;
                case ValueStrength.SemiStrong:
                    developmentName = "Kokenut";
                    break;
                case ValueStrength.Medium:
                    developmentName = "Perusosaaja";
                    break;
                case ValueStrength.SemiWeak:
                    developmentName = "Harjoittelija";
                    break;
                case ValueStrength.Weak:
                    developmentName = "Aloittelija";
                    break;
                case ValueStrength.VeryWeak:
                    developmentName = "Taidoton";
                    break;
                case ValueStrength.None:
                default:
                    developmentName = "Ei tietoa";
                    break;
            }
            _developmentName.text = developmentName;

            UpdateStatLevel(_statType);
            UpdateStatValue(_statType);
            UpdateDiamondCost(_statType);
            UpdateEraserCost(_statType);

            if (_statType == StatType.Defence)
            {
                _defenceIconColor.SetActive(true);
            }
            else
            {
                _defenceIconColor.SetActive(false);
            }
        }

        private void UpdateStatLevel(StatType statType)
        {
            if (statType != _statType) return;
            _statLevel.text = _controller.GetStat(statType).ToString();
        }

        private void UpdateStatValue(StatType statType)
        {
            if (statType != _statType) return;
            _statValue.text = _controller.GetStatValue(statType).ToString();
        }

        private void UpdateDiamondCost(StatType statType)
        {
            if (statType != _statType) return;
            int diamondCost = _controller.GetDiamondCost(statType);
            _diamondCost.text = diamondCost.ToString();
            if (_controller.CheckIfEnoughDiamonds(diamondCost))
            {
                _diamondCost.color = Color.black;
            }
            else
            {
                _diamondCost.color = Color.red;
            }
        }


        private void UpdateEraserCost(StatType statType)
        {
            if (statType != _statType) return;
            _eraserCost.text = "1";
            if (_controller.CheckIfEnoughErasers(1))
            {
                _eraserCost.color = Color.black;
            }
            else
            {
                _eraserCost.color = Color.red;
            }
        }

        private bool CanUpdateCharacter()
        {
            if (_controller.IsCurrentCharacterLocked())
            {
                PopupSignalBus.OnChangePopupInfoSignal("Et voi muokata lukittua hahmoa.");
                return false;
            }

            if (_controller.GetCurrentCharacterClass() == CharacterClassID.Obedient) // obedient characters can't be modified
            {
                PopupSignalBus.OnChangePopupInfoSignal("Tottelijoita ei voi muokata.");
                return false;
            }
            return true;
        }
        private void IncreaseStat()
        {
            if (!CanUpdateCharacter()) return;
            _controller.TryIncreaseStat(_statType);
        }

        private void DecreaseStat()
        {
            if (!CanUpdateCharacter()) return;
            _controller.TryDecreaseStat(_statType);
        }
    }
}
