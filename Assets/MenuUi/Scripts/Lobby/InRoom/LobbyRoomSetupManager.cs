using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using UnityEngine;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>Two opposing player slots for the 1v1 lobby.</summary>
    public class LobbyRoomSetupManager : RoomSetupBase
    {
        [SerializeField] private InRoomPlayerSlot _alphaPlayerSlot;
        [SerializeField] private InRoomPlayerSlot _betaPlayerSlot;

        protected override void InitializeSlots()
        {
            // Slot events use zero-based indices; opposing teams occupy positions 1 and 3.
            if (_alphaPlayerSlot != null) _alphaPlayerSlot.Initialize(0);
            if (_betaPlayerSlot != null) _betaPlayerSlot.Initialize(2);
        }

        protected override void UpdateStatus()
        {
            if (!isActiveAndEnabled || !PhotonRealtimeClient.InRoom) return;

            LobbyPlayer localPlayer = PhotonRealtimeClient.LocalLobbyPlayer;
            UpdateSlot(_alphaPlayerSlot, PlayerPosition1, localPlayer);
            UpdateSlot(_betaPlayerSlot, PlayerPosition3, localPlayer);

            int position = localPlayer.GetCustomProperty(PlayerPositionKey, 0);
            if (_buttonStartPlay != null)
                _buttonStartPlay.interactable = localPlayer.IsMasterClient &&
                    (position == PlayerPosition1 || position == PlayerPosition3);
        }

        private void UpdateSlot(InRoomPlayerSlot slot, int position, LobbyPlayer localPlayer)
        {
            if (slot == null) return;

            foreach (LobbyPlayer player in PhotonRealtimeClient.GetCurrentRoomPlayers())
            {
                if (player.GetCustomProperty(PlayerPositionKey, 0) != position) continue;
                int[] characters = player.GetCustomProperty<int[]>(PlayerCharactersKey, null);
                if (characters == null || characters.Length == 0) continue;

                slot.SetCharacters(player.UserId, player.NickName, characters,
                    player.Equals(localPlayer), player.IsMasterClient, localPlayer.IsMasterClient);
                slot.SetReadyState(player.GetCustomProperty(PlayerReadyKey, false));
                return;
            }

            slot.SetCharacters(null, null, null, false, false, localPlayer.IsMasterClient);
            if (PhotonBattleRoom.CheckIfPositionHasBot(position))
                slot.SetBotCharacters(localPlayer.IsMasterClient);
        }

        protected override void ResetSlots()
        {
            if (_buttonStartPlay != null) _buttonStartPlay.interactable = false;
            if (_alphaPlayerSlot != null) _alphaPlayerSlot.SetCharacters(null, null, null, false, false, false);
            if (_betaPlayerSlot != null) _betaPlayerSlot.SetCharacters(null, null, null, false, false, false);
        }
    }
}
