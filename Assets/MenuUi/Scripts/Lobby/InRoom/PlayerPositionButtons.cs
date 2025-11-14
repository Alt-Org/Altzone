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
        [SerializeField] private PlayerPos[] positions;

        private static readonly int[] PositionMap =
        {
            PhotonBattleRoom.PlayerPosition1, PhotonBattleRoom.PlayerPosition2, PhotonBattleRoom.PlayerPosition3, PhotonBattleRoom.PlayerPosition4,
        };

        private void Start()
        {
            for (var i = 0; i < positions.Length; ++i)
            {
                var capturedPositionIndex = i;
                positions[i]._button.onClick.AddListener(() => SetPlayerPosition(capturedPositionIndex));
                positions[i]._botToggle.onValueChanged.AddListener((value) => SetPositionBotToggle(capturedPositionIndex, value));
            }
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
            Debug.Log($"SetPlayerPosition {positionIndex}");
            if (positionIndex < 0 || positionIndex >= PositionMap.Length)
            {
                throw new UnityException($"invalid positionIndex: {positionIndex}");
            }
            this.Publish(new LobbyManager.PlayerPosEvent(PositionMap[positionIndex]));
        }

        [System.Serializable]
        private class PlayerPos
        {
            public Button _button;
            public Toggle _botToggle;
        }
    }
}
