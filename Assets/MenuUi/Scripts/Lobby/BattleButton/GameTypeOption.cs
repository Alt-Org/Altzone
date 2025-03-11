using MenuUi.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.BattleButton
{
    /// <summary>
    /// Used for GameTypeOption -prefabs which are instantiated in BattleButton.cs script. GameTypeOptions are shown in the popup menu which opens from BT_ALTZONE where you can select game type.
    /// </summary>
    public class GameTypeOption : MonoBehaviour
    {
        [SerializeField] public Button ButtonComponent;
        [SerializeField] private Image _gameTypeImage;
        [SerializeField] private TMP_Text _gameTypeText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _unselectedColor;

        private GameTypeInfo _gameTypeInfo;

        public GameTypeInfo Info { get { return _gameTypeInfo; } }

        public delegate void GameTypeOptionSelectedHandler(GameTypeInfo gameTypeInfo);
        public GameTypeOptionSelectedHandler OnGameTypeOptionSelected;


        private void Awake()
        {
            ButtonComponent.onClick.AddListener(OnButtonPressed);
        }


        private void OnDestroy()
        {
            ButtonComponent.onClick.RemoveListener(OnButtonPressed);
        }


        private void OnButtonPressed()
        {
            OnGameTypeOptionSelected?.Invoke(_gameTypeInfo);
        }

        /// <summary>
        /// Set visual info for this game type option.
        /// </summary>
        /// <param name="info">GameTypeInfo object which has data for this game type option button.</param>
        /// <param name="selected">If this game type option is selected or no boolean.</param>
        public void SetInfo(GameTypeInfo info, bool selected)
        {
            _gameTypeImage.sprite = info.Icon;
            _gameTypeText.text = info.Name;
            _gameTypeInfo = info;
            SetSelected(selected);
        }

        /// <summary>
        /// Set selected background color to this game type option.
        /// </summary>
        /// <param name="selected">If this game type option is selected or no boolean.</param>
        public void SetSelected(bool selected)
        {
            if (selected)
            {
                _backgroundImage.color = _selectedColor;
            }
            else
            {
                _backgroundImage.color = _unselectedColor;
            }
        }
    }
}
