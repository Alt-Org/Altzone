using UnityEngine;
using UnityEngine.UI;
using GameType = MenuUI.Scripts.Lobby.InLobby.InLobbyController;

namespace MenuUi.Scripts.Lobby.BattleButton
{
    [RequireComponent(typeof(Button))]
    public class GameTypeOption : MonoBehaviour
    {
        [SerializeField] private GameType _gameType;
        [SerializeField] private Image _gameTypeImage;

        [HideInInspector] public Button ButtonComponent;

        public delegate void GameTypeOptionSelectedHandler(Sprite optionSprite, GameType gameType);
        public GameTypeOptionSelectedHandler OnGameTypeOptionSelected;


        private void Awake()
        {
            ButtonComponent = GetComponent<Button>();
            ButtonComponent.onClick.AddListener(OnButtonPressed);
        }


        private void OnDestroy()
        {
            ButtonComponent.onClick.RemoveListener(OnButtonPressed);
        }


        private void OnButtonPressed()
        {
            OnGameTypeOptionSelected?.Invoke(_gameTypeImage.sprite, _gameType);
        }
    }
}
