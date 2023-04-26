using UnityEngine;
using UnityEngine.UI;

namespace Battle0.Scripts.Lobby.InRoom
{
    /// <summary>
    /// View for "InRoom" that is not used in any way!
    /// </summary>
    public class InRoomView : MonoBehaviour
    {
        [SerializeField] private Button _player1Button;
        [SerializeField] private Button _player2Button;
        [SerializeField] private Button _player3Button;
        [SerializeField] private Button _player4Button;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _raidButton;
        [SerializeField] private Button _spectatorButton;
        [SerializeField] private Button _questButton;

        public Button Player1Button => _player1Button;
        public Button Player2Button => _player2Button;
        public Button Player3Button => _player3Button;
        public Button Player4Button => _player4Button;
        public Button StartButton => _startButton;
        public Button RaidButton => _raidButton;
        public Button SpectatorButton => _spectatorButton;
        public Button QuestButton => _questButton;
    }
}