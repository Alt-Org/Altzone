using Photon.Pun;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Player
{
    internal class PlayerActor : MonoBehaviour
    {
        [SerializeField] private PhotonPlayerInstantiate _photonPlayerInstantiate;

        public void SetPhotonPlayerInstantiate(PhotonPlayerInstantiate photonPlayerInstantiate)
        {
            _photonPlayerInstantiate = photonPlayerInstantiate;
            enabled = true;
        }

        private void Awake()
        {
            enabled = false;
        }

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"{player.GetDebugLabel()}");
            _photonPlayerInstantiate.OnPhotonPlayerInstantiated(player);
        }
    }
}