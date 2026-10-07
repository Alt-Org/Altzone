using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Photon.Client.StructWrapping;
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
        [SerializeField] private TMP_Text _roomGameType;
        [SerializeField] private TMP_Text _roomPlayerCount;
        [SerializeField] private Image _openStatusLockImage;
        [SerializeField] private Image _backgroundImage;
        [Header("GameObject references")]
        [SerializeField] private Color _mainColour;
        [SerializeField] private Color _alternativeColour;
        [Header("Sprite references")]
        [SerializeField] private Sprite _lockedSprite;
        [SerializeField] private Sprite _unlockedSprite;


        /// <summary>
        /// Set visual elements for this room slot button.
        /// </summary>
        /// <param name="roomInfo">The room's info for this room slot button.</param>
        public void SetInfo(LobbyRoomInfo roomInfo, bool altColour)
        {
            if (altColour) _backgroundImage.color = _alternativeColour;
            else _backgroundImage.color = _mainColour;

            _roomName.text = roomInfo.Name;
            if (altColour) _roomName.color = Color.black;
            else _roomName.color = Color.white;
                _roomPlayerCount.text = $"{roomInfo.PlayerCount}/4";
            if (altColour) _roomPlayerCount.color = Color.black;
            else _roomPlayerCount.color = Color.white;

            if (roomInfo.CustomProperties.TryGetValue(PhotonBattleRoom.GameTypeKey, out int gameType))
            {
                _roomGameType.text = ((GameType)gameType).GetString();
                if (altColour) _roomGameType.color = Color.black;
                else _roomGameType.color = Color.white;
            }

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
