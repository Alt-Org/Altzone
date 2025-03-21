using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby.Wrappers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Sets the visual elements for RoomSlot button which is instansiated to SearchPanel in Battle popup.
    /// </summary>
    public class RoomSlot : MonoBehaviour
    {
        [Header("GameObject references")]
        [SerializeField] private TMP_Text _roomName;
        [SerializeField] private TMP_Text _roomPlayerCount;
        [SerializeField] private Image _openStatusLockImage;
        [Header("Sprite references")]
        [SerializeField] private Sprite _lockedSprite;
        [SerializeField] private Sprite _unlockedSprite;


        /// <summary>
        /// Set visual elements for this room slot button.
        /// </summary>
        /// <param name="roomInfo">The room's info for this room slot button.</param>
        public void SetInfo(LobbyRoomInfo roomInfo)
        {
            _roomName.text = roomInfo.Name;
            _roomPlayerCount.text = $"{roomInfo.PlayerCount}/4";

            bool hasPassword = roomInfo.CustomProperties.ContainsKey(PhotonBattleRoom.PasswordKey);

            if (hasPassword)
            {
                _openStatusLockImage.sprite = _lockedSprite;
            }
            else
            {
                _openStatusLockImage.sprite = _unlockedSprite;
            }
        }
    }
}
