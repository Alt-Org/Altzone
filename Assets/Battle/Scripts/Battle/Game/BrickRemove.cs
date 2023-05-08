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
        
        public void BrickHitInit(int damage)
        {
            _photonView.RPC(nameof(BrickHitRPC), RpcTarget.All, damage);
        }

        [PunRPC]
        private void BrickHitRPC(int damage)
        {
            
            var color = _spriteRenderer.color;
            color.g -= _colorChangeFactor;
            color.b -= _colorChangeFactor;
            _spriteRenderer.color = color;
            Health = Health - damage;
            if (Health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
