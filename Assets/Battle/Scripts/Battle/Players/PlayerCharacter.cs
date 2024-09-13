using UnityEngine;
using static Battle.Scripts.Battle.Players.PlayerActor;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Handles PlayerCharacter GameObject in PlayerActor prefabs.
    /// </summary>
    internal class PlayerCharacter : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] [Tooltip("0: Idle without shield sprite\n1: Idle with shield sprite\n2: Moving sprite")] private Sprite[] _spriteSheetA;
        [SerializeField] [Tooltip("0: Idle without shield sprite\n1: Idle with shield sprite\n2: Moving sprite")] private Sprite[] _spriteSheetB;
        #endregion Serialized Fields

        #region Public

        #region Public - Enums
        public enum SpriteIndexEnum
        { IdleWithoutShield = 0, IdleWithShield = 1, Moving = 2 }
        #endregion Public - Enums

        #region Public - Properties

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        /// <summary>
        /// Shows or hides <c>PlayerCharacter</c> sprite. (true = show, false = hide)
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

                switch (_spriteVariant)
                {
                    case SpriteVariant.A:
                        _spriteSheet = _spriteSheetA;
                        break;

                    case SpriteVariant.B:
                        _spriteSheet = _spriteSheetB;
                        break;
                }
                _spriteRenderers[(int)_spriteVariant].sprite = _spriteSheet[(int)_spriteIndex];
            }
        }

        /// <summary>
        /// Selects a <c>Sprite</c> from spriteSheetA or spriteSheetB depending on current sprite variant. <br></br>
        /// (spriteSheets are defined in serialized fields)
        /// </summary>
        public SpriteIndexEnum SpritIndex
        {
            get { return _spriteIndex; }
            set
            {
                _spriteIndex = value;
                _spriteRenderers[(int)_spriteVariant].sprite = _spriteSheet[(int)value];
            }
        }

        #endregion Public - Properties

        #region Public - Methods

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
        }

        #endregion Public - Methods

        #endregion Public

        #region Private

        #region Private - Fields
        private IReadOnlyBattlePlayer _battlePlayer;
        private bool _show;
        private SpriteVariant _spriteVariant;
        private SpriteIndexEnum _spriteIndex;
        private Sprite[] _spriteSheet;
        private GameObject[] _spriteGameObjects;
        private SpriteRenderer[] _spriteRenderers;
        #endregion Private - Fields

        private void Start()
        {
            _show = true;
            _spriteVariant = SpriteVariant.A;
            _spriteIndex = SpriteIndexEnum.IdleWithShield;
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
