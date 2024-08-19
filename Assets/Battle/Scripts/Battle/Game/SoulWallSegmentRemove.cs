using UnityEngine;

using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.AudioPlayer;

namespace Battle.Scripts.Battle.Game
{
    #region Message Classes
    internal class SoulWallSegmentRemoved
    {
        /// <summary>
        /// PhotonBattle team number
        /// </summary>
        public int Side;

        public SoulWallSegmentRemoved(int side)
        {
            Side = side;
        }
    }
    #endregion Message Classes

    /// <summary>
    /// Removes a brick from the wall when hit conditions are met.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    internal class SoulWallSegmentRemove : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private PlayerPlayArea _playerPlayArea;
        [SerializeField] private AudioPlayer _audioPlayer;
        #endregion Serialized Fields

        #region Public Properties
        public int Health { get; private set; } = 0;
        #endregion Public Properties

        #region Public Methods
        public void BrickHitInit(int damage)
        {
            _spriteIndex = _ballHandler.SpriteIndex();
            Color color = _spriteRenderer.color;
            color.g -= _colorChangeFactor;
            color.b -= _colorChangeFactor;
            _spriteRenderer.color = color;
            Health -= damage;
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Brick hit (health: {1})", _syncedFixedUpdateClock.UpdateCount, Health));
            if (Health <= 0)
            {
                Destroy(gameObject);
                this.Publish(new SoulWallSegmentRemoved(_side));
                //_audioPlayer.Play(BREAK_EFFECT_INDEX);
            }
            else
            {
                //_audioPlayer.Play(HIT_EFFECT_INDEX);
            }
            Debug.Log("spriteIndex: " + _spriteIndex);
            _audioPlayer.Play(_spriteIndex);
        }
        #endregion Public Methods

        #region Private

        #region Private - Constants
        private const int HIT_EFFECT_INDEX = 0;
        private const int BREAK_EFFECT_INDEX = 1;
        #endregion Private - Constants

        #region Private - Fields

        private int _side;
        private float _colorChangeFactor;
        private int _spriteIndex;
        private BallHandler _ballHandler;

        // Components
        private SpriteRenderer _spriteRenderer;

        #endregion Private - Fields

        #region DEBUG
        private const string DEBUG_LOG_NAME = "[BATTLE] [BRICK REMOVE] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time
        #endregion DEBUG

        #region Private - Methods
        private void Start()
        {
            _side = transform.position.y < 0 ? PhotonBattle.TeamAlphaValue : PhotonBattle.TeamBetaValue;
            Health = _playerPlayArea.soulWallSegmentHeahlt;
            _colorChangeFactor = 1f / Health;

            // get components
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _ballHandler = Context.GetBallHandler;

            // debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        #endregion Private - Methods

        #endregion Private
    }
}
