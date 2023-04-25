using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class CameraRotate : MonoBehaviour
    {
        [SerializeField] Transform Camera;
        [SerializeField] Transform Background;
        [SerializeField] Transform GridOverlay;
        [SerializeField] Transform DiamondCounters;
        [SerializeField] RectTransform BetaDiamonds;
        [SerializeField] RectTransform AlphaDiamonds;

        private int TeamNumber;

        private void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                var player = PhotonNetwork.LocalPlayer;
                var playerPos = PhotonBattle.GetPlayerPos(player);
                TeamNumber = PhotonBattle.GetTeamNumber(playerPos);
                Debug.Log($"TeamNumber {TeamNumber} pos {playerPos} {player.GetDebugLabel()}");
                if (TeamNumber == 2)   //2
                {
                    Camera.eulerAngles = new Vector3(0, 0, 180);
                    Background.eulerAngles = new Vector3(0, 0, 180);
                    GridOverlay.eulerAngles = new Vector3(0, 0, 180);
                    DiamondCounters.eulerAngles = new Vector3(0, 0, 180);
                    BetaDiamonds.eulerAngles = new Vector3(0, 0, 0);
                    AlphaDiamonds.eulerAngles = new Vector3(0, 0, 0);
                }
            }
        }
    }
}