using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class CameraRotateTest : MonoBehaviour
    {
        [SerializeField] Transform Camera;
        [SerializeField] Transform Background;
        [SerializeField] Transform GridOverlay;
        [SerializeField] Transform DiamondCounters;
        [SerializeField] RectTransform BetaDiamonds;
        [SerializeField] RectTransform AlphaDiamonds;
        [SerializeField] TeamDiamondCount BetaDiamondsCount;
        [SerializeField] TeamDiamondCount AlphaDiamondsCount;

        private int TeamNumber;
        private bool Over99 = false;
        private bool Over999 = false;

        private void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                var player = PhotonNetwork.LocalPlayer;
                var playerPos = PhotonBattle.GetPlayerPos(player);
                TeamNumber = (int)PhotonBattle.GetTeamNumber(playerPos);
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

        private void Update()
        {
            if (TeamNumber == 1)
            {
                if (BetaDiamondsCount.TeamDiamondCounter > 99 && Over99 == false)
                {
                    BetaDiamonds.anchoredPosition  = new Vector2(256.0f, BetaDiamonds.anchoredPosition.y);
                    Over99 = true;
                }

                if (BetaDiamondsCount.TeamDiamondCounter > 999 && Over999 == false)
                {
                    BetaDiamonds.anchoredPosition = new Vector2(210.0f, BetaDiamonds.anchoredPosition.y);
                    Over999 = true;
                }
            }

            if (TeamNumber == 2)
            {
                if (AlphaDiamondsCount.TeamDiamondCounter > 99 && Over99 == false)
                {
                    AlphaDiamonds.anchoredPosition = new Vector2(256.0f, AlphaDiamonds.anchoredPosition.y);
                    Over99 = true;
                }

                if (AlphaDiamondsCount.TeamDiamondCounter > 999 && Over999 == false)
                {
                    AlphaDiamonds.anchoredPosition = new Vector2(210.0f, AlphaDiamonds.anchoredPosition.y);
                    Over999 = true;
                }
            }
        }
    }
}
