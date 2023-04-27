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
        [SerializeField] Transform DiamondCounters2;
        [SerializeField] RectTransform BetaDiamonds2;
        [SerializeField] RectTransform AlphaDiamonds2;

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
                    AlphaDiamonds.anchoredPosition = new Vector2(BetaDiamonds.anchoredPosition.x, BetaDiamonds.anchoredPosition.y);
                    AlphaDiamonds2.anchoredPosition = new Vector2(BetaDiamonds2.anchoredPosition.x, BetaDiamonds2.anchoredPosition.y);
                    BetaDiamonds.anchoredPosition = new Vector2(-BetaDiamonds2.anchoredPosition.x, -BetaDiamonds2.anchoredPosition.y);
                    BetaDiamonds2.anchoredPosition = new Vector2(-AlphaDiamonds.anchoredPosition.x, -AlphaDiamonds.anchoredPosition.y);
                }
            }
        }
    }
}