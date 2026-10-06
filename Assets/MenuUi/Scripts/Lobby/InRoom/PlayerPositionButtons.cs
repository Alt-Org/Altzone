using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Handles publishing PlayerPosEvent when one of the position buttons is pressed.
    /// </summary>
    public class PlayerPositionButtons : MonoBehaviour
    {

        private static readonly int[] PositionMap =
        {
            PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2, PhotonBattleRoom.PlayerPosition3, PhotonBattleRoom.PlayerPosition4,
        };

        private void Start()
        {
            InRoomPlayerSlot.OnSetPositon += SetPlayerPosition;
            InRoomPlayerSlot.OnSetBot += SetPositionBotToggle;
            InRoomPlayerSlot.OnSetReady += SetReadyToggle;
        }

        private void SetPlayerPosition(int positionIndex)
        {
            Debug.Log($"SetPlayerPosition {positionIndex}");
            if (positionIndex < 0 || positionIndex >= PositionMap.Length)
            {
                throw new UnityException($"invalid positionIndex: {positionIndex}");
            }
            this.Publish(new LobbyManager.PlayerPosEvent(PositionMap[positionIndex]));
        }

        private void SetPositionBotToggle(int positionIndex, bool value)
        {
            Debug.Log($"SetPositionBotToggle {positionIndex}:{value}");
            if (positionIndex < 0 || positionIndex >= PositionMap.Length)
            {
                throw new UnityException($"invalid positionIndex: {positionIndex}");
            }
            this.Publish(new LobbyManager.BotToggleEvent(PositionMap[positionIndex], value));
        }

        private void SetReadyToggle(int positionIndex, bool value)
        {
            Debug.Log($"SetReadyToggle {positionIndex}:{value}");
            if (positionIndex < 0 || positionIndex >= PositionMap.Length)
            {
                throw new UnityException($"invalid positionIndex: {positionIndex}");
            }
            this.Publish(new LobbyManager.ReadyToggleEvent(PositionMap[positionIndex], value));
        }

        [System.Serializable]
        private class PlayerPos
        {
            public Button _button;
            public Toggle _botToggle;
        }
    }
}
