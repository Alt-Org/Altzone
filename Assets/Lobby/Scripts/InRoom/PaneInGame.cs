using Altzone.Scripts.Battle;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InRoom
{
    /// <summary>
    /// Top most pane in lobby while in room to manage player position in the game.
    /// </summary>
    public class PaneInGame : MonoBehaviour
    {
        [SerializeField] private Button[] buttons;

        private static readonly int[] PositionMap =
        {
            PhotonBattle.PlayerPosition1, PhotonBattle.PlayerPosition2, PhotonBattle.PlayerPosition3, PhotonBattle.PlayerPosition4,
        };

        private void Start()
        {
            for (var i = 0; i < buttons.Length; ++i)
            {
                var capturedPositionIndex = i;
                buttons[i].onClick.AddListener(() => SetPlayerPosition(capturedPositionIndex));
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
    }
}