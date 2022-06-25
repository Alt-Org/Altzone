using Photon.Pun;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Brick
{
    [RequireComponent(typeof(PhotonView))]
    public class BrickRemove : MonoBehaviour
    {
        private int _viewId;

        private void Awake()
        {
            _viewId = PhotonView.Get(this).ViewID;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball))
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            Debug.Log($"{name} view {_viewId}");
        }
    }
}