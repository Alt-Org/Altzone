using UnityEngine;
using static Battle.Scripts.Battle.Players.PlayerActor;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Handles PlayerSoul GameObject in PlayerActor prefabs.
    /// </summary>
    internal class PlayerSoul : MonoBehaviour
    {
        #region Public

        #region Public - Methods

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
            GetComponentInChildren<Pickup>().InitInstance(battlePlayer);
        }

        #endregion Public - Methods

        #region Public - Properties

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        /// <summary>
        /// Shows or hides <c>PlayerSoul</c> sprite. (true = show, false = hide)
        /// </summary>
        public bool Show
        {
            get { return _show; }
            set
            {
                _show = value;
                _spriteRenderers[(int)_spriteVariant].enabled = value;
            }
        }

        /// <summary>
        /// Sprite variant. (A or B)
        /// </summary>
        public SpriteVariant SpriteVariant
        {
            get { return _spriteVariant; }
            set
            {
                _spriteGameObjects[(int)_spriteVariant].SetActive(false);
                _spriteVariant = value;
                _spriteGameObjects[(int)_spriteVariant].SetActive(true);
                _spriteRenderers[(int)_spriteVariant].enabled = _show;
            }
        }

        #endregion Public - Properties

        #endregion Public

        #region Private

        #region Private - Fields
        private IReadOnlyBattlePlayer _battlePlayer;
        private bool _show;
        private SpriteVariant _spriteVariant;
        private GameObject[] _spriteGameObjects;
        private SpriteRenderer[] _spriteRenderers;
        #endregion Private - Fields

        private void Start()
        {
            _show = true;
            _spriteVariant = SpriteVariant.A;
            _spriteGameObjects = new GameObject[SpriteVariantCount];
            _spriteGameObjects[(int)SpriteVariant.A] = transform.Find("SpriteA").gameObject;
            _spriteGameObjects[(int)SpriteVariant.B] = transform.Find("SpriteB").gameObject;
            _spriteRenderers = new SpriteRenderer[SpriteVariantCount];
            _spriteRenderers[(int)SpriteVariant.A] = _spriteGameObjects[(int)SpriteVariant.A].GetComponent<SpriteRenderer>();
            _spriteRenderers[(int)SpriteVariant.B] = _spriteGameObjects[(int)SpriteVariant.B].GetComponent<SpriteRenderer>();
        }

        #endregion Private
    }
}
