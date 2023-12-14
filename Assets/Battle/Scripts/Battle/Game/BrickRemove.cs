using UnityConstants;
using UnityEngine;
using Prg.Scripts.Common.PubSub;
//using Photon.Pun;

namespace Battle.Scripts.Battle.Game
{
    #region Message Classes
    class BrickRemoved
    {
        /// <summary>
        /// PhotonBattle team number
        /// </summary>
        public int Side;

        public BrickRemoved(int side)
        {
            Side = side;
        }
    }
    #endregion Message Classes

    /// <summary>
    /// Removes a brick from the wall when hit conditions are met.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    internal class BrickRemove : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] public PlayerPlayArea PlayerPlayArea;
        [SerializeField] public AudioPlayer _audioPlayer;

        // Public Properties
        public int Health { get; private set; } = 0;

        #region Public Methods
        public void BrickHitInit(int damage)
        {
            var color = _spriteRenderer.color;
            color.g -= _colorChangeFactor;
            color.b -= _colorChangeFactor;
            _spriteRenderer.color = color;
            Health = Health - damage;
            if (Health <= 0)
            {
                Destroy(gameObject);
                this.Publish(new BrickRemoved(_side));
                //_audioPlayer.Play(BREAK_EFFECT_INDEX);
            }
            else
            {
                //_audioPlayer.Play(HIT_EFFECT_INDEX);
            }
            _audioPlayer.PlayRandom();
        }
        #endregion Public Methods

        // Private Constants
        private const int HIT_EFFECT_INDEX = 0;
        private const int BREAK_EFFECT_INDEX = 1;
        
        private int _side;
        private float _colorChangeFactor;

        // Components
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _side = transform.position.y < 0 ? PhotonBattle.TeamAlphaValue : PhotonBattle.TeamBetaValue;
            Health = PlayerPlayArea.BrickHealth;
            _colorChangeFactor = 1f / Health;

            // get components
            _spriteRenderer = GetComponent<SpriteRenderer>();

        }
    }
}
