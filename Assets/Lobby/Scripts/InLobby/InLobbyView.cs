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

        public Button CharacterButton => _characterButton;
        public Button RoomButton => _roomButton;

        public string TitleText
        {
            set => _titleText.text = value;
        }

        public string LobbyText
        {
            set => _lobbyText.text = value;
        }
    }
}