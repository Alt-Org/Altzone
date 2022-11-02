using Altzone.Scripts.Config;
using Photon.Pun;
using UnityConstants;
using UnityEngine;

namespace Battle0.Scripts.Battle.Brick
{
    [RequireComponent(typeof(PhotonView))]
    internal class BrickRemove : MonoBehaviour
    {
        private void Awake()
        {
            var isDisableBricks = RuntimeGameConfig.Get().Features._isDisableBricks;
            if (isDisableBricks)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}