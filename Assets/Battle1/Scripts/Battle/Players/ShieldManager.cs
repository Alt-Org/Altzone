//#define SHIELD_MANAGER_DEBUG

using Altzone.Scripts.GA;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Battle1.Scripts.Battle.Players
{
    internal class ShieldManager : MonoBehaviour
    {
        #region Public

        #region Public - Properties

        public bool Initialized => _initialized;

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        #endregion Public - Properties

        #region Public - Methods

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
            _currentShield = new Shield(transform.GetChild(0).gameObject, this, _battlePlayer);
            _initialized = true;
        }

        #region Public - Methods - Setters

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

            _currentShield = new Shield(Instantiate(shieldGameObject, transform.position, transform.rotation, transform), this, BattlePlayer);
            _currentShield.ShieldGameObject.transform.localPosition = Vector3.zero;
            _currentShield.ShieldGameObject.SetActive(true);
            _currentShield.ShieldHitbox.SetActive(_hitboxActive && _timer <= 0);
            _currentShield.ShieldSpriteRenderer.enabled = _showShield;

#if SHIELD_MANAGER_DEBUG
            if (ShieldManagerDebug)
            {
                _currentShield.ShieldGameObject.transform.Find("ShieldHitBoxIndicators").gameObject.SetActive(true);
            }
#endif
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

        #endregion Public - Methods - Setters

        public void OnShieldBoxCollision()
        {
            _currentShield.ShieldHitbox.SetActive(false);
            _timer = 5;

            /*if (PhotonNetwork.IsMasterClient) GameAnalyticsManager.Instance.OnShieldHit(_battlePlayer.PlayerPosition.ToString());*/
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

            public Shield(GameObject shieldGameObject, ShieldManager shieldManager, IReadOnlyBattlePlayer battlePlayer)
            {
                ShieldGameObject = shieldGameObject;
                ShieldHitbox = shieldGameObject.transform.Find("Colliders").gameObject;
                _spriteGameObjects = new GameObject[PlayerActor.SpriteVariantCount];
                _spriteGameObjects[(int)PlayerActor.SpriteVariant.A] = shieldGameObject.transform.Find("SpriteA").gameObject;
                _spriteGameObjects[(int)PlayerActor.SpriteVariant.B] = shieldGameObject.transform.Find("SpriteB").gameObject;
                _spriteRenderers = new SpriteRenderer[PlayerActor.SpriteVariantCount];
                _spriteRenderers[(int)PlayerActor.SpriteVariant.A] = _spriteGameObjects[(int)PlayerActor.SpriteVariant.A].GetComponent<SpriteRenderer>();
                _spriteRenderers[(int)PlayerActor.SpriteVariant.B] = _spriteGameObjects[(int)PlayerActor.SpriteVariant.B].GetComponent<SpriteRenderer>();

                foreach (Transform transform in ShieldHitbox.transform)
                {
                    transform.GetComponent<ShieldBoxCollider>().InitInstance(shieldManager, battlePlayer);
                }
            }

            private readonly GameObject[] _spriteGameObjects;
            private readonly SpriteRenderer[] _spriteRenderers;

        }

        #region Private - Fields
        private IReadOnlyBattlePlayer _battlePlayer;
        private Shield _currentShield;
        private bool _initialized = false;
        private bool _hitboxActive;
        private bool _showShield;
        private int _timer = -1;

        #endregion Private - Fields

        #region Private - Methods

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



