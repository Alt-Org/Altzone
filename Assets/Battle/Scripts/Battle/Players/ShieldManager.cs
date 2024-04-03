using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class ShieldManager : MonoBehaviour
    {
        #region Public Variables
        public int MaxShieldIndex => _maxShieldIndex;
        #endregion

        #region Public Methods
        public void SetSpriteVariant(int variant)
        {
            foreach (Shield shield in _shields)
            {
                shield.ShieldSpriteGameObject.SetActive(false);
                shield.SpriteVariant = variant;
                shield.ShieldSpriteGameObject.SetActive(true);
            }
            _currentShield.ShieldSpriteRenderer.enabled = _showShield;
        }

        public void SetShield(GameObject gameObject, int shieldIndex)
        {
            if (_currentShield != null)
            {
                Destroy(_currentShield.ShieldGameObject); 
            }
            _currentShield = Instantiate(_shields[shieldIndex].ShieldGameObject, gameObject.transform);
            _currentShield.transform.localPosition = Vector3.zero;
            _currentShield.SetActive(true);
            _currentShield.GetComponent<Shield>().ShieldHitbox.SetActive(_hitboxActive);
            _currentShield.GetComponent<Shield>().ShieldSpriteRenderer.enabled = _showShield;
        }

        public void SetShieldSpriteOpacity(float opacity)
        {
            foreach (Shield shield in _shields)
            {
                shield.Opacity = opacity;
            }
        }

        public void SetHitboxActive(bool active)
        {
            _hitboxActive = active;
            _currentShield.ShieldHitbox.SetActive(active);
        }

        public void SetShow(bool show)
        {
            _showShield = show;
            _currentShield.ShieldSpriteRenderer.enabled = show;
        }
        #endregion

        #region Private Classes and Methods
        private class Shield
        {
            public GameObject ShieldGameObject { get; }
            public GameObject ShieldHitbox { get; }
            public GameObject ShieldSpriteGameObject { get => _spriteGameObjects[SpriteVariant]; }
            public SpriteRenderer ShieldSpriteRenderer { get => _spriteRenderers[SpriteVariant]; }
            public float Opacity
            {
                get => _spriteRenderers[SpriteVariant].color.a;
                set
                {
                    Color color = _spriteRenderers[SpriteVariant].color;
                    color.a = value;
                    _spriteRenderers[SpriteVariant].color = color;
                }
            }
            public int SpriteVariant;

            public Shield(GameObject shieldGameObject)
            {
                ShieldGameObject = shieldGameObject;
                ShieldHitbox = shieldGameObject.transform.Find("Colliders").gameObject;
                _spriteGameObjects = new GameObject[PlayerActor.SPRITE_VARIANT_COUNT];
                _spriteGameObjects[PlayerActor.SPRITE_VARIANT_A] = shieldGameObject.transform.Find("SpriteA").gameObject;
                _spriteGameObjects[PlayerActor.SPRITE_VARIANT_B] = shieldGameObject.transform.Find("SpriteB").gameObject;
                _spriteRenderers = new SpriteRenderer[PlayerActor.SPRITE_VARIANT_COUNT];
                _spriteRenderers[PlayerActor.SPRITE_VARIANT_A] = _spriteGameObjects[PlayerActor.SPRITE_VARIANT_A].GetComponent<SpriteRenderer>();
                _spriteRenderers[PlayerActor.SPRITE_VARIANT_B] = _spriteGameObjects[PlayerActor.SPRITE_VARIANT_B].GetComponent<SpriteRenderer>();
            }

            private GameObject[] _spriteGameObjects;
            private SpriteRenderer[] _spriteRenderers;
        }

        private Shield[] _shields;
        private Shield _currentShield;
        private int _maxShieldIndex = -1;
        private bool _hitboxActive;
        private bool _showShield;

        private void Awake()
        {
            int childCount = transform.childCount;
            _maxShieldIndex = childCount - 1;
            _shields = new Shield[childCount];
            for (int i = 0; i <= _maxShieldIndex; i++)
            {
                _shields[i] = new Shield(transform.GetChild(i).gameObject);
                _shields[i].ShieldGameObject.SetActive(false);
            }
            _shields[0].ShieldGameObject.SetActive(true);
        }
        #endregion
    }
}

