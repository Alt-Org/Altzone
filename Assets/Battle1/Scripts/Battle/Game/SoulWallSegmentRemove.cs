using Altzone.Scripts.GA;
using Prg.Scripts.Common.AudioPlayer;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Battle1.Scripts.Battle.Game
{
    #region Message Classes
    internal class SoulWallSegmentRemoved
    {
        public BattleTeamNumber Side;

        public SoulWallSegmentRemoved(BattleTeamNumber side)
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
            _battleDebugLogger.LogInfo("Brick hit (health: {0})", Health);
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
            _battleDebugLogger.LogInfo("spriteIndex: " + _spriteIndex);
            _audioPlayer.Play(_spriteIndex);

            /*if (PhotonNetwork.IsMasterClient) GameAnalyticsManager.Instance.OnWallHit(_side.ToString());*/
        }
        #endregion Public Methods

        #region Private

        #region Private - Constants
        private const int HitEffectIndex = 0;
        private const int BreakEffectIndex = 1;
        #endregion Private - Constants

        #region Private - Fields

        private BattleTeamNumber _side;
        private float _colorChangeFactor;
        private int _spriteIndex;
        private BallHandler _ballHandler;

        // Components
        private SpriteRenderer _spriteRenderer;

        #endregion Private - Fields

        #region DEBUG
        private BattleDebugLogger _battleDebugLogger;
        #endregion DEBUG

        #region Private - Methods
        private void Start()
        {
            _side = transform.position.y < 0 ? BattleTeamNumber.TeamAlpha : BattleTeamNumber.TeamBeta;
            Health = _playerPlayArea.soulWallSegmentHeahlt;
            _colorChangeFactor = 1f / Health;

            // get components
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _ballHandler = Context.GetBallHandler;

            // debug
            _battleDebugLogger = new BattleDebugLogger(this);
        }

        #endregion Private - Methods

        #endregion Private
    }
}
