using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            }
            else
            {
                _cancelButton.gameObject.SetActive(false);
            }
        }
    }
}
