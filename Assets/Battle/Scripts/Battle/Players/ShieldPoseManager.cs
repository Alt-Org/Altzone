using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class ShieldPoseManager : MonoBehaviour
    {
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
        private Shield _currentPose;
        private int _maxPoseIndex = -1;
        private bool _hitboxActive;
        private bool _showShield;

        private void Awake()
        {
            int childCount = transform.childCount;
            _maxPoseIndex = childCount - 1;
            _shields = new Shield[childCount];
            for (int i = 0; i <= _maxPoseIndex; i++)
            {
                _shields[i] = new Shield(transform.GetChild(i).gameObject);
                _shields[i].ShieldGameObject.SetActive(false);
            }
            _shields[0].ShieldGameObject.SetActive(true);
        }

        public int MaxPoseIndex => _maxPoseIndex;

        public void SetSpriteVariant(int variant)
        {
            foreach (Shield shield in _shields)
            {
                shield.ShieldSpriteGameObject.SetActive(false);
                shield.SpriteVariant = variant;
                shield.ShieldSpriteGameObject.SetActive(true);
            }
            _currentPose.ShieldSpriteRenderer.enabled = _showShield;
        }

        public void SetPose(int poseIndex)
        {
            if (_currentPose != null)
            {
                _currentPose.ShieldGameObject.SetActive(false);
            }
            _currentPose = _shields[poseIndex];
            _currentPose.ShieldGameObject.transform.localPosition = Vector3.zero;
            _currentPose.ShieldGameObject.SetActive(true);
            _currentPose.ShieldHitbox.SetActive(_hitboxActive);
            _currentPose.ShieldSpriteRenderer.enabled = _showShield;
        }

        /* old
        public void SetShieldSpriteRotation(float angle)
        {
            foreach (Shield shield in _shields)
            {
                shield.ShieldSpriteGameObject.transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }
        */

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
            _currentPose.ShieldHitbox.SetActive(active);
        }

        public void SetShow(bool show)
        {
            _showShield = show;
            _currentPose.ShieldSpriteRenderer.enabled = show;
        }
    }
}



