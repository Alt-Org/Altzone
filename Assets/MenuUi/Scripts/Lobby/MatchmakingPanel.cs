using Altzone.Scripts.Lobby;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Lobby.InLobby;

namespace MenuUi.Scripts.Lobby
{
    public class MatchmakingPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _matchmakingText;
        [SerializeField] private Button _cancelButton;
        private void OnEnable()
        {
            if (PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
            {
                _cancelButton.gameObject.SetActive(true);
                _cancelButton.onClick.AddListener(() => this.Publish(new LobbyManager.StopMatchmakingEvent(InLobbyController.SelectedGameType)));
            }
            else
            {
                _cancelButton.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            _cancelButton.onClick.RemoveAllListeners();
        }
    }
}
