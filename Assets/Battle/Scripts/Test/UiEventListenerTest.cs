using Battle.Scripts.Ui;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;

namespace Battle.Scripts.Test
{
    internal class UiEventListenerTest : MonoBehaviour
    {
        private void Awake()
        {
            ScoreFlashNet.RegisterEventListener();
        }

        private void OnEnable()
        {
            this.Subscribe<UiEvents.HeadCollision>(OnHeadCollision);
            this.Subscribe<UiEvents.ShieldCollision>(OnShieldCollision);
            this.Subscribe<UiEvents.WallCollision>(OnWallCollision);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private static void OnHeadCollision(UiEvents.HeadCollision data)
        {
            Debug.Log($"{data}");
            if (PhotonNetwork.IsMasterClient)
            {
                var collision = data.Collision;
                var contactPoint = collision.GetFirstContactPoint();
                ScoreFlashNet.Push("HEAD", contactPoint.point);
            }
        }

        private static void OnShieldCollision(UiEvents.ShieldCollision data)
        {
            Debug.Log($"{data}");
            if (PhotonNetwork.IsMasterClient)
            {
                var collision = data.Collision;
                var contactPoint = collision.GetFirstContactPoint();
                var info = data.HitType;
                ScoreFlashNet.Push($"SHIELD {info}", contactPoint.point);
            }
        }

        private static void OnWallCollision(UiEvents.WallCollision data)
        {
            Debug.Log($"{data}");
            if (PhotonNetwork.IsMasterClient)
            {
                var collision = data.Collision;
                var contactPoint = collision.GetFirstContactPoint();
                ScoreFlashNet.Push("WALL", contactPoint.point);
            }
        }
    }
}