using System.Collections.Generic;
using MenuUi.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Signals;
using MenuUi.Scripts.SwipeNavigation;
using Altzone.Scripts.Lobby;
using TMPro;
using Altzone.Scripts.Window;
using Altzone.Scripts.Language;

namespace MenuUi.Scripts.Lobby.BattleButton
{
    /// <summary>
    /// Attached to BT_ALTZONE prefab. Has logic related to selecting game type for opening the battle popup.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BattleButton : MonoBehaviour
    {
        [SerializeField] private Image _gameTypeIcon;
        [SerializeField] private Image _gameTypeBanner;
        [SerializeField] private Image _gameTypeBackground;
        [SerializeField] private TextLanguageSelectorCaller _gameTypeName;
        [SerializeField] private TextLanguageSelectorCaller _gameTypeDescription;
        [SerializeField] private Button _openBattleUiEditorButton;
        [SerializeField] private GameTypeReference _gameTypeReference;
        [SerializeField] private GameObject _touchBlocker;

        private const string SelectedGameTypeKey = "BattleButtonGameType";

        private GameType _selectedGameType = GameType.Random2v2;

        private List<GameTypeOption> _gameTypeOptionList = new();
        private Button _button;
        private SwipeUI _swipe;

        public GameType SelectedGameType { get => _selectedGameType;}
        public Button Button { get => _button;}

        private void Awake()
        {
            _swipe = FindObjectOfType<SwipeUI>();
            SettingsCarrier.OnLanguageChanged += ChangeLanguage;

            _button = GetComponent<Button>();
            _button.onClick.AddListener(RequestBattlePopup);

            // Loading selected game type from player prefs Note: Only custom available for now
            _selectedGameType = GameType.Random2v2; //(GameType)PlayerPrefs.GetInt(SelectedGameTypeKey, (int)_selectedGameType);

            UpdateGameType(_gameTypeReference.GetGameTypeInfos().Find(x => x.gameType == _selectedGameType));

            _openBattleUiEditorButton.transform.SetAsLastSibling();
            _openBattleUiEditorButton.onClick.AddListener(OnOpenBattleUiEditorButtonPressed);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(RequestBattlePopup);
            _openBattleUiEditorButton.onClick.RemoveListener(OnOpenBattleUiEditorButtonPressed);
            SettingsCarrier.OnLanguageChanged -= ChangeLanguage;
        }


        private void RequestBattlePopup()
        {
            SignalBus.OnBattlePopupRequestedSignal(_selectedGameType);
        }

        public void UpdateGameType(GameTypeInfo gameTypeInfo)
        {
            _gameTypeIcon.sprite = gameTypeInfo.Icon;
            _gameTypeBanner.sprite = gameTypeInfo.Banner;
            _gameTypeBackground.sprite = gameTypeInfo.Background;
            _gameTypeName.SetText(gameTypeInfo.Name);
            _gameTypeDescription.SetText(gameTypeInfo.Description);
            _selectedGameType = gameTypeInfo.gameType;

            // Saving battle button selected game type to playerprefs
            PlayerPrefs.SetInt(SelectedGameTypeKey, (int)_selectedGameType);

            // Setting selected visuals for option buttons
            foreach (GameTypeOption gameTypeOption in _gameTypeOptionList)
            {
                bool selected = gameTypeOption.Info.gameType == _selectedGameType;
                gameTypeOption.SetSelected(selected);
            }
            
            // Opening battle popup after selecting a game type
            //SignalBus.OnBattlePopupRequestedSignal(_selectedGameType);
        }

        private void ChangeLanguage(SettingsCarrier.LanguageType language)
        {
            foreach (GameTypeInfo gameTypeInfo in _gameTypeReference.GetGameTypeInfos())
            {
                if (gameTypeInfo.gameType == _selectedGameType)
                {
                    _gameTypeName.SetText(gameTypeInfo.Name);
                    _gameTypeDescription.SetText(gameTypeInfo.Description);
                }          
            }

            for (int i = 0; i < _gameTypeOptionList.Count; i++)
            {
                GameTypeOption gameTypeOption = _gameTypeOptionList[i];

                foreach (GameTypeInfo gameTypeInfo in _gameTypeReference.GetGameTypeInfos())
                {
                    if (gameTypeInfo.gameType == gameTypeOption.Info.gameType)
                    {
                        bool selected = gameTypeInfo.gameType == _selectedGameType;
                        gameTypeOption.SetInfo(gameTypeInfo, selected);
                    }
                }
            }
        }


        private void OnOpenBattleUiEditorButtonPressed()
        {
            DataCarrier.AddData(DataCarrier.BattleUiEditorRequested, true);
        }
    }
}
