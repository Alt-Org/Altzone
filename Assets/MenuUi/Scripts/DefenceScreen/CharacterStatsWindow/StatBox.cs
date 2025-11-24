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
        [SerializeField] private Image _statLock;
        [SerializeField] private TMP_Text _statName;
        [SerializeField] private TMP_Text _statLevel;
        [SerializeField] private TMP_Text _statLevel2;
        [SerializeField] private TMP_Text _statNextLevel;
        [SerializeField] private TMP_Text _statValue;
        [SerializeField] private TMP_Text _statNextLevelValue;
        [SerializeField] private TMP_Text _diamondCost;
        [SerializeField] private Button _eraserButton;
        [SerializeField] private Button _diamondButton;
        [SerializeField] private TMP_Text _statDescription;
        [SerializeField] private GameObject _contents;
        [SerializeField] private TMP_Text _developmentName;


        private StatsWindowController _controller;
        StatInfo _statInfo;

        private void OnEnable()
        {
            if (_contents != null) _contents.SetActive(false);
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();
            if (_statLevel != null) _controller.OnStatUpdated += UpdateStatLevel;
            if (_diamondCost != null) _controller.OnStatUpdated += UpdateDiamondCost;
            if (_statValue != null) _controller.OnStatUpdated += UpdateStatValue;
            if (_statValue != null) UpdateStatValue(_statType);
            if (_statLevel != null) UpdateStatLevel(_statType);
            if (_statLock != null)
                if (!SettingsCarrier.Instance.StatDebuggingMode)
                {
                    if (_statType is StatType.Speed or StatType.CharacterSize) _statLock.enabled = true;
                    else _statLock.enabled = false;
                }
                else _statLock.enabled = false;
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

            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    _statName.text = _statInfo.Name;
                    _statDescription.text = _statInfo.Description;
                    break;

                case SettingsCarrier.LanguageType.English:
                    _statName.text = string.IsNullOrEmpty(_statInfo.EnglishName) ? _statInfo.Name : _statInfo.EnglishName;
                    _statDescription.text = string.IsNullOrEmpty(_statInfo.EnglishDescription) ? _statInfo.Description : _statInfo.EnglishDescription;
                    break;

                default:
                    goto case SettingsCarrier.LanguageType.Finnish;
            }

            string developmentName = string.Empty;
            ValueStrength statStrenght = _controller.GetStatStrength(_statType);
            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    switch (statStrenght)
                    {
                        case ValueStrength.VeryStrong: developmentName = "Mestari"; break;
                        case ValueStrength.Strong: developmentName = "Asiantuntija"; break;
                        case ValueStrength.SemiStrong: developmentName = "Kokenut"; break;
                        case ValueStrength.Medium: developmentName = "Perusosaaja"; break;
                        case ValueStrength.SemiWeak: developmentName = "Harjoittelija"; break;
                        case ValueStrength.Weak: developmentName = "Aloittelija"; break;
                        case ValueStrength.VeryWeak: developmentName = "Taidoton"; break;
                        case ValueStrength.None:
                        default: developmentName = "Ei tietoa"; break;
                    }
                    break;

                case SettingsCarrier.LanguageType.English:
                    switch (statStrenght)
                    {
                        case ValueStrength.VeryStrong: developmentName = "Master"; break;
                        case ValueStrength.Strong: developmentName = "Expert"; break;
                        case ValueStrength.SemiStrong: developmentName = "Experienced"; break;
                        case ValueStrength.Medium: developmentName = "Competent"; break;
                        case ValueStrength.SemiWeak: developmentName = "Apprentice"; break;
                        case ValueStrength.Weak: developmentName = "Beginner"; break;
                        case ValueStrength.VeryWeak: developmentName = "Unskilled"; break;
                        case ValueStrength.None:
                        default: developmentName = "No data"; break;
                    }
                    break;

                default: 
                    goto case SettingsCarrier.LanguageType.Finnish;
            }
            _developmentName.text = developmentName;

            UpdateStatLevel(_statType);
            UpdateStatValue(_statType);
            UpdateDiamondCost(_statType);
        }

        private void UpdateStatLevel(StatType statType)
        {
            if (statType != _statType) return;
            int statLevel = _controller.GetStat(statType);
            if (_statLevel != null) _statLevel.text = statLevel.ToString();
            if (_statLevel2 != null) _statLevel2.text = statLevel.ToString();
            if (_statNextLevel != null) _statNextLevel.text = (statLevel+1).ToString();
        }

        private void UpdateStatValue(StatType statType)
        {
            if (statType != _statType) return;
            _statValue.text = _controller.GetStatValue(statType).ToString();
            if (_statNextLevelValue == null)
            {
                return;
            }
            int statLevel = _controller.GetStat(statType);
            _statNextLevelValue.text = _controller.GetStatValue(statType,statLevel+1).ToString();
            if(_statLock != null)
            if (!SettingsCarrier.Instance.StatDebuggingMode)
            {
                if(_statType is StatType.Speed or StatType.CharacterSize) _statLock.enabled = true;
                else _statLock.enabled = false;
            }
            else _statLock.enabled = false;
        }

        private void UpdateDiamondCost(StatType statType)
        {
            if (statType != _statType) return;
            int diamondCost = _controller.GetUpgradeMaterialCost(statType);
            _diamondCost.text = diamondCost.ToString();
            if (_controller.CheckIfEnoughUpgradeMaterial(diamondCost))
            {
                _diamondCost.color = Color.black;
            }
            else
            {
                _diamondCost.color = Color.red;
            }
        }

        private bool CanUpdateCharacter()
        {
            if (_controller.IsCurrentCharacterLocked())
            {
                PopupSignalBus.OnChangePopupInfoSignal("Et voi muokata lukittua hahmoa.");
                return false;
            }

            if (_controller.GetCurrentCharacterClass() == CharacterClassType.Obedient) // obedient characters can't be modified
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
