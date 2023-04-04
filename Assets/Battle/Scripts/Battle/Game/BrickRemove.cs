using UnityConstants;
using UnityEngine;
using Photon.Pun;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Removes a brick from the wall when hit conditions are met.
    /// </summary>
    internal class BrickRemove : MonoBehaviour
    {
        private PhotonView _photonView;
        private SpriteRenderer _spriteRenderer;
        public PlayerPlayArea PlayerPlayArea;
        private float _colorChangeFactor;
        private int Health = 0;

        private void Start()
        {
            Health = PlayerPlayArea.BrickHealth;
            _colorChangeFactor = 1f / Health;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _photonView = GetComponent<PhotonView>();
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag("BallRigidBody") && PhotonNetwork.IsMasterClient)    //Tags.Ball
            {
                _photonView.RPC(nameof(BrickHitRPC), RpcTarget.All);
            }
        }

        [PunRPC]
        private void BrickHitRPC()
        {
            var color = _spriteRenderer.color;
            color.g -= _colorChangeFactor;
            color.b -= _colorChangeFactor;
            _spriteRenderer.color = color;
            Health = Health - 1;
            if (Health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
