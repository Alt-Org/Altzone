using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class ShieldManager : MonoBehaviour
    {
        #region Public

        #region Public - Properties
        public bool Initialized => _initialized;
        #endregion Public - Properties

        #region Public - Setter Methods

        public void SetSpriteVariant(PlayerActor.SpriteVariant variant)
        {
            _currentShield.ShieldSpriteGameObject.SetActive(false);
            _currentShield.SpriteVariant = variant;
            _currentShield.ShieldSpriteGameObject.SetActive(true);
            _currentShield.ShieldSpriteRenderer.enabled = _showShield;
        }

        public void SetShield(GameObject shieldGameObject)
        {
            Destroy(_currentShield.ShieldGameObject);

            _currentShield = new Shield(Instantiate(shieldGameObject, transform.position, transform.rotation, transform));
            _currentShield.ShieldGameObject.transform.localPosition = Vector3.zero;
            _currentShield.ShieldGameObject.SetActive(true);
            _currentShield.ShieldHitbox.SetActive(_hitboxActive && _timer <= 0);
            _currentShield.ShieldSpriteRenderer.enabled = _showShield;
        }

        public void SetHitboxActive(bool active)
        {
            _hitboxActive = active;
            if (_timer <= 0) _currentShield.ShieldHitbox.SetActive(active);
        }

        public void SetShow(bool show)
        {
            _showShield = show;
            _currentShield.ShieldSpriteRenderer.enabled = show;
        }

        public void OnShieldBoxCollision()
        {
            _currentShield.ShieldHitbox.SetActive(false);
            _timer = 5;
        }

        #endregion Public - Methods

        #endregion Public

        #region Private

        private class Shield
        {
            public GameObject ShieldGameObject { get; }
            public GameObject ShieldHitbox { get; }
            public GameObject ShieldSpriteGameObject { get => _spriteGameObjects[(int)SpriteVariant]; }
            public SpriteRenderer ShieldSpriteRenderer { get => _spriteRenderers[(int)SpriteVariant]; }
            public PlayerActor.SpriteVariant SpriteVariant;

            public Shield(GameObject shieldGameObject)
            {
                ShieldGameObject = shieldGameObject;
                ShieldHitbox = shieldGameObject.transform.Find("Colliders").gameObject;
                _spriteGameObjects = new GameObject[PlayerActor.SPRITE_VARIANT_COUNT];
                _spriteGameObjects[(int)PlayerActor.SpriteVariant.A] = shieldGameObject.transform.Find("SpriteA").gameObject;
                _spriteGameObjects[(int)PlayerActor.SpriteVariant.B] = shieldGameObject.transform.Find("SpriteB").gameObject;
                _spriteRenderers = new SpriteRenderer[PlayerActor.SPRITE_VARIANT_COUNT];
                _spriteRenderers[(int)PlayerActor.SpriteVariant.A] = _spriteGameObjects[(int)PlayerActor.SpriteVariant.A].GetComponent<SpriteRenderer>();
                _spriteRenderers[(int)PlayerActor.SpriteVariant.B] = _spriteGameObjects[(int)PlayerActor.SpriteVariant.B].GetComponent<SpriteRenderer>();
            }

            private readonly GameObject[] _spriteGameObjects;
            private readonly SpriteRenderer[] _spriteRenderers;
        }

        #region Private - Fields
        private Shield _currentShield;
        private bool _initialized = false;
        private bool _hitboxActive;
        private bool _showShield;
        private int _timer = -1;
        #endregion Private - Fields

        #region Private - Methods

        private void Awake()
        {
            _currentShield = new Shield(transform.GetChild(0).gameObject);
            _initialized = true;
        }

        private void FixedUpdate()
        {
            if (_timer < 0) return;
            if (_timer == 0 && _hitboxActive) _currentShield.ShieldHitbox.SetActive(true);
            _timer--;
        }

        #endregion Private - Methods

        #endregion Private
    }
}



