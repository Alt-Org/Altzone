using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Ball
{
    public class StartTheBall : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"{name}");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
        }

        public void StartBall1()
        {
            var ball = BallManager.Get();
            Debug.Log($"{name} {ball}");
            Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
            Assert.IsTrue(PhotonNetwork.IsMasterClient, "PhotonNetwork.IsMasterClient");
            Assert.IsNotNull(ball, "ball != null");
        }

        public void StartBall2()
        {
            Debug.Log($"{name}");
        }
    }
}
