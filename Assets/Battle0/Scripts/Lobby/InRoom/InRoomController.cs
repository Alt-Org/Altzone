using UnityEngine;

namespace Battle0.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Controller for "InRoom" that is not used in any way!
    /// </summary>
    /// <remarks>
    /// It seems that <c>PaneInRoom</c> does the work (which should be here).
    /// </remarks>
    public class InRoomController : MonoBehaviour
    {
        [SerializeField] private InRoomView _view;

        private void Awake()
        {
            Debug.Log("Awake");
            _view.Player1Button.onClick.AddListener(Player1Button);
            _view.Player2Button.onClick.AddListener(Player2Button);
            _view.Player3Button.onClick.AddListener(Player3Button);
            _view.Player4Button.onClick.AddListener(Player4Button);
            _view.StartButton.onClick.AddListener(StartButton);
            _view.RaidButton.onClick.AddListener(RaidButton);
            _view.SpectatorButton.onClick.AddListener(SpectatorButton);
            _view.QuestButton.onClick.AddListener(QuestButton);
        }

        private void Player1Button()
        {
            Debug.Log("Player1Button");
        }
        private void Player2Button()
        {
            Debug.Log("Player2Button");
        }
        private void Player3Button()
        {
            Debug.Log("Player3Button");
        }
        private void Player4Button()
        {
            Debug.Log("Player4Button");
        }

        private void StartButton()
        {
            Debug.Log("StartButton");
        }

        private void RaidButton()
        {
            Debug.Log("StartButton");
        }
        private void SpectatorButton()
        {
            Debug.Log("SpectatorButton");
        }
        private void QuestButton()
        {
            Debug.Log("QuestButton");
        }
    }
}
