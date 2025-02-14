using MenuUi.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.BattleButton
{
    public class GameTypeOption : MonoBehaviour
    {
        [SerializeField] public Button ButtonComponent;
        [SerializeField] private Image _gameTypeImage;
        [SerializeField] private TMP_Text _gameTypeText;

        private GameTypeInfo _gameTypeInfo;

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
        public void SetInfo(GameTypeInfo info)
        {
            _gameTypeImage.sprite = info.Icon;
            _gameTypeText.text = info.Name;
            _gameTypeInfo = info;
        }
    }
}
