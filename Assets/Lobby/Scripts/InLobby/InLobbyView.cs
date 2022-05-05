using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InLobby
{
    public class InLobbyView : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _lobbyText;
        [SerializeField] private Button _characterButton;
        [SerializeField] private Button _roomButton;
        [SerializeField] private Button _quickGameButton;

        public Button CharacterButton => _characterButton;
        public Button RoomButton => _roomButton;
        public Button QuickGameButton => _quickGameButton;

        public string TitleText
        {
            set => _titleText.text = value;
        }

        public string LobbyText
        {
            set => _lobbyText.text = value;
        }

        public void Reset()
        {
            _titleText.text = string.Empty;
            _lobbyText.text = string.Empty;
            QuickGameButton.gameObject.SetActive(false);
            DisableButtons();
        }

        public void ShowDebugButtons()
        {
            QuickGameButton.gameObject.SetActive(true);
        }
        
        private void DisableButtons()
        {
            CharacterButton.interactable = false;
            RoomButton.interactable = false;
            QuickGameButton.interactable = false;
        }

        public void EnableButtons()
        {
            CharacterButton.interactable = true;
            RoomButton.interactable = true;
            QuickGameButton.interactable = true;
        }
    }
}