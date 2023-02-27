using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battle0.Scripts.Lobby.InLobby
{
    public class InLobbyView : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _lobbyText;
        [SerializeField] private Button _characterButton;
        [SerializeField] private Button _roomButton;
        [SerializeField] private Button _raidButton;
        [SerializeField] private Button _quickGameButton;

        public Action CharacterButtonOnClick
        {
            set { _characterButton.onClick.AddListener(() => value()); }
        }
        public Action RoomButtonOnClick
        {
            set { _roomButton.onClick.AddListener(() => value()); }
        }
        public Action RaidButtonOnClick
        {
            set { _raidButton.onClick.AddListener(() => value()); }
        }
        public Action QuickGameButtonOnClick
        {
            set { _quickGameButton.onClick.AddListener(() => value()); }
        }
        
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
            DisableButtons();
        }

        private void DisableButtons()
        {
            _characterButton.interactable = false;
            _roomButton.interactable = false;
            _raidButton.interactable = false;
            _quickGameButton.interactable = false;
        }

        public void EnableButtons()
        {
            _characterButton.interactable = true;
            _roomButton.interactable = true;
            _raidButton.interactable = true;
            _quickGameButton.interactable = true;
        }
    }
}
